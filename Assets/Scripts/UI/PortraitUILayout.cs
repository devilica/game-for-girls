using DressUpGame.Character;
using DressUpGame.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Forces portrait-friendly UI anchors at runtime so panels stay at top/bottom
    /// and do not cover the character in the middle.
    /// </summary>
    [ExecuteAlways]
    public class PortraitUILayout : MonoBehaviour
    {
        private const float WizardPanelTopInset = 680f;
        private const float DonePanelMinimumSafeBottom = 96f;
        private const float DonePanelBottomPadding = 48f;
        private const float DonePanelHeight = 980f;
        private const float DonePanelTextHeight = 64f;
        private const float DonePanelButtonSpacing = 20f;
        private const float DoneViewExtraBottomClearance = 32f;
        private const float DoneRowHorizontalInset = 16f;
        private const float DoneBannerPadding = 20f;
        private const float ArrowHorizontalInset = 24f;
        private static readonly Color CardPanelBackgroundColor = new Color(0.98f, 0.94f, 0.98f, 1f);
        private static readonly Color CardScrollOverlayColor = new Color(1f, 1f, 1f, 0.15f);
        private void Awake()
        {
            BottomBannerAdController.LayoutInsetChanged += HandleLayoutInsetChanged;
            Apply();
        }

        private void OnDestroy()
        {
            BottomBannerAdController.LayoutInsetChanged -= HandleLayoutInsetChanged;
        }

        private void HandleLayoutInsetChanged()
        {
            if (IsDoneViewActive())
            {
                CharacterCustomizer customizer = FindAnyObjectByType<CharacterCustomizer>();
                customizer?.RefreshDisplayScaleForDoneView();
                return;
            }

            Apply();
        }

        private bool IsDoneViewActive()
        {
            Transform donePanel = transform.Find("DonePanel");
            return donePanel != null && donePanel.gameObject.activeInHierarchy;
        }

        private void Start()
        {
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        public void Apply()
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                return;
            }

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 1f;
            }

            Transform characterArea = transform.Find("CharacterArea");
            if (characterArea != null)
            {
                characterArea.gameObject.SetActive(false);
            }

            float bottomUiInset = SafeAreaInsets.GetBottomUiInset(scaler);
            float doneSafeBottom = SafeAreaInsets.GetEffectiveBottomInset(scaler, DonePanelMinimumSafeBottom)
                + DonePanelBottomPadding
                + BottomBannerAdController.LayoutInsetReferenceUnits;
            float safeTop = SafeAreaInsets.GetTopInsetReferenceUnits(scaler);

            Transform topBar = transform.Find("TopBar");
            if (topBar != null)
            {
                topBar.gameObject.SetActive(false);
            }
            StretchPanel("CategoryBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 8f + bottomUiInset), new Vector2(-8f, 148f + bottomUiInset));
            StretchPanel("ItemsPanel", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 160f + bottomUiInset), new Vector2(-8f, 460f + bottomUiInset));
            StretchPanel("WizardPanel", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 8f + bottomUiInset), new Vector2(-8f, WizardPanelTopInset + bottomUiInset));
            StretchPanel("DonePanel", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(24f, doneSafeBottom),
                new Vector2(-24f, DonePanelHeight + doneSafeBottom));

            Transform itemsPanel = transform.Find("ItemsPanel");
            if (itemsPanel != null)
            {
                EnsureItemsPanelLayout(itemsPanel);
                ApplyClassicCardPanelBackground(itemsPanel, "ItemsScroll");
            }

            Transform wizardPanel = transform.Find("WizardPanel");
            if (wizardPanel != null)
            {
                EnsureWizardPanelLayout(wizardPanel);
                ApplyClassicCardPanelBackground(wizardPanel, "WizardItemsScroll");
            }

            Transform donePanel = transform.Find("DonePanel");
            if (donePanel != null)
            {
                EnsureDonePanelLayout(donePanel);
            }

            PositionWizardNavOverlay(bottomUiInset);
            PositionTopBarButtons(safeTop);

            Transform soundOverlay = transform.Find(SoundToggleButton.OverlayName);
            soundOverlay?.SetAsLastSibling();

            BottomBannerAdController.RefreshVisibility();
        }

        public void ApplyForDoneView()
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                return;
            }

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 1f;
            }

            Transform characterArea = transform.Find("CharacterArea");
            if (characterArea != null)
            {
                characterArea.gameObject.SetActive(false);
            }

            float doneSafeBottom = GetDoneViewBottomInset(scaler);
            float safeTop = SafeAreaInsets.GetTopInsetReferenceUnits(scaler);

            Transform topBar = transform.Find("TopBar");
            if (topBar != null)
            {
                topBar.gameObject.SetActive(false);
            }

            StretchPanel("DonePanel", new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(24f, doneSafeBottom),
                new Vector2(-24f, -safeTop));

            Transform donePanel = transform.Find("DonePanel");
            if (donePanel != null)
            {
                EnsureDonePanelLayoutForDoneView(donePanel, scaler);
            }

            PositionTopBarButtons(safeTop);

            Transform soundOverlay = transform.Find(SoundToggleButton.OverlayName);
            soundOverlay?.SetAsLastSibling();

            BottomBannerAdController.RefreshVisibility();
        }

        private void StretchPanel(string panelName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            Transform panel = transform.Find(panelName);
            if (panel == null)
            {
                return;
            }

            RectTransform rect = panel.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, anchorMin.y >= 0.5f ? 1f : 0f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.anchoredPosition3D = Vector3.zero;
        }

        private static void EnsureItemsPanelLayout(Transform itemsPanel)
        {
            VerticalLayoutGroup vertical = itemsPanel.GetComponent<VerticalLayoutGroup>();
            if (vertical == null)
            {
                vertical = itemsPanel.gameObject.AddComponent<VerticalLayoutGroup>();
                vertical.childAlignment = TextAnchor.UpperCenter;
                vertical.spacing = 8f;
                vertical.padding = new RectOffset(12, 12, 12, 12);
                vertical.childForceExpandWidth = true;
                vertical.childForceExpandHeight = false;
                vertical.childControlWidth = true;
                vertical.childControlHeight = true;
            }

            Transform makeupBar = itemsPanel.Find("MakeupSubBar");
            if (makeupBar != null)
            {
                EnsureRowLayout(makeupBar, 72f);
            }

            Transform hairRow = itemsPanel.Find("HairColorRow");
            if (hairRow != null)
            {
                EnsureRowLayout(hairRow, 96f);
            }

            Transform itemsScroll = itemsPanel.Find("ItemsScroll");
            if (itemsScroll != null)
            {
                LayoutElement layout = itemsScroll.GetComponent<LayoutElement>();
                if (layout == null)
                {
                    layout = itemsScroll.gameObject.AddComponent<LayoutElement>();
                }

                layout.minHeight = 180f;
                layout.flexibleHeight = 1f;

                RectTransform scrollRect = itemsScroll.GetComponent<RectTransform>();
                scrollRect.anchorMin = Vector2.zero;
                scrollRect.anchorMax = Vector2.one;
                scrollRect.offsetMin = Vector2.zero;
                scrollRect.offsetMax = Vector2.zero;
            }
        }

        private static float GetDoneViewBottomInset(CanvasScaler scaler)
        {
            return SafeAreaInsets.GetEffectiveBottomInset(scaler, DonePanelMinimumSafeBottom)
                + DonePanelBottomPadding
                + DoneViewExtraBottomClearance
                + BottomBannerAdController.LayoutInsetReferenceUnits;
        }

        private static void ApplyClassicCardPanelBackground(Transform panel, string scrollChildName)
        {
            Image panelImage = panel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = null;
                panelImage.material = null;
                panelImage.type = Image.Type.Simple;
                panelImage.color = CardPanelBackgroundColor;
                panelImage.raycastTarget = true;
            }

            Transform scroll = panel.Find(scrollChildName);
            if (scroll == null)
            {
                return;
            }

            Image scrollImage = scroll.GetComponent<Image>();
            if (scrollImage == null)
            {
                return;
            }

            scrollImage.sprite = null;
            scrollImage.material = null;
            scrollImage.type = Image.Type.Simple;
            scrollImage.color = CardScrollOverlayColor;
        }

        private static void EnsureRowLayout(Transform row, float height)
        {
            LayoutElement layout = row.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = row.gameObject.AddComponent<LayoutElement>();
            }

            layout.preferredHeight = height;
            layout.minHeight = height;
        }

        private static void EnsureDonePanelLayout(Transform donePanel)
        {
            Image panelBackground = donePanel.GetComponent<Image>();
            if (panelBackground != null)
            {
                panelBackground.color = Color.clear;
                panelBackground.raycastTarget = false;
            }
        }

        private void EnsureDonePanelLayoutForDoneView(Transform donePanel, CanvasScaler scaler)
        {
            EnsureDonePanelLayout(donePanel);

            Transform doneText = donePanel.Find("DoneText");
            if (doneText != null)
            {
                doneText.gameObject.SetActive(false);
            }

            Transform buttonRow = donePanel.Find("DoneButtonRow");
            if (buttonRow == null)
            {
                return;
            }

            EnsureHorizontalDoneButtonRow(buttonRow);
            PositionDoneButtonRowUnderLegs(donePanel, buttonRow, scaler);
        }

        private static void EnsureHorizontalDoneButtonRow(Transform buttonRow)
        {
            VerticalLayoutGroup vertical = buttonRow.GetComponent<VerticalLayoutGroup>();
            if (vertical != null)
            {
                Object.DestroyImmediate(vertical);
            }

            HorizontalLayoutGroup horizontal = buttonRow.GetComponent<HorizontalLayoutGroup>();
            if (horizontal == null)
            {
                horizontal = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            if (horizontal == null)
            {
                return;
            }

            horizontal.childAlignment = TextAnchor.MiddleCenter;
            horizontal.spacing = DonePanelButtonSpacing;
            horizontal.padding = new RectOffset(0, 0, 0, 0);
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = true;
            horizontal.childForceExpandHeight = false;
        }

        private void PositionDoneButtonRowUnderLegs(Transform donePanel, Transform buttonRow, CanvasScaler scaler)
        {
            LayoutElement rowLayout = buttonRow.GetComponent<LayoutElement>();
            if (rowLayout == null)
            {
                rowLayout = buttonRow.gameObject.AddComponent<LayoutElement>();
            }

            rowLayout.ignoreLayout = true;

            float canvasWidth = scaler != null ? scaler.referenceResolution.x : 1080f;
            RectTransform panelRect = donePanel.GetComponent<RectTransform>();
            if (panelRect != null && panelRect.rect.width > 0f)
            {
                canvasWidth = panelRect.rect.width;
            }

            float rowInnerWidth = canvasWidth - (DoneRowHorizontalInset * 2f);
            float cellWidth = (rowInnerWidth - DonePanelButtonSpacing) * 0.5f;
            float rowHeight = 0f;

            foreach (Transform child in buttonRow)
            {
                DonePanelButton styledButton = child.GetComponent<DonePanelButton>();
                if (styledButton == null)
                {
                    continue;
                }

                styledButton.ApplyStyleForRowCell(cellWidth);
                LayoutElement buttonLayout = child.GetComponent<LayoutElement>();
                if (buttonLayout != null)
                {
                    rowHeight = Mathf.Max(rowHeight, buttonLayout.preferredHeight);
                }
            }

            if (rowHeight <= 0f)
            {
                rowHeight = 120f;
            }

            float bannerTop = BottomBannerAdController.LayoutInsetReferenceUnits
                + SafeAreaInsets.GetBottomInsetReferenceUnits(scaler);
            float rowBottom = bannerTop + DoneBannerPadding;

            RectTransform rowRect = buttonRow.GetComponent<RectTransform>();
            if (rowRect == null)
            {
                return;
            }

            rowRect.anchorMin = new Vector2(0f, 0f);
            rowRect.anchorMax = new Vector2(1f, 0f);
            rowRect.pivot = new Vector2(0.5f, 0f);
            rowRect.offsetMin = new Vector2(DoneRowHorizontalInset, rowBottom);
            rowRect.offsetMax = new Vector2(-DoneRowHorizontalInset, rowBottom + rowHeight);
        }

        private static void EnsureWizardPanelLayout(Transform wizardPanel)
        {
            VerticalLayoutGroup vertical = wizardPanel.GetComponent<VerticalLayoutGroup>();
            if (vertical == null)
            {
                vertical = wizardPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.spacing = 4f;
            vertical.padding = new RectOffset(4, 4, 4, 4);
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;

            Transform navBar = wizardPanel.Find("WizardNavBar");
            if (navBar != null)
            {
                navBar.gameObject.SetActive(false);
            }

            Transform title = wizardPanel.Find("WizardStepTitle");
            if (title != null)
            {
                title.gameObject.SetActive(false);
            }

            Transform scroll = wizardPanel.Find("WizardItemsScroll");
            if (scroll != null)
            {
                LayoutElement layout = scroll.GetComponent<LayoutElement>();
                if (layout == null)
                {
                    layout = scroll.gameObject.AddComponent<LayoutElement>();
                }

                layout.minHeight = 320f;
                layout.preferredHeight = -1f;
                layout.flexibleHeight = 1f;
            }
        }

        private void PositionWizardNavOverlay(float bottomUiInset)
        {
            Transform overlay = transform.Find("WizardNavOverlay");
            if (overlay == null)
            {
                return;
            }

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            float arrowY = ResolveWizardArrowCanvasY(scaler);
            PositionWizardArrow(overlay, "PrevArrowButton", true, arrowY);
            PositionWizardArrow(overlay, "NextArrowButton", false, arrowY);
        }

        private static float ResolveWizardArrowCanvasY(CanvasScaler scaler)
        {
            PetCustomizer petCustomizer = FindAnyObjectByType<PetCustomizer>();
            PetDisplayScaler petScaler = petCustomizer != null
                ? petCustomizer.GetComponent<PetDisplayScaler>()
                : null;

            if (petScaler != null)
            {
                return petScaler.GetBodyBottomCanvasY(scaler);
            }

            CharacterCustomizer customizer = FindAnyObjectByType<CharacterCustomizer>();
            CharacterDisplayScaler displayScaler = customizer != null
                ? customizer.GetComponent<CharacterDisplayScaler>()
                : null;

            if (displayScaler != null)
            {
                return displayScaler.GetBodyBottomCanvasY(scaler);
            }

            const float fallbackBottomFraction = 0.38f;
            float referenceHeight = scaler != null
                ? scaler.referenceResolution.y
                : SafeAreaInsets.ReferenceHeight;
            return fallbackBottomFraction * referenceHeight;
        }

        private void PositionTopBarButtons(float safeTop)
        {
            PositionHomeMenuButton(safeTop);
            PositionSoundToggleButton(safeTop);
        }

        private void PositionHomeMenuButton(float safeTop)
        {
            HomeMenuButton home = HomeMenuButton.EnsureOnCanvas(transform);
            if (home == null)
            {
                return;
            }

            home.ApplyTopLeftLayout(safeTop);
            home.RefreshVisibility();
        }

        private void PositionSoundToggleButton(float safeTop)
        {
            SoundToggleButton toggle = SoundToggleButton.EnsureOnCanvas(transform);
            if (toggle == null)
            {
                return;
            }

            toggle.ApplyTopRightLayout(safeTop);
        }

        private static void PositionWizardArrow(Transform overlay, string buttonName, bool isLeft, float bodyBottomY)
        {
            Transform buttonTransform = overlay.Find(buttonName);
            if (buttonTransform == null)
            {
                return;
            }

            LayoutElement layoutElement = buttonTransform.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = true;
            }

            RectTransform rect = buttonTransform.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.anchorMax = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.pivot = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.anchoredPosition = new Vector2(isLeft ? ArrowHorizontalInset : -ArrowHorizontalInset, bodyBottomY);

            WizardArrowButton arrowStyle = buttonTransform.GetComponent<WizardArrowButton>();
            if (arrowStyle != null)
            {
                arrowStyle.Configure(isLeft
                    ? WizardArrowButton.ArrowDirection.Previous
                    : WizardArrowButton.ArrowDirection.Next);
            }
        }
    }
}
