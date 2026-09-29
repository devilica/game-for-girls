using System;
using DressUpGame.Character;
using DressUpGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Reusable item button used in the horizontal items panel.
    /// Supports icon preview, label, and selected-state highlight.
    /// </summary>
    public class ItemSlotButton : MonoBehaviour
    {
        private const string CardBorderResourcePath = "UI/border";
        private const string AdCardBorderResourcePath = "UI/borderad";
        private const string NoneIconResourcePath = "UI/none";

        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private Text label;

        [Header("Visuals")]
        [SerializeField] private Sprite defaultBorderSprite;
        [SerializeField] private Sprite adBorderSprite;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(0.2f, 0.82f, 0.38f, 1f);
        [SerializeField] private Vector2 selectedOutlineDistance = new Vector2(3f, -3f);
        [SerializeField] private Color normalIconColor = Color.white;
        [SerializeField] private Color selectedIconColor = Color.white;
        [Header("Icon inset inside border.png")]
        [SerializeField] private Vector2 iconInsetMin = new Vector2(0.20f, 0.14f);
        [SerializeField] private Vector2 iconInsetMax = new Vector2(0.84f, 0.86f);
        [SerializeField] private Vector2 iconInsetWithLabelMin = new Vector2(0.18f, 0.36f);
        [SerializeField] private Vector2 iconInsetWithLabelMax = new Vector2(0.82f, 0.84f);
        [SerializeField] private Vector2 noneIconInsetMin = new Vector2(0.36f, 0.36f);
        [SerializeField] private Vector2 noneIconInsetMax = new Vector2(0.64f, 0.64f);

        private static Sprite cardBorderSprite;
        private static Sprite adCardBorderSprite;
        private static Sprite noneIconSprite;
        private bool useAdBorder;
        private string itemId;
        private bool isSelected;
        private Outline selectionOutline;

        public string ItemId => itemId;
        public int CardIndex { get; private set; }
        public event Action<ItemSlotButton> Clicked;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (background != null && defaultBorderSprite == null && background.sprite != null)
            {
                defaultBorderSprite = background.sprite;
            }

            ApplyCardBorderStyle();
            EnsureSelectionOutline();
            button.onClick.AddListener(HandleClick);
        }

        private void Start()
        {
            ApplyCardBorderStyle();
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        public void Setup(string id, string displayName, Sprite sprite, Color? iconTint = null)
        {
            itemId = id;
            isSelected = false;

            if (label != null)
            {
                label.text = displayName;
            }

            if (icon != null)
            {
                icon.material = null;
                icon.sprite = sprite;
                icon.enabled = sprite != null;
                icon.color = iconTint ?? normalIconColor;
            }

            UpdateVisuals();
        }

        public void SetupNone(string id)
        {
            itemId = id;
            isSelected = false;

            if (label != null)
            {
                label.text = string.Empty;
            }

            SetLabelVisible(false);

            if (icon != null)
            {
                icon.material = null;
                Sprite noneSprite = LoadNoneIconSprite();
                icon.sprite = noneSprite;
                icon.enabled = noneSprite != null;
                icon.color = normalIconColor;
                ApplyNoneIconLayout();
            }

            UpdateVisuals();
        }

        public void SetupEye(MakeupItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item.IsNoneOption)
            {
                SetupNone(item.Id);
                return;
            }

            itemId = item.Id;
            isSelected = false;

            if (label != null)
            {
                label.text = string.Empty;
            }

            if (icon != null)
            {
                icon.sprite = item.Sprite;
                icon.enabled = item.Sprite != null;
                EyeIrisTintUtility.ApplyToImage(icon, item);
            }

            UpdateVisuals();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisuals();
        }

        public void SetCardIndex(int index)
        {
            SetCardIndex(index, RewardedAdCardPolicy.IsAdGated(index, itemId));
        }

        public void SetCardIndex(int index, bool isAdGated)
        {
            CardIndex = index;
            useAdBorder = isAdGated;
            ApplyCardBorderStyle();
        }

        /// <summary>
        /// Hides the text label and expands the icon to fill the card.
        /// </summary>
        public void SetLabelVisible(bool visible)
        {
            if (label != null)
            {
                label.gameObject.SetActive(visible);
            }

            if (icon == null)
            {
                return;
            }

            RectTransform iconRect = icon.rectTransform;
            if (visible)
            {
                iconRect.anchorMin = iconInsetWithLabelMin;
                iconRect.anchorMax = iconInsetWithLabelMax;
            }
            else
            {
                iconRect.anchorMin = iconInsetMin;
                iconRect.anchorMax = iconInsetMax;
            }

            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
        }

        private void ApplyNoneIconLayout()
        {
            if (icon == null)
            {
                return;
            }

            RectTransform iconRect = icon.rectTransform;
            iconRect.anchorMin = noneIconInsetMin;
            iconRect.anchorMax = noneIconInsetMax;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
        }

        private void ApplyCardBorderStyle()
        {
            if (background == null)
            {
                return;
            }

            Sprite borderSprite = ResolveBorderSprite();
            if (borderSprite != null)
            {
                background.sprite = borderSprite;
                background.type = Image.Type.Simple;
                background.preserveAspect = true;
            }
        }

        private Sprite ResolveBorderSprite()
        {
            if (useAdBorder)
            {
                Sprite adSprite = adBorderSprite != null ? adBorderSprite : LoadAdCardBorderSprite();
                if (adSprite != null)
                {
                    return adSprite;
                }

                Debug.LogWarning("Ad border sprite is missing. Assign UI/borderad on ItemButton prefab.");
            }

            if (defaultBorderSprite != null)
            {
                return defaultBorderSprite;
            }

            return LoadCardBorderSprite();
        }

        private static Sprite LoadCardBorderSprite()
        {
            if (cardBorderSprite == null)
            {
                cardBorderSprite = Resources.Load<Sprite>(CardBorderResourcePath);
            }

            return cardBorderSprite;
        }

        private static Sprite LoadAdCardBorderSprite()
        {
            if (adCardBorderSprite == null)
            {
                adCardBorderSprite = Resources.Load<Sprite>(AdCardBorderResourcePath);
            }

            return adCardBorderSprite;
        }

        private static Sprite LoadNoneIconSprite()
        {
            if (noneIconSprite == null)
            {
                noneIconSprite = Resources.Load<Sprite>(NoneIconResourcePath);
            }

            return noneIconSprite;
        }

        private void HandleClick()
        {
            HapticFeedback.PlayCardTap();
            Clicked?.Invoke(this);
        }

        private void EnsureSelectionOutline()
        {
            if (background == null || selectionOutline != null)
            {
                return;
            }

            selectionOutline = background.GetComponent<Outline>();
            if (selectionOutline == null)
            {
                selectionOutline = background.gameObject.AddComponent<Outline>();
            }

            selectionOutline.useGraphicAlpha = true;
            selectionOutline.enabled = false;
        }

        private void UpdateVisuals()
        {
            if (background != null)
            {
                background.color = isSelected ? selectedColor : normalColor;
            }

            if (selectionOutline != null)
            {
                selectionOutline.enabled = isSelected;
                if (isSelected)
                {
                    selectionOutline.effectColor = selectedColor;
                    selectionOutline.effectDistance = selectedOutlineDistance;
                }
            }
        }

#if UNITY_EDITOR
        public void AssignReferences(Button btn, Image bg, Image iconImage, Text labelText)
        {
            button = btn;
            background = bg;
            icon = iconImage;
            label = labelText;
        }
#endif
    }
}
