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
    /// Generates original + 4 color variants per shoe shape (shoes_1.png, …).
    /// </summary>
    public static class GenerateShoesColorsTool
    {
        private const string LibraryPath = "Assets/Resources/ShoesColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string ShoeFolder = "Assets/Resources/Items/Shoes";
        private const string ShoesSpriteFolder = "Assets/Sprites/Character/Shoes";

        [MenuItem("Dress Up Game/Generate Shoes Colors")]
        public static void GenerateShoesColors()
        {
            ShoeItem shoesNone = EnsureShoesNoneItem();
            ShoesColorLibrary library = LoadOrCreateLibrary();
            Dictionary<int, ShoesLayout> layouts = CollectShoesLayouts();
            Dictionary<int, Sprite> shapeSprites = LoadShoesShapeSprites();

            if (shapeSprites.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Shoe Sprites",
                    "Add shoes_1.png, shoes_2.png, … under Assets/Sprites/Character/Shoes first.",
                    "OK");
                return;
            }

            ShoesColorLibrary.ColorEntry[] palette = library.Colors;
            List<ShoeItem> generatedItems = new List<ShoeItem>();
            List<int> shapeIndices = new List<int>(shapeSprites.Keys);
            shapeIndices.Sort();

            foreach (int shapeIndex in shapeIndices)
            {
                Sprite shapeSprite = shapeSprites[shapeIndex];
                ShoesLayout layout = layouts.TryGetValue(shapeIndex, out ShoesLayout found)
                    ? found
                    : ShoesLayout.Default;

                string originalId = $"shoes_{shapeIndex}";
                ShoeItem originalItem = LoadOrCreate<ShoeItem>($"{ShoeFolder}/{originalId}.asset");
                originalItem.SetRuntimeData(
                    originalId,
                    $"Shoes {shapeIndex} Original",
                    shapeSprite,
                    false,
                    Color.white);
                originalItem.SetLayerLayout(layout.Offset, layout.Scale);
                EditorUtility.SetDirty(originalItem);
                generatedItems.Add(originalItem);

                for (int variantIndex = 0; variantIndex < palette.Length; variantIndex++)
                {
                    int colorIndex = (shapeIndex - 1 + variantIndex) % palette.Length;
                    ShoesColorLibrary.ColorEntry colorEntry = palette[colorIndex];
                    string suffix = palette[variantIndex].suffix;
                    string id = $"shoes_{shapeIndex}_{suffix}";
                    string path = $"{ShoeFolder}/{id}.asset";
                    ShoeItem item = LoadOrCreate<ShoeItem>(path);
                    item.SetRuntimeData(
                        id,
                        $"Shoes {shapeIndex} {colorEntry.displayName}",
                        shapeSprite,
                        false,
                        colorEntry.color);
                    item.SetLayerLayout(layout.Offset, layout.Scale);
                    EditorUtility.SetDirty(item);
                    generatedItems.Add(item);
                }
            }

            UpdateCatalog(shoesNone, generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} shoe items (original + color variants, + shoes_none).");
        }

        private static Dictionary<int, Sprite> LoadShoesShapeSprites()
        {
            Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
            if (!Directory.Exists(ShoesSpriteFolder))
            {
                return sprites;
            }

            foreach (string pngPath in Directory.GetFiles(ShoesSpriteFolder, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(pngPath);
                Match match = Regex.Match(fileName, @"^shoes_(\d+)$", RegexOptions.IgnoreCase);
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

        private static Dictionary<int, ShoesLayout> CollectShoesLayouts()
        {
            Dictionary<int, ShoesLayout> layouts = new Dictionary<int, ShoesLayout>();
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                return layouts;
            }

            foreach (ShoeItem item in catalog.ShoeItems)
            {
                if (item == null || !item.IsShoesVariant)
                {
                    continue;
                }

                Match match = Regex.Match(item.Id, @"^shoes_(\d+)(?:_\d+)?$");
                if (!match.Success)
                {
                    continue;
                }

                int shapeIndex = int.Parse(match.Groups[1].Value);
                if (!layouts.ContainsKey(shapeIndex))
                {
                    layouts[shapeIndex] = new ShoesLayout(item.LayerOffset, item.LayerScale);
                }
            }

            return layouts;
        }

        private static void UpdateCatalog(ShoeItem shoesNone, List<ShoeItem> shoeItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<ShoeItem> rebuilt = new List<ShoeItem> { shoesNone };
            rebuilt.AddRange(shoeItems);

            foreach (ShoeItem item in catalog.ShoeItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.IsShoesNone || item.IsShoesVariant)
                {
                    continue;
                }

                rebuilt.Add(item);
            }

            catalog.SetShoeItems(rebuilt);
            EditorUtility.SetDirty(catalog);
        }

        private static ShoeItem EnsureShoesNoneItem()
        {
            ShoeItem shoesNone = LoadOrCreate<ShoeItem>($"{ShoeFolder}/shoes_none.asset");
            shoesNone.SetRuntimeData("shoes_none", "None", null, true);
            EditorUtility.SetDirty(shoesNone);
            return shoesNone;
        }

        private static ShoesColorLibrary LoadOrCreateLibrary()
        {
            ShoesColorLibrary library = AssetDatabase.LoadAssetAtPath<ShoesColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<ShoesColorLibrary>();
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

        private readonly struct ShoesLayout
        {
            public static ShoesLayout Default => new ShoesLayout(
                new Vector3(0f, -0.35f, 0f),
                new Vector3(0.4f, 0.4f, 1f));

            public Vector3 Offset { get; }
            public Vector3 Scale { get; }

            public ShoesLayout(Vector3 offset, Vector3 scale)
            {
                Offset = offset;
                Scale = scale;
            }
        }
    }
}
#endif
