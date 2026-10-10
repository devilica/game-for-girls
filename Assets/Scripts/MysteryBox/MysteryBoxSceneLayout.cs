using DressUpGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DressUpGame.MysteryBox
{
    /// <summary>
    /// Safe-area top chrome and bottom box row for the Mystery Box scene (matches Memory gameplay layout).
    /// </summary>
    public sealed class MysteryBoxSceneLayout : MonoBehaviour
    {
        private const float HeaderBannerWidthReference = 900f;
        private const float HeaderBannerHeightReference = 200f;
        private const float HeaderBannerTopOffsetReference = 32f;
        private const float HeaderHorizontalMarginReference = 48f;

        public static MysteryBoxSceneLayout EnsureOnCanvas(Canvas canvas)
        {
            if (canvas == null)
            {
                return null;
            }

            MysteryBoxSceneLayout layout = canvas.GetComponent<MysteryBoxSceneLayout>();
            if (layout == null)
            {
                layout = canvas.gameObject.AddComponent<MysteryBoxSceneLayout>();
            }

            layout.ApplyLayout();
            return layout;
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
            if (!IsMysteryBoxSceneActive() || !isActiveAndEnabled)
            {
                return;
            }

            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            if (canvas == null || !canvas.gameObject.activeInHierarchy)
            {
                return;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            float safeTop = SafeAreaInsets.GetTopInsetReferenceUnits(scaler);

            HomeMenuButton home = HomeMenuButton.EnsureOnCanvas(canvas.transform);
            home?.ApplyTopLeftLayout(safeTop);
            home?.RefreshVisibility();

            SoundToggleButton sound = SoundToggleButton.EnsureOnCanvas(canvas.transform);
            sound?.ApplyTopRightLayout(safeTop);

            ApplyHeaderBannerLayout(canvas, safeTop, scaler);

            MysteryBoxBottomUiLayout bottomLayout = canvas.GetComponentInChildren<MysteryBoxBottomUiLayout>(true);
            if (bottomLayout != null && bottomLayout.gameObject.activeInHierarchy)
            {
                bottomLayout.ApplyLayout();
            }

            Transform overlay = canvas.transform.Find(SoundToggleButton.OverlayName);
            if (overlay != null)
            {
                overlay.SetAsLastSibling();
            }
        }

        private static bool IsMysteryBoxSceneActive()
        {
            return SceneManager.GetActiveScene().name == MysteryBoxController.MysteryBoxSceneName;
        }

        private static void ApplyHeaderBannerLayout(Canvas canvas, float safeTopInset, CanvasScaler scaler)
        {
            if (canvas == null)
            {
                return;
            }

            Transform header = canvas.transform.Find("HeaderBanner");
            if (header is not RectTransform rect)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -(HeaderBannerTopOffsetReference + safeTopInset));

            Canvas.ForceUpdateCanvases();
            RectTransform canvasRect = canvas.transform as RectTransform;
            float canvasWidth = canvasRect != null && canvasRect.rect.width > 0f
                ? canvasRect.rect.width
                : scaler != null ? scaler.referenceResolution.x : 1080f;
            float maxBannerWidth = canvasWidth - HeaderHorizontalMarginReference;
            float bannerWidth = Mathf.Min(HeaderBannerWidthReference, maxBannerWidth);
            rect.sizeDelta = new Vector2(bannerWidth, HeaderBannerHeightReference);
        }
    }
}
