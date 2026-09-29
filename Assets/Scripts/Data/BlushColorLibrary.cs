using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Defines 21 light blush tint colors used to generate MakeupItem assets.
    /// </summary>
    [CreateAssetMenu(fileName = "BlushColorLibrary", menuName = "Dress Up Game/Blush Color Library")]
    public class BlushColorLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct ColorEntry
        {
            public string id;
            public Color color;
            public string displayName;
        }

        [SerializeField] private ColorEntry[] colors = BuildDefaultColors();

        public ColorEntry[] Colors => colors;

        public static ColorEntry[] BuildDefaultColors()
        {
            return new[]
            {
                Entry("blush_2_01", "Soft Pink", 0.98f, 0.82f, 0.86f),
                Entry("blush_2_02", "Ballet Pink", 0.98f, 0.78f, 0.84f),
                Entry("blush_2_03", "Rose Petal", 0.96f, 0.72f, 0.78f),
                Entry("blush_2_04", "Peachy Pink", 0.98f, 0.78f, 0.72f),
                Entry("blush_2_05", "Warm Peach", 0.98f, 0.82f, 0.72f),
                Entry("blush_2_06", "Light Coral", 0.98f, 0.72f, 0.68f),
                Entry("blush_2_07", "Dusty Rose", 0.92f, 0.68f, 0.72f),
                Entry("blush_2_08", "Baby Pink", 0.98f, 0.85f, 0.88f),
                Entry("blush_2_09", "Cotton Candy", 0.98f, 0.80f, 0.88f),
                Entry("blush_2_10", "Strawberry Milk", 0.98f, 0.75f, 0.82f),
                Entry("blush_2_11", "Blush Nude", 0.96f, 0.82f, 0.78f),
                Entry("blush_2_12", "Soft Apricot", 0.98f, 0.84f, 0.76f),
                Entry("blush_2_13", "Honey Peach", 0.96f, 0.78f, 0.68f),
                Entry("blush_2_14", "Light Rose", 0.96f, 0.70f, 0.76f),
                Entry("blush_2_15", "Mauve Pink", 0.92f, 0.72f, 0.80f),
                Entry("blush_2_16", "Lilac Blush", 0.94f, 0.78f, 0.88f),
                Entry("blush_2_17", "Cherry Blossom", 0.98f, 0.82f, 0.86f),
                Entry("blush_2_18", "Sunset Pink", 0.98f, 0.76f, 0.70f),
                Entry("blush_2_19", "Rosy Cheek", 0.95f, 0.65f, 0.72f),
                Entry("blush_2_20", "Pale Berry", 0.92f, 0.68f, 0.76f),
                Entry("blush_2_21", "Soft Bloom", 0.98f, 0.88f, 0.86f)
            };
        }

        private static ColorEntry Entry(string id, string name, float r, float g, float b)
        {
            return new ColorEntry
            {
                id = id,
                displayName = name,
                color = new Color(r, g, b)
            };
        }
    }
}