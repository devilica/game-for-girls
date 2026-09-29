using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Twenty distinct lipstick tint colors applied to each lipstick shape.
    /// </summary>
    [CreateAssetMenu(fileName = "LipstickColorLibrary", menuName = "Dress Up Game/Lipstick Color Library")]
    public class LipstickColorLibrary : ScriptableObject
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
                Entry("01", "Classic Red", 0.82f, 0.12f, 0.18f),
                Entry("02", "Cherry", 0.75f, 0.08f, 0.15f),
                Entry("03", "Deep Red", 0.62f, 0.05f, 0.12f),
                Entry("04", "Brick Red", 0.68f, 0.22f, 0.18f),
                Entry("05", "Wine", 0.48f, 0.08f, 0.18f),
                Entry("06", "Burgundy", 0.42f, 0.06f, 0.14f),
                Entry("07", "Plum", 0.52f, 0.12f, 0.28f),
                Entry("08", "Berry", 0.72f, 0.15f, 0.32f),
                Entry("09", "Raspberry", 0.85f, 0.18f, 0.38f),
                Entry("10", "Hot Pink", 0.92f, 0.22f, 0.48f),
                Entry("11", "Fuchsia", 0.88f, 0.15f, 0.52f),
                Entry("12", "Rose", 0.88f, 0.35f, 0.48f),
                Entry("13", "Pink", 0.92f, 0.45f, 0.58f),
                Entry("14", "Baby Pink", 0.95f, 0.55f, 0.62f),
                Entry("15", "Coral", 0.95f, 0.42f, 0.35f),
                Entry("16", "Peach", 0.92f, 0.52f, 0.42f),
                Entry("17", "Apricot", 0.95f, 0.58f, 0.45f),
                Entry("18", "Nude", 0.82f, 0.55f, 0.48f),
                Entry("19", "Mauve", 0.72f, 0.42f, 0.52f),
                Entry("20", "Brown", 0.55f, 0.28f, 0.22f)
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
