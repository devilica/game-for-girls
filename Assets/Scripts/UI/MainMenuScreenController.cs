using System.Collections;
using DressUpGame.Pet;
using DressUpGame.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Hub screen after splash: launch the dress-up game or show more games coming soon.
    /// </summary>
    public class MainMenuScreenController : MonoBehaviour
    {
        public const string MenuSceneName = "MenuScene";
        private const string MainSceneName = "MainScene";
        private const string PetSceneName = "PetScene";
        private const string GameButtonResourcePath = "UI/menu/game_1";
        private const string PetGameButtonResourcePath = "UI/menu/game_2";
        private const string MoreGamesResourcePath = "UI/menu/more_games";
        private const string BackgroundResourcePath = "UI/menu/background";
        private const string WelcomeResourcePath = "UI/menu/welcome";

        private const float WelcomeWidthReference = 920f;
        private const float WelcomeTopInsetReference = 32f;
        private const float GameCardWidthReference = 480f;
        private const float GameRowCardWidthReference = 340f;
        private const float GameRowSpacingReference = 20f;
        private const string GameCardsRowName = "GameCardsRow";
        private const float MoreGamesCardWidthReference = 560f;
        private const string MoreGamesRowName = "MoreGamesRow";
        private const float CardSpacingReference = 28f;
        private const float CardInnerPaddingReference = 0f;
        private const float MenuButtonsVerticalOffsetReference = -140f;
        private const float ColumnVerticalPaddingReference = 64f;

        private static readonly Color FrameColor = new Color(1f, 0.97f, 0.99f, 1f);
        private static readonly Color FrameShadowColor = new Color(0.92f, 0.45f, 0.68f, 0.45f);
        private static readonly Color DisabledFrameColor = new Color(1f, 0.98f, 1f, 0.92f);

        private bool isStartingGame;
        private bool isStartingPetGame;
        private bool isMoreGamesAdPending;

        private void Awake()
        {
            EnsureCamera();
            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();
            BuildMenu(canvas.transform);
        }

        private void Start()
        {
            MobileAdsInitializer.EnsureInitialized();
            RewardedAdController.EnsureExists();
            BottomBannerAdController.EnsureExists();
            BottomBannerAdController.LayoutInsetChanged += HandleMenuLayoutInsetChanged;

            ApplyMenuLayout();
            BottomBannerAdController.EnsureBannerActive();
            BottomBannerAdController.RefreshVisibility();

            Canvas canvas = FindAnyObjectByType<Canvas>();
            Transform soundOverlay = canvas != null
                ? canvas.transform.Find(SoundToggleButton.OverlayName)
                : null;
            soundOverlay?.SetAsLastSibling();

            StartCoroutine(ApplyMenuLayoutAfterFirstFrame());
            EnsureMenuCardsLayoutOnExistingMenu();
        }

        private void EnsureMenuCardsLayoutOnExistingMenu()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            Transform column = canvas.transform.Find("MainMenuRoot");
            if (column == null)
            {
                return;
            }

            Transform gamesRow = EnsureGameCardsRow(column);
            Transform playCard = column.Find("PlayGameCard");
            if (playCard == null)
            {
                playCard = gamesRow.Find("PlayGameCard");
            }

            if (playCard != null && playCard.parent != gamesRow)
            {
                playCard.SetParent(gamesRow, false);
            }

            Transform petCard = column.Find("PetGameCard") ?? gamesRow.Find("PetGameCard");
            Sprite petGameSprite = LoadMenuSprite(PetGameButtonResourcePath);

            if (petCard != null && petGameSprite != null && petCard.Find("CardContent/Art") == null)
            {
                Object.Destroy(petCard.gameObject);
                petCard = null;
            }

            if (playCard != null)
            {
                ResizeMenuCard(playCard, GameRowCardWidthReference);
                EnsurePlayHintBorder(playCard.gameObject, enabled: true);
            }

            if (petCard != null)
            {
                if (petCard.parent != gamesRow)
                {
                    petCard.SetParent(gamesRow, false);
                }

                ResizeMenuCard(petCard, GameRowCardWidthReference);
                EnsurePlayHintBorder(petCard.gameObject, enabled: false);
            }
            else if (playCard != null)
            {
                AddPetGameCard(gamesRow, petGameSprite);
                playCard.SetAsFirstSibling();
            }

            EnsureMoreGamesRow(column);
            ApplyMenuColumnWidth(column);

            Transform petCardFinal = gamesRow.Find("PetGameCard");
            if (playCard != null)
            {
                RewireMenuCardButton(playCard, StartDressUpGame);
            }

            if (petCardFinal != null)
            {
                RewireMenuCardButton(petCardFinal, StartPetGame);
            }

            Transform moreGamesCard = column.Find(MoreGamesRowName)?.Find("MoreGamesCard")
                ?? column.Find("MoreGamesCard");
            if (moreGamesCard != null)
            {
                RewireMenuCardButton(moreGamesCard, HandleMoreGamesClicked);
            }
        }

        private void RewireMenuCardButton(Transform cardRoot, UnityEngine.Events.UnityAction handler)
        {
            if (cardRoot == null || handler == null)
            {
                return;
            }

            Button button = cardRoot.Find("CardContent")?.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(handler);
        }

        private void AddPetGameCard(Transform gamesRow, Sprite petGameSprite)
        {
            if (petGameSprite != null)
            {
                CreateMenuCard(
                    gamesRow,
                    petGameSprite,
                    GameRowCardWidthReference,
                    true,
                    FrameColor,
                    StartPetGame,
                    showPlayHintBorder: false,
                    layoutHostName: "PetGameCard");
                return;
            }

            CreateTextMenuCard(
                gamesRow,
                "Pet Dress Up",
                "Game 2",
                GameRowCardWidthReference,
                StartPetGame);
        }

        private static Transform EnsureGameCardsRow(Transform column)
        {
            Transform existing = column.Find(GameCardsRowName);
            if (existing != null)
            {
                return existing;
            }

            GameObject rowGo = new GameObject(GameCardsRowName);
            rowGo.transform.SetParent(column, false);

            HorizontalLayoutGroup rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = GameRowSpacingReference;
            rowLayout.padding = new RectOffset(0, 0, 0, 0);
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            LayoutElement rowElement = rowGo.AddComponent<LayoutElement>();
            rowElement.preferredWidth = GameRowCardWidthReference * 2f + GameRowSpacingReference;

            Transform playCard = column.Find("PlayGameCard");
            if (playCard != null)
            {
                playCard.SetParent(rowGo.transform, false);
                playCard.SetAsFirstSibling();
            }

            return rowGo.transform;
        }

        private static Transform EnsureMoreGamesRow(Transform column)
        {
            Transform existing = column.Find(MoreGamesRowName);
            if (existing == null)
            {
                GameObject rowGo = new GameObject(MoreGamesRowName);
                rowGo.transform.SetParent(column, false);

                HorizontalLayoutGroup rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
                rowLayout.childAlignment = TextAnchor.MiddleCenter;
                rowLayout.spacing = 0f;
                rowLayout.padding = new RectOffset(0, 0, 0, 0);
                rowLayout.childControlWidth = true;
                rowLayout.childControlHeight = true;
                rowLayout.childForceExpandWidth = false;
                rowLayout.childForceExpandHeight = false;

                LayoutElement rowElement = rowGo.AddComponent<LayoutElement>();
                rowElement.preferredWidth = GetMenuColumnWidthReference();

                existing = rowGo.transform;
            }

            LayoutElement existingRowLayout = existing.GetComponent<LayoutElement>();
            if (existingRowLayout != null)
            {
                existingRowLayout.preferredWidth = GetMenuColumnWidthReference();
            }

            Transform moreGamesCard = column.Find("MoreGamesCard");
            if (moreGamesCard != null && moreGamesCard.parent != existing)
            {
                moreGamesCard.SetParent(existing, false);
            }

            moreGamesCard = existing.Find("MoreGamesCard");
            if (moreGamesCard != null)
            {
                ResizeMenuCard(moreGamesCard, MoreGamesCardWidthReference);
            }

            existing.SetSiblingIndex(column.Find(GameCardsRowName) != null ? 1 : 0);
            return existing;
        }

        private static float GetMenuColumnWidthReference()
        {
            float gameRowWidth = GameRowCardWidthReference * 2f + GameRowSpacingReference;
            return Mathf.Max(gameRowWidth, MoreGamesCardWidthReference);
        }

        private static void ApplyMenuColumnWidth(Transform column)
        {
            if (column is not RectTransform columnRect)
            {
                return;
            }

            columnRect.sizeDelta = new Vector2(GetMenuColumnWidthReference(), columnRect.sizeDelta.y);
        }

        private static void ResizeMenuCard(Transform card, float width)
        {
            LayoutElement layout = card.GetComponent<LayoutElement>();
            if (layout == null)
            {
                return;
            }

            layout.preferredWidth = width;
            Image art = card.Find("CardContent/Art")?.GetComponent<Image>();
            if (art != null && art.sprite != null)
            {
                float aspect = art.sprite.rect.height / art.sprite.rect.width;
                layout.preferredHeight = width * aspect;
            }
        }

        private static void EnsurePlayHintBorder(GameObject cardHost, bool enabled)
        {
            MenuCardPlayHintBorder hint = cardHost.GetComponent<MenuCardPlayHintBorder>();
            if (enabled)
            {
                if (hint == null)
                {
                    hint = cardHost.AddComponent<MenuCardPlayHintBorder>();
                    hint.Build();
                }

                return;
            }

            if (hint != null)
            {
                Object.Destroy(hint);
            }
        }

        private void HandleMenuLayoutInsetChanged()
        {
            ApplyMenuLayout();
        }

        private IEnumerator ApplyMenuLayoutAfterFirstFrame()
        {
            yield return null;
            ApplyMenuLayout();
            BottomBannerAdController.EnsureBannerActive();
            BottomBannerAdController.RefreshVisibility();

            Canvas canvas = FindAnyObjectByType<Canvas>();
            Transform soundOverlay = canvas != null
                ? canvas.transform.Find(SoundToggleButton.OverlayName)
                : null;
            soundOverlay?.SetAsLastSibling();
        }

        private void ApplyMenuLayout()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            float safeTop = SafeAreaInsets.GetTopInsetReferenceUnits(scaler);
            SoundToggleButton toggle = SoundToggleButton.EnsureOnCanvas(canvas.transform);
            toggle?.ApplyTopRightLayout(safeTop);

            Transform welcome = canvas.transform.Find("WelcomeBanner");
            if (welcome is RectTransform welcomeRect)
            {
                welcomeRect.anchoredPosition = new Vector2(0f, -WelcomeTopInsetReference - safeTop);
            }

            ApplyMenuColumnVerticalOffset(canvas.transform);
            ApplyMenuBottomInset();
        }

        private static void ApplyMenuColumnVerticalOffset(Transform canvasRoot)
        {
            Transform column = canvasRoot.Find("MainMenuRoot");
            if (column is not RectTransform columnRect)
            {
                return;
            }

            columnRect.anchoredPosition = new Vector2(
                columnRect.anchoredPosition.x,
                MenuButtonsVerticalOffsetReference);
        }

        private void OnDestroy()
        {
            BottomBannerAdController.LayoutInsetChanged -= HandleMenuLayoutInsetChanged;
        }

        private void ApplyMenuBottomInset()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            float safeBottom = SafeAreaInsets.GetBottomInsetReferenceUnits(scaler);
            int bottomPadding = Mathf.CeilToInt(ColumnVerticalPaddingReference + safeBottom);

            Transform column = canvas.transform.Find("MainMenuRoot");
            VerticalLayoutGroup columnLayout = column != null
                ? column.GetComponent<VerticalLayoutGroup>()
                : null;
            if (columnLayout != null)
            {
                columnLayout.padding = new RectOffset(
                    0,
                    0,
                    (int)ColumnVerticalPaddingReference,
                    bottomPadding);
            }
        }

        private static void EnsureCamera()
        {
            Camera camera = Camera.main ?? FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.98f, 0.82f, 0.90f, 1f);
            camera.orthographic = true;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static Canvas EnsureCanvas()
        {
            Canvas existing = FindAnyObjectByType<Canvas>();
            if (existing != null)
            {
                return existing;
            }

            GameObject canvasObject = new GameObject("MainMenuCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private void BuildMenu(Transform canvasRoot)
        {
            if (canvasRoot.Find("MainMenuRoot") != null)
            {
                return;
            }

            CreateFullScreenBackground(canvasRoot);

            CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
            float safeTop = SafeAreaInsets.GetTopInsetReferenceUnits(scaler);

            Sprite welcomeSprite = LoadMenuSprite(WelcomeResourcePath);
            if (welcomeSprite != null)
            {
                CreateTopWelcomeBanner(canvasRoot, welcomeSprite, safeTop);
            }

            GameObject columnGo = new GameObject("MainMenuRoot");
            columnGo.transform.SetParent(canvasRoot, false);

            RectTransform columnRect = columnGo.AddComponent<RectTransform>();
            columnRect.anchorMin = new Vector2(0.5f, 0.5f);
            columnRect.anchorMax = new Vector2(0.5f, 0.5f);
            columnRect.pivot = new Vector2(0.5f, 0.5f);
            columnRect.anchoredPosition = new Vector2(0f, MenuButtonsVerticalOffsetReference);
            columnRect.sizeDelta = new Vector2(GetMenuColumnWidthReference(), 0f);

            VerticalLayoutGroup layout = columnGo.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = CardSpacingReference;
            layout.padding = new RectOffset(0, 0, (int)ColumnVerticalPaddingReference, (int)ColumnVerticalPaddingReference);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            ContentSizeFitter columnFitter = columnGo.AddComponent<ContentSizeFitter>();
            columnFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            columnFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Sprite gameSprite = LoadMenuSprite(GameButtonResourcePath);
            Sprite petGameSprite = LoadMenuSprite(PetGameButtonResourcePath);
            Sprite moreGamesSprite = LoadMenuSprite(MoreGamesResourcePath);

            Transform gamesRow = EnsureGameCardsRow(columnGo.transform);

            if (gameSprite != null)
            {
                CreateMenuCard(
                    gamesRow,
                    gameSprite,
                    GameRowCardWidthReference,
                    true,
                    FrameColor,
                    StartDressUpGame,
                    showPlayHintBorder: true);
            }

            AddPetGameCard(gamesRow, petGameSprite);

            if (moreGamesSprite != null)
            {
                Transform moreGamesRow = EnsureMoreGamesRow(columnGo.transform);
                CreateMenuCard(
                    moreGamesRow,
                    moreGamesSprite,
                    MoreGamesCardWidthReference,
                    true,
                    FrameColor,
                    HandleMoreGamesClicked,
                    showPlayHintBorder: false);
            }
        }

        private void HandleMoreGamesClicked()
        {
            if (isMoreGamesAdPending || isStartingGame)
            {
                return;
            }

            StartCoroutine(ShowMoreGamesRewardedAdRoutine());
        }

        private IEnumerator ShowMoreGamesRewardedAdRoutine()
        {
            isMoreGamesAdPending = true;

            bool rewarded = false;
            yield return RewardedAdController.WaitForRewardRoutine(result => rewarded = result);

            GameplayPauseGuard.EnsureUnpaused();
            isMoreGamesAdPending = false;

            if (!rewarded)
            {
                Debug.Log("MainMenuScreenController: More games rewarded ad was not completed.");
            }
        }

        private static void CreateTopWelcomeBanner(Transform canvasRoot, Sprite sprite, float safeTop)
        {
            if (canvasRoot.Find("WelcomeBanner") != null)
            {
                return;
            }

            float width = WelcomeWidthReference;
            float aspect = sprite.rect.height / sprite.rect.width;
            float height = width * aspect;

            GameObject bannerGo = new GameObject("WelcomeBanner");
            bannerGo.transform.SetParent(canvasRoot, false);

            RectTransform bannerRect = bannerGo.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 1f);
            bannerRect.anchorMax = new Vector2(0.5f, 1f);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.sizeDelta = new Vector2(width, height);
            bannerRect.anchoredPosition = new Vector2(0f, -WelcomeTopInsetReference - safeTop);

            Image image = bannerGo.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private static void CreateFullScreenBackground(Transform canvasRoot)
        {
            if (canvasRoot.Find("MenuBackground") != null)
            {
                return;
            }

            GameObject backgroundGo = new GameObject("MenuBackground");
            backgroundGo.transform.SetParent(canvasRoot, false);
            backgroundGo.transform.SetAsFirstSibling();

            RectTransform backgroundRect = backgroundGo.AddComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            Image backgroundImage = backgroundGo.AddComponent<Image>();
            backgroundImage.sprite = LoadMenuSprite(BackgroundResourcePath);
            backgroundImage.preserveAspect = true;
            backgroundImage.color = Color.white;
            backgroundImage.raycastTarget = false;

            AspectRatioFitter fitter = backgroundGo.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            if (backgroundImage.sprite != null)
            {
                Rect spriteRect = backgroundImage.sprite.rect;
                fitter.aspectRatio = spriteRect.width / spriteRect.height;
            }
        }

        private static void CreateTextMenuCard(
            Transform parent,
            string title,
            string subtitle,
            float cardWidth,
            UnityEngine.Events.UnityAction onClick)
        {
            float cardHeight = cardWidth * (200f / GameCardWidthReference);

            GameObject layoutHost = new GameObject("PetGameCard");
            layoutHost.transform.SetParent(parent, false);

            LayoutElement layoutElement = layoutHost.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = cardWidth;
            layoutElement.preferredHeight = cardHeight;

            GameObject cardGo = new GameObject("CardContent");
            cardGo.transform.SetParent(layoutHost.transform, false);
            RectTransform cardRect = cardGo.AddComponent<RectTransform>();
            StretchRect(cardRect);

            GameObject shadowGo = new GameObject("FrameShadow");
            shadowGo.transform.SetParent(cardGo.transform, false);
            RectTransform shadowRect = shadowGo.AddComponent<RectTransform>();
            StretchRect(shadowRect);
            shadowRect.offsetMin = new Vector2(-6f, -10f);
            shadowRect.offsetMax = new Vector2(6f, 2f);
            Image shadowImage = shadowGo.AddComponent<Image>();
            shadowImage.sprite = UiRoundSpriteUtility.GetRoundedSquareSprite();
            shadowImage.color = FrameShadowColor;
            shadowImage.raycastTarget = false;

            Image frameImage = cardGo.AddComponent<Image>();
            frameImage.sprite = UiRoundSpriteUtility.GetRoundedSquareSprite();
            frameImage.color = Color.white;
            frameImage.raycastTarget = false;

            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(cardGo.transform, false);
            RectTransform titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.08f, 0.45f);
            titleRect.anchorMax = new Vector2(0.92f, 0.88f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            Text titleText = titleGo.AddComponent<Text>();
            titleText.text = title;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 40;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = new Color(0.35f, 0.15f, 0.35f);

            GameObject subtitleGo = new GameObject("Subtitle");
            subtitleGo.transform.SetParent(cardGo.transform, false);
            RectTransform subtitleRect = subtitleGo.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.08f, 0.12f);
            subtitleRect.anchorMax = new Vector2(0.92f, 0.42f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;
            Text subtitleText = subtitleGo.AddComponent<Text>();
            subtitleText.text = subtitle;
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subtitleText.fontSize = 28;
            subtitleText.color = new Color(0.55f, 0.25f, 0.45f);

            Button button = cardGo.AddComponent<Button>();
            button.targetGraphic = frameImage;
            button.onClick.AddListener(onClick);
        }

        private static void CreateMenuCard(
            Transform parent,
            Sprite artSprite,
            float cardWidth,
            bool interactable,
            Color frameColor,
            UnityEngine.Events.UnityAction onClick,
            bool showPlayHintBorder,
            string layoutHostName = null)
        {
            float aspect = artSprite.rect.height / artSprite.rect.width;
            float cardHeight = cardWidth * aspect;

            string cardName = layoutHostName ?? (interactable ? "PlayGameCard" : "MoreGamesCard");
            GameObject layoutHost = new GameObject(cardName);
            layoutHost.transform.SetParent(parent, false);

            LayoutElement layoutElement = layoutHost.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = cardWidth;
            layoutElement.preferredHeight = cardHeight;

            RectTransform hostRect = layoutHost.GetComponent<RectTransform>();
            hostRect.sizeDelta = new Vector2(cardWidth, cardHeight);

            if (showPlayHintBorder)
            {
                MenuCardPlayHintBorder hintBorder = layoutHost.AddComponent<MenuCardPlayHintBorder>();
                hintBorder.Build();
            }

            GameObject cardGo = new GameObject("CardContent");
            cardGo.transform.SetParent(layoutHost.transform, false);
            RectTransform cardRect = cardGo.AddComponent<RectTransform>();
            StretchRect(cardRect);

            GameObject shadowGo = new GameObject("FrameShadow");
            shadowGo.transform.SetParent(cardGo.transform, false);
            RectTransform shadowRect = shadowGo.AddComponent<RectTransform>();
            StretchRect(shadowRect);
            shadowRect.offsetMin = new Vector2(-6f, -10f);
            shadowRect.offsetMax = new Vector2(6f, 2f);
            Image shadowImage = shadowGo.AddComponent<Image>();
            shadowImage.sprite = UiRoundSpriteUtility.GetRoundedSquareSprite();
            shadowImage.type = Image.Type.Simple;
            shadowImage.color = FrameShadowColor;
            shadowImage.raycastTarget = false;

            Image frameImage = cardGo.AddComponent<Image>();
            frameImage.sprite = UiRoundSpriteUtility.GetRoundedSquareSprite();
            frameImage.type = Image.Type.Simple;
            frameImage.color = Color.white;
            frameImage.raycastTarget = false;

            Mask mask = cardGo.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject artGo = new GameObject("Art");
            artGo.transform.SetParent(cardGo.transform, false);
            RectTransform artRect = artGo.AddComponent<RectTransform>();
            StretchRect(artRect);

            Image artImage = artGo.AddComponent<Image>();
            artImage.sprite = artSprite;
            artImage.preserveAspect = false;
            artImage.type = Image.Type.Simple;
            artImage.color = Color.white;

            Button button = cardGo.AddComponent<Button>();
            button.targetGraphic = artImage;
            button.interactable = interactable;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
        }

        private static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Sprite LoadMenuSprite(string resourcePath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                return sprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                Debug.LogWarning($"MainMenuScreenController could not load sprite at Resources/{resourcePath}.");
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        private void StartDressUpGame()
        {
            if (isStartingGame || isStartingPetGame)
            {
                return;
            }

            CharacterSaveManager.ClearAllGameProgress();
            isStartingGame = true;
            StartCoroutine(ActivateMainSceneRoutine());
        }

        private void StartPetGame()
        {
            if (isStartingPetGame || isStartingGame)
            {
                return;
            }

            PetSaveManager.ClearAllPetProgress();
            isStartingPetGame = true;
            StartCoroutine(LoadGameplaySceneRoutine(PetSceneName, () => isStartingPetGame = false));
        }

        private IEnumerator ActivateMainSceneRoutine()
        {
            yield return LoadGameplaySceneRoutine(MainSceneName, () => isStartingGame = false);
        }

        private IEnumerator LoadGameplaySceneRoutine(string sceneName, System.Action onFailed)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (load == null)
            {
                Debug.LogError(
                    $"MainMenuScreenController could not load scene '{sceneName}'. " +
                    "Check File → Build Settings includes this scene.");
                onFailed?.Invoke();
                yield break;
            }

            load.allowSceneActivation = true;
            while (!load.isDone)
            {
                yield return null;
            }
        }
    }
}
