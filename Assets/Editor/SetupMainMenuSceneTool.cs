#if UNITY_EDITOR
using DressUpGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Creates MenuScene and inserts it into the build order after SplashScene.
    /// </summary>
    public static class SetupMainMenuSceneTool
    {
        private const string MenuScenePath = "Assets/Scenes/MenuScene.unity";
        private const string SplashScenePath = "Assets/Scenes/SplashScene.unity";
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";
        private const string PetScenePath = "Assets/Scenes/PetScene.unity";

        [MenuItem("Dress Up Game/Setup Main Menu Scene")]
        public static void SetupMainMenuScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Stop Play before setting up the main menu scene.",
                    "OK");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            EnsureEventSystem();

            GameObject menuRoot = new GameObject("MainMenuScreenController");
            menuRoot.AddComponent<MainMenuScreenController>();

            EditorSceneManager.SaveScene(scene, MenuScenePath);
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();

            Debug.Log("Dress Up Game: MenuScene created. Build order is Splash → Menu → Main → Pet.");
        }

        public static void ConfigureBuildSettings()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(SplashScenePath, true),
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(MainScenePath, true)
            };

            if (System.IO.File.Exists(PetScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(PetScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void CreateCamera()
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
    }
}
#endif
