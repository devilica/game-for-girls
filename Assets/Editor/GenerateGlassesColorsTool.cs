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
    /// Generates 33 color variants for glasses shape 2 (glasses_2.png).
    /// </summary>
    public static class GenerateGlassesColorsTool
    {
        private const string LibraryPath = "Assets/Resources/GlassesColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string AccessoryFolder = "Assets/Resources/Items/Accessories";
        private const string GlassesSpriteFolder = "Assets/Sprites/Character/Glasses";
        private const int ShapeIndex = 2;

        public static void GenerateGlasses2Colors()
        {
            GlassesColorLibrary library = LoadOrCreateLibrary();
            GlassesLayout layout = CollectGlasses2Layout();
            Sprite shapeSprite = LoadGlasses2Sprite();

            if (shapeSprite == null)
            {
                EditorUtility.DisplayDialog(
                    "No Glasses Sprite",
                    "Add glasses_2.png under Assets/Sprites/Character/Glasses first.",
                    "OK");
                return;
            }

            List<AccessoryItem> generatedItems = new List<AccessoryItem>();

            foreach (GlassesColorLibrary.ColorEntry colorEntry in library.Colors)
            {
                string id = $"glasses_{ShapeIndex}_{colorEntry.suffix}";
                string path = $"{AccessoryFolder}/{id}.asset";
                AccessoryItem item = LoadOrCreate<AccessoryItem>(path);
                item.SetRuntimeData(
                    id,
                    $"Glasses 2 {colorEntry.displayName}",
                    shapeSprite,
                    false,
                    colorEntry.color);
                item.SetLayerLayout(layout.Offset, layout.Scale);
                EditorUtility.SetDirty(item);
                generatedItems.Add(item);
            }

            UpdateCatalog(generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} glasses 2 color items.");
        }

        private static Sprite LoadGlasses2Sprite()
        {
            string path = $"{GlassesSpriteFolder}/glasses_{ShapeIndex}.png";
            return File.Exists(path) ? SpriteImportUtility.LoadSprite(path) : null;
        }

        private static GlassesLayout CollectGlasses2Layout()
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                return GlassesLayout.Default;
            }

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.Id == "glasses_2" || item.Id == "glasses_2_01")
                {
                    return new GlassesLayout(item.LayerOffset, item.LayerScale);
                }
            }

            AccessoryItem legacy = AssetDatabase.LoadAssetAtPath<AccessoryItem>($"{AccessoryFolder}/glasses_2.asset");
            if (legacy != null)
            {
                return new GlassesLayout(legacy.LayerOffset, legacy.LayerScale);
            }

            return GlassesLayout.Default;
        }

        private static void UpdateCatalog(List<AccessoryItem> colorItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<AccessoryItem> rebuilt = new List<AccessoryItem>();
            bool insertedColors = false;

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.Id == "glasses_2" || IsGlasses2ColorVariant(item.Id))
                {
                    continue;
                }

                rebuilt.Add(item);

                if (!insertedColors && item.Id == "glasses_1")
                {
                    rebuilt.AddRange(colorItems);
                    insertedColors = true;
                }
            }

            if (!insertedColors)
            {
                int insertIndex = rebuilt.FindIndex(entry => entry != null && entry.IsGlassesNone);
                if (insertIndex >= 0)
                {
                    rebuilt.InsertRange(insertIndex + 1, colorItems);
                }
                else
                {
                    rebuilt.AddRange(colorItems);
                }
            }

            catalog.SetAccessoryItems(rebuilt);
            EditorUtility.SetDirty(catalog);
        }

        private static bool IsGlasses2ColorVariant(string id)
        {
            return Regex.IsMatch(id ?? string.Empty, @"^glasses_2_\d+$");
        }

        private static GlassesColorLibrary LoadOrCreateLibrary()
        {
            GlassesColorLibrary library = AssetDatabase.LoadAssetAtPath<GlassesColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<GlassesColorLibrary>();
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

        private readonly struct GlassesLayout
        {
            public static GlassesLayout Default => new GlassesLayout(
                new Vector3(-0.018f, 1.99f, 0f),
                new Vector3(0.23348244f, 0.17900285f, 0.1f));

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
