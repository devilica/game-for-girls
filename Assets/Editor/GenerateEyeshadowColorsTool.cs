#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using DressUpGame.Data;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Generates 90 eyeshadow MakeupItem assets from EyeshadowColorLibrary.
    /// </summary>
    public static class GenerateEyeshadowColorsTool
    {
        private const string LibraryPath = "Assets/Resources/EyeshadowColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string MakeupFolder = "Assets/Resources/Items/Makeup";
        private const string EyeshadowSpritePath = "Assets/Sprites/Character/Makeup/eyeshadow_2.png";

        private static readonly Vector3 LayerOffset = new Vector3(-0.015f, 2.1375f, 0f);
        private static readonly Vector3 LayerScale = new Vector3(0.24443299f, 0.23374957f, 0.25124624f);

        public static void GenerateEyeshadowColors()
        {
            EyeshadowColorLibrary library = LoadOrCreateLibrary();
            Sprite eyeshadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(EyeshadowSpritePath);
            if (eyeshadowSprite == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Eyeshadow Sprite",
                    "Could not find eyeshadow shape sprite at:\n" + EyeshadowSpritePath,
                    "OK");
                return;
            }

            List<MakeupItem> generatedItems = new List<MakeupItem>();
            foreach (EyeshadowColorLibrary.ColorEntry entry in library.Colors)
            {
                if (string.IsNullOrEmpty(entry.id))
                {
                    continue;
                }

                string path = $"{MakeupFolder}/{entry.id}.asset";
                MakeupItem item = LoadOrCreate<MakeupItem>(path);
                item.SetRuntimeData(entry.id, entry.displayName, eyeshadowSprite, MakeupType.Eyeshadow, entry.color, false);
                item.SetLayerLayout(LayerOffset, LayerScale);
                EditorUtility.SetDirty(item);
                generatedItems.Add(item);
            }

            UpdateCatalog(generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} eyeshadow color items.");
        }

        private static EyeshadowColorLibrary LoadOrCreateLibrary()
        {
            EyeshadowColorLibrary library = AssetDatabase.LoadAssetAtPath<EyeshadowColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<EyeshadowColorLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
            EditorUtility.SetDirty(library);
            return library;
        }

        private static void UpdateCatalog(List<MakeupItem> eyeshadowItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<MakeupItem> rebuilt = new List<MakeupItem>();
            bool insertedEyeshadowColors = false;
            foreach (MakeupItem item in catalog.MakeupItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.MakeupType == MakeupType.Eyeshadow && !item.IsNoneOption)
                {
                    continue;
                }

                rebuilt.Add(item);

                if (!insertedEyeshadowColors && item.MakeupType == MakeupType.Eyeshadow && item.IsNoneOption)
                {
                    rebuilt.AddRange(eyeshadowItems);
                    insertedEyeshadowColors = true;
                }
            }

            if (!insertedEyeshadowColors)
            {
                rebuilt.AddRange(eyeshadowItems);
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
