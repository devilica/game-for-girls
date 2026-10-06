using System;

namespace DressUpGame.Pet
{
    /// <summary>
    /// Shared fur recolor targets and source key colors per base pet sprite.
    /// </summary>
    public static class PetRecolorPalette
    {
        public static readonly string[] ColorSuffixes =
        {
            "dark_yellow",
            "dark_orange",
            "grey",
            "black",
            "brown",
            "red",
            "green"
        };

        public static readonly (string suffix, byte r, byte g, byte b)[] TargetColors =
        {
            ("dark_yellow", 154, 126, 22),
            ("dark_orange", 168, 88, 32),
            ("grey", 110, 110, 110),
            ("black", 32, 32, 32),
            ("brown", 74, 52, 37),
            ("red", 168, 42, 48),
            ("green", 52, 118, 58)
        };

        public const float ReplaceColorMaxDistance = 58f;

        public static readonly string[] RecolorBasePetIds = { "pet_1", "pet_2" };

        public static bool IsVariantSkipped(string basePetId, string colorSuffix) =>
            string.Equals(basePetId, "pet_1", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(colorSuffix, "dark_orange", StringComparison.OrdinalIgnoreCase);

        public readonly struct FurRgb
        {
            public readonly byte R;
            public readonly byte G;
            public readonly byte B;

            public FurRgb(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
            }
        }

        public static bool SupportsRecolor(string basePetId) =>
            TryGetSourceKeyRgb(basePetId, out _, out _, out _);

        public static bool TryGetSourceKeyRgb(string basePetId, out byte r, out byte g, out byte b)
        {
            switch (basePetId?.ToLowerInvariant())
            {
                case "pet_1":
                    r = 178;
                    g = 104;
                    b = 51;
                    return true;
                case "pet_2":
                    r = 136;
                    g = 107;
                    b = 91;
                    return true;
                default:
                    r = g = b = 0;
                    return false;
            }
        }

        public static void RecolorPixelSingleKey(
            byte pr,
            byte pg,
            byte pb,
            byte pa,
            FurRgb key,
            FurRgb target,
            out byte r,
            out byte g,
            out byte b)
        {
            r = pr;
            g = pg;
            b = pb;
            if (pa < 8)
            {
                return;
            }

            if (!TryGetReplaceStrength(pr, pg, pb, key, out float strength))
            {
                return;
            }

            r = LerpByte(pr, target.R, strength);
            g = LerpByte(pg, target.G, strength);
            b = LerpByte(pb, target.B, strength);
        }

        private static bool TryGetReplaceStrength(byte pr, byte pg, byte pb, FurRgb key, out float strength)
        {
            strength = GetReplaceStrength(pr, pg, pb, key);
            return strength > 0f;
        }

        private static float GetReplaceStrength(byte pr, byte pg, byte pb, FurRgb key)
        {
            float dr = pr - key.R;
            float dg = pg - key.G;
            float db = pb - key.B;
            float dist = (float)Math.Sqrt(dr * dr + dg * dg + db * db);
            if (dist > ReplaceColorMaxDistance)
            {
                return 0f;
            }

            return 1f - Math.Min(1f, dist / ReplaceColorMaxDistance);
        }

        private static byte LerpByte(byte from, byte to, float t) =>
            (byte)Math.Round(from + (to - from) * t);

    }
}
