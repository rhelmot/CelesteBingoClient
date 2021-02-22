using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {

        public enum BingoVariant {
            NoGrab,
            NoDash,
            NoJump,
        }

        public static IEnumerable<BingoVariant> EnabledVariants() {
            return typeof(BingoVariant).GetEnumValues().Cast<BingoVariant>().Where(variant => IsVariantEnabled(variant));
        }

        public static bool IsVariantEnabled(BingoVariant variant) {
            if (SaveData.Instance == null) {
                return false;
            }

            var extvar = ExtendedVariants.Module.ExtendedVariantsModule.Settings;
            switch (variant) {
                case BingoVariant.NoGrab:
                    return SaveData.Instance.VariantMode && SaveData.Instance.Assists.NoGrabbing;
                case BingoVariant.NoJump:
                    return extvar.MasterSwitch && extvar.JumpCount == 0 && extvar.DisableClimbJumping && extvar.DisableNeutralJumping && extvar.DisableWallJumping;
                case BingoVariant.NoDash:
                    return extvar.MasterSwitch && extvar.DashCount == 0;
            }

            return false;
        }

        public static void SetVariantEnabled(BingoVariant variant, bool enabled) {
            if (SaveData.Instance == null) {
                return;
            }

            var extvar = ExtendedVariants.Module.ExtendedVariantsModule.Settings;
            switch (variant) {
                case BingoVariant.NoGrab:
                    if (enabled) SaveData.Instance.VariantMode = true;
                    SaveData.Instance.Assists.NoGrabbing = enabled;
                    break;
                case BingoVariant.NoJump:
                    if (enabled) extvar.MasterSwitch = true;
                    extvar.JumpCount = enabled ? 0 : 1;
                    extvar.DisableClimbJumping = extvar.DisableNeutralJumping = extvar.DisableWallJumping = enabled;
                    break;
                case BingoVariant.NoDash:
                    if (enabled) extvar.MasterSwitch = true;
                    extvar.DashCount = enabled ? 0 : -1;
                    break;
            }
        }

        public static int? AtCheckpoint() {
            if (SaveData.Instance?.CurrentSession == null) {
                return null;
            }

            var level = Engine.Scene as Level;
            var player = level?.Tracker.GetEntity<Player>();
            if (player == null) {
                return null;
            }

            bool first = ReferenceEquals(level.Session.LevelData, level.Session.MapData.Levels[0]);


            var checkpoint = level.Entities.FindFirst<Checkpoint>();
            if (!first && checkpoint == null) {
                return null;
            }

            Vector2 refpoint = checkpoint?.Position ?? level.Session.LevelData.Spawns[0];

            if ((refpoint - player.Position).LengthSquared() > 30 * 30) {
                return null;
            }

            if (first) {
                return 0;
            }

            return IsCheckpointRoom(level.Session.Level);
        }

        public static int? IsCheckpointRoom(string room) {
            var level = Engine.Scene as Level;
            if (level == null) {
                return null;
            }
            
            var checkpointList = AreaData.Get(level.Session.Area)
                .Mode[(int) level.Session.Area.Mode]
                .Checkpoints;
            if (checkpointList == null) {
                return null;
            }
            var list = checkpointList
                .Where(ch => ch != null)
                .Select(ch => ch.Level)
                .ToList();
            if (!list.Contains(room)) {
                return null;
            }

            return list.IndexOf(room) + 1;
        }

        public static int CountCheckpoints(AreaKey area) {
            return (AreaData.Get(area).Mode[(int) area.Mode].Checkpoints?.Length ?? 0) + 1;
        }

        public static IEnumerable<BingoVariant> RelevantVariants() {
            var checkpoint = AtCheckpoint();
            if (checkpoint == null) {
                yield break;
            }
            var area = SaveData.Instance.CurrentSession.Area;

            var seen = new HashSet<BingoVariant>();
            foreach (var square in Instance.Board) {
                if (!ObjectiveVariants.TryGetValue(square.Text, out var variants)) {
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

        private void OnTransitionVariants(Level level, string next) {
            var checkpoint = IsCheckpointRoom(next);
            UpdateOnCheckpoint(checkpoint, level.Session.Area);
        }

        private void OnCompleteVariants(Level level) {
            var checkpoint = CountCheckpoints(level.Session.Area);
            UpdateOnCheckpoint(checkpoint, level.Session.Area);
        }

        private void UpdateOnCheckpoint(int? checkpoint, AreaKey area) {
            if (checkpoint == null || Instance.ModSession.CheckpointStartedVariant == null) {
                return;
            }

            if (Instance.ModSession.CheckpointStartedVariant < checkpoint) {
                foreach (var variant in EnabledVariants()) {
                    Instance.ModSaveData.AddCheckpointVariant(area.ID, (int) area.Mode, checkpoint.Value - 1, variant);
                }
            }
        }
    }
}
