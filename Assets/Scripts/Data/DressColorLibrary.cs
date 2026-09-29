using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Twelve tint colors applied to each dress shape.
    /// </summary>
    [CreateAssetMenu(fileName = "DressColorLibrary", menuName = "Dress Up Game/Dress Color Library")]
    public class DressColorLibrary : ScriptableObject
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
                Entry("02", "Rose", 0.92f, 0.35f, 0.55f),
                Entry("03", "Red", 0.85f, 0.20f, 0.30f),
                Entry("04", "Coral", 0.98f, 0.55f, 0.45f),
                Entry("05", "Orange", 0.95f, 0.60f, 0.25f),
                Entry("06", "Gold", 0.92f, 0.78f, 0.30f),
                Entry("07", "Yellow", 0.98f, 0.90f, 0.40f),
                Entry("08", "Mint", 0.55f, 0.92f, 0.75f),
                Entry("09", "Teal", 0.25f, 0.72f, 0.68f),
                Entry("10", "Sky Blue", 0.45f, 0.75f, 0.95f),
                Entry("11", "Purple", 0.65f, 0.40f, 0.85f),
                Entry("12", "Lavender", 0.78f, 0.65f, 0.95f)
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
