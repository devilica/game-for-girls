using System;
using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS
using GoogleMobileAds.Api;
#endif

namespace DressUpGame.UI
{
    /// <summary>
    /// Initializes the Mobile Ads SDK once for all ad controllers.
    /// </summary>
    public static class MobileAdsInitializer
    {
        private static bool initStarted;
        private static bool initComplete;
        private static event Action Initialized;

        public static bool IsReady => initComplete;

        public static void EnsureInitialized(Action onReady = null)
        {
#if UNITY_ANDROID || UNITY_IOS
            if (onReady != null)
            {
                if (initComplete)
                {
                    onReady();
                }
                else
                {
                    Initialized += onReady;
                }
            }

            if (initStarted)
            {
                return;
            }

            initStarted = true;
            MobileAds.Initialize(_ =>
            {
                initComplete = true;
                Initialized?.Invoke();
                Initialized = null;
            });
#else
            onReady?.Invoke();
#endif
        }
    }
}
