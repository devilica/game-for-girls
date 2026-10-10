using System.Collections.Generic;
using DressUpGame.Data;
using DressUpGame.Pet;
using UnityEngine;

namespace DressUpGame.MysteryBox
{
    public static class MysteryLootCatalogBuilder
    {
        public static List<MysteryLootEntry> Build(MysteryLootRegistry registry)
        {
            List<MysteryLootEntry> entries = new List<MysteryLootEntry>();
            if (registry == null)
            {
                return entries;
            }

            GameCatalog gameCatalog = registry.GameCatalog;
            PetCatalog petCatalog = registry.PetCatalog;
            Dictionary<string, MysteryLootOverride> overrideMap = BuildOverrideMap(registry.Overrides);

            if (gameCatalog != null)
            {
                AddGirlEntries(entries, gameCatalog, overrideMap);
            }

            if (petCatalog != null)
            {
                AddPetEntries(entries, petCatalog, overrideMap);
            }

            int beforeFilter = entries.Count;
            FilterLegacyDuplicateEntries(entries, gameCatalog);
            int skipped = beforeFilter - entries.Count;
#if UNITY_EDITOR
            if (skipped > 0)
            {
                Debug.Log($"Mystery Box loot: excluded {skipped} legacy duplicate entries (color variants exist).");
            }
#endif
            return entries;
        }

        private static void FilterLegacyDuplicateEntries(List<MysteryLootEntry> entries, GameCatalog catalog)
        {
            if (entries == null || entries.Count == 0 || catalog == null)
            {
                return;
            }

            entries.RemoveAll(entry =>
                entry.IsGirl && MysteryLootLegacyFilter.ShouldSkipLegacyGirlLootId(entry.itemId, catalog));
        }

        private static Dictionary<string, MysteryLootOverride> BuildOverrideMap(
            IReadOnlyList<MysteryLootOverride> overrides)
        {
            Dictionary<string, MysteryLootOverride> map = new Dictionary<string, MysteryLootOverride>();
            if (overrides == null)
            {
                return map;
            }

            foreach (MysteryLootOverride entry in overrides)
            {
                if (entry == null || string.IsNullOrEmpty(entry.itemId))
                {
                    continue;
                }

                map[entry.itemId] = entry;
            }

            return map;
        }

        private static void AddGirlEntries(
            List<MysteryLootEntry> entries,
            GameCatalog catalog,
            Dictionary<string, MysteryLootOverride> overrideMap)
        {
            foreach (HairItem item in catalog.HairItems)
            {
                AddGirlEntry(entries, catalog, item?.Id, item?.DisplayName, item?.Sprite, Color.white,
                    MysteryGirlCategory.Hair, overrideMap);
            }

            foreach (DressItem item in catalog.DressItems)
            {
                AddGirlEntry(entries, catalog, item?.Id, item?.DisplayName, item?.Sprite, item?.TintColor ?? Color.white,
                    MysteryGirlCategory.Dresses, overrideMap);
            }

            foreach (ShoeItem item in catalog.GetShoesItems())
            {
                if (item == null || item.IsShoesNone)
                {
                    continue;
                }

                AddGirlEntry(entries, catalog, item.Id, item.DisplayName, item.Sprite, item.TintColor,
                    MysteryGirlCategory.Shoes, overrideMap);
            }

            foreach (MakeupType makeupType in new[]
                     {
                         MakeupType.Eyes, MakeupType.Lipstick, MakeupType.Eyeshadow, MakeupType.Blush
                     })
            {
                foreach (MakeupItem item in catalog.GetMakeupItems(makeupType))
                {
                    if (item == null || item.IsNoneOption)
                    {
                        continue;
                    }

                    MysteryGirlCategory category = makeupType == MakeupType.Eyes
                        ? MysteryGirlCategory.Eyes
                        : MysteryGirlCategory.Makeup;
                    AddGirlEntry(entries, catalog, item.Id, item.DisplayName, item.Sprite, item.TintColor, category,
                        overrideMap);
                }
            }

            foreach (AccessoryItem item in catalog.GetBagItems())
            {
                if (item == null || item.IsBagNone)
                {
                    continue;
                }

                AddGirlEntry(entries, catalog, item.Id, item.DisplayName, item.Sprite, item.TintColor,
                    MysteryGirlCategory.Bags, overrideMap);
            }

            foreach (AccessoryItem item in catalog.GetCrownItems())
            {
                if (item == null || item.IsCrownNone)
                {
                    continue;
                }

                AddGirlEntry(entries, catalog, item.Id, item.DisplayName, item.Sprite, item.TintColor,
                    MysteryGirlCategory.Crown, overrideMap);
            }

            foreach (AccessoryItem item in catalog.GetEarringItems())
            {
                if (item == null || item.IsEarringNone)
                {
                    continue;
                }

                AddGirlEntry(entries, catalog, item.Id, item.DisplayName, item.Sprite, item.TintColor,
                    MysteryGirlCategory.Earrings, overrideMap);
            }

            foreach (AccessoryItem item in catalog.GetGlassesItems())
            {
                if (item == null || item.IsGlassesNone)
                {
                    continue;
                }

                AddGirlEntry(entries, catalog, item.Id, item.DisplayName, item.Sprite, item.TintColor,
                    MysteryGirlCategory.Glasses, overrideMap);
            }

            foreach (AccessoryItem item in catalog.GetNecklaceItems())
            {
                if (item == null || item.IsNecklaceNone)
                {
                    continue;
                }

                AddGirlEntry(entries, catalog, item.Id, item.DisplayName, item.Sprite, item.TintColor,
                    MysteryGirlCategory.Necklace, overrideMap);
            }
        }

