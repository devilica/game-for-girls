using DressUpGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DressUpGame.MysteryBox
{
    /// <summary>
    /// Keeps mystery box buttons anchored above the bottom banner ad.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MysteryBoxBottomUiLayout : MonoBehaviour
    {
        private const float BannerReserveReferenceUnits = 48f;
        private const float SidePaddingReference = 24f;
        private const float BottomPaddingReference = 8f;
        private const int BoxColumnCount = 3;
        private const float ColumnWidthExtraReference = 16f;

        private RectTransform panelRect;
        private MysteryBoxUI mysteryUi;
        private bool layoutApplying;

        private void Awake()
        {
            panelRect = transform as RectTransform;
            mysteryUi = GetComponent<MysteryBoxUI>();
        }

        private void OnEnable()
        {
            BottomBannerAdController.LayoutInsetChanged += HandleBannerInsetChanged;
            ApplyLayout();
        }

        private void OnDisable()
        {
            BottomBannerAdController.LayoutInsetChanged -= HandleBannerInsetChanged;
        }

        private void OnDestroy()
        {
            BottomBannerAdController.LayoutInsetChanged -= HandleBannerInsetChanged;
        }

        private void Start()
        {
            ApplyLayout();
        }

        private void HandleBannerInsetChanged()
        {
            if (!IsMysteryBoxSceneActive())
            {
                return;
            }

            ApplyLayout();
        }

        public void ApplyLayout()
        {
            if (!IsMysteryBoxSceneActive() || !isActiveAndEnabled || layoutApplying)
            {
                return;
            }

            if (panelRect == null)
            {
                panelRect = transform as RectTransform;
            }

            if (panelRect == null || !panelRect.gameObject.activeInHierarchy)
            {
                return;
            }

            layoutApplying = true;
            try
            {
                if (mysteryUi == null)
                {
                    mysteryUi = GetComponent<MysteryBoxUI>();
                }

                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas == null || !canvas.gameObject.activeInHierarchy)
                {
                    return;
                }

                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                float safeLeft = SafeAreaInsets.GetLeftInsetReferenceUnits(scaler);
                float safeRight = SafeAreaInsets.GetRightInsetReferenceUnits(scaler);
                float sidePadding = SidePaddingReference + Mathf.Max(safeLeft, safeRight);

                float bottomInset = BottomBannerAdController.LayoutInsetReferenceUnits;
                float bottomBase = bottomInset + BannerReserveReferenceUnits + BottomPaddingReference;

                panelRect.anchorMin = new Vector2(0f, 0f);
                panelRect.anchorMax = new Vector2(1f, 0f);
                panelRect.pivot = new Vector2(0.5f, 0f);
                panelRect.offsetMin = new Vector2(sidePadding, bottomBase);
                panelRect.offsetMax = new Vector2(-sidePadding, bottomBase);
                panelRect.sizeDelta = Vector2.zero;

                Canvas.ForceUpdateCanvases();

                HorizontalLayoutGroup row = GetComponent<HorizontalLayoutGroup>();
                if (row != null)
                {
                    row.childAlignment = TextAnchor.LowerCenter;
                }

                float boxSize = ComputeResponsiveBoxTapSize(row, panelRect.rect.width);
                if (mysteryUi != null)
                {
                    mysteryUi.ApplyBoxTapSize(boxSize);
                }

                float panelHeight = boxSize + 12f;
                panelRect.offsetMax = new Vector2(-sidePadding, bottomBase + panelHeight);

                LayoutRebuilder.MarkLayoutForRebuild(panelRect);
            }
            finally
            {
                layoutApplying = false;
            }
        }

        private static bool IsMysteryBoxSceneActive()
        {
            return SceneManager.GetActiveScene().name == MysteryBoxController.MysteryBoxSceneName;
        }

        private static float ComputeResponsiveBoxTapSize(HorizontalLayoutGroup row, float innerWidth)
        {
            if (innerWidth <= 0f)
            {
                return MysteryBoxUI.BoxTapSizeReference;
            }

            float spacing = row != null ? row.spacing : 20f;
            float gaps = spacing * (BoxColumnCount - 1);
            float maxBox = (innerWidth - gaps) / BoxColumnCount - ColumnWidthExtraReference;
            return Mathf.Clamp(maxBox, MysteryBoxUI.MinBoxTapSizeReference, MysteryBoxUI.BoxTapSizeReference);
        }
    }
}
