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
    /// Generates 10 color variants for each bag shape (bag_1.png, bag_2.png, bag_3.png).
    /// </summary>
    public static class GenerateBagColorsTool
    {
        private const string LibraryPath = "Assets/Resources/BagColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string AccessoryFolder = "Assets/Resources/Items/Accessories";
        private const string BagSpriteFolder = "Assets/Sprites/Character/Bags";

        public static void GenerateBagColors()
        {
            EnsureBagNoneItem();
            BagColorLibrary library = LoadOrCreateLibrary();
            Dictionary<int, BagLayout> layouts = CollectBagLayouts();
            Dictionary<int, Sprite> shapeSprites = LoadBagShapeSprites();

            if (shapeSprites.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Bag Sprites",
                    "Add bag_1.png, bag_2.png, and bag_3.png under Assets/Sprites/Character/Bags first.",
                    "OK");
                return;
            }

            List<AccessoryItem> generatedItems = new List<AccessoryItem>();
            List<int> shapeIndices = new List<int>(shapeSprites.Keys);
            shapeIndices.Sort();

            foreach (int shapeIndex in shapeIndices)
            {
                Sprite shapeSprite = shapeSprites[shapeIndex];
                BagLayout layout = layouts.TryGetValue(shapeIndex, out BagLayout found)
                    ? found
                    : BagLayout.Default;
                generatedItems.Add(EnsureBaseBagItem(shapeIndex, shapeSprite, layout));
            }

            foreach (int shapeIndex in shapeIndices)
            {
                Sprite shapeSprite = shapeSprites[shapeIndex];
                BagLayout layout = layouts.TryGetValue(shapeIndex, out BagLayout found)
                    ? found
                    : BagLayout.Default;

                foreach (BagColorLibrary.ColorEntry colorEntry in library.Colors)
                {
                    string id = $"bag_{shapeIndex}_{colorEntry.suffix}";
                    string path = $"{AccessoryFolder}/{id}.asset";
                    AccessoryItem item = LoadOrCreate<AccessoryItem>(path);
                    item.SetRuntimeData(
                        id,
                        $"Bag {shapeIndex} {colorEntry.displayName}",
                        shapeSprite,
                        false,
                        colorEntry.color);
                    item.SetLayerLayout(layout.Offset, layout.Scale);
                    EditorUtility.SetDirty(item);
                    generatedItems.Add(item);
                }
            }

            UpdateCatalog(generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} bag color items.");
        }

        private static Dictionary<int, Sprite> LoadBagShapeSprites()
        {
            Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
            if (!Directory.Exists(BagSpriteFolder))
            {
                return sprites;
            }

            foreach (string pngPath in Directory.GetFiles(BagSpriteFolder, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(pngPath);
                Match match = Regex.Match(fileName, @"^bag_(\d+)$", RegexOptions.IgnoreCase);
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

        private static Dictionary<int, BagLayout> CollectBagLayouts()
        {
            Dictionary<int, BagLayout> layouts = new Dictionary<int, BagLayout>();

            for (int shapeIndex = 1; shapeIndex <= 3; shapeIndex++)
            {
                AccessoryItem baseBag = AssetDatabase.LoadAssetAtPath<AccessoryItem>(
                    $"{AccessoryFolder}/bag_{shapeIndex}.asset");
                if (baseBag != null)
                {
                    layouts[shapeIndex] = new BagLayout(baseBag.LayerOffset, baseBag.LayerScale);
                }
            }

            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                return layouts;
            }

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null || !item.IsBagVariant)
                {
                    continue;
                }

                Match baseShape = Regex.Match(item.Id, @"^bag_(\d+)$");
                if (baseShape.Success)
                {
                    int shapeIndex = int.Parse(baseShape.Groups[1].Value);
                    layouts[shapeIndex] = new BagLayout(item.LayerOffset, item.LayerScale);
                    continue;
                }

                Match colorVariant = Regex.Match(item.Id, @"^bag_(\d+)_\d+$");
                if (colorVariant.Success)
                {
                    int shapeIndex = int.Parse(colorVariant.Groups[1].Value);
                    if (!layouts.ContainsKey(shapeIndex))
                    {
                        layouts[shapeIndex] = new BagLayout(item.LayerOffset, item.LayerScale);
                    }
                }
            }

            return layouts;
        }

        private static AccessoryItem EnsureBaseBagItem(int shapeIndex, Sprite shapeSprite, BagLayout layout)
        {
            string path = $"{AccessoryFolder}/bag_{shapeIndex}.asset";
            AccessoryItem item = LoadOrCreate<AccessoryItem>(path);
            item.SetRuntimeData($"bag_{shapeIndex}", $"Bag {shapeIndex}", shapeSprite, false, Color.white);
            item.SetLayerLayout(layout.Offset, layout.Scale);
            EditorUtility.SetDirty(item);
            return item;
        }

        private static void UpdateCatalog(List<AccessoryItem> bagColorItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<AccessoryItem> rebuilt = new List<AccessoryItem>();
            bool insertedBagColors = false;

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.IsBagVariant || item.Id == "accessory_handbag")
                {
                    continue;
                }

                rebuilt.Add(item);

                if (!insertedBagColors && item.IsBagNone)
                {
                    rebuilt.AddRange(bagColorItems);
                    insertedBagColors = true;
                }
            }

            if (!insertedBagColors)
            {
                AccessoryItem bagNone = LoadOrCreate<AccessoryItem>($"{AccessoryFolder}/bag_none.asset");
                rebuilt.Add(bagNone);
                rebuilt.AddRange(bagColorItems);
            }

            catalog.SetAccessoryItems(rebuilt);
            EditorUtility.SetDirty(catalog);
        }

        private static void EnsureBagNoneItem()
        {
            AccessoryItem bagNone = LoadOrCreate<AccessoryItem>($"{AccessoryFolder}/bag_none.asset");
            bagNone.SetRuntimeData("bag_none", "None", null, true);
            EditorUtility.SetDirty(bagNone);
        }

        private static BagColorLibrary LoadOrCreateLibrary()
        {
            BagColorLibrary library = AssetDatabase.LoadAssetAtPath<BagColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<BagColorLibrary>();
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

        private readonly struct BagLayout
        {
            public static BagLayout Default => new BagLayout(
                new Vector3(1.074f, -0.614f, 0.042f),
                new Vector3(0.18464005f, 0.21125372f, 1f));

            public Vector3 Offset { get; }
            public Vector3 Scale { get; }

            public BagLayout(Vector3 offset, Vector3 scale)
            {
                Offset = offset;
                Scale = scale;
            }
        }
    }
}
#endif
