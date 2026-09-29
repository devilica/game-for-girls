using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Keeps the splash image pinned to the full screen on all devices.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class SplashScreenLayout : MonoBehaviour
    {
        [SerializeField] private Image splashImage;

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        public void Apply()
        {
            if (splashImage == null)
            {
                splashImage = GetComponent<Image>();
            }

            CanvasScaler scaler = GetComponentInParent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 1f;
            }

            if (splashImage == null)
            {
                return;
            }

            RectTransform rect = splashImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            splashImage.type = Image.Type.Simple;
            splashImage.preserveAspect = false;
            splashImage.raycastTarget = false;

            Canvas.ForceUpdateCanvases();
        }
    }
}
