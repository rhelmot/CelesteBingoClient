using System;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.BingoClient {
    public class BingoChat {
        private const float ChatTime = 4f;
        private List<string> ChatMessages = new List<string>();
        private List<string> ChatHistory = new List<string>();
        private List<float> ChatTimers = new List<float>();

        public bool ChatOpen;
        private string Buffer = "";
        private bool Underscore;
        private float UnderscoreCounter;
        private bool InhibitOne;
        private Action<string> Submit;

        public BingoChat(Action<string> submit) {
            this.Submit = submit;
            TextInput.OnInput += OnInput;
        }

        public void Update() {
            lock (this.ChatMessages) {
                for (int i = 0; i < this.ChatTimers.Count; i++) {
                    this.ChatTimers[i] += Engine.RawDeltaTime;
                    if (this.ChatTimers[i] > ChatTime) {
                        this.ChatTimers.RemoveAt(i);
                        this.ChatMessages.RemoveAt(i);
                        i--;
                    }
                }
            }

            if (this.ChatOpen) {
                if (Input.ESC.Pressed) {
                    this.ChatOpen = false;
                }
            } else {
                if (BingoClient.Instance.ModSettings.OpenChat.Pressed &&
                    !Engine.Commands.Open &&
                    !BingoClient.Instance.IsInappropriateTimeForMenu() &&
                    (this.ChatHistory.Count > 0 || BingoClient.Instance.Connected)) {
                    this.ChatOpen = true;
                    this.InhibitOne = true;
                    Engine.Scene.OnEndOfFrame += () => this.InhibitOne = false;
                }
            }

            this.UnderscoreCounter += Engine.RawDeltaTime;
            while (this.UnderscoreCounter >= 0.5f) {
              this.UnderscoreCounter -= 0.5f;
              this.Underscore = !this.Underscore;
            }
        }

        private void OnInput(char ch) {
            if (this.InhibitOne) {
                this.InhibitOne = false;
                return;
            }

            if (!this.ChatOpen) {
                return;
            }

            this.Underscore = false;
            this.UnderscoreCounter = 0f;
            if (ch == '\r' || ch == '\n') {
                if (this.Buffer != "") {
                    this.Submit(this.Buffer);
                    this.Buffer = "";
                }
            } else if (ch == '\b') {
                if (this.Buffer.Length > 0) {
                    this.Buffer = this.Buffer.Substring(0, this.Buffer.Length - 1);
                }
            } else if (!char.IsControl(ch)) {
                this.Buffer += ch;
            }
        }

        public void Render() {
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Engine.ScreenMatrix);

            var currentBase = 1080f - 50f;
            var scale = 0.7f;
            var nLines = 0;
            lock (this.ChatMessages) {
                var chatTexts = this.ChatOpen ? this.ChatHistory : this.ChatMessages;
                var chatTimers = this.ChatOpen ? null : this.ChatTimers;
                for (int i = chatTexts.Count - 1; i >= 0 && currentBase > 0; i--) {
                    if (nLines >= 5 && !this.ChatOpen) {
                        this.ChatTimers[i] = Math.Max(this.ChatTimers[i], ChatTime - 0.5f);
                    }
                    var timer = chatTimers?[i] ?? (ChatTime / 2f);
                    var text = chatTexts[i];
                    var alpha = timer < 0.25f ? timer * 4f : timer > (ChatTime - 0.5f) ? (ChatTime - timer) * 2 : 1f;
                    var rise = timer < 0.25f ? timer * 4f : 1f;

                    var textSize = ActiveFont.Measure(text) * scale;
                    var lines = new List<string>();
                    if (textSize.X > 1880f) {
                        while (!string.IsNullOrEmpty(text)) {
                            var textSplit = new List<string>(text.Split(' '));
                            textSplit.Reverse();
                            text = "";
                            while (textSplit.Count > 0) {
                                var maybetext = text + ' ' + textSplit[textSplit.Count - 1];
                                if (ActiveFont.Measure(maybetext).X * scale > 1880f) {
                                    break;
                                }
                                text = maybetext;
                                textSplit.RemoveAt(textSplit.Count - 1);
                            }

                            if (text == "") {
                                text = textSplit[textSplit.Count - 1];
                                textSplit.RemoveAt(textSplit.Count - 1);
                            }

                            lines.Add(text);
                            textSplit.Reverse();
                            text = string.Join(" ", textSplit);
                        }
                    } else {
                        lines.Add(text);
                    }

                    for (var j = lines.Count - 1; j >= 0; j--) {
                        ActiveFont.DrawOutline(lines[j], new Vector2(1920f - 20, currentBase), new Vector2(1f, rise), Vector2.One * scale, Color.White * alpha, 2f, Color.Black * alpha);
                        currentBase -= textSize.Y * (j == 0 ? 1.1f : 0.9f) * rise;
                        nLines++;
                    }
                }
            }

            if (this.ChatOpen) {
                var uch = this.Underscore ? "_" : "";
                var prompt = "> " + this.Buffer;
                var width = ActiveFont.Measure(prompt).X * scale;
                if (width > 1920f - 20f) {
                    ActiveFont.DrawOutline(prompt, new Vector2(1920f - 10f, 1080f - 10f), new Vector2(1f, 1f), Vector2.One * scale, Color.White, 2f, Color.Black);
                    ActiveFont.DrawOutline(uch, new Vector2(1920f - 10f, 1080f - 10f), new Vector2(0f, 1f), Vector2.One * scale, Color.White, 2f, Color.Black);
                } else {
                    ActiveFont.DrawOutline(prompt, new Vector2(10, 1080f - 10f), new Vector2(0f, 1f), Vector2.One * scale, Color.White, 2f, Color.Black);
                    ActiveFont.DrawOutline(uch, new Vector2(10 + width, 1080f - 10f), new Vector2(0f, 1f), Vector2.One * scale, Color.White, 2f, Color.Black);
                }
            }
            Draw.SpriteBatch.End();
        }

        public void Chat(string text) {
            lock (this.ChatMessages) {
                this.ChatHistory.Add(text);
                this.ChatMessages.Add(text);
                this.ChatTimers.Add(0f);
            }
        }

        public void SetHistory(IEnumerable<string> history) {
            this.ChatHistory.Clear();
            this.ChatHistory.AddRange(history);
        }
    }
}
