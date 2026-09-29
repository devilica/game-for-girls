using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Soft blinking rings around the main play card on the menu hub.
    /// </summary>
    public class MenuCardPlayHintBorder : MonoBehaviour
    {
        private const float PulsePeriodSeconds = 0.9f;
        private const float ScalePulseAmount = 0.045f;

        private static readonly Color OuterBright = new Color(1f, 0.32f, 0.68f, 0.98f);
        private static readonly Color OuterDim = new Color(1f, 0.62f, 0.82f, 0.12f);
        private static readonly Color InnerBright = new Color(1f, 0.88f, 0.28f, 0.92f);
        private static readonly Color InnerDim = new Color(1f, 0.94f, 0.55f, 0.1f);

        private Image outerRing;
        private Image innerRing;
        private float phase;

        public void Build()
        {
            if (outerRing != null)
            {
                return;
            }

            Sprite ringSprite = UiRoundSpriteUtility.GetRoundedSquareSprite();
            outerRing = CreateRing("OuterBlinkRing", ringSprite, 12f, 12f);
            innerRing = CreateRing("InnerBlinkRing", ringSprite, 5f, 5f);
            outerRing.transform.SetAsFirstSibling();
            innerRing.transform.SetSiblingIndex(1);
        }

        private void Update()
        {
            if (outerRing == null || innerRing == null)
            {
                return;
            }

            phase += Time.unscaledDeltaTime;
            float pulse = (Mathf.Sin(phase * (Mathf.PI * 2f) / PulsePeriodSeconds) + 1f) * 0.5f;
            float blink = pulse * pulse;

            outerRing.color = Color.Lerp(OuterDim, OuterBright, blink);
            innerRing.color = Color.Lerp(InnerDim, InnerBright, blink);

            float scale = 1f + blink * ScalePulseAmount;
            outerRing.rectTransform.localScale = new Vector3(scale, scale, 1f);
            innerRing.rectTransform.localScale = Vector3.one;
        }

        private Image CreateRing(string name, Sprite sprite, float expandX, float expandY)
        {
            GameObject ringGo = new GameObject(name);
            ringGo.transform.SetParent(transform, false);

            RectTransform rect = ringGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(-expandX, -expandY);
            rect.offsetMax = new Vector2(expandX, expandY);

            Image image = ringGo.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }
    }
}
