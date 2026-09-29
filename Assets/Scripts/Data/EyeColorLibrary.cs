using System.Collections.Generic;
using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Sixty iris target colors for eyes_2 brown recoloring (30 light + 30 dark).
    /// </summary>
    [CreateAssetMenu(fileName = "EyeColorLibrary", menuName = "Dress Up Game/Eye Color Library")]
    public class EyeColorLibrary : ScriptableObject
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

        public void EnsureDefaultColors()
        {
            if (colors == null || colors.Length == 0)
            {
                colors = BuildDefaultColors();
            }
        }

        public static ColorEntry[] BuildDefaultColors()
        {
            List<ColorEntry> entries = new List<ColorEntry>(60);
            AddPair(entries, 1, "Sky Blue", Hex("#8FD4F0"), Hex("#2596BE"));
            AddPair(entries, 2, "Ocean Blue", Hex("#58AEE8"), Hex("#1568A0"));
            AddPair(entries, 3, "Emerald", Hex("#58E088"), Hex("#1A9850"));
            AddPair(entries, 4, "Forest Green", Hex("#48C860"), Hex("#157030"));
            AddPair(entries, 5, "Light Green", Hex("#7EE878"), Hex("#38A838"));
            AddPair(entries, 6, "Spring Green", Hex("#98F088"), Hex("#48C040"));
            AddPair(entries, 7, "Orange", Hex("#FFA040"), Hex("#E06818"));
            AddPair(entries, 8, "Tangerine", Hex("#FF8830"), Hex("#D04810"));
            AddPair(entries, 9, "Red", Hex("#F04848"), Hex("#C01828"));
            AddPair(entries, 10, "Cherry", Hex("#F05868"), Hex("#B01830"));
            AddPair(entries, 11, "Hazel Gold", Hex("#E8C868"), Hex("#A88030"));
            AddPair(entries, 12, "Honey Amber", Hex("#F0B848"), Hex("#C88018"));
            AddPair(entries, 13, "Warm Brown", Hex("#D0A060"), Hex("#906028"));
            AddPair(entries, 14, "Chocolate", Hex("#B87848"), Hex("#704018"));
            AddPair(entries, 15, "Soft Grey", Hex("#B8C0C8"), Hex("#687078"));
            AddPair(entries, 16, "Storm Grey", Hex("#9098A0"), Hex("#404850"));
            AddPair(entries, 17, "Violet", Hex("#C098F0"), Hex("#7848B0"));
            AddPair(entries, 18, "Lavender", Hex("#D0B0F8"), Hex("#9068C0"));
            AddPair(entries, 19, "Rose Pink", Hex("#F088A8"), Hex("#C04068"));
            AddPair(entries, 20, "Magenta", Hex("#F068C0"), Hex("#B02888"));
            AddPair(entries, 21, "Teal", Hex("#40D0C8"), Hex("#188880"));
            AddPair(entries, 22, "Turquoise", Hex("#38D8E8"), Hex("#10A0B0"));
            AddPair(entries, 23, "Mint", Hex("#78F0A8"), Hex("#30B068"));
            AddPair(entries, 24, "Seafoam", Hex("#90F8C8"), Hex("#40B888"));
            AddPair(entries, 25, "Navy", Hex("#5878B8"), Hex("#183058"));
            AddPair(entries, 26, "Ice Blue", Hex("#C8ECFF"), Hex("#88C0E0"));
            AddPair(entries, 27, "Plum", Hex("#D078C0"), Hex("#903868"));
            AddPair(entries, 28, "Copper", Hex("#F09850"), Hex("#C05820"));
            AddPair(entries, 29, "Golden", Hex("#FFE048"), Hex("#E0A818"));
            AddPair(entries, 30, "Ruby", Hex("#E83848"), Hex("#B01020"));
            return entries.ToArray();
        }

        private static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            return Color.white;
        }

        private static void AddPair(
            List<ColorEntry> entries,
            int familyIndex,
            string hueName,
            Color light,
            Color dark)
        {
            int lightSuffix = familyIndex * 2;
            int darkSuffix = familyIndex * 2 + 1;
            entries.Add(Entry($"eyes_2_{lightSuffix:D2}", $"Light {hueName}", light));
            entries.Add(Entry($"eyes_2_{darkSuffix:D2}", $"Dark {hueName}", dark));
        }

        private static ColorEntry Entry(string id, string name, Color color)
        {
            return new ColorEntry
            {
                id = id,
                displayName = name,
                color = color
            };
        }
    }
}
