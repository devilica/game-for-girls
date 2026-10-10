using DressUpGame.Memory;
using DressUpGame.MysteryBox;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DressUpGame.UI
{
    /// <summary>
    /// Handles the Android system back key (Escape) for in-app navigation and quitting from the menu.
    /// </summary>
    public sealed class AndroidBackNavigationHandler : MonoBehaviour
    {
        private const string MainSceneName = "MainScene";
        private const string PetSceneName = "PetScene";
        private const string MemorySceneName = MemoryGameController.MemorySceneName;
        private const string MysteryBoxSceneName = MysteryBoxController.MysteryBoxSceneName;
        private const string SplashSceneName = "SplashScene";
        private const float ExitConfirmWindowSeconds = 2f;

        private static AndroidBackNavigationHandler instance;
        private bool isHandlingBack;
        private float lastHubBackPressUnscaledTime = float.NegativeInfinity;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            GameObject host = new GameObject(nameof(AndroidBackNavigationHandler));
            instance = host.AddComponent<AndroidBackNavigationHandler>();
            DontDestroyOnLoad(host);
        }

        private void Update()
        {
            if (!ShouldListenForBack() || !WasBackPressedThisFrame() || isHandlingBack)
            {
                return;
            }

            if (AdGatedSelectionHelper.IsGateInProgress)
            {
                return;
            }

            BreakScreenController breakScreen = FindAnyObjectByType<BreakScreenController>();
            if (breakScreen != null && breakScreen.IsShowing)
            {
                return;
            }

            isHandlingBack = true;
            try
            {
                GameplayPauseGuard.EnsureUnpaused();
                instance.HandleBackNavigation();
            }
            finally
            {
                isHandlingBack = false;
            }
        }

        private static bool ShouldListenForBack()
        {
            return Application.platform == RuntimePlatform.Android || Application.isEditor;
        }

        private static bool WasBackPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
            return Input.GetKeyDown(KeyCode.Escape);
        }

        private void HandleBackNavigation()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == SplashSceneName || sceneName == MainMenuScreenController.MenuSceneName)
            {
                TryQuitHubWithDoubleBack();
                return;
            }

            lastHubBackPressUnscaledTime = float.NegativeInfinity;

            if (sceneName == MainSceneName)
            {
                GameUIController ui = FindAnyObjectByType<GameUIController>();
                if (ui != null)
                {
                    ui.TryHandleBackNavigation();
                    return;
                }
            }

            if (sceneName == PetSceneName)
            {
                PetGameUIController ui = FindAnyObjectByType<PetGameUIController>();
                if (ui != null)
                {
                    ui.TryHandleBackNavigation();
                    return;
                }
            }

            if (sceneName == MemorySceneName)
            {
                MemoryGameController memory = FindAnyObjectByType<MemoryGameController>();
                if (memory != null)
                {
                    memory.TryHandleBackNavigation();
                    return;
                }
            }

            if (sceneName == MysteryBoxSceneName)
            {
                MysteryBoxController mystery = FindAnyObjectByType<MysteryBoxController>();
                if (mystery != null)
                {
                    mystery.TryHandleBackNavigation();
                    return;
                }
            }

            QuitApplication();
        }

        private void TryQuitHubWithDoubleBack()
        {
            float now = Time.unscaledTime;
            if (now - lastHubBackPressUnscaledTime <= ExitConfirmWindowSeconds)
            {
                lastHubBackPressUnscaledTime = float.NegativeInfinity;
                QuitApplication();
                return;
            }

            lastHubBackPressUnscaledTime = now;
            Debug.Log("Dress Up Game: Press back again to exit.");
        }

        private static void QuitApplication()
        {
            Application.Quit();

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                activity?.Call("finish");
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Android finish() after Quit failed: {exception.Message}");
            }
#endif
        }
    }
}
