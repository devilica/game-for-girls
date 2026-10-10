#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class MysteryBoxSpriteImportTool
    {
        public const string BoxSpritePath = "Assets/Sprites/Mistery Box/box.png";
        public const string HeaderSpritePath = "Assets/Sprites/Mistery Box/mistery.png";
        public const string RevealBackgroundSpritePath = "Assets/Sprites/Mistery Box/mistery-bg.png";
        public const string ApplyButtonSpritePath = "Assets/Sprites/Mistery Box/apply.png";
        public const string RefreshButtonSpritePath = "Assets/Sprites/Mistery Box/refresh.png";
        public const string BoxResourcesPath = "Assets/Resources/MysteryBox/box.png";
        public const string HeaderResourcesPath = "Assets/Resources/MysteryBox/mistery.png";
        public const string RevealBackgroundResourcesPath = "Assets/Resources/MysteryBox/mistery-bg.png";
        public const string ApplyButtonResourcesPath = "Assets/Resources/MysteryBox/apply.png";
        public const string RefreshButtonResourcesPath = "Assets/Resources/MysteryBox/refresh.png";

        [MenuItem("Dress Up Game/Fix Mystery Box UI Sprites")]
        public static void FixMysteryBoxUiSpritesFromMenu()
        {
            int fixedCount = 0;
            if (ApplySpriteImportSettings(BoxSpritePath))
            {
                fixedCount++;
            }

            if (ApplySpriteImportSettings(HeaderSpritePath))
            {
                fixedCount++;
            }

            if (ApplySpriteImportSettings(RevealBackgroundSpritePath))
            {
                fixedCount++;
            }

            if (ApplySpriteImportSettings(ApplyButtonSpritePath))
            {
                fixedCount++;
            }

            if (ApplySpriteImportSettings(RefreshButtonSpritePath))
            {
                fixedCount++;
            }

            SyncResourcesCopy(BoxSpritePath, BoxResourcesPath);
            SyncResourcesCopy(HeaderSpritePath, HeaderResourcesPath);
            SyncResourcesCopy(RevealBackgroundSpritePath, RevealBackgroundResourcesPath);
            SyncResourcesCopy(ApplyButtonSpritePath, ApplyButtonResourcesPath);
            SyncResourcesCopy(RefreshButtonSpritePath, RefreshButtonResourcesPath);
            if (ApplySpriteImportSettings(BoxResourcesPath))
            {
                fixedCount++;
            }

            if (ApplySpriteImportSettings(HeaderResourcesPath))
            {
                fixedCount++;
            }

            if (ApplySpriteImportSettings(RevealBackgroundResourcesPath))
            {
                fixedCount++;
            }

            if (ApplySpriteImportSettings(ApplyButtonResourcesPath))
            {
                fixedCount++;
            }

            if (ApplySpriteImportSettings(RefreshButtonResourcesPath))
            {
                fixedCount++;
            }

            AssetDatabase.Refresh();
            Sprite box = LoadUiSprite(BoxSpritePath);
            Sprite header = LoadUiSprite(HeaderSpritePath);
            Sprite revealBg = LoadUiSprite(RevealBackgroundSpritePath);
            Sprite apply = LoadUiSprite(ApplyButtonSpritePath);
            Sprite refresh = LoadUiSprite(RefreshButtonSpritePath);
            if (box == null || header == null || revealBg == null || apply == null || refresh == null)
            {
                Debug.LogError(
                    "Dress Up Game: Mystery Box UI sprite fix failed. " +
                    $"box={(box != null ? "OK" : "FAIL")}, header={(header != null ? "OK" : "FAIL")}, " +
                    $"revealBg={(revealBg != null ? "OK" : "FAIL")}, apply={(apply != null ? "OK" : "FAIL")}, " +
                    $"refresh={(refresh != null ? "OK" : "FAIL")}.");
                return;
            }

            Debug.Log($"Dress Up Game: Mystery Box UI sprites fixed ({fixedCount} textures). Run Setup Mystery Box Scene to refresh layout.");
        }

        public static bool ApplySpriteImportSettings(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"Mystery Box UI: missing texture at {assetPath}");
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Bilinear;
            if (assetPath.Contains("/Resources/MysteryBox/"))
            {
                importer.isReadable = true;
            }

            importer.SaveAndReimport();
            return true;
        }

        public static Sprite LoadUiSprite(string assetPath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                return sprite;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (Object asset in assets)
            {
                if (asset is Sprite assetSprite)
                {
                    return assetSprite;
                }
            }

            return null;
        }

        public static void SyncResourcesCopy(string sourceAssetPath, string resourcesAssetPath)
        {
            if (!System.IO.File.Exists(sourceAssetPath))
            {
                return;
            }

            string directory = System.IO.Path.GetDirectoryName(resourcesAssetPath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            System.IO.File.Copy(sourceAssetPath, resourcesAssetPath, true);
        }
    }
}
#endif
