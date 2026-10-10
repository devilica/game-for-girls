using System.Text.RegularExpressions;
using DressUpGame.Data;
using UnityEngine;

namespace DressUpGame.MysteryBox
{
    /// <summary>
    /// Detects legacy base item ids that should not appear in mystery loot when color variants exist in the catalog.
    /// </summary>
    public static class MysteryLootLegacyFilter
    {
        public static bool ShouldSkipLegacyGirlLootId(string itemId, GameCatalog catalog)
        {
            if (string.IsNullOrEmpty(itemId) || catalog == null)
            {
                return false;
            }

            Match dress = Regex.Match(itemId, @"^dress_(\d+)$");
            if (dress.Success && CatalogHasDressColorVariants(catalog, dress.Groups[1].Value))
            {
                return true;
            }

            Match lipstick = Regex.Match(itemId, @"^lipstick_(\d+)$");
            if (lipstick.Success && CatalogHasMakeupId(catalog, $"lipstick_{lipstick.Groups[1].Value}_01"))
            {
                return true;
            }

            if ((itemId == "eyes_1" || itemId == "eyes_2") && CatalogHasMakeupId(catalog, "eyes_2_01"))
            {
                return true;
            }

            Match eyeshadow = Regex.Match(itemId, @"^eyeshadow_(\d+)$");
            if (eyeshadow.Success)
            {
                string index = eyeshadow.Groups[1].Value;
                string variantId = int.Parse(index) < 10
                    ? $"eyeshadow_2_0{index}"
                    : $"eyeshadow_2_{index}";
                if (CatalogHasMakeupId(catalog, variantId))
                {
                    return true;
                }
            }

            Match blush = Regex.Match(itemId, @"^blush_(\d+)$");
            if (blush.Success)
            {
                string index = blush.Groups[1].Value;
                string variantId = int.Parse(index) < 10
                    ? $"blush_2_0{index}"
                    : $"blush_2_{index}";
                if (CatalogHasMakeupId(catalog, variantId))
                {
                    return true;
                }
            }

            Match crown = Regex.Match(itemId, @"^crown_(\d+)$");
            if (crown.Success && CatalogHasAccessoryId(catalog, $"crown_{crown.Groups[1].Value}_01"))
            {
                return true;
            }

            if (itemId == "accessory_crown" && CatalogHasAccessoryId(catalog, "crown_1_01"))
            {
                return true;
            }

            return false;
        }

        private static bool CatalogHasDressColorVariants(GameCatalog catalog, string dressIndex)
        {
            string prefix = $"dress_{dressIndex}_";
            foreach (DressItem dress in catalog.DressItems)
            {
                if (dress != null && dress.Id.StartsWith(prefix))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CatalogHasMakeupId(GameCatalog catalog, string id)
        {
            return catalog.GetMakeupByIdExact(id) != null;
        }

        private static bool CatalogHasAccessoryId(GameCatalog catalog, string id)
        {
            return catalog.GetAccessoryByIdExact(id) != null;
        }
    }
}
