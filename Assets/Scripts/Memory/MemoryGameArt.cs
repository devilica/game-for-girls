using UnityEngine;

namespace DressUpGame.Memory
{
    public static class MemoryGameArt
    {
        public const string HeaderResourcePath = "Memory/memory";

        private static Sprite cachedHeaderSprite;

        public static Sprite LoadHeaderSprite()
        {
            if (cachedHeaderSprite != null)
            {
                return cachedHeaderSprite;
            }

            cachedHeaderSprite = LoadSprite(HeaderResourcePath);
            return cachedHeaderSprite;
        }

        private static Sprite LoadSprite(string resourcePath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                return sprite;
            }

            Object[] assets = Resources.LoadAll(resourcePath);
            foreach (Object asset in assets)
            {
                if (asset is Sprite assetSprite)
                {
                    return assetSprite;
                }
            }

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                Debug.LogWarning($"Memory game: missing header art at Resources/{resourcePath}.");
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }
    }
}
