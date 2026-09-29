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
    /// Generates color variants for each dress shape PNG in Assets/Sprites/Character/Dresses.
    /// </summary>
    public static class GenerateDressColorsTool
    {
        private const string LibraryPath = "Assets/Resources/DressColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string DressDataFolder = "Assets/Resources/Items/Dresses";
        private const string DressSpriteFolder = "Assets/Sprites/Character/Dresses";

        [MenuItem("Dress Up Game/Generate Dress Colors")]
        public static void GenerateDressColors()
        {
            DressColorLibrary library = LoadOrCreateLibrary();
            Dictionary<int, DressLayout> layouts = CollectDressLayouts();
            Dictionary<int, Sprite> shapeSprites = LoadDressShapeSprites();

            if (shapeSprites.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Dress Sprites",
                    "Add dress_1.png through dress_N.png under Assets/Sprites/Character/Dresses first.",
                    "OK");
                return;
            }

            List<DressItem> generatedItems = new List<DressItem>();
            List<int> shapeIndices = new List<int>(shapeSprites.Keys);
            shapeIndices.Sort();

            foreach (int shapeIndex in shapeIndices)
            {
                Sprite shapeSprite = shapeSprites[shapeIndex];
                DressLayout layout = layouts.TryGetValue(shapeIndex, out DressLayout found)
                    ? found
                    : DressLayout.Default;

                if (GenerateDress11ColorSpritesTool.IsDressShapeWithRecoloredVariants(shapeIndex))
                {
                    var dress11Layout = new GenerateDress11ColorSpritesTool.DressLayout(layout.Offset, layout.Scale);
                    GenerateDress11ColorSpritesTool.EnsureVariantTexturesAndItems(dress11Layout, shapeSprite);
                    generatedItems.AddRange(GenerateDress11ColorSpritesTool.LoadDress11ItemsForCatalog());
                    continue;
                }

                string originalId = $"dress_{shapeIndex}";
                string originalAssetName = $"Dress_{shapeIndex}";
                DressItem originalItem = LoadOrCreate<DressItem>($"{DressDataFolder}/{originalAssetName}.asset");
                originalItem.SetRuntimeData(originalId, $"Dress {shapeIndex} Original", shapeSprite, Color.white);
                originalItem.SetLayerLayout(layout.Offset, layout.Scale);
                EditorUtility.SetDirty(originalItem);
                generatedItems.Add(originalItem);

                foreach (DressColorLibrary.ColorEntry colorEntry in library.Colors)
                {
                    string id = $"dress_{shapeIndex}_{colorEntry.suffix}";
                    string assetName = $"Dress_{shapeIndex}_{colorEntry.suffix}";
                    string path = $"{DressDataFolder}/{assetName}.asset";
                    DressItem item = LoadOrCreate<DressItem>(path);
                    item.SetRuntimeData(
                        id,
                        $"Dress {shapeIndex} {colorEntry.displayName}",
                        shapeSprite,
                        colorEntry.color);
                    item.SetLayerLayout(layout.Offset, layout.Scale);
                    EditorUtility.SetDirty(item);
                    generatedItems.Add(item);
                }
            }

            UpdateCatalog(generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} dress items (original + color variants).");
        }

        private static Dictionary<int, Sprite> LoadDressShapeSprites()
        {
            Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
            if (!Directory.Exists(DressSpriteFolder))
            {
                return sprites;
            }

            foreach (string pngPath in Directory.GetFiles(DressSpriteFolder, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(pngPath);
                Match match = Regex.Match(fileName, @"^dress_(\d+)$", RegexOptions.IgnoreCase);
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

        private static Dictionary<int, DressLayout> CollectDressLayouts()
        {
            Dictionary<int, DressLayout> layouts = new Dictionary<int, DressLayout>();
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                return layouts;
            }

            foreach (DressItem item in catalog.DressItems)
            {
                if (item == null)
                {
                    continue;
                }

                Match match = Regex.Match(item.Id, @"^dress_(\d+)$");
                if (!match.Success)
                {
                    continue;
                }

                int shapeIndex = int.Parse(match.Groups[1].Value);
                if (!layouts.ContainsKey(shapeIndex))
                {
                    layouts[shapeIndex] = new DressLayout(item.LayerOffset, item.LayerScale);
                }
            }

            return layouts;
        }

        private static void UpdateCatalog(List<DressItem> dressItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<MakeupItem> makeupItems = new List<MakeupItem>(catalog.MakeupItems);
            List<AccessoryItem> accessoryItems = new List<AccessoryItem>(catalog.AccessoryItems);
            List<HairItem> hairItems = new List<HairItem>(catalog.HairItems);

            catalog.SetHairItems(hairItems);
            catalog.SetDressItems(dressItems);
            catalog.SetMakeupItems(makeupItems);
            catalog.SetAccessoryItems(accessoryItems);
            EditorUtility.SetDirty(catalog);
        }

        private static DressColorLibrary LoadOrCreateLibrary()
        {
            DressColorLibrary library = AssetDatabase.LoadAssetAtPath<DressColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<DressColorLibrary>();
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

        private readonly struct DressLayout
        {
            public static DressLayout Default => new DressLayout(Vector3.zero, Vector3.one);

            public Vector3 Offset { get; }
            public Vector3 Scale { get; }

            public DressLayout(Vector3 offset, Vector3 scale)
            {
                Offset = offset;
                Scale = scale;
            }
        }
    }
}
#endif
