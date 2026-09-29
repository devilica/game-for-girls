#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DressUpGame.Character;
using DressUpGame.Data;
using DressUpGame.Pet;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Connects PNG files in Assets/Sprites to game item ScriptableObjects and base character layers.
    /// </summary>
    public static class SpriteAssetLinker
    {
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string SpritesRoot = "Assets/Sprites/Character";
        private const string PetSpritesRoot = "Assets/Sprites/Pet";

        [MenuItem("Dress Up Game/Link Sprites From Folder")]
        public static void ApplySpritesFromFolder()
        {
            if (!File.Exists(CatalogPath))
            {
                GameDataGenerator.GenerateAll();
            }

            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("Dress Up Game: GameCatalog not found. Run Full Project Setup first.");
                return;
            }

            int linked = 0;
            linked += LinkHairItems(catalog);
            linked += LinkDressItems(catalog);
            linked += LinkShoeItems(catalog);
            linked += LinkMakeupItems(catalog);
            linked += LinkEyeItems(catalog);
            linked += LinkAccessoryItems(catalog);
            linked += LinkPetItems();
            linked += LinkBaseCharacterLayers();

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Dress Up Game: Applied {linked} sprite link(s) from Assets/Sprites/Character.");
        }

        private static int LinkHairItems(GameCatalog catalog)
        {
            List<HairItem> hairItems = SyncHairItemsFromFolder(catalog);
            catalog.SetHairItems(hairItems);

            int count = 0;
            foreach (HairItem item in hairItems)
            {
                if (item == null)
                {
                    continue;
                }

                string path = $"{SpritesRoot}/Hair/{item.Id}.png";
                Sprite sprite = SpriteImportUtility.LoadSprite(path);
                if (sprite == null)
                {
                    continue;
                }

                item.SetRuntimeData(item.Id, item.DisplayName, sprite);
                EditorUtility.SetDirty(item);
                count++;
            }

            return count;
        }

        /// <summary>
        /// Creates HairItem assets and catalog entries for any hair_*.png not yet registered.
        /// </summary>
        private static List<HairItem> SyncHairItemsFromFolder(GameCatalog catalog)
        {
            string spriteFolder = $"{SpritesRoot}/Hair";
            Dictionary<int, HairItem> itemsByIndex = new Dictionary<int, HairItem>();

            foreach (HairItem item in catalog.HairItems)
            {
                if (item == null || !TryParseHairIndex(item.Id, out int index))
                {
                    continue;
                }

                itemsByIndex[index] = item;
            }

            HairItem templateItem = GetHairLayoutTemplate(catalog);
            if (!Directory.Exists(spriteFolder))
            {
                return SortHairItems(itemsByIndex);
            }

            int created = 0;
            foreach (string pngPath in Directory.GetFiles(spriteFolder, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(pngPath);
                if (!TryParseHairIndex(fileName, out int index))
                {
                    continue;
                }

                if (itemsByIndex.ContainsKey(index))
                {
                    continue;
                }

                string hairId = $"hair_{index}";
                string assetPath = $"Assets/Resources/Items/Hair/Hair_{index}.asset";
                HairItem item = LoadOrCreateAsset<HairItem>(assetPath);
                item.SetRuntimeData(hairId, $"Hair {index}", null);

                if (templateItem != null)
                {
                    item.SetLayerLayout(templateItem.LayerOffset, templateItem.LayerScale);
                }

                EditorUtility.SetDirty(item);
                itemsByIndex[index] = item;
                created++;
            }

            if (created > 0)
            {
                Debug.Log($"Dress Up Game: Created {created} new hair item(s) from PNG files.");
            }

            return SortHairItems(itemsByIndex);
        }

        private static HairItem GetHairLayoutTemplate(GameCatalog catalog)
        {
            foreach (HairItem item in catalog.HairItems)
            {
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }

        private static List<HairItem> SortHairItems(Dictionary<int, HairItem> itemsByIndex)
        {
            List<int> indices = new List<int>(itemsByIndex.Keys);
            indices.Sort();
            List<HairItem> sorted = new List<HairItem>();
            foreach (int index in indices)
            {
                sorted.Add(itemsByIndex[index]);
            }

            return sorted;
        }

        private static bool TryParseHairIndex(string value, out int index)
        {
            index = 0;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            Match match = Regex.Match(value, @"^hair_(\d+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return false;
            }

            return int.TryParse(match.Groups[1].Value, out index) && index > 0;
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
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

        private static int LinkDressItems(GameCatalog catalog)
        {
            int count = 0;
            foreach (DressItem item in catalog.DressItems)
            {
                if (item == null)
                {
                    continue;
                }

                string path = ResolveDressSpritePath(item.Id);
                Sprite sprite = SpriteImportUtility.LoadSprite(path);
                if (sprite == null)
                {
                    continue;
                }

                item.SetRuntimeData(item.Id, item.DisplayName, sprite, item.TintColor);
                EditorUtility.SetDirty(item);
                count++;
            }

            return count;
        }

        private static string ResolveDressSpritePath(string itemId)
        {
            Match colorVariant = Regex.Match(itemId, @"^dress_(\d+)_\d+$", RegexOptions.IgnoreCase);
            if (colorVariant.Success)
            {
                return $"{SpritesRoot}/Dresses/dress_{colorVariant.Groups[1].Value}.png";
            }

            return $"{SpritesRoot}/Dresses/{itemId}.png";
        }

        private static int LinkShoeItems(GameCatalog catalog)
        {
            int count = 0;
            foreach (ShoeItem item in catalog.ShoeItems)
            {
                if (item == null || item.IsShoesNone)
                {
                    continue;
                }

                string path = ResolveShoeSpritePath(item.Id);
                Sprite sprite = SpriteImportUtility.LoadSprite(path);
                if (sprite == null)
                {
                    continue;
                }

                item.SetRuntimeData(item.Id, item.DisplayName, sprite, false, item.TintColor);
                EditorUtility.SetDirty(item);
                count++;
            }

            return count;
        }

        private static string ResolveShoeSpritePath(string itemId)
        {
            Match colorVariant = Regex.Match(itemId, @"^shoes_(\d+)_\d+$", RegexOptions.IgnoreCase);
            if (colorVariant.Success)
            {
                return $"{SpritesRoot}/Shoes/shoes_{colorVariant.Groups[1].Value}.png";
            }

            Match shape = Regex.Match(itemId, @"^shoes_(\d+)$", RegexOptions.IgnoreCase);
            if (shape.Success)
            {
                return $"{SpritesRoot}/Shoes/shoes_{shape.Groups[1].Value}.png";
            }

            return $"{SpritesRoot}/Shoes/{itemId}.png";
        }

        private static int LinkEyeItems(GameCatalog catalog)
        {
            int count = 0;
            foreach (MakeupItem item in catalog.GetMakeupItems(MakeupType.Eyes))
            {
                if (item == null || item.IsNoneOption)
                {
                    continue;
                }

                string path = ResolveEyeSpritePath(item.Id);
                Sprite sprite = SpriteImportUtility.LoadSprite(path);
                if (sprite == null)
                {
                    continue;
                }

                item.SetRuntimeData(item.Id, item.DisplayName, sprite, MakeupType.Eyes, item.TintColor, false);
                EditorUtility.SetDirty(item);
                count++;
            }

            return count;
        }

        private static int LinkMakeupItems(GameCatalog catalog)
        {
            int count = 0;
            foreach (MakeupItem item in catalog.MakeupItems)
            {
                if (item == null || item.IsNoneOption || item.MakeupType == MakeupType.Eyes)
                {
                    continue;
                }

                string path = ResolveMakeupSpritePath(item.Id);
                Sprite sprite = SpriteImportUtility.LoadSprite(path);
                if (sprite == null)
                {
                    continue;
                }

                item.SetRuntimeData(item.Id, item.DisplayName, sprite, item.MakeupType, item.TintColor, false);
                EditorUtility.SetDirty(item);
                count++;
            }

            return count;
        }

        private static string ResolveEyeSpritePath(string itemId)
        {
            Match colorVariant = Regex.Match(itemId ?? string.Empty, @"^eyes_(\d+)_\d+$", RegexOptions.IgnoreCase);
            if (colorVariant.Success)
            {
                return $"{SpritesRoot}/Eyes/eyes_{colorVariant.Groups[1].Value}.png";
            }

            return $"{SpritesRoot}/Eyes/{itemId}.png";
        }

        private static string ResolveMakeupSpritePath(string itemId)
        {
            string makeupFolder = $"{SpritesRoot}/Makeup";
            string standardPath = $"{makeupFolder}/{itemId}.png";
            if (File.Exists(standardPath))
            {
                return standardPath;
            }

            if (itemId.StartsWith("lipstick_"))
            {
                Match colorVariant = Regex.Match(itemId, @"^lipstick_(\d+)_\d+$", RegexOptions.IgnoreCase);
                if (colorVariant.Success)
                {
                    string shapeIndex = colorVariant.Groups[1].Value;
                    string typoPath = $"{makeupFolder}/lipstic_{shapeIndex}.png";
                    if (File.Exists(typoPath))
                    {
                        return typoPath;
                    }

                    return $"{makeupFolder}/lipstick_{shapeIndex}.png";
                }

                string suffix = itemId.Substring("lipstick_".Length);
                string legacyTypoPath = $"{makeupFolder}/lipstic_{suffix}.png";
                if (File.Exists(legacyTypoPath))
                {
                    return legacyTypoPath;
                }

                if (suffix == "1" && File.Exists($"{makeupFolder}/lipstic.png"))
                {
                    return $"{makeupFolder}/lipstic.png";
                }
            }

            return standardPath;
        }

        private static int LinkAccessoryItems(GameCatalog catalog)
        {
            int count = 0;
            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null || item.IsNoneOption)
                {
                    continue;
                }

                string path = ResolveAccessorySpritePath(item.Id);
                Sprite sprite = SpriteImportUtility.LoadSprite(path);
                if (sprite == null)
                {
                    continue;
                }

                item.SetRuntimeData(item.Id, item.DisplayName, sprite, false, item.TintColor);
                EditorUtility.SetDirty(item);
                count++;
            }

            return count;
        }

        private static string ResolveAccessorySpritePath(string itemId)
        {
            Match necklaceVariant = Regex.Match(itemId, @"^necklace_(\d+)_\d+$", RegexOptions.IgnoreCase);
            if (necklaceVariant.Success)
            {
                return $"{SpritesRoot}/Necklace/necklace_{necklaceVariant.Groups[1].Value}.png";
            }

            Match necklaceShape = Regex.Match(itemId, @"^necklace_(\d+)$", RegexOptions.IgnoreCase);
            if (necklaceShape.Success)
            {
                return $"{SpritesRoot}/Necklace/necklace_{necklaceShape.Groups[1].Value}.png";
            }

            Match earVariant = Regex.Match(itemId, @"^ear_(\d+)_\d+$", RegexOptions.IgnoreCase);
            if (earVariant.Success)
            {
                return $"{SpritesRoot}/Earrings/ear_{earVariant.Groups[1].Value}.png";
            }

            Match earShape = Regex.Match(itemId, @"^ear_(\d+)$", RegexOptions.IgnoreCase);
            if (earShape.Success)
            {
                return $"{SpritesRoot}/Earrings/ear_{earShape.Groups[1].Value}.png";
            }

            Match crownVariant = Regex.Match(itemId, @"^crown_(\d+)_\d+$", RegexOptions.IgnoreCase);
            if (crownVariant.Success)
            {
                return $"{SpritesRoot}/Crown/crown_{crownVariant.Groups[1].Value}.png";
            }

            Match crownShape = Regex.Match(itemId, @"^crown_(\d+)$", RegexOptions.IgnoreCase);
            if (crownShape.Success)
            {
                return $"{SpritesRoot}/Crown/crown_{crownShape.Groups[1].Value}.png";
            }

            Match glassesColorVariant = Regex.Match(itemId, @"^glasses_(\d+)_\d+$", RegexOptions.IgnoreCase);
            if (glassesColorVariant.Success)
            {
                return $"{SpritesRoot}/Glasses/glasses_{glassesColorVariant.Groups[1].Value}.png";
            }

            Match glassesMatch = Regex.Match(itemId, @"^glasses_(\d+)$", RegexOptions.IgnoreCase);
            if (glassesMatch.Success)
            {
                return $"{SpritesRoot}/Glasses/glasses_{glassesMatch.Groups[1].Value}.png";
            }

            Match bagColorVariant = Regex.Match(itemId, @"^bag_(\d+)_\d+$", RegexOptions.IgnoreCase);
            if (bagColorVariant.Success)
            {
                return $"{SpritesRoot}/Bags/bag_{bagColorVariant.Groups[1].Value}.png";
            }

            Match bagMatch = Regex.Match(itemId, @"^bag_(\d+)$", RegexOptions.IgnoreCase);
            if (bagMatch.Success)
            {
                return $"{SpritesRoot}/Bags/bag_{bagMatch.Groups[1].Value}.png";
            }

            if (itemId == "accessory_crown")
            {
                string crownOnePath = $"{SpritesRoot}/Crown/crown_1.png";
                if (File.Exists(crownOnePath))
                {
                    return crownOnePath;
                }
            }

            return $"{SpritesRoot}/Accessories/{itemId}.png";
        }

        private static int LinkBaseCharacterLayers()
        {
            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                return 0;
            }

            int count = 0;
            SerializedObject so = new SerializedObject(customizer);

            count += AssignLayerSprite(so, "bodyRenderer", $"{SpritesRoot}/body.png");

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(customizer);

            if (customizer.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            }

            return count;
        }

        private static int LinkPetItems()
        {
            const string petCatalogPath = "Assets/Resources/PetCatalog.asset";
            PetCatalog catalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(petCatalogPath);
            if (catalog == null)
            {
                return 0;
            }

            int count = 0;
            foreach (PetItem item in catalog.Items)
            {
                if (item == null || item.IsNoneOption)
                {
                    continue;
                }

                string path = ResolvePetSpritePath(item.Id);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                Sprite sprite = SpriteImportUtility.LoadSprite(path);
                if (sprite == null)
                {
                    continue;
                }

                item.SetRuntimeData(item.Id, item.DisplayName, sprite, false);
                EditorUtility.SetDirty(item);
                count++;
            }

            EditorUtility.SetDirty(catalog);
            return count;
        }

        private static string ResolvePetSpritePath(string itemId)
        {
            Match pet = Regex.Match(itemId, @"^pet_(\d+)$", RegexOptions.IgnoreCase);
            if (pet.Success)
            {
                return $"{PetSpritesRoot}/Pets/pet_{pet.Groups[1].Value}.png";
            }

            Match bow = Regex.Match(itemId, @"^bow_(\d+)$", RegexOptions.IgnoreCase);
            if (bow.Success)
            {
                return $"{PetSpritesRoot}/Hairbows/bow_{bow.Groups[1].Value}.png";
            }

            Match collar = Regex.Match(itemId, @"^collar_(\d+)$", RegexOptions.IgnoreCase);
            if (collar.Success)
            {
                return $"{PetSpritesRoot}/Collar/collar_{collar.Groups[1].Value}.png";
            }

            Match glasses = Regex.Match(itemId, @"^glasses_(\d+)$", RegexOptions.IgnoreCase);
            if (glasses.Success)
            {
                return $"{PetSpritesRoot}/Glasses/glasses_{glasses.Groups[1].Value}.png";
            }

            return null;
        }

        private static int AssignLayerSprite(SerializedObject customizerSo, string propertyName, string spritePath)
        {
            if (!File.Exists(spritePath))
            {
                return 0;
            }

            Sprite sprite = SpriteImportUtility.LoadSprite(spritePath);
            SerializedProperty rendererProp = customizerSo.FindProperty(propertyName);
            if (rendererProp?.objectReferenceValue is SpriteRenderer renderer && sprite != null)
            {
                renderer.sprite = sprite;
                renderer.enabled = true;
                EditorUtility.SetDirty(renderer);
                return 1;
            }

            return 0;
        }
    }
}
#endif
