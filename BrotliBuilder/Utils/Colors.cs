using System.Drawing;

namespace BrotliBuilder.Utils{
    static class Colors{
        public static readonly Color[] List = {
            Color.FromArgb(183,  69,  59),
            Color.FromArgb(239, 173,  44),
            Color.FromArgb(  0, 176,  51),
            Color.FromArgb( 33, 203, 255),
            Color.FromArgb(246, 175, 255),
            Color.FromArgb(204, 104,  51),
            Color.FromArgb(184, 202,   0),
            Color.FromArgb(  0, 203, 176),
            Color.FromArgb(142, 134, 244),
            Color.FromArgb(223, 138,  43),
            Color.FromArgb(252, 208,  62),
            Color.FromArgb(122, 190,   0),
            Color.FromArgb(  0, 175, 177),
            Color.FromArgb(162,  95, 218)
        };

        public static Color Mix(Color color1, Color color2, double amount){
            double r = color1.R * (1D - amount) + color2.R * amount;
            double g = color1.G * (1D - amount) + color2.G * amount;
            double b = color1.B * (1D - amount) + color2.B * amount;

            return Color.FromArgb((byte)r, (byte)g, (byte)b);
        }
    }
}
