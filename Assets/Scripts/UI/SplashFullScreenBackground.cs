using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Draws splash.png as a camera-fitted sprite (cover), avoiding Canvas scaler shrink issues.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SplashFullScreenBackground : MonoBehaviour
    {
        private const string SplashResourcePath = "UI/splash";
        private const float SpriteDepth = 5f;

        private Camera targetCamera;
        private SpriteRenderer spriteRenderer;
        private Transform spriteTransform;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            EnsureSpriteFill();
        }

        private void OnEnable()
        {
            EnsureSpriteFill();
            ApplyFill();
        }

        private void LateUpdate()
        {
            ApplyFill();
        }

        private void EnsureSpriteFill()
        {
            if (spriteRenderer != null)
            {
                return;
            }

            Sprite splashSprite = Resources.Load<Sprite>(SplashResourcePath);
            if (splashSprite == null)
            {
                Debug.LogWarning("SplashFullScreenBackground could not load UI/splash.");
                return;
            }

            GameObject spriteObject = new GameObject("SplashSpriteFill");
            spriteObject.transform.SetParent(transform, false);
            spriteObject.transform.localPosition = new Vector3(0f, 0f, SpriteDepth);

            spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = splashSprite;
            spriteRenderer.sortingOrder = -32760;
            spriteTransform = spriteObject.transform;
        }

        private void ApplyFill()
        {
            if (spriteRenderer == null || spriteTransform == null || targetCamera == null || !targetCamera.orthographic)
            {
                return;
            }

            float worldHeight = targetCamera.orthographicSize * 2f;
            float worldWidth = worldHeight * targetCamera.aspect;
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            {
                return;
            }

            float scaleX = worldWidth / spriteSize.x;
            float scaleY = worldHeight / spriteSize.y;
            float uniformScale = Mathf.Max(scaleX, scaleY);
            spriteTransform.localScale = new Vector3(uniformScale, uniformScale, 1f);
            spriteTransform.localPosition = new Vector3(0f, 0f, SpriteDepth);
        }
    }
}
