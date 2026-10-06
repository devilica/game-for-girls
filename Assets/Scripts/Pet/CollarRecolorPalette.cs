using System;

namespace DressUpGame.Pet
{
    /// <summary>
    /// Full-collar recolor (luminance-mapped), same approach as bow variants — not single-hex replace.
    /// </summary>
    public static class CollarRecolorPalette
    {
        public const string BaseCollarId = "collar_4";

        public const byte SourceR = 252;
        public const byte SourceG = 63;
        public const byte SourceB = 62;

        public const int FirstOutputIndex = 7;

        public static readonly (byte r, byte g, byte b)[] VariantTargetColors =
        {
            (70, 120, 220),
            (55, 150, 75),
            (140, 70, 180),
            (230, 110, 45),
            (240, 100, 150),
            (220, 180, 50)
        };

        public static int VariantCount => VariantTargetColors.Length;

        public static PetRecolorPalette.FurRgb SourceKey =>
            new PetRecolorPalette.FurRgb(SourceR, SourceG, SourceB);

        public const string BaseCollar2Id = "collar_2";

        public const byte Source2R = 89;
        public const byte Source2G = 191;
        public const byte Source2B = 253;

        public const int Collar2FirstOutputIndex = 13;

        public static readonly (byte r, byte g, byte b)[] Collar2VariantTargetColors =
        {
            (168, 42, 48),
            (240, 100, 150),
            (55, 150, 75),
            (140, 70, 180),
            (230, 110, 45)
        };

        public static int Collar2VariantCount => Collar2VariantTargetColors.Length;

        public static PetRecolorPalette.FurRgb Source2Key =>
            new PetRecolorPalette.FurRgb(Source2R, Source2G, Source2B);

        public const float MinLuminanceToRecolor = 0.12f;

        public static void RecolorPixelWholeCollar(
            byte pr,
            byte pg,
            byte pb,
            byte pa,
            PetRecolorPalette.FurRgb sourceKey,
            PetRecolorPalette.FurRgb target,
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

            float lum = GetLuminance(pr, pg, pb);
            if (lum < MinLuminanceToRecolor)
            {
                return;
            }

            float keyLum = GetLuminance(sourceKey.R, sourceKey.G, sourceKey.B);
            if (keyLum < 0.01f)
            {
                keyLum = 0.01f;
            }

            float scale = lum / keyLum;
            r = ScaleChannel(target.R, scale);
            g = ScaleChannel(target.G, scale);
            b = ScaleChannel(target.B, scale);
        }

        private static float GetLuminance(byte cr, byte cg, byte cb) =>
            (0.299f * cr + 0.587f * cg + 0.114f * cb) / 255f;

        private static byte ScaleChannel(byte targetChannel, float scale)
        {
            int value = (int)Math.Round(targetChannel * scale);
            if (value < 0)
            {
                return 0;
            }

            if (value > 255)
            {
                return 255;
            }

            return (byte)value;
        }
    }
}
