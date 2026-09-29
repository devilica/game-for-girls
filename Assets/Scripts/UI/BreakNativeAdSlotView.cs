using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Sweet pink native-ad frame shown at the bottom of the tea break overlay.
    /// </summary>
    public class BreakNativeAdSlotView : MonoBehaviour
    {
        public const string SlotObjectName = "BreakNativeAdSlot";
        public const float SlotHeightReferenceUnits = 220f;
        private const float SlotBottomPaddingReferenceUnits = 8f;

        private static readonly Color SweetPanelColor = new Color(1f, 0.94f, 0.98f, 1f);
        private static readonly Color SweetAccentColor = new Color(1f, 0.55f, 0.72f, 1f);
        private static readonly Color SweetHeaderColor = new Color(0.72f, 0.28f, 0.52f, 1f);
        private static readonly Color SweetBodyColor = new Color(0.58f, 0.38f, 0.52f, 1f);

        [SerializeField] private RectTransform slotRect;
        [SerializeField] private GameObject editorPlaceholder;

        public RectTransform SlotRect => slotRect;

        public static BreakNativeAdSlotView EnsureOnOverlay(Transform overlayRoot)
        {
            if (overlayRoot == null)
            {
                return null;
            }

            Transform existing = overlayRoot.Find(SlotObjectName);
            if (existing != null)
            {
                BreakNativeAdSlotView view = existing.GetComponent<BreakNativeAdSlotView>();
                if (view == null)
                {
                    view = existing.gameObject.AddComponent<BreakNativeAdSlotView>();
                    view.BindReferences(existing);
                }

                return view;
            }

            GameObject slotGo = new GameObject(SlotObjectName);
            slotGo.transform.SetParent(overlayRoot, false);

            RectTransform rect = slotGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, SlotHeightReferenceUnits);

            Image panelImage = slotGo.AddComponent<Image>();
            panelImage.color = SweetPanelColor;
            panelImage.raycastTarget = false;
            ConfigureDevicePanelVisual(panelImage);

            GameObject headerGo = new GameObject("Header");
            headerGo.transform.SetParent(slotGo.transform, false);
            RectTransform headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.offsetMin = new Vector2(16f, -52f);
            headerRect.offsetMax = new Vector2(-16f, -8f);

            Text headerText = headerGo.AddComponent<Text>();
            headerText.text = "Sponsored";
            headerText.alignment = TextAnchor.MiddleCenter;
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerText.fontSize = 26;
            headerText.fontStyle = FontStyle.Bold;
            headerText.color = SweetHeaderColor;
            headerText.raycastTarget = false;

            GameObject contentGo = new GameObject("AdContentArea");
            contentGo.transform.SetParent(slotGo.transform, false);
            RectTransform contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(12f, 12f);
            contentRect.offsetMax = new Vector2(-12f, -56f);

            Image contentBg = contentGo.AddComponent<Image>();
            contentBg.color = new Color(1f, 0.98f, 1f, 0.92f);
            contentBg.raycastTarget = false;

            GameObject editorPlaceholderGo = BuildEditorPlaceholder(contentGo.transform);

            BreakNativeAdSlotView created = slotGo.AddComponent<BreakNativeAdSlotView>();
            created.slotRect = rect;
            created.editorPlaceholder = editorPlaceholderGo;
            slotGo.SetActive(false);
            return created;
        }

        public void ApplyLayout()
        {
            if (slotRect == null)
            {
                return;
            }

            float bannerInset = BottomBannerAdController.LayoutInsetReferenceUnits;
            float slotBottom = bannerInset + SlotBottomPaddingReferenceUnits;
            slotRect.offsetMin = new Vector2(0f, slotBottom);
            slotRect.offsetMax = new Vector2(0f, slotBottom + SlotHeightReferenceUnits);
            ApplyDeviceChromeVisibility();
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);

            if (visible)
            {
                ApplyDeviceChromeVisibility();
                transform.SetAsLastSibling();
            }

#if UNITY_EDITOR
            if (editorPlaceholder != null)
            {
                editorPlaceholder.SetActive(visible && !Application.isMobilePlatform);
            }
#else
            if (editorPlaceholder != null)
            {
                editorPlaceholder.SetActive(false);
            }
