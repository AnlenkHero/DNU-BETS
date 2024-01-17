using UnityEngine;

public static class TextureScale
{
    // Method to resize the texture
    public static Texture2D Bilinear(Texture2D texture, int newWidth, int newHeight)
    {
        return ThreadedScale(texture, newWidth, newHeight, true);
    }

    private static Texture2D ThreadedScale(Texture2D texture, int newWidth, int newHeight, bool useBilinear)
    {
        Texture2D newTexture = new Texture2D(newWidth, newHeight, texture.format, false);
        float scaleX = (float)texture.width / newWidth;
        float scaleY = (float)texture.height / newHeight;
        int newPixelCount = newWidth * newHeight;
        Color[] newColors = new Color[newPixelCount];
        int k = 0;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                if (useBilinear)
                {
                    float gx = ((float)x) * scaleX;
                    float gy = ((float)y) * scaleY;
                    int gxi = (int)gx;
                    int gyi = (int)gy;
                    Color c00 = texture.GetPixel(gxi, gyi);
                    Color c10 = texture.GetPixel(gxi + 1, gyi);
                    Color c01 = texture.GetPixel(gxi, gyi + 1);
                    Color c11 = texture.GetPixel(gxi + 1, gyi + 1);

                    float u = gx - gxi;
                    float v = gy - gyi;
                    float u1 = 1 - u;
                    float v1 = 1 - v;

                    newColors[k++] = u1 * v1 * c00 + u * v1 * c10 + u1 * v * c01 + u * v * c11;
                }
            }
        }

        newTexture.SetPixels(newColors);
        newTexture.Apply();

        return newTexture;
    }
}