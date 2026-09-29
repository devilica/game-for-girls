using DressUpGame.Pet;

namespace DressUpGame.UI
{
    /// <summary>
    /// Pet game 2: every 2nd item card (2, 4, 6, …) in all wizard/edit grids, including pet pick.
    /// </summary>
    public static class PetRewardedAdCardPolicy
    {
        public static bool IsAdGated(PetCustomizationCategory category, int zeroBasedIndex, string itemId)
        {
            return IsEverySecondCard(zeroBasedIndex);
        }

        private static bool IsEverySecondCard(int zeroBasedIndex)
        {
            return (zeroBasedIndex + 1) % 2 == 0;
        }
    }
}
