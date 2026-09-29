using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Ten fashion tint colors applied to each bag shape.
    /// </summary>
    [CreateAssetMenu(fileName = "BagColorLibrary", menuName = "Dress Up Game/Bag Color Library")]
    public class BagColorLibrary : ScriptableObject
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
                Entry("01", "Pink", 0.95f, 0.45f, 0.65f),
                Entry("02", "Red", 0.85f, 0.15f, 0.20f),
                Entry("03", "Purple", 0.55f, 0.25f, 0.75f),
                Entry("04", "Blue", 0.25f, 0.45f, 0.85f),
                Entry("05", "Mint", 0.45f, 0.85f, 0.75f),
                Entry("06", "Yellow", 0.95f, 0.85f, 0.25f),
                Entry("07", "Black", 0.15f, 0.15f, 0.18f),
                Entry("08", "Cream", 0.96f, 0.92f, 0.82f),
                Entry("09", "Coral", 0.95f, 0.50f, 0.42f),
                Entry("10", "Lavender", 0.72f, 0.58f, 0.88f)
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
