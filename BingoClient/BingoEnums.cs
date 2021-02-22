using Microsoft.Xna.Framework;

namespace Celeste.Mod.BingoClient {
    public enum BingoColors {
        Blank,
        Orange,
        Red,
        Blue,
        Green,
        Purple,
        Navy,
        Teal,
        Brown,
        Pink,
        Yellow,
    }

    public enum ObjectiveStatus {
        Nothing, Unknown, Progress, Completed, Claimed
    }
    
    public enum BingoVariant {
        NoGrab,
        NoDash,
        NoJump,
    }
    
    public static class BingoEnumExtensions {
        public static Color ToColor(this BingoColors self) {
            return BingoClient.ColorMap[self.ToString().ToLowerInvariant()];
        }
    }
}
