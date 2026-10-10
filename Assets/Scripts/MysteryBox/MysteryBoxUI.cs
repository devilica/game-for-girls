using System;
using DressUpGame.Character;
using DressUpGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.MysteryBox
{
    public class MysteryBoxUI : MonoBehaviour
    {
        [Header("Box buttons")]
        [SerializeField] private Button girlFashionButton;
        [SerializeField] private Button petSurpriseButton;
        [SerializeField] private Button bestiesButton;
        [SerializeField] private Image girlBoxImage;
        [SerializeField] private Image petBoxImage;
        [SerializeField] private Image bestiesBoxImage;

        [Header("Reveal panel")]
        [SerializeField] private GameObject revealPanel;
        [SerializeField] private Text headlineText;
        [SerializeField] private Text rewardTitleText;
        [SerializeField] private Text rewardDetailText;
        [SerializeField] private Text rarityText;
        [SerializeField] private Image rewardPreviewImage;
        [SerializeField] private Image rewardPreviewSecondaryImage;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button openAnotherButton;
        [SerializeField] private GameObject emptyMessagePanel;
        [SerializeField] private Text emptyMessageText;

        public event Action<MysteryBoxType> BoxClicked;
        public event Action ContinueClicked;
        public event Action RefreshClicked;

        public const string BoxShakeVisualName = "BoxShakeVisual";
        private const float RevealOverlayAlpha = 0.55f;

        private Button refreshButton;

        private void Awake()
        {
            if (girlFashionButton != null)
            {
                girlFashionButton.onClick.AddListener(() => BoxClicked?.Invoke(MysteryBoxType.GirlFashion));
            }

            if (petSurpriseButton != null)
            {
                petSurpriseButton.onClick.AddListener(() => BoxClicked?.Invoke(MysteryBoxType.PetSurprise));
            }

            if (bestiesButton != null)
            {
                bestiesButton.onClick.AddListener(() => BoxClicked?.Invoke(MysteryBoxType.Besties));
            }

            if (continueButton == null)
            {
                continueButton = ResolveApplyButton();
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(() => ContinueClicked?.Invoke());
            }

            refreshButton = ResolveRefreshButton();
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(() => RefreshClicked?.Invoke());
            }

            EnsureBoxImagesHaveSprites();
            EnsureBoxShakeVisuals();
            ConfigureBoxPresentation();
            RefreshRevealArt();
            HideReveal();
        }

        /// <summary>
        /// Sprites and visibility only — keeps RectTransform positions/sizes saved in the scene.
        /// </summary>
        public void RefreshRevealArt()
        {
            EnsureRevealPanelFullScreenOverlay();
            EnsureRevealCardBackground(applyCardSize: false);
            EnsureRevealActionButtons(applyDefaultButtonSizes: false);
            SetRevealRewardLabelsVisible(false);
            EnsureSecondaryPreviewHiddenWhenIdle();
        }

        /// <summary>
        /// Reapplies coded preview/button layout (overwrites scene transforms).
        /// </summary>
        public void ApplyDefaultRevealLayout()
        {
            EnsureRevealCardBackground(applyCardSize: true);
            EnsureRevealActionButtons(applyDefaultButtonSizes: true);
            SetRevealRewardLabelsVisible(false);
            EnsureRevealPreviewLayout();
            EnsureRevealActionRowLayout();
        }

        /// <summary>
        /// Full art + default layout reset (editor tooling).
        /// </summary>
        public void RefreshRevealVisuals()
        {
            RefreshRevealArt();
            ApplyDefaultRevealLayout();
        }

        /// <summary>
        /// Shows the reveal modal in the Scene/Game view with optional fake loot sprites (Edit Mode layout work).
        /// </summary>
        public void ShowEditorRevealPreview(Sprite primaryPreview, Sprite secondaryPreview = null)
        {
            RefreshRevealArt();

            if (revealPanel != null)
            {
                revealPanel.SetActive(true);
            }

            if (emptyMessagePanel != null)
            {
                emptyMessagePanel.SetActive(false);
            }

            EnsurePreviewImageReferences();
            SetRewardPreview(primaryPreview, secondaryPreview, Color.white, Color.white);
            SetRefreshButtonVisible(true);
            EnsureRevealActionButtonsDrawOnTop();
        }

        private Button ResolveApplyButton()
        {
            if (revealPanel == null)
            {
                return null;
            }

            Transform card = revealPanel.transform.Find("Card");
            if (card == null)
            {
                return null;
            }

            Transform apply = card.Find("ApplyButton");
            if (apply != null)
            {
                return apply.GetComponent<Button>();
            }

            Transform legacyContinue = card.Find("ContinueButton");
            return legacyContinue != null ? legacyContinue.GetComponent<Button>() : null;
        }

        private Button ResolveRefreshButton()
        {
            if (openAnotherButton != null)
            {
                return openAnotherButton;
            }

            if (revealPanel == null)
            {
                return null;
            }

            Transform card = revealPanel.transform.Find("Card");
            if (card == null)
            {
                return null;
            }

            Transform refresh = card.Find("RefreshButton");
            if (refresh != null)
            {
                return refresh.GetComponent<Button>();
            }

            Transform legacy = card.Find("OpenAnotherButton");
            return legacy != null ? legacy.GetComponent<Button>() : null;
        }

        private void EnsureRevealActionButtons(bool applyDefaultButtonSizes)
        {
            if (continueButton == null)
            {
                continueButton = ResolveApplyButton();
            }

            refreshButton = ResolveRefreshButton();
            if (applyDefaultButtonSizes)
            {
                ApplyActionButtonSprite(
                    continueButton,
                    MysteryBoxArt.LoadApplyButtonSprite(),
                    MysteryBoxArt.ApplyButtonWidthReference);
                ApplyActionButtonSprite(
                    refreshButton,
                    MysteryBoxArt.LoadRefreshButtonSprite(),
                    MysteryBoxArt.RefreshButtonWidthReference);
            }
            else
            {
                ApplyActionButtonArtOnly(continueButton, MysteryBoxArt.LoadApplyButtonSprite());
                ApplyActionButtonArtOnly(refreshButton, MysteryBoxArt.LoadRefreshButtonSprite());
            }
        }

        private void SetRevealRewardLabelsVisible(bool visible)
        {
            if (headlineText != null)
            {
                headlineText.gameObject.SetActive(visible);
            }

            if (rewardTitleText != null)
            {
                rewardTitleText.gameObject.SetActive(visible);
            }

            if (rewardDetailText != null)
            {
                rewardDetailText.gameObject.SetActive(visible);
            }

            if (rarityText != null)
            {
                rarityText.gameObject.SetActive(visible);
            }
        }

        private void EnsureRevealPreviewLayout()
        {
            EnsurePreviewImageReferences();

            float centerY = MysteryBoxArt.RevealPreviewCenterYReference;
            float dualSize = MysteryBoxArt.RevealPreviewDualSizeReference;
            float dualOffset = dualSize * 0.55f;

            if (rewardPreviewImage != null && rewardPreviewImage.rectTransform != null)
            {
                RectTransform primaryRect = rewardPreviewImage.rectTransform;
                primaryRect.sizeDelta = new Vector2(
                    MysteryBoxArt.RevealPreviewSizeReference,
                    MysteryBoxArt.RevealPreviewSizeReference);
                primaryRect.anchoredPosition = new Vector2(0f, centerY);
            }

            if (rewardPreviewSecondaryImage != null && rewardPreviewSecondaryImage.rectTransform != null)
            {
                RectTransform secondaryRect = rewardPreviewSecondaryImage.rectTransform;
                secondaryRect.sizeDelta = new Vector2(dualSize, dualSize);
                secondaryRect.anchoredPosition = new Vector2(dualOffset, centerY);
            }
        }

        private void EnsureRevealActionRowLayout()
        {
            if (revealPanel == null)
            {
                return;
            }

            Transform card = revealPanel.transform.Find("Card");
            if (card == null)
            {
                return;
            }

            Transform actionRow = card.Find("RevealActionRow");
            if (actionRow == null)
            {
                StackRevealActionButtons(card);
                return;
            }

            if (actionRow is RectTransform actionRowRect)
            {
                actionRowRect.anchoredPosition = Vector2.zero;
                actionRowRect.anchorMin = new Vector2(0.5f, 0.5f);
                actionRowRect.anchorMax = new Vector2(0.5f, 0.5f);
                actionRowRect.pivot = new Vector2(0.5f, 0.5f);
                actionRowRect.sizeDelta = new Vector2(640f, 420f);
            }

            HorizontalLayoutGroup horizontal = actionRow.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null)
            {
                DestroyLayoutBehaviour(horizontal);
            }

            VerticalLayoutGroup vertical = actionRow.GetComponent<VerticalLayoutGroup>();
            if (vertical != null)
            {
                DestroyLayoutBehaviour(vertical);
            }

            StackRevealActionButtons(actionRow);
        }

        private void StackRevealActionButtons(Transform container)
        {
            if (continueButton == null)
            {
                continueButton = ResolveApplyButton();
            }

            refreshButton = ResolveRefreshButton();
            if (continueButton == null)
            {
                return;
            }

            ApplyActionButtonSprite(
                continueButton,
                MysteryBoxArt.LoadApplyButtonSprite(),
                MysteryBoxArt.ApplyButtonWidthReference);
            ApplyActionButtonSprite(
                refreshButton,
                MysteryBoxArt.LoadRefreshButtonSprite(),
                MysteryBoxArt.RefreshButtonWidthReference);

            RectTransform applyRect = continueButton.transform as RectTransform;
            if (applyRect == null)
            {
                return;
            }

            if (applyRect.parent != container)
            {
                applyRect.SetParent(container, false);
            }

            ConfigureCenteredActionRect(applyRect);
            float applyCenterY = MysteryBoxArt.RevealApplyCenterYReference;
            applyRect.anchoredPosition = new Vector2(0f, applyCenterY);
            applyRect.SetAsFirstSibling();

            if (refreshButton == null)
            {
                return;
            }

            RectTransform refreshRect = refreshButton.transform as RectTransform;
            if (refreshRect == null)
            {
                return;
            }

            if (refreshRect.parent != container)
            {
                refreshRect.SetParent(container, false);
            }

            ConfigureCenteredActionRect(refreshRect);
            float applyHalfHeight = applyRect.sizeDelta.y * 0.5f;
            float refreshHalfHeight = refreshRect.sizeDelta.y * 0.5f;
            float gap = MysteryBoxArt.RevealActionRowSpacingReference;
            float extra = MysteryBoxArt.RevealApplyLayoutExtraBottomReference;
            float refreshCenterY = applyCenterY - applyHalfHeight - gap - extra - refreshHalfHeight;
            refreshRect.anchoredPosition = new Vector2(0f, refreshCenterY);
            refreshRect.SetAsLastSibling();
        }

        private static void ConfigureCenteredActionRect(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localEulerAngles = Vector3.zero;
        }

        private static void ApplyActionButtonSprite(Button button, Sprite sprite, float widthReference)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null && sprite != null)
            {
                MysteryBoxArt.ApplySpriteButton(image, sprite, widthReference);
            }

            HideActionButtonCaption(button);
        }

        private static void ApplyActionButtonArtOnly(Button button, Sprite sprite)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null && sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }

            HideActionButtonCaption(button);
        }

        private static void HideActionButtonCaption(Button button)
        {
            Transform textChild = button.transform.Find("Text");
            if (textChild != null)
            {
                textChild.gameObject.SetActive(false);
            }
        }

        public void SetRefreshButtonVisible(bool visible)
        {
            if (refreshButton == null)
            {
                refreshButton = ResolveRefreshButton();
            }

            if (refreshButton != null)
            {
                refreshButton.gameObject.SetActive(visible);
                refreshButton.interactable = visible;
            }
        }

        private void EnsureRevealActionButtonsDrawOnTop()
        {
            if (revealPanel == null)
            {
                return;
            }

            Transform card = revealPanel.transform.Find("Card");
            if (card == null)
            {
                return;
            }

            Transform preview = card.Find("Preview");
            if (preview != null)
            {
                preview.SetAsFirstSibling();
            }

            Transform previewSecondary = card.Find("PreviewSecondary");
            if (previewSecondary != null)
            {
                previewSecondary.SetSiblingIndex(preview != null ? 1 : 0);
            }

            if (continueButton == null)
            {
                continueButton = ResolveApplyButton();
            }

            refreshButton = ResolveRefreshButton();
            if (continueButton != null)
            {
                continueButton.transform.SetAsLastSibling();
            }

            if (refreshButton != null)
            {
                refreshButton.transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// Dim overlay must cover the entire canvas. Only resize the child <see cref="Card"/>, never scale RevealPanel.
        /// </summary>
        private void EnsureRevealPanelFullScreenOverlay()
        {
            if (revealPanel == null)
            {
                return;
            }

            Transform panelTransform = revealPanel.transform;
            panelTransform.localScale = Vector3.one;
            panelTransform.localRotation = Quaternion.identity;

            if (panelTransform is RectTransform panelRect)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
            }

            Image overlay = revealPanel.GetComponent<Image>();
            if (overlay == null)
            {
                overlay = revealPanel.AddComponent<Image>();
            }

            overlay.sprite = null;
            overlay.type = Image.Type.Simple;
            overlay.color = new Color(0f, 0f, 0f, RevealOverlayAlpha);
            overlay.raycastTarget = true;

            panelTransform.SetAsLastSibling();
        }

        private void EnsureRevealCardBackground(bool applyCardSize)
        {
            if (revealPanel == null)
            {
                return;
            }

            Transform card = revealPanel.transform.Find("Card");
            if (!card)
            {
                return;
            }

            Image cardBackground = card.GetComponent<Image>();
            if (cardBackground == null)
            {
                cardBackground = card.gameObject.AddComponent<Image>();
            }

            cardBackground.enabled = true;
            MysteryBoxArt.ApplyRevealCardFrame(cardBackground, card as RectTransform, applyCardSize);
            DestroyLegacyRevealPresentationChildren(card);
        }

        private static readonly string[] LegacyRevealCardChildNames =
        {
            "FrameClip",
            "FrameArt",
            "RevealPulseBorder",
        };

        private static void DestroyLegacyRevealPresentationChildren(Transform card)
        {
            for (int i = 0; i < LegacyRevealCardChildNames.Length; i++)
            {
                Transform legacy = card.Find(LegacyRevealCardChildNames[i]);
                if (legacy == null)
                {
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEngine.Object.DestroyImmediate(legacy.gameObject);
                    continue;
                }
#endif
                UnityEngine.Object.Destroy(legacy.gameObject);
            }
        }

        /// <summary>
        /// Scene may leave PreviewSecondary active for layout; hide it until dual loot is shown.
        /// </summary>
        private void EnsureSecondaryPreviewHiddenWhenIdle()
        {
            if (revealPanel != null && revealPanel.activeSelf)
            {
                return;
            }

            EnsurePreviewImageReferences();
            if (rewardPreviewSecondaryImage == null)
            {
                return;
            }

            rewardPreviewSecondaryImage.sprite = null;
            rewardPreviewSecondaryImage.enabled = false;
            rewardPreviewSecondaryImage.gameObject.SetActive(false);
        }

        public const float BoxTapSizeReference = 300f;
        public const float MinBoxTapSizeReference = 200f;

        public void ApplyBoxTapSize(float boxSize)
        {
            float clamped = Mathf.Clamp(boxSize, MinBoxTapSizeReference, BoxTapSizeReference);
            EnsureBoxTapSize(girlFashionButton, clamped);
            EnsureBoxTapSize(petSurpriseButton, clamped);
            EnsureBoxTapSize(bestiesButton, clamped);
        }

        private void ConfigureBoxPresentation()
        {
            HideBoxColumnLabels(girlFashionButton);
            HideBoxColumnLabels(petSurpriseButton);
            HideBoxColumnLabels(bestiesButton);
            EnsureBoxTapSize(girlFashionButton, BoxTapSizeReference);
            EnsureBoxTapSize(petSurpriseButton, BoxTapSizeReference);
            EnsureBoxTapSize(bestiesButton, BoxTapSizeReference);
        }

        private void EnsureBoxShakeVisuals()
        {
            EnsureBoxShakeVisual(girlFashionButton);
            EnsureBoxShakeVisual(petSurpriseButton);
            EnsureBoxShakeVisual(bestiesButton);
        }

        private static void EnsureBoxShakeVisual(Button boxButton)
        {
            if (boxButton == null)
            {
                return;
            }

            Transform parent = boxButton.transform.parent;
            if (parent != null && parent.name == BoxShakeVisualName)
            {
                StretchRect(boxButton.transform as RectTransform);
                return;
            }

            Transform column = parent;
            if (column == null)
            {
                return;
            }

            GameObject shakeGo = new GameObject(BoxShakeVisualName);
            shakeGo.transform.SetParent(column, false);
            RectTransform shakeRect = shakeGo.AddComponent<RectTransform>();
            StretchRect(shakeRect);

            boxButton.transform.SetParent(shakeGo.transform, false);
            StretchRect(boxButton.transform as RectTransform);

            LayoutElement buttonLayout = boxButton.GetComponent<LayoutElement>();
            if (buttonLayout != null)
            {
                Destroy(buttonLayout);
            }
        }

        private static void StretchRect(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localEulerAngles = Vector3.zero;
        }

        private static void HideBoxColumnLabels(Button boxButton)
        {
            Transform column = boxButton != null ? boxButton.transform.parent : null;
            if (column != null && column.name == BoxShakeVisualName)
            {
                column = column.parent;
            }

            if (column == null)
            {
                return;
            }

            for (int i = column.childCount - 1; i >= 0; i--)
            {
                Transform child = column.GetChild(i);
                if (child.name == "Title" || child.name == "Subtitle")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static void EnsureBoxTapSize(Button boxButton, float boxSize)
        {
            if (boxButton == null || boxButton.transform == null)
            {
                return;
            }

            Transform column = boxButton.transform.parent;
            while (column != null && column.name == BoxShakeVisualName)
            {
                column = column.parent;
            }

            if (column != null)
            {
                LayoutElement columnLayout = column.GetComponent<LayoutElement>();
                if (columnLayout == null)
                {
                    columnLayout = column.gameObject.AddComponent<LayoutElement>();
                }

                columnLayout.preferredWidth = boxSize + 16f;
                columnLayout.preferredHeight = boxSize;
                columnLayout.minWidth = boxSize + 16f;
                columnLayout.minHeight = boxSize;
            }

            LayoutElement boxLayout = boxButton.GetComponent<LayoutElement>();
            if (boxLayout != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(boxLayout);
                }
                else
                {
                    DestroyImmediate(boxLayout);
                }
            }

            RectTransform buttonRect = boxButton.transform as RectTransform;
            if (buttonRect != null)
            {
                StretchRect(buttonRect);
            }
        }

        public void SetBoxSprites(Sprite girlSprite, Sprite petSprite, Sprite bestiesSprite)
        {
            Sprite fallback = MysteryBoxArt.LoadBoxSprite();
            ApplyBoxSprite(girlBoxImage, CoalesceBoxSprite(girlSprite, fallback));
            ApplyBoxSprite(petBoxImage, CoalesceBoxSprite(petSprite, fallback));
            ApplyBoxSprite(bestiesBoxImage, CoalesceBoxSprite(bestiesSprite, fallback));
        }

        private void EnsureBoxImagesHaveSprites()
        {
            Sprite fallback = MysteryBoxArt.LoadBoxSprite();
            ApplyBoxSpriteIfMissing(girlBoxImage, fallback);
            ApplyBoxSpriteIfMissing(petBoxImage, fallback);
            ApplyBoxSpriteIfMissing(bestiesBoxImage, fallback);
        }

        private static Sprite CoalesceBoxSprite(Sprite primary, Sprite fallback)
        {
            if (IsUsableSprite(primary))
            {
                return primary;
            }

            return fallback;
        }

        private static bool IsUsableSprite(Sprite sprite)
        {
            return sprite != null && sprite.texture != null;
        }

        private static void ApplyBoxSprite(Image image, Sprite sprite)
        {
            if (image != null && sprite != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
        }

        private static void ApplyBoxSpriteIfMissing(Image image, Sprite sprite)
        {
            if (image != null && !IsUsableSprite(image.sprite))
            {
                ApplyBoxSprite(image, sprite);
            }
        }

        public void SetBoxButtonsInteractable(bool interactable)
        {
            SetBoxButtonInteractable(MysteryBoxType.GirlFashion, interactable);
            SetBoxButtonInteractable(MysteryBoxType.PetSurprise, interactable);
            SetBoxButtonInteractable(MysteryBoxType.Besties, interactable);
        }

        public void SetBoxButtonInteractable(MysteryBoxType boxType, bool interactable)
        {
            Button button = GetBoxButton(boxType);
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        public Button GetBoxButton(MysteryBoxType boxType)
        {
            return boxType switch
            {
                MysteryBoxType.GirlFashion => girlFashionButton,
                MysteryBoxType.PetSurprise => petSurpriseButton,
                MysteryBoxType.Besties => bestiesButton,
                _ => null
            };
        }

        public void ShowReveal(MysteryOpenResult result)
        {
            if (revealPanel != null)
            {
                revealPanel.SetActive(true);
            }

            if (emptyMessagePanel != null)
            {
                emptyMessagePanel.SetActive(result.Status == MysteryOpenStatus.AllUnlocked);
            }

            SetRevealRewardLabelsVisible(false);

            if (result.Status == MysteryOpenStatus.AllUnlocked)
            {
                if (emptyMessageText != null)
                {
                    emptyMessageText.text = string.IsNullOrEmpty(result.Message)
                        ? "You've collected everything in this box!"
                        : result.Message;
                }

                SetRewardPreview(null, null, Color.white, Color.white);
                SetRefreshButtonVisible(false);
                return;
            }

            if (emptyMessagePanel != null)
            {
                emptyMessagePanel.SetActive(false);
            }

            if (result.Rewards == null || result.Rewards.Count == 0)
            {
                SetRewardPreview(null, null, Color.white, Color.white);
                SetRefreshButtonVisible(false);
                return;
            }

            MysteryLootEntry primary = result.Rewards[0];
            if (result.Rewards.Count >= 2)
            {
                MysteryLootEntry secondary = result.Rewards[1];
                SetRewardPreview(primary, secondary);
            }
            else
            {
                SetRewardPreview(primary, null);
            }

            RefreshRevealArt();
            SetRefreshButtonVisible(true);
            EnsureRevealActionButtonsDrawOnTop();
        }

        private void SetRewardPreview(MysteryLootEntry primaryEntry, MysteryLootEntry? secondaryEntry)
        {
            Sprite secondarySprite = secondaryEntry.HasValue ? secondaryEntry.Value.previewSprite : null;
            Color secondaryTint = secondaryEntry.HasValue ? secondaryEntry.Value.previewTint : Color.white;
            SetRewardPreview(
                primaryEntry.previewSprite,
                secondarySprite,
                primaryEntry.previewTint,
                secondaryTint,
                primaryEntry,
                secondaryEntry);
        }

        private void SetRewardPreview(
            Sprite primarySprite,
            Sprite secondarySprite,
            Color primaryTint,
            Color secondaryTint,
            MysteryLootEntry? primaryEntry = null,
            MysteryLootEntry? secondaryEntry = null)
        {
            bool dual = secondarySprite != null;

            EnsurePreviewImageReferences();

            if (rewardPreviewImage != null)
            {
                rewardPreviewImage.enabled = primarySprite != null;
                rewardPreviewImage.sprite = primarySprite;
                rewardPreviewImage.preserveAspect = true;
                ApplyRewardPreviewTint(rewardPreviewImage, primarySprite, primaryTint, primaryEntry);
            }

            if (rewardPreviewSecondaryImage != null)
            {
                rewardPreviewSecondaryImage.gameObject.SetActive(dual);
                rewardPreviewSecondaryImage.enabled = dual;
                rewardPreviewSecondaryImage.sprite = secondarySprite;
                rewardPreviewSecondaryImage.preserveAspect = true;
                ApplyRewardPreviewTint(
                    rewardPreviewSecondaryImage,
                    secondarySprite,
                    secondaryTint,
                    dual ? secondaryEntry : null);
            }

            if (dual)
            {
                ApplyDualRevealPreviewRowLayout();
            }
        }

        /// <summary>
        /// Besties / dual gifts: same size, left and right on one row (single-gift scene layout is unchanged).
        /// </summary>
        private void ApplyDualRevealPreviewRowLayout()
        {
            float dualSize = MysteryBoxArt.RevealPreviewDualSizeReference;
            float dualOffset = dualSize * 0.55f;
            float rowY = MysteryBoxArt.RevealPreviewCenterYReference;

            if (rewardPreviewSecondaryImage != null && rewardPreviewSecondaryImage.rectTransform != null)
            {
                rowY = rewardPreviewSecondaryImage.rectTransform.anchoredPosition.y;
            }
            else if (rewardPreviewImage != null && rewardPreviewImage.rectTransform != null)
            {
                rowY = rewardPreviewImage.rectTransform.anchoredPosition.y;
            }

            if (rewardPreviewImage != null && rewardPreviewImage.rectTransform != null)
            {
                RectTransform primaryRect = rewardPreviewImage.rectTransform;
                primaryRect.sizeDelta = new Vector2(dualSize, dualSize);
                primaryRect.anchoredPosition = new Vector2(-dualOffset, rowY);
            }

            if (rewardPreviewSecondaryImage != null && rewardPreviewSecondaryImage.rectTransform != null)
            {
                RectTransform secondaryRect = rewardPreviewSecondaryImage.rectTransform;
                secondaryRect.sizeDelta = new Vector2(dualSize, dualSize);
                secondaryRect.anchoredPosition = new Vector2(dualOffset, rowY);
            }
        }

        private static GameCatalog cachedPreviewCatalog;

        private static void ApplyRewardPreviewTint(
            Image image,
            Sprite sprite,
            Color fallbackTint,
            MysteryLootEntry? entry)
        {
            if (image == null)
            {
                return;
            }

            if (sprite != null && entry.HasValue && TryResolveEyeMakeup(entry.Value, out MakeupItem eyes))
            {
                EyeIrisTintUtility.ApplyToImage(image, eyes);
                return;
            }

            EyeIrisTintUtility.ApplyOriginalToImage(image);
            image.color = sprite != null ? fallbackTint : Color.white;
        }

        private static bool TryResolveEyeMakeup(MysteryLootEntry entry, out MakeupItem makeup)
        {
            makeup = null;
            if (!entry.IsGirl
                || (entry.girlCategory != MysteryGirlCategory.Eyes
                    && entry.girlCategory != MysteryGirlCategory.Makeup))
            {
                return false;
            }

            if (string.IsNullOrEmpty(entry.itemId))
            {
                return false;
            }

            if (cachedPreviewCatalog == null)
            {
                cachedPreviewCatalog = Resources.Load<GameCatalog>("GameCatalog");
            }

            if (cachedPreviewCatalog == null)
            {
                return false;
            }

            makeup = cachedPreviewCatalog.GetMakeupByIdExact(entry.itemId);
            return makeup != null && makeup.MakeupType == MakeupType.Eyes;
        }

        private void EnsurePreviewImageReferences()
        {
            if (revealPanel == null)
            {
                return;
            }

            Transform card = revealPanel.transform.Find("Card");
            if (card == null)
            {
                return;
            }

            if (rewardPreviewImage == null)
            {
                Transform preview = card.Find("Preview");
                rewardPreviewImage = preview != null ? preview.GetComponent<Image>() : null;
            }

            if (rewardPreviewSecondaryImage == null)
            {
                Transform previewSecondary = card.Find("PreviewSecondary");
                rewardPreviewSecondaryImage = previewSecondary != null
                    ? previewSecondary.GetComponent<Image>()
                    : null;
            }
        }

        private static void DestroyLayoutBehaviour(Behaviour behaviour)
        {
            if (behaviour == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(behaviour);
                return;
            }
#endif
            Destroy(behaviour);
        }

        public void HideReveal()
        {
            if (revealPanel != null)
            {
                revealPanel.SetActive(false);
            }

            if (emptyMessagePanel != null)
            {
                emptyMessagePanel.SetActive(false);
            }

            if (rewardPreviewSecondaryImage != null)
            {
                rewardPreviewSecondaryImage.gameObject.SetActive(false);
            }
        }

        public RectTransform GetBoxRect(MysteryBoxType boxType)
        {
            Image image = GetBoxImage(boxType);
            if (image == null)
            {
                return null;
            }

            Transform parent = image.transform.parent;
            if (parent != null && parent.name == BoxShakeVisualName)
            {
                return parent as RectTransform;
            }

            return image.rectTransform;
        }

        public Sprite GetAssignedBoxSprite(MysteryBoxType boxType)
        {
            Image image = GetBoxImage(boxType);
            return image != null ? image.sprite : null;
        }

        private Image GetBoxImage(MysteryBoxType boxType)
        {
            return boxType switch
            {
                MysteryBoxType.GirlFashion => girlBoxImage,
                MysteryBoxType.PetSurprise => petBoxImage,
                MysteryBoxType.Besties => bestiesBoxImage,
                _ => null
            };
        }

        private static string FormatRarityLine(System.Collections.Generic.IReadOnlyList<MysteryLootEntry> rewards)
        {
            if (rewards.Count == 1)
            {
                return rewards[0].rarity.ToString().ToUpperInvariant();
            }

            return $"{rewards[0].rarity} + {rewards[1].rarity}".ToUpperInvariant();
        }

        private static Color RarityColor(MysteryRarity rarity)
        {
            return rarity switch
            {
                MysteryRarity.Legendary => new Color(1f, 0.72f, 0.18f),
                MysteryRarity.Epic => new Color(0.72f, 0.38f, 0.95f),
                MysteryRarity.Rare => new Color(0.35f, 0.62f, 0.98f),
                _ => new Color(0.55f, 0.55f, 0.58f)
            };
        }
    }
}
