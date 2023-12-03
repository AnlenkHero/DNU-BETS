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
        
        public static string ToHexString(Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }
    }
}