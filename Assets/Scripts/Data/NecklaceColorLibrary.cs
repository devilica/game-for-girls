using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Four distinct tint colors applied to each necklace shape (one per variant suffix).
    /// </summary>
    [CreateAssetMenu(fileName = "NecklaceColorLibrary", menuName = "Dress Up Game/Necklace Color Library")]
    public class NecklaceColorLibrary : ScriptableObject
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
                Entry("01", "Red", 0.92f, 0.18f, 0.22f),
                Entry("02", "Blue", 0.22f, 0.42f, 0.95f),
                Entry("03", "Gold", 0.95f, 0.78f, 0.22f),
                Entry("04", "Purple", 0.62f, 0.28f, 0.88f)
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
