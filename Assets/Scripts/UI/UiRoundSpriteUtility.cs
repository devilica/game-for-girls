using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Provides a round UI sprite without relying on Unity built-in skin assets.
    /// </summary>
    public static class UiRoundSpriteUtility
    {
        private const string ResourcePath = "UI/round_button";
        private static Sprite cachedSprite;
        private static Sprite cachedRoundedSquareSprite;

        public static Sprite GetRoundSprite(out bool useSlicedImage)
        {
            useSlicedImage = false;

            if (cachedSprite != null)
            {
                return cachedSprite;
            }

            cachedSprite = Resources.Load<Sprite>(ResourcePath);
            if (cachedSprite != null)
            {
                return cachedSprite;
            }

            cachedSprite = CreateCircleSprite(96);
            return cachedSprite;
        }

        public static Sprite GetRoundedSquareSprite()
        {
            if (cachedRoundedSquareSprite != null)
            {
                return cachedRoundedSquareSprite;
            }

            cachedRoundedSquareSprite = CreateRoundedSquareSprite(128, 22f);
            return cachedRoundedSquareSprite;
        }

        private static Sprite CreateCircleSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            float radius = size * 0.5f - 1f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        private static Sprite CreateRoundedSquareSprite(int size, float cornerRadius)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = RoundedRectAlpha(x + 0.5f, y + 0.5f, size, size, cornerRadius);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        private static float RoundedRectAlpha(float x, float y, float width, float height, float radius)
        {
            float halfWidth = width * 0.5f - radius;
            float halfHeight = height * 0.5f - radius;
            float ax = Mathf.Abs(x - width * 0.5f);
            float ay = Mathf.Abs(y - height * 0.5f);
            float dx = Mathf.Max(ax - halfWidth, 0f);
            float dy = Mathf.Max(ay - halfHeight, 0f);
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01(radius - distance + 0.5f);
        }
    }
}
