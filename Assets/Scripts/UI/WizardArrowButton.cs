using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Wizard navigation arrow using custom sprite assets with a soft press bounce.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class WizardArrowButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum ArrowDirection
        {
            Previous,
            Next
        }

        public const float TargetHeight = 88f;

        private const string ArrowResourcePath = "UI/arrow_left";
        private const string GraphicChildName = "ArrowGraphic";

        [SerializeField] private ArrowDirection direction = ArrowDirection.Next;
        [SerializeField] private float pressedScale = 0.92f;

        private static Sprite arrowSprite;

        private Button button;
        private Image hitTarget;
        private Image arrowGraphic;
        private Vector3 normalScale = Vector3.one;

        private void Awake()
        {
            ApplyStyle();
        }

        private void OnEnable()
        {
            transform.localScale = normalScale;
        }

        public void Configure(ArrowDirection arrowDirection)
        {
            direction = arrowDirection;
            ApplyStyle();
        }

        public void ApplyStyle()
        {
            button = GetComponent<Button>();
            hitTarget = GetComponent<Image>();
            RectTransform rect = GetComponent<RectTransform>();
            normalScale = Vector3.one;

            Sprite sprite = GetArrowSprite();
            if (sprite == null)
            {
                Debug.LogWarning($"Wizard arrow sprite missing for direction {direction}.");
                return;
            }

            EnsureArrowGraphic();
            ApplyFace(sprite);
            ApplyButtonSize(rect, sprite);
            ApplyButtonColors();
            transform.localScale = normalScale;
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

        private static Sprite GetArrowSprite()
        {
            if (arrowSprite == null)
            {
                arrowSprite = Resources.Load<Sprite>(ArrowResourcePath);
            }

            return arrowSprite;
        }

        private void EnsureArrowGraphic()
        {
            Transform graphicTransform = transform.Find(GraphicChildName);
            if (graphicTransform == null)
            {
                GameObject graphicObject = new GameObject(
                    GraphicChildName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                graphicTransform = graphicObject.transform;
                graphicTransform.SetParent(transform, false);
            }

            arrowGraphic = graphicTransform.GetComponent<Image>();
            RectTransform graphicRect = graphicTransform as RectTransform;
            if (graphicRect != null)
            {
                graphicRect.anchorMin = Vector2.zero;
                graphicRect.anchorMax = Vector2.one;
                graphicRect.offsetMin = Vector2.zero;
                graphicRect.offsetMax = Vector2.zero;
                graphicRect.pivot = new Vector2(0.5f, 0.5f);
                graphicRect.localRotation = Quaternion.identity;
            }
        }

        private void HideLegacyDecorations()
        {
            Transform shadowTransform = transform.Find("Shadow");
            if (shadowTransform != null)
            {
                shadowTransform.gameObject.SetActive(false);
            }

            Transform labelTransform = transform.Find("Text");
            if (labelTransform != null)
            {
                labelTransform.gameObject.SetActive(false);
            }
        }

        private void ApplyFace(Sprite sprite)
        {
            HideLegacyDecorations();

            if (hitTarget != null)
            {
                hitTarget.sprite = null;
                hitTarget.color = new Color(1f, 1f, 1f, 0f);
                hitTarget.raycastTarget = true;
            }

            if (arrowGraphic == null)
            {
                return;
            }

            arrowGraphic.sprite = sprite;
            arrowGraphic.type = Image.Type.Simple;
            arrowGraphic.preserveAspect = true;
            arrowGraphic.color = Color.white;
            arrowGraphic.raycastTarget = false;

            RectTransform graphicRect = arrowGraphic.rectTransform;
            graphicRect.localScale = direction == ArrowDirection.Next
                ? new Vector3(-1f, 1f, 1f)
                : Vector3.one;
        }

        private static void ApplyButtonSize(RectTransform rect, Sprite sprite)
        {
            if (rect == null || sprite == null)
            {
                return;
            }

            float aspect = sprite.rect.width / sprite.rect.height;
            rect.sizeDelta = new Vector2(TargetHeight * aspect, TargetHeight);
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
            button.targetGraphic = hitTarget != null ? hitTarget : arrowGraphic;
        }
    }
}
