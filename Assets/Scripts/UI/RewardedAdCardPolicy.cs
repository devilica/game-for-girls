using System.Text.RegularExpressions;

namespace DressUpGame.UI
{
    /// <summary>
    /// Marks every 4th and 5th item card (4, 5, 8, 10, 12, 15, …) and original dress shapes as ad-gated.
    /// </summary>
    public static class RewardedAdCardPolicy
    {
        public static bool IsAdGatedCardIndex(int zeroBasedIndex)
        {
            int oneBasedIndex = zeroBasedIndex + 1;
            return oneBasedIndex % 4 == 0 || oneBasedIndex % 5 == 0;
        }

        public static bool IsOriginalDressId(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && Regex.IsMatch(itemId, @"^dress_\d+$");
        }

        public static bool IsAdGated(int zeroBasedIndex, string itemId = null)
        {
            return IsAdGatedCardIndex(zeroBasedIndex) || IsOriginalDressId(itemId);
        }
    }
}
