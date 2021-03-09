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
        private String SavedRoomId, SavedPassword;
        public bool Connected;
        private BingoColors SentColor;
        public bool IsBoardHidden, IsLockout;

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
        private string RevealUrl => $"{this.RoomDomain}/api/revealed";
        private string SettingsUrl => $"{this.RoomDomain}/room/{this.RoomId}/room-settings";

        private CookieAwareWebClient Session;
        private ClientWebSocket Sock;
        private CancellationTokenSource CancelToken;
        private SemaphoreSlim Lock = new SemaphoreSlim(1);

        public void Connect() {
            string sessionKey;
            if (this.Session == null || this.Password != this.SavedPassword || this.RoomId != this.SavedRoomId) {
                try {
                    this.SavedPassword = this.Password;
                    this.SavedRoomId = this.RoomId;
                    using (this.Lock.Use(CancellationToken.None)) {
                        this.Session = new CookieAwareWebClient(new CookieContainer());
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
                        if (r2.Contains("Incorrect Password")) {
                            throw new Exception("Incorrect Password");
                        }

                        this.IsBoardHidden = r2.Contains("hide_card\\u0022: true");
                        this.IsLockout = r2.Contains("\\u0022lockout_mode\\u0022: \\u0022Lockout\\u0022");
                        sessionKey = RecoverFormValue("temporarySocketKey", r2);
                    }
                } catch (Exception) {
                    this.Session = null;
                    throw;
                }
            } else {
                using (this.Lock.Use(CancellationToken.None)) {
                    var r2 = this.Session.DownloadString(this.RoomUrl);
                    sessionKey = RecoverFormValue("temporarySocketKey", r2);
                }
            }

            this.CancelToken = new CancellationTokenSource();
            this.Chat.SetHistory(this.GetHistory().events.Select(x => x.Render()));

            // https://stackoverflow.com/questions/40502921/net-websockets-forcibly-closed-despite-keep-alive-and-activity-on-the-connectio
            ServicePointManager.MaxServicePointIdleTime = 1000 * 60 * 60 * 24;

            this.Sock = new ClientWebSocket();
            Uri uri = new Uri("wss://sockets.bingosync.com/broadcast");
            //Uri uri = new Uri("ws://localhost:8902/");
            this.Sock.ConnectAsync(uri, this.CancelToken.Token).Wait();
            string msg = JsonConvert.SerializeObject(new HelloMessage {
                socket_key = sessionKey,
            });
            this.Sock.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)),
                WebSocketMessageType.Text,
                true,
                this.CancelToken.Token).Wait();

            new Task(this.RecvThreadFunc).Start();

            while (!this.Connected) {
                if (this.Sock == null) {
                    throw new Exception("Encountered something wrong while logging in");
                }
                Thread.Sleep(10);
            }

            this.RefreshBoard();
            this.SendColor();
        }

        public void Disconnect() {
            this.CancelToken?.Cancel();
            //this.CancelToken = null;  // don't null this so we can preemptively cancel future things
            this.Sock = null;
            this.Connected = false;
        }

        public void SendClaim(int slot) {
            new Task(() => {
                using (this.Lock.Use(this.CancelToken.Token)) {
                    this.Session.UploadString(this.SelectUrl, JsonConvert.SerializeObject(new SelectMessage {
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
                using (this.Lock.Use(this.CancelToken.Token)) {
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
                    using (this.Lock.Use(this.CancelToken.Token)) {
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
                using (this.Lock.Use(this.CancelToken.Token)) {
                    this.Session.UploadString(this.ChatUrl, JsonConvert.SerializeObject(new ChatMessage {
                        text = text,
                        room = this.RoomId,
                    }));
                }
            }).Start();
        }

        public void RevealBoard() {
            if (this.IsBoardHidden) {
                new Task(() => {
                    using (this.Lock.Use(this.CancelToken.Token)) {
                        this.Session.UploadString(this.RevealUrl, JsonConvert.SerializeObject(new RevealMessage {
                            room = this.RoomId,
                        }));
                    }
                }).Start();
                this.IsBoardHidden = false;
            }
        }

        public List<SquareMsg> GetBoard() {
            using (this.Lock.Use(this.CancelToken.Token)) {
                return JsonConvert.DeserializeObject<List<SquareMsg>>(this.Session.DownloadString(this.RoomUrl + "/board"));
            }
        }

        public HistoryMessage GetHistory() {
            using (this.Lock.Use(this.CancelToken.Token)) {
                var result = this.Session.DownloadString(this.RoomUrl + "/feed");
                return JsonConvert.DeserializeObject<HistoryMessage>(result);
            }
        }

        // returns (hide_card, lockout)
        public Tuple<bool, bool> GetSettings() {
            using (this.Lock.Use(this.CancelToken.Token)) {
                var result = this.Session.DownloadString(this.SettingsUrl);
                return Tuple.Create(result.Contains("\"hide_card\": true"), result.Contains("\"lockout_mode\": \"Lockout\""));
            }
        }

        private void RecvThreadFunc() {
            var token = this.CancelToken.Token;
            var buffer = new byte[1024];
            var sock = this.Sock;
            while (!token.IsCancellationRequested) {
                var t = sock.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                try {
                    t.Wait(token);
                } catch (OperationCanceledException) {
                    break;
                } catch (WebSocketException) {
                    this.Disconnect();
                    this.LogChat(Dialog.Clean("modoptions_bingoclient_disconnect_message"));
                    return;
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

        public class StatusMessage {
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

            public string Render() {
                switch (this.type) {
                    case "connection" when this.event_type == "disconnected":
                        return $"{this.player.name} disconnected";
                    case "connection" when this.event_type == "connected":
                        return $"{this.player.name} connected";
                    case "connection":
                        Logger.Log("BingoClient", $"Unknown connection message {this.event_type}");
                        return null;
                    case "goal" when this.remove:
                        return $"{this.player.name} cleared \"{this.square.name}\"";
                    case "goal" when !this.remove:
                        return $"{this.player.name} marked \"{this.square.name}\"";
                    case "color":
                        return $"{this.player.name} changed color to {this.player.color}";
                    case "chat":
                        return $"{this.player.name} said: {this.text}";
                    case "revealed":
                        return $"{this.player.name} revealed the board";
                    case "new-card":
                        return $"{this.player.name} generated a new card";
                    case "error":
                        return $"Error from server: {this.error}";
                    default:
                        Logger.Log("BingoClient", $"Unknown message {this.type}");
                        return null;
                }
            }
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

        public class HistoryMessage {
            public bool allincluded;
            public List<StatusMessage> events;
        }

        public class RevealMessage {
            public string room;
        }
    }
}
