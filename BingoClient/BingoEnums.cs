using System.Collections.Generic;
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
        Invisible,
        LowFriction,
        Speed70,
        Speed160,
        NoJumpNoDash,
        Mirrored,
        Hiccups,
    }
    
    public static class BingoEnumExtensions {
        public static Dictionary<BingoColors, Color> SquareColors = new Dictionary<BingoColors, Color> {
            {BingoColors.Blank, Color.Black},
            {BingoColors.Orange, new Color(0xF9, 0x8E, 0x1E)},
            {BingoColors.Red, new Color(0xDA, 0x44, 0x40)},
            {BingoColors.Blue, new Color(0x37, 0xA1, 0xDE)},
            {BingoColors.Green, new Color(0x00, 0xB5, 0x00)},
            {BingoColors.Purple, new Color(0x82, 0x2d, 0xbf)},
            {BingoColors.Navy, new Color(0x0d, 0x48, 0xb5)},
            {BingoColors.Teal, new Color(0x41, 0x96, 0x95)},
            {BingoColors.Brown, new Color(0xab, 0x5c, 0x23)},
            {BingoColors.Pink, new Color(0xed, 0x86, 0xaa)},
            {BingoColors.Yellow, new Color(0xd8, 0xd0, 0x14)},
        };

        public static Dictionary<string, BingoColors> ColorNames = new Dictionary<string, BingoColors> {
            {"blank", BingoColors.Blank},
            {"orange", BingoColors.Orange},
            {"red", BingoColors.Red},
            {"blue", BingoColors.Blue},
            {"green", BingoColors.Green},
            {"purple", BingoColors.Purple},
            {"navy", BingoColors.Navy},
            {"teal", BingoColors.Teal},
            {"brown", BingoColors.Brown},
            {"pink", BingoColors.Pink},
            {"yellow", BingoColors.Yellow},
        };
        
        public static Color ToSquareColor(this BingoColors self) {
            return SquareColors[self];
        }

        public static BingoColors ParseColor(string name) {
            return ColorNames[name.ToLowerInvariant()];
        }

        public static IEnumerable<BingoColors> ParseColors(string names) {
            if (names == "blank") {
                yield break;
            }

            foreach (var name in names.Split(' ')) {
                yield return ParseColor(name);
            }
        }
    }
}
