#if UNITY_EDITOR
using System.IO;
using DressUpGame.Pet;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class GenerateCollar2ColorVariantsTool
    {
        private const string CollarFolder = "Assets/Sprites/Pet/Collar";

        [MenuItem("Dress Up Game/Generate Collar 2 Color Variants (5)")]
        public static void GenerateCollar2ColorVariants()
        {
            GenerateCollarColorVariantsTool.GenerateFromCollar2();
        }
    }

    /// <summary>Shared collar recolor generation for editor menus.</summary>
    public static class GenerateCollarColorVariantsTool
    {
        private const string CollarFolder = "Assets/Sprites/Pet/Collar";
        private const int MaxCollarIndexToPrune = 25;

        public static void GenerateFromCollar4()
        {
            Generate(
                CollarRecolorPalette.BaseCollarId,
                CollarRecolorPalette.SourceKey,
                CollarRecolorPalette.FirstOutputIndex,
                CollarRecolorPalette.VariantTargetColors,
                CollarRecolorPalette.FirstOutputIndex + CollarRecolorPalette.VariantCount);
        }

        public static void GenerateFromCollar2()
        {
            Generate(
                CollarRecolorPalette.BaseCollar2Id,
                CollarRecolorPalette.Source2Key,
                CollarRecolorPalette.Collar2FirstOutputIndex,
                CollarRecolorPalette.Collar2VariantTargetColors,
                CollarRecolorPalette.Collar2FirstOutputIndex + CollarRecolorPalette.Collar2VariantCount);
        }

        private static void Generate(
            string baseCollarId,
            PetRecolorPalette.FurRgb sourceKey,
            int firstOutputIndex,
            (byte r, byte g, byte b)[] targets,
            int firstIndexToPruneFrom)
        {
            string sourcePath = $"{CollarFolder}/{baseCollarId}.png";
            if (!File.Exists(sourcePath))
            {
                EditorUtility.DisplayDialog("Missing Sprite", $"Could not find {sourcePath}.", "OK");
                return;
            }

            EnsureTextureReadable(sourcePath);

            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (source == null)
            {
                EditorUtility.DisplayDialog("Load Failed", $"Could not load {baseCollarId} texture.", "OK");
                return;
            }

            Color32[] sourcePixels = source.GetPixels32();
            int width = source.width;
            int height = source.height;

            for (int i = 0; i < targets.Length; i++)
            {
                (byte tr, byte tg, byte tb) = targets[i];
                int collarIndex = firstOutputIndex + i;
                var target = new PetRecolorPalette.FurRgb(tr, tg, tb);
                Color32[] outputPixels = new Color32[sourcePixels.Length];

                for (int p = 0; p < sourcePixels.Length; p++)
                {
                    Color32 pixel = sourcePixels[p];
                    CollarRecolorPalette.RecolorPixelWholeCollar(
                        pixel.r,
                        pixel.g,
                        pixel.b,
                        pixel.a,
                        sourceKey,
                        target,
                        out byte nr,
                        out byte ng,
                        out byte nb);
                    outputPixels[p] = new Color32(nr, ng, nb, pixel.a);
                }

                Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);
                output.SetPixels32(outputPixels);
                output.Apply();

                string outputPath = $"{CollarFolder}/collar_{collarIndex}.png";
                File.WriteAllBytes(outputPath, output.EncodeToPNG());
                Object.DestroyImmediate(output);
            }

            PruneExtraCollars(firstIndexToPruneFrom);
            AssetDatabase.Refresh();
            ConfigureVariantImports(firstOutputIndex, targets.Length);
            GeneratePetItemsTool.GeneratePetItems();

            int last = firstOutputIndex + targets.Length - 1;
            Debug.Log($"Dress Up Game: Generated collar_{firstOutputIndex} … collar_{last} from {baseCollarId}.");
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

        private static void PruneExtraCollars(int firstExtraIndex)
        {
            for (int index = firstExtraIndex; index <= MaxCollarIndexToPrune; index++)
            {
                string png = $"{CollarFolder}/collar_{index}.png";
                if (File.Exists(png))
                {
                    AssetDatabase.DeleteAsset(png);
                }

                string item = $"Assets/Resources/Items/Pet/Collar/collar_{index}.asset";
                if (File.Exists(item))
                {
                    AssetDatabase.DeleteAsset(item);
                }
            }
        }

        private static void ConfigureVariantImports(int firstOutputIndex, int count)
        {
            if (firstOutputIndex == CollarRecolorPalette.FirstOutputIndex)
            {
                PetVariantSpriteImportUtility.AlignCollar4Variants();
                return;
            }

            if (firstOutputIndex == CollarRecolorPalette.Collar2FirstOutputIndex)
            {
                PetVariantSpriteImportUtility.AlignCollar2Variants();
            }
        }
    }
}
#endif
