using System;
using System.Collections;
using DressUpGame.Pet;
using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Runs rewarded ads before applying selection on ad-gated item cards.
    /// </summary>
    public static class AdGatedSelectionHelper
    {
        private static bool gateInProgress;

        public static bool IsGateInProgress => gateInProgress;

        public static void Run(MonoBehaviour host, int cardIndex, string itemId, Action applySelection)
        {
            if (!RewardedAdCardPolicy.IsAdGated(cardIndex, itemId))
            {
                applySelection();
                return;
            }

            if (gateInProgress || host == null)
            {
                return;
            }

            host.StartCoroutine(GateWithRewardedAd(applySelection));
        }

        public static void RunForPet(
            MonoBehaviour host,
            int cardIndex,
            string itemId,
            PetCustomizationCategory category,
            Action applySelection)
        {
            if (!PetRewardedAdCardPolicy.IsAdGated(category, cardIndex, itemId))
            {
                applySelection();
                return;
            }

            if (gateInProgress || host == null)
            {
                return;
            }

            host.StartCoroutine(GateWithRewardedAd(applySelection));
        }

        private static IEnumerator GateWithRewardedAd(Action applySelection)
        {
            gateInProgress = true;
            try
            {
                bool unlocked = false;
                yield return RewardedAdController.WaitForRewardRoutine(result => unlocked = result);

                if (unlocked)
                {
                    applySelection();
                }
            }
            finally
            {
                gateInProgress = false;
            }
        }
    }
}
