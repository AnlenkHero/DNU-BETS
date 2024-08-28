using UnityEngine;

namespace Libs.Helpers
{
    public static class ColorHelper
    {
        public static readonly Color32 LightGreen = new(0x90, 0xEE, 0x90, 0xFF);
        public static readonly string LightGreenString = ToHexString(LightGreen);
        
        public static readonly Color32 HotPink = new(0xFF, 0x69, 0xB4, 0xFF);
        public static readonly string HotPinkString = ToHexString(HotPink);
        
        public static readonly Color32 PaleYellow = new(0xFD, 0xFD, 0x96, 0xFF);
        public static readonly string PaleYellowString = ToHexString(PaleYellow);

        public static readonly Color32 Orange = new(0xE3,0xA0,0x18, 0xFF);
        public static readonly string OrangeString = ToHexString(Orange);
        
        public static readonly Color32 PastelGray = new(0xB6, 0xA8, 0xA8, 0xFF);
        public static readonly string PastelGrayString = ToHexString(PastelGray);

        public static readonly Color32 Black = new(0x00, 0x00, 0x00, 0xFF);
        public static readonly string BlackString = ToHexString(Black);

        public static readonly Color32 White = new(0xFF, 0xFF, 0xFF, 0xFF);
        public static readonly string WhiteString = ToHexString(White);
        
        public static string ToHexString(Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }
    }
}