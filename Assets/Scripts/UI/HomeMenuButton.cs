using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Top-left home control during gameplay; returns to the main menu hub.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class HomeMenuButton : MonoBehaviour
    {
        public const string ButtonName = "HomeMenuButton";
        public const float TargetHeight = 96f;

        private const string HomeIconResourcePath = "UI/menu/home";
        private const string MainSceneName = "MainScene";
        private const string PetSceneName = "PetScene";

        private static Sprite homeSprite;

        private Button button;
        private Image icon;

        public static HomeMenuButton EnsureOnCanvas(Transform canvas)
        {
            if (canvas == null)
            {
                return null;
            }

            Transform overlay = canvas.Find(SoundToggleButton.OverlayName);
            if (overlay == null)
            {
                SoundToggleButton.EnsureOnCanvas(canvas);
                overlay = canvas.Find(SoundToggleButton.OverlayName);
            }

            if (overlay == null)
            {
                return null;
            }

            Transform existing = overlay.Find(ButtonName);
            if (existing != null)
            {
                HomeMenuButton home = existing.GetComponent<HomeMenuButton>();
                if (home == null)
                {
                    home = existing.gameObject.AddComponent<HomeMenuButton>();
                }

                home.Initialize();
                home.RefreshVisibility();
                return home;
            }

            GameObject buttonGo = new GameObject(ButtonName);
            buttonGo.transform.SetParent(overlay, false);

            RectTransform rect = buttonGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(16f, -16f);

            Image image = buttonGo.AddComponent<Image>();
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = true;

            buttonGo.AddComponent<Button>();
            HomeMenuButton created = buttonGo.AddComponent<HomeMenuButton>();
            created.Initialize();
            created.RefreshVisibility();
            return created;
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            RefreshIcon();
            RefreshVisibility();
        }

        public void Initialize()
        {
            button = GetComponent<Button>();
            icon = GetComponent<Image>();

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

        public void ApplyTopLeftLayout(float safeTop)
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(16f, -16f - safeTop);
            }

            ApplyLayout();
        }

        public void ApplyLayout()
        {
            Sprite sprite = GetHomeSprite();
            if (sprite == null || icon == null)
            {
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

        public void RefreshVisibility()
        {
            gameObject.SetActive(IsGameplayScene(SceneManager.GetActiveScene().name));
        }

        private static bool IsGameplayScene(string sceneName)
        {
            return sceneName == MainSceneName || sceneName == PetSceneName;
        }

        private void RefreshIcon()
        {
            if (icon == null)
            {
                return;
            }

            Sprite sprite = GetHomeSprite();
            if (sprite == null)
            {
                Debug.LogWarning("Home menu icon was not found at Resources/UI/menu/home.");
                return;
            }

            icon.sprite = sprite;
            icon.color = Color.white;
        }

        private void HandleClicked()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!IsGameplayScene(SceneManager.GetActiveScene().name))
            {
                return;
            }

            GameplayPauseGuard.EnsureUnpaused();
            SceneManager.LoadScene(MainMenuScreenController.MenuSceneName);
        }

        private static Sprite GetHomeSprite()
        {
            if (homeSprite != null)
            {
                return homeSprite;
            }

            homeSprite = Resources.Load<Sprite>(HomeIconResourcePath);
            if (homeSprite != null)
            {
                return homeSprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(HomeIconResourcePath);
            if (texture == null)
            {
                return null;
            }

            homeSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            return homeSprite;
        }
    }
}
