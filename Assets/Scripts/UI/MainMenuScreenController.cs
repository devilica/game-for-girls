using System.Collections;
using DressUpGame.MysteryBox;
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
        private const string MemorySceneName = "MemoryScene";
        private const string MysteryBoxSceneName = MysteryBoxController.MysteryBoxSceneName;
        private const string GameButtonResourcePath = "UI/menu/game_1";
        private const string PetGameButtonResourcePath = "UI/menu/game_2";
        private const string MemoryGameButtonResourcePath = "UI/menu/game_3";
        private const string MysteryBoxGameButtonResourcePath = "UI/menu/game_4";
        private const string MoreGamesResourcePath = "UI/menu/banner-more";
        private const string BackgroundResourcePath = "UI/menu/background";
        private const string WelcomeResourcePath = "UI/menu/welcome";

        private const float WelcomeWidthReference = 920f;
        private const float WelcomeTopInsetReference = 32f;
        private const float GameCardWidthReference = 480f;
        private const float GameRowCardWidthReference = 340f;
        private const float GameRowSpacingReference = 20f;
        private const string GameCardsRowName = "GameCardsRow";
        private const string MoreGamesRowName = "MoreGamesRow";
        private const string MoreGamesBannerRowName = "MoreGamesBannerRow";
        private const float MoreGamesBannerAspectHeightOverWidth = 9f / 16f;
        private const string MemoryGameCardName = "MemoryGameCard";
        private const string MysteryBoxGameCardName = "MysteryBoxGameCard";
        private const float CardSpacingReference = 28f;
        private const float CardInnerPaddingReference = 0f;
        private const float MenuButtonsVerticalOffsetReference = -140f;
        private const float ColumnVerticalPaddingReference = 64f;

        private static readonly Color FrameColor = new Color(1f, 0.97f, 0.99f, 1f);
        private static readonly Color FrameShadowColor = new Color(0.92f, 0.45f, 0.68f, 0.45f);
        private static readonly Color DisabledFrameColor = new Color(1f, 0.98f, 1f, 0.92f);

        private bool isStartingGame;
        private bool isStartingPetGame;
        private bool isStartingMemoryGame;
        private bool isStartingMysteryBoxGame;
        private bool isMoreGamesAdPending;
        private Coroutine deferredMenuLayoutRebuild;

        private static float? cachedStandardGameRowCardHeight;
        private static bool menuUiBuildInProgress;

        private void Awake()
        {
            EnsureCamera();
            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();
            PrepareStaleMainMenuTeardown(canvas.transform);

            if (menuUiBuildInProgress)
            {
                return;
            }

            menuUiBuildInProgress = true;
            try
            {
                BuildMenu(canvas.transform);
            }
            finally
            {
                menuUiBuildInProgress = false;
            }
        }

        /// <summary>
        /// Destroys and rebuilds menu UI (used after layout changes). Safe in Play Mode from editor menu.
        /// </summary>
        public void ForceReloadMenuLayout()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            ClearMainMenuUiForRebuild(canvas.transform, destroyAllRoots: true);
            menuUiBuildInProgress = true;
            try
            {
                BuildMenu(canvas.transform);
            }
            finally
            {
                menuUiBuildInProgress = false;
            }

            StampMenuLayoutVersion(canvas.transform);
            ApplyMenuLayout();
            EnsureMenuCardsLayoutOnExistingMenu();
            RebuildMainMenuLayoutImmediate(canvas.transform);
        }

        private static void StampMenuLayoutVersion(Transform canvasRoot)
        {
            Transform root = canvasRoot.Find("MainMenuRoot");
            if (MainMenuLayoutMarker.HasMoreGamesBanner(root))
            {
                MainMenuLayoutMarker.Stamp(root);
            }
        }

        private static void PrepareStaleMainMenuTeardown(Transform canvasRoot)
        {
            if (!canvasRoot)
            {
                return;
            }

            bool hasStaleRoot = false;
            for (int i = 0; i < canvasRoot.childCount; i++)
            {
                Transform child = canvasRoot.GetChild(i);
                if (!child || child.name != "MainMenuRoot")
                {
                    continue;
                }

                if (!MainMenuLayoutMarker.IsCurrent(child))
                {
                    hasStaleRoot = true;
                    break;
                }
            }

            if (hasStaleRoot)
            {
                ClearMainMenuUiForRebuild(canvasRoot, destroyAllRoots: true);
            }
        }

        private static void ClearMainMenuUiForRebuild(Transform canvasRoot, bool destroyAllRoots)
        {
            if (!canvasRoot)
            {
                return;
            }

            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            bool canvasWasEnabled = canvas != null && canvas.enabled;
            if (canvas != null)
            {
                canvas.enabled = false;
            }

            try
            {
                for (int i = canvasRoot.childCount - 1; i >= 0; i--)
                {
                    Transform child = canvasRoot.GetChild(i);
                    if (!child || child.name != "MainMenuRoot")
                    {
                        continue;
                    }

                    if (destroyAllRoots || !MainMenuLayoutMarker.IsCurrent(child))
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }

                Canvas.ForceUpdateCanvases();
            }
            finally
            {
                if (canvas != null)
                {
                    canvas.enabled = canvasWasEnabled;
                }
            }
        }

        private static void DestroyMenuUi(Transform canvasRoot, bool immediate = false)
        {
            if (immediate)
            {
                ClearMainMenuUiForRebuild(canvasRoot, destroyAllRoots: true);
                return;
            }

            Transform root = canvasRoot.Find("MainMenuRoot");
            if (!root)
            {
                return;
            }

            Object.Destroy(root.gameObject);
        }

        private static void RebuildMainMenuLayoutImmediate(Transform canvasRoot)
        {
            if (!canvasRoot)
            {
                return;
            }

            Transform column = canvasRoot.Find("MainMenuRoot");
            if (column is RectTransform columnRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(columnRect);
            }
        }

        private static Transform FindMenuCardDeep(Transform searchRoot, string cardName)
        {
            if (!searchRoot || string.IsNullOrEmpty(cardName))
            {
                return null;
            }

            if (searchRoot.name == cardName)
            {
                return searchRoot;
            }

            for (int i = 0; i < searchRoot.childCount; i++)
            {
                Transform child = searchRoot.GetChild(i);
                if (!child)
                {
                    continue;
                }

                Transform found = FindMenuCardDeep(child, cardName);
                if (found)
                {
                    return found;
                }
            }

            return null;
        }

        private static bool TryReparentMenuCard(Transform card, Transform parent)
        {
            if (!card || !parent || card.parent == parent)
            {
                return card && parent;
            }

            card.SetParent(parent, false);
            return true;
        }

        private static float GetStandardGameRowCardHeight()
        {
            if (cachedStandardGameRowCardHeight.HasValue)
            {
                return cachedStandardGameRowCardHeight.Value;
            }

            Sprite refSprite = LoadMenuSprite(GameButtonResourcePath);
            if (refSprite == null)
            {
                cachedStandardGameRowCardHeight = GameRowCardWidthReference;
                return cachedStandardGameRowCardHeight.Value;
            }

            float aspect = refSprite.rect.height / refSprite.rect.width;
            cachedStandardGameRowCardHeight = GameRowCardWidthReference * aspect;
            return cachedStandardGameRowCardHeight.Value;
        }

        private void Start()
        {
            MobileAdsInitializer.EnsureInitialized();
            BreakInterstitialAdController.EnsureExists();
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
            Transform playCard = FindMenuCardDeep(column, "PlayGameCard");

            TryReparentMenuCard(playCard, gamesRow);

            Transform petCard = FindMenuCardDeep(column, "PetGameCard");
            Sprite petGameSprite = LoadMenuSprite(PetGameButtonResourcePath);

            if (petCard && petGameSprite != null && petCard.Find("CardContent/Art") == null)
            {
                Object.Destroy(petCard.gameObject);
                petCard = null;
            }

            if (playCard)
            {
                TryReparentMenuCard(playCard, gamesRow);
                ResizeGameRowMenuCard(playCard);
                EnsurePlayHintBorder(playCard.gameObject, enabled: true);
            }

            if (petCard)
            {
                TryReparentMenuCard(petCard, gamesRow);
                ResizeGameRowMenuCard(petCard);
                EnsurePlayHintBorder(petCard.gameObject, enabled: false);
            }
            else if (playCard)
            {
                AddPetGameCard(gamesRow, petGameSprite);
                playCard.SetAsFirstSibling();
            }

            Transform secondaryRow = EnsureSecondaryGamesRow(column);
            Transform moreGamesBannerRow = EnsureMoreGamesBannerRow(column);
            if (secondaryRow == null)
            {
                EnsureMoreGamesBannerOnColumn(column);
                SetMenuRowOrder(column);
                ScheduleDeferredMenuLayoutRebuild(canvas.transform);
                return;
            }

            Transform memoryCardExisting = FindMenuCardDeep(column, MemoryGameCardName);
            TryReparentMenuCard(memoryCardExisting, secondaryRow);
            Sprite memoryMenuSprite = LoadMenuSprite(MemoryGameButtonResourcePath);
            if (memoryCardExisting == null && memoryMenuSprite != null)
            {
                CreateMenuCard(
                    secondaryRow,
                    memoryMenuSprite,
                    GameRowCardWidthReference,
                    true,
                    FrameColor,
                    StartMemoryGame,
                    showPlayHintBorder: false,
                    layoutHostName: MemoryGameCardName);
                memoryCardExisting = secondaryRow.Find(MemoryGameCardName);
                if (memoryCardExisting != null)
                {
                    memoryCardExisting.SetAsFirstSibling();
                }
            }

            Transform mysteryCardExisting = FindMenuCardDeep(column, MysteryBoxGameCardName);
            TryReparentMenuCard(mysteryCardExisting, secondaryRow);
            Sprite mysteryMenuSprite = LoadMenuSprite(MysteryBoxGameButtonResourcePath);
            if (mysteryCardExisting == null && mysteryMenuSprite != null)
            {
                CreateMenuCard(
                    secondaryRow,
                    mysteryMenuSprite,
                    GameRowCardWidthReference,
                    true,
                    FrameColor,
                    StartMysteryBoxGame,
                    showPlayHintBorder: false,
                    layoutHostName: MysteryBoxGameCardName);
            }

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

            memoryCardExisting = secondaryRow?.Find(MemoryGameCardName);
            if (memoryCardExisting != null)
            {
                RewireMenuCardButton(memoryCardExisting, StartMemoryGame);
            }

            mysteryCardExisting = secondaryRow?.Find(MysteryBoxGameCardName);
            if (mysteryCardExisting != null)
            {
                RewireMenuCardButton(mysteryCardExisting, StartMysteryBoxGame);
            }

            if (memoryCardExisting != null)
            {
                ResizeGameRowMenuCard(memoryCardExisting);
            }

            if (mysteryCardExisting != null)
            {
                ResizeGameRowMenuCard(mysteryCardExisting);
            }

            EnsureMoreGamesBannerOnColumn(column);

            SetMenuRowOrder(column);
            ScheduleDeferredMenuLayoutRebuild(canvas.transform);
        }

        private void EnsureMoreGamesBannerPresent()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            Transform column = canvas.transform.Find("MainMenuRoot");
            if (!column)
            {
                return;
            }

            EnsureMoreGamesBannerOnColumn(column);
        }

        private void EnsureMoreGamesBannerOnColumn(Transform column)
        {
            if (!column)
            {
                return;
            }

            Transform moreGamesBannerRow = EnsureMoreGamesBannerRow(column);
            Transform moreGamesCard = moreGamesBannerRow.Find("MoreGamesCard");
            if (!moreGamesCard)
            {
                moreGamesCard = FindMenuCardDeep(column, "MoreGamesCard");
            }

            TryReparentMenuCard(moreGamesCard, moreGamesBannerRow);

            Sprite moreGamesSprite = LoadMenuSprite(MoreGamesResourcePath);
            if (!moreGamesSprite)
            {
                Debug.LogError(
                    "MainMenuScreenController: More Games banner sprite missing at Resources/UI/menu/banner-more. " +
                    "Run Dress Up Game → Fix Menu Button Sprites, then Reload Main Menu Layout.");
                return;
            }

            if (!moreGamesCard)
            {
                CreateMoreGamesBannerCard(moreGamesBannerRow, moreGamesSprite, HandleMoreGamesClicked);
                moreGamesCard = moreGamesBannerRow.Find("MoreGamesCard");
            }

            if (!moreGamesCard)
            {
                return;
            }

            TryReparentMenuCard(moreGamesCard, moreGamesBannerRow);
            UpgradeMoreGamesBannerCard(moreGamesCard, moreGamesSprite, HandleMoreGamesClicked);
            ResizeMoreGamesBanner(moreGamesCard);
            RewireMenuCardButton(moreGamesCard, HandleMoreGamesClicked);
        }

        private void ScheduleDeferredMenuLayoutRebuild(Transform canvasRoot)
        {
            if (canvasRoot == null)
            {
                return;
            }

            if (deferredMenuLayoutRebuild != null)
            {
                StopCoroutine(deferredMenuLayoutRebuild);
            }

            deferredMenuLayoutRebuild = StartCoroutine(DeferredMenuLayoutRebuildRoutine(canvasRoot));
        }

        private IEnumerator DeferredMenuLayoutRebuildRoutine(Transform canvasRoot)
        {
            yield return null;
            deferredMenuLayoutRebuild = null;
            if (canvasRoot != null)
            {
                RebuildMainMenuLayoutImmediate(canvasRoot);
            }
        }

        private void RewireMenuCardButton(Transform cardRoot, UnityEngine.Events.UnityAction handler)
        {
            if (cardRoot == null || handler == null)
            {
                return;
            }

            Button button = cardRoot.Find("BannerArt")?.GetComponent<Button>();
            if (button == null)
            {
                button = cardRoot.Find("CardContent")?.GetComponent<Button>();
            }

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

            Transform playCard = FindMenuCardDeep(column, "PlayGameCard");
            if (playCard != null)
            {
                playCard.SetParent(rowGo.transform, false);
                playCard.SetAsFirstSibling();
            }

            return rowGo.transform;
        }

        private static Transform EnsureSecondaryGamesRow(Transform column)
        {
            if (!column)
            {
                return null;
            }

            Transform existing = column.Find(MoreGamesRowName);
            if (!existing)
            {
                GameObject rowGo = new GameObject(MoreGamesRowName);
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

                existing = rowGo.transform;
            }

            HorizontalLayoutGroup layout = existing.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = GameRowSpacingReference;
            }

            Transform memoryCard = FindMenuCardDeep(column, MemoryGameCardName);
            if (memoryCard != null && memoryCard.parent != existing)
            {
                memoryCard.SetParent(existing, false);
            }

            if (memoryCard != null)
            {
                ResizeGameRowMenuCard(memoryCard);
            }

            Transform mysteryCard = FindMenuCardDeep(column, MysteryBoxGameCardName);
            if (mysteryCard != null && mysteryCard.parent != existing)
            {
                mysteryCard.SetParent(existing, false);
            }

            if (mysteryCard != null)
            {
                ResizeGameRowMenuCard(mysteryCard);
            }

            if (!existing)
            {
                return null;
            }

            LayoutElement existingRowLayoutElement = existing.GetComponent<LayoutElement>();
            if (existingRowLayoutElement != null)
            {
                existingRowLayoutElement.preferredWidth = GameRowCardWidthReference * 2f + GameRowSpacingReference;
            }

            return existing;
        }

        private static Transform EnsureMoreGamesBannerRow(Transform column)
        {
            if (!column)
            {
                return null;
            }

            Transform existing = column.Find(MoreGamesBannerRowName);
            if (!existing)
            {
                GameObject rowGo = new GameObject(MoreGamesBannerRowName);
                rowGo.transform.SetParent(column, false);
                existing = rowGo.transform;
            }

            ConfigureMoreGamesBannerRow(column, existing);
            return existing;
        }

        /// <summary>
        /// Banner row must report real height to the menu column; otherwise the card draws over rows above.
        /// </summary>
        private static void ConfigureMoreGamesBannerRow(Transform column, Transform row)
        {
            if (!row)
            {
                return;
            }

            GameObject rowGo = row.gameObject;
            if (!rowGo)
            {
                return;
            }

            VerticalLayoutGroup columnLayout = column != null ? column.GetComponent<VerticalLayoutGroup>() : null;
            ContentSizeFitter columnFitter = column != null ? column.GetComponent<ContentSizeFitter>() : null;
            bool columnLayoutWasEnabled = columnLayout != null && columnLayout.enabled;
            bool columnFitterWasEnabled = columnFitter != null && columnFitter.enabled;
            if (columnLayout != null)
            {
                columnLayout.enabled = false;
            }

            if (columnFitter != null)
            {
                columnFitter.enabled = false;
            }

            try
            {
                float width = GetMenuColumnWidthReference();
                LayoutElement rowElement = rowGo.GetComponent<LayoutElement>();
                if (rowElement == null)
                {
                    rowElement = rowGo.AddComponent<LayoutElement>();
                }

                rowElement.preferredWidth = width;
                rowElement.minWidth = width;
                rowElement.flexibleWidth = 0f;
                rowElement.flexibleHeight = 0f;

                VerticalLayoutGroup rowLayout = rowGo.GetComponent<VerticalLayoutGroup>();
                if (rowLayout == null)
                {
                    rowLayout = rowGo.AddComponent<VerticalLayoutGroup>();
                }

                rowLayout.childAlignment = TextAnchor.MiddleCenter;
                rowLayout.spacing = 0f;
                rowLayout.padding = new RectOffset(0, 0, 0, 0);
                rowLayout.childControlWidth = true;
                rowLayout.childControlHeight = true;
                rowLayout.childForceExpandWidth = false;
                rowLayout.childForceExpandHeight = false;

                ContentSizeFitter fitter = rowGo.GetComponent<ContentSizeFitter>();
                if (fitter == null)
                {
                    fitter = rowGo.AddComponent<ContentSizeFitter>();
                }

                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            finally
            {
                if (columnLayout != null)
                {
                    columnLayout.enabled = columnLayoutWasEnabled;
                }

                if (columnFitter != null)
                {
                    columnFitter.enabled = columnFitterWasEnabled;
                }
            }
        }

        private static void SetMenuRowOrder(Transform column)
        {
            Transform gamesRow = column.Find(GameCardsRowName);
            Transform extraRow = column.Find(MoreGamesRowName);
            Transform bannerRow = column.Find(MoreGamesBannerRowName);
            int index = 0;
            if (gamesRow != null)
            {
                gamesRow.SetSiblingIndex(index++);
            }

            if (extraRow != null)
            {
                extraRow.SetSiblingIndex(index++);
            }

            if (bannerRow != null)
            {
                bannerRow.SetSiblingIndex(index);
            }
        }

        private static void ResizeMoreGamesBanner(Transform card)
        {
            if (card == null)
            {
                return;
            }

            float width = GetMenuColumnWidthReference();
            Image art = GetMoreGamesBannerArt(card);
            float height = GetMoreGamesBannerHeight(width, art != null ? art.sprite : null);
            LayoutElement layout = card.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = card.gameObject.AddComponent<LayoutElement>();
            }

            layout.preferredWidth = width;
            layout.preferredHeight = height;
            layout.minWidth = width;
            layout.minHeight = height;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

            if (card is RectTransform cardRect)
            {
                cardRect.sizeDelta = new Vector2(width, height);
            }

            if (art != null)
            {
                art.preserveAspect = true;
            }

            Transform bannerRow = card.parent;
            if (bannerRow != null && bannerRow.name == MoreGamesBannerRowName)
            {
                ConfigureMoreGamesBannerRow(bannerRow.parent, bannerRow);
            }

            MarkMainMenuLayoutDirty(card);
        }

        private static void MarkMainMenuLayoutDirty(Transform fromCard)
        {
            if (!fromCard)
            {
                return;
            }

            Transform column = fromCard;
            while (column)
            {
                if (column.name == "MainMenuRoot")
                {
                    if (column is RectTransform columnRect)
                    {
                        LayoutRebuilder.MarkLayoutForRebuild(columnRect);
                    }

                    return;
                }

                Transform parent = column.parent;
                if (!parent)
                {
                    return;
                }

                column = parent;
            }
        }

        private static float GetMoreGamesBannerHeight(float width, Sprite sprite)
        {
            float maxHeight = width * MoreGamesBannerAspectHeightOverWidth;
            if (sprite == null)
            {
                return maxHeight;
            }

            float naturalHeight = width * (sprite.rect.height / sprite.rect.width);
            return Mathf.Min(naturalHeight, maxHeight);
        }

        private static Image GetMoreGamesBannerArt(Transform card)
        {
            if (card == null)
            {
                return null;
            }

            Transform bannerArt = card.Find("BannerArt");
            if (bannerArt != null)
            {
                return bannerArt.GetComponent<Image>();
            }

            return card.Find("CardContent/Art")?.GetComponent<Image>();
        }

        private static void UpgradeMoreGamesBannerCard(
            Transform card,
            Sprite sprite,
            UnityEngine.Events.UnityAction onClick)
        {
            if (!card)
            {
                return;
            }

            if (card.Find("BannerArt") != null)
            {
                Image existingArt = GetMoreGamesBannerArt(card);
                if (existingArt != null && sprite != null)
                {
                    existingArt.sprite = sprite;
                    existingArt.preserveAspect = true;
                }

                return;
            }

            Transform legacyContent = card.Find("CardContent");
            if (legacyContent != null)
            {
                legacyContent.SetParent(null, false);
                Object.Destroy(legacyContent.gameObject);
            }

            if (!card)
            {
                return;
            }

            BuildMoreGamesBannerContent(card, sprite, onClick);
        }

        private static float GetMenuColumnWidthReference()
        {
            return GameRowCardWidthReference * 2f + GameRowSpacingReference;
        }

        private static void ApplyMenuColumnWidth(Transform column)
        {
            if (column is not RectTransform columnRect)
            {
                return;
            }

            columnRect.sizeDelta = new Vector2(GetMenuColumnWidthReference(), columnRect.sizeDelta.y);
        }

        private static void ResizeGameRowMenuCard(Transform card)
        {
            LayoutElement layout = card.GetComponent<LayoutElement>();
            if (layout == null)
            {
                return;
            }

            layout.preferredWidth = GameRowCardWidthReference;
            layout.preferredHeight = GetStandardGameRowCardHeight();

            Image art = card.Find("CardContent/Art")?.GetComponent<Image>();
            if (art != null)
            {
                art.preserveAspect = true;
            }
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
                }

                hint.Build();
                return;
            }

            if (hint != null)
            {
                Object.Destroy(hint);
            }
        }

        private void HandleMenuLayoutInsetChanged()
        {
            if (SceneManager.GetActiveScene().name != MenuSceneName)
            {
                return;
            }

            ApplyMenuLayout();
        }

        private IEnumerator ApplyMenuLayoutAfterFirstFrame()
        {
            yield return null;
            ApplyMenuLayout();
            EnsureMenuCardsLayoutOnExistingMenu();
            EnsureMoreGamesBannerPresent();
            Canvas canvasForStamp = FindAnyObjectByType<Canvas>();
            if (canvasForStamp != null)
            {
                StampMenuLayoutVersion(canvasForStamp.transform);
            }

            BottomBannerAdController.EnsureBannerActive();
            BottomBannerAdController.RefreshVisibility();

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                RebuildMainMenuLayoutImmediate(canvas.transform);
            }

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
            if (deferredMenuLayoutRebuild != null)
            {
                StopCoroutine(deferredMenuLayoutRebuild);
                deferredMenuLayoutRebuild = null;
            }
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
            Transform existingRoot = canvasRoot.Find("MainMenuRoot");
            if (existingRoot != null)
            {
                if (MainMenuLayoutMarker.IsCurrent(existingRoot))
                {
                    return;
                }

                ClearMainMenuUiForRebuild(canvasRoot, destroyAllRoots: true);
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
            Sprite memoryGameSprite = LoadMenuSprite(MemoryGameButtonResourcePath);
            Sprite mysteryBoxGameSprite = LoadMenuSprite(MysteryBoxGameButtonResourcePath);
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

            Transform secondaryGamesRow = EnsureSecondaryGamesRow(columnGo.transform);

            if (memoryGameSprite != null)
            {
                CreateMenuCard(
                    secondaryGamesRow,
                    memoryGameSprite,
                    GameRowCardWidthReference,
                    true,
                    FrameColor,
                    StartMemoryGame,
                    showPlayHintBorder: false,
                    layoutHostName: MemoryGameCardName);
            }

            if (mysteryBoxGameSprite != null)
            {
                CreateMenuCard(
                    secondaryGamesRow,
                    mysteryBoxGameSprite,
                    GameRowCardWidthReference,
                    true,
                    FrameColor,
                    StartMysteryBoxGame,
                    showPlayHintBorder: false,
                    layoutHostName: MysteryBoxGameCardName);
            }

            Transform moreGamesBannerRow = EnsureMoreGamesBannerRow(columnGo.transform);

            if (moreGamesBannerRow != null && moreGamesSprite != null)
            {
                CreateMoreGamesBannerCard(moreGamesBannerRow, moreGamesSprite, HandleMoreGamesClicked);
            }
            else
            {
                Debug.LogWarning(
                    "MainMenuScreenController: More Games banner sprite missing at Resources/UI/menu/banner-more.");
            }

            if (memoryGameSprite == null)
            {
                Debug.LogWarning(
                    "MainMenuScreenController: Memory (game 3) button sprite missing at Resources/UI/menu/game_3.");
            }

            if (mysteryBoxGameSprite == null)
            {
                Debug.LogWarning(
                    "MainMenuScreenController: Mystery Box (game 4) button sprite missing at Resources/UI/menu/game_4.");
            }

            SetMenuRowOrder(columnGo.transform);
        }

        private void HandleMoreGamesClicked()
        {
            if (isMoreGamesAdPending || isStartingGame || isStartingMemoryGame || isStartingMysteryBoxGame)
            {
                return;
            }

            StartCoroutine(ShowMoreGamesInterstitialRoutine());
        }

        private IEnumerator ShowMoreGamesInterstitialRoutine()
        {
            isMoreGamesAdPending = true;

            yield return BreakInterstitialAdController.WaitForBreakAdRoutine();

            GameplayPauseGuard.EnsureUnpaused();
            isMoreGamesAdPending = false;
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

        private static void CreateMoreGamesBannerCard(
            Transform parent,
            Sprite artSprite,
            UnityEngine.Events.UnityAction onClick)
        {
            float width = GetMenuColumnWidthReference();
            float height = GetMoreGamesBannerHeight(width, artSprite);

            GameObject layoutHost = new GameObject("MoreGamesCard");
            layoutHost.transform.SetParent(parent, false);

            LayoutElement layoutElement = layoutHost.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = width;
            layoutElement.preferredHeight = height;
            layoutElement.minWidth = width;
            layoutElement.minHeight = height;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            RectTransform hostRect = layoutHost.GetComponent<RectTransform>();
            hostRect.sizeDelta = new Vector2(width, height);

            BuildMoreGamesBannerContent(layoutHost.transform, artSprite, onClick);
        }

        /// <summary>
        /// Wide menu banner: the sprite is the button graphic (no card frame/mask).
        /// </summary>
        private static void BuildMoreGamesBannerContent(
            Transform layoutHost,
            Sprite artSprite,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject artGo = new GameObject("BannerArt");
            artGo.transform.SetParent(layoutHost, false);
            RectTransform artRect = artGo.AddComponent<RectTransform>();
            StretchRect(artRect);

            Image artImage = artGo.AddComponent<Image>();
            artImage.sprite = artSprite;
            artImage.preserveAspect = true;
            artImage.type = Image.Type.Simple;
            artImage.color = Color.white;
            artImage.raycastTarget = true;

            Button button = artGo.AddComponent<Button>();
            button.targetGraphic = artImage;
            button.interactable = true;
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
            bool isGameRowCard = Mathf.Approximately(cardWidth, GameRowCardWidthReference);
            float cardHeight = isGameRowCard ? GetStandardGameRowCardHeight() : cardWidth * aspect;

            string cardName = layoutHostName ?? (interactable ? "PlayGameCard" : "MoreGamesCard");
            GameObject layoutHost = new GameObject(cardName);
            layoutHost.transform.SetParent(parent, false);

            LayoutElement layoutElement = layoutHost.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = cardWidth;
            layoutElement.preferredHeight = cardHeight;

            RectTransform hostRect = layoutHost.GetComponent<RectTransform>();
            hostRect.sizeDelta = new Vector2(cardWidth, cardHeight);

            BuildMenuCardContent(
                layoutHost.transform,
                artSprite,
                interactable,
                onClick,
                preserveAspect: isGameRowCard);

            if (showPlayHintBorder)
            {
                MenuCardPlayHintBorder hintBorder = layoutHost.AddComponent<MenuCardPlayHintBorder>();
                hintBorder.Build();
            }
        }

        private static void BuildMenuCardContent(
            Transform layoutHost,
            Sprite artSprite,
            bool interactable,
            UnityEngine.Events.UnityAction onClick,
            bool preserveAspect)
        {
            GameObject cardGo = new GameObject("CardContent");
            cardGo.transform.SetParent(layoutHost, false);
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
            artImage.preserveAspect = preserveAspect;
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

            Object[] subAssets = Resources.LoadAll(resourcePath);
            if (subAssets != null)
            {
                for (int i = 0; i < subAssets.Length; i++)
                {
                    if (subAssets[i] is Sprite subSprite)
                    {
                        return subSprite;
                    }
                }
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
            if (isStartingGame || isStartingPetGame || isStartingMemoryGame || isStartingMysteryBoxGame)
            {
                return;
            }

            CharacterSaveManager.ClearAllGameProgress();
            isStartingGame = true;
            StartCoroutine(ActivateMainSceneRoutine());
        }

        private void StartPetGame()
        {
            if (isStartingPetGame || isStartingGame || isStartingMemoryGame || isStartingMysteryBoxGame)
            {
                return;
            }

            PetSaveManager.ClearAllPetProgress();
            isStartingPetGame = true;
            StartCoroutine(LoadGameplaySceneRoutine(PetSceneName, () => isStartingPetGame = false));
        }

        private void StartMemoryGame()
        {
            if (isStartingMemoryGame || isStartingGame || isStartingPetGame || isStartingMysteryBoxGame)
            {
                return;
            }

            isStartingMemoryGame = true;
            StartCoroutine(LoadGameplaySceneRoutine(MemorySceneName, () => isStartingMemoryGame = false));
        }

        private void StartMysteryBoxGame()
        {
            if (isStartingMysteryBoxGame || isStartingGame || isStartingPetGame || isStartingMemoryGame)
            {
                return;
            }

            isStartingMysteryBoxGame = true;
            StartCoroutine(LoadGameplaySceneRoutine(MysteryBoxSceneName, () => isStartingMysteryBoxGame = false));
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
