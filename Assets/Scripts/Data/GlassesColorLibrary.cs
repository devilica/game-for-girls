using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Thirty-three frame tint colors for glasses shape 2.
    /// </summary>
    [CreateAssetMenu(fileName = "GlassesColorLibrary", menuName = "Dress Up Game/Glasses Color Library")]
    public class GlassesColorLibrary : ScriptableObject
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
                Entry("01", "Black", 0.12f, 0.12f, 0.12f),
                Entry("02", "Charcoal", 0.28f, 0.28f, 0.30f),
                Entry("03", "Graphite", 0.42f, 0.42f, 0.44f),
                Entry("04", "Silver", 0.78f, 0.78f, 0.82f),
                Entry("05", "White", 0.96f, 0.96f, 0.96f),
                Entry("06", "Gold", 0.92f, 0.78f, 0.28f),
                Entry("07", "Rose Gold", 0.90f, 0.65f, 0.55f),
                Entry("08", "Bronze", 0.72f, 0.48f, 0.26f),
                Entry("09", "Copper", 0.82f, 0.42f, 0.28f),
                Entry("10", "Tortoise", 0.55f, 0.32f, 0.18f),
                Entry("11", "Dark Brown", 0.32f, 0.20f, 0.12f),
                Entry("12", "Mahogany", 0.45f, 0.22f, 0.15f),
                Entry("13", "Walnut", 0.58f, 0.35f, 0.22f),
                Entry("14", "Navy", 0.12f, 0.18f, 0.38f),
                Entry("15", "Royal Blue", 0.18f, 0.32f, 0.72f),
                Entry("16", "Sky Blue", 0.42f, 0.72f, 0.95f),
                Entry("17", "Teal", 0.18f, 0.58f, 0.58f),
                Entry("18", "Mint", 0.55f, 0.88f, 0.78f),
                Entry("19", "Forest Green", 0.18f, 0.42f, 0.28f),
                Entry("20", "Olive", 0.48f, 0.52f, 0.28f),
                Entry("21", "Burgundy", 0.48f, 0.12f, 0.22f),
                Entry("22", "Red", 0.78f, 0.15f, 0.18f),
                Entry("23", "Coral", 0.95f, 0.42f, 0.35f),
                Entry("24", "Hot Pink", 0.92f, 0.22f, 0.48f),
                Entry("25", "Blush Pink", 0.92f, 0.55f, 0.62f),
                Entry("26", "Lavender", 0.72f, 0.58f, 0.88f),
                Entry("27", "Purple", 0.52f, 0.28f, 0.72f),
                Entry("28", "Plum", 0.42f, 0.18f, 0.38f),
                Entry("29", "Amber", 0.92f, 0.62f, 0.18f),
                Entry("30", "Yellow", 0.95f, 0.85f, 0.28f),
                Entry("31", "Orange", 0.95f, 0.52f, 0.18f),
                Entry("32", "Peach", 0.95f, 0.68f, 0.52f),
                Entry("33", "Cream", 0.96f, 0.90f, 0.78f)
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
