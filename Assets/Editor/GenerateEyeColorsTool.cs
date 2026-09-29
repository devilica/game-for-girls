#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DressUpGame.Data;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Generates 60 eye color MakeupItem assets (30 light + 30 dark) from EyeColorLibrary.
    /// </summary>
    public static class GenerateEyeColorsTool
    {
        private const string LibraryPath = "Assets/Resources/EyeColorLibrary.asset";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string MakeupFolder = "Assets/Resources/Items/Makeup";
        private const string EyeSpritePath = "Assets/Sprites/Character/Eyes/eyes_2.png";

        private static readonly Vector3 DefaultOffset = new Vector3(0f, 2.01f, 0f);
        private static readonly Vector3 DefaultScale = new Vector3(0.13f, 0.13f, 0.13f);

        public static void GenerateEyeColors()
        {
            EnsureEyeNoneItem();
            EyeColorLibrary library = LoadOrCreateLibrary();
            library.EnsureDefaultColors();
            EditorUtility.SetDirty(library);
            Sprite eyeSprite = SpriteImportUtility.LoadSprite(EyeSpritePath);
            if (eyeSprite == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Eye Sprite",
                    "Could not find eyes_2.png at:\n" + EyeSpritePath,
                    "OK");
                return;
            }

            Vector3 offset = DefaultOffset;
            Vector3 scale = DefaultScale;
            CollectExistingLayout(ref offset, ref scale);

            MakeupItem originalItem = EnsureOriginalEyeItem(eyeSprite, offset, scale);
            List<MakeupItem> generatedItems = new List<MakeupItem> { originalItem };

            foreach (EyeColorLibrary.ColorEntry entry in library.Colors)
            {
                if (string.IsNullOrEmpty(entry.id))
                {
                    continue;
                }

                string path = $"{MakeupFolder}/{entry.id}.asset";
                MakeupItem item = LoadOrCreate<MakeupItem>(path);
                item.SetRuntimeData(entry.id, entry.displayName, eyeSprite, MakeupType.Eyes, entry.color, false);
                item.SetLayerLayout(offset, scale);
                EditorUtility.SetDirty(item);
                generatedItems.Add(item);
            }

            UpdateCatalog(generatedItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {generatedItems.Count} eye color items.");
        }

        private static void CollectExistingLayout(ref Vector3 offset, ref Vector3 scale)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                return;
            }

            foreach (MakeupItem item in catalog.MakeupItems)
            {
                if (item == null || item.MakeupType != MakeupType.Eyes || item.IsNoneOption)
                {
                    continue;
                }

                offset = item.LayerOffset;
                scale = item.LayerScale;
                return;
            }

            MakeupItem original = AssetDatabase.LoadAssetAtPath<MakeupItem>($"{MakeupFolder}/eyes_2_01.asset");
            if (original != null)
            {
                offset = original.LayerOffset;
                scale = original.LayerScale;
            }
        }

        private static void EnsureEyeNoneItem()
        {
            MakeupItem noneItem = LoadOrCreate<MakeupItem>($"{MakeupFolder}/eyes_none.asset");
            noneItem.SetRuntimeData("eyes_none", "None", null, MakeupType.Eyes, Color.white, true);
            EditorUtility.SetDirty(noneItem);
        }

        private static MakeupItem EnsureOriginalEyeItem(Sprite eyeSprite, Vector3 offset, Vector3 scale)
        {
            MakeupItem originalItem = LoadOrCreate<MakeupItem>($"{MakeupFolder}/eyes_2_01.asset");
            originalItem.SetRuntimeData("eyes_2_01", "Original", eyeSprite, MakeupType.Eyes, Color.white, false);
            originalItem.SetLayerLayout(offset, scale);
            EditorUtility.SetDirty(originalItem);
            return originalItem;
        }

        private static EyeColorLibrary LoadOrCreateLibrary()
        {
            EyeColorLibrary library = AssetDatabase.LoadAssetAtPath<EyeColorLibrary>(LibraryPath);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<EyeColorLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
            EditorUtility.SetDirty(library);
            return library;
        }

        private static void UpdateCatalog(List<MakeupItem> eyeColorItems)
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog.asset not found.");
                return;
            }

            List<MakeupItem> rebuilt = new List<MakeupItem>();
            bool insertedEyeColors = false;

            foreach (MakeupItem item in catalog.MakeupItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (IsEyeColorItem(item))
                {
                    continue;
                }

                rebuilt.Add(item);

                if (!insertedEyeColors && item.MakeupType == MakeupType.Eyes && item.IsNoneOption)
                {
                    rebuilt.AddRange(eyeColorItems);
                    insertedEyeColors = true;
                }
            }

            if (!insertedEyeColors)
            {
                MakeupItem noneItem = LoadOrCreate<MakeupItem>($"{MakeupFolder}/eyes_none.asset");
                rebuilt.Insert(0, noneItem);
                rebuilt.InsertRange(1, eyeColorItems);
            }

            catalog.SetMakeupItems(rebuilt);
            EditorUtility.SetDirty(catalog);
        }

        private static bool IsEyeColorItem(MakeupItem item)
        {
            if (item.MakeupType != MakeupType.Eyes || item.IsNoneOption)
            {
                return false;
            }

            return Regex.IsMatch(item.Id ?? string.Empty, @"^eyes_\d+_\d+$");
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
#endif
