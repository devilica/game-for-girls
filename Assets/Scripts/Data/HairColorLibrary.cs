using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Maps hair color presets to Unity colors for SpriteRenderer tinting.
    /// </summary>
    [CreateAssetMenu(fileName = "HairColorLibrary", menuName = "Dress Up Game/Hair Color Library")]
    public class HairColorLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct ColorEntry
        {
            public HairColorPreset preset;
            public Color color;
            public string displayName;
        }

        [SerializeField] private HairColorPreset defaultPreset = HairColorPreset.Original;

        [SerializeField] private ColorEntry[] colors =
        {
            new ColorEntry { preset = HairColorPreset.Original, color = Color.white, displayName = "Original" },
            new ColorEntry { preset = HairColorPreset.Black, color = new Color(0.12f, 0.12f, 0.12f), displayName = "Black" },
            new ColorEntry { preset = HairColorPreset.Brown, color = new Color(0.45f, 0.28f, 0.16f), displayName = "Brown" },
            new ColorEntry { preset = HairColorPreset.DarkBrown, color = new Color(0.28f, 0.16f, 0.08f), displayName = "Dark Brown" },
            new ColorEntry { preset = HairColorPreset.Blonde, color = new Color(0.95f, 0.82f, 0.45f), displayName = "Blonde" },
            new ColorEntry { preset = HairColorPreset.Red, color = new Color(0.75f, 0.18f, 0.12f), displayName = "Red" },
            new ColorEntry { preset = HairColorPreset.Pink, color = new Color(0.95f, 0.45f, 0.65f), displayName = "Pink" },
            new ColorEntry { preset = HairColorPreset.Purple, color = new Color(0.55f, 0.25f, 0.75f), displayName = "Purple" },
            new ColorEntry { preset = HairColorPreset.DarkGrey, color = new Color(0.22f, 0.22f, 0.24f), displayName = "Dark Grey" },
            new ColorEntry { preset = HairColorPreset.Grey, color = new Color(0.45f, 0.45f, 0.48f), displayName = "Grey" },
            new ColorEntry { preset = HairColorPreset.Silver, color = new Color(0.72f, 0.72f, 0.76f), displayName = "Silver" },
            new ColorEntry { preset = HairColorPreset.Platinum, color = new Color(0.88f, 0.86f, 0.82f), displayName = "Platinum" },
            new ColorEntry { preset = HairColorPreset.Chestnut, color = new Color(0.55f, 0.28f, 0.14f), displayName = "Chestnut" },
            new ColorEntry { preset = HairColorPreset.Auburn, color = new Color(0.62f, 0.22f, 0.12f), displayName = "Auburn" },
            new ColorEntry { preset = HairColorPreset.GoldenBlonde, color = new Color(0.92f, 0.72f, 0.28f), displayName = "Golden Blonde" },
            new ColorEntry { preset = HairColorPreset.StrawberryBlonde, color = new Color(0.92f, 0.58f, 0.38f), displayName = "Strawberry Blonde" },
            new ColorEntry { preset = HairColorPreset.Burgundy, color = new Color(0.48f, 0.08f, 0.18f), displayName = "Burgundy" },
            new ColorEntry { preset = HairColorPreset.Copper, color = new Color(0.82f, 0.42f, 0.18f), displayName = "Copper" },
            new ColorEntry { preset = HairColorPreset.Orange, color = new Color(0.92f, 0.48f, 0.12f), displayName = "Orange" },
            new ColorEntry { preset = HairColorPreset.Peach, color = new Color(0.98f, 0.68f, 0.48f), displayName = "Peach" },
            new ColorEntry { preset = HairColorPreset.ForestGreen, color = new Color(0.18f, 0.42f, 0.22f), displayName = "Forest Green" },
            new ColorEntry { preset = HairColorPreset.OliveGreen, color = new Color(0.42f, 0.48f, 0.22f), displayName = "Olive Green" },
            new ColorEntry { preset = HairColorPreset.MintGreen, color = new Color(0.55f, 0.88f, 0.72f), displayName = "Mint Green" },
            new ColorEntry { preset = HairColorPreset.EmeraldGreen, color = new Color(0.12f, 0.72f, 0.48f), displayName = "Emerald Green" },
            new ColorEntry { preset = HairColorPreset.NavyBlue, color = new Color(0.12f, 0.18f, 0.42f), displayName = "Navy Blue" },
            new ColorEntry { preset = HairColorPreset.RoyalBlue, color = new Color(0.18f, 0.38f, 0.82f), displayName = "Royal Blue" },
            new ColorEntry { preset = HairColorPreset.SkyBlue, color = new Color(0.45f, 0.72f, 0.95f), displayName = "Sky Blue" },
            new ColorEntry { preset = HairColorPreset.Teal, color = new Color(0.12f, 0.58f, 0.62f), displayName = "Teal" },
            new ColorEntry { preset = HairColorPreset.Lavender, color = new Color(0.72f, 0.58f, 0.92f), displayName = "Lavender" },
            new ColorEntry { preset = HairColorPreset.Magenta, color = new Color(0.82f, 0.18f, 0.58f), displayName = "Magenta" },
            new ColorEntry { preset = HairColorPreset.RoseGold, color = new Color(0.88f, 0.58f, 0.52f), displayName = "Rose Gold" },
            new ColorEntry { preset = HairColorPreset.White, color = new Color(0.98f, 0.98f, 0.96f), displayName = "White" },
            new ColorEntry { preset = HairColorPreset.OffWhite, color = new Color(0.94f, 0.92f, 0.88f), displayName = "Off White" },
            new ColorEntry { preset = HairColorPreset.Ivory, color = new Color(0.96f, 0.94f, 0.86f), displayName = "Ivory" },
            new ColorEntry { preset = HairColorPreset.Cream, color = new Color(0.98f, 0.95f, 0.82f), displayName = "Cream" },
            new ColorEntry { preset = HairColorPreset.LightYellow, color = new Color(0.98f, 0.95f, 0.62f), displayName = "Light Yellow" },
            new ColorEntry { preset = HairColorPreset.Lemon, color = new Color(0.98f, 0.92f, 0.38f), displayName = "Lemon" },
            new ColorEntry { preset = HairColorPreset.Canary, color = new Color(0.95f, 0.88f, 0.22f), displayName = "Canary" },
            new ColorEntry { preset = HairColorPreset.Butter, color = new Color(0.98f, 0.90f, 0.55f), displayName = "Butter" },
            new ColorEntry { preset = HairColorPreset.Champagne, color = new Color(0.95f, 0.88f, 0.72f), displayName = "Champagne" },
            new ColorEntry { preset = HairColorPreset.Honey, color = new Color(0.92f, 0.78f, 0.38f), displayName = "Honey" },
            new ColorEntry { preset = HairColorPreset.Sand, color = new Color(0.88f, 0.78f, 0.62f), displayName = "Sand" },
            new ColorEntry { preset = HairColorPreset.Beige, color = new Color(0.82f, 0.72f, 0.58f), displayName = "Beige" },
            new ColorEntry { preset = HairColorPreset.PastelPink, color = new Color(0.98f, 0.82f, 0.88f), displayName = "Pastel Pink" },
            new ColorEntry { preset = HairColorPreset.BabyPink, color = new Color(0.98f, 0.78f, 0.85f), displayName = "Baby Pink" },
            new ColorEntry { preset = HairColorPreset.CottonCandy, color = new Color(0.95f, 0.72f, 0.92f), displayName = "Cotton Candy" },
            new ColorEntry { preset = HairColorPreset.PastelBlue, color = new Color(0.78f, 0.88f, 0.98f), displayName = "Pastel Blue" },
            new ColorEntry { preset = HairColorPreset.BabyBlue, color = new Color(0.72f, 0.85f, 0.98f), displayName = "Baby Blue" },
            new ColorEntry { preset = HairColorPreset.PowderBlue, color = new Color(0.82f, 0.92f, 0.98f), displayName = "Powder Blue" },
            new ColorEntry { preset = HairColorPreset.PastelGreen, color = new Color(0.78f, 0.95f, 0.82f), displayName = "Pastel Green" },
            new ColorEntry { preset = HairColorPreset.SageGreen, color = new Color(0.72f, 0.82f, 0.68f), displayName = "Sage Green" },
            new ColorEntry { preset = HairColorPreset.PastelPurple, color = new Color(0.85f, 0.72f, 0.95f), displayName = "Pastel Purple" },
            new ColorEntry { preset = HairColorPreset.Lilac, color = new Color(0.88f, 0.78f, 0.95f), displayName = "Lilac" },
            new ColorEntry { preset = HairColorPreset.PastelOrange, color = new Color(0.98f, 0.82f, 0.62f), displayName = "Pastel Orange" },
            new ColorEntry { preset = HairColorPreset.Apricot, color = new Color(0.98f, 0.78f, 0.58f), displayName = "Apricot" },
            new ColorEntry { preset = HairColorPreset.Coral, color = new Color(0.98f, 0.65f, 0.58f), displayName = "Coral" },
            new ColorEntry { preset = HairColorPreset.LightCoral, color = new Color(0.98f, 0.72f, 0.68f), displayName = "Light Coral" },
            new ColorEntry { preset = HairColorPreset.IceBlue, color = new Color(0.85f, 0.92f, 0.98f), displayName = "Ice Blue" },
            new ColorEntry { preset = HairColorPreset.Pearl, color = new Color(0.92f, 0.90f, 0.88f), displayName = "Pearl" },
            new ColorEntry { preset = HairColorPreset.RoseQuartz, color = new Color(0.95f, 0.82f, 0.85f), displayName = "Rose Quartz" },
            new ColorEntry { preset = HairColorPreset.SoftGold, color = new Color(0.95f, 0.88f, 0.55f), displayName = "Soft Gold" }
        };

        public ColorEntry[] Colors => colors;

        public Color GetColor(HairColorPreset preset)
        {
            foreach (ColorEntry entry in colors)
            {
                if (entry.preset == preset)
                {
                    return entry.color;
                }
            }

            return Color.white;
        }

        public HairColorPreset GetDefaultPreset()
        {
            return defaultPreset;
        }
    }
}
