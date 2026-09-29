#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DressUpGame.Data;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Generates 20 color variants for each lipstick shape (lipstic_1.png, lipstic_2.png).
    /// </summary>
    public static class GenerateLipstickColorsTool
    {
        private const string LibraryPath = "Assets/Resources/LipstickColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string MakeupFolder = "Assets/Resources/Items/Makeup";
        private const string MakeupSpriteFolder = "Assets/Sprites/Character/Makeup";

        public static void GenerateLipstickColors()
        {
            LipstickColorLibrary library = LoadOrCreateLibrary();
            Dictionary<int, LipstickLayout> layouts = CollectLipstickLayouts();
            Dictionary<int, Sprite> shapeSprites = LoadLipstickShapeSprites();

            if (shapeSprites.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Lipstick Sprites",
                    "Add lipstic_1.png and lipstic_2.png under Assets/Sprites/Character/Makeup first.",
                    "OK");
                return;
            }

            List<MakeupItem> generatedItems = new List<MakeupItem>();
            List<int> shapeIndices = new List<int>(shapeSprites.Keys);
            shapeIndices.Sort();

            foreach (int shapeIndex in shapeIndices)
            {
                Sprite shapeSprite = shapeSprites[shapeIndex];
                LipstickLayout layout = layouts.TryGetValue(shapeIndex, out LipstickLayout found)
                    ? found
                    : LipstickLayout.Default;

                foreach (LipstickColorLibrary.ColorEntry colorEntry in library.Colors)
                {
                    string id = $"lipstick_{shapeIndex}_{colorEntry.suffix}";
                    string assetName = id;
                    string path = $"{MakeupFolder}/{assetName}.asset";
                    MakeupItem item = LoadOrCreate<MakeupItem>(path);
                    item.SetRuntimeData(
                        id,
                        $"Lipstick {shapeIndex} {colorEntry.displayName}",
                        shapeSprite,
                        MakeupType.Lipstick,
                        colorEntry.color,
                        false);
                    item.SetLayerLayout(layout.Offset, layout.Scale);
                    EditorUtility.SetDirty(item);
                    generatedItems.Add(item);
                }
            }

            UpdateCatalog(generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} lipstick color items.");
        }

        private static Dictionary<int, Sprite> LoadLipstickShapeSprites()
        {
            Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
            if (!Directory.Exists(MakeupSpriteFolder))
            {
                return sprites;
            }

            foreach (string pngPath in Directory.GetFiles(MakeupSpriteFolder, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(pngPath);
                Match match = Regex.Match(fileName, @"^lipstic_(\d+)$", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    match = Regex.Match(fileName, @"^lipstick_(\d+)$", RegexOptions.IgnoreCase);
                }

                if (!match.Success)
                {
                    continue;
                }

                int shapeIndex = int.Parse(match.Groups[1].Value);
                Sprite sprite = SpriteImportUtility.LoadSprite(pngPath);
                if (sprite != null)
                {
                    sprites[shapeIndex] = sprite;
                }
            }

            return sprites;
        }

        private static Dictionary<int, LipstickLayout> CollectLipstickLayouts()
        {
            Dictionary<int, LipstickLayout> layouts = new Dictionary<int, LipstickLayout>();
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                return layouts;
            }

            foreach (MakeupItem item in catalog.MakeupItems)
            {
                if (item == null || item.MakeupType != MakeupType.Lipstick)
                {
                    continue;
                }

                Match match = Regex.Match(item.Id, @"^lipstick_(\d+)$");
                if (!match.Success)
                {
                    continue;
                }

                int shapeIndex = int.Parse(match.Groups[1].Value);
                layouts[shapeIndex] = new LipstickLayout(item.LayerOffset, item.LayerScale);
            }

            return layouts;
        }

        private static void UpdateCatalog(List<MakeupItem> lipstickItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<MakeupItem> rebuilt = new List<MakeupItem>();
            bool insertedLipstickColors = false;
            foreach (MakeupItem item in catalog.MakeupItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.MakeupType == MakeupType.Lipstick && !item.IsNoneOption)
                {
                    continue;
                }

                rebuilt.Add(item);

                if (!insertedLipstickColors && item.MakeupType == MakeupType.Lipstick && item.IsNoneOption)
                {
                    rebuilt.AddRange(lipstickItems);
                    insertedLipstickColors = true;
                }
            }

            if (!insertedLipstickColors)
            {
                rebuilt.InsertRange(0, lipstickItems);
            }

            catalog.SetMakeupItems(rebuilt);
            EditorUtility.SetDirty(catalog);
        }

        private static LipstickColorLibrary LoadOrCreateLibrary()
        {
            LipstickColorLibrary library = AssetDatabase.LoadAssetAtPath<LipstickColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<LipstickColorLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
            EditorUtility.SetDirty(library);
            return library;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private readonly struct LipstickLayout
        {
            public static LipstickLayout Default => new LipstickLayout(Vector3.zero, Vector3.one);

            public Vector3 Offset { get; }
            public Vector3 Scale { get; }

            public LipstickLayout(Vector3 offset, Vector3 scale)
            {
                Offset = offset;
                Scale = scale;
            }
        }
    }
}
#endif
