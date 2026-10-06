#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using DressUpGame.Data;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Dress 11: original mint sprite plus seven recolored variant textures (stored under Resources, not Sprites/Character).
    /// Each catalog item uses its own sprite with white tint so colors read clearly in the UI.
    /// </summary>
    public static class GenerateDress11ColorSpritesTool
    {
        private const int ShapeIndex = 11;
        private const string SourcePath = "Assets/Sprites/Character/Dresses/dress_11.png";
        private const string DressDataFolder = "Assets/Resources/Items/Dresses";
        private const string VariantTextureFolder = "Assets/Resources/Items/Dresses/Dress11VariantTextures";

        private static bool syncInProgress;

        public static bool IsSyncInProgress => syncInProgress;

        private static readonly (string suffix, string fileStem, string displayName, Color fabricColor)[] Variants =
        {
            ("01", "dress_11_yellow", "Yellow", new Color(0.98f, 0.88f, 0.22f)),
            ("02", "dress_11_red", "Red", new Color(0.92f, 0.22f, 0.28f)),
            ("03", "dress_11_orange", "Orange", new Color(0.96f, 0.52f, 0.14f)),
            ("04", "dress_11_blue", "Blue", new Color(0.32f, 0.52f, 0.96f)),
            ("05", "dress_11_purple", "Purple", new Color(0.62f, 0.32f, 0.88f)),
            ("06", "dress_11_brown", "Brown", new Color(0.55f, 0.36f, 0.22f)),
            ("07", "dress_11_green", "Green", new Color(0.28f, 0.68f, 0.38f)),
        };

        [MenuItem("Dress Up Game/Sync Dress 11 Variants")]
        public static void SyncFromMenu()
        {
            Sprite original = AssetDatabase.LoadAssetAtPath<Sprite>(SourcePath);
            DressLayout layout = ReadLayoutFromOriginalItem();
            EnsureVariantTexturesAndItems(layout, original);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Dress 11",
                "Updated Dress 11 catalog: original mint + 7 distinct color variants (yellow, red, orange, blue, purple, brown, green).",
                "OK");
        }

        [MenuItem("Dress Up Game/Validate Dress 11 Variants")]
        public static void ValidateDress11Variants()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("Dress 11 variant validation:");

            string baseGuid = AssetDatabase.AssetPathToGUID(SourcePath);
            if (string.IsNullOrEmpty(baseGuid))
            {
                report.AppendLine($"- FAIL: Missing source sprite at {SourcePath}");
                Debug.LogError(report.ToString());
                return;
            }

            report.AppendLine($"- OK: Base dress_11.png GUID {baseGuid}");

            int failures = 0;
            foreach ((string suffix, string fileStem, string displayName, _) in Variants)
            {
                string itemPath = $"{DressDataFolder}/Dress_{ShapeIndex}_{suffix}.asset";
                DressItem item = AssetDatabase.LoadAssetAtPath<DressItem>(itemPath);
                if (item == null)
                {
                    failures++;
                    report.AppendLine($"- FAIL: Missing {itemPath}");
                    continue;
                }

                if (item.Sprite == null)
                {
                    failures++;
                    report.AppendLine($"- FAIL: dress_11_{suffix} ({displayName}) has no sprite. Run Sync Dress 11 Variants.");
                    continue;
                }

                string spritePath = AssetDatabase.GetAssetPath(item.Sprite);
                string spriteGuid = AssetDatabase.AssetPathToGUID(spritePath);
                if (spriteGuid == baseGuid)
                {
                    failures++;
                    report.AppendLine(
                        $"- FAIL: dress_11_{suffix} ({displayName}) still uses base dress_11.png. Run Sync Dress 11 Variants.");
                    continue;
                }

                string expectedPath = $"{VariantTextureFolder}/{fileStem}.png";
                if (!spritePath.Replace('\\', '/').Equals(expectedPath, System.StringComparison.OrdinalIgnoreCase))
                {
                    failures++;
                    report.AppendLine(
                        $"- FAIL: dress_11_{suffix} sprite path is {spritePath}, expected {expectedPath}.");
                    continue;
                }

                report.AppendLine($"- OK: dress_11_{suffix} ({displayName}) → {fileStem}.png");
            }

            if (failures == 0)
            {
                report.AppendLine("- OK: All 7 color variants use distinct recolored sprites.");
            }

            if (failures > 0)
            {
                Debug.LogError(report.ToString());
            }
            else
            {
                Debug.Log(report.ToString());
            }
        }

        public static bool IsDressShapeWithRecoloredVariants(int shapeIndex) => shapeIndex == ShapeIndex;

        public static void EnsureVariantTexturesAndItems(DressLayout layout, Sprite originalSprite)
        {
            if (syncInProgress)
            {
                return;
            }

            syncInProgress = true;
            try
            {
                GenerateVariantTextures();
                SyncDress11ItemsInternal(layout, originalSprite);
            }
            finally
            {
                syncInProgress = false;
            }
        }

        public static List<DressItem> LoadDress11ItemsForCatalog()
        {
            List<DressItem> items = new List<DressItem>();
            DressItem original = AssetDatabase.LoadAssetAtPath<DressItem>($"{DressDataFolder}/Dress_{ShapeIndex}.asset");
            if (original != null)
            {
                items.Add(original);
            }

            foreach ((string suffix, _, _, _) in Variants)
            {
                DressItem item = AssetDatabase.LoadAssetAtPath<DressItem>(
                    $"{DressDataFolder}/Dress_{ShapeIndex}_{suffix}.asset");
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private static void GenerateVariantTextures()
        {
            if (!Directory.Exists(VariantTextureFolder))
            {
                Directory.CreateDirectory(VariantTextureFolder);
            }

            Texture2D source = LoadReadableSource();
            if (source == null)
            {
                Debug.LogError("Dress Up Game: Could not load dress_11.png for recoloring.");
                return;
            }

            Color32[] pixels = source.GetPixels32();
            int width = source.width;
            int height = source.height;

            foreach ((string _, string fileStem, string __, Color fabricColor) in Variants)
            {
                Color32[] recolored = new Color32[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                {
                    recolored[i] = RecolorPixel(pixels[i], fabricColor);
                }

                Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);
                output.SetPixels32(recolored);
                output.Apply();

                string pngPath = $"{VariantTextureFolder}/{fileStem}.png";
                File.WriteAllBytes(pngPath, output.EncodeToPNG());
                Object.DestroyImmediate(output);
            }

            Object.DestroyImmediate(source);
            AssetDatabase.Refresh();
            ConfigureImportedSprites();
        }

        private static void SyncDress11ItemsInternal(DressLayout layout, Sprite originalSprite)
        {
            DressItem originalItem = LoadOrCreate<DressItem>($"{DressDataFolder}/Dress_{ShapeIndex}.asset");
            originalItem.SetRuntimeData("dress_11", "Dress 11 Original", originalSprite, Color.white);
            originalItem.SetLayerLayout(layout.Offset, layout.Scale);
            EditorUtility.SetDirty(originalItem);

            foreach ((string suffix, string fileStem, string displayName, _) in Variants)
            {
                string spritePath = $"{VariantTextureFolder}/{fileStem}.png";
                Sprite variantSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                string path = $"{DressDataFolder}/Dress_{ShapeIndex}_{suffix}.asset";
                DressItem item = LoadOrCreate<DressItem>(path);
                if (variantSprite == null)
                {
                    Debug.LogError(
                        $"Dress Up Game: Missing variant texture {spritePath} for dress_11_{suffix}. " +
                        "Run Dress Up Game → Sync Dress 11 Variants.");
                    item.SetRuntimeData($"dress_11_{suffix}", $"Dress 11 {displayName}", null, Color.white);
                    item.SetLayerLayout(layout.Offset, layout.Scale);
                    EditorUtility.SetDirty(item);
                    continue;
                }

                item.SetRuntimeData(
                    $"dress_11_{suffix}",
                    $"Dress 11 {displayName}",
                    variantSprite,
                    Color.white);
                item.SetLayerLayout(layout.Offset, layout.Scale);
                EditorUtility.SetDirty(item);
            }
        }

        private static DressLayout ReadLayoutFromOriginalItem()
        {
            DressItem existing = AssetDatabase.LoadAssetAtPath<DressItem>($"{DressDataFolder}/Dress_{ShapeIndex}.asset");
            if (existing != null)
            {
                return new DressLayout(existing.LayerOffset, existing.LayerScale);
            }

            return DressLayout.Default;
        }

        private static Texture2D LoadReadableSource()
        {
            TextureImporter importer = AssetImporter.GetAtPath(SourcePath) as TextureImporter;
            if (importer == null)
            {
                return null;
            }

            bool wasReadable = importer.isReadable;
            if (!wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(SourcePath);
            if (source == null)
            {
                return null;
            }

            Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            copy.SetPixels32(source.GetPixels32());
            copy.Apply();

            if (!wasReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            return copy;
        }

        private static void ConfigureImportedSprites()
        {
            foreach ((string _, string fileStem, _, _) in Variants)
            {
                string path = $"{VariantTextureFolder}/{fileStem}.png";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.spritePixelsPerUnit = 103.833336f;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        private static Color32 RecolorPixel(Color32 pixel, Color targetFabric)
        {
            if (pixel.a < 8)
            {
                return pixel;
            }

            if (ShouldPreserveDetail(pixel))
            {
                return pixel;
            }

            if (!IsFabricPixel(pixel))
            {
                return pixel;
            }

            float luminance = (0.299f * pixel.r + 0.587f * pixel.g + 0.114f * pixel.b) / 255f;
            Color dark = targetFabric * 0.42f;
            dark.a = 1f;
            Color light = Color.Lerp(targetFabric, Color.white, 0.22f);
            light.a = 1f;
            Color result = Color.Lerp(dark, light, Mathf.Clamp01(luminance * 1.15f));
            return new Color32(
                (byte)(result.r * 255f),
                (byte)(result.g * 255f),
                (byte)(result.b * 255f),
                pixel.a);
        }

        private static bool ShouldPreserveDetail(Color32 pixel)
        {
            float r = pixel.r / 255f;
            float g = pixel.g / 255f;
            float b = pixel.b / 255f;

            if (r < 0.18f && g < 0.18f && b < 0.18f)
            {
                return true;
            }

            if (r > 0.82f && g > 0.82f && b > 0.72f)
            {
                return true;
            }

            if (r > 0.78f && g > 0.68f && b < 0.45f)
            {
                return true;
            }

            if (r > 0.72f && g > 0.42f && g < 0.78f && b < 0.38f)
            {
                return true;
            }

            return false;
        }

        private static bool IsFabricPixel(Color32 pixel)
        {
            int r = pixel.r;
            int g = pixel.g;
            int b = pixel.b;
            return g > r * 0.82f && g > b * 0.88f && g > 70;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public readonly struct DressLayout
        {
            public static DressLayout Default => new DressLayout(Vector3.zero, Vector3.one);

            public Vector3 Offset { get; }
            public Vector3 Scale { get; }

            public DressLayout(Vector3 offset, Vector3 scale)
            {
                Offset = offset;
                Scale = scale;
            }
        }
    }
}
#endif
