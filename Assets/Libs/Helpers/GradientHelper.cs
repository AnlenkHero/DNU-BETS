using UnityEngine;

namespace Libs.Helpers
{
    public static class GradientHelper
    {
        public static Gradient CreateGradient(Color32[] colors, float[] times)
        {
            if (colors.Length != times.Length)
            {
                throw new System.ArgumentException("Colors and times arrays must have the same length");
            }

            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[colors.Length];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                colorKeys[i].color = colors[i];
                colorKeys[i].time = times[i];

                alphaKeys[i].alpha = colors[i].a / 255.0f;
                alphaKeys[i].time = times[i];
            }

            gradient.SetKeys(colorKeys, alphaKeys);

            return gradient;
        }
    }
}