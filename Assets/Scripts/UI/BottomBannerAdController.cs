using System;
using DressUpGame.Character;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_ANDROID || UNITY_IOS
using GoogleMobileAds.Api;
#endif

namespace DressUpGame.UI
{
    /// <summary>
    /// Persistent bottom adaptive banner. Starts after the first scene loads (Unity splash stays ad-free).
    /// </summary>
    public class BottomBannerAdController : MonoBehaviour
    {
        public const string ProductionBannerAdUnitId = "ca-app-pub-5452343649745884/9454232429";

        private const float FallbackBannerHeightReferenceUnits = 90f;

        private static BottomBannerAdController instance;

        private float layoutInsetReferenceUnits = FallbackBannerHeightReferenceUnits;

#if UNITY_ANDROID || UNITY_IOS
        private BannerView bannerView;
        private bool sdkInitialized;
        private bool pendingBannerLoad;
        private int bannerLoadAttempts;
        private float nextBannerRetryTime;
        private const int MaxBannerLoadAttempts = 4;
#endif

        private GameObject editorPlaceholderRoot;

        public static float LayoutInsetReferenceUnits
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    return 0f;
                }
#endif
                return instance != null
                    ? instance.layoutInsetReferenceUnits
                    : FallbackBannerHeightReferenceUnits;
            }
        }

        public static event Action LayoutInsetChanged;

        public static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            GameObject controllerObject = new GameObject("BottomBannerAdController");
            controllerObject.AddComponent<BottomBannerAdController>();
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
            SetLayoutInset(FallbackBannerHeightReferenceUnits);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            InitializeMobileAds();
            EnsureCanvasPlaceholder();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == MainMenuScreenController.MenuSceneName || scene.name == "MainScene")
            {
                EnsureBannerActive();
            }
        }

        private void Update()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (!sdkInitialized || !pendingBannerLoad)
            {
                return;
            }

            if (Time.unscaledTime < nextBannerRetryTime)
            {
                return;
            }

            pendingBannerLoad = false;
            LoadBanner();
#endif
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }

#if UNITY_ANDROID || UNITY_IOS
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
            }
#endif
        }

        private void InitializeMobileAds()
        {
#if UNITY_ANDROID || UNITY_IOS
            MobileAdsInitializer.EnsureInitialized(() =>
            {
                sdkInitialized = true;
                pendingBannerLoad = true;
            });
#else
            SetLayoutInset(FallbackBannerHeightReferenceUnits);
            EnsureCanvasPlaceholder();
#endif
        }

#if UNITY_ANDROID || UNITY_IOS
        private void LoadBanner()
        {
            bannerLoadAttempts++;

            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
            }

            int safeWidth = MobileAds.Utils.GetDeviceSafeWidth();
            AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(safeWidth);
            bannerView = new BannerView(ProductionBannerAdUnitId, adaptiveSize, AdPosition.Bottom);

            bannerView.OnBannerAdLoaded += HandleBannerLoaded;
            bannerView.OnBannerAdLoadFailed += HandleBannerLoadFailed;
            bannerView.LoadAd(new AdRequest());
        }

        private void HandleBannerLoaded()
        {
            float heightPixels = bannerView != null ? bannerView.GetHeightInPixels() : 0f;
            float inset = ConvertPixelsToReferenceUnits(heightPixels);
            if (inset <= 0f)
            {
                inset = FallbackBannerHeightReferenceUnits;
            }

            bannerView?.Show();
            SetLayoutInset(inset);
            if (editorPlaceholderRoot != null)
            {
                editorPlaceholderRoot.SetActive(false);
            }
        }

        private void HandleBannerLoadFailed(LoadAdError error)
        {
            Debug.LogWarning($"Bottom banner failed to load (attempt {bannerLoadAttempts}): {error}");
            SetLayoutInset(FallbackBannerHeightReferenceUnits);

            if (bannerLoadAttempts < MaxBannerLoadAttempts)
            {
                nextBannerRetryTime = Time.unscaledTime + (2f * bannerLoadAttempts);
                pendingBannerLoad = true;
            }
        }
#endif

        private static float ConvertPixelsToReferenceUnits(float pixels)
        {
            if (pixels <= 0f || Screen.height <= 0f)
            {
                return 0f;
            }

            return pixels * (SafeAreaInsets.ReferenceHeight / Screen.height);
        }

        private void SetLayoutInset(float inset)
        {
            inset = Mathf.Max(0f, inset);
            bool changed = !Mathf.Approximately(layoutInsetReferenceUnits, inset);
            layoutInsetReferenceUnits = inset;

            if (changed)
            {
                LayoutInsetChanged?.Invoke();
                RefreshPortraitLayout();
            }

            EnsureCanvasPlaceholder();
        }

        public static void EnsureBannerActive()
        {
            EnsureExists();
            if (instance == null)
            {
                return;
            }

            instance.EnsureBannerShown();
            instance.EnsureCanvasPlaceholder();
        }

        public static void RefreshVisibility()
        {
            EnsureBannerActive();
            BringToFront();
        }

        private void EnsureBannerShown()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (bannerView != null)
            {
                bannerView.Show();
                return;
            }

            if (sdkInitialized)
            {
                pendingBannerLoad = true;
                bannerLoadAttempts = Mathf.Min(bannerLoadAttempts, MaxBannerLoadAttempts - 1);
            }
            else
            {
                InitializeMobileAds();
            }
