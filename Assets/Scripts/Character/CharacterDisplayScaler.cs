using DressUpGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.Character
{
    /// <summary>
    /// Scales and positions the Girl in the clear area between the top bar and bottom UI.
    /// Uses the Body sprite bounds only so large hair/dress PNG padding does not break layout.
    /// </summary>
    public class CharacterDisplayScaler : MonoBehaviour
    {
        [Header("Screen zones (fraction of full screen height)")]
        [SerializeField] private float topUiFraction = 0.11f;
        [SerializeField] private float bottomUiFraction = 0.36f;
        [SerializeField] private float doneTopUiFraction = 0.08f;
        [SerializeField] private float doneBottomUiFraction = 0.30f;
        [SerializeField] private float paddingFraction = 0.02f;

        [Header("Bounds")]
        [SerializeField] private SpriteRenderer bodyRenderer;

        public void Apply()
        {
            ApplyWithUiFractions(topUiFraction, bottomUiFraction);
        }

        public void ApplyForDoneView()
        {
            ApplyWithUiFractions(doneTopUiFraction, doneBottomUiFraction);
        }

        private void ApplyWithUiFractions(float topFraction, float bottomFraction)
        {
            Camera camera = Camera.main;
            if (camera == null || !camera.orthographic)
            {
                return;
            }

            SpriteRenderer boundsSource = ResolveBoundsRenderer();
            if (boundsSource == null || boundsSource.sprite == null)
            {
                return;
            }

            transform.localScale = Vector3.one;

            Bounds bounds = boundsSource.bounds;
            if (bounds.size.y <= 0.001f)
            {
                return;
            }

            float orthoSize = camera.orthographicSize;
            float viewHeight = orthoSize * 2f;
            float padding = viewHeight * paddingFraction;

            float worldBottom = camera.transform.position.y - orthoSize + (viewHeight * bottomFraction) + padding;
            float worldTop = camera.transform.position.y + orthoSize - (viewHeight * topFraction) - padding;
            float availableHeight = worldTop - worldBottom;

            if (availableHeight <= 0.001f)
            {
                return;
            }

            float scale = availableHeight / bounds.size.y;
            transform.localScale = Vector3.one * scale;

            bounds = boundsSource.bounds;
            float targetCenterY = (worldBottom + worldTop) * 0.5f;
            float deltaY = targetCenterY - bounds.center.y;
            Vector3 position = transform.position;
            position.y += deltaY;
            transform.position = new Vector3(camera.transform.position.x, position.y, 0f);
        }

        /// <summary>
        /// Returns the Body sprite bottom in canvas reference units (origin at screen bottom).
        /// Call after <see cref="Apply"/> for the live character position.
        /// </summary>
        public float GetBodyBottomCanvasY(CanvasScaler uiScaler = null)
        {
            return GetCharacterBottomCanvasY(uiScaler);
        }

        /// <summary>
        /// Lowest visible sprite edge (dress hem, shoes, etc.) in canvas reference units.
        /// </summary>
        public float GetCharacterBottomCanvasY(CanvasScaler uiScaler = null)
        {
            Camera camera = Camera.main;
            if (camera != null && camera.orthographic && TryGetCharacterBottomWorldY(out float bottomWorldY))
            {
                Vector3 screenPoint = camera.WorldToScreenPoint(
                    new Vector3(transform.position.x, bottomWorldY, 0f));

                float referenceHeight = uiScaler != null
                    ? uiScaler.referenceResolution.y
                    : SafeAreaInsets.ReferenceHeight;

                if (Screen.height > 0f)
                {
                    return screenPoint.y * (referenceHeight / Screen.height);
                }
            }

            return GetPredictedBodyBottomCanvasY(uiScaler);
        }

        private bool TryGetCharacterBottomWorldY(out float bottomWorldY)
        {
            bottomWorldY = float.MaxValue;
            bool found = false;

            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(false);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || renderer.sprite == null)
                {
                    continue;
                }

                float candidate = renderer.bounds.min.y;
                if (candidate < bottomWorldY)
                {
                    bottomWorldY = candidate;
                    found = true;
                }
            }

            if (!found)
            {
                SpriteRenderer boundsSource = ResolveBoundsRenderer();
                if (boundsSource != null && boundsSource.sprite != null)
                {
                    bottomWorldY = boundsSource.bounds.min.y;
                    found = true;
                }
            }

            return found;
        }

        public float GetPredictedBodyBottomCanvasY(CanvasScaler uiScaler = null)
        {
            float referenceHeight = uiScaler != null
                ? uiScaler.referenceResolution.y
                : SafeAreaInsets.ReferenceHeight;

            return (bottomUiFraction + paddingFraction) * referenceHeight;
        }

        private SpriteRenderer ResolveBoundsRenderer()
        {
            if (bodyRenderer != null)
            {
                return bodyRenderer;
            }

            Transform bodyTransform = transform.Find("Body");
            if (bodyTransform != null)
            {
                bodyRenderer = bodyTransform.GetComponent<SpriteRenderer>();
            }

            return bodyRenderer;
        }
    }
}
