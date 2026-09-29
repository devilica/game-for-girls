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
    /// Generates bag_none plus bag_1..bag_N from Assets/Sprites/Character/Bags/.
    /// </summary>
    public static class GenerateBagTool
    {
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string AccessoryFolder = "Assets/Resources/Items/Accessories";
        private const string BagSpriteFolder = "Assets/Sprites/Character/Bags";

        [MenuItem("Dress Up Game/Generate Bag Items")]
        public static void GenerateBagItems()
        {
            Dictionary<int, Sprite> bagSprites = LoadBagSprites();
            if (bagSprites.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Bag Sprites",
                    "Add bag_1.png, bag_2.png, etc. under Assets/Sprites/Character/Bags first.",
                    "OK");
                return;
            }

            AccessoryItem noneItem = LoadOrCreate<AccessoryItem>($"{AccessoryFolder}/bag_none.asset");
            noneItem.SetRuntimeData("bag_none", "None", null, true);
            EditorUtility.SetDirty(noneItem);

            List<AccessoryItem> generatedItems = new List<AccessoryItem> { noneItem };
            List<int> bagIndices = new List<int>(bagSprites.Keys);
            bagIndices.Sort();

            foreach (int bagIndex in bagIndices)
            {
                string path = $"{AccessoryFolder}/bag_{bagIndex}.asset";
                AccessoryItem item = LoadOrCreate<AccessoryItem>(path);
                item.SetRuntimeData($"bag_{bagIndex}", $"Bag {bagIndex}", bagSprites[bagIndex], false, Color.white);
                item.SetLayerLayout(BagLayout.Default.Offset, BagLayout.Default.Scale);
                EditorUtility.SetDirty(item);
                generatedItems.Add(item);
            }

            UpdateCatalog(generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} bag items.");
        }

        private static Dictionary<int, Sprite> LoadBagSprites()
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

                int bagIndex = int.Parse(match.Groups[1].Value);
                Sprite sprite = SpriteImportUtility.LoadSprite(pngPath);
                if (sprite != null)
                {
                    sprites[bagIndex] = sprite;
                }
            }

            return sprites;
        }

        private static void UpdateCatalog(List<AccessoryItem> bagItems)
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
                if (item == null)
                {
                    continue;
                }

                if (item.IsBagNone || item.IsBagVariant || item.Id == "accessory_handbag")
                {
                    continue;
                }

                rebuilt.Add(item);
            }

            rebuilt.AddRange(bagItems);
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

        private readonly struct BagLayout
        {
            public static BagLayout Default => new BagLayout(
                new Vector3(0.45f, 0.55f, 0f),
                new Vector3(0.12f, 0.12f, 0.12f));

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
