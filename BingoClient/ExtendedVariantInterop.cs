using System;
using MonoMod.ModInterop;

namespace Celeste.Mod.BingoClient {
    [ModImportName("ExtendedVariantMode")]
    public class ExtendedVariantInterop {
        public static Func<string, object> GetCurrentVariantValue;
        public static Action<string, object, bool> TriggerVariant;

        public static int JumpCount {
            get => (int) GetCurrentVariantValue("JumpCount");
            set => TriggerVariant("JumpCount", value, false);
        }
        public static int DashCount {
            get => (int) GetCurrentVariantValue("DashCount");
            set => TriggerVariant("DashCount", value, false);
        }
        public static bool DisableClimbJumping {
            get => (bool) GetCurrentVariantValue("DisableClimbJumping");
            set => TriggerVariant("DisableClimbJumping", value, false);
        }
        public static bool DisableNeutralJumping {
            get => (bool) GetCurrentVariantValue("DisableNeutralJumping");
            set => TriggerVariant("DisableNeutralJumping", value, false);
        }
        public static bool DisableWallJumping {
            get => (bool) GetCurrentVariantValue("DisableWallJumping");
            set => TriggerVariant("DisableWallJumping", value, false);
        }
    }
}
