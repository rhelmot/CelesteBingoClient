using FMOD.Studio;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient : EverestModule {
        public static BingoClient Instance;

        public BingoClient() {
            Instance = this;
        }
        
        public override void LoadContent(bool firstLoad) {
        }

        public override void Load() {
            if (this.ModSettings.MasterSwitch) {
                this.HookStuff();
            }
        }

        public override void Unload() {
            this.UnhookStuff();
        }

        private bool StuffIsHooked;
        private List<IDetour> SpecialHooks = new List<IDetour>();
        internal void HookStuff() {
            if (this.StuffIsHooked) {
                return;
            }
            
            On.Celeste.OuiFileSelectSlot.CreateButtons += CreateBingoButton;
            On.Monocle.Engine.RenderCore += RenderBingoHud;
            IL.Celeste.StrawberryPoints.Added += Track1up;
            IL.Celeste.ClutterSwitch.OnDashed += TrackClutter;
            On.Celeste.Pico8.Classic.load_room += TrackPicoRooms;
            IL.Celeste.Pico8.Classic.fruit.update += TrackPicoBerries;
            IL.Celeste.Pico8.Classic.fly_fruit.update += TrackPicoBerries;
            IL.Monocle.Engine.Update += HookUpdateEarly;
            Everest.Events.Level.OnTransitionTo += OnTransition;
            Everest.Events.Level.OnComplete += OnComplete;
            Everest.Events.Level.OnCreatePauseMenuButtons += OnPause;
            Everest.Events.Level.OnExit += OnExit;
            On.Celeste.CutsceneEntity.EndCutscene += OnEndCutscene;
            On.Celeste.PicoConsole.Update += CheckPicoProximity;
            On.Celeste.CoreModeToggle.OnChangeMode += CheckIceRoom;
            On.Celeste.CrushBlock.OnDashed += KevinDash;
            IL.Celeste.SummitCheckpoint.Update += TrackSummitCheckpoints;
            On.Celeste.IntroCar.Update += TrackIntroCar;

            SpecialHooks.Add(new ILHook(typeof(Seeker).GetMethod("<.ctor>b__58_2", BindingFlags.Instance | BindingFlags.NonPublic), TrackSeekerDeath));
            SpecialHooks.Add(new ILHook(typeof(HeartGem).GetMethod("orig_CollectRoutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(), TrackEmptySpace));
            this.StuffIsHooked = true;
        }

        internal void UnhookStuff() {
            if (!this.StuffIsHooked) {
                return;
            }
            
            On.Celeste.OuiFileSelectSlot.CreateButtons -= CreateBingoButton;
            On.Monocle.Engine.RenderCore -= RenderBingoHud;
            IL.Celeste.StrawberryPoints.Added -= Track1up;
            IL.Celeste.ClutterSwitch.OnDashed -= TrackClutter;
            On.Celeste.Pico8.Classic.load_room -= TrackPicoRooms;
            IL.Celeste.Pico8.Classic.fruit.update -= TrackPicoBerries;
            IL.Celeste.Pico8.Classic.fly_fruit.update -= TrackPicoBerries;
            IL.Monocle.Engine.Update -= HookUpdateEarly;
            Everest.Events.Level.OnTransitionTo -= OnTransition;
            Everest.Events.Level.OnComplete -= OnComplete;
            Everest.Events.Level.OnCreatePauseMenuButtons -= OnPause;
            Everest.Events.Level.OnExit -= OnExit;
            On.Celeste.CutsceneEntity.EndCutscene -= OnEndCutscene;
            On.Celeste.PicoConsole.Update -= CheckPicoProximity;
            On.Celeste.CoreModeToggle.OnChangeMode -= CheckIceRoom;
            On.Celeste.CrushBlock.OnDashed -= KevinDash;
            IL.Celeste.SummitCheckpoint.Update -= TrackSummitCheckpoints;
            On.Celeste.IntroCar.Update -= TrackIntroCar;

            foreach (var hook in this.SpecialHooks) {
                hook.Dispose();
            }
            this.SpecialHooks.Clear();
            
            this.StuffIsHooked = false;
        }

        private void TrackIntroCar(On.Celeste.IntroCar.orig_Update orig, IntroCar self) {
            orig(self);
            if (self.HasRider() && SaveData.Instance.CurrentSession.Level == "e-01") {
                this.ModSaveData.AddFlag("remembered_intro_car");
            }
        }

        private void TrackSummitCheckpoints(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchCall(typeof(Audio), "Play"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.EmitDelegate<Action>(() => {
                var missed = false;
                for (var i = 1; i <= 30; i++) {
                    if (!SaveData.Instance.CurrentSession.GetFlag("summit_checkpoint_" + i)) {
                        missed = true;
                        break;
                    }
                }

                if (!missed) {
                    this.ModSaveData.AddFlag("all_summit_flags");
                }
            });
        }

        private void TrackEmptySpace(ILContext il) {
            var cursor = new ILCursor(il);
            for (var i = 0; i < 5; i++) {
                if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchLdfld(typeof(HeartGem), "IsFake"))) {
                    throw new Exception("Could not find patch point");
                }
            }

            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate<Action<bool>>(b => {
                if (b) {
                    this.ModSaveData.AddFlag("empty_space");
                }
            });
        }

        private DashCollisionResults KevinDash(On.Celeste.CrushBlock.orig_OnDashed orig, CrushBlock self, Player player, Vector2 dir) {
            var dirCh = dir.Y < 0 ? 'u' : dir.Y > 0 ? 'd' : dir.X < 0 ? 'l' : 'r';
            var level = self.Scene as Level ?? throw new Exception("but why tho");
            level.Session.SetFlag("kevin:"+ dirCh);
            if (new[] {"kevin:u", "kevin:d", "kevin:l", "kevin:r"}.All(f => level.Session.GetFlag(f))) {
                this.ModSaveData.AddFlag("kevin");
            }
            return orig(self, player, dir);
        }

        private void CheckIceRoom(On.Celeste.CoreModeToggle.orig_OnChangeMode orig, CoreModeToggle self, Session.CoreModes mode) {
            orig(self, mode);
            var level = self.Scene as Level ?? throw new Exception("dude what");
            if (mode == global::Celeste.Session.CoreModes.Cold && level.Session.Level == "b-04") {
                this.ModSaveData.AddFlag("first_ice");
            }
        }

        private void CheckPicoProximity(On.Celeste.PicoConsole.orig_Update orig, PicoConsole self) {
            orig(self);
            var player = self.Scene.Tracker.GetEntity<Player>();
            if (player == null) {
                return;
            }
            var level = self.Scene as Level ?? throw new Exception("how'd you do that");

            if (level.Camera.Viewport.Bounds.Contains(new Point((int)self.X, (int)self.Y)) && player.Y < self.Y + 16) {
                this.ModSaveData.AddFlag("foundpico");
            }
        }

        private void OnEndCutscene(On.Celeste.CutsceneEntity.orig_EndCutscene orig, CutsceneEntity self, Level level, bool removeself) {
            orig(self, level, removeself);
            if (self.WasSkipped) {
                return;
            }

            var where = level.Session.Level;
            if (level.Session.Area.ID == 5 && where == "e-00" && level.Session.RespawnPoint.Value.Y > 1300) {
                where = "search";
            }
            this.ModSaveData.AddFlag($"cutscene:{level.Session.Area.ID}:{where}");
        }

        static BingoClient() {
            InitObjectives();
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
                        Chat(Dialog.Clean("bingoclient_connect_error"));
                    }
                };
                menu.Add(retryBtn);
            }
            if (this.Connected) {
                var disconnectBtn = new TextMenu.Button(Dialog.Clean("modoptions_bingoclient_disconnect"));
                disconnectBtn.OnPressed = () => {
                    this.Disconnect();
                    Chat(Dialog.Clean("modoptions_bingoclient_disconnect_message"));
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

        public override Type SaveDataType => typeof(BingoSaveData);
        public override Type SessionType => typeof(BingoSession);
        public override Type SettingsType => typeof(BingoSettings);
        public BingoSaveData ModSaveData => (BingoSaveData) this._SaveData;
        public BingoSession ModSession => (BingoSession) this._Session;
        public BingoSettings ModSettings => (BingoSettings) this._Settings;
        

        private void OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            foreach (BingoVariant variant in typeof(BingoVariant).GetEnumValues()) {
                SetVariantEnabled(variant, false);
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
        
        private void OnComplete(Level level) {
            this.OnCompleteVariants(level);
        }

        private void OnTransition(Level level, LevelData next, Vector2 direction) {
            var player = level.Tracker.GetEntity<Player>();
            var area = level.Session.Area;
            var prev = level.Session.MapData.GetAt(player.Position - direction * 8);
            if (prev.Name == next.Name) {
                // just in case!
                return;
            }

            this.OnTransitionVariants(level, next.Name);

            switch (next.Name) {
                case "b-00c" when area == new AreaKey(6):
                    this.ModSaveData.AddFlag("room:easteregg");
                    break;
                case "9" when area == new AreaKey(6): // 0
                case "11" when area == new AreaKey(6): // 1
                case "13" when area == new AreaKey(6): // 2
                case "15" when area == new AreaKey(6): // 3
                case "17" when area == new AreaKey(6): // 4
                case "19" when area == new AreaKey(6): // 5
                    var idx = (int.Parse(next.Name) - 9) / 2;
                    var from = prev.Name.EndsWith("b") ? "top" : "bottom";
                    level.Session.SetFlag($"hollows:{idx}:{from}");
                    if (new[] {"hollows:0:bottom", "hollows:1:bottom", "hollows:2:bottom", "hollows:3:bottom", "hollows:4:bottom", "hollows:5:bottom"}.All(f => level.Session.GetFlag(f))) {
                        this.ModSaveData.AddFlag("room:hollows:bottom");
                    }
                    if (new[] {"hollows:0:top", "hollows:1:top", "hollows:2:top", "hollows:3:top", "hollows:4:top", "hollows:5:top"}.All(f => level.Session.GetFlag(f))) {
                        this.ModSaveData.AddFlag("room:hollows:top");
                    }
                    break;
                case "d-00" when prev.Name == "c-10" && area == new AreaKey(4):
                    this.ModSaveData.AddFlag("room:oldtrailsecret");
                    break;
                case "secret" when area == new AreaKey(8):
                    this.ModSaveData.AddFlag("room:birdnest");
                    break;
            }
        }

        private void HookUpdateEarly(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchCall(typeof(MInput), "Update"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.EmitDelegate<Action>(this.Update);
        }
        
        private void TrackPicoBerries(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchLdfld(typeof(Pico8.Classic), "got_fruit"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate<Action<HashSet<int>>>(set => {
                this.ModSaveData.PicoBerries = Math.Max(this.ModSaveData.PicoBerries, set.Count);
            });
        }

        private void TrackPicoRooms(On.Celeste.Pico8.Classic.orig_load_room orig, Pico8.Classic self, int x, int y) {
            orig(self, x, y);
            if (this.ModSaveData == null) {
                return;
            }

            if (x == 3 && y == 1) {
                this.ModSaveData.AddFlag("pico_oldsite");
            }

            if (x == 5 && y == 2) {
                // todo move this into the actual orb collect routine
                this.ModSaveData.AddFlag("pico_orb");
            }

            if (x == 6 && y == 3) {
                this.ModSaveData.AddFlag("pico_complete");
            }
        }

        private void TrackClutter(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall(typeof(Input), "Rumble"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<ClutterSwitch>>(entity => {
                var color = (int)typeof(ClutterSwitch).GetField("color", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entity);
                this.ModSession.HugeMessOrder.Add(color);
                if (this.ModSession.HugeMessOrder.Count == 3) {
                    this.ModSaveData.AddHugeMessOrder(this.ModSession.HugeMessOrder[0], this.ModSession.HugeMessOrder[1], this.ModSession.HugeMessOrder[2]);
                }
            });
        }

        private void TrackSeekerDeath(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchCall<Entity>("RemoveSelf"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.EmitDelegate<Action>(() => {
                // technically this can't account for killing two different seekers in the same room (technically possible in 5a but stupid hard)
                // but who cares
                var ident = SaveData.Instance.CurrentSession.Level;
                this.ModSaveData.AddSeekerKill(ident);
            });
        }

        private void Track1up(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchLdfld(typeof(StrawberryPoints), "index"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate<Action<int>>(idx => {
                this.ModSaveData.OneUps[SaveData.Instance.CurrentSession.Area.ID]++;
                this.ModSaveData.MaxOneUpCombo = Math.Max(idx, this.ModSaveData.MaxOneUpCombo);
            });
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
                            Chat(Dialog.Clean("BINGOCLIENT_BAD_PASTE"));
                        }
                    },
                    Label = Dialog.Clean("BINGOCLIENT_START_BUTTON"),
                    Scale = 0.7f,
                };
                var buttons = (List<OuiFileSelectSlot.Button>) typeof(OuiFileSelectSlot).GetField("buttons", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self);
                buttons.Add(bingoButton);
            }
        }

        private void Update() {
            if (Dialog.Language == null || ActiveFont.Font == null || ActiveFont.Font.Sizes.Count == 0) {
                return;
            }
            
            this.UpdateChat();
            this.PreUpdateMenu();
            this.UpdateObjectives();
        }

        private void RenderBingoHud(On.Monocle.Engine.orig_RenderCore orig, Engine self) {
            orig(self);
            if (Dialog.Language == null || ActiveFont.Font == null || ActiveFont.Font.Sizes.Count == 0) {
                return;
            }
            
            this.RenderMenu();
            this.RenderChat();
        }
        
        public class OuiBingoConnecting : Oui {
            public override IEnumerator Enter(Oui from) {
                var task = new Task(() => {
                    try {
                        Instance.Connect();
                    } catch (Exception e) {
                        Logger.LogDetailed(e, "BingoClient");
                        Chat(Dialog.Clean("BINGOCLIENT_CONNECT_ERROR"));
                        return;
                    }
                    
                });
                task.Start();

                while (!task.IsCompleted) {
                    yield return null;
                }

                this.Overworld.Goto<OuiFileSelect>();
            }

            public override IEnumerator Leave(Oui next) {
                yield return null;
            }
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

        public void AddCheckpointVariant(int chapter, int mode, int checkpoint, BingoClient.BingoVariant variant) {
            var str = $"{chapter}:{mode}:{checkpoint}:{variant}";
            if (!this.VariantCompletions.Contains(str)) {
                this.VariantCompletions.Add(str);
            }
        }

        public bool HasCheckpointVariant(int chapter, int mode, int checkpoint, BingoClient.BingoVariant variant) {
            var str = $"{chapter}:{mode}:{checkpoint}:{variant}";
            return this.VariantCompletions.Contains(str);
        }
    }

    public class BingoSession : EverestModuleSession {
        public List<int> HugeMessOrder = new List<int>();
        public int? CheckpointStartedVariant;
    }

    public class BingoSettings : EverestModuleSettings {
        public bool MasterSwitch { get; set; } = true;
        [SettingMaxLength(20)]
        [SettingMinLength(0)]
        public string PlayerName { get; set; } = "";
        public BingoClient.BingoColors PlayerColor { get; set; } = BingoClient.BingoColors.Orange;
        [DefaultButtonBinding(Buttons.RightStick, Keys.Tab)]
        public ButtonBinding MenuToggle { get; set; }
        public ButtonBinding MenuTrigger { get; set; }
        [DefaultButtonBinding(Buttons.LeftStick, Keys.Enter)]
        public ButtonBinding QuickClaim { get; set; }
        [DefaultButtonBinding(Buttons.Back, Keys.T)]
        public ButtonBinding OpenChat { get; set; }

        public void CreatePlayerColorEntry(TextMenu menu, bool inGame) {
            var enumValues = new List<BingoClient.BingoColors>((BingoClient.BingoColors[])Enum.GetValues(typeof(BingoClient.BingoColors)));
            enumValues.Remove(BingoClient.BingoColors.Blank);
            enumValues.Sort();
            string enumNamePrefix = $"modoptions_bingoclient_playercolor";
            var item =
                new TextMenu.Slider(Dialog.Clean(enumNamePrefix), (i) => {
                    string enumName = enumValues[i].ToString();
                    return
                        $"{enumNamePrefix}_{enumName.ToLowerInvariant()}".DialogCleanOrNull() ??
                        enumName;
                }, 0, enumValues.Count - 1, (int)this.PlayerColor - 1)
                .Change(v => this.PlayerColor = (BingoClient.BingoColors) v + 1)
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
