using System.Collections;
using System.Collections.Generic;
using System.Text;
using DressUpGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DressUpGame.Memory
{
    public sealed class MemoryGameController : MonoBehaviour
    {
        public const string MemorySceneName = "MemoryScene";
        private const int GridColumns = 4;
        private const int GridRows = 5;
        private const int CellCount = GridColumns * GridRows;
        private const int BowPairCount = 8;
        private const int AdPairCount = 2;
        private const int TotalPairCount = BowPairCount + AdPairCount;
        private const int AdPairIdStart = 9001;
        private const string BowResourceFolder = "MemoryBows";
        private const string AdSpriteResourcePath = "UI/ad";
        private const string StartAgainSpriteResourcePath = "UI/startagain";
        private const float MismatchHideDelaySeconds = 0.75f;
        private const float BoardPaddingReference = 36f;
        private const float BoardTopInsetReference = 240f;
        private const float HeaderBannerWidthReference = 900f;
        private const float HeaderBannerHeightReference = 200f;
        private const float HeaderBannerTopOffsetReference = 32f;
        private const float BoardBottomInsetReference = 140f;
        private const float BoardBannerReserveReferenceUnits = 90f;
        private const float GridSpacingReference = 8f;
        private const float NativeSlotGapReference = 12f;
        private const float BoardNativeSlotReserveReferenceUnits =
            NativeSlotGapReference + BreakNativeAdSlotView.MemorySlotHeightReferenceUnits;
        private const float StartAgainWidthReference = 920f;
        private const float StartAgainHeightReference = 240f;
        private const float StartAgainBlinkMinAlpha = 0.72f;
        private const float StartAgainBlinkPeriodSeconds = 0.9f;
        private const float StartAgainBlinkMaxScale = 1.06f;

        private static readonly Color CardBackColor = new Color(1f, 0.94f, 0.97f, 1f);
        private static readonly Color AdCardFaceColor = new Color(1f, 0.88f, 0.94f, 1f);

        [SerializeField] private MemoryBowCatalog bowCatalog;
        [SerializeField] private RectTransform boardRoot;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private BreakNativeAdSlotView nativeAdSlot;

        private readonly List<MemoryCardView> cards = new List<MemoryCardView>(CellCount);
        private BreakNativeAdController nativeAdController;
        private bool nativeAdSlotDisplayed;
        private static Sprite cachedAdSprite;
        private static Sprite cachedStartAgainSprite;
        private MemoryCardView firstFlipped;
        private MemoryCardView secondFlipped;
        private int matchedPairs;
        private bool inputLocked;
        private bool isResolvingMismatch;
        private bool gameWon;
        private bool isRestartPending;
        private Coroutine startAgainBlinkRoutine;
        private CanvasGroup startAgainCanvasGroup;
        private RectTransform startAgainRect;
        private Vector3 startAgainBaseScale = Vector3.one;
        private Dictionary<int, Sprite> bowSpritesById;

        private void Awake()
        {
            EnsureCameraAndAudioListener();
            EnsureRuntimeHierarchy();
            if (grid == null && boardRoot != null)
            {
                grid = boardRoot.GetComponent<GridLayoutGroup>();
            }
        }

        private static void EnsureCameraAndAudioListener()
        {
            AudioListener listener = FindAnyObjectByType<AudioListener>();
            if (listener != null && listener.enabled)
            {
                return;
            }

            Camera camera = Camera.main ?? FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                GameObject cameraGo = new GameObject("Main Camera");
                camera = cameraGo.AddComponent<Camera>();
                cameraGo.tag = "MainCamera";
                camera.orthographic = true;
                camera.orthographicSize = 6f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.backgroundColor = new Color(0.98f, 0.92f, 0.96f);
                camera.clearFlags = CameraClearFlags.SolidColor;
            }

            if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }
        }

        private void EnsureRuntimeHierarchy()
        {
            if (boardRoot != null && grid != null)
            {
                return;
            }

            Camera camera = Camera.main ?? FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                EnsureCameraAndAudioListener();
                camera = Camera.main ?? FindAnyObjectByType<Camera>();
            }

            GameBackgroundDisplay background = GameBackgroundDisplay.EnsureExists(camera);
            background.SetBackgroundSources("UI/menu/background", "Assets/Resources/UI/menu/background.png");
            background.Apply(camera);
            EnsureEventSystemExists();
            BuildRuntimeCanvas();
        }

        private static void EnsureEventSystemExists()
        {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private void BuildRuntimeCanvas()
        {
            Canvas existingCanvas = GetComponent<Canvas>();
            if (existingCanvas == null)
            {
                existingCanvas = FindAnyObjectByType<Canvas>();
            }

            if (existingCanvas != null && boardRoot != null)
            {
                return;
            }

            GameObject canvasGo = existingCanvas != null ? existingCanvas.gameObject : new GameObject("Canvas");
            if (existingCanvas == null)
            {
                canvasGo.transform.SetParent(null, false);
                existingCanvas = canvasGo.AddComponent<Canvas>();
                existingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 1f;
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            if (boardRoot == null)
            {
                GameObject boardGo = new GameObject("MemoryBoard");
                boardGo.transform.SetParent(canvasRect, false);
                boardRoot = boardGo.AddComponent<RectTransform>();
                boardRoot.anchorMin = Vector2.zero;
                boardRoot.anchorMax = Vector2.one;
                boardRoot.offsetMin = Vector2.zero;
                boardRoot.offsetMax = Vector2.zero;
                grid = boardGo.AddComponent<GridLayoutGroup>();
            }

            EnsureWinPanel(canvasRect);
        }

        private void EnsureWinPanel(Transform canvasRoot)
        {
            if (canvasRoot == null)
            {
                return;
            }

            if (winPanel != null && IsLegacyWinPanel(winPanel))
            {
                winPanel = UpgradeLegacyWinPanel(winPanel.transform);
                restartButton = winPanel.GetComponent<Button>();
            }

            if (winPanel == null)
            {
                Transform startAgain = canvasRoot.Find("StartAgainButton");
                if (startAgain != null)
                {
                    winPanel = startAgain.gameObject;
                    restartButton = startAgain.GetComponent<Button>();
                }
                else
                {
                    Transform legacyPanel = canvasRoot.Find("WinPanel");
                    if (legacyPanel != null)
                    {
                        winPanel = UpgradeLegacyWinPanel(legacyPanel);
                        restartButton = winPanel.GetComponent<Button>();
                    }
                }
            }

            if (winPanel == null)
            {
                winPanel = CreateStartAgainButtonHierarchy(canvasRoot, out restartButton);
            }
            else if (!IsLegacyWinPanel(winPanel))
            {
                ApplyStartAgainLayout(winPanel.GetComponent<RectTransform>(), GetCanvasScaler(canvasRoot));
                if (restartButton == null)
                {
                    restartButton = winPanel.GetComponent<Button>();
                }

                ApplyStartAgainButtonAppearance(restartButton);
            }

            WireRestartButton();
            EnsureStartAgainPresentation();
            winPanel.SetActive(false);
        }

        private void EnsureStartAgainPresentation()
        {
            if (winPanel == null)
            {
                return;
            }

            startAgainRect = winPanel.GetComponent<RectTransform>();
            startAgainCanvasGroup = winPanel.GetComponent<CanvasGroup>();
            if (startAgainCanvasGroup == null)
            {
                startAgainCanvasGroup = winPanel.AddComponent<CanvasGroup>();
            }

            startAgainBaseScale = Vector3.one;
            if (startAgainRect != null)
            {
                startAgainRect.localScale = startAgainBaseScale;
            }

            startAgainCanvasGroup.alpha = 1f;
        }

        private static bool IsLegacyWinPanel(GameObject panel)
        {
            if (panel == null)
            {
                return false;
            }

            if (panel.name == "StartAgainButton" && panel.GetComponent<Button>() != null)
            {
                return false;
            }

            if (panel.name == "WinPanel")
            {
                return true;
            }

            return panel.transform.Find("WinText") != null;
        }

        private static void StripLegacyWinChrome(Transform legacyPanel)
        {
            if (legacyPanel == null)
            {
                return;
            }

            Transform winText = legacyPanel.Find("WinText");
            if (winText != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(winText.gameObject);
                }
                else
                {
                    winText.gameObject.SetActive(false);
                }
            }

            Image panelBackground = legacyPanel.GetComponent<Image>();
            if (panelBackground != null)
            {
                panelBackground.raycastTarget = false;
                panelBackground.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        private static GameObject UpgradeLegacyWinPanel(Transform legacyPanel)
        {
            StripLegacyWinChrome(legacyPanel);

            Transform restartTransform = legacyPanel.Find("RestartButton");
            GameObject host;
            Button restart;
            if (restartTransform != null)
            {
                host = restartTransform.gameObject;
                host.transform.SetParent(legacyPanel.parent, false);
                restart = host.GetComponent<Button>();
                ApplyStartAgainLayout(host.GetComponent<RectTransform>(), GetCanvasScaler(legacyPanel.parent));
                ApplyStartAgainButtonAppearance(restart);
                legacyPanel.gameObject.SetActive(false);
            }
            else
            {
                host = CreateStartAgainButtonHierarchy(legacyPanel.parent, out restart);
                legacyPanel.gameObject.SetActive(false);
            }

            host.name = "StartAgainButton";
            return host;
        }

        private static GameObject CreateStartAgainButtonHierarchy(Transform canvasRoot, out Button restart)
        {
            GameObject startAgainGo = new GameObject("StartAgainButton");
            startAgainGo.transform.SetParent(canvasRoot, false);
            RectTransform rect = startAgainGo.AddComponent<RectTransform>();
            ApplyStartAgainLayout(rect, GetCanvasScaler(canvasRoot));

            Image image = startAgainGo.AddComponent<Image>();
            ApplyStartAgainSprite(image);
            image.raycastTarget = true;

            restart = startAgainGo.AddComponent<Button>();
            restart.targetGraphic = image;
            ColorBlock colors = restart.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            restart.colors = colors;

            startAgainGo.SetActive(false);
            return startAgainGo;
        }

        private static void ApplyStartAgainButtonAppearance(Button restart)
        {
            if (restart == null)
            {
                return;
            }

            Transform label = restart.transform.Find("Label");
            if (label != null)
            {
                label.gameObject.SetActive(false);
            }

            Image image = restart.GetComponent<Image>();
            if (image != null)
            {
                ApplyStartAgainSprite(image);
                image.preserveAspect = true;
            }

            ColorBlock colors = restart.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            restart.colors = colors;
        }

        private static void ApplyStartAgainSprite(Image image)
        {
            if (image == null)
            {
                return;
            }

            Sprite sprite = GetStartAgainSprite();
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.color = Color.white;
                image.preserveAspect = true;
            }
        }

        private static Sprite GetStartAgainSprite()
        {
            if (cachedStartAgainSprite != null)
            {
                return cachedStartAgainSprite;
            }

            cachedStartAgainSprite = Resources.Load<Sprite>(StartAgainSpriteResourcePath);
            return cachedStartAgainSprite;
        }

        private static void ApplyStartAgainLayout(RectTransform rect, CanvasScaler scaler)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(StartAgainWidthReference, StartAgainHeightReference);
            rect.localScale = Vector3.one;
        }

        private static CanvasScaler GetCanvasScaler(Transform canvasRoot)
        {
            if (canvasRoot == null)
            {
                return null;
            }

            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasRoot.GetComponentInParent<Canvas>();
            }

            return canvas != null ? canvas.GetComponent<CanvasScaler>() : null;
        }

        private void ApplyMemoryTopBarLayout(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            float safeTop = SafeAreaInsets.GetTopInsetReferenceUnits(scaler);
            HomeMenuButton.EnsureOnCanvas(canvas.transform)?.ApplyTopLeftLayout(safeTop);
            SoundToggleButton.EnsureOnCanvas(canvas.transform)?.ApplyTopRightLayout(safeTop);
            ApplyHeaderBannerLayout(canvas.transform, safeTop);
        }

        private static void EnsureHeaderBanner(Transform canvasRoot)
        {
            if (canvasRoot == null)
            {
                return;
            }

            Transform existing = canvasRoot.Find("HeaderBanner");
            if (existing != null)
            {
                Image existingImage = existing.GetComponent<Image>();
                if (existingImage != null && existingImage.sprite == null)
                {
                    existingImage.sprite = MemoryGameArt.LoadHeaderSprite();
                }

                return;
            }

            GameObject headerGo = new GameObject("HeaderBanner");
            headerGo.transform.SetParent(canvasRoot, false);
            RectTransform rect = headerGo.AddComponent<RectTransform>();
            Image image = headerGo.AddComponent<Image>();
            image.sprite = MemoryGameArt.LoadHeaderSprite();
            image.preserveAspect = true;
            image.raycastTarget = false;
            ApplyHeaderBannerLayout(canvasRoot, 0f);
        }

        private static void ApplyHeaderBannerLayout(Transform canvasRoot, float safeTopInset)
        {
            Transform header = canvasRoot != null ? canvasRoot.Find("HeaderBanner") : null;
            if (header == null)
            {
                return;
            }

            RectTransform rect = header as RectTransform;
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -(HeaderBannerTopOffsetReference + safeTopInset));
            rect.sizeDelta = new Vector2(HeaderBannerWidthReference, HeaderBannerHeightReference);
        }

        private void ApplyStartAgainLayoutIfVisible(Canvas canvas)
        {
            if (!gameWon || winPanel == null || !winPanel.activeSelf)
            {
                return;
            }

            CanvasScaler scaler = canvas != null ? canvas.GetComponent<CanvasScaler>() : null;
            ApplyStartAgainLayout(winPanel.GetComponent<RectTransform>(), scaler);
        }

        private void StartStartAgainBlink()
        {
            StopStartAgainBlink();
            if (winPanel == null)
            {
                return;
            }

            EnsureStartAgainPresentation();
            startAgainBlinkRoutine = StartCoroutine(StartAgainBlinkRoutine());
        }

        private void StopStartAgainBlink()
        {
            if (startAgainBlinkRoutine != null)
            {
                StopCoroutine(startAgainBlinkRoutine);
                startAgainBlinkRoutine = null;
            }

            if (startAgainCanvasGroup != null)
            {
                startAgainCanvasGroup.alpha = 1f;
            }

            if (startAgainRect != null)
            {
                startAgainRect.localScale = startAgainBaseScale;
            }
        }

        private IEnumerator StartAgainBlinkRoutine()
        {
            EnsureStartAgainPresentation();
            while (true)
            {
                float t = Mathf.PingPong(Time.unscaledTime / StartAgainBlinkPeriodSeconds, 1f);
                if (startAgainCanvasGroup != null)
                {
                    startAgainCanvasGroup.alpha = Mathf.Lerp(StartAgainBlinkMinAlpha, 1f, t);
                }

                if (startAgainRect != null)
                {
                    float scale = Mathf.Lerp(1f, StartAgainBlinkMaxScale, t);
                    startAgainRect.localScale = startAgainBaseScale * scale;
                }

                yield return null;
            }
        }

        private void WireRestartButton()
        {
            if (restartButton == null)
            {
                return;
            }

            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }

        private void Start()
        {
            MobileAdsInitializer.EnsureInitialized();
            BottomBannerAdController.EnsureExists();
            BottomBannerAdController.LayoutInsetChanged += HandleBannerInsetChanged;

            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = FindAnyObjectByType<Canvas>();
            }

            if (canvas != null)
            {
                EnsureHeaderBanner(canvas.transform);
                SoundToggleButton.EnsureOnCanvas(canvas.transform);
                HomeMenuButton.EnsureOnCanvas(canvas.transform);
                ApplyMemoryTopBarLayout(canvas);
            }

            if (canvas != null)
            {
                EnsureWinPanel(canvas.transform);
            }

            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }

            if (canvas != null)
            {
                EnsureNativeAdSlot(canvas);
            }

            BuildBoard();
            ApplyBoardLayout();
            BottomBannerAdController.EnsureBannerActive();
            BottomBannerAdController.RefreshVisibility();

            if (nativeAdSlot != null)
            {
                nativeAdController = BreakNativeAdController.EnsureExists();
                nativeAdSlot.SetVisible(false);
                StartCoroutine(ShowNativeAdRoutine());
            }
        }

        private void OnDestroy()
        {
            BottomBannerAdController.LayoutInsetChanged -= HandleBannerInsetChanged;
            nativeAdController?.HideFromSlot();
            nativeAdSlotDisplayed = false;
            nativeAdSlot?.SetVisible(false);
        }

        private void EnsureNativeAdSlot(Canvas canvas)
        {
            if (nativeAdSlot != null)
            {
                return;
            }

            nativeAdSlot = BreakNativeAdSlotView.EnsureForMemory(canvas.transform);
        }

        private IEnumerator ShowNativeAdRoutine()
        {
            if (nativeAdController == null || nativeAdSlot == null)
            {
                yield break;
            }

            nativeAdSlot.ApplyMemoryLayout(BoardPaddingReference);
            nativeAdSlot.PrepareHiddenForAdRender();

            RectTransform renderRect = nativeAdSlot.ContentRect != null
                ? nativeAdSlot.ContentRect
                : nativeAdSlot.SlotRect;

            bool shown = false;
            yield return nativeAdController.ShowForBreakRoutine(renderRect, success => shown = success);

            if (shown)
            {
                nativeAdSlotDisplayed = true;
                nativeAdSlot.PrepareHiddenForAdRender();
            }
            else
            {
                nativeAdSlotDisplayed = false;
                nativeAdSlot.HideSlotCompletely();
            }

            nativeAdSlot.ApplyMemoryLayout(BoardPaddingReference);
        }

        private void OnRectTransformDimensionsChange()
        {
            Canvas canvas = GetComponent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                ApplyMemoryTopBarLayout(canvas);
            }

            ApplyBoardLayout();
            ApplyStartAgainLayoutIfVisible(canvas);
        }

        private void HandleBannerInsetChanged()
        {
            if (nativeAdSlotDisplayed)
            {
                nativeAdSlot?.ApplyMemoryLayout(BoardPaddingReference);
            }
        }

        public bool TryHandleBackNavigation()
        {
            GameplayPauseGuard.EnsureUnpaused();
            SceneManager.LoadScene(MainMenuScreenController.MenuSceneName);
            return true;
        }

        public void HandleCardClicked(MemoryCardView card)
        {
            if (gameWon || inputLocked || isResolvingMismatch || card == null || card.IsMatched || card.IsFaceUp)
            {
                return;
            }

            card.SetFaceUp(true);

            if (firstFlipped == null)
            {
                firstFlipped = card;
                return;
            }

            if (firstFlipped == card)
            {
                return;
            }

            secondFlipped = card;
            inputLocked = true;

            if (AreMatchingPair(firstFlipped, secondFlipped))
            {
                bool isAdPair = IsAdPairId(firstFlipped.PairId) || IsAdPairId(secondFlipped.PairId);
                firstFlipped.SetMatched(true);
                secondFlipped.SetMatched(true);
                matchedPairs++;
                firstFlipped = null;
                secondFlipped = null;

                if (isAdPair)
                {
                    StartCoroutine(ShowInterstitialAfterAdPairMatchedRoutine());
                }
                else
                {
                    inputLocked = false;
                    TryShowWin();
                }

                return;
            }

            StartCoroutine(HideMismatchRoutine());
        }

        private IEnumerator HideMismatchRoutine()
        {
            isResolvingMismatch = true;
            yield return new WaitForSeconds(MismatchHideDelaySeconds);

            if (firstFlipped != null)
            {
                firstFlipped.SetFaceUp(false);
            }

            if (secondFlipped != null)
            {
                secondFlipped.SetFaceUp(false);
            }

            firstFlipped = null;
            secondFlipped = null;
            inputLocked = false;
            isResolvingMismatch = false;
        }

        private IEnumerator ShowInterstitialAfterAdPairMatchedRoutine()
        {
            BreakInterstitialAdController.EnsureExists();
            yield return BreakInterstitialAdController.WaitForBreakAdRoutine();
            GameplayPauseGuard.EnsureUnpaused();
            inputLocked = false;
            TryShowWin();
        }

        private void TryShowWin()
        {
            if (matchedPairs >= TotalPairCount)
            {
                ShowWin();
            }
        }

        private void ShowWin()
        {
            gameWon = true;
            inputLocked = true;

            for (int i = 0; i < cards.Count; i++)
            {
                float phase = i * 0.11f + Random.Range(0f, 0.08f);
                cards[i].StartWinBorderCelebration(phase);
            }

            if (winPanel != null)
            {
                Canvas canvas = GetComponent<Canvas>() ?? FindAnyObjectByType<Canvas>();
                ApplyStartAgainLayout(
                    winPanel.GetComponent<RectTransform>(),
                    canvas != null ? canvas.GetComponent<CanvasScaler>() : null);
                winPanel.SetActive(true);
                winPanel.transform.SetAsLastSibling();
                StartStartAgainBlink();
            }
        }

        public void RestartGame()
        {
            if (isRestartPending)
            {
                return;
            }

            StartCoroutine(RestartAfterInterstitialRoutine());
        }

        private IEnumerator RestartAfterInterstitialRoutine()
        {
            isRestartPending = true;
            inputLocked = true;

            BreakInterstitialAdController.EnsureExists();
            yield return BreakInterstitialAdController.WaitForBreakAdRoutine();
            GameplayPauseGuard.EnsureUnpaused();

            gameWon = false;
            StopAllWinCelebrations();
            StopStartAgainBlink();

            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }

            BuildBoard();
            ApplyBoardLayout();

            inputLocked = false;
            isRestartPending = false;
        }

        private void StopAllWinCelebrations()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].StopWinBorderCelebration();
            }
        }

        private static bool AreMatchingPair(MemoryCardView a, MemoryCardView b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (a.PairId == b.PairId)
            {
                return true;
            }

            return IsAdPairId(a.PairId) && IsAdPairId(b.PairId);
        }

        private static bool IsAdPairId(int pairId)
        {
            return pairId >= AdPairIdStart && pairId < AdPairIdStart + AdPairCount;
        }

        private void BuildBoard()
        {
            if (grid == null || boardRoot == null)
            {
                Debug.LogError("MemoryGameController is missing boardRoot or GridLayoutGroup.");
                return;
            }

            ClearBoard();
            EnsureBowSpritesLoaded();

            List<int> pairIds = new List<int>(CellCount);
            for (int i = 1; i <= BowPairCount; i++)
            {
                pairIds.Add(i);
                pairIds.Add(i);
            }

            for (int adIndex = 0; adIndex < AdPairCount; adIndex++)
            {
                int adPairId = AdPairIdStart + adIndex;
                pairIds.Add(adPairId);
                pairIds.Add(adPairId);
            }

            Shuffle(pairIds);

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = GridColumns;
            grid.spacing = new Vector2(GridSpacingReference, GridSpacingReference);
            grid.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < pairIds.Count; i++)
            {
                int id = pairIds[i];
                Sprite faceSprite = GetSpriteForPairId(id);
                MemoryCardView card = CreateCardView(id, faceSprite);
                cards.Add(card);
            }
        }

        private void ClearBoard()
        {
            StopAllWinCelebrations();
            cards.Clear();
            firstFlipped = null;
            secondFlipped = null;
            matchedPairs = 0;
            inputLocked = false;
            isResolvingMismatch = false;

            if (grid == null)
            {
                return;
            }

            for (int i = grid.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(grid.transform.GetChild(i).gameObject);
            }
        }

        private MemoryCardView CreateCardView(int pairId, Sprite bowSprite)
        {
            GameObject cardGo = new GameObject($"Card_{pairId}");
            cardGo.transform.SetParent(grid.transform, false);

            Image borderImage = cardGo.AddComponent<Image>();
            borderImage.sprite = UiRoundSpriteUtility.GetRoundedSquareSprite();
            borderImage.type = Image.Type.Sliced;
            borderImage.color = UiBorderCelebrationStyle.BaseBorderColor;
            borderImage.raycastTarget = true;

            Button button = cardGo.AddComponent<Button>();
            button.targetGraphic = borderImage;
            ColorBlock colors = button.colors;
            colors.normalColor = UiBorderCelebrationStyle.BaseBorderColor;
            colors.highlightedColor = new Color(1f, 0.55f, 0.78f, 1f);
            colors.pressedColor = new Color(0.88f, 0.38f, 0.62f, 1f);
            colors.disabledColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            button.colors = colors;

            GameObject backGo = new GameObject("Back");
            backGo.transform.SetParent(cardGo.transform, false);
            RectTransform backRect = backGo.AddComponent<RectTransform>();
            Stretch(backRect, 6f);
            Image backImage = backGo.AddComponent<Image>();
            backImage.sprite = UiRoundSpriteUtility.GetRoundedSquareSprite();
            backImage.type = Image.Type.Sliced;
            backImage.color = CardBackColor;
            backImage.raycastTarget = false;

            GameObject frontBgGo = new GameObject("FrontBackground");
            frontBgGo.transform.SetParent(cardGo.transform, false);
            RectTransform frontBgRect = frontBgGo.AddComponent<RectTransform>();
            Stretch(frontBgRect, 14f);
            Image frontBgImage = frontBgGo.AddComponent<Image>();
            frontBgImage.sprite = UiRoundSpriteUtility.GetRoundedSquareSprite();
            frontBgImage.type = Image.Type.Sliced;
            frontBgImage.color = CardBackColor;
            frontBgImage.raycastTarget = false;
            frontBgImage.enabled = false;

            GameObject frontGo = new GameObject("Front");
            frontGo.transform.SetParent(cardGo.transform, false);
            RectTransform frontRect = frontGo.AddComponent<RectTransform>();
            Stretch(frontRect, 18f);
            Image frontImage = frontGo.AddComponent<Image>();
            frontImage.type = Image.Type.Simple;
            frontImage.color = Color.white;
            frontImage.preserveAspect = true;
            frontImage.raycastTarget = false;
            frontImage.enabled = false;

            if (IsAdPairId(pairId))
            {
                frontBgImage.color = AdCardFaceColor;
                frontImage.color = new Color(1f, 0.95f, 0.98f, 1f);
            }

            MemoryCardView view = cardGo.AddComponent<MemoryCardView>();
            view.ConfigureVisuals(
                borderImage,
                backImage,
                frontBgImage,
                frontImage,
                button,
                UiBorderCelebrationStyle.BaseBorderColor,
                UiBorderCelebrationStyle.AccentBorderColor);
            view.Bind(this, pairId, bowSprite);
            return view;
        }

        private void ApplyBoardLayout()
        {
            if (boardRoot == null || grid == null)
            {
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            CanvasScaler scaler = canvas != null ? canvas.GetComponent<CanvasScaler>() : null;
            float safeTop = SafeAreaInsets.GetTopInsetReferenceUnits(scaler);

            float top = BoardTopInsetReference + safeTop;
            float bottom = BoardBottomInsetReference
                + BoardBannerReserveReferenceUnits
                + BoardNativeSlotReserveReferenceUnits;

            boardRoot.anchorMin = Vector2.zero;
            boardRoot.anchorMax = Vector2.one;
            boardRoot.offsetMin = new Vector2(BoardPaddingReference, bottom);
            boardRoot.offsetMax = new Vector2(-BoardPaddingReference, -top);

            nativeAdSlot?.ApplyMemoryLayout(BoardPaddingReference);

            float width = boardRoot.rect.width;
            float height = boardRoot.rect.height;
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            float spacing = GridSpacingReference;
            float cellWidth = (width - spacing * (GridColumns - 1)) / GridColumns;
            float cellHeight = (height - spacing * (GridRows - 1)) / GridRows;
            float cell = Mathf.Floor(Mathf.Min(cellWidth, cellHeight));
            grid.cellSize = new Vector2(cell, cell);
        }

        private Sprite GetSpriteForPairId(int pairId)
        {
            if (IsAdPairId(pairId))
            {
                return GetAdSprite();
            }

            return GetBowSprite(pairId);
        }

        private static Sprite GetAdSprite()
        {
            if (cachedAdSprite != null)
            {
                return cachedAdSprite;
            }

            cachedAdSprite = Resources.Load<Sprite>(AdSpriteResourcePath);
            if (cachedAdSprite != null)
            {
                return cachedAdSprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(AdSpriteResourcePath);
            if (texture == null)
            {
                Debug.LogWarning($"Memory game: ad sprite missing at Resources/{AdSpriteResourcePath}.");
                return null;
            }

            cachedAdSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            return cachedAdSprite;
        }

        private void EnsureBowSpritesLoaded()
        {
            bowSpritesById ??= new Dictionary<int, Sprite>(BowPairCount);
            bowSpritesById.Clear();

            MemoryBowCatalog catalog = bowCatalog;
            if (catalog == null)
            {
                catalog = Resources.Load<MemoryBowCatalog>(MemoryBowCatalog.ResourcePath);
            }

            if (catalog != null)
            {
                for (int i = 1; i <= BowPairCount; i++)
                {
                    Sprite catalogSprite = catalog.GetBow(i);
                    if (catalogSprite != null)
                    {
                        bowSpritesById[i] = catalogSprite;
                    }
                }
            }

            Sprite[] folderSprites = Resources.LoadAll<Sprite>(BowResourceFolder);
            foreach (Sprite sprite in folderSprites)
            {
                if (sprite == null || !TryParseBowIndex(sprite.name, out int index))
                {
                    continue;
                }

                bowSpritesById[index] = sprite;
            }

            List<int> missing = new List<int>();
            for (int i = 1; i <= BowPairCount; i++)
            {
                if (bowSpritesById.ContainsKey(i))
                {
                    continue;
                }

                Sprite fallback = LoadBowSpriteFallback(i);
                if (fallback != null)
                {
                    bowSpritesById[i] = fallback;
                }
                else
                {
                    missing.Add(i);
                }
            }

            if (missing.Count > 0)
            {
                Debug.LogError(
                    $"Memory game: missing {missing.Count} bow sprite(s): {FormatBowIdList(missing)}. " +
                    "Run Dress Up Game → Relocate Memory Game Resources, then Generate Memory Bow Catalog.");
                return;
            }

            if (GetAdSprite() == null)
            {
                Debug.LogWarning($"Memory game: ad pair sprite missing at Resources/{AdSpriteResourcePath}.");
            }

            Debug.Log($"Memory game: loaded {BowPairCount}/{BowPairCount} bow sprites (+ {AdPairCount} ad pairs).");
        }

        private Sprite GetBowSprite(int index)
        {
            if (bowSpritesById != null && bowSpritesById.TryGetValue(index, out Sprite cached))
            {
                return cached;
            }

            return LoadBowSpriteFallback(index);
        }

        private static bool TryParseBowIndex(string spriteName, out int index)
        {
            index = 0;
            if (string.IsNullOrEmpty(spriteName) || !spriteName.StartsWith("bow_"))
            {
                return false;
            }

            return int.TryParse(spriteName.Substring(4), out index);
        }

        private static Sprite LoadBowSpriteFallback(int index)
        {
            string path = $"{BowResourceFolder}/bow_{index}";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }

            Object[] assets = Resources.LoadAll(path);
            foreach (Object asset in assets)
            {
                if (asset is Sprite assetSprite)
                {
                    return assetSprite;
                }
            }

            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        private static string FormatBowIdList(List<int> ids)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append("bow_");
                builder.Append(ids[i]);
            }

            return builder.ToString();
        }

        private static void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }
    }
}
