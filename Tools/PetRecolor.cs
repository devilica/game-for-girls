using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

class PetRecolor
{
    const float MaxDist = 58f;

    static readonly string[][] Variants =
    {
        new[] { "dark_yellow", "154", "126", "22" },
        new[] { "dark_orange", "168", "88", "32" },
        new[] { "grey", "110", "110", "110" },
        new[] { "black", "32", "32", "32" },
        new[] { "brown", "74", "52", "37" },
        new[] { "red", "168", "42", "48" },
        new[] { "green", "52", "118", "58" }
    };

    static readonly string[][] SingleKeyPets =
    {
        new[] { "pet_1", "178", "104", "51" },
        new[] { "pet_2", "136", "107", "91" }
    };

    static int Main(string[] args)
    {
        string basePetId = args.Length > 0 ? args[0] : "pet_1";
        string srcPath = args.Length > 1
            ? args[1]
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Assets", "Sprites", "Pet", "Pets", basePetId + ".png");
        srcPath = Path.GetFullPath(srcPath);
        if (!File.Exists(srcPath))
        {
            Console.Error.WriteLine("Missing source: " + srcPath);
            return 1;
        }

        byte[] key = ResolveSingleKey(basePetId);
        if (key == null)
        {
            Console.Error.WriteLine("Unknown pet id. Use pet_1 or pet_2.");
            return 1;
        }

        string outDir = Path.GetDirectoryName(srcPath);
        using (Bitmap source = new Bitmap(srcPath))
        {
            BitmapData srcData = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            int byteCount = Math.Abs(srcData.Stride) * source.Height;
            byte[] srcBytes = new byte[byteCount];
            Marshal.Copy(srcData.Scan0, srcBytes, 0, byteCount);
            source.UnlockBits(srcData);

            foreach (string[] variant in Variants)
            {
                byte tdR = byte.Parse(variant[1]);
                byte tdG = byte.Parse(variant[2]);
                byte tdB = byte.Parse(variant[3]);

                byte[] outBytes = (byte[])srcBytes.Clone();
                for (int i = 0; i < outBytes.Length; i += 4)
                {
                    byte b = outBytes[i];
                    byte g = outBytes[i + 1];
                    byte r = outBytes[i + 2];
                    byte a = outBytes[i + 3];
                    if (a < 8)
                    {
                        continue;
                    }

                    RecolorSingle(ref r, ref g, ref b, key[0], key[1], key[2], tdR, tdG, tdB);
                    outBytes[i] = b;
                    outBytes[i + 1] = g;
                    outBytes[i + 2] = r;
                }

                using (Bitmap output = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb))
                {
                    BitmapData outData = output.LockBits(
                        new Rectangle(0, 0, output.Width, output.Height),
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format32bppArgb);
                    Marshal.Copy(outBytes, 0, outData.Scan0, outBytes.Length);
                    output.UnlockBits(outData);
                    string outPath = Path.Combine(outDir, basePetId + "_" + variant[0] + ".png");
                    output.Save(outPath, ImageFormat.Png);
                    Console.WriteLine("Wrote " + outPath);
                }
            }
        }

        return 0;
    }

    static void RecolorSingle(ref byte r, ref byte g, ref byte b, byte kr, byte kg, byte kb, byte tr, byte tg, byte tb)
    {
        float strength = GetStrength(r, g, b, kr, kg, kb);
        if (strength <= 0f)
        {
            return;
        }

        r = Lerp(r, tr, strength);
        g = Lerp(g, tg, strength);
        b = Lerp(b, tb, strength);
    }

    static float GetStrength(byte r, byte g, byte b, byte kr, byte kg, byte kb)
    {
        float dr = r - kr;
        float dg = g - kg;
        float db = b - kb;
        float dist = (float)Math.Sqrt(dr * dr + dg * dg + db * db);
        if (dist > MaxDist)
        {
            return 0f;
        }

        return 1f - Math.Min(1f, dist / MaxDist);
    }

    static byte Lerp(byte from, byte to, float t)
    {
        return (byte)Math.Round(from + (to - from) * t);
    }

    static byte[] ResolveSingleKey(string basePetId)
    {
        foreach (string[] entry in SingleKeyPets)
        {
            if (string.Equals(entry[0], basePetId, StringComparison.OrdinalIgnoreCase))
            {
                return new[]
                {
                    byte.Parse(entry[1]),
                    byte.Parse(entry[2]),
                    byte.Parse(entry[3])
                };
            }
        }

        return null;
    }
}
