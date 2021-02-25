using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.UI;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        public bool MenuToggled, MenuTriggered;
        public bool BoardSelected = true;
        public int BoardSelX, BoardSelY;
        public int BoardSelSlot => this.BoardSelX + this.BoardSelY * 5;
        private Wiggler Wiggle = Wiggler.Create(0.25f, 3f);

        private const int DISABLE_OFFSET = 1;
        private const int ENABLE_OFFSET = 4;
        private const int OBJECTIVE_OFFSET = 9;

        public TextMenu Menu;

        private MTexture CircleDark, CircleLight, CircleSlice;

        #region init
        private void InitMenu() {
            CircleDark = GFX.Gui["menu/bingo/dark"];
            CircleLight = GFX.Gui["menu/bingo/light"];
            CircleSlice = GFX.Gui["menu/bingo/slice"];
            
            Menu = new TextMenu {
                new TextMenuExt.SubHeaderExt(Dialog.Clean("bingoclient_menu_variants")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablegrabless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disabledashless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablejumpless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablegrabless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enabledashless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablejumpless")),
                new TextMenuExt.SubHeaderExt(Dialog.Clean("bingoclient_menu_objectives")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_claimall")),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
                new TextMenuExt.ButtonExt("X"),
            };

            foreach (var item in this.Menu.Items) {
                if (item is TextMenuExt.ButtonExt button) {
                    button.Scale = Vector2.One * 0.8f;
                }
            }

            Action makeMenuCallback(int i) {
                return () => {
                    this.SendClaim(i);
                };
            }

            Action makeVariantCallback(BingoVariant v, bool value) {
                return () => {
                    BingoMonitor.SetVariantEnabled(v, value);
                    if (value) {
                        this.ModSession.CheckpointStartedVariant = BingoMonitor.AtCheckpoint();
                    }
                };
            }

            for (int idx = 0; idx < 25; idx++) {
                var button = this.Menu.Items[OBJECTIVE_OFFSET + idx] as TextMenuExt.ButtonExt ?? throw new Exception("programming error");
                button.OnPressed = makeMenuCallback(idx);
            }
            
            foreach (BingoVariant variant in typeof(BingoVariant).GetEnumValues()) {
                this.GetDisableButton(variant).OnPressed = makeVariantCallback(variant, false);
                this.GetEnableButton(variant).OnPressed = makeVariantCallback(variant, true);
            }

            (this.Menu.Items[OBJECTIVE_OFFSET - 1] as TextMenuExt.ButtonExt ?? throw new Exception("programming error")).OnPressed = () => {
                for (int idx = 0; idx < 25; idx++) {
                    if (this.IsObjectiveClaimable(idx)) {
                        this.SendClaim(idx);
                    }
                }
            };
            
            this.Menu.Justify = new Vector2(0.0f, 0.5f);
            this.Menu.InnerContent = TextMenu.InnerContentMode.TwoColumn;
            this.Menu.Position = new Vector2(1100f, 0f);
            this.Menu.ItemSpacing = 3f;
        }
        
        public TextMenuExt.ButtonExt GetSlotButton(int idx) {
            return (TextMenuExt.ButtonExt) this.Menu.Items[OBJECTIVE_OFFSET + idx];
        }
        
        public TextMenuExt.ButtonExt GetEnableButton(BingoVariant idx) {
            return (TextMenuExt.ButtonExt) this.Menu.Items[ENABLE_OFFSET + (int)idx];
        }
        
        public TextMenuExt.ButtonExt GetDisableButton(BingoVariant idx) {
            return (TextMenuExt.ButtonExt) this.Menu.Items[DISABLE_OFFSET + (int)idx];
        }
        #endregion

        #region update
        public void ToggleSquare(int i) {
            if (this.Board[i].Color == Color.Black) {
                this.SendClaim(i);
            } else if (this.Board[i].Color == this.ModSettings.PlayerColor.ToColor()) {
                this.SendClear(i);
            }
        }

        public static bool IsInappropriateTimeForMenu() {
            return (Engine.Scene is Overworld ow && (ow.Current is OuiModOptionString || ow.Current is OuiFileNaming)) ||
                Engine.Scene.Entities.FindFirst<ButtonConfigUI>() != null ||
                Engine.Scene.Entities.FindFirst<KeyboardConfigUI>() != null;
        }

        private void PreUpdateMenu() {
            // this runs with higher priority and is in charge of controlling the flow of time
            Engine.OverloadGameLoop = null;
            if ((this.ModSettings.MenuToggle.Pressed || (this.MenuToggled && Input.MenuCancel.Pressed)) && !IsInappropriateTimeForMenu()) {
                this.MenuToggled ^= true;
                Audio.Play(this.MenuToggled ? SFX.ui_game_pause : SFX.ui_game_unpause);
                // if we're unpausing, hijack the game for another frame to eat the unpause input
                if (this.MenuToggled) {
                    if (this.Menu != null) {
                        this.Menu.Selection = this.Menu.FirstPossibleSelection;
                    }
                } else {
                    Engine.OverloadGameLoop = () => { };
                }
            }
            this.MenuTriggered = this.ModSettings.MenuTrigger.Check && !IsInappropriateTimeForMenu();

            if (this.MenuTriggered) {
                this.UpdateMenuOpen();
            } else if (this.MenuToggled) {
                Engine.OverloadGameLoop = this.UpdateMenuOpen;
            }
        }

        private void OnMotion() {
            Audio.Play(SFX.ui_main_rename_entry_roll);
            this.Wiggle.Start();
        }

        private void UpdateMenuOpen() {
            if (!this.MenuTriggered) {
                // force update the console
                if (Engine.Commands.Open) {
                    typeof(Monocle.Commands).GetMethod("UpdateOpen", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(Engine.Commands, new object[] { });
                } else if (Engine.Commands.Enabled) {
                    typeof(Monocle.Commands).GetMethod("UpdateClosed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(Engine.Commands, new object[] { });
                }
            }

            if (this.IsBoardHidden && Input.MenuConfirm.Pressed) {
                this.RevealBoard();
                Input.MenuConfirm.ConsumePress();
            }

            if (this.IsBoardHidden || !this.Connected) {
                return;
            }
            
            this.Wiggle.UseRawDeltaTime = true;
            this.Wiggle.Update();

            if (this.Menu == null) {
                this.InitMenu();
            }
            this.Menu.Focused = !this.BoardSelected && !this.MenuTriggered;
            
            // visibility for claim buttons
            var anyVisible = false;
            if (this.Board != null) {
                for (var i = 0; i < 25; i++) {
                    this.GetSlotButton(i).Label = this.Board[i].Text;
                    var visible = this.IsObjectiveClaimable(i);
                    this.GetSlotButton(i).Visible = visible;
                    anyVisible |= visible;
                }
            }

            // visibility for claim header and claimall
            this.Menu.Items[OBJECTIVE_OFFSET - 1].Visible = anyVisible;
            this.Menu.Items[OBJECTIVE_OFFSET - 2].Visible = anyVisible;

            // visibility for variant buttons
            anyVisible = false;
            foreach (BingoVariant variant in typeof(BingoVariant).GetEnumValues()) {
                anyVisible |= (GetDisableButton(variant).Visible = BingoMonitor.IsVariantEnabled(variant));
                GetEnableButton(variant).Visible = false;
            }
            foreach (var variant in RelevantVariants()) {
                if (!BingoMonitor.IsVariantEnabled(variant)) {
                    GetEnableButton(variant).Visible = true;
                    anyVisible = true;
                }
            }
            
            // visibility for variant button
            this.Menu.Items[0].Visible = anyVisible;
            
            this.Menu.Update();

            if (!this.MenuTriggered) {
                // only handle keypresses if we're not in trigger mode
                
                if (this.BoardSelected) {
                    if (Input.MenuUp.Pressed) {
                        this.BoardSelY--;
                        if (this.BoardSelY < 0) {
                            this.BoardSelY = 4;
                        }
                        this.OnMotion();
                    }
                    if (Input.MenuDown.Pressed) {
                        this.BoardSelY++;
                        if (this.BoardSelY >= 5) {
                            this.BoardSelY = 0;
                        }
                        this.OnMotion();
                    }
                    if (Input.MenuConfirm.Pressed) {
                        Audio.Play(SFX.ui_main_button_select);
                        this.Wiggle.Start();
                        this.ToggleSquare(this.BoardSelSlot);
                    }
                }
                if (Input.MenuLeft.Pressed) {
                    this.OnMotion();
                    if (!this.BoardSelected) {
                        this.BoardSelX = 4;
                        this.BoardSelected = true;
                    } else {
                        this.BoardSelX--;
                        if (this.BoardSelX < 0) {
                            this.BoardSelected = false;
                            this.Menu.Selection = this.Menu.FirstPossibleSelection;
                            this.Menu.Current?.SelectWiggler.Start();
                        }
                    }
                }
                if (Input.MenuRight.Pressed) {
                    this.OnMotion();
                    if (!this.BoardSelected) {
                        this.BoardSelX = 0;
                        this.BoardSelected = true;
                    } else {
                        this.BoardSelX++;
                        if (this.BoardSelX >= 5) {
                            this.BoardSelected = false;
                            this.Menu.Selection = this.Menu.FirstPossibleSelection;
                            this.Menu.Current?.SelectWiggler.Start();
                        }
                    }
                }
            }
        }
        #endregion
        
        #region render
        private void RenderMenu() {
            if (!this.MenuTriggered && !this.MenuToggled) {
                return;
            }

            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null,
                Engine.ScreenMatrix);
            
            // TODO make this a setting
            var masterAlpha = this.MenuTriggered ? 0.6f : 1f;
            
            Draw.Rect(0, 0, 1920f, 1080f, Color.Black * 0.5f * masterAlpha);

            if (!this.Connected) {
                ActiveFont.DrawOutline(
                    Dialog.Clean("bingoclient_menu_noboard"),
                    new Vector2(1920f, 1080f) / 2f,
                    new Vector2(0.5f, 0.5f),
                    Vector2.One,
                    Color.White,
                    1,
                    Color.Black);
                Draw.SpriteBatch.End();
                return;
            }

            if (this.IsBoardHidden) {
                ActiveFont.DrawOutline(
                    Dialog.Clean("bingoclient_menu_hidden"),
                    new Vector2(1920f, 1080f) / 2f,
                    new Vector2(0.5f, 0.5f),
                    Vector2.One,
                    Color.White,
                    1,
                    Color.Black);
                Draw.SpriteBatch.End();
                return;
            }
            
            var size = Vector2.One * 1080f * 4f / 5f;
            var subsize = size / 5f;
            var corner = new Vector2(1920f * 1f / 12f, 1080f / 2f - size.Y / 2f);
            var margin = 1f / 20f;
            var padding = 1f / 10f;
            var wiggle = this.Wiggle.Value;
            
            // draw boxes and text
            for (int x = 0; x < 5; x++) {
                for (int y = 0; y < 5; y++) {
                    var slot = y * 5 + x;
                    var subcorner = corner + subsize * new Vector2(x, y);

                    if (!this.MenuTriggered && x == this.BoardSelX && y == this.BoardSelY) {
                        Draw.Rect(subcorner - Vector2.One * wiggle * 3f + Vector2.UnitY * wiggle * 2f, subsize.X + wiggle*3*2, subsize.Y + wiggle*3*2, Color.WhiteSmoke * masterAlpha);
                    }
                    
                    Draw.Rect(subcorner + subsize * margin / 2, subsize.X * (1 - margin), subsize.Y * (1 - margin), this.Board[slot].Color * masterAlpha);
                    DrawTextBox(this.Board[slot].Text, subcorner + subsize / 2, subsize.X * (1 - padding), subsize.Y * (1 - padding), 0.5f, 1.0f, Color.White, 1f, Color.Black);
                }
            }
            
            // draw notification icons
            for (int x = 0; x < 5; x++) {
                for (int y = 0; y < 5; y++) {
                    var slot = y * 5 + x;
                    var subcorner = corner + subsize * new Vector2(x, y) + Vector2.One * 15f;

                    var status = this.GetObjectiveStatus(slot);
                    var origin = new Vector2(50f, 50f);
                    var scale = Vector2.One * 0.4f;
                    var textScale = Vector2.One * 0.5f;
                    switch (status) {
                        case ObjectiveStatus.Completed:
                            this.CircleDark.Draw(subcorner, origin, Color.White, scale, 0f);
                            ActiveFont.Draw("!", subcorner, new Vector2(0.5f, 0.5f), textScale, Color.Black);
                            break;
                        case ObjectiveStatus.Unknown:
                            this.CircleLight.Draw(subcorner, origin, Color.White, scale, 0f);
                            ActiveFont.Draw("?", subcorner, new Vector2(0.5f, 0.5f), textScale, Color.Black);
                            break;
                        case ObjectiveStatus.Progress:
                            this.CircleLight.Draw(subcorner, origin, Color.White, scale, 0f);
                            var progress = BingoMonitor.ObjectiveProgress(this.Board[slot].Text);
                            var progressInt = Calc.Clamp((int) (progress * 30), 1, 29);
                            for (var i = 0; i < progressInt; i++) {
                                this.CircleSlice.Draw(subcorner, origin, Color.White, scale, MathHelper.WrapAngle(MathHelper.TwoPi / 30f * i));
                            }

                            if (progress > 1f / 30f) {
                                this.CircleSlice.Draw(subcorner, origin, Color.White, scale, MathHelper.WrapAngle(MathHelper.TwoPi * (progress - 1f / 30f)));
                            }
                            break;
                    }
                }
            }
            
            this.Menu?.Render();
            Draw.SpriteBatch.End();
        }

        public static void DrawTextBox(string text, Vector2 center, float width, float height, float scale, float lineHeight, Color color, float stroke, Color strokeColor) {
            var words = text.Split(' ');
            var singleHeight = ActiveFont.Measure(text).Y;
            while (true) {
                var cumHeight = singleHeight * scale;
                var result = new List<string>();
                string line = null;
                foreach (var word in words) {
                    tryagain:
                    string maybeline = line == null ? word : line + ' ' + word;
                    var linewidth = ActiveFont.Measure(maybeline).X * scale;
                    if (linewidth > width) {
                        if (line == null) {
                            result = null;
                            break;
                        } else {
                            cumHeight += singleHeight * lineHeight * scale;
                            if (cumHeight > height) {
                                result = null;
                                break;
                            }
                            result.Add(line);
                            line = null;
                            goto tryagain;  // continue without advancing foreach
                        }
                    }

                    line = maybeline;
                }

                if (result == null) {
                    scale *= 0.8f;
                } else {
                    if (line != null) {
                        result.Add(line);
                    }

                    var offsetY = -singleHeight * lineHeight * scale * (result.Count - 1) / 2;
                    foreach (var finalline in result) {
                        ActiveFont.DrawOutline(finalline, center + Vector2.UnitY * offsetY, new Vector2(0.5f, 0.5f), Vector2.One * scale, color, stroke, strokeColor);
                        offsetY += singleHeight * lineHeight * scale;
                    }

                    return;
                }
            }
        }
        #endregion
    }
}
