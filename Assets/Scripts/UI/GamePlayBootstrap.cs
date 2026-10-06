using DressUpGame.Character;
using DressUpGame.Memory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DressUpGame.UI
{
    /// <summary>
    /// Runs once when the game starts to fix camera, UI layout, and character scale.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GamePlayBootstrap : MonoBehaviour
    {
        private const string SplashSceneName = "SplashScene";
        private const string MenuSceneName = MainMenuScreenController.MenuSceneName;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrapExists()
        {
            if (Object.FindAnyObjectByType<GamePlayBootstrap>() != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject("GamePlayBootstrap");
            bootstrapObject.AddComponent<GamePlayBootstrap>();
        }

        [SerializeField] private Camera mainCamera;
        [SerializeField] private CharacterCustomizer customizer;

        private void Awake()
        {
            EnsureAudioListenerExists();
            BackgroundMusicController.EnsureExists();

            if (IsNonGameplayScene())
            {
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (customizer == null)
            {
                customizer = FindAnyObjectByType<CharacterCustomizer>();
            }

            ConfigureCamera();
            ConfigureBackground();
            ConfigureAudio();
            ConfigureAds();
            ConfigureUILayout();
        }

        private static bool IsNonGameplayScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            return sceneName == SplashSceneName || sceneName == MenuSceneName;
        }

        public static void EnsureAudioListenerForActiveScene()
        {
            AudioListener existingListener = Object.FindAnyObjectByType<AudioListener>();
            if (existingListener != null && existingListener.enabled)
            {
                return;
            }

            Camera camera = Camera.main ?? Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                GameObject cameraGo = new GameObject("Main Camera");
                camera = cameraGo.AddComponent<Camera>();
                cameraGo.tag = "MainCamera";
                camera.orthographic = true;
                camera.orthographicSize = 6f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.backgroundColor = new Color(0.98f, 0.92f, 0.96f);
                camera.clearFlags = CameraClearFlags.SolidColor;
            }

            if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }
        }

        private static void EnsureAudioListenerExists()
        {
            EnsureAudioListenerForActiveScene();
        }

        private void ConfigureCamera()
        {
            if (mainCamera == null)
            {
                return;
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 6f;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            mainCamera.backgroundColor = new Color(0.98f, 0.92f, 0.96f);

            EnsureAudioListenerExists();
        }

        private void ConfigureBackground()
        {
            if (mainCamera == null)
            {
                return;
            }

            GameBackgroundDisplay.EnsureExists(mainCamera);
        }

        private void ConfigureUILayout()
        {
            if (SceneManager.GetActiveScene().name == MemoryGameController.MemorySceneName)
            {
                return;
            }

            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            if (layout == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    layout = canvas.gameObject.AddComponent<PortraitUILayout>();
                }
            }

            layout?.Apply();
        }

        private void ConfigureAudio()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : null;
            SoundToggleButton.EnsureOnCanvas(canvasTransform);
            HomeMenuButton.EnsureOnCanvas(canvasTransform);
        }

        private void ConfigureAds()
        {
            MobileAdsInitializer.EnsureInitialized();
            BottomBannerAdController.EnsureExists();
            BreakNativeAdController.EnsureExists();
            BreakInterstitialAdController.EnsureExists();
            RewardedAdController.EnsureExists();
        }
    }
}
