using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Twenty distinct jewel and metallic tint colors applied to each crown shape.
    /// </summary>
    [CreateAssetMenu(fileName = "CrownColorLibrary", menuName = "Dress Up Game/Crown Color Library")]
    public class CrownColorLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct ColorEntry
        {
            public string suffix;
            public Color color;
            public string displayName;
        }

        [SerializeField] private ColorEntry[] colors = BuildDefaultColors();

        public ColorEntry[] Colors => colors;

        public static ColorEntry[] BuildDefaultColors()
        {
            return new[]
            {
                Entry("01", "Gold", 0.95f, 0.78f, 0.25f),
                Entry("02", "Silver", 0.82f, 0.82f, 0.85f),
                Entry("03", "Rose Gold", 0.92f, 0.65f, 0.55f),
                Entry("04", "Bronze", 0.75f, 0.48f, 0.25f),
                Entry("05", "Copper", 0.85f, 0.42f, 0.28f),
                Entry("06", "Platinum", 0.88f, 0.88f, 0.90f),
                Entry("07", "Champagne", 0.95f, 0.88f, 0.72f),
                Entry("08", "Ruby", 0.78f, 0.12f, 0.22f),
                Entry("09", "Emerald", 0.15f, 0.65f, 0.35f),
                Entry("10", "Sapphire", 0.18f, 0.35f, 0.78f),
                Entry("11", "Amethyst", 0.58f, 0.28f, 0.72f),
                Entry("12", "Topaz", 0.95f, 0.72f, 0.28f),
                Entry("13", "Pearl", 0.96f, 0.94f, 0.90f),
                Entry("14", "Onyx", 0.18f, 0.16f, 0.20f),
                Entry("15", "Turquoise", 0.28f, 0.78f, 0.82f),
                Entry("16", "Coral Gem", 0.95f, 0.45f, 0.38f),
                Entry("17", "Lavender Jewel", 0.72f, 0.55f, 0.88f),
                Entry("18", "Crimson", 0.62f, 0.08f, 0.18f),
                Entry("19", "Aqua Crystal", 0.35f, 0.85f, 0.88f),
                Entry("20", "Magenta Gem", 0.82f, 0.15f, 0.55f)
            };
        }

        private static ColorEntry Entry(string suffix, string name, float r, float g, float b)
        {
            return new ColorEntry
            {
                suffix = suffix,
                displayName = name,
                color = new Color(r, g, b)
            };
        }
    }
}