        private static void AddGirlEntry(
            List<MysteryLootEntry> entries,
            GameCatalog catalog,
            string id,
            string displayName,
            Sprite sprite,
            Color previewTint,
            MysteryGirlCategory category,
            Dictionary<string, MysteryLootOverride> overrideMap)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (MysteryLootLegacyFilter.ShouldSkipLegacyGirlLootId(id, catalog))
            {
                return;
            }

            overrideMap.TryGetValue(id, out MysteryLootOverride entryOverride);
            MysteryRarity rarity = entryOverride?.rarity ?? MysteryRarity.Common;
            string[] tags = entryOverride?.matchTags != null && entryOverride.matchTags.Length > 0
                ? entryOverride.matchTags
                : MysteryThemeInference.InferTags(id, displayName);

            entries.Add(new MysteryLootEntry
            {
                target = MysteryLootTarget.Girl,
                girlCategory = category,
                itemId = id,
                displayName = displayName ?? id,
                rarity = rarity,
                matchTags = tags,
                previewSprite = sprite,
                previewTint = previewTint
            });
        }

        private static void AddPetEntries(
            List<MysteryLootEntry> entries,
            PetCatalog catalog,
            Dictionary<string, MysteryLootOverride> overrideMap)
        {
            foreach (PetCustomizationCategory category in new[]
                     {
                         PetCustomizationCategory.Pet,
                         PetCustomizationCategory.Collar,
                         PetCustomizationCategory.Glasses,
                         PetCustomizationCategory.Hairbow
                     })
            {
                foreach (PetItem item in catalog.GetItems(category))
                {
                    if (item == null || !IsPetLootEligible(item, category))
                    {
                        continue;
                    }

                    MysteryPetCategory petCategory = MapPetCategory(category);
                    overrideMap.TryGetValue(item.Id, out MysteryLootOverride entryOverride);
                    MysteryRarity rarity = entryOverride?.rarity ?? MysteryRarity.Common;
                    string[] tags = entryOverride?.matchTags != null && entryOverride.matchTags.Length > 0
                        ? entryOverride.matchTags
                        : MysteryThemeInference.InferTags(item.Id, item.DisplayName);

                    entries.Add(new MysteryLootEntry
                    {
                        target = MysteryLootTarget.Pet,
                        petCategory = petCategory,
                        itemId = item.Id,
                        displayName = item.DisplayName,
                        rarity = rarity,
                        matchTags = tags,
                        previewSprite = item.Sprite
                    });
                }
            }
        }

        private static bool IsPetLootEligible(PetItem item, PetCustomizationCategory category)
        {
            if (item.IsNoneOption)
            {
                return false;
            }

            if (category == PetCustomizationCategory.Pet)
            {
                return item.IsPetVariant;
            }

            if (category == PetCustomizationCategory.Hairbow)
            {
                return item.IsHairbowVariant;
            }

            if (category == PetCustomizationCategory.Collar)
            {
                return item.IsCollarVariant;
            }

            if (category == PetCustomizationCategory.Glasses)
            {
                return item.IsGlassesVariant;
            }

            return false;
        }

        private static MysteryPetCategory MapPetCategory(PetCustomizationCategory category)
        {
            return category switch
            {
                PetCustomizationCategory.Pet => MysteryPetCategory.Pet,
                PetCustomizationCategory.Collar => MysteryPetCategory.Collar,
                PetCustomizationCategory.Glasses => MysteryPetCategory.Glasses,
                PetCustomizationCategory.Hairbow => MysteryPetCategory.Hairbows,
                _ => MysteryPetCategory.Pet
            };
        }
    }
}
