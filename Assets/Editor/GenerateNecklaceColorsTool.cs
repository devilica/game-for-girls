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
    /// Generates 4 distinct color variants per necklace shape (necklace_1.png, …).
    /// </summary>
    public static class GenerateNecklaceColorsTool
    {
        private const string LibraryPath = "Assets/Resources/NecklaceColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string AccessoryFolder = "Assets/Resources/Items/Accessories";
        private const string NecklaceSpriteFolder = "Assets/Sprites/Character/Necklace";

        [MenuItem("Dress Up Game/Generate Necklace Colors")]
        public static void GenerateNecklaceColors()
        {
            AccessoryItem necklaceNone = EnsureNecklaceNoneItem();
            NecklaceColorLibrary library = LoadOrCreateLibrary();
            Dictionary<int, NecklaceLayout> layouts = CollectNecklaceLayouts();
            Dictionary<int, Sprite> shapeSprites = LoadNecklaceShapeSprites();

            if (shapeSprites.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Necklace Sprites",
                    "Add necklace_1.png, necklace_2.png, … under Assets/Sprites/Character/Necklace first.",
                    "OK");
                return;
            }

            NecklaceColorLibrary.ColorEntry[] palette = library.Colors;
            List<AccessoryItem> generatedItems = new List<AccessoryItem>();
            List<int> shapeIndices = new List<int>(shapeSprites.Keys);
            shapeIndices.Sort();

            foreach (int shapeIndex in shapeIndices)
            {
                Sprite shapeSprite = shapeSprites[shapeIndex];
                NecklaceLayout layout = layouts.TryGetValue(shapeIndex, out NecklaceLayout found)
                    ? found
                    : NecklaceLayout.Default;

                string originalId = $"necklace_{shapeIndex}";
                AccessoryItem originalItem = LoadOrCreate<AccessoryItem>($"{AccessoryFolder}/{originalId}.asset");
                originalItem.SetRuntimeData(
                    originalId,
                    $"Necklace {shapeIndex} Original",
                    shapeSprite,
                    false,
                    Color.white);
                originalItem.SetLayerLayout(layout.Offset, layout.Scale);
                EditorUtility.SetDirty(originalItem);
                generatedItems.Add(originalItem);

                for (int variantIndex = 0; variantIndex < palette.Length; variantIndex++)
                {
                    int colorIndex = (shapeIndex - 1 + variantIndex) % palette.Length;
                    NecklaceColorLibrary.ColorEntry colorEntry = palette[colorIndex];
                    string suffix = palette[variantIndex].suffix;
                    string id = $"necklace_{shapeIndex}_{suffix}";
                    string path = $"{AccessoryFolder}/{id}.asset";
                    AccessoryItem item = LoadOrCreate<AccessoryItem>(path);
                    item.SetRuntimeData(
                        id,
                        $"Necklace {shapeIndex} {colorEntry.displayName}",
                        shapeSprite,
                        false,
                        colorEntry.color);
                    item.SetLayerLayout(layout.Offset, layout.Scale);
                    EditorUtility.SetDirty(item);
                    generatedItems.Add(item);
                }
            }

            UpdateCatalog(necklaceNone, generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} necklace items (original + color variants, + necklace_none).");
        }

        private static Dictionary<int, Sprite> LoadNecklaceShapeSprites()
        {
            Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
            if (!Directory.Exists(NecklaceSpriteFolder))
            {
                return sprites;
            }

            foreach (string pngPath in Directory.GetFiles(NecklaceSpriteFolder, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(pngPath);
                Match match = Regex.Match(fileName, @"^necklace_(\d+)$", RegexOptions.IgnoreCase);
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

        private static Dictionary<int, NecklaceLayout> CollectNecklaceLayouts()
        {
            Dictionary<int, NecklaceLayout> layouts = new Dictionary<int, NecklaceLayout>();
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                return layouts;
            }

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null || !item.IsNecklaceVariant)
                {
                    continue;
                }

                Match match = Regex.Match(item.Id, @"^necklace_(\d+)(?:_\d+)?$");
                if (!match.Success)
                {
                    continue;
                }

                int shapeIndex = int.Parse(match.Groups[1].Value);
                if (!layouts.ContainsKey(shapeIndex))
                {
                    layouts[shapeIndex] = new NecklaceLayout(item.LayerOffset, item.LayerScale);
                }
            }

            return layouts;
        }

        private static void UpdateCatalog(AccessoryItem necklaceNone, List<AccessoryItem> necklaceItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<AccessoryItem> rebuilt = new List<AccessoryItem> { necklaceNone };
            rebuilt.AddRange(necklaceItems);

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item != null && (item.IsEarringNone || item.IsEarringVariant))
                {
                    rebuilt.Add(item);
                }
            }

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.IsNecklaceNone || item.IsNecklaceVariant || item.Id == "accessory_necklace"
                    || item.IsEarringNone || item.IsEarringVariant)
                {
                    continue;
                }

                rebuilt.Add(item);
            }

            catalog.SetAccessoryItems(rebuilt);
            EditorUtility.SetDirty(catalog);
        }

        private static AccessoryItem EnsureNecklaceNoneItem()
        {
            AccessoryItem necklaceNone = LoadOrCreate<AccessoryItem>($"{AccessoryFolder}/necklace_none.asset");
            necklaceNone.SetRuntimeData("necklace_none", "None", null, true);
            EditorUtility.SetDirty(necklaceNone);
            return necklaceNone;
        }

        private static NecklaceColorLibrary LoadOrCreateLibrary()
        {
            NecklaceColorLibrary library = AssetDatabase.LoadAssetAtPath<NecklaceColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<NecklaceColorLibrary>();
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

        private readonly struct NecklaceLayout
        {
            public static NecklaceLayout Default => new NecklaceLayout(
                new Vector3(0f, 1.85f, 0f),
                new Vector3(0.35f, 0.35f, 1f));

            public Vector3 Offset { get; }
            public Vector3 Scale { get; }

            public NecklaceLayout(Vector3 offset, Vector3 scale)
            {
                Offset = offset;
                Scale = scale;
            }
        }
    }
}
#endif
