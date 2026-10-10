using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.MysteryBox
{
    /// <summary>
    /// Runtime-loaded mystery box UI sprites from Resources (build-safe fallback).
    /// </summary>
    public static class MysteryBoxArt
    {
        public const string BoxResourcePath = "MysteryBox/box";
        public const string HeaderResourcePath = "MysteryBox/mistery";
        public const string RevealBackgroundResourcePath = "MysteryBox/mistery-bg";
        public const string ApplyButtonResourcePath = "MysteryBox/apply";
        public const string RefreshButtonResourcePath = "MysteryBox/refresh";
        public const float RevealCardWidthReference = 880f;
        public const float ApplyButtonWidthReference = 520f;
        public const float RefreshButtonWidthReference = 152f;
        public const float RevealActionRowSpacingReference = 56f;
        public const float RevealApplyLayoutExtraBottomReference = 36f;
        public const float RevealApplyCenterYReference = -248f;
        public const float RevealPreviewSizeReference = 400f;
        public const float RevealPreviewDualSizeReference = 280f;
        public const float RevealPreviewCenterYReference = 110f;

        private static Sprite cachedBoxSprite;
        private static Sprite cachedHeaderSprite;
        private static Sprite cachedRevealBackgroundSprite;
        private static Sprite cachedApplyButtonSprite;
        private static Sprite cachedRefreshButtonSprite;

        public static Sprite LoadBoxSprite()
        {
            if (cachedBoxSprite != null)
            {
                return cachedBoxSprite;
            }

            cachedBoxSprite = LoadSprite(BoxResourcePath);
            return cachedBoxSprite;
        }

        public static Sprite LoadHeaderSprite()
        {
            if (cachedHeaderSprite != null)
            {
                return cachedHeaderSprite;
            }

            cachedHeaderSprite = LoadSprite(HeaderResourcePath);
            return cachedHeaderSprite;
        }

        public static Sprite LoadRevealBackgroundSprite()
        {
            if (cachedRevealBackgroundSprite != null)
            {
                return cachedRevealBackgroundSprite;
            }

            cachedRevealBackgroundSprite = LoadSprite(RevealBackgroundResourcePath);
            return cachedRevealBackgroundSprite;
        }

        public static Sprite LoadApplyButtonSprite()
        {
            if (cachedApplyButtonSprite != null)
            {
                return cachedApplyButtonSprite;
            }

            cachedApplyButtonSprite = LoadSprite(ApplyButtonResourcePath);
            return cachedApplyButtonSprite;
        }

        public static Sprite LoadRefreshButtonSprite()
        {
            if (cachedRefreshButtonSprite != null)
            {
                return cachedRefreshButtonSprite;
            }

            cachedRefreshButtonSprite = LoadSprite(RefreshButtonResourcePath);
            return cachedRefreshButtonSprite;
        }

        public static void ApplySpriteButton(Image target, Sprite sprite, float widthReference)
        {
            if (target == null || sprite == null)
            {
                return;
            }

            target.sprite = sprite;
            target.color = Color.white;
            target.type = Image.Type.Simple;
            target.preserveAspect = true;

            if (target.transform is not RectTransform rect)
            {
                return;
            }

            float aspect = sprite.rect.height / sprite.rect.width;
            float height = widthReference * aspect;
            rect.sizeDelta = new Vector2(widthReference, height);

            LayoutElement layout = target.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = target.gameObject.AddComponent<LayoutElement>();
            }

            layout.preferredWidth = widthReference;
            layout.preferredHeight = height;
            layout.minWidth = widthReference;
            layout.minHeight = height;
        }

        public static void ApplyRevealCardFrame(Image cardBackground, RectTransform cardRect, bool applyCardSize = true)
        {
            if (cardBackground == null)
            {
                return;
            }

            Sprite frameSprite = LoadRevealBackgroundSprite();
            if (frameSprite == null)
            {
                return;
            }

            cardBackground.sprite = frameSprite;
            cardBackground.color = Color.white;
            cardBackground.type = Image.Type.Simple;
            cardBackground.preserveAspect = true;
            cardBackground.raycastTarget = false;

            if (applyCardSize && cardRect != null)
            {
                float width = RevealCardWidthReference;
                float aspect = frameSprite.rect.height / frameSprite.rect.width;
                cardRect.sizeDelta = new Vector2(width, width * aspect);
            }
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
                Debug.LogWarning($"Mystery Box: missing UI art at Resources/{resourcePath}.");
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
