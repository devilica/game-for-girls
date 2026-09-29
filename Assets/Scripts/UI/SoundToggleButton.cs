using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Top-right sound toggle. Shows sound_on when music is playing and sound_off when muted.
    /// Turning sound on or off requires watching a rewarded ad on device builds.
    /// Lives on a persistent canvas overlay so it stays visible during wizard and done views.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class SoundToggleButton : MonoBehaviour
    {
        public const string OverlayName = "SoundToggleOverlay";
        public const string ButtonName = "SoundToggleButton";
        public const float TargetHeight = 80f;

        private const string SoundOnResourcePath = "UI/sound_on";
        private const string SoundOffResourcePath = "UI/sound_off";

        private static Sprite soundOnSprite;
        private static Sprite soundOffSprite;

        private Button button;
        private Image icon;
        private BackgroundMusicController musicController;
        private bool isTogglePending;

        public static SoundToggleButton EnsureOnCanvas(Transform canvas)
        {
            if (canvas == null)
            {
                return null;
            }

            Transform overlay = EnsureOverlay(canvas);
            MigrateFromTopBar(canvas, overlay);

            Transform existing = overlay.Find(ButtonName);
            if (existing != null)
            {
                SoundToggleButton toggle = existing.GetComponent<SoundToggleButton>();
                if (toggle == null)
                {
                    toggle = existing.gameObject.AddComponent<SoundToggleButton>();
                }

                toggle.Initialize();
                return toggle;
            }

            GameObject buttonGo = new GameObject(ButtonName);
            buttonGo.transform.SetParent(overlay, false);

            RectTransform rect = buttonGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-16f, -16f);

            Image image = buttonGo.AddComponent<Image>();
            image.sprite = null;
            image.color = Color.clear;
            image.preserveAspect = true;
            image.raycastTarget = true;

            buttonGo.AddComponent<Button>();
            SoundToggleButton created = buttonGo.AddComponent<SoundToggleButton>();
            created.Initialize();
            created.ApplyLayout();
            return created;
        }

        private static Transform EnsureOverlay(Transform canvas)
        {
            Transform overlay = canvas.Find(OverlayName);
            if (overlay != null)
            {
                return overlay;
            }

            GameObject overlayGo = new GameObject(OverlayName);
            overlayGo.transform.SetParent(canvas, false);

            RectTransform overlayRect = overlayGo.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            return overlayGo.transform;
        }

        private static void MigrateFromTopBar(Transform canvas, Transform overlay)
        {
            Transform legacyButton = canvas.Find("TopBar/" + ButtonName);
            if (legacyButton != null)
            {
                legacyButton.SetParent(overlay, false);
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            RefreshIcon();
        }

        public void Initialize()
        {
            button = GetComponent<Button>();
            icon = GetComponent<Image>();
            musicController = Application.isPlaying
                ? BackgroundMusicController.EnsureExists()
                : null;

            if (icon != null)
            {
                icon.type = Image.Type.Simple;
            }

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
                button.onClick.AddListener(HandleClicked);

                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
                colors.pressedColor = new Color(1f, 1f, 1f, 0.7f);
                colors.selectedColor = colors.highlightedColor;
                colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
                colors.fadeDuration = 0.08f;
                button.colors = colors;
                button.targetGraphic = icon;
            }

            ApplyLayout();
            RefreshIcon();
        }

        public void ApplyTopRightLayout(float safeTop)
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-16f, -16f - safeTop);
            }

            ApplyLayout();
            RefreshIcon();
        }

        public void ApplyLayout()
        {
            Sprite sprite = GetSoundOnSprite() ?? GetSoundOffSprite();
            if (sprite == null || icon == null)
            {
                Debug.LogWarning("Sound toggle sprites were not found in Resources/UI.");
                return;
            }

            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            float aspect = sprite.rect.width / sprite.rect.height;
            rect.sizeDelta = new Vector2(TargetHeight * aspect, TargetHeight);
        }

        public void RefreshIcon()
        {
            if (icon == null)
            {
                return;
            }

            if (musicController == null && Application.isPlaying)
            {
                musicController = BackgroundMusicController.EnsureExists();
            }

            Sprite offSprite = GetSoundOffSprite();
            Sprite onSprite = GetSoundOnSprite();
            if (offSprite == null && onSprite == null)
            {
                return;
            }

            bool musicIsOn = musicController != null
                ? !musicController.IsMuted
                : !BackgroundMusicController.IsMutedInPreferences();
            icon.sprite = musicIsOn ? onSprite : offSprite;
            icon.color = Color.white;
        }

        private void HandleClicked()
        {
            if (!Application.isPlaying || isTogglePending)
            {
                return;
            }

            StartCoroutine(ToggleWithRewardedAdRoutine());
        }

        private IEnumerator ToggleWithRewardedAdRoutine()
        {
            isTogglePending = true;
            SetInteractable(false);

            bool rewarded = false;
            yield return RewardedAdController.WaitForRewardRoutine(result => rewarded = result);

            if (rewarded)
            {
                if (musicController == null)
                {
                    musicController = BackgroundMusicController.EnsureExists();
                }

                musicController?.ToggleMuted();
                RefreshIcon();
            }

            GameplayPauseGuard.EnsureUnpaused();
            SetInteractable(true);
            isTogglePending = false;
        }

        private void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private static Sprite GetSoundOnSprite()
        {
            if (soundOnSprite == null)
            {
                soundOnSprite = Resources.Load<Sprite>(SoundOnResourcePath);
            }

            return soundOnSprite;
        }

        private static Sprite GetSoundOffSprite()
        {
            if (soundOffSprite == null)
            {
                soundOffSprite = Resources.Load<Sprite>(SoundOffResourcePath);
            }

            return soundOffSprite;
        }
    }
}
