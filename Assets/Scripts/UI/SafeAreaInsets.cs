using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Converts device safe-area insets into canvas reference units (1080×1920 portrait).
    /// </summary>
    public static class SafeAreaInsets
    {
        public const float ReferenceHeight = 1920f;

        public static float GetBottomInsetReferenceUnits(CanvasScaler scaler = null)
        {
            return GetInsetReferenceUnits(Screen.safeArea.y, scaler);
        }

        public static float GetTopInsetReferenceUnits(CanvasScaler scaler = null)
        {
            if (Screen.height <= 0)
            {
                return 0f;
            }

            float topInsetPixels = Screen.height - Screen.safeArea.yMax;
            return GetInsetReferenceUnits(topInsetPixels, scaler);
        }

        /// <summary>
        /// Device safe-area bottom inset plus a minimum cushion for home indicators in editor/simulator.
        /// </summary>
        public static float GetEffectiveBottomInset(CanvasScaler scaler = null, float minimumReferenceUnits = 96f)
        {
            return Mathf.Max(GetBottomInsetReferenceUnits(scaler), minimumReferenceUnits);
        }

        public static float GetBottomUiInset(CanvasScaler scaler = null, float extraPadding = 0f)
        {
            return GetBottomInsetReferenceUnits(scaler)
                + BottomBannerAdController.LayoutInsetReferenceUnits
                + extraPadding;
        }

        private static float GetInsetReferenceUnits(float insetPixels, CanvasScaler scaler)
        {
            if (Screen.height <= 0)
            {
                return 0f;
            }

            float scale = ReferenceHeight / Screen.height;
            if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                scale = scaler.referenceResolution.y / Screen.height;
            }

            return insetPixels * scale;
        }
    }
}
