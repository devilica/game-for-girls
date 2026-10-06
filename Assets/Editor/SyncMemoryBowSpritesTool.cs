#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class SyncMemoryBowSpritesTool
    {
        public const string SourceFolder = "Assets/Sprites/Memory/Bows";
        public const string DestFolder = GenerateMemoryBowCatalogTool.BowsFolder;
        public const int FirstBowIndex = 1;
        public const int LastBowIndex = 18;

        private static bool syncInProgress;

        public static bool IsSyncInProgress => syncInProgress;

        [MenuItem("Dress Up Game/Fix Memory Bow Resource Imports")]
        public static void FixMemoryBowResourceImports()
        {
            int fixedCount = 0;
            for (int i = FirstBowIndex; i <= LastBowIndex; i++)
            {
                string destPath = $"{DestFolder}/bow_{i}.png";
                if (!File.Exists(destPath))
                {
                    continue;
                }

                if (ApplySpriteImportSettings(destPath))
                {
                    fixedCount++;
                }
            }

            AssetDatabase.Refresh();
            GenerateMemoryBowCatalogTool.UpdateCatalogFromResourceBows();
            Debug.Log($"Dress Up Game: reimported {fixedCount} memory bow sprites under {DestFolder}.");
        }

        [MenuItem("Dress Up Game/Sync Memory Bow Sprites To Resources")]
        public static void SyncMemoryBowSpritesToResources()
        {
            SyncMemoryBowSpritesFromSource(regenerateCatalog: true);
        }

        /// <summary>
        /// Copies bow PNGs from Sprites/Memory/Bows into Resources/MemoryBows for runtime loading.
        /// </summary>
        public static int SyncMemoryBowSpritesFromSource(bool regenerateCatalog)
        {
            if (syncInProgress)
            {
                return 0;
            }

            syncInProgress = true;
            try
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                if (!AssetDatabase.IsValidFolder(DestFolder))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "MemoryBows");
                }

                int copied = 0;
                for (int i = FirstBowIndex; i <= LastBowIndex; i++)
                {
                    string fileName = $"bow_{i}.png";
                    string sourcePath = $"{SourceFolder}/{fileName}";
                    string destPath = $"{DestFolder}/{fileName}";

                    if (!File.Exists(sourcePath))
                    {
                        Debug.LogWarning($"Sync Memory Bows: missing source {sourcePath}");
                        continue;
                    }

                    if (AssetDatabase.LoadAssetAtPath<Object>(destPath) != null)
                    {
                        AssetDatabase.DeleteAsset(destPath);
                    }

                    if (!AssetDatabase.CopyAsset(sourcePath, destPath))
                    {
                        Debug.LogWarning($"Sync Memory Bows: CopyAsset failed for {fileName}");
                        continue;
                    }

                    if (!ApplySpriteImportSettings(destPath))
                    {
                        Debug.LogWarning($"Sync Memory Bows: import settings failed for {fileName}");
                        continue;
                    }

                    copied++;
                }

                AssetDatabase.Refresh();

                if (regenerateCatalog)
                {
                    GenerateMemoryBowCatalogTool.UpdateCatalogFromResourceBows();
                }

                if (copied > 0)
                {
                    Debug.Log($"Dress Up Game: synced {copied} memory bow sprites to {DestFolder}.");
                }

                return copied;
            }
            finally
            {
                syncInProgress = false;
            }
        }

        private static bool ApplySpriteImportSettings(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            return true;
        }
    }
}
#endif
