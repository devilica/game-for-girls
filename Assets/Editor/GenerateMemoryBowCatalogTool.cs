#if UNITY_EDITOR
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DressUpGame.Memory;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class GenerateMemoryBowCatalogTool
    {
        public const string CatalogPath = "Assets/Resources/MemoryBowCatalog.asset";
        public const string BowsFolder = "Assets/Resources/MemoryBows";
        private const string LegacyCatalogPath = "Assets/Resources/Memory/MemoryBowCatalog.asset";
        private const string LegacyBowsFolder = "Assets/Resources/Memory/Bows";
        private const string LegacyMemoryFolder = "Assets/Resources/Memory";
        private const int FirstBowIndex = 1;
        private const int LastBowIndex = 18;

        [MenuItem("Dress Up Game/Relocate Memory Game Resources")]
        public static void RelocateMemoryGameResources()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources/MemoryBows"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "MemoryBows");
            }

            if (File.Exists(LegacyCatalogPath))
            {
                string moveResult = AssetDatabase.MoveAsset(LegacyCatalogPath, CatalogPath);
                if (!string.IsNullOrEmpty(moveResult))
                {
                    Debug.LogWarning($"Relocate Memory: catalog move note — {moveResult}");
                }
            }

            for (int i = FirstBowIndex; i <= LastBowIndex; i++)
            {
                string legacyPath = $"{LegacyBowsFolder}/bow_{i}.png";
                string destPath = $"{BowsFolder}/bow_{i}.png";
                if (!File.Exists(legacyPath))
                {
                    continue;
                }

                if (File.Exists(destPath))
                {
                    AssetDatabase.DeleteAsset(destPath);
                }

                string moveResult = AssetDatabase.MoveAsset(legacyPath, destPath);
                if (!string.IsNullOrEmpty(moveResult))
                {
                    Debug.LogWarning($"Relocate Memory: {legacyPath} — {moveResult}");
                }
            }

            if (AssetDatabase.IsValidFolder(LegacyBowsFolder))
            {
                AssetDatabase.DeleteAsset(LegacyBowsFolder);
            }

            if (AssetDatabase.IsValidFolder(LegacyMemoryFolder))
            {
                AssetDatabase.DeleteAsset(LegacyMemoryFolder);
            }

            AssetDatabase.Refresh();
            GenerateMemoryBowCatalog();
            Debug.Log("Dress Up Game: Memory game resources relocated to Resources root (MemoryBowCatalog + MemoryBows).");
        }

        [MenuItem("Dress Up Game/Generate Memory Bow Catalog")]
        public static void GenerateMemoryBowCatalog()
        {
            SyncMemoryBowSpritesTool.SyncMemoryBowSpritesFromSource(regenerateCatalog: false);
            UpdateCatalogFromResourceBows();
        }

        public static void UpdateCatalogFromResourceBows()
        {
            MemoryBowCatalog catalog = AssetDatabase.LoadAssetAtPath<MemoryBowCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<MemoryBowCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            SerializedObject so = new SerializedObject(catalog);
            SerializedProperty bows = so.FindProperty("bows");
            bows.arraySize = LastBowIndex;

            int assigned = 0;
            for (int i = FirstBowIndex; i <= LastBowIndex; i++)
            {
                string assetPath = $"{BowsFolder}/bow_{i}.png";
                Sprite sprite = LoadSpriteAtAssetPath(assetPath);
                bows.GetArrayElementAtIndex(i - 1).objectReferenceValue = sprite;
                if (sprite != null)
                {
                    assigned++;
                }
                else
                {
                    Debug.LogWarning($"Generate Memory Bow Catalog: missing sprite at {assetPath}");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"Dress Up Game: MemoryBowCatalog updated ({assigned}/{LastBowIndex} bows).");
        }

        [MenuItem("Dress Up Game/Validate Memory Game Resources")]
        public static void ValidateMemoryGameResources()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("Memory game resource validation:");

            if (File.Exists("Assets/Resources/Memory.meta"))
            {
                string guid = ReadMetaGuid("Assets/Resources/Memory.meta");
                if (guid.Length != 32)
                {
                    report.AppendLine($"- WARN: Assets/Resources/Memory.meta still exists (GUID len {guid.Length}). Run Relocate Memory Game Resources.");
                }
                else
                {
                    report.AppendLine("- WARN: Legacy Assets/Resources/Memory/ folder meta still present. Run Relocate Memory Game Resources.");
                }
            }

            MemoryBowCatalog assetCatalog = AssetDatabase.LoadAssetAtPath<MemoryBowCatalog>(CatalogPath);
            report.AppendLine(assetCatalog != null
                ? $"- OK: Catalog asset at {CatalogPath}"
                : $"- FAIL: Missing catalog at {CatalogPath}");

            int pngCount = 0;
            for (int i = FirstBowIndex; i <= LastBowIndex; i++)
            {
                if (File.Exists($"{BowsFolder}/bow_{i}.png"))
                {
                    pngCount++;
                }
            }

            report.AppendLine(pngCount == LastBowIndex
                ? $"- OK: {pngCount}/{LastBowIndex} bow PNGs in {BowsFolder}"
                : $"- FAIL: {pngCount}/{LastBowIndex} bow PNGs in {BowsFolder}");

            int outOfSync = 0;
            for (int i = FirstBowIndex; i <= LastBowIndex; i++)
            {
                string sourcePath = $"{SyncMemoryBowSpritesTool.SourceFolder}/bow_{i}.png";
                string destPath = $"{BowsFolder}/bow_{i}.png";
                if (!File.Exists(sourcePath) || !File.Exists(destPath))
                {
                    continue;
                }

                if (!FilesHaveSameHash(sourcePath, destPath))
                {
                    outOfSync++;
                    report.AppendLine(
                        $"- FAIL: bow_{i} out of sync with source ({SyncMemoryBowSpritesTool.SourceFolder}). " +
                        "Run Dress Up Game → Sync Memory Bow Sprites To Resources.");
                }
            }

            if (outOfSync == 0)
            {
                report.AppendLine($"- OK: bow_{FirstBowIndex}..{LastBowIndex} Resources copies match source art.");
            }

            MemoryBowCatalog resourcesCatalog = Resources.Load<MemoryBowCatalog>(MemoryBowCatalog.ResourcePath);
            if (resourcesCatalog == null)
            {
                report.AppendLine($"- FAIL: Resources.Load<{nameof(MemoryBowCatalog)}>(\"{MemoryBowCatalog.ResourcePath}\") returned null.");
            }
            else
            {
                int loaded = CountLoadedBows(resourcesCatalog);
                report.AppendLine(loaded == LastBowIndex
                    ? $"- OK: Resources catalog has {loaded}/{LastBowIndex} bow sprites."
                    : $"- FAIL: Resources catalog has {loaded}/{LastBowIndex} bow sprites.");
            }

            Debug.Log(report.ToString());
        }

        private static Sprite LoadSpriteAtAssetPath(string assetPath)
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

        private static int CountLoadedBows(MemoryBowCatalog catalog)
        {
            int loaded = 0;
            for (int i = FirstBowIndex; i <= LastBowIndex; i++)
            {
                if (catalog.GetBow(i) != null)
                {
                    loaded++;
                }
            }

            return loaded;
        }

        private static string ReadMetaGuid(string metaPath)
        {
            if (!File.Exists(metaPath))
            {
                return string.Empty;
            }

            foreach (string line in File.ReadAllLines(metaPath))
            {
                if (line.StartsWith("guid: "))
                {
                    return line.Substring("guid: ".Length).Trim();
                }
            }

            return string.Empty;
        }

        internal static bool FilesHaveSameHash(string pathA, string pathB)
        {
            using MD5 md5 = MD5.Create();
            byte[] hashA = md5.ComputeHash(File.ReadAllBytes(pathA));
            byte[] hashB = md5.ComputeHash(File.ReadAllBytes(pathB));
            if (hashA.Length != hashB.Length)
            {
                return false;
            }

            for (int i = 0; i < hashA.Length; i++)
            {
                if (hashA[i] != hashB[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif
