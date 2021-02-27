using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        private List<BingoSquare> Board;
        public List<bool> ObjectivesCompleted;

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
            var rendered = msg.Render();
            if (rendered != null) {
                this.LogChat(rendered);
            }
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
                    break;
                }
                case "new-card":
                    var settings = this.GetSettings();
                    this.IsBoardHidden = settings.Item1;
                    this.IsLockout = settings.Item2;
                    this.RefreshBoard();
                    break;
                case "color":
                case "chat":
                case "revealed":
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
            public List<BingoColors> Colors = new List<BingoColors>();
            public string Text = "";
        }

        public void UpdateObjectives() {
            if (this.ObjectivesCompleted == null) {
                return;
            }

            if (!(Engine.Scene is Level)) {
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
                
                if (this.ModSettings.QuickClaim.Pressed) {
                    this.SendClaim(i);
                }
            }
        }

        public ObjectiveStatus GetObjectiveStatus(int i) {
            if (this.Board[i].Colors.Contains(this.ModSettings.PlayerColor)) {
                return ObjectiveStatus.Claimed;
            }

            if (this.ObjectivesCompleted[i]) {
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

        public bool IsObjectiveClaimable(int i) {
            return this.Board[i].Colors.Count == 0 && this.ObjectivesCompleted[i];
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
