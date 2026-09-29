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
    /// Generates None + Original + 4 color variants per earring shape (ear_1.png, …).
    /// </summary>
    public static class GenerateEarringColorsTool
    {
        private const string LibraryPath = "Assets/Resources/EarringColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string AccessoryFolder = "Assets/Resources/Items/Accessories";
        private const string EarringSpriteFolder = "Assets/Sprites/Character/Earrings";

        [MenuItem("Dress Up Game/Generate Earring Colors")]
        public static void GenerateEarringColors()
        {
            AccessoryItem earringNone = EnsureEarringNoneItem();
            EarringColorLibrary library = LoadOrCreateLibrary();
            Dictionary<int, EarringLayout> layouts = CollectEarringLayouts();
            Dictionary<int, Sprite> shapeSprites = LoadEarringShapeSprites();

            if (shapeSprites.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Earring Sprites",
                    "Add ear_1.png, ear_2.png, … under Assets/Sprites/Character/Earrings first.",
                    "OK");
                return;
            }

            EarringColorLibrary.ColorEntry[] palette = library.Colors;
            List<AccessoryItem> generatedItems = new List<AccessoryItem>();
            List<int> shapeIndices = new List<int>(shapeSprites.Keys);
            shapeIndices.Sort();

            foreach (int shapeIndex in shapeIndices)
            {
                Sprite shapeSprite = shapeSprites[shapeIndex];
                EarringLayout layout = layouts.TryGetValue(shapeIndex, out EarringLayout found)
                    ? found
                    : EarringLayout.Default;

                string originalId = $"ear_{shapeIndex}";
                AccessoryItem originalItem = LoadOrCreate<AccessoryItem>($"{AccessoryFolder}/{originalId}.asset");
                originalItem.SetRuntimeData(
                    originalId,
                    $"Earrings {shapeIndex} Original",
                    shapeSprite,
                    false,
                    Color.white);
                originalItem.SetLayerLayout(layout.Offset, layout.Scale);
                EditorUtility.SetDirty(originalItem);
                generatedItems.Add(originalItem);

                for (int variantIndex = 0; variantIndex < palette.Length; variantIndex++)
                {
                    int colorIndex = (shapeIndex - 1 + variantIndex) % palette.Length;
                    EarringColorLibrary.ColorEntry colorEntry = palette[colorIndex];
                    string suffix = palette[variantIndex].suffix;
                    string id = $"ear_{shapeIndex}_{suffix}";
                    string path = $"{AccessoryFolder}/{id}.asset";
                    AccessoryItem item = LoadOrCreate<AccessoryItem>(path);
                    item.SetRuntimeData(
                        id,
                        $"Earrings {shapeIndex} {colorEntry.displayName}",
                        shapeSprite,
                        false,
                        colorEntry.color);
                    item.SetLayerLayout(layout.Offset, layout.Scale);
                    EditorUtility.SetDirty(item);
                    generatedItems.Add(item);
                }
            }

            UpdateCatalog(earringNone, generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} earring items (original + color variants, + earring_none).");
        }

        private static Dictionary<int, Sprite> LoadEarringShapeSprites()
        {
            Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
            if (!Directory.Exists(EarringSpriteFolder))
            {
                return sprites;
            }

            foreach (string pngPath in Directory.GetFiles(EarringSpriteFolder, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(pngPath);
                Match match = Regex.Match(fileName, @"^ear_(\d+)$", RegexOptions.IgnoreCase);
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

        private static Dictionary<int, EarringLayout> CollectEarringLayouts()
        {
            Dictionary<int, EarringLayout> layouts = new Dictionary<int, EarringLayout>();
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                return layouts;
            }

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null || !item.IsEarringVariant)
                {
                    continue;
                }

                Match match = Regex.Match(item.Id, @"^ear_(\d+)(?:_\d+)?$");
                if (!match.Success)
                {
                    continue;
                }

                int shapeIndex = int.Parse(match.Groups[1].Value);
                if (!layouts.ContainsKey(shapeIndex))
                {
                    layouts[shapeIndex] = new EarringLayout(item.LayerOffset, item.LayerScale);
                }
            }

            return layouts;
        }

        private static void UpdateCatalog(AccessoryItem earringNone, List<AccessoryItem> earringItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<AccessoryItem> rebuilt = new List<AccessoryItem>();

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item != null && (item.IsNecklaceNone || item.IsNecklaceVariant))
                {
                    rebuilt.Add(item);
                }
            }

            rebuilt.Add(earringNone);
            rebuilt.AddRange(earringItems);

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.IsNecklaceNone || item.IsNecklaceVariant
                    || item.IsEarringNone || item.IsEarringVariant
                    || item.Id == "accessory_earrings")
                {
                    continue;
                }

                rebuilt.Add(item);
            }

            catalog.SetAccessoryItems(rebuilt);
            EditorUtility.SetDirty(catalog);
        }

        private static AccessoryItem EnsureEarringNoneItem()
        {
            AccessoryItem earringNone = LoadOrCreate<AccessoryItem>($"{AccessoryFolder}/earring_none.asset");
            earringNone.SetRuntimeData("earring_none", "None", null, true);
            EditorUtility.SetDirty(earringNone);
            return earringNone;
        }

        private static EarringColorLibrary LoadOrCreateLibrary()
        {
            EarringColorLibrary library = AssetDatabase.LoadAssetAtPath<EarringColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<EarringColorLibrary>();
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

        private readonly struct EarringLayout
        {
            public static EarringLayout Default => new EarringLayout(
                new Vector3(0f, 2.05f, 0f),
                new Vector3(0.35f, 0.35f, 1f));

            public Vector3 Offset { get; }
            public Vector3 Scale { get; }

            public EarringLayout(Vector3 offset, Vector3 scale)
            {
                Offset = offset;
                Scale = scale;
            }
        }
    }
}
#endif
