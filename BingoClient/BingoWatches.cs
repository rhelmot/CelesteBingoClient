using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

// BingoClient.Instance class includes all the patches which watch the game state and update the mod state accordingly

namespace Celeste.Mod.BingoClient {
    public static class BingoWatches {
        private static List<IDetour> SpecialHooks = new List<IDetour>();
        
        internal static void HookStuff() {
            IL.Celeste.StrawberryPoints.Added += Track1up;
            IL.Celeste.ClutterSwitch.OnDashed += TrackClutter;
            On.Celeste.Pico8.Classic.load_room += TrackPicoRooms;
            IL.Celeste.Pico8.Classic.fruit.update += TrackPicoBerries;
            IL.Celeste.Pico8.Classic.fly_fruit.update += TrackPicoBerries;
            IL.Celeste.Pico8.Classic.orb.draw += TrackPicoOrb;
            Everest.Events.Level.OnTransitionTo += OnTransition;
            Everest.Events.Level.OnComplete += OnComplete;
            On.Celeste.Level.EndCutscene += OnEndCutscene;
            On.Celeste.PicoConsole.Update += CheckPicoProximity;
            On.Celeste.CoreModeToggle.OnChangeMode += CheckIceRoom;
            On.Celeste.CrushBlock.OnDashed += KevinDash;
            IL.Celeste.SummitCheckpoint.Update += TrackSummitCheckpoints;
            On.Celeste.IntroCar.Update += TrackIntroCar;
            On.Celeste.Key.OnPlayer += TrackKeys;

            SpecialHooks.Add(new ILHook(typeof(Seeker).GetMethod("<.ctor>b__58_2", BindingFlags.Instance | BindingFlags.NonPublic), TrackSeekerDeath));
            SpecialHooks.Add(new ILHook(typeof(HeartGem).GetMethod("orig_CollectRoutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(), TrackEmptySpace));
            SpecialHooks.Add(new ILHook(typeof(Level).GetMethod("SkipCutsceneRoutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(), MarkSkippedCutscene));
            IL.Celeste.CutsceneEntity.EndCutscene += DebugTheo2;
        }

        internal static void UnhookStuff() {
            IL.Celeste.StrawberryPoints.Added -= Track1up;
            IL.Celeste.ClutterSwitch.OnDashed -= TrackClutter;
            On.Celeste.Pico8.Classic.load_room -= TrackPicoRooms;
            IL.Celeste.Pico8.Classic.fruit.update -= TrackPicoBerries;
            IL.Celeste.Pico8.Classic.fly_fruit.update -= TrackPicoBerries;
            IL.Celeste.Pico8.Classic.orb.draw -= TrackPicoOrb;
            Everest.Events.Level.OnTransitionTo -= OnTransition;
            Everest.Events.Level.OnComplete -= OnComplete;
            On.Celeste.Level.EndCutscene -= OnEndCutscene;
            On.Celeste.PicoConsole.Update -= CheckPicoProximity;
            On.Celeste.CoreModeToggle.OnChangeMode -= CheckIceRoom;
            On.Celeste.CrushBlock.OnDashed -= KevinDash;
            IL.Celeste.SummitCheckpoint.Update -= TrackSummitCheckpoints;
            On.Celeste.IntroCar.Update -= TrackIntroCar;
            On.Celeste.Key.OnPlayer -= TrackKeys;


            foreach (var hook in SpecialHooks) {
                hook.Dispose();
            }
            SpecialHooks.Clear();
        }

        private static void TrackPicoOrb(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchStfld(typeof(Pico8.Classic.player), "djump"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.EmitDelegate<Action>(() => {
                if (BingoClient.Instance.ModSaveData == null) {
                    return;
                }
                
                BingoClient.Instance.ModSaveData.AddFlag("pico_orb");
            });
        }

        private static void TrackKeys(On.Celeste.Key.orig_OnPlayer orig, Key self, Player player) {
            orig(self, player);
            var area = SaveData.Instance.CurrentSession.Area;
            BingoClient.Instance.ModSaveData.AddFlag($"key:{area.ID}:{(int) area.Mode}:{self.ID}");
        }

        private static void TrackIntroCar(On.Celeste.IntroCar.orig_Update orig, IntroCar self) {
            orig(self);
            if (self.HasRider() && SaveData.Instance.CurrentSession.Level == "e-01") {
                BingoClient.Instance.ModSaveData.AddFlag("remembered_intro_car");
            }
        }

        private static void TrackSummitCheckpoints(ILContext il) {
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
                    BingoClient.Instance.ModSaveData.AddFlag("all_summit_flags");
                }
            });
        }

        private static void TrackEmptySpace(ILContext il) {
            var cursor = new ILCursor(il);
            for (var i = 0; i < 5; i++) {
                if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchLdfld(typeof(HeartGem), "IsFake"))) {
                    throw new Exception("Could not find patch point");
                }
            }

            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate<Action<bool>>(b => {
                if (b) {
                    BingoClient.Instance.ModSaveData.AddFlag("empty_space");
                }
            });
        }

        private static DashCollisionResults KevinDash(On.Celeste.CrushBlock.orig_OnDashed orig, CrushBlock self, Player player, Vector2 dir) {
            var dirCh = dir.Y < 0 ? 'u' : dir.Y > 0 ? 'd' : dir.X < 0 ? 'l' : 'r';
            var level = self.Scene as Level ?? throw new Exception("but why tho");
            level.Session.SetFlag("kevin:"+ dirCh);
            if (new[] {"kevin:u", "kevin:d", "kevin:l", "kevin:r"}.All(f => level.Session.GetFlag(f))) {
                BingoClient.Instance.ModSaveData.AddFlag("kevin");
            }
            return orig(self, player, dir);
        }

        private static void CheckIceRoom(On.Celeste.CoreModeToggle.orig_OnChangeMode orig, CoreModeToggle self, Session.CoreModes mode) {
            orig(self, mode);
            var level = self.Scene as Level ?? throw new Exception("dude what");
            if (mode == global::Celeste.Session.CoreModes.Cold && level.Session.Level == "b-04") {
                BingoClient.Instance.ModSaveData.AddFlag("first_ice");
            }
        }

        private static void CheckPicoProximity(On.Celeste.PicoConsole.orig_Update orig, PicoConsole self) {
            orig(self);
            var player = self.Scene.Tracker.GetEntity<Player>();
            if (player == null) {
                return;
            }
            var level = self.Scene as Level ?? throw new Exception("how'd you do that");

            var screenSpacePos = level.Camera.CameraToScreen(self.Position);
            if (level.Camera.Viewport.Bounds.Contains(new Point((int)screenSpacePos.X, (int)screenSpacePos.Y)) && player.Y < self.Y + 16) {
                BingoClient.Instance.ModSaveData.AddFlag("foundpico");
            }
        }

        private static bool IsSkipping;
        private static void OnEndCutscene(On.Celeste.Level.orig_EndCutscene orig, Level self) {
            orig(self);
            if (self.SkippingCutscene) {
                return;
            }

            if (IsSkipping) {
                IsSkipping = false;
                return;
            }

            var where = self.Session.Level;
            if (self.Session.Area.ID == 5 && where == "e-00" && self.Session.RespawnPoint.Value.Y > 1300) {
                where = "search";
            }
            BingoClient.Instance.ModSaveData.AddFlag($"cutscene:{self.Session.Area.ID}:{where}");
        }

        private static void DebugTheo2(ILContext il) {
            var cursor = new ILCursor(il);
            
            if (!cursor.TryGotoNext(MoveType.Before, insn => insn.MatchCallvirt<Level>("EndCutscene"))) {
                throw new Exception("Could not find patch point");
            }

            // load-bearing nop. do not remove
            cursor.EmitDelegate<Action>(() => { });
        }

        private static void MarkSkippedCutscene(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before, insn => insn.MatchCallvirt<Level>("EndCutscene"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.EmitDelegate<Action>(() => IsSkipping = true);
        }

        private static void OnComplete(Level level) {
            var checkpoint = BingoMonitor.CountCheckpoints(level.Session.Area);
            UpdateOnCheckpoint(level.Session.Area, checkpoint);
        }

        private static void OnTransition(Level level, LevelData next, Vector2 direction) {
            var player = level.Tracker.GetEntity<Player>();
            var area = level.Session.Area;
            var prev = level.Session.MapData.GetAt(player.Position - direction * 16);
            if (prev.Name == next.Name) {
                // just in case!
                return;
            }

            var checkpoint = BingoMonitor.IsCheckpointRoom(next.Name);
            UpdateOnCheckpoint(level.Session.Area, checkpoint);
        
            switch (next.Name) {
                case "b-00c" when area == new AreaKey(6):
                    BingoClient.Instance.ModSaveData.AddFlag("room:easteregg");
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
                        BingoClient.Instance.ModSaveData.AddFlag("room:hollows:bottom");
                    }
                    if (new[] {"hollows:0:top", "hollows:1:top", "hollows:2:top", "hollows:3:top", "hollows:4:top", "hollows:5:top"}.All(f => level.Session.GetFlag(f))) {
                        BingoClient.Instance.ModSaveData.AddFlag("room:hollows:top");
                    }
                    break;
                case "d-00" when prev.Name == "c-10" && area == new AreaKey(4):
                    BingoClient.Instance.ModSaveData.AddFlag("room:oldtrailsecret");
                    break;
                case "secret" when area == new AreaKey(8):
                    BingoClient.Instance.ModSaveData.AddFlag("room:birdnest");
                    break;
            }
        }
        
        private static void TrackPicoBerries(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchLdfld(typeof(Pico8.Classic), "got_fruit"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate<Action<HashSet<int>>>(set => {
                if (BingoClient.Instance.ModSaveData == null) {
                    return;
                }
                
                BingoClient.Instance.ModSaveData.PicoBerries = Math.Max(BingoClient.Instance.ModSaveData.PicoBerries, set.Count + 1);
            });
        }

        private static void TrackPicoRooms(On.Celeste.Pico8.Classic.orig_load_room orig, Pico8.Classic self, int x, int y) {
            orig(self, x, y);
            if (BingoClient.Instance.ModSaveData == null) {
                return;
            }

            if (x == 3 && y == 1) {
                BingoClient.Instance.ModSaveData.AddFlag("pico_oldsite");
            }

            if (x == 6 && y == 3) {
                BingoClient.Instance.ModSaveData.AddFlag("pico_complete");
            }
        }

        private static void TrackClutter(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall(typeof(Input), "Rumble"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<ClutterSwitch>>(entity => {
                var color = (int)typeof(ClutterSwitch).GetField("color", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entity);
                BingoClient.Instance.ModSession.HugeMessOrder.Add(color);
                if (BingoClient.Instance.ModSession.HugeMessOrder.Count == 3) {
                    BingoClient.Instance.ModSaveData.AddHugeMessOrder(BingoClient.Instance.ModSession.HugeMessOrder[0], BingoClient.Instance.ModSession.HugeMessOrder[1], BingoClient.Instance.ModSession.HugeMessOrder[2]);
                }
            });
        }

        private static void TrackSeekerDeath(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchCall<Entity>("RemoveSelf"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.EmitDelegate<Action>(() => {
                // technically BingoClient.Instance can't account for killing two different seekers in the same room (technically possible in 5a but stupid hard)
                // but who cares
                var ident = SaveData.Instance.CurrentSession.Level;
                BingoClient.Instance.ModSaveData.AddSeekerKill(ident);
            });
        }

        private static void Track1up(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, insn => insn.MatchLdfld(typeof(StrawberryPoints), "index"))) {
                throw new Exception("Could not find patch point");
            }

            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate<Action<int>>(idx => {
                if (idx >= 5) {
                    BingoClient.Instance.ModSaveData.OneUps[SaveData.Instance.CurrentSession.Area.ID]++;
                }
                BingoClient.Instance.ModSaveData.MaxOneUpCombo = Math.Max(idx, BingoClient.Instance.ModSaveData.MaxOneUpCombo);
            });
        }


        public static void UpdateOnCheckpoint(AreaKey area, int? checkpoint) {
            if (checkpoint == null || BingoClient.Instance.ModSession.CheckpointStartedVariant == null) {
                return;
            }

            if (BingoClient.Instance.ModSession.CheckpointStartedVariant < checkpoint) {
                foreach (var variant in BingoMonitor.EnabledVariants()) {
                    BingoClient.Instance.ModSaveData.AddCheckpointVariant(area.ID, (int) area.Mode, checkpoint.Value - 1, variant);
                }
            }
        }

    }
}
