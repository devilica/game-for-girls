using System;
using UnityEngine;

namespace DressUpGame.MysteryBox
{
    [Serializable]
    public struct MysteryLootEntry
    {
        public MysteryLootTarget target;
        public MysteryGirlCategory girlCategory;
        public MysteryPetCategory petCategory;
        public string itemId;
        public string displayName;
        public MysteryRarity rarity;
        public string[] matchTags;
        public Sprite previewSprite;
        public Color previewTint;

        public bool IsGirl => target == MysteryLootTarget.Girl;
        public bool IsPet => target == MysteryLootTarget.Pet;

        public string CategoryLabel =>
            IsGirl ? girlCategory.ToString() : petCategory.ToString();
    }
}
