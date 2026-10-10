using System.Collections.Generic;
using DressUpGame.Character;
using DressUpGame.Data;
using DressUpGame.Pet;
using DressUpGame.Save;
using UnityEngine;

namespace DressUpGame.MysteryBox
{
    public class MysteryRewardApplier
    {
        private readonly CharacterCustomizer girlCustomizer;
        private readonly PetCustomizer petCustomizer;
        private readonly CharacterSaveManager girlSave;
        private readonly PetSaveManager petSave;
        private readonly GameCatalog gameCatalog;
        private readonly PetCatalog petCatalog;

        public MysteryRewardApplier(
            CharacterCustomizer girlCustomizer,
            PetCustomizer petCustomizer,
            CharacterSaveManager girlSave,
            PetSaveManager petSave,
            GameCatalog gameCatalog,
            PetCatalog petCatalog)
        {
            this.girlCustomizer = girlCustomizer;
            this.petCustomizer = petCustomizer;
            this.girlSave = girlSave;
            this.petSave = petSave;
            this.gameCatalog = gameCatalog;
            this.petCatalog = petCatalog;
        }

        public void ApplyRewards(IReadOnlyList<MysteryLootEntry> rewards)
        {
            if (rewards == null)
            {
                return;
            }

            bool girlDirty = false;
            bool petDirty = false;

            foreach (MysteryLootEntry reward in rewards)
            {
                if (reward.IsGirl)
                {
                    if (ApplyGirlReward(reward))
                    {
                        girlDirty = true;
                        MysteryUnlockStore.Unlock(reward.itemId);
                    }
                }
                else if (reward.IsPet)
                {
                    if (ApplyPetReward(reward))
                    {
                        petDirty = true;
                        MysteryUnlockStore.Unlock(reward.itemId);
                    }
                }
            }

            if (girlDirty)
            {
                girlSave?.SaveCharacter();
            }

            if (petDirty)
            {
                petSave?.SavePet();
            }
        }

        private bool ApplyGirlReward(MysteryLootEntry reward)
        {
            if (girlCustomizer == null || gameCatalog == null)
            {
                return false;
            }

            switch (reward.girlCategory)
            {
                case MysteryGirlCategory.Hair:
                    HairItem hair = gameCatalog.GetHairByIdExact(reward.itemId);
                    if (hair != null && ConfirmExactItemId(reward.itemId, hair.Id))
                    {
                        girlCustomizer.SetHair(hair);
                        return true;
                    }

                    break;
                case MysteryGirlCategory.Dresses:
                    DressItem dress = gameCatalog.GetDressByIdExact(reward.itemId);
                    if (dress != null && ConfirmExactItemId(reward.itemId, dress.Id))
                    {
                        girlCustomizer.SetDress(dress);
                        return true;
                    }

                    break;
                case MysteryGirlCategory.Shoes:
                    ShoeItem shoe = gameCatalog.GetShoeByIdExact(reward.itemId);
                    if (shoe != null && ConfirmExactItemId(reward.itemId, shoe.Id))
                    {
                        girlCustomizer.SetShoe(shoe);
                        return true;
                    }

                    break;
                case MysteryGirlCategory.Eyes:
                case MysteryGirlCategory.Makeup:
                    MakeupItem makeup = gameCatalog.GetMakeupByIdExact(reward.itemId);
                    if (makeup != null && ConfirmExactItemId(reward.itemId, makeup.Id))
                    {
                        ApplyMakeup(makeup);
                        return true;
                    }

                    break;
                case MysteryGirlCategory.Bags:
                case MysteryGirlCategory.Crown:
                case MysteryGirlCategory.Earrings:
                case MysteryGirlCategory.Glasses:
                case MysteryGirlCategory.Necklace:
                    AccessoryItem accessory = gameCatalog.GetAccessoryByIdExact(reward.itemId);
                    if (accessory != null && ConfirmExactItemId(reward.itemId, accessory.Id))
                    {
                        ApplyAccessory(reward.girlCategory, accessory);
                        return true;
                    }

                    break;
            }

            Debug.LogWarning($"Mystery Box: could not apply girl reward '{reward.itemId}' (exact id lookup failed).");
            return false;
        }

        private static bool ConfirmExactItemId(string expectedId, string resolvedId)
        {
            if (resolvedId == expectedId)
            {
                return true;
            }

            Debug.LogWarning(
                $"Mystery Box: refusing to apply '{expectedId}' — resolved id was '{resolvedId ?? "null"}'.");
            return false;
        }

        private void ApplyMakeup(MakeupItem makeup)
        {
            switch (makeup.MakeupType)
            {
                case MakeupType.Eyes:
                    girlCustomizer.SetEyes(makeup);
                    break;
                case MakeupType.Lipstick:
                    girlCustomizer.SetLipstick(makeup);
                    break;
                case MakeupType.Eyeshadow:
                    girlCustomizer.SetEyeshadow(makeup);
                    break;
                case MakeupType.Blush:
                    girlCustomizer.SetBlush(makeup);
                    break;
            }
        }

        private void ApplyAccessory(MysteryGirlCategory category, AccessoryItem accessory)
        {
            switch (category)
            {
                case MysteryGirlCategory.Bags:
                    girlCustomizer.SetBag(accessory);
                    break;
                case MysteryGirlCategory.Crown:
                    girlCustomizer.SetCrown(accessory);
                    break;
                case MysteryGirlCategory.Earrings:
                    girlCustomizer.SetEarrings(accessory);
                    break;
                case MysteryGirlCategory.Glasses:
                    girlCustomizer.SetGlasses(accessory);
                    break;
                case MysteryGirlCategory.Necklace:
                    girlCustomizer.SetNecklace(accessory);
                    break;
            }
        }

        private bool ApplyPetReward(MysteryLootEntry reward)
        {
            if (petCustomizer == null || petCatalog == null)
            {
                return false;
            }

            PetItem item = petCatalog.GetById(reward.itemId);
            if (item == null)
            {
                Debug.LogWarning($"Mystery Box: could not find pet item '{reward.itemId}'.");
                return false;
            }

            PetCustomizationCategory category = reward.petCategory switch
            {
                MysteryPetCategory.Pet => PetCustomizationCategory.Pet,
                MysteryPetCategory.Collar => PetCustomizationCategory.Collar,
                MysteryPetCategory.Glasses => PetCustomizationCategory.Glasses,
                MysteryPetCategory.Hairbows => PetCustomizationCategory.Hairbow,
                _ => PetCustomizationCategory.Pet
            };

            petCustomizer.SetItemForCategory(category, item);
            return true;
        }
    }
}
