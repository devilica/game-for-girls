#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DressUpGame.Pet;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class GeneratePetItemsTool
    {
        private const string CatalogPath = "Assets/Resources/PetCatalog.asset";
        private const string ItemsRoot = "Assets/Resources/Items/Pet";

        private static readonly (string folder, string pattern, string noneId, string noneName, PetCustomizationCategory category)[] Categories =
        {
            ("Pets", @"^pet_(\d+)$", "pet_none", "None", PetCustomizationCategory.Pet),
            ("Hairbows", @"^bow_(\d+)$", "bow_none", "None", PetCustomizationCategory.Hairbow),
            ("Collar", @"^collar_(\d+)$", "collar_none", "None", PetCustomizationCategory.Collar),
            ("Glasses", @"^glasses_(\d+)$", "glasses_none", "None", PetCustomizationCategory.Glasses)
        };

        [MenuItem("Dress Up Game/Fix Pet Sprite Imports")]
        public static void FixPetSpriteImports()
        {
            int fixedCount = 0;
            string root = "Assets/Sprites/Pet";
            if (!Directory.Exists(root))
            {
                return;
            }

            foreach (string pngPath in Directory.GetFiles(root, "*.png", SearchOption.AllDirectories))
            {
                string assetPath = pngPath.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null || importer.textureType == TextureImporterType.Sprite)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit =
                    PetVariantSpriteImportUtility.GetDefaultPixelsPerUnitForPetSpritePath(assetPath);
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                fixedCount++;
            }

            Debug.Log($"Dress Up Game: Fixed {fixedCount} pet PNG import(s) to Sprite.");
        }

        [MenuItem("Dress Up Game/Sync Pet Catalog From Items")]
        public static void SyncPetCatalogFromItems()
        {
            List<PetItem> allItems = CollectAllPetItemAssets();
            SortCatalogItems(allItems);
            UpdateCatalog(allItems);
            AssetDatabase.SaveAssets();
            Debug.Log($"Dress Up Game: Pet catalog synced from {allItems.Count} item asset(s).");
        }

        [MenuItem("Dress Up Game/Generate Pet Items")]
        public static void GeneratePetItems()
        {
            FixPetSpriteImports();
            EnsureFolders();
            List<PetItem> allItems = new List<PetItem>();

            foreach (var cat in Categories)
            {
                if (cat.category != PetCustomizationCategory.Pet)
                {
                    PetItem none = EnsureNone(cat.noneId, cat.noneName, cat.category, $"{ItemsRoot}/{cat.folder}");
                    allItems.Add(none);
                }

                string spriteFolder = $"Assets/Sprites/Pet/{cat.folder}";
                if (!Directory.Exists(spriteFolder))
                {
                    continue;
                }

                List<int> indices = new List<int>();
                foreach (string pngPath in Directory.GetFiles(spriteFolder, "*.png"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(pngPath);
                    Match match = Regex.Match(fileName, cat.pattern, RegexOptions.IgnoreCase);
                    if (!match.Success)
                    {
                        continue;
                    }

                    indices.Add(int.Parse(match.Groups[1].Value));
                }

                indices.Sort();
                foreach (int index in indices)
                {
                    string id = fileNameForCategory(cat.category, index);
                    string assetPath = $"{ItemsRoot}/{cat.folder}/{id}.asset";
                    PetItem item = LoadOrCreate<PetItem>(assetPath);
                    Sprite sprite = SpriteImportUtility.LoadSprite($"{spriteFolder}/{SpriteFileName(cat.category, index)}.png");
                    item.SetEditorData(id, DisplayName(cat.category, index), cat.category, sprite, false);
                    EditorUtility.SetDirty(item);
                    allItems.Add(item);
                }

                if (cat.category != PetCustomizationCategory.Pet)
                {
                    AppendExistingCategoryAssets(allItems, cat.folder, cat.pattern, cat.noneId);
                }

                if (cat.category == PetCustomizationCategory.Pet)
                {
                    CollectPetRecolorVariants(allItems, spriteFolder);
                    AppendExistingCategoryAssets(allItems, cat.folder, @"^pet_", "pet_none");
                }
            }

            PropagateBow6LayoutsToVariants(allItems);
            PropagateCollarVariantLayouts(allItems);

            SortCatalogItems(allItems);

            UpdateCatalog(allItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {allItems.Count} pet catalog item(s).");
        }

        private static void CollectPetRecolorVariants(List<PetItem> allItems, string spriteFolder)
        {
            foreach (string basePetId in PetRecolorPalette.RecolorBasePetIds)
            {
                if (!PetRecolorPalette.SupportsRecolor(basePetId))
                {
                    continue;
                }

                PetItem basePet = FindPetItem(allItems, basePetId);
                if (basePet == null)
                {
                    continue;
                }

                int petNumber = int.Parse(basePetId.Substring(4));
                foreach (string suffix in PetRecolorPalette.ColorSuffixes)
                {
                    if (PetRecolorPalette.IsVariantSkipped(basePetId, suffix))
                    {
                        continue;
                    }

                    string variantId = PetLayoutIds.BuildColorVariantId(basePetId, suffix);
                    string pngPath = $"{spriteFolder}/{variantId}.png";
                    if (!File.Exists(pngPath))
                    {
                        continue;
                    }

                    string assetPath = $"{ItemsRoot}/Pets/{variantId}.asset";
                    PetItem item = LoadOrCreate<PetItem>(assetPath);
                    Sprite sprite = SpriteImportUtility.LoadSprite(pngPath);
                    item.SetRecolorVariantFromBase(
                        basePet,
                        variantId,
                        PetVariantDisplayName(petNumber, suffix),
                        sprite);
                    EditorUtility.SetDirty(item);
                    allItems.Add(item);
                }
            }

            PropagateRecolorAccessoryLayouts(allItems);
        }

        private static List<PetItem> CollectAllPetItemAssets()
        {
            List<PetItem> allItems = new List<PetItem>();
            string[] guids = AssetDatabase.FindAssets("t:PetItem", new[] { ItemsRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PetItem item = AssetDatabase.LoadAssetAtPath<PetItem>(path);
                if (item != null)
                {
                    allItems.Add(item);
                }
            }

            return allItems;
        }

        private static void AppendExistingCategoryAssets(
            List<PetItem> allItems,
            string folderName,
            string idPattern,
            string noneId)
        {
            string folder = $"{ItemsRoot}/{folderName}";
            if (!Directory.Exists(folder))
            {
                return;
            }

            Regex regex = new Regex(idPattern, RegexOptions.IgnoreCase);
            foreach (string assetPath in Directory.GetFiles(folder, "*.asset"))
            {
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (string.Equals(fileName, noneId, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!regex.IsMatch(fileName))
                {
                    continue;
                }

                if (FindPetItem(allItems, fileName) != null)
                {
                    continue;
                }

                string unityPath = assetPath.Replace('\\', '/');
                PetItem item = AssetDatabase.LoadAssetAtPath<PetItem>(unityPath);
                if (item != null)
                {
                    allItems.Add(item);
                }
            }
        }

        private static PetItem FindPetItem(List<PetItem> allItems, string petId)
        {
            foreach (PetItem candidate in allItems)
            {
                if (candidate != null && candidate.Id == petId)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void PropagateBow6LayoutsToVariants(List<PetItem> allItems)
        {
            PetItem bow6 = FindPetItem(allItems, "bow_6");
            if (bow6 == null)
            {
                return;
            }

            for (int index = 8; index <= 20; index++)
            {
                PetItem target = FindPetItem(allItems, $"bow_{index}");
                if (target == null)
                {
                    continue;
                }

                target.CopyLayerLayoutsFrom(bow6);
                foreach (string basePetId in PetRecolorPalette.RecolorBasePetIds)
                {
                    target.EnsureColorVariantPetLayouts(basePetId);
                }

                EditorUtility.SetDirty(target);
            }
        }

        private static void PropagateCollarVariantLayouts(List<PetItem> allItems)
        {
            PetItem collar4 = FindPetItem(allItems, "collar_4");
            if (collar4 != null)
            {
                for (int index = 7; index <= 12; index++)
                {
                    ApplyCollarLayoutSource(allItems, collar4, index);
                }
            }

            PetItem collar2 = FindPetItem(allItems, "collar_2");
            if (collar2 != null)
            {
                for (int index = 13; index <= 17; index++)
                {
                    ApplyCollarLayoutSource(allItems, collar2, index);
                }
            }
        }

        private static void ApplyCollarLayoutSource(List<PetItem> allItems, PetItem source, int collarIndex)
        {
            PetItem target = FindPetItem(allItems, $"collar_{collarIndex}");
            if (target == null)
            {
                return;
            }

            target.CopyLayerLayoutsFrom(source);
            foreach (string basePetId in PetRecolorPalette.RecolorBasePetIds)
            {
                target.EnsureColorVariantPetLayouts(basePetId);
            }

            EditorUtility.SetDirty(target);
        }

        private static void PropagateRecolorAccessoryLayouts(List<PetItem> allItems)
        {
            foreach (PetItem item in allItems)
            {
                if (item == null || item.Category == PetCustomizationCategory.Pet)
                {
                    continue;
                }

                foreach (string basePetId in PetRecolorPalette.RecolorBasePetIds)
                {
                    item.EnsureColorVariantPetLayouts(basePetId);
                }

                EditorUtility.SetDirty(item);
            }
        }

        private static string PetVariantDisplayName(int petNumber, string suffix) =>
            suffix switch
            {
                "dark_yellow" => $"Pet {petNumber} Dark Yellow",
                "dark_orange" => $"Pet {petNumber} Dark Orange",
                "grey" => $"Pet {petNumber} Grey",
                "black" => $"Pet {petNumber} Black",
                "brown" => $"Pet {petNumber} Brown",
                "red" => $"Pet {petNumber} Red",
                "green" => $"Pet {petNumber} Green",
                _ => $"Pet {petNumber} {suffix}"
            };

        private static void SortCatalogItems(List<PetItem> items)
        {
            items.Sort((a, b) =>
            {
                if (a == null && b == null)
                {
                    return 0;
                }

                if (a == null)
                {
                    return 1;
                }

                if (b == null)
                {
                    return -1;
                }

                int categoryOrder = a.Category.CompareTo(b.Category);
                if (categoryOrder != 0)
                {
                    return categoryOrder;
                }

                if (a.Category == PetCustomizationCategory.Pet)
                {
                    return ComparePetIds(a.Id, b.Id);
                }

                int noneOrder = CompareNoneFirst(a, b);
                if (noneOrder != 0)
                {
                    return noneOrder;
                }

                return PetAccessorySort.CompareNumberedIds(a.Category, a.Id, b.Id);
            });
        }

        private static int CompareNoneFirst(PetItem left, PetItem right)
        {
            bool leftNone = left.IsNoneOption;
            bool rightNone = right.IsNoneOption;
            if (leftNone == rightNone)
            {
                return 0;
            }

            return leftNone ? -1 : 1;
        }

        private static int ComparePetIds(string leftId, string rightId)
        {
            if (leftId == "pet_none")
            {
                return -1;
            }

            if (rightId == "pet_none")
            {
                return 1;
            }

            bool leftBase = PetLayoutIds.IsBasePetId(leftId);
            bool rightBase = PetLayoutIds.IsBasePetId(rightId);
            if (leftBase != rightBase)
            {
                return leftBase ? -1 : 1;
            }

            ParsePetSortKey(leftId, out int leftIndex, out int leftColorRank);
            ParsePetSortKey(rightId, out int rightIndex, out int rightColorRank);
            int indexCompare = leftIndex.CompareTo(rightIndex);
            return indexCompare != 0 ? indexCompare : leftColorRank.CompareTo(rightColorRank);
        }

        private static void ParsePetSortKey(string petId, out int petIndex, out int colorRank)
        {
            petIndex = 999;
            colorRank = 0;
            if (string.IsNullOrEmpty(petId))
            {
                return;
            }

            if (PetLayoutIds.IsBasePetId(petId))
            {
                petIndex = int.Parse(petId.Substring(4));
                return;
            }

            if (!PetLayoutIds.IsColorVariantPetId(petId))
            {
                return;
            }

            string baseId = PetLayoutIds.ResolveBasePetId(petId);
            if (PetLayoutIds.IsBasePetId(baseId))
            {
                petIndex = int.Parse(baseId.Substring(4));
            }

            string suffix = petId.Substring(baseId.Length + 1).ToLowerInvariant();
            for (int i = 0; i < PetRecolorPalette.ColorSuffixes.Length; i++)
            {
                if (PetRecolorPalette.ColorSuffixes[i] == suffix)
                {
                    colorRank = i + 1;
                    return;
                }
            }
        }

        private static string fileNameForCategory(PetCustomizationCategory category, int index)
        {
            return category switch
            {
                PetCustomizationCategory.Pet => $"pet_{index}",
                PetCustomizationCategory.Hairbow => $"bow_{index}",
                PetCustomizationCategory.Collar => $"collar_{index}",
                PetCustomizationCategory.Glasses => $"glasses_{index}",
                _ => $"item_{index}"
            };
        }

        private static string SpriteFileName(PetCustomizationCategory category, int index)
        {
            return fileNameForCategory(category, index);
        }

        private static string DisplayName(PetCustomizationCategory category, int index)
        {
            return category switch
            {
                PetCustomizationCategory.Pet => $"Pet {index}",
                PetCustomizationCategory.Hairbow => $"Bow {index}",
                PetCustomizationCategory.Collar => $"Collar {index}",
                PetCustomizationCategory.Glasses => $"Glasses {index}",
                _ => $"Item {index}"
            };
        }

        private static PetItem EnsureNone(string id, string name, PetCustomizationCategory category, string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            PetItem none = LoadOrCreate<PetItem>($"{folder}/{id}.asset");
            none.SetEditorData(id, name, category, null, true);
            EditorUtility.SetDirty(none);
            return none;
        }

        private static void UpdateCatalog(List<PetItem> items)
        {
            PetCatalog catalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<PetCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.SetItems(items);
            EditorUtility.SetDirty(catalog);
        }

        private static void EnsureFolders()
        {
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }

            if (!Directory.Exists(ItemsRoot))
            {
                Directory.CreateDirectory(ItemsRoot);
            }

            foreach (var cat in Categories)
            {
                string path = $"{ItemsRoot}/{cat.folder}";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
#endif
