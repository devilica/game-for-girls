#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private static readonly (string suffix, Color tint)[] PetColorVariants =
        {
            ("yellow", new Color(1f, 0.93f, 0.58f)),
            ("brown", new Color(0.72f, 0.52f, 0.38f)),
            ("orange", new Color(1f, 0.72f, 0.42f)),
            ("grey", new Color(0.78f, 0.78f, 0.78f))
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
                importer.spritePixelsPerUnit = 256;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                fixedCount++;
            }

            Debug.Log($"Dress Up Game: Fixed {fixedCount} pet PNG import(s) to Sprite.");
        }

        [MenuItem("Dress Up Game/Generate Pet Items")]
        public static void GeneratePetItems()
        {
            FixPetSpriteImports();
            EnsureFolders();
            List<PetItem> allItems = new List<PetItem>();
            List<PetItem> basePets = new List<PetItem>();

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

                    if (cat.category == PetCustomizationCategory.Pet)
                    {
                        basePets.Add(item);
                    }
                }
            }

            EnsurePetColorVariants(basePets, allItems);
            PropagateAccessoryLayoutsForColorVariants(allItems, basePets);
            SortCatalogItems(allItems);

            UpdateCatalog(allItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Dress Up Game: Generated {allItems.Count} pet catalog item(s) (includes color variants).");
        }

        private static void EnsurePetColorVariants(List<PetItem> basePets, List<PetItem> allItems)
        {
            string petsFolder = $"{ItemsRoot}/Pets";
            foreach (PetItem basePet in basePets)
            {
                if (basePet == null || !PetLayoutIds.IsBasePetId(basePet.Id))
                {
                    continue;
                }

                foreach ((string suffix, Color tint) in PetColorVariants)
                {
                    string variantId = PetLayoutIds.BuildColorVariantId(basePet.Id, suffix);
                    string assetPath = $"{petsFolder}/{variantId}.asset";
                    PetItem variant = LoadOrCreate<PetItem>(assetPath);
                    string variantName = $"{basePet.DisplayName} ({Capitalize(suffix)})";
                    variant.SetColorVariantFromBase(basePet, variantId, variantName, tint);
                    EditorUtility.SetDirty(variant);

                    if (!allItems.Contains(variant))
                    {
                        allItems.Add(variant);
                    }
                }
            }
        }

        private static void PropagateAccessoryLayoutsForColorVariants(List<PetItem> allItems, List<PetItem> basePets)
        {
            List<string> basePetIds = basePets
                .Where(p => p != null && PetLayoutIds.IsBasePetId(p.Id))
                .Select(p => p.Id)
                .ToList();

            foreach (PetItem item in allItems)
            {
                if (item == null || item.Category == PetCustomizationCategory.Pet)
                {
                    continue;
                }

                foreach (string basePetId in basePetIds)
                {
                    item.EnsureColorVariantPetLayouts(basePetId);
                }

                EditorUtility.SetDirty(item);
            }
        }

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

                return string.CompareOrdinal(a.Id, b.Id);
            });
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

            ParsePetSortKey(leftId, out int leftIndex, out int leftColorRank);
            ParsePetSortKey(rightId, out int rightIndex, out int rightColorRank);

            bool leftIsBase = leftColorRank == 0;
            bool rightIsBase = rightColorRank == 0;
            if (leftIsBase != rightIsBase)
            {
                return leftIsBase ? -1 : 1;
            }

            int indexCompare = leftIndex.CompareTo(rightIndex);
            return indexCompare != 0 ? indexCompare : leftColorRank.CompareTo(rightColorRank);
        }

        private static void ParsePetSortKey(string petId, out int petIndex, out int colorRank)
        {
            petIndex = 999;
            colorRank = 0;
            if (string.IsNullOrEmpty(petId) || petId == "pet_none")
            {
                return;
            }

            Match baseMatch = Regex.Match(petId, @"^pet_(\d+)$", RegexOptions.IgnoreCase);
            if (baseMatch.Success)
            {
                petIndex = int.Parse(baseMatch.Groups[1].Value);
                colorRank = 0;
                return;
            }

            Match variantMatch = Regex.Match(
                petId,
                @"^pet_(\d+)_(yellow|brown|orange|grey)$",
                RegexOptions.IgnoreCase);
            if (variantMatch.Success)
            {
                petIndex = int.Parse(variantMatch.Groups[1].Value);
                string color = variantMatch.Groups[2].Value.ToLowerInvariant();
                for (int i = 0; i < PetLayoutIds.ColorSuffixes.Length; i++)
                {
                    if (PetLayoutIds.ColorSuffixes[i] == color)
                    {
                        colorRank = i + 1;
                        return;
                    }
                }
            }
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
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
