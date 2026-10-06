#if UNITY_EDITOR
using System.IO;
using DressUpGame.Pet;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class GenerateBow6ColorVariantsTool
    {
        private const string HairbowsFolder = "Assets/Sprites/Pet/Hairbows";
        private const int MaxBowIndexToPrune = 30;

        [MenuItem("Dress Up Game/Generate Bow 6 Color Variants")]
        public static void GenerateBow6ColorVariants()
        {
            string sourcePath = $"{HairbowsFolder}/{BowRecolorPalette.BaseBowId}.png";
            if (!File.Exists(sourcePath))
            {
                EditorUtility.DisplayDialog("Missing Sprite", $"Could not find {sourcePath}.", "OK");
                return;
            }

            EnsureTextureReadable(sourcePath);

            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (source == null)
            {
                EditorUtility.DisplayDialog("Load Failed", "Could not load bow_6 texture.", "OK");
                return;
            }

            Color32[] sourcePixels = source.GetPixels32();
            int width = source.width;
            int height = source.height;

            for (int i = 0; i < BowRecolorPalette.VariantCount; i++)
            {
                BowRecolorPalette.BowVariantSpec spec = BowRecolorPalette.AllVariants[i];
                int bowIndex = BowRecolorPalette.FirstOutputIndex + i;
                Color32[] outputPixels = new Color32[sourcePixels.Length];

                for (int p = 0; p < sourcePixels.Length; p++)
                {
                    int x = p % width;
                    int y = p / width;
                    Color32 pixel = sourcePixels[p];
                    BowRecolorPalette.ApplyVariantPixel(
                        x,
                        y,
                        width,
                        pixel.r,
                        pixel.g,
                        pixel.b,
                        pixel.a,
                        spec,
                        out byte nr,
                        out byte ng,
                        out byte nb);
                    outputPixels[p] = new Color32(nr, ng, nb, pixel.a);
                }

                Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);
                output.SetPixels32(outputPixels);
                output.Apply();

                string outputPath = $"{HairbowsFolder}/bow_{bowIndex}.png";
                File.WriteAllBytes(outputPath, output.EncodeToPNG());
                Object.DestroyImmediate(output);
            }

            RemoveExtraBowVariants();
            AssetDatabase.Refresh();
            ConfigureVariantImports();
            GeneratePetItemsTool.GeneratePetItems();

            int last = BowRecolorPalette.FirstOutputIndex + BowRecolorPalette.VariantCount - 1;
            Debug.Log($"Dress Up Game: Generated bow_{BowRecolorPalette.FirstOutputIndex} … bow_{last} from bow_6.");
        }

        private static void EnsureTextureReadable(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                changed = true;
            }

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void RemoveExtraBowVariants()
        {
            int firstExtra = BowRecolorPalette.FirstOutputIndex + BowRecolorPalette.VariantCount;
            for (int bowIndex = firstExtra; bowIndex <= MaxBowIndexToPrune; bowIndex++)
            {
                string png = $"{HairbowsFolder}/bow_{bowIndex}.png";
                if (File.Exists(png))
                {
                    AssetDatabase.DeleteAsset(png);
                }

                string item = $"Assets/Resources/Items/Pet/Hairbows/bow_{bowIndex}.asset";
                if (File.Exists(item))
                {
                    AssetDatabase.DeleteAsset(item);
                }
            }
        }

        private static void ConfigureVariantImports()
        {
            PetVariantSpriteImportUtility.AlignBowVariantsFromBow6();
        }
    }
}
#endif
