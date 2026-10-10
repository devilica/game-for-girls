#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class MenuButtonSpritesImportTool
    {
        private static readonly string[] MenuButtonPaths =
        {
            "Assets/Resources/UI/menu/game_1.png",
            "Assets/Resources/UI/menu/game_2.png",
            "Assets/Resources/UI/menu/game_3.png",
            "Assets/Resources/UI/menu/game_4.png",
            "Assets/Resources/UI/menu/banner-more.png",
        };

        [MenuItem("Dress Up Game/Fix Menu Button Sprites")]
        public static void FixMenuButtonSpritesFromMenu()
        {
            int fixedCount = 0;
            foreach (string path in MenuButtonPaths)
            {
                if (ApplySpriteImportSettings(path))
                {
                    fixedCount++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Fixed sprite import on {fixedCount} menu button texture(s). Re-enter Play Mode on MenuScene.");
        }

        public static bool ApplySpriteImportSettings(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"Dress Up Game: Missing menu texture at {assetPath}");
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
            return true;
        }
    }
}
#endif
