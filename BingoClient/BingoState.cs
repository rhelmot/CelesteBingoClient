using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        private List<BingoSquare> Board;

        public enum BingoColors {
            Blank,
            Orange,
            Red,
            Blue,
            Green,
            Purple,
            Navy,
            Teal,
            Brown,
            Pink,
            Yellow,
        }
        
        public static Dictionary<string, Color> ColorMap = new Dictionary<string, Color> {
            {"blank", Color.Black},
            {"orange", new Color(0xF9, 0x8E, 0x1E)},
            {"red", new Color(0xDA, 0x44, 0x40)},
            {"blue", new Color(0x37, 0xA1, 0xDE)},
            {"green", new Color(0x00, 0xB5, 0x00)},
            {"purple", new Color(0x82, 0x2d, 0xbf)},
            {"navy", new Color(0x0d, 0x48, 0xb5)},
            {"teal", new Color(0x41, 0x96, 0x95)},
            {"brown", new Color(0xab, 0x5c, 0x23)},
            {"pink", new Color(0xed, 0x86, 0xaa)},
            {"yellow", new Color(0xd8, 0xd0, 0x14)},
        };

        private void SetState(List<SquareMsg> board) {
            this.Board = new List<BingoSquare>();
            for (int i = 0; i < 25; i++) {
                this.Board.Add(new BingoSquare {
                    Idx = i,
                });
            }

            foreach (var square in board) {
                int i = int.Parse(square.slot.Substring(4)) - 1;
                this.Board[i].Color = ColorMap[square.colors];
                this.Board[i].Text = square.name;
            }
        }
        
        private void BingoEvent(StatusMessage msg) {
            switch (msg.type) {
                case "connection" when msg.event_type == "disconnected":
                    Toast($"{msg.player.name} disconnected");
                    break;
                case "connection" when msg.event_type == "connected":
                    Toast($"{msg.player.name} connected");
                    this.Connected = true;
                    break;
                case "connection":
                    Logger.Log("BingoClient", $"Unknown connection message {msg.event_type}");
                    break;
                case "goal": {
                    var i = int.Parse(msg.square.slot.Substring(4)) - 1;
                    if (msg.remove) {
                        this.Board[i].Color = ColorMap["blank"];
                        Toast($"{msg.player.name} cleared \"{this.Board[i].Text}\"");
                    } else {
                        this.Board[i].Color = ColorMap[msg.player.color];
                        Toast($"{msg.player.name} marked \"{this.Board[i].Text}\"");
                    }

                    break;
                }
                case "color":
                    Toast($"{msg.player.name} changed color to {msg.player.color}");
                    break;
                case "chat":
                    Toast($"{msg.player.name} said: {msg.text}");
                    break;
                case "error":
                    Toast($"Error from server: {msg.error}");
                    if (this.Connected) {
                        Toast(Dialog.Clean("bingoclient_connect_retrying"));
                        this.Disconnect();
                        try {
                            this.Connect();
                        } catch (Exception e) {
                            Logger.LogDetailed(e, "BingoClient");
                            Toast(Dialog.Clean("bingoclient_connect_error"));
                        }
                    } else {
                        Toast(Dialog.Clean("bingoclient_connect_tryagain"));
                        this.Disconnect();
                    }
                    break;
                default:
                    Logger.Log("BingoClient", $"Unknown message {msg.type}");
                    break;
            }
        }

        private class BingoSquare {
            public int Idx;
            public Color Color = Color.Black;
            public string Text = "";
        }
    }

    public static class BingoColorsExt {
        public static Color ToColor(this BingoClient.BingoColors self) {
            return BingoClient.ColorMap[self.ToString().ToLowerInvariant()];
        }
    }
}
