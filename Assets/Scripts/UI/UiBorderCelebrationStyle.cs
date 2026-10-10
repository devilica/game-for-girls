using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Shared pink-to-gold border pulse used by Memory win celebration and main menu play hint.
    /// </summary>
    public static class UiBorderCelebrationStyle
    {
        public static readonly Color BaseBorderColor = new Color(0.95f, 0.45f, 0.72f, 1f);
        public static readonly Color AccentBorderColor = new Color(1f, 0.92f, 0.55f, 1f);
        public const float PulsePeriodSeconds = 0.85f;
    }
}
