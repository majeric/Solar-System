using UnityEngine;

namespace Visualization {
    public static class Palette {
        public static readonly Color red = ColorFromHex ("#ff424c");
        public static readonly Color green = ColorFromHex ("#80fc4e");
        public static readonly Color blue = ColorFromHex ("#4794ff");

        private static Color ColorFromHex (string hex) {
            Color col;
            ColorUtility.TryParseHtmlString (hex, out col);
            return col;
        }
    }
}