using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DressUpGame.UI
{
    /// <summary>
    /// Shows splash.png, then loads the main menu scene.
    /// </summary>
    public class SplashScreenController : MonoBehaviour
    {
        private const string NextSceneName = MainMenuScreenController.MenuSceneName;
        private const float MinimumDisplaySeconds = 2.2f;

        private void Awake()
        {
            EnsureCameraSplashFill();

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform legacyLoadingBar = canvas.transform.Find("SplashLoadingBar");
                if (legacyLoadingBar != null)
                {
                    Destroy(legacyLoadingBar.gameObject);
                }

                Transform splashBackground = canvas.transform.Find("SplashBackground");
                if (splashBackground != null)
                {
                    splashBackground.gameObject.SetActive(false);
                }

                canvas.enabled = false;
            }

            HideSoundToggleIfPresent();
        }

        private static void EnsureCameraSplashFill()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = FindAnyObjectByType<Camera>();
            }

            if (camera == null)
            {
                return;
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.98f, 0.82f, 0.90f);
            camera.orthographic = true;

            if (camera.GetComponent<SplashFullScreenBackground>() == null)
            {
                camera.gameObject.AddComponent<SplashFullScreenBackground>();
            }
        }

        private void Start()
        {
            HideSoundToggleIfPresent();
            StartCoroutine(LoadMainSceneRoutine());
        }

        private static void HideSoundToggleIfPresent()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            Transform soundOverlay = canvas != null
                ? canvas.transform.Find(SoundToggleButton.OverlayName)
                : null;
            if (soundOverlay != null)
            {
                soundOverlay.gameObject.SetActive(false);
            }
        }

        private IEnumerator LoadMainSceneRoutine()
        {
            float startedAt = Time.unscaledTime;

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(NextSceneName);
            if (loadOperation == null)
            {
                Debug.LogError($"SplashScreenController could not start loading scene '{NextSceneName}'.");
                yield break;
            }

            loadOperation.allowSceneActivation = false;

            while (loadOperation.progress < 0.9f)
            {
                yield return null;
            }

            float elapsed = Time.unscaledTime - startedAt;
            if (elapsed < MinimumDisplaySeconds)
            {
                yield return new WaitForSecondsRealtime(MinimumDisplaySeconds - elapsed);
            }

            loadOperation.allowSceneActivation = true;
        }
    }
}
