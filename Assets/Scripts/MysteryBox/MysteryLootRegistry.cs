using System;
using System.Collections.Generic;
using DressUpGame.Data;
using DressUpGame.Pet;
using UnityEngine;

namespace DressUpGame.MysteryBox
{
    [Serializable]
    public class MysteryRarityWeights
    {
        [Range(0f, 1f)] public float common = 0.60f;
        [Range(0f, 1f)] public float rare = 0.25f;
        [Range(0f, 1f)] public float epic = 0.10f;
        [Range(0f, 1f)] public float legendary = 0.05f;

        public float GetWeight(MysteryRarity rarity)
        {
            return rarity switch
            {
                MysteryRarity.Common => common,
                MysteryRarity.Rare => rare,
                MysteryRarity.Epic => epic,
                MysteryRarity.Legendary => legendary,
                _ => 0f
            };
        }

        public void NormalizeIfNeeded()
        {
            float sum = common + rare + epic + legendary;
            if (sum <= 0.0001f)
            {
                common = 0.60f;
                rare = 0.25f;
                epic = 0.10f;
                legendary = 0.05f;
                return;
            }

            if (Mathf.Approximately(sum, 1f))
            {
                return;
            }

            common /= sum;
            rare /= sum;
            epic /= sum;
            legendary /= sum;
        }
    }

    [Serializable]
    public class MysteryLootOverride
    {
        public string itemId;
        public MysteryRarity rarity = MysteryRarity.Common;
        public string[] matchTags = Array.Empty<string>();
    }

    [CreateAssetMenu(fileName = "MysteryLootRegistry", menuName = "Dress Up Game/Mystery Loot Registry")]
    public class MysteryLootRegistry : ScriptableObject
    {
        public const string ResourcePath = "MysteryLootRegistry";

        [SerializeField] private GameCatalog gameCatalog;
        [SerializeField] private PetCatalog petCatalog;
        [SerializeField] private MysteryRarityWeights rarityWeights = new MysteryRarityWeights();
        [SerializeField] private List<MysteryLootOverride> overrides = new List<MysteryLootOverride>();

        public GameCatalog GameCatalog => ResolveGameCatalog();
        public PetCatalog PetCatalog => ResolvePetCatalog();
        public MysteryRarityWeights RarityWeights => rarityWeights;
        public IReadOnlyList<MysteryLootOverride> Overrides => overrides;

        public IReadOnlyList<MysteryLootEntry> BuildLootTable()
        {
            return MysteryLootCatalogBuilder.Build(this);
        }

        private GameCatalog ResolveGameCatalog()
        {
            if (gameCatalog != null)
            {
                return gameCatalog;
            }

            return Resources.Load<GameCatalog>("GameCatalog");
        }

        private PetCatalog ResolvePetCatalog()
        {
            if (petCatalog != null)
            {
                return petCatalog;
            }

            return Resources.Load<PetCatalog>("PetCatalog");
        }

#if UNITY_EDITOR
        public void SetCatalogs(GameCatalog girlCatalog, PetCatalog petCat)
        {
            gameCatalog = girlCatalog;
            petCatalog = petCat;
        }
#endif
    }
}
