#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class MemoryHeaderSpriteImportTool
    {
        public const string HeaderSpritePath = "Assets/Sprites/Memory/memory.png";
        public const string HeaderResourcesPath = "Assets/Resources/Memory/memory.png";

        [MenuItem("Dress Up Game/Fix Memory Header Sprite")]
        public static void FixMemoryHeaderSpriteFromMenu()
        {
            ApplySpriteImportSettings(HeaderSpritePath);
            SyncResourcesCopy(HeaderSpritePath, HeaderResourcesPath);
            ApplySpriteImportSettings(HeaderResourcesPath);
            AssetDatabase.Refresh();

            Sprite sprite = LoadUiSprite(HeaderResourcesPath);
            if (sprite == null)
            {
                Debug.LogError("Dress Up Game: Memory header sprite fix failed.");
                return;
            }

            Debug.Log("Dress Up Game: Memory header sprite ready. Run Setup Memory Scene to add banner.");
        }

        public static void EnsureMemoryHeaderSpriteReady()
        {
            ApplySpriteImportSettings(HeaderSpritePath);
            SyncResourcesCopy(HeaderSpritePath, HeaderResourcesPath);
            ApplySpriteImportSettings(HeaderResourcesPath);
        }

        public static bool ApplySpriteImportSettings(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Bilinear;
            if (assetPath.Contains("/Resources/Memory/"))
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
