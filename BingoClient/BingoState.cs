using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        private List<BingoSquare> Board;
        public List<bool> ObjectivesCompleted;
        public List<bool> ObjectivesSeen;

        private void RefreshBoard() {
            var board = this.GetBoard();
            this.Board = new List<BingoSquare>();
            for (int i = 0; i < 25; i++) {
                this.Board.Add(new BingoSquare {
                    Idx = i,
                });
            }

            foreach (var square in board) {
                int i = int.Parse(square.slot.Substring(4)) - 1;
                this.Board[i].Colors = new List<BingoColors>(BingoEnumExtensions.ParseColors(square.colors));
                this.Board[i].Text = square.name;
                this.Board[i].Tier = square.tier;
            }

            this.RefreshObjectives();
        }

        private void RefreshObjectives() {
            this.ObjectivesCompleted = new List<bool>();
            this.ObjectivesSeen = new List<bool>();
            for (int i = 0; i < 25; i++) {
                if (this.Board == null) {
                    this.ObjectivesCompleted.Add(false);
                } else {
                    this.ObjectivesCompleted.Add(this.Board[i].Colors.Contains(this.ModSettings.PlayerColor));
                }
                this.ObjectivesSeen.Add(false);
            }
        }

        private void BingoEvent(StatusMessage msg) {
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
                    var colors = msg.square.colors.Split(' ');
                    this.Board[i].Colors = new List<BingoColors>(BingoEnumExtensions.ParseColors(msg.square.colors));
                    if (this.IsObjectiveHidden(i)) {
                        msg.square.name = "[???]";
                    }
                    break;
                }
                case "new-card":
                    var settings = this.GetSettings();
                    this.IsBoardHidden = settings.Item1;
                    this.IsLockout = settings.Item2;
                    this.IsFog = settings.Item3;
                    Thread.Sleep(500);
                    this.RefreshBoard();
                    break;
                case "color":
                case "chat":
                case "revealed":
                    break;
                case "error":
                    if (this.Connected) {
                        this.LogChat(Dialog.Clean("bingoclient_connect_retrying"));
                        this.Reconnect();
                    } else {
                        this.LogChat(Dialog.Clean("bingoclient_connect_tryagain"));
                        this.Disconnect();
                    }
                    break;
                default:
                    Logger.Log("BingoClient", $"Unknown message {msg.type}");
                    break;
            }
            var rendered = msg.Render();
            if (rendered != null) {
                this.LogChat(rendered);
            }
        }

        public IEnumerable<Tuple<BingoColors, int>> Score() {
            var score = new Dictionary<BingoColors, int>();
            foreach (var square in this.Board) {
                foreach (var color in square.Colors) {
                    if (score.TryGetValue(color, out var count)) {
                        count++;
                    } else {
                        count = 1;
                    }
                    score[color] = count;
                }
            }

            var keys = new List<BingoColors>(score.Keys);
            keys.Sort();
            foreach (var color in keys) {
                yield return Tuple.Create(color, score[color]);
            }
        }

        private class BingoSquare {
            public int Idx;
            public int Tier;
            public List<BingoColors> Colors = new List<BingoColors>();
            public string Text = "";
        }

        public void UpdateObjectives() {
            if (this.ObjectivesCompleted == null) {
                return;
            }

            if (this.IsBoardHidden) {
                return;
            }

            if (Engine.Scene is Overworld) {
                return;
            }

            for (var i = 0; i < 25; i++) {
                if (this.IsObjectiveHidden(i)) {
                    continue;
                }

                var status = this.GetObjectiveStatus(i, true);
                var statusComplete = status == ObjectiveStatus.Completed;

                if (this.ObjectivesCompleted[i] != statusComplete) {
                    this.ObjectivesCompleted[i] = statusComplete;
                    if (this.IsObjectiveClaimable(i, statusComplete)) {
                        if (statusComplete) {
                            if (this.ModSettings.ClaimAssist == BingoClientSettings.ClaimAssistMode.Auto) {
                                this.SendClaim(i);
                            } else {
                                this.LogChat(string.Format(Dialog.Get("bingoclient_objective_claimable"), this.Board[i].Text));
                            }
                        } else {
                            if (this.ModSettings.ClaimAssist == BingoClientSettings.ClaimAssistMode.Auto) {
                                this.SendClear(i);
                            }
                        }
                    }
                }

                if (this.ModSettings.ClaimAssist == BingoClientSettings.ClaimAssistMode.Button && this.IsObjectiveClaimable(i) && this.ModSettings.QuickClaim.Pressed) {
                    this.SendClaim(i);
                }
            }
        }

        public void DowngradeObjectives() {
            if (this.ObjectivesCompleted == null) {
                return;
            }
            for (var i = 0; i < 25; i++) {
                this.ObjectivesCompleted[i] = false;
                if (!this.IsObjectiveHidden(i) && this.GetObjectiveStatus(i) == ObjectiveStatus.Completed) {
                    this.ObjectivesCompleted[i] = true;
                }
            }
        }

        private bool IsObjectiveLiftingFog(int i) {
            if (this.Board[i].Colors.Contains(this.ModSettings.PlayerColor)) {
                return true;
            }
            return false;
        }

        public bool IsObjectiveHidden(int i) {
            if (this.IsBoardHidden) {
                return true;
            }
            if (!this.IsFog) {
                return false;
            }
            if (this.ModSettings.FogPersistence && this.ObjectivesSeen[i]) {
                return false;
            }
            if (this.Board[i].Tier == 0
                    || this.IsObjectiveLiftingFog(i)
                    || (i < 20 && this.IsObjectiveLiftingFog(i+5))
                    || (i >= 5 && this.IsObjectiveLiftingFog(i-5))
                    || (i % 5 != 0 && this.IsObjectiveLiftingFog(i-1))
                    || (i % 5 != 4 && this.IsObjectiveLiftingFog(i+1))) {
                this.ObjectivesSeen[i] = true;
                return false;
            }
            return true;
        }

        public ObjectiveStatus GetObjectiveStatus(int i, bool force = false) {
            if (this.Board == null || this.Board.Count <= i || this.Board[i] == null || this.ObjectivesCompleted == null) {
                return ObjectiveStatus.Nothing;
            }

            if (this.IsLockout && this.Board[i].Colors.Count != 0) {
                return ObjectiveStatus.Claimed;
            }

            if (!force && this.Board[i].Colors.Contains(this.ModSettings.PlayerColor)) {
                return ObjectiveStatus.Claimed;
            }

            if (!force && this.ObjectivesCompleted[i]) {
                return ObjectiveStatus.Completed;
            }

            if (SaveData.Instance == null) {
                return ObjectiveStatus.Nothing;
            }

            if (!BingoMonitor.Objectives.ContainsKey(this.Board[i].Text)) {
                return ObjectiveStatus.Unknown;
            }

            var progress = BingoMonitor.ObjectiveProgress(this.Board[i].Text);
            if (progress < 0.001f) {
                return ObjectiveStatus.Nothing;
            }

            if (progress > 0.999f) {
                return ObjectiveStatus.Completed;
            }

            return ObjectiveStatus.Progress;
        }

        public bool IsObjectiveClaimable(int i, bool claiming = true) {
            if (this.IsObjectiveHidden(i)) {
                return false;
            }
            if (claiming != (this.ObjectivesCompleted?[i] ?? false)) {
                return false;
            }
            if (this.Board == null) {
                return false;
            }
            bool b;
            if (this.IsLockout) {
                b = this.Board[i].Colors.Count == 0;
            } else {
                b = !this.Board[i].Colors.Contains(this.ModSettings.PlayerColor);
            }
            return b == claiming;
        }

        public IEnumerable<BingoVariant> RelevantVariants() {
            var checkpoint = BingoMonitor.AtCheckpoint();
            if (checkpoint == null) {
                yield break;
            }
            var area = SaveData.Instance.CurrentSession.Area;

            var seen = new HashSet<BingoVariant>();
            int i = -1;
            foreach (var square in Instance.Board) {
                i++;
                if (this.IsObjectiveHidden(i)) {
                    continue;
                }
                if(square.Text == "Grabless Rock Bottom" ||  square.Text == "Grabless Rock Bottom (6A/6B Checkpoint)")
                {
                    if((area.ID == 6) && ((int)area.Mode == 0 && checkpoint == 4) ||
                                        ((int)area.Mode == 1 && checkpoint == 2)) {
                        yield return BingoVariant.NoGrab;
                        continue;
                    }
                }

                if (!BingoMonitor.ObjectiveVariants.TryGetValue(square.Text, out var variants)) {
                    continue;
                }

                foreach (var entry in variants) {
                    if ((entry.Item1 == area.ID || entry.Item1 == -1) &&
                        (entry.Item2 == (int) area.Mode || entry.Item2 == -1) &&
                        (entry.Item3 == checkpoint || entry.Item3 == -1) &&
                        !seen.Contains(entry.Item4)) {
                        seen.Add(entry.Item4);
                        yield return entry.Item4;
                    }
                }
            }
        }
    }
}
