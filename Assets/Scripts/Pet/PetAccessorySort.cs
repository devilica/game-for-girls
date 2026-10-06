namespace DressUpGame.Pet
{
    /// <summary>
    /// Numeric ordering for bow_N, collar_N, glasses_N ids (bow_2 before bow_10).
    /// </summary>
    public static class PetAccessorySort
    {
        public static int CompareNumberedIds(
            PetCustomizationCategory category,
            string leftId,
            string rightId)
        {
            int indexCompare = GetNumberedIndex(category, leftId).CompareTo(GetNumberedIndex(category, rightId));
            return indexCompare != 0 ? indexCompare : string.CompareOrdinal(leftId, rightId);
        }

        public static int GetNumberedIndex(PetCustomizationCategory category, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return int.MaxValue;
            }

            string prefix = category switch
            {
                PetCustomizationCategory.Hairbow => "bow_",
                PetCustomizationCategory.Collar => "collar_",
                PetCustomizationCategory.Glasses => "glasses_",
                _ => null
            };

            if (prefix == null || !id.StartsWith(prefix))
            {
                return int.MaxValue;
            }

            string suffix = id.Substring(prefix.Length);
            return int.TryParse(suffix, out int index) ? index : int.MaxValue;
        }
    }
}
