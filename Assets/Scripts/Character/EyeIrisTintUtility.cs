using DressUpGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.Character
{
    /// <summary>
    /// Applies iris-only tinting to eye sprites via IrisTintSprite material.
    /// </summary>
    public static class EyeIrisTintUtility
    {
        private const string IrisMaterialResourcePath = "Materials/EyeIrisTint";
        private const string IrisUiMaterialResourcePath = "Materials/EyeIrisTintUI";
        private static Material sharedIrisMaterial;
        private static Material sharedIrisUiMaterial;
        private static Material sharedDefaultMaterial;

        public static bool IsOriginalEye(MakeupItem item)
        {
            return item != null && item.Id == "eyes_2_01";
        }

        public static void ApplyToRenderer(SpriteRenderer renderer, MakeupItem item)
        {
            if (renderer == null || item == null)
            {
                return;
            }

            if (IsOriginalEye(item))
            {
                ApplyOriginal(renderer);
                return;
            }

            ApplyIrisTint(renderer, item.TintColor);
        }

        public static void ApplyOriginal(SpriteRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = GetDefaultSpriteMaterial();
            renderer.color = Color.white;
        }

        public static void ApplyIrisTint(SpriteRenderer renderer, Color irisTint)
        {
            if (renderer == null)
            {
                return;
            }

            Material material = GetIrisMaterialInstance(renderer);
            material.SetColor("_IrisTint", irisTint);
            renderer.material = material;
            renderer.color = Color.white;
        }

        public static void ApplyToImage(Image image, MakeupItem item)
        {
            if (image == null || item == null)
            {
                return;
            }

            if (item.IsNoneOption || IsOriginalEye(item))
            {
                ApplyOriginalToImage(image);
                return;
            }

            ApplyIrisTintToImage(image, item.TintColor);
        }

        public static void ApplyOriginalToImage(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.material = null;
            image.color = Color.white;
        }

        public static void ApplyIrisTintToImage(Image image, Color irisTint)
        {
            if (image == null)
            {
                return;
            }

            Material material = GetIrisUiMaterialInstance(image);
            if (material == null)
            {
                ApplyOriginalToImage(image);
                return;
            }

            material.SetColor("_IrisTint", irisTint);
            image.material = material;
            image.color = Color.white;
        }

        private static Material GetDefaultSpriteMaterial()
        {
            if (sharedDefaultMaterial == null)
            {
                sharedDefaultMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            return sharedDefaultMaterial;
        }

        private static Material GetIrisMaterialInstance(SpriteRenderer renderer)
        {
            if (sharedIrisMaterial == null)
            {
                sharedIrisMaterial = Resources.Load<Material>(IrisMaterialResourcePath);
                if (sharedIrisMaterial == null)
                {
                    Shader shader = Shader.Find("DressUpGame/IrisTintSprite");
                    sharedIrisMaterial = shader != null ? new Material(shader) : GetDefaultSpriteMaterial();
                }
            }

            if (renderer.sharedMaterial != null
                && renderer.sharedMaterial.shader != null
                && renderer.sharedMaterial.shader.name == "DressUpGame/IrisTintSprite")
            {
                return renderer.material;
            }

            return new Material(sharedIrisMaterial);
        }

        private static Material GetIrisUiMaterialInstance(Image image)
        {
            if (sharedIrisUiMaterial == null)
            {
                sharedIrisUiMaterial = Resources.Load<Material>(IrisUiMaterialResourcePath);
                if (sharedIrisUiMaterial == null)
                {
                    Shader shader = Shader.Find("DressUpGame/IrisTintUI");
                    sharedIrisUiMaterial = shader != null ? new Material(shader) : null;
                }
            }

            if (sharedIrisUiMaterial == null)
            {
                return null;
            }

            if (image.material != null
                && image.material.shader != null
                && image.material.shader.name == "DressUpGame/IrisTintUI")
            {
                return image.material;
            }

            return new Material(sharedIrisUiMaterial);
        }
    }
}
