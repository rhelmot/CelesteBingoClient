using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.UI;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        public bool MenuToggled, MenuTriggered;
        public bool BoardSelected = true;
        public int BoardSelX, BoardSelY;
        private Vector2 MousePos;
        private bool MouseShown;
        public int BoardSelSlot => this.BoardSelX + this.BoardSelY * 5;
        private Wiggler Wiggle = Wiggler.Create(0.25f, 3f);

        private const int DISABLE_OFFSET = 1;
        private const int ENABLE_OFFSET = 11;
        private const int OBJECTIVE_OFFSET = 23;
        private const int PINNED_OFFSET = 49;

        public TextMenu Menu;
        public List<int> Pinned = new List<int>();

        #region init
        private void InitMenu() {
            Menu = new TextMenu {
                new TextMenuExt.SubHeaderExt(Dialog.Clean("bingoclient_menu_variants")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablegrabless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disabledashless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablejumpless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disableinvisible")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablelowfriction")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablespeed70")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablespeed160")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablejumplessdashless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablemirrored")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_disablehiccups")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablegrabless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enabledashless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablejumpless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enableinvisible")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablelowfriction")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablespeed70")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablespeed160")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablejumplessdashless")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablemirrored")),
                new TextMenuExt.ButtonExt(Dialog.Clean("bingoclient_menu_enablehiccups")),
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
                new TextMenuExt.SubHeaderExt(Dialog.Clean("bingoclient_menu_pinned")),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
                new PieButton("X"),
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

            Action makePinnedCallback(int i) {
                return () => {
                    var j = this.Pinned[i];
                    this.BoardSelX = j % 5;
                    this.BoardSelY = j / 5;
                    this.BoardSelected = true;
                    this.OnMotion();
                    Input.MenuConfirm.ConsumePress();
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

            for (var idx = 0; idx < 25; idx++) {
                var button = this.Menu.Items[PINNED_OFFSET + idx] as PieButton ?? throw new Exception("programming error");
                button.OnPressed += makePinnedCallback(idx);
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

        public PieButton GetPinnedButton(int idx) {
            return (PieButton) this.Menu.Items[PINNED_OFFSET + idx];
        }
        #endregion

        #region update
        public void ToggleSquare(int i) {
            if (this.Board[i].Colors.Contains(this.ModSettings.PlayerColor)) {
                this.SendClear(i);
            } else {
                this.SendClaim(i);
            }
        }

        public void PinSquare(int i) {
            if (!this.Pinned.Contains(i)) {
                this.Pinned.Add(i);
            } else {
                this.Pinned.Remove(i);
            }
        }

        public static bool IsInappropriateTimeForMenu() {
            return (Engine.Scene is Overworld ow && (ow.Current is OuiModOptionString || ow.Current is OuiFileNaming)) ||
                Engine.Scene.Entities.FindFirst<ButtonConfigUI>() != null ||
                Engine.Scene.Entities.FindFirst<KeyboardConfigUI>() != null;
        }

        private bool FirstFrame = false;
        private void UpdateMenu() {
            this.FirstFrame = false;
            if ((this.ModSettings.MenuToggle.Pressed
                 || (!this.MenuToggled && (MInput.Mouse.PressedLeftButton || MInput.Mouse.PressedRightButton))
                 || (this.MenuToggled && (Input.MenuCancel.Pressed || Input.ESC.Pressed)))
                && !IsInappropriateTimeForMenu()) {
                this.MenuToggled ^= true;
                this.FirstFrame = true;
                Audio.Play(this.MenuToggled ? SFX.ui_game_pause : SFX.ui_game_unpause);
                if (this.MenuToggled) {
                    if (this.Menu != null) {
                        this.Menu.Selection = this.Menu.FirstPossibleSelection;
                    }
                }
            }

            if (this.ModSettings.TriggerBehavior == BingoClientSettings.TriggerMode.Hasty) {
                this.MenuTriggered = this.ModSettings.MenuTrigger.Check && !IsInappropriateTimeForMenu();
            } else if (this.ModSettings.MenuTrigger.Pressed && !IsInappropriateTimeForMenu()) {
                this.MenuTriggered ^= true;
            }

            if (this.MenuToggled || this.MenuTriggered) {
                this.UpdateMenuOpen();
            }
        }

        private void OnMotion() {
            Audio.Play(SFX.ui_main_rename_entry_roll);
            this.Wiggle.Start();
        }

        private void UpdateMenuOpen() {
            if (this.IsBoardHidden && (Input.MenuConfirm.Pressed || MInput.Mouse.PressedLeftButton)) {
                this.RevealBoard();
                Input.MenuConfirm.ConsumePress();
                MInput.Mouse.PreviousState = MInput.Mouse.CurrentState; // hack to consume press
            }

            this.MousePos = MInput.Mouse.Position;
            this.MouseShown |= MInput.Mouse.WasMoved;

            if (this.IsBoardHidden || !this.Connected) {
                return;
            }

            var mouseInBounds = this.MousePos.X >= corner.X && this.MousePos.X < corner.X + size.X && this.MousePos.Y >= corner.Y && this.MousePos.Y < corner.Y + size.Y;
            if (this.MouseShown && mouseInBounds) {
                this.BoardSelected = true;
                var oldPos = this.BoardSelSlot;
                this.BoardSelX = (int) Math.Floor((this.MousePos.X - corner.X) / size.X * 5);
                this.BoardSelY = (int) Math.Floor((this.MousePos.Y - corner.Y) / size.Y * 5);
                if (oldPos != this.BoardSelSlot) {
                    this.OnMotion();
                }
            }

            this.Wiggle.UseRawDeltaTime = true;
            this.Wiggle.Update();

            if (this.Menu == null) {
                this.InitMenu();
            }
            this.Menu.Focused = !this.BoardSelected && !this.MenuTriggered;

            // visibility and text for claim buttons
            var anyVisible = false;
            if (this.Board != null) {
                for (var i = 0; i < 25; i++) {
                    this.GetSlotButton(i).Label = this.Board[i].Text;
                    var visible = this.ModSettings.ClaimAssist && this.IsObjectiveClaimable(i);
                    this.GetSlotButton(i).Visible = visible;
                    anyVisible |= visible;
                }
            }

            // visibility and status for pinned buttons
            if (this.Board != null) {
                for (var i = 0; i < 25; i++) {
                    if (i >= this.Pinned.Count) {
                        this.GetPinnedButton(i).Visible = false;
                    } else {
                        var j = this.Pinned[i];
                        var btn = this.GetPinnedButton(i);
                        btn.Visible = true;
                        btn.Label = this.Board[j].Text;

                        switch (this.GetObjectiveStatus(j)) {
                            case ObjectiveStatus.Unknown:
                                btn.Progress = 0f;
                                btn.PieText = "?";
                                break;
                            case ObjectiveStatus.Progress:
                            case ObjectiveStatus.Nothing:
                                btn.Progress = BingoMonitor.ObjectiveProgress(this.Board[j].Text);
                                btn.PieText = "";
                                break;
                            case ObjectiveStatus.Completed:
                                btn.Progress = 1f;
                                btn.PieText = "!";
                                break;
                            case ObjectiveStatus.Claimed:
                                if (this.ModSettings.AutoUnpin) {
                                    btn.Visible = false;
                                    this.PinSquare(j);
                                    i--;
                                } else {
                                    btn.Progress = 0f;
                                    btn.PieText = "*";
                                }
                                break;
                        }
                    }
                }
            }

            // visibility for claim header and claimall
            this.Menu.Items[OBJECTIVE_OFFSET - 1].Visible = anyVisible;
            this.Menu.Items[OBJECTIVE_OFFSET - 2].Visible = anyVisible;

            // visibility for pin header
            this.Menu.Items[PINNED_OFFSET - 1].Visible = this.Pinned.Count != 0;

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

            if (!this.MenuTriggered && !this.FirstFrame && !this.Chat.ChatOpen) {
                // only handle keypresses if we're not in trigger mode or on the first frame of input or have the chat open

                if (this.BoardSelected) {
                    if (Input.MenuUp.Pressed) {
                        this.MouseShown = false;
                        this.BoardSelY--;
                        if (this.BoardSelY < 0) {
                            this.BoardSelY = 4;
                        }
                        this.OnMotion();
                    }
                    if (Input.MenuDown.Pressed) {
                        this.MouseShown = false;
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
                    if (this.ModSettings.PinObjective.Pressed) {
                        Audio.Play(SFX.ui_main_button_select);
                        this.Wiggle.Start();
                        this.PinSquare(this.BoardSelSlot);
                    }
                }
                if (Input.MenuLeft.Pressed) {
                    this.MouseShown = false;
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
                    this.MouseShown = false;
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

            if (!this.FirstFrame && !this.Chat.ChatOpen) {
                // handle mouse clicks even in trigger mode
                if (this.MouseShown && !mouseInBounds && (MInput.Mouse.PressedLeftButton || MInput.Mouse.PressedRightButton)) {
                    this.MenuToggled = false;
                    Audio.Play(SFX.ui_game_unpause);
                }
                if (this.MouseShown && mouseInBounds && MInput.Mouse.PressedLeftButton) {
                    Audio.Play(SFX.ui_main_button_select);
                    this.Wiggle.Start();
                    this.ToggleSquare(this.BoardSelSlot);
                }
                if (this.MouseShown && mouseInBounds && MInput.Mouse.PressedRightButton) {
                    Audio.Play(SFX.ui_main_button_select);
                    this.Wiggle.Start();
                    this.PinSquare(this.BoardSelSlot);
                }
            }
        }
        #endregion

        #region render

        public static readonly Vector2 size = Vector2.One * 1080f * 4f / 5f;
        public static readonly Vector2 subsize = size / 5f;
        public static readonly Vector2 corner = new Vector2(1920f * 1f / 12f, 1080f / 2f - size.Y / 2f);
        public static readonly float margin = 1f / 20f;
        public static readonly float padding = 1f / 10f;

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

            float masterAlpha;
            if (this.MenuTriggered) {
                masterAlpha = this.ModSettings.TriggerAlpha == BingoClientSettings.TriggerAlphaMode.High ? 1f : this.ModSettings.TriggerAlpha == BingoClientSettings.TriggerAlphaMode.Medium ? 0.6f : 0.4f;
            } else {
                masterAlpha = 1;
            }

            Draw.Rect(0, 0, 1920f, 1080f, Color.Black * 0.5f * masterAlpha);

            if (!this.Connected || this.Board == null) {
                ActiveFont.DrawOutline(
                    Dialog.Clean("bingoclient_menu_noboard"),
                    new Vector2(1920f, 1080f) / 2f,
                    new Vector2(0.5f, 0.5f),
                    Vector2.One,
                    Color.White,
                    1,
                    Color.Black);

                if (this.MouseShown) {
                    GFX.Gui["menu/bingo/cursor"].Draw(this.MousePos, Vector2.Zero, Color.White, 0.5f);
                }
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

                if (this.MouseShown) {
                    GFX.Gui["menu/bingo/cursor"].Draw(this.MousePos, Vector2.Zero, Color.White, 0.5f);
                }
                Draw.SpriteBatch.End();
                return;
            }

            var wiggle = this.Wiggle.Value;

            // draw boxes and text
            for (int x = 0; x < 5; x++) {
                for (int y = 0; y < 5; y++) {
                    var slot = y * 5 + x;
                    var subcorner = corner + subsize * new Vector2(x, y);

                    if (!this.MenuTriggered && x == this.BoardSelX && y == this.BoardSelY) {
                        Draw.Rect(subcorner - Vector2.One * wiggle * 3f + Vector2.UnitY * wiggle * 2f, subsize.X + wiggle*3*2, subsize.Y + wiggle*3*2, Color.WhiteSmoke * masterAlpha);
                    }

                    if (this.Board[slot].Colors.Count == 0) {
                        Draw.Rect(subcorner + subsize * margin / 2, subsize.X * (1 - margin), subsize.Y * (1 - margin), BingoColors.Blank.ToSquareColor() * masterAlpha);
                    } else {
                        var chunkWidth = subsize.X * (1 - margin) / this.Board[slot].Colors.Count;
                        for (var i = 0; i < this.Board[slot].Colors.Count; i++) {
                            Draw.Rect(subcorner + subsize * margin / 2 + Vector2.UnitX * chunkWidth * i, chunkWidth, subsize.Y * (1 - margin), this.Board[slot].Colors[i].ToSquareColor() * masterAlpha);
                        }
                    }

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
                    if (status == ObjectiveStatus.Completed) {
                        PieButton.DrawPieAndText(subcorner, 0.5f, 1f, "!");
                    }

                }
            }

            var scoreCorner = new Vector2(20f, 20f);
            foreach (var entry in this.Score()) {
                var scoreSize1 = new Vector2(100f, 100f);
                Draw.Rect(scoreCorner, scoreSize1.X, scoreSize1.Y, entry.Item1.ToSquareColor());
                ActiveFont.DrawOutline(entry.Item2.ToString(), scoreCorner + scoreSize1 / 2, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.White, 2f, Color.Black);
                scoreCorner += new Vector2(0f, 120f);
            }

            this.Menu?.Render();

            if (this.MouseShown) {
                GFX.Gui["menu/bingo/cursor"].Draw(this.MousePos, Vector2.Zero, Color.White, 0.5f);
            }

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

        public class PieButton : TextMenuExt.ButtonExt {
            public float Progress;
            public string PieText;

            public PieButton(string label) : base(label) {
            }

            public static Vector2 PieOffset = new Vector2(-30f, 0f);

            public override void Render(Vector2 position, bool highlighted) {
                base.Render(position, highlighted);
                DrawPieAndText(position + PieOffset, this.Scale.Y * 0.5f, this.Progress, this.PieText);
            }

            private static MTexture CircleDark => GFX.Gui["menu/bingo/dark"];
            private static MTexture CircleLight => GFX.Gui["menu/bingo/light"];
            private static MTexture CircleSlice => GFX.Gui["menu/bingo/slice"];
            private static Vector2 Origin = new Vector2(50f, 50f);

            public static void DrawPie(Vector2 position, float scale, float completion) {
                if (completion < 0.999f) {
                    CircleLight.Draw(position, Origin, Color.White, scale, 0f);
                    if (completion >= 0.001f) {
                        var progressInt = Calc.Clamp((int) (completion * 30), 1, 29);
                        for (var i = 0; i < progressInt; i++) {
                            CircleSlice.Draw(position, Origin, Color.White, scale, MathHelper.WrapAngle(MathHelper.TwoPi / 30f * i));
                        }

                        if (completion > 1f / 30f) {
                            CircleSlice.Draw(position, Origin, Color.White, scale, MathHelper.WrapAngle(MathHelper.TwoPi * (completion - 1f / 30f)));
                        }
                    }
                } else {
                    CircleDark.Draw(position, Origin, Color.White, scale, 0f);
                }
            }

            public static void DrawPieAndText(Vector2 position, float scale, float completion, string text) {
                DrawPie(position, scale, completion);

                if (!string.IsNullOrEmpty(text)) {
                    ActiveFont.Draw(text, position, new Vector2(0.5f, 0.5f), Vector2.One * scale * 1.25f, Color.Black);
                }
            }
        }
        #endregion
    }
}
