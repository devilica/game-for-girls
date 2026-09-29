#if UNITY_EDITOR
using DressUpGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Creates SplashScene with splash.png only, then sets build order.
    /// </summary>
    public static class SetupSplashSceneTool
    {
        private const string SplashScenePath = "Assets/Scenes/SplashScene.unity";
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";
        private const string SplashTexturePath = "Assets/Resources/UI/splash.png";

        [MenuItem("Dress Up Game/Setup Splash Scene")]
        public static void SetupSplashScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Stop Play before setting up the splash scene.",
                    "OK");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateCanvas();

            GameObject controllerObject = new GameObject("SplashScreenController");
            controllerObject.AddComponent<SplashScreenController>();

            EnsureEventSystem();

            EditorSceneManager.SaveScene(scene, SplashScenePath);
            ConfigureBuildSettings();
            ConfigureSplashScreenTool.ConfigureSplashScreen();
            AssetDatabase.SaveAssets();

            Debug.Log("Dress Up Game: SplashScene created with splash.png. Splash screen and app icon configured.");
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.98f, 0.82f, 0.90f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<SplashFullScreenBackground>();
            return camera;
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateBackground(Transform canvas)
        {
            Sprite splashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SplashTexturePath);
            GameObject backgroundObject = new GameObject("SplashBackground");
            backgroundObject.transform.SetParent(canvas, false);

            RectTransform rect = backgroundObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = backgroundObject.AddComponent<Image>();
            image.sprite = splashSprite;
            image.preserveAspect = false;
            image.color = Color.white;
            image.raycastTarget = false;
            backgroundObject.AddComponent<SplashScreenLayout>();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static void ConfigureBuildSettings()
        {
            SetupMainMenuSceneTool.ConfigureBuildSettings();
        }
    }
}
#endif
