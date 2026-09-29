using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID || UNITY_IOS
using GoogleMobileAds.Api;
#endif

namespace DressUpGame.UI
{
    /// <summary>
    /// Preloads and shows a native overlay ad in the tea break sweet slot.
    /// </summary>
    public class BreakNativeAdController : MonoBehaviour
    {
        public const string ProductionAdUnitId = "ca-app-pub-5452343649745884/8157019081";

        private static BreakNativeAdController instance;

#if UNITY_ANDROID || UNITY_IOS
        private NativeOverlayAd nativeOverlayAd;
        private bool sdkInitialized;
        private bool pendingPreload;
        private bool isVisible;
#endif

        public static BreakNativeAdController EnsureExists()
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject controllerObject = new GameObject("BreakNativeAdController");
            return controllerObject.AddComponent<BreakNativeAdController>();
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

        public IEnumerator ShowForBreakRoutine(RectTransform slotRect = null)
        {
#if UNITY_ANDROID || UNITY_IOS
            const float loadTimeoutSeconds = 2.5f;
            float elapsed = 0f;
            bool preloadRequested = false;

            while (nativeOverlayAd == null && elapsed < loadTimeoutSeconds)
            {
                if (!preloadRequested)
                {
                    Preload();
                    preloadRequested = true;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            ShowForBreak(slotRect);
#else
            yield break;
#endif
        }

        public void ShowForBreak(RectTransform slotRect = null)
        {
#if UNITY_ANDROID || UNITY_IOS
            if (nativeOverlayAd == null)
            {
                Debug.LogWarning("Break native ad is not loaded yet.");
                return;
            }

            NativeTemplateStyle style = CreateSweetTemplateStyle();
            if (slotRect != null && TryGetSlotScreenRect(slotRect, out int x, out int y, out int width, out int height))
            {
                nativeOverlayAd.RenderTemplate(style, new AdSize(width, height), x, y);
            }
            else
            {
                nativeOverlayAd.RenderTemplate(style, AdPosition.Bottom);
            }

            nativeOverlayAd.Show();
            isVisible = true;
#endif
        }

        private static bool TryGetSlotScreenRect(RectTransform slotRect, out int x, out int y, out int width, out int height)
        {
            x = 0;
            y = 0;
            width = 0;
            height = 0;

            if (slotRect == null)
            {
                return false;
            }

            Vector3[] corners = new Vector3[4];
            slotRect.GetWorldCorners(corners);

            float minX = corners[0].x;
            float minY = corners[0].y;
            float maxX = corners[2].x;
            float maxY = corners[2].y;

            width = Mathf.Max(1, Mathf.RoundToInt(maxX - minX));
            height = Mathf.Max(1, Mathf.RoundToInt(maxY - minY));
            x = Mathf.RoundToInt(minX);
            y = Mathf.RoundToInt(Screen.height - maxY);
            return width > 0 && height > 0;
        }

        public void HideAfterBreak()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (nativeOverlayAd == null)
            {
                return;
            }

            if (isVisible)
            {
                nativeOverlayAd.Hide();
                isVisible = false;
            }

            DestroyCurrentAd();
            pendingPreload = true;
#endif
        }

#if UNITY_ANDROID || UNITY_IOS
        private void Preload()
        {
            if (!sdkInitialized)
            {
                pendingPreload = true;
                return;
            }

            DestroyCurrentAd();

            NativeAdOptions options = new NativeAdOptions
            {
                AdChoicesPlacement = AdChoicesPlacement.TopRightCorner,
                MediaAspectRatio = MediaAspectRatio.Any,
            };

            NativeOverlayAd.Load(ProductionAdUnitId, new AdRequest(), options,
                (NativeOverlayAd ad, LoadAdError error) =>
                {
                    if (error != null)
                    {
                        Debug.LogWarning($"Break native ad failed to load: {error}");
                        return;
                    }

                    if (ad == null)
                    {
                        Debug.LogWarning("Break native ad load returned a null ad.");
                        return;
                    }

                    nativeOverlayAd = ad;
                });
        }

        private static NativeTemplateStyle CreateSweetTemplateStyle()
        {
            return new NativeTemplateStyle
            {
                TemplateId = NativeTemplateId.Small,
                MainBackgroundColor = new Color(1f, 0.96f, 0.98f, 1f),
                PrimaryText = new NativeTemplateTextStyle
                {
                    TextColor = new Color(0.55f, 0.2f, 0.45f, 1f),
                    FontSize = 14,
                    Style = NativeTemplateFontStyle.Bold,
                },
                SecondaryText = new NativeTemplateTextStyle
                {
                    TextColor = new Color(0.65f, 0.45f, 0.6f, 1f),
                    FontSize = 12,
                },
                CallToActionText = new NativeTemplateTextStyle
                {
                    BackgroundColor = new Color(1f, 0.55f, 0.72f, 1f),
                    TextColor = Color.white,
                    FontSize = 13,
                    Style = NativeTemplateFontStyle.Bold,
                },
            };
        }

        private void DestroyCurrentAd()
        {
            if (nativeOverlayAd == null)
            {
                return;
            }

            nativeOverlayAd.Destroy();
            nativeOverlayAd = null;
            isVisible = false;
        }
#endif
    }
}
