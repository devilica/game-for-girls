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
    /// Generates 20 color variants for each crown shape (crown_1.png, crown_2.png).
    /// </summary>
    public static class GenerateCrownColorsTool
    {
        private const string LibraryPath = "Assets/Resources/CrownColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string AccessoryFolder = "Assets/Resources/Items/Accessories";
        private const string CrownSpriteFolder = "Assets/Sprites/Character/Crown";

        public static void GenerateCrownColors()
        {
            EnsureCrownNoneItem();
            CrownColorLibrary library = LoadOrCreateLibrary();
            Dictionary<int, CrownLayout> layouts = CollectCrownLayouts();
            Dictionary<int, Sprite> shapeSprites = LoadCrownShapeSprites();

            if (shapeSprites.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Crown Sprites",
                    "Add crown_1.png and crown_2.png under Assets/Sprites/Character/Crown first.",
                    "OK");
                return;
            }

            List<AccessoryItem> generatedItems = new List<AccessoryItem>();
            List<int> shapeIndices = new List<int>(shapeSprites.Keys);
            shapeIndices.Sort();

            foreach (int shapeIndex in shapeIndices)
            {
                Sprite shapeSprite = shapeSprites[shapeIndex];
                CrownLayout layout = layouts.TryGetValue(shapeIndex, out CrownLayout found)
                    ? found
                    : CrownLayout.Default;

                foreach (CrownColorLibrary.ColorEntry colorEntry in library.Colors)
                {
                    string id = $"crown_{shapeIndex}_{colorEntry.suffix}";
                    string path = $"{AccessoryFolder}/{id}.asset";
                    AccessoryItem item = LoadOrCreate<AccessoryItem>(path);
                    item.SetRuntimeData(
                        id,
                        $"Crown {shapeIndex} {colorEntry.displayName}",
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
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} crown color items.");
        }

        private static Dictionary<int, Sprite> LoadCrownShapeSprites()
        {
            Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
            if (!Directory.Exists(CrownSpriteFolder))
            {
                return sprites;
            }

            foreach (string pngPath in Directory.GetFiles(CrownSpriteFolder, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(pngPath);
                Match match = Regex.Match(fileName, @"^crown_(\d+)$", RegexOptions.IgnoreCase);
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

        private static Dictionary<int, CrownLayout> CollectCrownLayouts()
        {
            Dictionary<int, CrownLayout> layouts = new Dictionary<int, CrownLayout>();
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                return layouts;
            }

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null || !item.IsCrownVariant)
                {
                    continue;
                }

                Match match = Regex.Match(item.Id, @"^crown_(\d+)_\d+$");
                if (!match.Success)
                {
                    continue;
                }

                int shapeIndex = int.Parse(match.Groups[1].Value);
                if (!layouts.ContainsKey(shapeIndex))
                {
                    layouts[shapeIndex] = new CrownLayout(item.LayerOffset, item.LayerScale);
                }
            }

            return layouts;
        }

        private static void UpdateCatalog(List<AccessoryItem> crownItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<AccessoryItem> rebuilt = new List<AccessoryItem>();
            bool insertedCrownColors = false;

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.IsCrownVariant || item.Id == "accessory_crown")
                {
                    continue;
                }

                rebuilt.Add(item);

                if (!insertedCrownColors && item.IsCrownNone)
                {
                    rebuilt.AddRange(crownItems);
                    insertedCrownColors = true;
                }
            }

            if (!insertedCrownColors)
            {
                rebuilt.InsertRange(0, crownItems);
            }

            catalog.SetAccessoryItems(rebuilt);
            EditorUtility.SetDirty(catalog);
        }

        private static void EnsureCrownNoneItem()
        {
            AccessoryItem crownNone = LoadOrCreate<AccessoryItem>($"{AccessoryFolder}/crown_none.asset");
            crownNone.SetRuntimeData("crown_none", "None", null, true);
            EditorUtility.SetDirty(crownNone);
        }

        private static CrownColorLibrary LoadOrCreateLibrary()
        {
            CrownColorLibrary library = AssetDatabase.LoadAssetAtPath<CrownColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<CrownColorLibrary>();
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

        private readonly struct CrownLayout
        {
            public static CrownLayout Default => new CrownLayout(
                new Vector3(-0.008f, 2.906f, 0f),
                new Vector3(0.24370669f, 0.1833684f, 0.12f));

            public Vector3 Offset { get; }
            public Vector3 Scale { get; }

            public CrownLayout(Vector3 offset, Vector3 scale)
            {
                Offset = offset;
                Scale = scale;
            }
        }
    }
}
#endif
