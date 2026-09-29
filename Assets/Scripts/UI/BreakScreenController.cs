using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Full-screen tea break overlay with a sweet native ad slot at the bottom.
    /// </summary>
    public class BreakScreenController : MonoBehaviour
    {
        public const string OverlayName = "BreakScreenOverlay";
        public const float DefaultDurationSeconds = 3f;

        private const string BreakResourcePath = "UI/break";

        [SerializeField] private GameObject overlayRoot;
        [SerializeField] private Image breakImage;
        [SerializeField] private BreakNativeAdSlotView nativeAdSlot;

        private static Sprite cachedBreakSprite;
        private BreakNativeAdController nativeAdController;
        private bool isShowing;

        public bool IsShowing => isShowing;

        public static BreakScreenController EnsureOnCanvas(Transform canvas)
        {
            if (canvas == null)
            {
                return null;
            }

            Transform existingOverlay = canvas.Find(OverlayName);
            if (existingOverlay != null)
            {
                BreakScreenController existing = existingOverlay.GetComponent<BreakScreenController>();
                if (existing == null)
                {
                    existing = existingOverlay.gameObject.AddComponent<BreakScreenController>();
                }

                existing.Initialize();
                return existing;
            }

            GameObject overlayGo = new GameObject(OverlayName);
            overlayGo.transform.SetParent(canvas, false);

            RectTransform overlayRect = overlayGo.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image backdrop = overlayGo.AddComponent<Image>();
            backdrop.color = new Color(0.98f, 0.82f, 0.90f, 1f);
            backdrop.raycastTarget = false;

            GameObject imageGo = new GameObject("BreakImage");
            imageGo.transform.SetParent(overlayGo.transform, false);

            RectTransform imageRect = imageGo.AddComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;

            Image image = imageGo.AddComponent<Image>();
            image.preserveAspect = true;
            image.type = Image.Type.Simple;
            image.raycastTarget = true;

            BreakScreenController created = overlayGo.AddComponent<BreakScreenController>();
            created.overlayRoot = overlayGo;
            created.breakImage = image;
            created.Initialize();
            overlayGo.SetActive(false);
            overlayGo.transform.SetAsLastSibling();
            return created;
        }

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (overlayRoot == null)
            {
                overlayRoot = gameObject;
            }

            if (breakImage == null)
            {
                breakImage = transform.Find("BreakImage")?.GetComponent<Image>();
            }

            EnsureBreakSpriteLoaded();
            ApplyFullScreenBreakLayout();
        }

        public IEnumerator ShowRoutine(float durationSeconds = DefaultDurationSeconds)
        {
            if (isShowing)
            {
                yield break;
            }

            isShowing = true;
            EnsureBreakSpriteLoaded();

            try
            {
                EnsureNativeAdSlot();
                ApplyFullScreenBreakLayout();

                if (overlayRoot != null)
                {
                    overlayRoot.SetActive(true);
                    overlayRoot.transform.SetAsLastSibling();
                }

                nativeAdSlot?.ApplyLayout();
                nativeAdSlot?.SetVisible(true);

                BottomBannerAdController.RefreshVisibility();
                BottomBannerAdController.BringToFront();

                float breakEndTime = Time.unscaledTime + durationSeconds;
                BreakNativeAdController nativeController = ResolveNativeAdController();
                if (nativeController != null)
                {
                    yield return nativeController.ShowForBreakRoutine(nativeAdSlot?.SlotRect);
                }

                while (Time.unscaledTime < breakEndTime)
                {
                    yield return null;
                }

                ResolveNativeAdController()?.HideAfterBreak();
                nativeAdSlot?.SetVisible(false);

                if (overlayRoot != null)
                {
                    overlayRoot.SetActive(false);
                }

                BottomBannerAdController.RefreshVisibility();
            }
            finally
            {
                isShowing = false;
                GameplayPauseGuard.EnsureUnpaused();
            }
        }

        private void EnsureNativeAdSlot()
        {
            if (overlayRoot == null)
            {
                return;
            }

            if (nativeAdSlot == null)
            {
                nativeAdSlot = BreakNativeAdSlotView.EnsureOnOverlay(overlayRoot.transform);
            }
        }

        private BreakNativeAdController ResolveNativeAdController()
        {
            if (nativeAdController == null)
            {
                nativeAdController = BreakNativeAdController.EnsureExists();
            }

            return nativeAdController;
        }

        private void ApplyFullScreenBreakLayout()
        {
            if (overlayRoot == null)
            {
                return;
            }

            RectTransform overlayRect = overlayRoot.GetComponent<RectTransform>();
            if (overlayRect != null)
            {
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
            }

            if (breakImage != null)
            {
                breakImage.type = Image.Type.Simple;
                breakImage.preserveAspect = true;

                RectTransform imageRect = breakImage.rectTransform;
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = Vector2.zero;
                imageRect.offsetMax = Vector2.zero;
                imageRect.localScale = Vector3.one;
                breakImage.transform.SetAsFirstSibling();

                AspectRatioFitter fitter = breakImage.GetComponent<AspectRatioFitter>();
                if (fitter == null)
                {
                    fitter = breakImage.gameObject.AddComponent<AspectRatioFitter>();
                }

                fitter.enabled = true;
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                if (breakImage.sprite != null)
                {
                    Rect spriteRect = breakImage.sprite.rect;
                    fitter.aspectRatio = spriteRect.width / spriteRect.height;
                }
            }

            Image backdrop = overlayRoot.GetComponent<Image>();
            if (backdrop != null)
            {
                backdrop.color = new Color(0.98f, 0.82f, 0.90f, 1f);
            }
        }

        private void EnsureBreakSpriteLoaded()
        {
            if (breakImage == null)
            {
                return;
            }

            if (cachedBreakSprite == null)
            {
                cachedBreakSprite = Resources.Load<Sprite>(BreakResourcePath);
            }

            if (cachedBreakSprite != null)
            {
                breakImage.sprite = cachedBreakSprite;
            }
        }
    }
}
