using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Memory-style pink-to-gold border pulse on the main play card in the menu hub.
    /// </summary>
    public class MenuCardPlayHintBorder : MonoBehaviour
    {
        private const float ContentInsetPixels = 6f;
        private const string BorderObjectName = "PlayHintBorder";
        private const string LegacyOuterRingName = "OuterBlinkRing";
        private const string LegacyInnerRingName = "InnerBlinkRing";

        private Image borderImage;
        private Coroutine pulseRoutine;

        public void Build()
        {
            DestroyLegacyRings();

            Transform content = transform.Find("CardContent");
            if (content != null)
            {
                RectTransform contentRect = content as RectTransform;
                if (contentRect != null)
                {
                    contentRect.anchorMin = Vector2.zero;
                    contentRect.anchorMax = Vector2.one;
                    contentRect.offsetMin = new Vector2(ContentInsetPixels, ContentInsetPixels);
                    contentRect.offsetMax = new Vector2(-ContentInsetPixels, -ContentInsetPixels);
                }
            }

            Transform existingBorder = transform.Find(BorderObjectName);
            if (existingBorder != null)
            {
                borderImage = existingBorder.GetComponent<Image>();
                if (borderImage != null)
                {
                    borderImage.color = UiBorderCelebrationStyle.BaseBorderColor;
                    existingBorder.SetAsFirstSibling();
                    return;
                }
            }

            GameObject borderGo = new GameObject(BorderObjectName);
            borderGo.transform.SetParent(transform, false);
            borderGo.transform.SetAsFirstSibling();

            RectTransform borderRect = borderGo.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;

            borderImage = borderGo.AddComponent<Image>();
            borderImage.sprite = UiRoundSpriteUtility.GetRoundedSquareSprite();
            borderImage.type = Image.Type.Sliced;
            borderImage.color = UiBorderCelebrationStyle.BaseBorderColor;
            borderImage.raycastTarget = false;
        }

        private void OnEnable()
        {
            if (borderImage == null)
            {
                Build();
            }

            StartPulse();
        }

        private void OnDisable()
        {
            StopPulse();
        }

        private void StartPulse()
        {
            StopPulse();
            if (borderImage == null)
            {
                return;
            }

            pulseRoutine = StartCoroutine(BorderPulseRoutine());
        }

        private void StopPulse()
        {
            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }

            if (borderImage != null)
            {
                borderImage.color = UiBorderCelebrationStyle.BaseBorderColor;
            }
        }

        private IEnumerator BorderPulseRoutine()
        {
            while (true)
            {
                float t = Mathf.PingPong(Time.unscaledTime / UiBorderCelebrationStyle.PulsePeriodSeconds, 1f);
                if (borderImage != null)
                {
                    borderImage.color = Color.Lerp(
                        UiBorderCelebrationStyle.BaseBorderColor,
                        UiBorderCelebrationStyle.AccentBorderColor,
                        t);
                }

                yield return null;
            }
        }

        private void DestroyLegacyRings()
        {
            if (!transform)
            {
                return;
            }

            Transform outer = transform.Find(LegacyOuterRingName);
            if (outer)
            {
                Destroy(outer.gameObject);
            }

            Transform inner = transform.Find(LegacyInnerRingName);
            if (inner)
            {
                Destroy(inner.gameObject);
            }
        }
    }
}
