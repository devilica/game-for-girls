using System;
using System.Collections;
using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS
using GoogleMobileAds.Api;
#endif

namespace DressUpGame.UI
{
    /// <summary>
    /// Preloads and shows rewarded ads when the player selects ad-gated item cards.
    /// </summary>
    public class RewardedAdController : MonoBehaviour
    {
        public const string ProductionAdUnitId = "ca-app-pub-5452343649745884/6812591365";

        private static RewardedAdController instance;

#if UNITY_ANDROID || UNITY_IOS
        private RewardedAd rewardedAd;
        private bool sdkInitialized;
        private bool pendingPreload;
        private bool showCompleted;
        private bool rewardGranted;
        private bool handlersRegistered;
#endif

        public static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            GameObject controllerObject = new GameObject("RewardedAdController");
            controllerObject.AddComponent<RewardedAdController>();
        }

        public static IEnumerator WaitForRewardRoutine(Action<bool> onComplete)
        {
#if UNITY_ANDROID || UNITY_IOS
            EnsureExists();
            if (instance == null)
            {
                onComplete?.Invoke(true);
                yield break;
            }

            yield return instance.ShowForRewardRoutine(onComplete);
#else
            onComplete?.Invoke(true);
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
        private IEnumerator ShowForRewardRoutine(Action<bool> onComplete)
        {
            if (rewardedAd == null || !rewardedAd.CanShowAd())
            {
                Preload();
                onComplete?.Invoke(true);
                yield break;
            }

            showCompleted = false;
            rewardGranted = false;
            RegisterShowHandlers();

            rewardedAd.Show(reward =>
            {
                rewardGranted = true;
            });

            while (!showCompleted)
            {
                yield return null;
            }

            onComplete?.Invoke(rewardGranted);
        }

        private void Preload()
        {
            if (!sdkInitialized)
            {
                pendingPreload = true;
                return;
            }

            DestroyCurrentAd();

            RewardedAd.Load(ProductionAdUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"Rewarded ad failed to load: {error}");
                    return;
                }

                rewardedAd = ad;
            });
        }

        private void RegisterShowHandlers()
        {
            if (rewardedAd == null || handlersRegistered)
            {
                return;
            }

            rewardedAd.OnAdFullScreenContentClosed += HandleShowFinished;
            rewardedAd.OnAdFullScreenContentFailed += HandleShowFailed;
            handlersRegistered = true;
        }

        private void UnregisterShowHandlers()
        {
            if (rewardedAd == null || !handlersRegistered)
            {
                return;
            }

            rewardedAd.OnAdFullScreenContentClosed -= HandleShowFinished;
            rewardedAd.OnAdFullScreenContentFailed -= HandleShowFailed;
            handlersRegistered = false;
        }

        private void HandleShowFinished()
        {
            FinishShow();
        }

        private void HandleShowFailed(AdError error)
        {
            Debug.LogWarning($"Rewarded ad failed to open: {error}");
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

            if (rewardedAd == null)
            {
                return;
            }

            rewardedAd.Destroy();
            rewardedAd = null;
        }
#endif
    }
}
