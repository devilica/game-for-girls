using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DressUpGame.Pet
{
    [CreateAssetMenu(fileName = "PetCatalog", menuName = "Dress Up Game/Pet Catalog")]
    public class PetCatalog : ScriptableObject
    {
        [SerializeField] private List<PetItem> items = new List<PetItem>();

        public IReadOnlyList<PetItem> Items => items;

        public PetItem GetById(string id) =>
            items.FirstOrDefault(item => item != null && item.Id == id);

        public IEnumerable<PetItem> GetItems(PetCustomizationCategory category)
        {
            IEnumerable<PetItem> filtered = items.Where(item =>
                item != null && item.Category == category && IsListable(item, category));

            if (category != PetCustomizationCategory.Pet)
            {
                return filtered;
            }

            return filtered.OrderBy(item => item, PetItemDisplayOrderComparer.Instance);
        }

        public PetItem GetDefault(PetCustomizationCategory category)
        {
            if (category == PetCustomizationCategory.Pet)
            {
                return GetById("pet_1") ?? GetItems(category).FirstOrDefault();
            }

            foreach (PetItem item in GetItems(category))
            {
                if (item.IsNoneOption || IsNoneForCategory(item, category))
                {
                    return item;
                }
            }

            return GetItems(category).FirstOrDefault();
        }

        private static bool IsListable(PetItem item, PetCustomizationCategory category)
        {
            if (category == PetCustomizationCategory.Pet)
            {
                return item.IsPetVariant;
            }

            return item.IsNoneOption
                || item.IsHairbowVariant
                || item.IsCollarVariant
                || item.IsGlassesVariant;
        }

        private static bool IsNoneForCategory(PetItem item, PetCustomizationCategory category)
        {
            return category switch
            {
                PetCustomizationCategory.Pet => item.IsPetNone,
                PetCustomizationCategory.Hairbow => item.IsHairbowNone,
                PetCustomizationCategory.Collar => item.IsCollarNone,
                PetCustomizationCategory.Glasses => item.IsGlassesNone,
                _ => false
            };
        }

        private sealed class PetItemDisplayOrderComparer : IComparer<PetItem>
        {
            public static readonly PetItemDisplayOrderComparer Instance = new PetItemDisplayOrderComparer();

            public int Compare(PetItem left, PetItem right)
            {
                if (left == null && right == null)
                {
                    return 0;
                }

                if (left == null)
                {
                    return 1;
                }

                if (right == null)
                {
                    return -1;
                }

                return ComparePetIds(left.Id, right.Id);
            }

            private static int ComparePetIds(string leftId, string rightId)
            {
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
                for (int i = 0; i < PetLayoutIds.ColorSuffixes.Length; i++)
                {
                    if (PetLayoutIds.ColorSuffixes[i] == suffix)
                    {
                        colorRank = i + 1;
                        return;
                    }
                }
            }
        }

#if UNITY_EDITOR
        public void SetItems(List<PetItem> rebuilt)
        {
            items = rebuilt;
        }
#endif
    }
}
