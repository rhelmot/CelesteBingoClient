using FMOD.Studio;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient : EverestModule {
        public static BingoClient Instance;
        private BingoChat Chat;

        public override Type SaveDataType => typeof(BingoSaveData);
        public override Type SessionType => typeof(BingoSession);
        public override Type SettingsType => typeof(BingoClientSettings);
        public BingoSaveData ModSaveData => (BingoSaveData) this._SaveData;
        public BingoSession ModSession => (BingoSession) this._Session;
        public BingoClientSettings ModSettings => (BingoClientSettings) this._Settings;

        public BingoClient() {
            Instance = this;
        }
        
        public override void LoadContent(bool firstLoad) {
        }

        public override void Load() {
            if (this.ModSettings.MasterSwitch) {
                this.HookStuff();
            }
            
            this.Chat = new BingoChat(this.SendChat);
        }

        public override void Unload() {
            this.UnhookStuff();
        }

        private bool StuffIsHooked;
        internal void HookStuff() {
            if (this.StuffIsHooked) {
                return;
            }
            
            On.Celeste.OuiFileSelectSlot.CreateButtons += CreateBingoButton;
            On.Monocle.Engine.RenderCore += this.Render;
            IL.Monocle.Engine.Update += HookUpdateEarly;
            Everest.Events.Level.OnCreatePauseMenuButtons += OnPause;
            Everest.Events.Level.OnExit += OnExit;
            On.Celeste.SaveData.Start += OnSaveStart;
            On.Celeste.SaveData.InitializeDebugMode += WipeDebugFile;
            On.Celeste.SaveData.Start += WipeObjectiveCache;
            
            BingoWatches.HookStuff();
            
            this.StuffIsHooked = true;
        }

        internal void UnhookStuff() {
            if (!this.StuffIsHooked) {
                return;
            }
            
            On.Celeste.OuiFileSelectSlot.CreateButtons -= CreateBingoButton;
            On.Monocle.Engine.RenderCore -= this.Render;
            IL.Monocle.Engine.Update -= HookUpdateEarly;
            Everest.Events.Level.OnCreatePauseMenuButtons -= OnPause;
            Everest.Events.Level.OnExit -= OnExit;
            On.Celeste.SaveData.Start -= OnSaveStart;
            On.Celeste.SaveData.InitializeDebugMode -= WipeDebugFile;
            On.Celeste.SaveData.Start -= WipeObjectiveCache;
            
            BingoWatches.UnhookStuff();
            
            this.StuffIsHooked = false;
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            base.CreateModMenuSection(menu, inGame, snapshot);

            foreach (var item in menu.Items) {
                if (!(item is TextMenu.Button btn) || !btn.Label.StartsWith(Dialog.Clean("modoptions_bingoclient_playername"))) {
                    continue;
                }

                var messageHeader = new TextMenuExt.EaseInSubHeaderExt(Dialog.Clean("modoptions_bingoclient_playername_about"), false, menu) {
                    HeightExtra = 17f,
                    Offset = new Vector2(30, -5),
                };

                menu.Insert(menu.Items.IndexOf(item) + 1, messageHeader);
                btn.OnEnter = () => messageHeader.FadeVisible = true;
                btn.OnLeave = () => messageHeader.FadeVisible = false;
                break;
            }

            if (this.Password != null && !this.Connected) {
                var retryBtn = new TextMenu.Button(Dialog.Clean("modoptions_bingoclient_reconnect"));
                retryBtn.OnPressed = () => {
                    try {
                        this.Connect();
                    } catch (Exception e) {
                        Logger.LogDetailed(e, "BingoClient");
                        this.LogChat(Dialog.Clean("bingoclient_connect_error"));
                    }
                };
                menu.Add(retryBtn);
            }
            if (this.Connected) {
                var disconnectBtn = new TextMenu.Button(Dialog.Clean("modoptions_bingoclient_disconnect"));
                disconnectBtn.OnPressed = () => {
                    this.Disconnect();
                    this.LogChat(Dialog.Clean("modoptions_bingoclient_disconnect_message"));
                };
                menu.Add(disconnectBtn);
            }
        }

        public override void SaveSettings() {
            base.SaveSettings();
            if (!this.Connected) {
                return;
            }

            this.SendColor();
        }

        private void WipeObjectiveCache(On.Celeste.SaveData.orig_Start orig, SaveData data, int slot) {
            orig(data, slot);
            this.RefreshObjectives();
        }

        private void WipeDebugFile(On.Celeste.SaveData.orig_InitializeDebugMode orig, bool loadexisting) {
            orig(loadexisting);
            this.ModSaveData.Reset();
        }

        private void OnSaveStart(On.Celeste.SaveData.orig_Start orig, SaveData data, int slot) {
            orig(data, slot);
            if (this.Connected) {
                this.RefreshObjectives();
            }
        }

        private void OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            foreach (BingoVariant variant in typeof(BingoVariant).GetEnumValues()) {
                BingoMonitor.SetVariantEnabled(variant, false);
            }
        }

        private void OnPause(Level level, TextMenu menu, bool minimal) {
            void tweakVariantsMenu(TextMenu.Button btn, int idx) {
                var messageHeader = new TextMenuExt.EaseInSubHeaderExt(Dialog.Clean("bingoclient_variants_warning"), false, menu) {
                    HeightExtra = 17f,
                    Offset = new Vector2(30, -5),
                };

                var oldAction = btn.OnPressed;
                btn.OnPressed = () => {
                    this.ModSession.CheckpointStartedVariant = null;
                    oldAction?.Invoke();
                };
                btn.OnEnter = () => messageHeader.FadeVisible = true;
                btn.OnLeave = () => messageHeader.FadeVisible = false;

                menu.Insert(idx + 1, messageHeader);
            }

            for (int i = 0; i < menu.Items.Count; i++) {
                var item = menu.Items[i];
                if (!(item is TextMenu.Button btn)) {
                    continue;
                }

                if (btn.Label == Dialog.Clean("menu_pause_variant") || btn.Label == Dialog.Clean("modoptions_extendedvariants_pausemenu_button")) {
                    tweakVariantsMenu(btn, i);
                }
            }
        }
        
        private void HookUpdateEarly(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchCall(typeof(MInput), "Update"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.EmitDelegate<Action>(this.Update);
        }

        private KeyboardState personalKeyboard;
        private GamePadState[] personalGamepads = {default, default, default, default};
        private bool gameSawNothing;
        private void Update() {
            if (Dialog.Language == null || ActiveFont.Font == null || ActiveFont.Font.Sizes.Count == 0) {
                return;
            }

            var gameSeesNothing = this.Chat.ChatOpen || (this.MenuToggled && !this.MenuTriggered);
            if (this.gameSawNothing) {
                this.UneatInput();
            }

            this.UpdateMenu();
            this.Chat.Update();
            this.UpdateObjectives();
            
            if (gameSeesNothing) {
                this.EatInput();
            }
            this.gameSawNothing = gameSeesNothing;
        }

        public void EatInput() {
            // prevent game from seeing any inputs
            // but keep a copy so we can get accurate pressed/released data for ourselves
            this.personalKeyboard = MInput.Keyboard.CurrentState;
            MInput.Keyboard.CurrentState = new KeyboardState();
            for (var i = 0; i < 4; i++) {
                this.personalGamepads[i] = MInput.GamePads[i].CurrentState;
                MInput.GamePads[i].CurrentState = new GamePadState();
            }
            typeof(MInput).GetMethod("UpdateVirtualInputs", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { });
        }

        public void UneatInput() {
            MInput.Keyboard.PreviousState = this.personalKeyboard;
            for (var i = 0; i < 4; i++) {
                MInput.GamePads[i].PreviousState = this.personalGamepads[i];
            }
        }

        private void Render(On.Monocle.Engine.orig_RenderCore orig, Engine self) {
            orig(self);
            if (Dialog.Language == null || ActiveFont.Font == null || ActiveFont.Font.Sizes.Count == 0) {
                return;
            }
            
            this.RenderMenu();
            this.Chat?.Render();
        }
        private void CreateBingoButton(On.Celeste.OuiFileSelectSlot.orig_CreateButtons orig, OuiFileSelectSlot self) {
            orig(self);
            if (!self.Exists) {
                var bingoButton = new OuiFileSelectSlot.Button {
                    Action = () => {
                        var contents = TextInput.GetClipboardText();
                        if (contents.StartsWith("https://bingosync.com/room/")) {
                            this.Password = "password";
                            (self.Scene as Overworld).Goto<OuiTextEntry>().Init<OuiBingoConnecting>("password", s => {
                                this.Password = s;
                            });
                            this.Username = this.ModSettings.PlayerName.Length == 0 ? self.Name : this.ModSettings.PlayerName;
                            this.RoomUrl = contents;
                        } else {
                            this.LogChat(Dialog.Clean("BINGOCLIENT_BAD_PASTE"));
                        }
                    },
                    Label = Dialog.Clean("BINGOCLIENT_START_BUTTON"),
                    Scale = 0.7f,
                };
                var buttons = (List<OuiFileSelectSlot.Button>) typeof(OuiFileSelectSlot).GetField("buttons", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self);
                buttons.Add(bingoButton);
            }
        }

        public void LogChat(string message) {
            this.Chat?.Chat(message);
        }
    }
    
    public class OuiBingoConnecting : Oui {
        public override IEnumerator Enter(Oui from) {
            if (!OuiModOptionString.Cancelled) {
                var task = new Task(() => {
                    try {
                        if (BingoClient.Instance.Connected) {
                            BingoClient.Instance.Disconnect();
                            BingoClient.Instance.LogChat(Dialog.Clean("modoptions_bingoclient_disconnect_message"));
                        }
                        BingoClient.Instance.Connect();
                    } catch (Exception e) {
                        Logger.LogDetailed(e, "BingoClient");
                        BingoClient.Instance.LogChat(Dialog.Clean("BINGOCLIENT_CONNECT_ERROR"));
                    }
                });
                task.Start();

                while (!task.IsCompleted) {
                    yield return null;
                }
            }

            this.Overworld.Goto<OuiFileSelect>();
        }

        public override IEnumerator Leave(Oui next) {
            yield return null;
        }
    }

    public class BingoSaveData : EverestModuleSaveData {
        public int[] OneUps = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        public List<string> SeekerKills = new List<string>();
        public List<string> HugeMessOrders = new List<string>();
        public int PicoBerries;
        public int MaxOneUpCombo = -1;
        public List<string> FileFlags = new List<string>();
        public List<string> VariantCompletions = new List<string>();

        public void Reset() {
            this.OneUps = new [] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            this.SeekerKills = new List<string>();
            this.HugeMessOrders = new List<string>();
            this.PicoBerries = 0;
            this.MaxOneUpCombo = -1;
            this.FileFlags = new List<string>();
            this.VariantCompletions = new List<string>();
        }

        public void AddSeekerKill(string seeker) {
            if (this.SeekerKills.Contains(seeker)) {
                return;
            }
            this.SeekerKills.Add(seeker);
        }
        
        public void AddFlag(string cp) {
            if (this.FileFlags.Contains(cp)) {
                return;
            }
            this.FileFlags.Add(cp);
        }
        
        public void AddHugeMessOrder(int a, int b, int c) {
            var order = string.Join(",", a.ToString(), b.ToString(), c.ToString());
            if (this.HugeMessOrders.Contains(order)) {
                return;
            }
            this.HugeMessOrders.Add(order);
        }

        public bool HasHugeMessOrder(int a, int b, int c) {
            var order = string.Join(",", a.ToString(), b.ToString(), c.ToString());
            return this.HugeMessOrders.Contains(order);
        }

        public void AddCheckpointVariant(int chapter, int mode, int checkpoint, BingoVariant variant) {
            var str = $"{chapter}:{mode}:{checkpoint}:{variant}";
            if (!this.VariantCompletions.Contains(str)) {
                this.VariantCompletions.Add(str);
            }
        }

        public bool HasCheckpointVariant(int chapter, int mode, int checkpoint, BingoVariant variant) {
            var str = $"{chapter}:{mode}:{checkpoint}:{variant}";
            return this.VariantCompletions.Contains(str);
        }
    }

    public class BingoSession : EverestModuleSession {
        public List<int> HugeMessOrder = new List<int>();
        public int? CheckpointStartedVariant;
    }

    public class BingoClientSettings : EverestModuleSettings {
        public enum TriggerMode { Patient, Hasty }
        public enum TriggerAlphaMode { Low, Medium, High }
        public bool MasterSwitch { get; set; } = true;
        [SettingMaxLength(20)]
        [SettingMinLength(0)]
        public string PlayerName { get; set; } = "";
        public BingoColors PlayerColor { get; set; } = BingoColors.Orange;
        [DefaultButtonBinding(Buttons.RightStick, Keys.Tab)]
        public ButtonBinding MenuToggle { get; set; }
        public ButtonBinding MenuTrigger { get; set; }
        [DefaultButtonBinding(Buttons.LeftStick, Keys.OemBackslash)]
        public ButtonBinding QuickClaim { get; set; }
        [DefaultButtonBinding(Buttons.Back, Keys.T)]
        public ButtonBinding OpenChat { get; set; }
        public TriggerMode TriggerBehavior { get; set; } = TriggerMode.Hasty;
        public TriggerAlphaMode TriggerAlpha { get; set; } = TriggerAlphaMode.Medium;

        public void CreatePlayerColorEntry(TextMenu menu, bool inGame) {
            var enumValues = new List<BingoColors>((BingoColors[])Enum.GetValues(typeof(BingoColors)));
            enumValues.Remove(BingoColors.Blank);
            enumValues.Sort();
            string enumNamePrefix = $"modoptions_bingoclient_playercolor";
            var item =
                new TextMenu.Slider(Dialog.Clean(enumNamePrefix), (i) => {
                    string enumName = enumValues[i].ToString();
                    return
                        $"{enumNamePrefix}_{enumName.ToLowerInvariant()}".DialogCleanOrNull() ??
                        enumName;
                }, 0, enumValues.Count - 1, (int)this.PlayerColor - 1)
                .Change(v => this.PlayerColor = (BingoColors) v + 1)
            ;
            menu.Add(item);
        }

        public void CreateMasterSwitchEntry(TextMenu menu, bool inGame) {
            if (inGame) {
                return;
            }
            
            var toggle = new TextMenu.OnOff(Dialog.Clean("modoptions_bingoclient_masterswitch"), this.MasterSwitch);
            toggle.OnValueChange = v => {
                this.MasterSwitch = v;
                if (v) {
                    BingoClient.Instance.HookStuff();
                } else {
                    BingoClient.Instance.UnhookStuff();
                }
            };
            menu.Add(toggle);
        }
    }
}
