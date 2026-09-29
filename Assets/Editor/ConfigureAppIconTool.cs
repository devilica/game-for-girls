#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Generates Android/iOS app icons from splash.png and assigns them in Player Settings.
    /// </summary>
    public static class ConfigureAppIconTool
    {
        private const string SplashPath = "Assets/Resources/UI/splash.png";
        private const string IconsFolder = "Assets/Resources/UI/AppIcons";
        private static readonly Color IconBackgroundColor = new Color(0.98f, 0.82f, 0.90f, 1f);

        [MenuItem("Dress Up Game/Configure App Icon From Splash")]
        public static void ConfigureAppIconFromSplash()
        {
            Sprite splashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SplashPath);
            if (splashSprite == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Splash Image",
                    "Could not find splash.png at:\n" + SplashPath,
                    "OK");
                return;
            }

            ConfigureFromSplashSprite(splashSprite);
            AssetDatabase.SaveAssets();
            Debug.Log("Dress Up Game: App icons configured from splash.png.");
        }

        public static void ConfigureFromSplashSprite(Sprite splashSprite)
        {
            if (splashSprite == null)
            {
                return;
            }

            EnsureIconsFolderExists();

            Texture2D source = ExtractSpriteTexture(splashSprite);
            if (source == null)
            {
                Debug.LogError("Dress Up Game: Could not read splash.png for app icon generation.");
                return;
            }

            try
            {
                ConfigurePlatformIcons(NamedBuildTarget.Android, "android", source);
                ConfigurePlatformIcons(NamedBuildTarget.iOS, "ios", source);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        private static void ConfigurePlatformIcons(NamedBuildTarget buildTarget, string filePrefix, Texture2D source)
        {
            PlatformIconKind[] kinds = PlayerSettings.GetSupportedIconKinds(buildTarget);
            if (kinds == null || kinds.Length == 0)
            {
                return;
            }

            foreach (PlatformIconKind kind in kinds)
            {
                PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(buildTarget, kind);
                if (icons == null || icons.Length == 0)
                {
                    continue;
                }

                string kindName = kind.ToString().ToLowerInvariant();
                for (int i = 0; i < icons.Length; i++)
                {
                    PlatformIcon icon = icons[i];
                    int size = icon.width;

                    if (icon.maxLayerCount > 1)
                    {
                        Texture2D foreground = SaveProjectTexture(
                            BuildForeground(source, size),
                            $"{IconsFolder}/{filePrefix}_{kindName}_{size}_fg.png");
                        Texture2D background = SaveProjectTexture(
                            CreateSolidTexture(size, size, IconBackgroundColor),
                            $"{IconsFolder}/{filePrefix}_{kindName}_{size}_bg.png");
                        icon.SetTextures(new[] { foreground, background });
                    }
                    else
                    {
                        Texture2D iconTexture = SaveProjectTexture(
                            ResizeTexture(source, size, size),
                            $"{IconsFolder}/{filePrefix}_{kindName}_{size}.png");
                        icon.SetTexture(iconTexture);
                    }
                }

                PlayerSettings.SetPlatformIcons(buildTarget, kind, icons);
            }
        }

        private static void EnsureIconsFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(IconsFolder))
            {
                const string parent = "Assets/Resources/UI";
                AssetDatabase.CreateFolder(parent, "AppIcons");
            }
        }

        private static Texture2D ExtractSpriteTexture(Sprite sprite)
        {
            string assetPath = AssetDatabase.GetAssetPath(sprite.texture);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            bool restoreUnreadable = false;

            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(assetPath);
                restoreUnreadable = true;
            }

            Rect rect = sprite.textureRect;
            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);
            Color[] pixels = sprite.texture.GetPixels(
                Mathf.RoundToInt(rect.x),
                Mathf.RoundToInt(rect.y),
                width,
                height);

            Texture2D extracted = new Texture2D(width, height, TextureFormat.RGBA32, false);
            extracted.SetPixels(pixels);
            extracted.Apply();

            if (restoreUnreadable && importer != null)
            {
                importer.isReadable = false;
                AssetDatabase.ImportAsset(assetPath);
            }

            return extracted;
        }

        private static Texture2D BuildForeground(Texture2D source, int size)
        {
            Texture2D canvas = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] clearPixels = new Color[size * size];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = Color.clear;
            }

            canvas.SetPixels(clearPixels);

            int inset = Mathf.Max(1, Mathf.RoundToInt(size * 0.06f));
            int innerSize = size - (inset * 2);
            Texture2D scaled = ResizeTexture(source, innerSize, innerSize);
            canvas.SetPixels(inset, inset, innerSize, innerSize, scaled.GetPixels());
            canvas.Apply();
            Object.DestroyImmediate(scaled);
            return canvas;
        }

        private static Texture2D CreateSolidTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            RenderTexture renderTarget = RenderTexture.GetTemporary(width, height);
            renderTarget.filterMode = FilterMode.Bilinear;

            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(source, renderTarget);
            RenderTexture.active = renderTarget;

            Texture2D resized = new Texture2D(width, height, TextureFormat.RGBA32, false);
            resized.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            resized.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTarget);
            return resized;
        }

        private static Texture2D SaveProjectTexture(Texture2D texture, string assetPath)
        {
            byte[] pngBytes = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);

            File.WriteAllBytes(assetPath, pngBytes);
            AssetDatabase.ImportAsset(assetPath);

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                AssetDatabase.ImportAsset(assetPath);
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }
    }
}
#endif
