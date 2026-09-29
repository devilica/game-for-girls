using DressUpGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.Pet
{
    public class PetDisplayScaler : MonoBehaviour
    {
        [SerializeField] private float topUiFraction = 0.11f;
        [SerializeField] private float bottomUiFraction = 0.36f;
        [SerializeField] private float doneTopUiFraction = 0.08f;
        [SerializeField] private float doneBottomUiFraction = 0.30f;
        [SerializeField] private float paddingFraction = 0.02f;
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

            SpriteRenderer boundsSource = bodyRenderer != null ? bodyRenderer : GetComponentInChildren<SpriteRenderer>();
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

        public float GetBodyBottomCanvasY(CanvasScaler uiScaler = null)
        {
            Camera camera = Camera.main;
            if (camera != null && camera.orthographic && TryGetPetBottomWorldY(out float bottomWorldY))
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

            float refHeight = uiScaler != null
                ? uiScaler.referenceResolution.y
                : SafeAreaInsets.ReferenceHeight;

            return (bottomUiFraction + paddingFraction) * refHeight;
        }

        private bool TryGetPetBottomWorldY(out float bottomWorldY)
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

            return found;
        }

#if UNITY_EDITOR
        public void SetBodyRenderer(SpriteRenderer renderer)
        {
            bodyRenderer = renderer;
        }
#endif
    }
}