#endif
        }

        private void BindReferences(Transform root)
        {
            slotRect = root.GetComponent<RectTransform>();
            editorPlaceholder = root.Find("AdContentArea/EditorPlaceholder")?.gameObject;
            ConfigureDevicePanelVisual(root.GetComponent<Image>());
        }

        private static void ConfigureDevicePanelVisual(Image panelImage)
        {
            if (panelImage == null)
            {
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            panelImage.color = new Color(1f, 1f, 1f, 0f);
            panelImage.raycastTarget = false;
#else
            panelImage.color = SweetPanelColor;
#endif
        }

        private void ApplyDeviceChromeVisibility()
        {
#if UNITY_ANDROID || UNITY_IOS
            Image panelImage = GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(1f, 1f, 1f, 0f);
            }

            Transform header = transform.Find("Header");
            if (header != null)
            {
                Text headerText = header.GetComponent<Text>();
                if (headerText != null)
                {
                    headerText.color = new Color(1f, 1f, 1f, 0.72f);
                }
            }

            Transform contentArea = transform.Find("AdContentArea");
            Image contentBackground = contentArea != null ? contentArea.GetComponent<Image>() : null;
            if (contentBackground != null)
            {
                contentBackground.color = new Color(1f, 1f, 1f, 0f);
            }
#endif
        }

        private static GameObject BuildEditorPlaceholder(Transform parent)
        {
            GameObject placeholderGo = new GameObject("EditorPlaceholder");
            placeholderGo.transform.SetParent(parent, false);

            RectTransform placeholderRect = placeholderGo.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;

            GameObject iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(placeholderGo.transform, false);
            RectTransform iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.sizeDelta = new Vector2(72f, 72f);
            iconRect.anchoredPosition = new Vector2(12f, 0f);

            Image iconImage = iconGo.AddComponent<Image>();
            iconImage.color = SweetAccentColor;
            iconImage.raycastTarget = false;

            GameObject headlineGo = new GameObject("Headline");
            headlineGo.transform.SetParent(placeholderGo.transform, false);
            RectTransform headlineRect = headlineGo.AddComponent<RectTransform>();
            headlineRect.anchorMin = new Vector2(0f, 0.55f);
            headlineRect.anchorMax = new Vector2(1f, 1f);
            headlineRect.offsetMin = new Vector2(96f, 0f);
            headlineRect.offsetMax = new Vector2(-120f, -8f);

            Text headline = headlineGo.AddComponent<Text>();
            headline.text = "Sample Native Ad";
            headline.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headline.fontSize = 24;
            headline.fontStyle = FontStyle.Bold;
            headline.color = SweetHeaderColor;
            headline.alignment = TextAnchor.UpperLeft;
            headline.raycastTarget = false;

            GameObject bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(placeholderGo.transform, false);
            RectTransform bodyRect = bodyGo.AddComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 0.55f);
            bodyRect.offsetMin = new Vector2(96f, 8f);
            bodyRect.offsetMax = new Vector2(-120f, 0f);

            Text body = bodyGo.AddComponent<Text>();
            body.text = "A sweet break suggestion just for you.";
            body.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            body.fontSize = 18;
            body.color = SweetBodyColor;
            body.alignment = TextAnchor.UpperLeft;
            body.raycastTarget = false;

            GameObject buttonGo = new GameObject("CallToAction");
            buttonGo.transform.SetParent(placeholderGo.transform, false);
            RectTransform buttonRect = buttonGo.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 0.5f);
            buttonRect.anchorMax = new Vector2(1f, 0.5f);
            buttonRect.pivot = new Vector2(1f, 0.5f);
            buttonRect.sizeDelta = new Vector2(104f, 44f);
            buttonRect.anchoredPosition = new Vector2(-12f, 0f);

            Image buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.color = SweetAccentColor;
            buttonImage.raycastTarget = false;

            GameObject buttonLabelGo = new GameObject("Label");
            buttonLabelGo.transform.SetParent(buttonGo.transform, false);
            RectTransform buttonLabelRect = buttonLabelGo.AddComponent<RectTransform>();
            buttonLabelRect.anchorMin = Vector2.zero;
            buttonLabelRect.anchorMax = Vector2.one;
            buttonLabelRect.offsetMin = Vector2.zero;
            buttonLabelRect.offsetMax = Vector2.zero;

            Text buttonLabel = buttonLabelGo.AddComponent<Text>();
            buttonLabel.text = "Install";
            buttonLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonLabel.fontSize = 20;
            buttonLabel.fontStyle = FontStyle.Bold;
            buttonLabel.color = Color.white;
            buttonLabel.alignment = TextAnchor.MiddleCenter;
            buttonLabel.raycastTarget = false;

            placeholderGo.SetActive(false);
            return placeholderGo;
        }
    }
}
