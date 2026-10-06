#if UNITY_EDITOR
using System.IO;
using DressUpGame.Pet;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class GeneratePetRecolorVariantsTool
    {
        private const string OutputFolder = "Assets/Sprites/Pet/Pets";

        [MenuItem("Dress Up Game/Generate Pet 1 Color Variants")]
        public static void GeneratePet1ColorVariants() => GenerateForBasePet("pet_1");

        [MenuItem("Dress Up Game/Generate Pet 2 Color Variants")]
        public static void GeneratePet2ColorVariants() => GenerateForBasePet("pet_2");

        private static void GenerateForBasePet(string basePetId)
        {
            if (!PetRecolorPalette.TryGetSourceKeyRgb(basePetId, out byte kr, out byte kg, out byte kb))
            {
                EditorUtility.DisplayDialog("Unsupported Pet", $"No recolor key defined for {basePetId}.", "OK");
                return;
            }

            string sourceSpritePath = $"{OutputFolder}/{basePetId}.png";
            if (!File.Exists(sourceSpritePath))
            {
                EditorUtility.DisplayDialog("Missing Sprite", $"Could not find {sourceSpritePath}.", "OK");
                return;
            }

            var sourceKeyColor = new PetRecolorPalette.FurRgb(kr, kg, kb);
            EnsureTextureReadable(sourceSpritePath);

            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourceSpritePath);
            if (source == null)
            {
                EditorUtility.DisplayDialog("Load Failed", $"Could not load {basePetId} texture.", "OK");
                return;
            }

            Color32[] sourcePixels = source.GetPixels32();
            int width = source.width;
            int height = source.height;

            foreach ((string suffix, byte tr, byte tg, byte tb) in PetRecolorPalette.TargetColors)
            {
                if (PetRecolorPalette.IsVariantSkipped(basePetId, suffix))
                {
                    continue;
                }

                var targetColor = new PetRecolorPalette.FurRgb(tr, tg, tb);
                Color32[] outputPixels = new Color32[sourcePixels.Length];
                for (int i = 0; i < sourcePixels.Length; i++)
                {
                    Color32 pixel = sourcePixels[i];
                    PetRecolorPalette.RecolorPixelSingleKey(
                        pixel.r,
                        pixel.g,
                        pixel.b,
                        pixel.a,
                        sourceKeyColor,
                        targetColor,
                        out byte nr,
                        out byte ng,
                        out byte nb);
                    outputPixels[i] = new Color32(nr, ng, nb, pixel.a);
                }

                Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);
                output.SetPixels32(outputPixels);
                output.Apply();

                string outputPath = $"{OutputFolder}/{basePetId}_{suffix}.png";
                File.WriteAllBytes(outputPath, output.EncodeToPNG());
                Object.DestroyImmediate(output);
            }

            AssetDatabase.Refresh();
            ConfigureVariantImports(basePetId);
            GeneratePetItemsTool.GeneratePetItems();

            Debug.Log($"Dress Up Game: Generated {basePetId} recolor sprites and refreshed pet items.");
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

            if (importer.spritePixelsPerUnit != 256)
            {
                importer.spritePixelsPerUnit = 256;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureVariantImports(string basePetId)
        {
            foreach ((string suffix, _, _, _) in PetRecolorPalette.TargetColors)
            {
                string path = $"{OutputFolder}/{basePetId}_{suffix}.png";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 256;
                importer.alphaIsTransparency = true;
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
