using System.Text.RegularExpressions;

namespace DressUpGame.Pet
{
    /// <summary>
    /// Pet ids like pet_1_yellow share accessory layouts with their base pet (pet_1).
    /// </summary>
    public static class PetLayoutIds
    {
        public static readonly string[] ColorSuffixes = { "yellow", "brown", "orange", "grey" };

        private static readonly Regex BasePetPattern = new Regex(@"^pet_(\d+)$", RegexOptions.IgnoreCase);
        private static readonly Regex ColorVariantPattern = new Regex(
            @"^(pet_\d+)_(yellow|brown|orange|grey)$",
            RegexOptions.IgnoreCase);

        public static bool IsBasePetId(string petId) =>
            !string.IsNullOrEmpty(petId) && BasePetPattern.IsMatch(petId);

        public static bool IsColorVariantPetId(string petId) =>
            !string.IsNullOrEmpty(petId) && ColorVariantPattern.IsMatch(petId);

        public static string ResolveBasePetId(string petId)
        {
            if (string.IsNullOrEmpty(petId))
            {
                return petId;
            }

            Match colorMatch = ColorVariantPattern.Match(petId);
            if (colorMatch.Success)
            {
                return colorMatch.Groups[1].Value;
            }

            return petId;
        }

        public static string BuildColorVariantId(string basePetId, string colorSuffix) =>
            $"{basePetId}_{colorSuffix.ToLowerInvariant()}";
    }
}
