#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Generates placeholder assets and MainScene automatically when they are missing.
    /// Retries after script compilation finishes.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoProjectSetup
    {
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string ScenePath = "Assets/Scenes/MainScene.unity";

        static AutoProjectSetup()
        {
            EditorApplication.delayCall += TrySetup;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        private static void OnCompilationFinished(object _)
        {
            EditorApplication.delayCall += TrySetup;
        }

        private static void TrySetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorApplication.isCompiling)
            {
                return;
            }

            if (!NeedsSetup())
            {
                return;
            }

            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
            }

            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }

            try
            {
                if (!File.Exists(CatalogPath))
                {
                    Debug.Log("Dress Up Game: Generating placeholder sprites and item data...");
                    GameDataGenerator.GenerateAll();
                }

                if (!File.Exists(ScenePath))
                {
                    Debug.Log("Dress Up Game: Building MainScene...");
                    GameSceneBuilder.SetupMainScene();
                }

                SpriteAssetLinker.ApplySpritesFromFolder();

                Debug.Log("Dress Up Game: Setup complete. Open Assets/Scenes/MainScene.unity and press Play.");
            }
            catch (System.Exception exception)
            {
                Debug.LogError("Dress Up Game: Auto setup failed. Use menu Dress Up Game → Full Project Setup.");
                Debug.LogException(exception);
            }
        }

        private static bool NeedsSetup()
        {
            return !File.Exists(CatalogPath) || !File.Exists(ScenePath);
        }
    }
}
#endif
