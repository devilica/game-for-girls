#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Ensures PNG files under Assets/Sprites are imported as 2D sprites.
    /// </summary>
    public static class SpriteImportUtility
    {
        public static Sprite LoadSprite(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            ConfigureAsSprite(assetPath);
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        public static void ConfigureAsSprite(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            int width = 0;
            int height = 0;
            importer.GetSourceTextureWidthAndHeight(out width, out height);
            float targetUnits = 6f;
            float maxDimension = Mathf.Max(width, height);
            float suggestedPpu = maxDimension > 0 ? maxDimension / targetUnits : 100f;
            suggestedPpu = Mathf.Clamp(suggestedPpu, 100f, 800f);

            if (Mathf.Abs(importer.spritePixelsPerUnit - suggestedPpu) > 0.01f)
            {
                importer.spritePixelsPerUnit = suggestedPpu;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