#else
            EnsureCanvasPlaceholder();
#endif
        }

        public static void BringToFront()
        {
            if (instance?.editorPlaceholderRoot != null)
            {
                instance.editorPlaceholderRoot.transform.SetAsLastSibling();
            }
        }

        public static void SetBannerVisible(bool visible)
        {
            if (instance == null)
            {
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            if (visible)
            {
                instance.bannerView?.Show();
            }
            else
            {
                instance.bannerView?.Hide();
            }
#else
            if (instance.editorPlaceholderRoot != null)
            {
                instance.editorPlaceholderRoot.SetActive(visible);
            }
            else if (visible)
            {
                instance.EnsureCanvasPlaceholder();
            }
#endif
        }

        private bool ShouldShowCanvasPlaceholder()
        {
#if UNITY_EDITOR
            return Application.isPlaying;
#elif UNITY_ANDROID || UNITY_IOS
            return bannerView == null;
#else
            return Application.isPlaying;
#endif
        }

        private void EnsureCanvasPlaceholder(Transform canvasTransform = null)
        {
            if (!Application.isPlaying || !ShouldShowCanvasPlaceholder())
            {
                if (editorPlaceholderRoot != null)
                {
                    editorPlaceholderRoot.SetActive(false);
                }

                return;
            }

            if (canvasTransform == null)
            {
                canvasTransform = ResolveGameCanvasTransform();
            }

            if (canvasTransform == null)
            {
                return;
            }

            if (editorPlaceholderRoot == null)
            {
                editorPlaceholderRoot = new GameObject("BannerCanvasPlaceholder");
                editorPlaceholderRoot.transform.SetParent(canvasTransform, false);

                RectTransform rootRect = editorPlaceholderRoot.AddComponent<RectTransform>();
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;

                GameObject strip = new GameObject("Strip");
                strip.transform.SetParent(editorPlaceholderRoot.transform, false);
                RectTransform stripRect = strip.AddComponent<RectTransform>();
                stripRect.anchorMin = new Vector2(0f, 0f);
                stripRect.anchorMax = new Vector2(1f, 0f);
                stripRect.pivot = new Vector2(0.5f, 0f);
                stripRect.offsetMin = Vector2.zero;
                stripRect.offsetMax = new Vector2(0f, layoutInsetReferenceUnits);

                Image stripImage = strip.AddComponent<Image>();
                stripImage.color = new Color(0.97f, 0.97f, 0.97f, 1f);
                stripImage.raycastTarget = false;
            }
            else if (editorPlaceholderRoot.transform.parent != canvasTransform)
            {
                editorPlaceholderRoot.transform.SetParent(canvasTransform, false);
            }

            EnsureEditorPlaceholderRootRect();
            RectTransform placeholderStrip = editorPlaceholderRoot.transform.Find("Strip")?.GetComponent<RectTransform>();
            if (placeholderStrip != null)
            {
                placeholderStrip.offsetMax = new Vector2(0f, layoutInsetReferenceUnits);
            }

            Transform legacyLabel = editorPlaceholderRoot.transform.Find("Strip/Label");
            if (legacyLabel != null)
            {
                Destroy(legacyLabel.gameObject);
            }

            editorPlaceholderRoot.SetActive(true);
            editorPlaceholderRoot.transform.SetAsLastSibling();
        }

        private void EnsureEditorPlaceholderRootRect()
        {
            if (editorPlaceholderRoot == null)
            {
                return;
            }

            RectTransform rootRect = editorPlaceholderRoot.GetComponent<RectTransform>();
            if (rootRect == null)
            {
                rootRect = editorPlaceholderRoot.AddComponent<RectTransform>();
            }

            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
        }

        private static Transform ResolveGameCanvasTransform()
        {
            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            if (layout != null)
            {
                return layout.transform;
            }

            Canvas canvas = FindAnyObjectByType<Canvas>();
            return canvas != null ? canvas.transform : null;
        }

        private static void RefreshPortraitLayout()
        {
            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            if (layout == null)
            {
                return;
            }

            Transform donePanel = layout.transform.Find("DonePanel");
            if (donePanel != null && donePanel.gameObject.activeInHierarchy)
            {
                CharacterCustomizer customizer = FindAnyObjectByType<CharacterCustomizer>();
                customizer?.RefreshDisplayScaleForDoneView();
                return;
            }

            layout.Apply();
        }
    }
}
