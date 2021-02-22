using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        private List<BingoSquare> Board;
        public List<bool> ObjectivesCompleted;

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
            
            this.RefreshObjectives();
        }

        private void RefreshObjectives() {
            this.ObjectivesCompleted = new List<bool>();
            for (int i = 0; i < 25; i++) {
                this.ObjectivesCompleted.Add(false);
            }
        }

        private void BingoEvent(StatusMessage msg) {
            this.LogChat(msg.Render());
            switch (msg.type) {
                case "connection" when msg.event_type == "disconnected":
                    break;
                case "connection" when msg.event_type == "connected":
                    this.Connected = true;
                    break;
                case "connection":
                    break;
                case "goal": {
                    var i = int.Parse(msg.square.slot.Substring(4)) - 1;
                    this.Board[i].Color = msg.remove ? ColorMap["blank"] : ColorMap[msg.player.color];
                    break;
                }
                case "color":
                    break;
                case "chat":
                    break;
                case "error":
                    if (this.Connected) {
                        this.LogChat(Dialog.Clean("bingoclient_connect_retrying"));
                        this.Disconnect();
                        try {
                            this.Connect();
                        } catch (Exception e) {
                            Logger.LogDetailed(e, "BingoClient");
                            this.LogChat(Dialog.Clean("bingoclient_connect_error"));
                        }
                    } else {
                        this.LogChat(Dialog.Clean("bingoclient_connect_tryagain"));
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

        public void UpdateObjectives() {
            if (this.ObjectivesCompleted == null) {
                return;
            }
            
            for (var i = 0; i < 25; i++) {
                if (this.GetObjectiveStatus(i) != ObjectiveStatus.Completed) {
                    continue;
                }

                if (!this.ObjectivesCompleted[i]) {
                    this.ObjectivesCompleted[i] = true;
                    this.LogChat(string.Format(Dialog.Get("bingoclient_objective_claimable"), this.Board[i].Text));
                }
                
                if (this.ModSettings.QuickClaim.Check) {
                    this.SendClaim(i);
                }
            }
        }

        public ObjectiveStatus GetObjectiveStatus(int i) {
            if (this.Board[i].Color != Color.Black) {
                return ObjectiveStatus.Claimed;
            }

            if (this.ObjectivesCompleted[i]) {
                return ObjectiveStatus.Completed;
            }

            if (SaveData.Instance == null) {
                return ObjectiveStatus.Nothing;
            }
            
            if (!BingoMonitor.Objectives.TryGetValue(this.Board[i].Text, out var checker) || checker == null) {
                return ObjectiveStatus.Unknown;
            }
            
            var progress = checker();
            if (progress < 0.001f) {
                return ObjectiveStatus.Nothing;
            }

            if (progress > 0.999f) {
                return ObjectiveStatus.Completed;
            }

            return ObjectiveStatus.Progress;
        }

        public bool IsObjectiveClaimable(int i) {
            return this.Board[i].Color == Color.Black && this.ObjectivesCompleted[i];
        }
        
        public IEnumerable<BingoVariant> RelevantVariants() {
            var checkpoint = BingoMonitor.AtCheckpoint();
            if (checkpoint == null) {
                yield break;
            }
            var area = SaveData.Instance.CurrentSession.Area;

            var seen = new HashSet<BingoVariant>();
            foreach (var square in Instance.Board) {
                if (!BingoMonitor.ObjectiveVariants.TryGetValue(square.Text, out var variants)) {
                    continue;
                }

                foreach (var entry in variants) {
                    if (entry.Item1 == area.ID && entry.Item2 == (int) area.Mode && entry.Item3 == checkpoint && !seen.Contains(entry.Item4)) {
                        seen.Add(entry.Item4);
                        yield return entry.Item4;
                    }
                }
            }
        }
    }
}
