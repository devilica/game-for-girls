using System.Collections;
using DressUpGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.Memory
{
    public sealed class MemoryCardView : MonoBehaviour
    {
        [SerializeField] private Image borderImage;
        [SerializeField] private Image backImage;
        [SerializeField] private Image frontBackgroundImage;
        [SerializeField] private Image frontImage;
        [SerializeField] private Button button;

        private MemoryGameController board;
        private int pairId;
        private bool isFaceUp;
        private bool isMatched;
        private Coroutine winBorderCelebrationRoutine;
        private Color celebrationBaseBorderColor;
        private Color celebrationAccentBorderColor;
        private Color celebrationButtonNormalColor;

        public int PairId => pairId;
        public bool IsFaceUp => isFaceUp;
        public bool IsMatched => isMatched;

        public void ConfigureVisuals(
            Image border,
            Image back,
            Image frontBackground,
            Image front,
            Button clickButton,
            Color baseBorderColor,
            Color accentBorderColor)
        {
            borderImage = border;
            backImage = back;
            frontBackgroundImage = frontBackground;
            frontImage = front;
            button = clickButton;
            celebrationBaseBorderColor = baseBorderColor;
            celebrationAccentBorderColor = accentBorderColor;
            if (button != null)
            {
                celebrationButtonNormalColor = button.colors.normalColor;
            }
        }

        public void Bind(MemoryGameController owner, int id, Sprite bowSprite)
        {
            board = owner;
            pairId = id;
            if (frontImage != null)
            {
                frontImage.sprite = bowSprite;
                frontImage.enabled = bowSprite != null;
            }

            SetFaceUp(false, instant: true);
            SetMatched(false);
            StopWinBorderCelebration();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClicked);
            }
        }

        public void SetFaceUp(bool faceUp, bool instant = false)
        {
            isFaceUp = faceUp;
            if (backImage != null)
            {
                backImage.enabled = !faceUp;
            }

            if (frontBackgroundImage != null)
            {
                frontBackgroundImage.enabled = faceUp;
            }

            if (frontImage != null)
            {
                frontImage.enabled = faceUp;
            }

            if (button != null)
            {
                button.interactable = !isMatched && !faceUp;
            }
        }

        public void SetMatched(bool matched)
        {
            isMatched = matched;
            if (button != null)
            {
                button.interactable = !matched;
            }
        }

        public void StartWinBorderCelebration(float phaseOffsetSeconds)
        {
            StopWinBorderCelebration();
            if (borderImage == null)
            {
                return;
            }

            winBorderCelebrationRoutine = StartCoroutine(WinBorderCelebrationRoutine(phaseOffsetSeconds));
        }

        public void StopWinBorderCelebration()
        {
            if (winBorderCelebrationRoutine != null)
            {
                StopCoroutine(winBorderCelebrationRoutine);
                winBorderCelebrationRoutine = null;
            }

            if (borderImage != null)
            {
                borderImage.color = celebrationBaseBorderColor;
            }

            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = celebrationButtonNormalColor;
                button.colors = colors;
            }
        }

        private void OnDisable()
        {
            StopWinBorderCelebration();
        }

        private IEnumerator WinBorderCelebrationRoutine(float phaseOffsetSeconds)
        {
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                button.colors = colors;
            }

            while (true)
            {
                float t = Mathf.PingPong(
                    (Time.unscaledTime + phaseOffsetSeconds) / UiBorderCelebrationStyle.PulsePeriodSeconds,
                    1f);
                if (borderImage != null)
                {
                    borderImage.color = Color.Lerp(celebrationBaseBorderColor, celebrationAccentBorderColor, t);
                }

                yield return null;
            }
        }

        private void HandleClicked()
        {
            if (board == null || isMatched || isFaceUp)
            {
                return;
            }

            board.HandleCardClicked(this);
        }
    }
}
