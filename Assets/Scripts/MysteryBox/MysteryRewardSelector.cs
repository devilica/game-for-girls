using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DressUpGame.MysteryBox
{
    public static class MysteryRewardSelector
    {
        public static MysteryOpenResult Roll(MysteryBoxType boxType, MysteryLootRegistry registry, System.Random random)
        {
            if (registry == null)
            {
                return MysteryOpenResult.Failed("Mystery loot registry is missing.");
            }

            IReadOnlyList<MysteryLootEntry> table = registry.BuildLootTable();
            if (table == null || table.Count == 0)
            {
                return MysteryOpenResult.Failed("No mystery loot entries found in catalogs.");
            }

            random ??= new System.Random();

            return boxType switch
            {
                MysteryBoxType.GirlFashion => RollDualFromTarget(
                    table,
                    MysteryLootTarget.Girl,
                    random,
                    registry.RarityWeights),
                MysteryBoxType.PetSurprise => RollDualFromTarget(
                    table,
                    MysteryLootTarget.Pet,
                    random,
                    registry.RarityWeights),
                MysteryBoxType.Besties => RollBesties(table, random, registry.RarityWeights),
                _ => MysteryOpenResult.Failed("Unknown box type.")
            };
        }

        private static MysteryOpenResult RollDualFromTarget(
            IReadOnlyList<MysteryLootEntry> table,
            MysteryLootTarget target,
            System.Random random,
            MysteryRarityWeights weights)
        {
            List<MysteryLootEntry> pool = table
                .Where(entry => entry.target == target)
                .ToList();

            if (pool.Count == 0)
            {
                return MysteryOpenResult.Failed($"No loot entries for {target} in the mystery catalog.");
            }

            MysteryLootEntry primary = PickFromPool(pool, random, weights);
            List<MysteryLootEntry> secondaryPool = FilterDifferentCategoryPool(pool, primary);
            if (secondaryPool.Count == 0)
            {
                secondaryPool = pool
                    .Where(entry => entry.itemId != primary.itemId)
                    .ToList();
            }

            if (secondaryPool.Count == 0)
            {
                secondaryPool = pool;
            }

            MysteryLootEntry secondary = PickFromPool(secondaryPool, random, weights);
            if (secondaryPool.Count > 1)
            {
                for (int attempt = 0;
                     attempt < 8 && secondary.itemId == primary.itemId;
                     attempt++)
                {
                    secondary = PickFromPool(secondaryPool, random, weights);
                }
            }

            return MysteryOpenResult.Success(new[] { primary, secondary });
        }

        private static bool SharesLootCategory(MysteryLootEntry a, MysteryLootEntry b)
        {
            if (a.target != b.target)
            {
                return false;
            }

            return a.IsGirl
                ? a.girlCategory == b.girlCategory
                : a.petCategory == b.petCategory;
        }

        private static List<MysteryLootEntry> FilterDifferentCategoryPool(
            List<MysteryLootEntry> pool,
            MysteryLootEntry primary)
        {
            return pool
                .Where(entry => !SharesLootCategory(entry, primary))
                .ToList();
        }

        private static MysteryLootEntry PickFromPool(
            List<MysteryLootEntry> pool,
            System.Random random,
            MysteryRarityWeights weights)
        {
            MysteryLootEntry? picked = PickWeightedByRarity(pool, random, weights);
            return picked ?? pool[random.Next(pool.Count)];
        }

        private static MysteryOpenResult RollBesties(
            IReadOnlyList<MysteryLootEntry> table,
            System.Random random,
            MysteryRarityWeights weights)
        {
            List<MysteryLootEntry> girlPool = table
                .Where(entry => entry.IsGirl)
                .ToList();
            List<MysteryLootEntry> petPool = table
                .Where(entry => entry.IsPet)
                .ToList();

            if (girlPool.Count == 0 || petPool.Count == 0)
            {
                return MysteryOpenResult.Failed("Besties box needs both girl and pet loot in the catalog.");
            }

            MysteryLootEntry girl = PickFromPool(girlPool, random, weights);
            List<MysteryLootEntry> matchingPets = petPool
                .Where(pet => MysteryThemeInference.SharesAnyTag(girl.matchTags, pet.matchTags))
                .ToList();

            List<MysteryLootEntry> themedPetPool = matchingPets.Count > 0 ? matchingPets : petPool;
            MysteryLootEntry pet = PickFromPool(themedPetPool, random, weights);

            return MysteryOpenResult.Success(new[] { girl, pet });
        }

        private static MysteryLootEntry? PickWeightedByRarity(
            List<MysteryLootEntry> available,
            System.Random random,
            MysteryRarityWeights weights)
        {
            if (available == null || available.Count == 0)
            {
                return null;
            }

            weights?.NormalizeIfNeeded();
            MysteryRarity[] rarities =
            {
                MysteryRarity.Common,
                MysteryRarity.Rare,
                MysteryRarity.Epic,
                MysteryRarity.Legendary
            };

            foreach (MysteryRarity rarity in OrderRaritiesByWeight(rarities, weights, random))
            {
                List<MysteryLootEntry> pool = available.Where(entry => entry.rarity == rarity).ToList();
                if (pool.Count == 0)
                {
                    continue;
                }

                return pool[random.Next(pool.Count)];
            }

            return available[random.Next(available.Count)];
        }

        private static IEnumerable<MysteryRarity> OrderRaritiesByWeight(
            MysteryRarity[] rarities,
            MysteryRarityWeights weights,
            System.Random random)
        {
            float total = 0f;
            foreach (MysteryRarity rarity in rarities)
            {
                total += weights?.GetWeight(rarity) ?? 0f;
            }

            if (total <= 0f)
            {
                foreach (MysteryRarity rarity in rarities)
                {
                    yield return rarity;
                }

                yield break;
            }

            float roll = (float)random.NextDouble() * total;
            float cumulative = 0f;
            foreach (MysteryRarity rarity in rarities)
            {
                cumulative += weights.GetWeight(rarity);
                if (roll <= cumulative)
                {
                    yield return rarity;
                    yield break;
                }
            }

            yield return MysteryRarity.Common;
        }
    }
}
