using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Done-screen action button using custom sprite art (Back to Edit / Start Again).
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class DonePanelButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum ButtonKind
        {
            EditLook,
            StartOver
        }

        public const float TargetWidthFraction = 0.74f;
        public const float DoneViewWidthFraction = 0.62f;

        private const string BackEditResourcePath = "UI/backedit";
        private const string StartAgainResourcePath = "UI/startagain";

        [SerializeField] private ButtonKind kind = ButtonKind.EditLook;
        [SerializeField] private float pressedScale = 0.95f;

        private static Sprite backEditSprite;
        private static Sprite startAgainSprite;

        private Button button;
        private Image background;
        private Vector3 normalScale = Vector3.one;

        private void Awake()
        {
            ApplyStyle();
        }

        public void Configure(ButtonKind buttonKind)
        {
            kind = buttonKind;
            ApplyStyle();
        }

        public void ApplyStyle()
        {
            ApplyStyle(DoneViewWidthFraction);
        }

        public void ApplyStyle(float widthFraction)
        {
            float canvasWidth = GetCanvasWidth();
            ApplyStyleForRowCell(canvasWidth * widthFraction);
        }

        public void ApplyStyleForRowCell(float cellWidth)
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            normalScale = Vector3.one;

            Sprite sprite = GetSprite(kind);
            if (sprite == null)
            {
                Debug.LogWarning($"Done panel sprite missing for {kind}.");
                return;
            }

            HideLegacyLabel();
            ApplyFace(sprite);
            ApplyButtonSize(GetComponent<RectTransform>(), sprite, cellWidth);
            ApplyButtonColors();
        }

        public static Vector2 GetPreferredSize(Sprite sprite, float canvasWidth, float widthFraction = DoneViewWidthFraction)
        {
            if (sprite == null)
            {
                return Vector2.zero;
            }

            return GetPreferredSizeForCell(sprite, canvasWidth * widthFraction);
        }

        public static Vector2 GetPreferredSizeForCell(Sprite sprite, float cellWidth)
        {
            if (sprite == null || cellWidth <= 0f)
            {
                return Vector2.zero;
            }

            float aspect = sprite.rect.width / sprite.rect.height;
            return new Vector2(cellWidth, cellWidth / aspect);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable)
            {
                return;
            }

            transform.localScale = normalScale * pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = normalScale;
        }

        private void OnDisable()
        {
            transform.localScale = normalScale;
        }

        private static Sprite GetSprite(ButtonKind buttonKind)
        {
            if (buttonKind == ButtonKind.EditLook)
            {
                if (backEditSprite == null)
                {
                    backEditSprite = Resources.Load<Sprite>(BackEditResourcePath);
                }

                return backEditSprite;
            }

            if (startAgainSprite == null)
            {
                startAgainSprite = Resources.Load<Sprite>(StartAgainResourcePath);
            }

            return startAgainSprite;
        }

        private void HideLegacyLabel()
        {
            Transform labelTransform = transform.Find("Text");
            if (labelTransform != null)
            {
                labelTransform.gameObject.SetActive(false);
            }
        }

        private void ApplyFace(Sprite sprite)
        {
            if (background == null)
            {
                return;
            }

            background.sprite = sprite;
            background.type = Image.Type.Simple;
            background.preserveAspect = true;
            background.color = Color.white;
            background.raycastTarget = true;
        }

        private float GetCanvasWidth()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform root = canvas.rootCanvas.GetComponent<RectTransform>();
                if (root != null && root.rect.width > 0f)
                {
                    return root.rect.width;
                }
            }

            return 1080f;
        }

        private static void ApplyButtonSize(RectTransform rect, Sprite sprite, float cellWidth)
        {
            if (rect == null || sprite == null)
            {
                return;
            }

            Vector2 size = GetPreferredSizeForCell(sprite, cellWidth);
            rect.sizeDelta = size;

            LayoutElement layout = rect.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.preferredWidth = -1f;
                layout.preferredHeight = size.y;
                layout.minWidth = 0f;
                layout.minHeight = size.y;
                layout.flexibleWidth = 1f;
            }
        }

        private void ApplyButtonColors()
        {
            if (button == null)
            {
                return;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f);
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.55f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.targetGraphic = background;
        }
    }
}
