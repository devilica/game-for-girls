using System;

namespace DressUpGame.Pet
{
    public enum BowVariantKind
    {
        Solid,
        Rainbow,
        Glitter
    }

    /// <summary>
    /// Recolor entire bow_6 sprites (preserves shading and dark outlines).
    /// </summary>
    public static class BowRecolorPalette
    {
        public const string BaseBowId = "bow_6";

        public const byte SourceR = 248;
        public const byte SourceG = 246;
        public const byte SourceB = 245;

        public const int FirstOutputIndex = 8;

        public const float MinLuminanceToRecolor = 0.12f;

        public readonly struct BowVariantSpec
        {
            public readonly BowVariantKind Kind;
            public readonly byte R;
            public readonly byte G;
            public readonly byte B;

            public BowVariantSpec(BowVariantKind kind, byte r, byte g, byte b)
            {
                Kind = kind;
                R = r;
                G = g;
                B = b;
            }
        }

        /// <summary>bow_8 … bow_20</summary>
        public static readonly BowVariantSpec[] AllVariants =
        {
            new BowVariantSpec(BowVariantKind.Solid, 240, 160, 190),
            new BowVariantSpec(BowVariantKind.Solid, 168, 42, 48),
            new BowVariantSpec(BowVariantKind.Solid, 90, 160, 220),
            new BowVariantSpec(BowVariantKind.Solid, 160, 120, 210),
            new BowVariantSpec(BowVariantKind.Solid, 100, 190, 140),
            new BowVariantSpec(BowVariantKind.Solid, 230, 120, 45),
            new BowVariantSpec(BowVariantKind.Solid, 200, 150, 230),
            new BowVariantSpec(BowVariantKind.Solid, 218, 175, 55),
            new BowVariantSpec(BowVariantKind.Solid, 192, 192, 200),
            new BowVariantSpec(BowVariantKind.Solid, 160, 220, 150),
            new BowVariantSpec(BowVariantKind.Solid, 140, 210, 120),
            new BowVariantSpec(BowVariantKind.Rainbow, 255, 255, 255),
            new BowVariantSpec(BowVariantKind.Glitter, 240, 180, 220)
        };

        public static int VariantCount => AllVariants.Length;

        public static PetRecolorPalette.FurRgb SourceKey =>
            new PetRecolorPalette.FurRgb(SourceR, SourceG, SourceB);

        private static float SourceLuminance =>
            GetLuminance(SourceR, SourceG, SourceB);

        public static void RecolorPixelWholeBow(
            byte pr,
            byte pg,
            byte pb,
            byte pa,
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

            float keyLum = SourceLuminance;
            if (keyLum < 0.01f)
            {
                keyLum = 0.01f;
            }

            float scale = lum / keyLum;
            r = ScaleChannel(target.R, scale);
            g = ScaleChannel(target.G, scale);
            b = ScaleChannel(target.B, scale);
        }

        public static void ApplyVariantPixel(
            int x,
            int y,
            int width,
            byte pr,
            byte pg,
            byte pb,
            byte pa,
            BowVariantSpec spec,
            out byte r,
            out byte g,
            out byte b)
        {
            switch (spec.Kind)
            {
                case BowVariantKind.Rainbow:
                    ApplyRainbowPixel(x, width, pr, pg, pb, pa, out r, out g, out b);
                    return;
                case BowVariantKind.Glitter:
                    var glitterBase = new PetRecolorPalette.FurRgb(spec.R, spec.G, spec.B);
                    RecolorPixelWholeBow(pr, pg, pb, pa, glitterBase, out r, out g, out b);
                    if (pa >= 8 && GetLuminance(pr, pg, pb) >= MinLuminanceToRecolor)
                    {
                        ApplyGlitterSpeck(x, y, ref r, ref g, ref b);
                    }

                    return;
                default:
                    var solid = new PetRecolorPalette.FurRgb(spec.R, spec.G, spec.B);
                    RecolorPixelWholeBow(pr, pg, pb, pa, solid, out r, out g, out b);
                    return;
            }
        }

        private static void ApplyRainbowPixel(
            int x,
            int width,
            byte pr,
            byte pg,
            byte pb,
            byte pa,
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

            float t = width <= 1 ? 0f : x / (float)(width - 1);
            float hue = t * 360f;
            HsvToRgb(hue, 0.72f, Math.Min(1f, lum * 1.15f + 0.08f), out r, out g, out b);
        }

        private static void ApplyGlitterSpeck(int x, int y, ref byte r, ref byte g, ref byte b)
        {
            int hash = (x * 73856093) ^ (y * 19349663);
            hash = Math.Abs(hash % 1000);
            if (hash < 25)
            {
                r = 255;
                g = 250;
                b = 245;
                return;
            }

            if (hash < 35)
            {
                r = 255;
                g = 230;
                b = 120;
            }
        }

        private static void HsvToRgb(float h, float s, float v, out byte r, out byte g, out byte b)
        {
            h = ((h % 360f) + 360f) % 360f;
            float c = v * s;
            float x = c * (1f - Math.Abs((h / 60f) % 2f - 1f));
            float m = v - c;
            float rf;
            float gf;
            float bf;
            if (h < 60f)
            {
                rf = c;
                gf = x;
                bf = 0f;
            }
            else if (h < 120f)
            {
                rf = x;
                gf = c;
                bf = 0f;
            }
            else if (h < 180f)
            {
                rf = 0f;
                gf = c;
                bf = x;
            }
            else if (h < 240f)
            {
                rf = 0f;
                gf = x;
                bf = c;
            }
            else if (h < 300f)
            {
                rf = x;
                gf = 0f;
                bf = c;
            }
            else
            {
                rf = c;
                gf = 0f;
                bf = x;
            }

            r = (byte)Math.Round((rf + m) * 255f);
            g = (byte)Math.Round((gf + m) * 255f);
            b = (byte)Math.Round((bf + m) * 255f);
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
