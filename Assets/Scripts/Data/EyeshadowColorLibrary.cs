using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Defines the 90 eyeshadow tint colors used to generate MakeupItem assets.
    /// </summary>
    [CreateAssetMenu(fileName = "EyeshadowColorLibrary", menuName = "Dress Up Game/Eyeshadow Color Library")]
    public class EyeshadowColorLibrary : ScriptableObject
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

        public Color GetColor(string id)
        {
            foreach (ColorEntry entry in colors)
            {
                if (entry.id == id)
                {
                    return entry.color;
                }
            }

            return Color.white;
        }

        public static ColorEntry[] BuildDefaultColors()
        {
            return new[]
            {
                Entry("eyeshadow_2_61", "Pure White", 0.99f, 0.99f, 0.98f),
                Entry("eyeshadow_2_62", "Snow", 0.96f, 0.96f, 0.98f),
                Entry("eyeshadow_2_63", "Ivory Mist", 0.96f, 0.94f, 0.90f),
                Entry("eyeshadow_2_64", "Vanilla", 0.98f, 0.96f, 0.88f),
                Entry("eyeshadow_2_65", "Soft Cream", 0.98f, 0.94f, 0.86f),
                Entry("eyeshadow_2_66", "Pale Beige", 0.94f, 0.88f, 0.80f),
                Entry("eyeshadow_2_67", "Light Nude", 0.92f, 0.82f, 0.74f),
                Entry("eyeshadow_2_68", "Blush Nude", 0.96f, 0.86f, 0.82f),
                Entry("eyeshadow_2_69", "Fairy Pink", 0.98f, 0.90f, 0.92f),
                Entry("eyeshadow_2_70", "Ballet Pink", 0.98f, 0.88f, 0.90f),
                Entry("eyeshadow_2_71", "Soft Peach", 0.98f, 0.90f, 0.84f),
                Entry("eyeshadow_2_72", "Light Peach", 0.98f, 0.88f, 0.78f),
                Entry("eyeshadow_2_73", "Pale Apricot", 0.98f, 0.86f, 0.72f),
                Entry("eyeshadow_2_74", "Light Lemon", 0.98f, 0.96f, 0.78f),
                Entry("eyeshadow_2_75", "Buttercream", 0.98f, 0.94f, 0.72f),
                Entry("eyeshadow_2_76", "Pale Gold", 0.96f, 0.90f, 0.68f),
                Entry("eyeshadow_2_77", "Champagne Light", 0.96f, 0.92f, 0.82f),
                Entry("eyeshadow_2_78", "Soft Lilac", 0.92f, 0.86f, 0.96f),
                Entry("eyeshadow_2_79", "Pale Lavender", 0.88f, 0.82f, 0.96f),
                Entry("eyeshadow_2_80", "Mist Purple", 0.90f, 0.84f, 0.98f),
                Entry("eyeshadow_2_81", "Baby Lilac", 0.94f, 0.88f, 0.98f),
                Entry("eyeshadow_2_82", "Powder Pink", 0.96f, 0.82f, 0.88f),
                Entry("eyeshadow_2_83", "Light Rose", 0.96f, 0.84f, 0.88f),
                Entry("eyeshadow_2_84", "Pale Mauve", 0.90f, 0.82f, 0.88f),
                Entry("eyeshadow_2_85", "Soft Sky", 0.84f, 0.92f, 0.98f),
                Entry("eyeshadow_2_86", "Cloud Blue", 0.88f, 0.94f, 0.99f),
                Entry("eyeshadow_2_87", "Pale Aqua", 0.82f, 0.94f, 0.96f),
                Entry("eyeshadow_2_88", "Mint Whisper", 0.86f, 0.96f, 0.92f),
                Entry("eyeshadow_2_89", "Pale Sage", 0.88f, 0.94f, 0.88f),
                Entry("eyeshadow_2_90", "Soft Lime", 0.92f, 0.98f, 0.86f),
                Entry("eyeshadow_2_01", "Soft Brown", 0.45f, 0.32f, 0.28f),
                Entry("eyeshadow_2_02", "Taupe", 0.55f, 0.48f, 0.42f),
                Entry("eyeshadow_2_03", "Chocolate", 0.32f, 0.18f, 0.12f),
                Entry("eyeshadow_2_04", "Espresso", 0.18f, 0.12f, 0.08f),
                Entry("eyeshadow_2_05", "Charcoal", 0.22f, 0.22f, 0.24f),
                Entry("eyeshadow_2_06", "Slate Grey", 0.38f, 0.38f, 0.42f),
                Entry("eyeshadow_2_07", "Smoky Grey", 0.48f, 0.45f, 0.48f),
                Entry("eyeshadow_2_08", "Black Smoke", 0.12f, 0.10f, 0.12f),
                Entry("eyeshadow_2_09", "Soft Black", 0.08f, 0.06f, 0.08f),
                Entry("eyeshadow_2_10", "Warm Nude", 0.72f, 0.58f, 0.48f),
                Entry("eyeshadow_2_11", "Gold", 0.85f, 0.68f, 0.22f),
                Entry("eyeshadow_2_12", "Bronze", 0.72f, 0.48f, 0.22f),
                Entry("eyeshadow_2_13", "Copper", 0.78f, 0.42f, 0.22f),
                Entry("eyeshadow_2_14", "Amber", 0.92f, 0.62f, 0.18f),
                Entry("eyeshadow_2_15", "Champagne", 0.92f, 0.82f, 0.62f),
                Entry("eyeshadow_2_16", "Honey", 0.85f, 0.62f, 0.28f),
                Entry("eyeshadow_2_17", "Terracotta", 0.78f, 0.38f, 0.28f),
                Entry("eyeshadow_2_18", "Rust", 0.72f, 0.28f, 0.18f),
                Entry("eyeshadow_2_19", "Peach", 0.92f, 0.62f, 0.52f),
                Entry("eyeshadow_2_20", "Coral", 0.92f, 0.45f, 0.42f),
                Entry("eyeshadow_2_21", "Rose", 0.85f, 0.42f, 0.52f),
                Entry("eyeshadow_2_22", "Dusty Rose", 0.72f, 0.48f, 0.52f),
                Entry("eyeshadow_2_23", "Mauve", 0.62f, 0.42f, 0.52f),
                Entry("eyeshadow_2_24", "Berry", 0.62f, 0.18f, 0.32f),
                Entry("eyeshadow_2_25", "Burgundy", 0.48f, 0.12f, 0.22f),
                Entry("eyeshadow_2_26", "Hot Pink", 0.92f, 0.28f, 0.55f),
                Entry("eyeshadow_2_27", "Blush Pink", 0.88f, 0.55f, 0.62f),
                Entry("eyeshadow_2_28", "Lilac Pink", 0.82f, 0.58f, 0.72f),
                Entry("eyeshadow_2_29", "Fuchsia", 0.82f, 0.22f, 0.55f),
                Entry("eyeshadow_2_30", "Wine", 0.42f, 0.08f, 0.18f),
                Entry("eyeshadow_2_31", "Lavender", 0.72f, 0.58f, 0.88f),
                Entry("eyeshadow_2_32", "Purple", 0.55f, 0.28f, 0.72f),
                Entry("eyeshadow_2_33", "Plum", 0.48f, 0.22f, 0.48f),
                Entry("eyeshadow_2_34", "Violet", 0.42f, 0.18f, 0.62f),
                Entry("eyeshadow_2_35", "Amethyst", 0.62f, 0.38f, 0.82f),
                Entry("eyeshadow_2_36", "Eggplant", 0.32f, 0.12f, 0.38f),
                Entry("eyeshadow_2_37", "Orchid", 0.78f, 0.42f, 0.82f),
                Entry("eyeshadow_2_38", "Pastel Purple", 0.78f, 0.62f, 0.88f),
                Entry("eyeshadow_2_39", "Navy", 0.12f, 0.18f, 0.38f),
                Entry("eyeshadow_2_40", "Royal Blue", 0.18f, 0.32f, 0.72f),
                Entry("eyeshadow_2_41", "Sky Blue", 0.42f, 0.62f, 0.92f),
                Entry("eyeshadow_2_42", "Teal", 0.12f, 0.48f, 0.52f),
                Entry("eyeshadow_2_43", "Turquoise", 0.22f, 0.68f, 0.72f),
                Entry("eyeshadow_2_44", "Ice Blue", 0.62f, 0.78f, 0.92f),
                Entry("eyeshadow_2_45", "Sapphire", 0.12f, 0.28f, 0.62f),
                Entry("eyeshadow_2_46", "Steel Blue", 0.38f, 0.48f, 0.58f),
                Entry("eyeshadow_2_47", "Olive", 0.42f, 0.48f, 0.22f),
                Entry("eyeshadow_2_48", "Forest", 0.18f, 0.38f, 0.22f),
                Entry("eyeshadow_2_49", "Emerald", 0.12f, 0.58f, 0.38f),
                Entry("eyeshadow_2_50", "Mint", 0.48f, 0.82f, 0.68f),
                Entry("eyeshadow_2_51", "Sage", 0.58f, 0.68f, 0.52f),
                Entry("eyeshadow_2_52", "Jade", 0.22f, 0.62f, 0.48f),
                Entry("eyeshadow_2_53", "Lime", 0.62f, 0.82f, 0.22f),
                Entry("eyeshadow_2_54", "Khaki", 0.62f, 0.58f, 0.38f),
                Entry("eyeshadow_2_55", "White Pearl", 0.92f, 0.90f, 0.88f),
                Entry("eyeshadow_2_56", "Silver", 0.72f, 0.72f, 0.76f),
                Entry("eyeshadow_2_57", "Light Yellow", 0.92f, 0.88f, 0.52f),
                Entry("eyeshadow_2_58", "Cream", 0.92f, 0.85f, 0.72f),
                Entry("eyeshadow_2_59", "Pastel Blue", 0.68f, 0.78f, 0.92f),
                Entry("eyeshadow_2_60", "Pastel Green", 0.68f, 0.88f, 0.72f)
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