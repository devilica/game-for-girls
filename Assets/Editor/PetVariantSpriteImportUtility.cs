#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Keeps recolored bow/collar PNG import settings aligned with their layout reference sprites.
    /// </summary>
    public static class PetVariantSpriteImportUtility
    {
        private const string HairbowsFolder = "Assets/Sprites/Pet/Hairbows";
        private const string CollarFolder = "Assets/Sprites/Pet/Collar";

        public static int AlignAllBowAndCollarVariantImports()
        {
            int changed = 0;
            changed += AlignVariantsFromReference($"{HairbowsFolder}/bow_6.png", HairbowsFolder, "bow", 8, 20);
            changed += AlignVariantsFromReference($"{CollarFolder}/collar_4.png", CollarFolder, "collar", 7, 12);
            changed += AlignVariantsFromReference($"{CollarFolder}/collar_2.png", CollarFolder, "collar", 13, 17);
            return changed;
        }

        public static int AlignBowVariantsFromBow6() =>
            AlignVariantsFromReference($"{HairbowsFolder}/bow_6.png", HairbowsFolder, "bow", 8, 20);

        public static int AlignCollar4Variants() =>
            AlignVariantsFromReference($"{CollarFolder}/collar_4.png", CollarFolder, "collar", 7, 12);

        public static int AlignCollar2Variants() =>
            AlignVariantsFromReference($"{CollarFolder}/collar_2.png", CollarFolder, "collar", 13, 17);

        public static int AlignVariantsFromReference(
            string referencePngPath,
            string folder,
            string filePrefix,
            int firstIndex,
            int lastIndex)
        {
            TextureImporter referenceImporter = AssetImporter.GetAtPath(referencePngPath) as TextureImporter;
            if (referenceImporter == null)
            {
                Debug.LogWarning($"Dress Up Game: Missing reference sprite import at {referencePngPath}.");
                return 0;
            }

            int changed = 0;
            for (int index = firstIndex; index <= lastIndex; index++)
            {
                string targetPath = $"{folder}/{filePrefix}_{index}.png";
                if (!File.Exists(targetPath))
                {
                    continue;
                }

                if (ApplyReferenceImportSettings(referenceImporter, targetPath))
                {
                    changed++;
                }
            }

            return changed;
        }

        public static bool ApplyReferenceImportSettings(TextureImporter referenceImporter, string targetPngPath)
        {
            TextureImporter targetImporter = AssetImporter.GetAtPath(targetPngPath) as TextureImporter;
            if (targetImporter == null)
            {
                return false;
            }

            CopySpriteImportSettings(referenceImporter, targetImporter);
            targetImporter.SaveAndReimport();
            return true;
        }

        public static void CopySpriteImportSettings(TextureImporter source, TextureImporter target)
        {
            target.textureType = TextureImporterType.Sprite;
            target.spriteImportMode = source.spriteImportMode;
            target.spritePixelsPerUnit = source.spritePixelsPerUnit;
            target.alphaIsTransparency = source.alphaIsTransparency;
            target.filterMode = source.filterMode;
            target.mipmapEnabled = source.mipmapEnabled;
            target.textureCompression = source.textureCompression;

            TextureImporterSettings sourceSettings = new TextureImporterSettings();
            source.ReadTextureSettings(sourceSettings);

            TextureImporterSettings targetSettings = new TextureImporterSettings();
            target.ReadTextureSettings(targetSettings);
            targetSettings.spriteMode = sourceSettings.spriteMode;
            targetSettings.spriteMeshType = sourceSettings.spriteMeshType;
            targetSettings.spriteAlignment = sourceSettings.spriteAlignment;
            targetSettings.spritePivot = sourceSettings.spritePivot;
            targetSettings.spriteExtrude = sourceSettings.spriteExtrude;
            targetSettings.spriteGenerateFallbackPhysicsShape = sourceSettings.spriteGenerateFallbackPhysicsShape;
            target.SetTextureSettings(targetSettings);
        }

        public static int GetDefaultPixelsPerUnitForPetSpritePath(string assetPath)
        {
            string normalized = assetPath.Replace('\\', '/');
            if (normalized.Contains("/Sprites/Pet/Pets/"))
            {
                return 256;
            }

            if (normalized.Contains("/Sprites/Pet/Hairbows/")
                || normalized.Contains("/Sprites/Pet/Collar/")
                || normalized.Contains("/Sprites/Pet/Glasses/"))
            {
                return 100;
            }

            return 256;
        }
    }
}
#endif
