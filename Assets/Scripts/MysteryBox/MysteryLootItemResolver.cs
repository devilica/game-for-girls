using System.Collections.Generic;
using DressUpGame.Data;
using DressUpGame.Pet;
using UnityEngine;

namespace DressUpGame.MysteryBox
{
    public static class MysteryLootItemResolver
    {
        public static List<MysteryLootEntry> RefreshRewards(
            IReadOnlyList<MysteryLootEntry> rewards,
            GameCatalog gameCatalog,
            PetCatalog petCatalog)
        {
            List<MysteryLootEntry> refreshed = new List<MysteryLootEntry>();
            if (rewards == null)
            {
                return refreshed;
            }

            foreach (MysteryLootEntry reward in rewards)
            {
                refreshed.Add(RefreshEntry(reward, gameCatalog, petCatalog));
            }

            return refreshed;
        }

        public static MysteryLootEntry RefreshEntry(
            MysteryLootEntry entry,
            GameCatalog gameCatalog,
            PetCatalog petCatalog)
        {
            if (entry.IsGirl)
            {
                if (TryResolveGirlVisual(entry, gameCatalog, out Sprite sprite, out string displayName, out Color previewTint))
                {
                    entry.previewSprite = sprite;
                    entry.previewTint = previewTint;
                    if (!string.IsNullOrEmpty(displayName))
                    {
                        entry.displayName = displayName;
                    }
                }
                else
                {
                    entry.previewTint = Color.white;
                }
            }
            else if (entry.IsPet)
            {
                PetItem pet = petCatalog?.GetById(entry.itemId);
                if (pet != null)
                {
                    entry.previewSprite = pet.Sprite;
                    entry.displayName = pet.DisplayName;
                    entry.previewTint = Color.white;
                }
            }

            return entry;
        }

        public static bool TryResolveGirlSprite(
            MysteryLootEntry entry,
            GameCatalog catalog,
            out Sprite sprite,
            out string displayName)
        {
            return TryResolveGirlVisual(entry, catalog, out sprite, out displayName, out _);
        }

        public static bool TryResolveGirlVisual(
            MysteryLootEntry entry,
            GameCatalog catalog,
            out Sprite sprite,
            out string displayName,
            out Color previewTint)
        {
            sprite = null;
            displayName = null;
            previewTint = Color.white;
            if (catalog == null || string.IsNullOrEmpty(entry.itemId))
            {
                return false;
            }

            switch (entry.girlCategory)
            {
                case MysteryGirlCategory.Hair:
                    HairItem hair = catalog.GetHairByIdExact(entry.itemId);
                    if (hair != null && hair.Id == entry.itemId)
                    {
                        sprite = hair.Sprite;
                        displayName = hair.DisplayName;
                        return true;
                    }

                    break;
                case MysteryGirlCategory.Dresses:
                    DressItem dress = catalog.GetDressByIdExact(entry.itemId);
                    if (dress != null && dress.Id == entry.itemId)
                    {
                        sprite = dress.Sprite;
                        displayName = dress.DisplayName;
                        previewTint = dress.TintColor;
                        return true;
                    }

                    break;
                case MysteryGirlCategory.Shoes:
                    ShoeItem shoe = catalog.GetShoeByIdExact(entry.itemId);
                    if (shoe != null && shoe.Id == entry.itemId)
                    {
                        sprite = shoe.Sprite;
                        displayName = shoe.DisplayName;
                        previewTint = shoe.TintColor;
                        return true;
                    }

                    break;
                case MysteryGirlCategory.Eyes:
                case MysteryGirlCategory.Makeup:
                    MakeupItem makeup = catalog.GetMakeupByIdExact(entry.itemId);
                    if (makeup != null && makeup.Id == entry.itemId)
                    {
                        sprite = makeup.Sprite;
                        displayName = makeup.DisplayName;
                        previewTint = makeup.MakeupType == MakeupType.Eyes ? Color.white : makeup.TintColor;
                        return true;
                    }

                    break;
                default:
                    AccessoryItem accessory = catalog.GetAccessoryByIdExact(entry.itemId);
                    if (accessory != null && accessory.Id == entry.itemId)
                    {
                        sprite = accessory.Sprite;
                        displayName = accessory.DisplayName;
                        previewTint = accessory.TintColor;
                        return true;
                    }

                    break;
            }

            return false;
        }
    }
}
