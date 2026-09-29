#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Generates simple colored placeholder PNG sprites for testing before real art is imported.
    /// Replace generated sprites by dropping PNGs into the same folder paths.
    /// </summary>
    public static class PlaceholderSpriteFactory
    {
        public static Sprite CreateAndSaveSprite(string assetPath, Color color, int width, int height, string label = null)
        {
            EnsureDirectory(assetPath);

            // Keep user-provided art; only generate placeholders when the PNG is missing.
            if (File.Exists(assetPath))
            {
                return SpriteImportUtility.LoadSprite(assetPath);
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            Color fill = color;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool border = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    pixels[y * width + x] = border ? Color.white : fill;
                }
            }

            if (!string.IsNullOrEmpty(label))
            {
                DrawLabel(pixels, width, height, label, Color.black);
            }

            texture.SetPixels(pixels);
            texture.Apply();

            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);

            File.WriteAllBytes(assetPath, png);
            AssetDatabase.ImportAsset(assetPath);
            return SpriteImportUtility.LoadSprite(assetPath);
        }

        public static Sprite CreateHairSprite(string assetPath, int styleIndex, Color baseColor)
        {
            EnsureDirectory(assetPath);

            if (File.Exists(assetPath))
            {
                return SpriteImportUtility.LoadSprite(assetPath);
            }

            int width = 180;
            int height = 140;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = clear;
            }

            int styleOffset = styleIndex * 8;
            FillEllipse(pixels, width, height, width / 2, height - 30, 70 + styleOffset, 50, baseColor);
            FillEllipse(pixels, width, height, width / 2 - 40, height - 20, 35, 60 + styleOffset / 2, baseColor);
            FillEllipse(pixels, width, height, width / 2 + 40, height - 20, 35, 60 + styleOffset / 2, baseColor);

            texture.SetPixels(pixels);
            texture.Apply();

            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);

            File.WriteAllBytes(assetPath, png);
            AssetDatabase.ImportAsset(assetPath);
            return SpriteImportUtility.LoadSprite(assetPath);
        }

        private static void EnsureDirectory(string assetPath)
        {
            string directory = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void FillEllipse(Color[] pixels, int width, int height, int cx, int cy, int rx, int ry, Color color)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - cx) / (float)rx;
                    float dy = (y - cy) / (float)ry;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }
        }

        private static void DrawLabel(Color[] pixels, int width, int height, string label, Color color)
        {
            // Simple marker dot so placeholders are distinguishable in the Project window preview.
            int cx = width / 2;
            int cy = height / 2;
            for (int y = cy - 2; y <= cy + 2; y++)
            {
                for (int x = cx - 2; x <= cx + 2; x++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }
        }
    }
}
#endif
