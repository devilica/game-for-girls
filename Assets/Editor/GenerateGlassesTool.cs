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
    /// Generates glasses_1 plus glasses_none. Use Generate Glasses 2 Colors for shape 2 variants.
    /// </summary>
    public static class GenerateGlassesTool
    {
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string AccessoryFolder = "Assets/Resources/Items/Accessories";
        private const string GlassesSpriteFolder = "Assets/Sprites/Character/Glasses";

        public static void GenerateGlassesItems()
        {
            Dictionary<int, Sprite> shapeSprites = LoadGlassesShapeSprites();
            if (shapeSprites.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Glasses Sprites",
                    "Add glasses_1.png and glasses_2.png under Assets/Sprites/Character/Glasses first.",
                    "OK");
                return;
            }

            AccessoryItem noneItem = LoadOrCreate<AccessoryItem>($"{AccessoryFolder}/glasses_none.asset");
            noneItem.SetRuntimeData("glasses_none", "None", null, true);
            EditorUtility.SetDirty(noneItem);

            List<AccessoryItem> generatedItems = new List<AccessoryItem> { noneItem };
            List<int> shapeIndices = new List<int>(shapeSprites.Keys);
            shapeIndices.Sort();

            if (shapeSprites.TryGetValue(1, out Sprite glasses1Sprite))
            {
                string path = $"{AccessoryFolder}/glasses_1.asset";
                AccessoryItem item = LoadOrCreate<AccessoryItem>(path);
                item.SetRuntimeData("glasses_1", "Glasses 1", glasses1Sprite, false, Color.white);
                item.SetLayerLayout(GlassesLayout.Default.Offset, GlassesLayout.Default.Scale);
                EditorUtility.SetDirty(item);
                generatedItems.Add(item);
            }

            UpdateCatalog(generatedItems);
            GenerateGlassesColorsTool.GenerateGlasses2Colors();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} glasses items.");
        }

        private static Dictionary<int, Sprite> LoadGlassesShapeSprites()
        {
            Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
            if (!Directory.Exists(GlassesSpriteFolder))
            {
                return sprites;
            }

            foreach (string pngPath in Directory.GetFiles(GlassesSpriteFolder, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(pngPath);
                Match match = Regex.Match(fileName, @"^glasses_(\d+)$", RegexOptions.IgnoreCase);
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

        private static void UpdateCatalog(List<AccessoryItem> glassesItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<AccessoryItem> rebuilt = new List<AccessoryItem>();
            bool insertedGlasses = false;

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.IsGlassesNone
                    || item.IsGlassesVariant
                    || item.Id == "accessory_glasses"
                    || item.Id == "glasses_2"
                    || Regex.IsMatch(item.Id ?? string.Empty, @"^glasses_2_\d+$"))
                {
                    continue;
                }

                rebuilt.Add(item);

                if (!insertedGlasses && item.IsCrownVariant && item.Id == "crown_2_20")
                {
                    rebuilt.AddRange(glassesItems);
                    insertedGlasses = true;
                }
            }

            if (!insertedGlasses)
            {
                int crownEndIndex = rebuilt.FindLastIndex(entry => entry != null && entry.IsCrownVariant);
                if (crownEndIndex >= 0)
                {
                    rebuilt.InsertRange(crownEndIndex + 1, glassesItems);
                }
                else
                {
                    rebuilt.AddRange(glassesItems);
                }
            }

            catalog.SetAccessoryItems(rebuilt);
            EditorUtility.SetDirty(catalog);
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

        private readonly struct GlassesLayout
        {
            public static GlassesLayout Default => new GlassesLayout(
                new Vector3(0f, 1.85f, 0f),
                new Vector3(0.1f, 0.1f, 0.1f));

            public Vector3 Offset { get; }
            public Vector3 Scale { get; }

            public GlassesLayout(Vector3 offset, Vector3 scale)
            {
                Offset = offset;
                Scale = scale;
            }
        }
    }
}
#endif
