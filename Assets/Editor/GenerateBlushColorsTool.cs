#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using DressUpGame.Data;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Generates 21 blush MakeupItem assets from BlushColorLibrary.
    /// </summary>
    public static class GenerateBlushColorsTool
    {
        private const string LibraryPath = "Assets/Resources/BlushColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string MakeupFolder = "Assets/Resources/Items/Makeup";
        private const string BlushSpritePath = "Assets/Sprites/Character/Makeup/blush_2.png";

        private static readonly Vector3 LayerOffset = new Vector3(-0.0096f, 1.776f, 0f);
        private static readonly Vector3 LayerScale = new Vector3(0.18507038f, 0.14534803f, 1f);

        public static void GenerateBlushColors()
        {
            BlushColorLibrary library = LoadOrCreateLibrary();
            Sprite blushSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BlushSpritePath);
            if (blushSprite == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Blush Sprite",
                    "Could not find blush shape sprite at:\n" + BlushSpritePath,
                    "OK");
                return;
            }

            List<MakeupItem> generatedItems = new List<MakeupItem>();
            foreach (BlushColorLibrary.ColorEntry entry in library.Colors)
            {
                if (string.IsNullOrEmpty(entry.id))
                {
                    continue;
                }

                string path = $"{MakeupFolder}/{entry.id}.asset";
                MakeupItem item = LoadOrCreate<MakeupItem>(path);
                item.SetRuntimeData(entry.id, entry.displayName, blushSprite, MakeupType.Blush, entry.color, false);
                item.SetLayerLayout(LayerOffset, LayerScale);
                EditorUtility.SetDirty(item);
                generatedItems.Add(item);
            }

            UpdateCatalog(generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} blush color items.");
        }

        private static BlushColorLibrary LoadOrCreateLibrary()
        {
            BlushColorLibrary library = AssetDatabase.LoadAssetAtPath<BlushColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<BlushColorLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
            EditorUtility.SetDirty(library);
            return library;
        }

        private static void UpdateCatalog(List<MakeupItem> blushItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<MakeupItem> rebuilt = new List<MakeupItem>();
            bool insertedBlushColors = false;
            foreach (MakeupItem item in catalog.MakeupItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.MakeupType == MakeupType.Blush && !item.IsNoneOption)
                {
                    continue;
                }

                rebuilt.Add(item);

                if (!insertedBlushColors && item.MakeupType == MakeupType.Blush && item.IsNoneOption)
                {
                    rebuilt.AddRange(blushItems);
                    insertedBlushColors = true;
                }
            }

            if (!insertedBlushColors)
            {
                rebuilt.AddRange(blushItems);
            }

            catalog.SetMakeupItems(rebuilt);
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
    }
}
#endif
