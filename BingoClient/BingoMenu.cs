using System;
using System.Collections.Generic;
using Celeste.Mod.UI;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        public bool MenuToggled, MenuTriggered;
        public bool BoardSelected = true;
        public bool CheatSheetSelected = false;
        public int CheatSheetPage = 0;
        public int BoardSelX, BoardSelY;
        private Vector2 MousePos;
        private bool MouseShown;
        public int BoardSelSlot => this.BoardSelX + this.BoardSelY * 5;
        private Wiggler Wiggle = Wiggler.Create(0.25f, 3f);
        private float CheatSheetEase, CheatPageEase;

        private const int DISABLE_OFFSET = 1;
        private const int ENABLE_OFFSET = 11;
        private const int OBJECTIVE_OFFSET = 23;
        private const int PINNED_OFFSET = 49;

        public TextMenu Menu;
        public List<List<DrawingDirective>> CheatSheetPages;
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

        private void InitCheatSheet() {
            this.CheatSheetPages = new List<List<DrawingDirective>> {
                new List<DrawingDirective> {
                    new TextDrawingDirective(Dialog.Clean("BINGO_CHEATSHEET_BERRIES_HEADER"), new Vector2(1920f / 2, 50), 1920, 1080, 2f),
                    new RectangleDrawingDirective(new Vector2(1920f / 2, 1080 / 2 + 50), 1920f, 950, new Color(20, 20, 20, 200)),
                    new BerriesTable(new Vector2(1920f / 2, 1080 / 2 + 50), 1600, 950),
                },
                new List<DrawingDirective> {
                    new TextDrawingDirective(Dialog.Clean("BINGO_CHEATSHEET_BINOS_HEADER"), new Vector2(1920f / 2, 50), 1920, 1080, 2f),
                    new RectangleDrawingDirective(new Vector2(1920f / 2, 1080 / 2 + 50), 1920f, 950, new Color(20, 20, 20, 200)),
                    new BinosTable(new Vector2(1920f / 2, 1080 / 2 + 50), 1600, 950),
                },
                new List<DrawingDirective> {
                    new TextDrawingDirective(Dialog.Clean("BINGO_CHEATSHEET_MISC_HEADER"), new Vector2(1920f / 2, 50), 1920, 1080, 2f),
                    new ImageDrawingDirective("controls/directions/0x-1",  new Vector2(200, 200), 1f),
                    new ImageDrawingDirective("controls/directions/-1x0",  new Vector2(300, 200), 1f),
                    new ImageDrawingDirective("controls/directions/1x1",   new Vector2(400, 200), 1f),
                    new ImageDrawingDirective("controls/directions/1x-1",  new Vector2(500, 200), 1f),
                    new ImageDrawingDirective("controls/directions/-1x0",  new Vector2(600, 200), 1f),
                    new ImageDrawingDirective("controls/directions/-1x-1", new Vector2(700, 200), 1f),

                    new ImageDrawingDirective("controls/directions/0x-1",  new Vector2(200, 320), 1f),
                    new ImageDrawingDirective("controls/directions/1x0",   new Vector2(300, 320), 1f),
                    new ImageDrawingDirective("controls/directions/-1x1",  new Vector2(400, 320), 1f),
                    new ImageDrawingDirective("controls/directions/-1x-1", new Vector2(500, 320), 1f),
                    new ImageDrawingDirective("controls/directions/1x0",   new Vector2(600, 320), 1f),
                    new ImageDrawingDirective("controls/directions/1x-1",  new Vector2(700, 320), 1f),

                    new ImageDrawingDirective("controls/directions/0x1",   new Vector2(200, 440), 1f),
                    new ImageDrawingDirective("controls/directions/-1x0",  new Vector2(300, 440), 1f),
                    new ImageDrawingDirective("controls/directions/1x-1",  new Vector2(400, 440), 1f),
                    new ImageDrawingDirective("controls/directions/1x1",   new Vector2(500, 440), 1f),
                    new ImageDrawingDirective("controls/directions/-1x0",  new Vector2(600, 440), 1f),
                    new ImageDrawingDirective("controls/directions/-1x1",  new Vector2(700, 440), 1f),

                    new ImageDrawingDirective("controls/directions/0x1",   new Vector2(200, 560), 1f),
                    new ImageDrawingDirective("controls/directions/1x0",   new Vector2(300, 560), 1f),
                    new ImageDrawingDirective("controls/directions/-1x-1", new Vector2(400, 560), 1f),
                    new ImageDrawingDirective("controls/directions/-1x1",  new Vector2(500, 560), 1f),
                    new ImageDrawingDirective("controls/directions/1x0",   new Vector2(600, 560), 1f),
                    new ImageDrawingDirective("controls/directions/1x1",   new Vector2(700, 560), 1f),
                },
            };
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

        public bool IsInappropriateTimeForMenu() {
            return (Engine.Scene is Overworld ow && (ow.Current is OuiModOptionString || ow.Current is OuiFileNaming)) ||
                Engine.Scene.Entities.FindFirst<ButtonConfigUI>() != null ||
                Engine.Scene.Entities.FindFirst<KeyboardConfigUI>() != null ||
                this.Chat.ChatOpen;
        }

        private bool FirstFrame = false;
        private void UpdateMenu() {
            this.FirstFrame = false;
            if ((this.ModSettings.MenuToggle.Pressed
                 || (!this.MenuToggled && this.ModSettings.MouseClickOpensMenu && (MInput.Mouse.PressedLeftButton || MInput.Mouse.PressedRightButton))
                 || (this.MenuToggled && (Input.MenuCancel.Pressed || Input.ESC.Pressed)))
                && !IsInappropriateTimeForMenu()) {
                this.MenuToggled ^= true;
                this.MenuTriggered = false;
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
                this.MenuToggled = false;
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
            if (this.IsBoardHidden && !this.FirstFrame && !this.Chat.ChatOpen && (Input.MenuConfirm.Pressed || MInput.Mouse.PressedLeftButton)) {
                this.RevealBoard();
                Input.MenuConfirm.ConsumePress();
                MInput.Mouse.PreviousState = MInput.Mouse.CurrentState; // hack to consume press
            }

            this.MousePos = MInput.Mouse.Position;
            this.MouseShown |= MInput.Mouse.WasMoved;
            this.MouseShown &= !this.MenuTriggered;

            if (this.IsBoardHidden || !this.Connected) {
                return;
            }

            var mouseInBounds = this.MousePos.X >= corner.X && this.MousePos.X < corner.X + size.X && this.MousePos.Y >= corner.Y && this.MousePos.Y < corner.Y + size.Y;
            mouseInBounds &= !this.CheatSheetSelected;
            if (this.MouseShown && mouseInBounds) {
                this.BoardSelected = true;
                var oldPos = this.BoardSelSlot;
                this.BoardSelX = (int) Math.Floor((this.MousePos.X - corner.X) / size.X * 5);
                this.BoardSelY = (int) Math.Floor((this.MousePos.Y - corner.Y) / size.Y * 5);
                if (oldPos != this.BoardSelSlot) {
                    this.OnMotion();
                }
            } else if (this.MouseShown) {
                this.BoardSelected = false;
            }

            this.Wiggle.UseRawDeltaTime = true;
            this.Wiggle.Update();

            this.CheatSheetEase = Calc.Approach(this.CheatSheetEase, this.CheatSheetSelected ? 1f : 0f, Engine.RawDeltaTime * 2);
            this.CheatPageEase = Calc.Approach(this.CheatPageEase, 0, Engine.RawDeltaTime * 2);

            if (this.Menu == null) {
                this.InitMenu();
                this.InitCheatSheet(); // this is the wrong place to put this but whatever
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

                if (this.BoardSelected || this.CheatSheetSelected) {
                    if (Input.MenuUp.Pressed) {
                        this.MouseShown = false;
                        if (this.CheatSheetSelected) {
                            this.CheatSheetSelected = false;
                            this.BoardSelY = 4;
                            Audio.Play(SFX.ui_world_journal_page_cover_back);
                        } else {
                            this.OnMotion();
                            this.BoardSelY--;
                            if (this.BoardSelY < 0) {
                                this.CheatSheetSelected = true;
                                Audio.Play(SFX.ui_world_journal_page_cover_forward);
                            }
                        }
                    }
                    if (Input.MenuDown.Pressed) {
                        this.MouseShown = false;
                        if (this.CheatSheetSelected) {
                            this.CheatSheetSelected = false;
                            this.BoardSelY = 0;
                            Audio.Play(SFX.ui_world_journal_page_cover_back);
                        } else {
                            this.OnMotion();
                            this.BoardSelY++;
                            if (this.BoardSelY >= 5) {
                                this.CheatSheetSelected = true;
                                Audio.Play(SFX.ui_world_journal_page_cover_forward);
                            }
                        }
                    }

                    if (this.BoardSelected) {
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
                }
                if (Input.MenuLeft.Pressed) {
                    this.MouseShown = false;
                    if (this.CheatSheetSelected) {
                        if (this.CheatSheetPage != 0) {
                            this.CheatSheetPage--;
                            this.CheatPageEase = 1f;
                            Audio.Play(SFX.ui_world_journal_page_main_back);
                        }
                    } else {
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
                }
                if (Input.MenuRight.Pressed) {
                    this.MouseShown = false;
                    if (this.CheatSheetSelected) {
                        if (this.CheatSheetPage < this.CheatSheetPages.Count - 1) {
                            this.CheatSheetPage++;
                            this.CheatPageEase = -1f;
                            Audio.Play(SFX.ui_world_journal_page_main_forward);
                        }
                    } else {
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
                DepthStencilState.None,
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

            var wiggle = this.Wiggle.Value;
            var screenOffset = (this.CheatSheetSelected ? Ease.CubeOut(this.CheatSheetEase) : Ease.CubeIn(this.CheatSheetEase)) * 1080f;
            var currentCorner = corner - screenOffset * Vector2.UnitY;

            // draw boxes and text
            for (int x = 0; x < 5; x++) {
                for (int y = 0; y < 5; y++) {
                    var slot = y * 5 + x;
                    var subcorner = currentCorner + subsize * new Vector2(x, y);

                    if (!this.MenuTriggered && x == this.BoardSelX && y == this.BoardSelY && this.BoardSelected) {
                        Draw.Rect(subcorner - Vector2.One * wiggle * 3f + Vector2.UnitY * wiggle * 2f, subsize.X + wiggle*3*2, subsize.Y + wiggle*3*2, Color.WhiteSmoke * masterAlpha);
                    }

                    var selectedAlpha = this.MenuTriggered && this.BoardSelected && this.BoardSelX == x && this.BoardSelY == y ? 1f : masterAlpha;
                    if (this.Board[slot].Colors.Count == 0) {
                        Draw.Rect(subcorner + subsize * margin / 2, subsize.X * (1 - margin), subsize.Y * (1 - margin), BingoColors.Blank.ToSquareColor() * selectedAlpha);
                    } else {
                        var chunkWidth = subsize.X * (1 - margin) / this.Board[slot].Colors.Count;
                        for (var i = 0; i < this.Board[slot].Colors.Count; i++) {
                            Draw.Rect(subcorner + subsize * margin / 2 + Vector2.UnitX * chunkWidth * i, chunkWidth, subsize.Y * (1 - margin), this.Board[slot].Colors[i].ToSquareColor() * selectedAlpha);
                        }
                    }

                    bool shrinkBox = false;
                    Vector2 iconPos = subcorner + new Vector2(30, subsize.Y - 30);
                    if (this.ModSettings.ScanAssist) {
                        foreach (var renderer in GetAccessibleTokens(this.Board[slot].Text)) {
                            shrinkBox = true;
                            renderer(iconPos, masterAlpha);
                            iconPos += new Vector2(30f, 0f);
                        }
                    }

                    DrawTextBox(
                        this.Board[slot].Text,
                        subcorner + subsize / 2,
                        subsize.X * (1 - padding),
                        subsize.Y * (1 - padding) * (shrinkBox ? 0.6f : 1f),
                        0.5f, 1.0f, Color.White, 1f, Color.Black);
                }
            }

            // draw notification icons
            for (int x = 0; x < 5; x++) {
                for (int y = 0; y < 5; y++) {
                    var slot = y * 5 + x;
                    var subcorner = currentCorner + subsize * new Vector2(x, y) + Vector2.One * 15f;

                    var status = this.GetObjectiveStatus(slot);
                    if (status == ObjectiveStatus.Completed) {
                        PieButton.DrawPieAndText(subcorner, 0.5f, 1f, "!");
                    } else if (this.ModSettings.ClaimAssist && status == ObjectiveStatus.Progress) {
                        PieButton.DrawPieAndText(subcorner, 0.5f, BingoMonitor.ObjectiveProgress(this.Board[slot].Text), "");
                    }
                }
            }

            var scoreCorner = new Vector2(20f, 20f - screenOffset);
            foreach (var entry in this.Score()) {
                var scoreSize1 = new Vector2(100f, 100f);
                Draw.Rect(scoreCorner, scoreSize1.X, scoreSize1.Y, entry.Item1.ToSquareColor());
                ActiveFont.DrawOutline(entry.Item2.ToString(), scoreCorner + scoreSize1 / 2, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.White, 2f, Color.Black);
                scoreCorner += new Vector2(0f, 120f);
            }

            if (this.Menu != null) {
                this.Menu.Y -= screenOffset;
                this.Menu.Render();
                this.Menu.Y += screenOffset;
            }

            // render cheat sheet
            // cheatpageease = 1 means LEFT was pressed and we're rendering the FOLLOWING page
            // cheatpageease = -1 means RIGHT was pressed and we're rendering the PREVIOUS page
            var pageOffset = -1920f * Ease.CubeIn(this.CheatPageEase);
            foreach (var item in this.CheatSheetPages[this.CheatSheetPage]) {
                item.Draw(new Vector2(pageOffset, 1080f - screenOffset), masterAlpha);
            }
            if (this.CheatPageEase != 0) {
                var otherPageIdx = (this.CheatPageEase < 0 ? -1 : 1) + this.CheatSheetPage;
                var otherPageOffset = (this.CheatPageEase < 0 ? -1 : 1) * 1920f;
                foreach (var item in this.CheatSheetPages[otherPageIdx]) {
                    item.Draw(new Vector2(pageOffset + otherPageOffset, 1080f - screenOffset), masterAlpha);
                }
            }

            if (this.MouseShown) {
                GFX.Gui["menu/bingo/cursor"].Draw(this.MousePos, Vector2.Zero, Color.White, 0.5f);
            }

            Draw.SpriteBatch.End();
        }

        public static void DrawTextBox(string text, Vector2 center, float width, float height, float scale, float lineHeight, Color color, float stroke, Color strokeColor, bool verticalCenter=true) {
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

                    var offsetY = verticalCenter ? -singleHeight * lineHeight * scale * (result.Count - 1) / 2 : -height / 2;
                    var justify = verticalCenter ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0f);
                    foreach (var finalline in result) {
                        ActiveFont.DrawOutline(finalline, center + Vector2.UnitY * offsetY, justify, Vector2.One * scale, color, stroke, strokeColor);
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

        public abstract class DrawingDirective {
            public abstract void Draw(Vector2 offset, float alpha);
        }

        public class TextDrawingDirective : DrawingDirective {
            private string Text;
            private Vector2 Center;
            private float Width, Height, Scale;
            private bool VerticalCenter;
            public TextDrawingDirective(string text, Vector2 center, float width, float height, float scale=1f, bool verticalCenter=true) {
                this.Text = text;
                this.Center = center;
                this.Width = width;
                this.Height = height;
                this.Scale = scale;
                this.VerticalCenter = verticalCenter;
            }


            public override void Draw(Vector2 offset, float alpha) {
                var newCenter = offset + this.Center;
                DrawTextBox(this.Text, newCenter, this.Width, this.Height, this.Scale, 1f, Color.White * alpha, this.Scale * 2f, Color.Black * alpha, this.VerticalCenter);
            }
        }

        public class RectangleDrawingDirective : DrawingDirective {
            private Vector2 Center;
            private float Width, Height;
            private Color Color, StrokeColor;
            private float Stroke;
            public RectangleDrawingDirective(Vector2 center, float width, float height, Color color, Color strokeColor=default, float stroke=1f) {
                this.Center = center;
                this.Width = width;
                this.Height = height;
                this.Color = color;
                this.StrokeColor = strokeColor;
                this.Stroke = stroke;
            }

            public override void Draw(Vector2 offset, float alpha) {
                var corner = offset + this.Center - new Vector2(this.Width / 2, this.Height / 2);
                Monocle.Draw.Rect(corner, this.Width, this.Height, this.Color * alpha);
                Monocle.Draw.HollowRect(corner, this.Width, this.Height, this.StrokeColor * alpha);
            }
        }

        public class ImageDrawingDirective : DrawingDirective {
            private Vector2 Center;
            private float Scale;
            private string Texture;

            public ImageDrawingDirective(string texture, Vector2 center, float scale) {
                this.Center = center;
                this.Texture = texture;
                this.Scale = scale;
            }

            public override void Draw(Vector2 offset, float alpha) {
                var txt = GFX.Game.Has(this.Texture) ? GFX.Game[this.Texture] : GFX.Gui[this.Texture];
                txt.DrawCentered(this.Center + offset, Color.White * alpha, this.Scale);
            }
        }

        public abstract class TableDrawing : DrawingDirective {
            protected Vector2 Center;
            protected float Width, Height;
            protected int Rows, Cols;

            protected TableDrawing(Vector2 center, float width, float height, int rows, int cols) {
                this.Center = center;
                this.Width = width;
                this.Height = height;
                this.Rows = rows;
                this.Cols = cols;
            }

            protected float CellWidth => this.Width / this.Cols;
            protected float CellHeight => this.Height / this.Rows;

            protected Vector2 Cell(int col, int row) {
                return this.Center
                    - new Vector2(this.Width / 2, this.Height / 2)
                    + new Vector2(this.CellWidth * col, this.CellHeight * row)
                    + new Vector2(this.CellWidth / 2, this.CellHeight / 2);
            }
        }

        public class BerriesTable : TableDrawing {
            public BerriesTable(Vector2 center, float width, float height) : base(center, width, height, 7, 8) {
            }

            public override void Draw(Vector2 offset, float alpha) {
                for (int chapterRow = 0; chapterRow < 7; chapterRow++) {
                    int chapterIdx = chapterRow + 1;
                    if (chapterIdx >= 6) {
                        chapterIdx++;
                    }
                    if (chapterIdx >= 8) {
                        chapterIdx++;
                    }

                    var chapterObj = AreaData.Areas[chapterIdx].Mode[0];

                    Vector2 cell;
                    var totalBerries = 0;

                    for (int checkpointIdx = 0; checkpointIdx <= chapterObj.Checkpoints.Length; checkpointIdx++) {
                        var cpChapterIdx = chapterIdx < 9 ? chapterIdx : chapterIdx - 1;
                        var cpName = checkpointIdx == 0 ? Dialog.Clean("overworld_start") : Dialog.Clean($"checkpoint_{cpChapterIdx}_{checkpointIdx - 1}");
                        int berries = 0;
                        bool hasWinged = false;
                        bool hasSeeded = false;
                        for (int berryNo = 0; berryNo < chapterObj.StrawberriesByCheckpoint.GetLength(1); berryNo++) {
                            var berry = chapterObj.StrawberriesByCheckpoint[checkpointIdx, berryNo];
                            if (berry != null) {
                                berries++;
                                totalBerries++;
                                if (berry.Bool("winged")) {
                                    hasWinged = true;
                                }
                                if (berry.Nodes.Length != 0) {
                                    hasSeeded = true;
                                }
                            }
                        }

                        cell = this.Cell(1 + checkpointIdx, chapterRow);
                        if (hasWinged) {
                            new ImageDrawingDirective("collectables/strawberry/wings02", cell + new Vector2(-50, 25), 2f).Draw(offset, alpha);
                        }
                        if (hasSeeded) {
                            new ImageDrawingDirective("collectables/strawberry/seed00", cell + new Vector2(50, 25), 3.5f).Draw(offset, alpha);
                        }
                        new TextDrawingDirective($"{cpName} ({berries})", cell, this.CellWidth * 0.9f, this.CellHeight * 0.7f, 0.75f * 0.8f, false).Draw(offset, alpha);
                    }

                    cell = this.Cell(0, chapterRow);
                    new TextDrawingDirective($"{Dialog.Clean($"area_{chapterIdx}")} ({totalBerries})", cell - Vector2.UnitX * 100, this.CellWidth, this.CellHeight).Draw(offset, alpha);
                }
            }
        }

        public class BinosTable : TableDrawing {
            private Vector2 Offset;
            private float Alpha;
            public BinosTable(Vector2 center, float width, float height) : base(center, width, height, 15, 5) {
            }

            public override void Draw(Vector2 offset, float alpha) {
                this.Offset = offset;
                this.Alpha = alpha;

                for (int row = 0; row < this.Rows; row++) {
                    for (int col = 0; col < this.Cols; col++) {
                        this.DrawCell(col, row,
                            scale: col == 0 ? 0.75f : 0.5f
                        );
                    }
                }
            }

            private void DrawCell(int col, int row, string text=null, float scale=1f, bool verticalCenter=true, Vector2 offset=default) {
                if (text == null) {
                    var key = $"bingo_cheatsheet_binos_{row}_{col}";
                    if (!Dialog.Has(key)) {
                        return;
                    }
                    text = Dialog.Clean(key);
                }
                new TextDrawingDirective(text, this.Cell(col, row) + offset, this.CellWidth, this.CellHeight, scale, verticalCenter).Draw(this.Offset, this.Alpha);
            }
        }

        public static IEnumerable<Action<Vector2, float>> GetAccessibleTokens(string text) {
            bool binos, berries, redhearts, bluehearts, cassettes;
            binos = berries = redhearts = bluehearts = cassettes = false;
            if (text.Contains("Binocular")) {
                binos = true;
            }
            if (text.Contains("Collectibles")) {
                berries = true;
                bluehearts = true;
                cassettes = true;
            }
            if (text.Contains("Blue")) {
                bluehearts = true;
            }
            if (text.Contains("Red") || text.Contains("B-Side")) {
                redhearts = true;
            }
            if (text.Contains("Heart") && !redhearts && !bluehearts && !text.Contains("Heart of the Mountain")) {
                redhearts = true;
                bluehearts = true;
            }
            if (text.Contains("Cassette")) {
                cassettes = true;
            }
            if (text.Contains("Berries") && !text.Contains("PICO")) {
                berries = true;
            }
            if (text.Contains("1-Up") || text.Contains("2-Up") || text.Contains("3-Up")) {
                berries = true;
            }

            if (binos) {
                yield return RenderBinoculars;
            }
            if (berries) {
                yield return RenderBerry;
            }
            if (cassettes) {
                yield return RenderCassette;
            }
            if (bluehearts) {
                yield return RenderBlueHeart;
            }
            if (redhearts) {
                yield return RenderRedHeart;
            }
        }

        private static void RenderBlueHeart(Vector2 pos, float alpha) {
            MTN.Journal["heartgem0"].DrawCentered(pos, Color.White * alpha, 0.5f);
        }

        private static void RenderRedHeart(Vector2 pos, float alpha) {
            MTN.Journal["heartgem1"].DrawCentered(pos, Color.White * alpha, 0.5f);
        }

        private static void RenderBerry(Vector2 pos, float alpha) {
            MTN.Journal["strawberry"].DrawCentered(pos, Color.White * alpha, 0.5f);
        }

        private static void RenderCassette(Vector2 pos, float alpha) {
            MTN.Journal["cassette"].DrawCentered(pos, Color.White * alpha, 0.5f);
        }

        //private static void RenderFlag(Vector2 pos, float alpha) {
        //    MTN.Journal["clear"].DrawCentered(pos, Color.White * alpha, 0.5f);
        //}

        private static void RenderBinoculars(Vector2 pos, float alpha) {
            GFX.Game["objects/lookout/lookout05"].DrawJustified(pos, new Vector2(0.5f, 0.75f), Color.White * alpha, 2f);
        }

        #endregion
    }
}
