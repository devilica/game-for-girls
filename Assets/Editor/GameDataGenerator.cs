#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using DressUpGame.Data;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Creates placeholder sprites and ScriptableObject item assets.
    /// </summary>
    public static class GameDataGenerator
    {
        private const string ResourcesPath = "Assets/Resources";
        private const string CatalogPath = ResourcesPath + "/GameCatalog.asset";
        private const string HairColorLibraryPath = ResourcesPath + "/HairColorLibrary.asset";

        private const string CharacterSpritesRoot = "Assets/Sprites/Character";
        private const string HairSpritesRoot = "Assets/Sprites/Character/Hair";
        private const string DressSpritesRoot = "Assets/Sprites/Character/Dresses";
        private const string MakeupSpritesRoot = "Assets/Sprites/Character/Makeup";
        private const string AccessorySpritesRoot = "Assets/Sprites/Character/Accessories";
        private const string DataRoot = "Assets/Resources/Items";

        public static void GenerateAll()
        {
            EnsureFolders();
            GenerateBaseCharacterSprites();
            HairColorLibrary colorLibrary = GenerateHairColorLibrary();
            List<HairItem> hairItems = GenerateHairItems();
            List<DressItem> dressItems = GenerateDressItems();
            List<MakeupItem> makeupItems = GenerateMakeupItems();
            List<AccessoryItem> accessoryItems = GenerateAccessoryItems();
            GenerateCatalog(colorLibrary, hairItems, dressItems, makeupItems, accessoryItems);
            GenerateEyeshadowColorsTool.GenerateEyeshadowColors();
            GenerateBlushColorsTool.GenerateBlushColors();
            GenerateDressColorsTool.GenerateDressColors();
            GenerateLipstickColorsTool.GenerateLipstickColors();
            GenerateNecklaceColorsTool.GenerateNecklaceColors();
            GenerateEarringColorsTool.GenerateEarringColors();
            GenerateShoesColorsTool.GenerateShoesColors();
            GeneratePetItemsTool.GeneratePetItems();
            GenerateCrownColorsTool.GenerateCrownColors();
            GenerateGlassesTool.GenerateGlassesItems();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SpriteAssetLinker.ApplySpritesFromFolder();
            Debug.Log("Dress Up Game: Placeholder assets generated successfully.");
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/Scripts", "Assets/Editor", "Assets/Scenes", "Assets/Prefabs",
                ResourcesPath, DataRoot,
                DataRoot + "/Shoes",
                CharacterSpritesRoot, HairSpritesRoot, DressSpritesRoot,
                MakeupSpritesRoot, AccessorySpritesRoot
            };

            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }

        private static void GenerateBaseCharacterSprites()
        {
            PlaceholderSpriteFactory.CreateAndSaveSprite($"{CharacterSpritesRoot}/body.png", new Color(1f, 0.86f, 0.78f), 220, 420);
            PlaceholderSpriteFactory.CreateAndSaveSprite($"{CharacterSpritesRoot}/eyes.png", new Color(0.25f, 0.45f, 0.85f), 120, 40);
        }

        private static HairColorLibrary GenerateHairColorLibrary()
        {
            HairColorLibrary library = AssetDatabase.LoadAssetAtPath<HairColorLibrary>(HairColorLibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<HairColorLibrary>();
                AssetDatabase.CreateAsset(library, HairColorLibraryPath);
            }

            return library;
        }

        private static List<HairItem> GenerateHairItems()
        {
            List<HairItem> items = new List<HairItem>();
            Color hairBase = new Color(0.45f, 0.28f, 0.16f);

            for (int i = 1; i <= 5; i++)
            {
                string path = $"{DataRoot}/Hair/Hair_{i}.asset";
                EnsureParentFolder(path);
                HairItem item = LoadOrCreate<HairItem>(path);
                Sprite sprite = PlaceholderSpriteFactory.CreateHairSprite($"{HairSpritesRoot}/hair_{i}.png", i, hairBase);
                item.SetRuntimeData($"hair_{i}", $"Hair {i}", sprite);
                EditorUtility.SetDirty(item);
                items.Add(item);
            }

            return items;
        }

        private static List<DressItem> GenerateDressItems()
        {
            List<DressItem> items = new List<DressItem>();
            Color[] dressColors =
            {
                new Color(0.95f, 0.45f, 0.65f),
                new Color(0.45f, 0.75f, 0.95f),
                new Color(0.75f, 0.55f, 0.95f),
                new Color(0.95f, 0.75f, 0.35f),
                new Color(0.55f, 0.95f, 0.75f),
                new Color(0.95f, 0.55f, 0.45f),
                new Color(0.65f, 0.65f, 0.95f),
                new Color(0.85f, 0.45f, 0.85f),
                new Color(0.45f, 0.85f, 0.85f),
                new Color(0.95f, 0.85f, 0.55f)
            };

            for (int i = 1; i <= 10; i++)
            {
                string path = $"{DataRoot}/Dresses/Dress_{i}.asset";
                EnsureParentFolder(path);
                DressItem item = LoadOrCreate<DressItem>(path);
                Sprite sprite = PlaceholderSpriteFactory.CreateAndSaveSprite(
                    $"{DressSpritesRoot}/dress_{i}.png",
                    dressColors[i - 1],
                    200,
                    220,
                    i.ToString());
                item.SetRuntimeData($"dress_{i}", $"Dress {i}", sprite);
                EditorUtility.SetDirty(item);
                items.Add(item);
            }

            return items;
        }

        private static List<MakeupItem> GenerateMakeupItems()
        {
            List<MakeupItem> items = new List<MakeupItem>();
            GenerateMakeupSet(items, MakeupType.Lipstick, "lipstick", new Color(0.85f, 0.2f, 0.35f));
            GenerateMakeupSet(items, MakeupType.Eyeshadow, "eyeshadow", new Color(0.55f, 0.35f, 0.85f));
            GenerateMakeupSet(items, MakeupType.Blush, "blush", new Color(0.95f, 0.55f, 0.65f));
            return items;
        }

        private static void GenerateMakeupSet(List<MakeupItem> items, MakeupType type, string prefix, Color baseColor)
        {
            string nonePath = $"{DataRoot}/Makeup/{prefix}_none.asset";
            EnsureParentFolder(nonePath);
            MakeupItem noneItem = LoadOrCreate<MakeupItem>(nonePath);
            noneItem.SetRuntimeData($"{prefix}_none", "None", null, type, Color.white, true);
            EditorUtility.SetDirty(noneItem);
            items.Add(noneItem);

            for (int i = 1; i <= 5; i++)
            {
                string path = $"{DataRoot}/Makeup/{prefix}_{i}.asset";
                EnsureParentFolder(path);
                MakeupItem item = LoadOrCreate<MakeupItem>(path);
                Color tint = Color.Lerp(baseColor, Color.white, (i - 1) * 0.12f);
                Sprite sprite = PlaceholderSpriteFactory.CreateAndSaveSprite(
                    $"{MakeupSpritesRoot}/{prefix}_{i}.png",
                    tint,
                    type == MakeupType.Lipstick ? 60 : 100,
                    type == MakeupType.Lipstick ? 30 : 40);
                item.SetRuntimeData($"{prefix}_{i}", $"{char.ToUpper(prefix[0])}{prefix.Substring(1)} {i}", sprite, type, tint, false);
                EditorUtility.SetDirty(item);
                items.Add(item);
            }
        }

        private static List<AccessoryItem> GenerateAccessoryItems()
        {
            List<AccessoryItem> items = new List<AccessoryItem>();

            string nonePath = $"{DataRoot}/Accessories/accessory_none.asset";
            EnsureParentFolder(nonePath);
            AccessoryItem none = LoadOrCreate<AccessoryItem>(nonePath);
            none.SetRuntimeData("accessory_none", "None", null, true);
            EditorUtility.SetDirty(none);
            items.Add(none);

            (string id, string name, Color color, Vector2Int size)[] accessories =
            {
                ("accessory_crown", "Crown", new Color(0.95f, 0.85f, 0.25f), new Vector2Int(120, 50)),
                ("accessory_necklace", "Necklace", new Color(0.85f, 0.75f, 0.35f), new Vector2Int(90, 30)),
                ("accessory_earrings", "Earrings", new Color(0.75f, 0.85f, 0.95f), new Vector2Int(100, 40)),
                ("accessory_glasses", "Glasses", new Color(0.2f, 0.2f, 0.2f), new Vector2Int(110, 35)),
                ("accessory_handbag", "Handbag", new Color(0.85f, 0.35f, 0.55f), new Vector2Int(70, 80))
            };

            foreach ((string id, string name, Color color, Vector2Int size) entry in accessories)
            {
                string path = $"{DataRoot}/Accessories/{entry.id}.asset";
                EnsureParentFolder(path);
                AccessoryItem item = LoadOrCreate<AccessoryItem>(path);
                Sprite sprite = PlaceholderSpriteFactory.CreateAndSaveSprite(
                    $"{AccessorySpritesRoot}/{entry.id}.png",
                    entry.color,
                    entry.size.x,
                    entry.size.y);
                item.SetRuntimeData(entry.id, entry.name, sprite, false);
                EditorUtility.SetDirty(item);
                items.Add(item);
            }

            return items;
        }

        private static void GenerateCatalog(
            HairColorLibrary colorLibrary,
            List<HairItem> hairItems,
            List<DressItem> dressItems,
            List<MakeupItem> makeupItems,
            List<AccessoryItem> accessoryItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<GameCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.SetHairColorLibrary(colorLibrary);
            catalog.SetHairItems(hairItems);
            catalog.SetDressItems(dressItems);
            catalog.SetMakeupItems(makeupItems);
            catalog.SetAccessoryItems(accessoryItems);
            EditorUtility.SetDirty(catalog);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            EnsureParentFolder(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureParentFolder(string assetPath)
        {
            string directory = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
#endif
