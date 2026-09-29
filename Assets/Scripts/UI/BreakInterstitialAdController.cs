using System.Collections;
using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS
using GoogleMobileAds.Api;
#endif

namespace DressUpGame.UI
{
    /// <summary>
    /// Preloads and shows a full-screen interstitial after the tea break screen.
    /// </summary>
    public class BreakInterstitialAdController : MonoBehaviour
    {
        public const string ProductionAdUnitId = "ca-app-pub-5452343649745884/6600838379";

        private static BreakInterstitialAdController instance;

#if UNITY_ANDROID || UNITY_IOS
        private InterstitialAd interstitialAd;
        private bool sdkInitialized;
        private bool pendingPreload;
        private bool showCompleted;
        private bool handlersRegistered;
#endif

        public static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            GameObject controllerObject = new GameObject("BreakInterstitialAdController");
            controllerObject.AddComponent<BreakInterstitialAdController>();
        }

        public static IEnumerator WaitForBreakAdRoutine()
        {
#if UNITY_ANDROID || UNITY_IOS
            EnsureExists();
            if (instance == null)
            {
                yield break;
            }

            yield return instance.ShowIfReadyRoutine();
#else
            yield break;
#endif
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

#if UNITY_ANDROID || UNITY_IOS
            DestroyCurrentAd();
#endif
        }

        private void Start()
        {
#if UNITY_ANDROID || UNITY_IOS
            MobileAdsInitializer.EnsureInitialized(() =>
            {
                sdkInitialized = true;
                pendingPreload = true;
            });
#endif
        }

        private void Update()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (sdkInitialized && pendingPreload)
            {
                pendingPreload = false;
                Preload();
            }
#endif
        }

#if UNITY_ANDROID || UNITY_IOS
        private IEnumerator ShowIfReadyRoutine()
        {
            const float loadTimeoutSeconds = 10f;
            float elapsed = 0f;
            bool preloadRequested = false;

            while ((interstitialAd == null || !interstitialAd.CanShowAd()) && elapsed < loadTimeoutSeconds)
            {
                if (!preloadRequested)
                {
                    Preload();
                    preloadRequested = true;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (interstitialAd == null || !interstitialAd.CanShowAd())
            {
                Debug.LogWarning("Break interstitial was not ready after preload timeout.");
                yield break;
            }

            showCompleted = false;
            RegisterShowHandlers();
            interstitialAd.Show();

            while (!showCompleted)
            {
                yield return null;
            }
        }

        private void Preload()
        {
            if (!sdkInitialized)
            {
                pendingPreload = true;
                return;
            }

            DestroyCurrentAd();

            InterstitialAd.Load(ProductionAdUnitId, new AdRequest(), (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"Break interstitial failed to load: {error}");
                    return;
                }

                interstitialAd = ad;
            });
        }

        private void RegisterShowHandlers()
        {
            if (interstitialAd == null || handlersRegistered)
            {
                return;
            }

            interstitialAd.OnAdFullScreenContentClosed += HandleShowFinished;
            interstitialAd.OnAdFullScreenContentFailed += HandleShowFailed;
            handlersRegistered = true;
        }

        private void UnregisterShowHandlers()
        {
            if (interstitialAd == null || !handlersRegistered)
            {
                return;
            }

            interstitialAd.OnAdFullScreenContentClosed -= HandleShowFinished;
            interstitialAd.OnAdFullScreenContentFailed -= HandleShowFailed;
            handlersRegistered = false;
        }

        private void HandleShowFinished()
        {
            FinishShow();
        }

        private void HandleShowFailed(AdError error)
        {
            Debug.LogWarning($"Break interstitial failed to open: {error}");
            FinishShow();
        }

        private void FinishShow()
        {
            UnregisterShowHandlers();
            DestroyCurrentAd();
            showCompleted = true;
            pendingPreload = true;
            GameplayPauseGuard.EnsureUnpaused();
        }

        private void DestroyCurrentAd()
        {
            UnregisterShowHandlers();

            if (interstitialAd == null)
            {
                return;
            }

            interstitialAd.Destroy();
            interstitialAd = null;
        }
#endif
    }
}
