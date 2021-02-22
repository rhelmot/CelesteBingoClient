using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        public string RoomDomain;
        public string RoomId;
        public String Username;
        public String Password;
        public bool Connected;
        private BingoColors SentColor;

        public string RoomUrl {
            get => $"{this.RoomDomain}/room/{this.RoomId}";
            set {
                var pieces = value.Split(new[] { "/room/" }, StringSplitOptions.None);
                this.RoomDomain = pieces[0];
                this.RoomId = pieces[1];
            }
        }

        private string SelectUrl => $"{this.RoomDomain}/api/select";
        private string ColorUrl => $"{this.RoomDomain}/api/color";
        private string ChatUrl => $"{this.RoomDomain}/api/chat";

        private CookieAwareWebClient Session;
        private ClientWebSocket Sock;
        private CancellationTokenSource CancelToken;
        
        public void Connect() {
            string sessionKey;
            if (this.Session == null) {
                this.Session = this.Session ?? new CookieAwareWebClient(new CookieContainer());
                var r1 = this.Session.DownloadString(this.RoomUrl);
                var postKeys = new NameValueCollection {
                    {"csrfmiddlewaretoken", RecoverFormValue("csrfmiddlewaretoken", r1)},
                    {"encoded_room_uuid", RecoverFormValue("encoded_room_uuid", r1)},
                    {"room_name", RecoverFormValue("room_name", r1)},
                    {"creator_name", RecoverFormValue("creator_name", r1)},
                    {"game_name", RecoverFormValue("game_name", r1)},
                    {"player_name", this.Username},
                    {"passphrase", this.Password},

                };
                var r2 = Encoding.UTF8.GetString(this.Session.UploadValues(this.RoomUrl, postKeys));
                sessionKey = RecoverFormValue("temporarySocketKey", r2);
            } else {
                var r2 = this.Session.DownloadString(this.RoomUrl);
                sessionKey = RecoverFormValue("temporarySocketKey", r2);
            }
            this.Sock = new ClientWebSocket();
            Uri uri = new Uri("wss://sockets.bingosync.com/broadcast");
            //Uri uri = new Uri("ws://localhost:8902/");
            this.Sock.ConnectAsync(uri, CancellationToken.None).Wait();
            string msg = JsonConvert.SerializeObject(new HelloMessage {
                socket_key = sessionKey,
            });
            this.Sock.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None).Wait();

            this.CancelToken = new CancellationTokenSource();
            new Task(() => {
                this.RecvThreadFunc(this.CancelToken.Token);
            }).Start();

            while (!this.Connected) {
                if (this.Sock == null) {
                    throw new Exception("Encountered something wrong while logging in");
                }
                Thread.Sleep(10);
            }
            
            Instance.SetState(Instance.GetBoard());
            Instance.StartObjectives();
            Instance.SendColor();
        }

        public void Disconnect() {
            this.CancelToken?.Cancel();
            this.CancelToken = null;
            this.Sock = null;
            this.Connected = false;
        }

        public void SendClaim(int slot) {
            new Task(() => {
                lock (this.Session) {
                    var result = this.Session.UploadString(this.SelectUrl, JsonConvert.SerializeObject(new SelectMessage {
                        color = this.ModSettings.PlayerColor.ToString().ToLowerInvariant(),
                        remove_color = false,
                        room = this.RoomId,
                        slot = (slot + 1).ToString(),
                    }));
                }
            }).Start();
        }

        public void SendClear(int slot) {
            new Task(() => {
                lock (this.Session) {
                    this.Session.UploadString(this.SelectUrl, JsonConvert.SerializeObject(new SelectMessage {
                        color = this.ModSettings.PlayerColor.ToString().ToLowerInvariant(),
                        remove_color = true,
                        room = this.RoomId,
                        slot = (slot + 1).ToString(),
                    }));
                }
            }).Start();
        }

        public void SendColor() {
            if (this.SentColor != this.ModSettings.PlayerColor) {
                new Task(() => {
                    lock (this.Session) {
                        this.Session.UploadString(this.ColorUrl, JsonConvert.SerializeObject(new ColorMessage {
                            color = this.ModSettings.PlayerColor.ToString().ToLowerInvariant(),
                            room = this.RoomId,
                        }));
                    }
                    this.SentColor = this.ModSettings.PlayerColor;
                }).Start();
            }
        }
        
        public void SendChat(string text) {
            new Task(() => {
                lock (this.Session) {
                    this.Session.UploadString(this.ColorUrl, JsonConvert.SerializeObject(new ChatMessage {
                        text = text,
                        room = this.RoomId,
                    }));
                }
            }).Start();
        }

        public List<SquareMsg> GetBoard() {
            return JsonConvert.DeserializeObject<List<SquareMsg>>(this.Session.DownloadString(this.RoomUrl + "/board"));
        }

        private void RecvThreadFunc(CancellationToken token) {
            var buffer = new byte[1024];
            var sock = this.Sock;
            while (!token.IsCancellationRequested) {
                var t = sock.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                try {
                    t.Wait(token);
                } catch (OperationCanceledException) {
                    break;
                } catch (Exception e) {
                    Logger.LogDetailed(e, "BingoClient");
                }

                StatusMessage obj;
                if (t.Result.Count == 0) {
                    // p sure this arm is impossible to reach
                    obj = new StatusMessage {
                        type = "error",
                        error = "Connection lost",
                        error_src = "client",
                    };
                } else {
                    var encoded = Encoding.UTF8.GetString(buffer, 0, t.Result.Count);
                    try {
                        obj = JsonConvert.DeserializeObject<StatusMessage>(encoded);
                    } catch (JsonException e) {
                        Logger.LogDetailed(e, "BingoClient");
                        obj = new StatusMessage {
                            type = "error",
                            error = "Malformed message",
                            error_src = "client",
                        };
                    }
                }
                this.BingoEvent(obj);
                if (obj.type == "error") {
                    break;
                }
            }

            sock.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
        }

        private static string RecoverFormValue(string name, string text) {
            // haha! funny parsing html (and js!) with regexes
            var searcher = new Regex(name + ".[^'\"]+['\"]([^'\"]+)");
            var match = searcher.Match(text);
            var result = match.Groups[1].Value;
            return result;
        }

        private class HelloMessage {
            public string socket_key;
        }

        class StatusMessage {
            public double timestamp;
            public string type;
            public string event_type;
            public PlayerMsg player;
            public string player_color;
            
            public string text;
            public string error;
            public string error_src = "server";

            public SquareMsg square;
            public bool remove;
        }
        
        public class PlayerMsg {
            public string uuid;
            public bool is_spectator;
            public string color;
            public string name;
        }

        public class SquareMsg {
            public string name;
            public string colors;
            public string slot;
        }

        public class SelectMessage {
            public string room;
            public string slot;
            public string color;
            public bool remove_color;
        }

        public class ColorMessage {
            public string room;
            public string color;
        }

        public class ChatMessage {
            public string room;
            public string text;
        }
    }
}
