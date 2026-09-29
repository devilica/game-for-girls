#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Shows a setup window when MainScene or game data is missing.
    /// </summary>
    public class DressUpSetupWindow : EditorWindow
    {
        private const string ScenePath = "Assets/Scenes/MainScene.unity";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";

        [InitializeOnLoadMethod]
        private static void ShowIfNeeded()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }

                if (!NeedsSetup())
                {
                    return;
                }

                DressUpSetupWindow window = GetWindow<DressUpSetupWindow>(true, "Dress Up Game Setup", true);
                window.minSize = new Vector2(420f, 220f);
                window.maxSize = new Vector2(420f, 220f);
            };
        }

        private static bool NeedsSetup()
        {
            return !File.Exists(ScenePath) || !File.Exists(CatalogPath);
        }

        private void OnGUI()
        {
            GUILayout.Space(12f);
            EditorGUILayout.LabelField("Dress Up Game", EditorStyles.boldLabel);
            GUILayout.Space(8f);

            EditorGUILayout.HelpBox(
                "MainScene has not been created yet.\n\n" +
                "Click the button below to generate the scene, UI, item data, and link your sprites.",
                MessageType.Info);

            GUILayout.Space(16f);

            if (GUILayout.Button("Create MainScene & Setup Game", GUILayout.Height(40f)))
            {
                RunFullSetup();
            }

            GUILayout.Space(8f);

            if (GUILayout.Button("Link Sprites Only", GUILayout.Height(28f)))
            {
                SpriteAssetLinker.ApplySpritesFromFolder();
            }
        }

        private static void RunFullSetup()
        {
            try
            {
                if (!Directory.Exists("Assets/Scenes"))
                {
                    Directory.CreateDirectory("Assets/Scenes");
                }

                if (!Directory.Exists("Assets/Resources"))
                {
                    Directory.CreateDirectory("Assets/Resources");
                }

                GameDataGenerator.GenerateAll();
                GameSceneBuilder.SetupMainScene();
                SpriteAssetLinker.ApplySpritesFromFolder();

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Setup Complete",
                    "MainScene was created at:\nAssets/Scenes/MainScene.unity\n\n" +
                    "Double-click it in the Project window, then press Play.",
                    "OK");

                if (File.Exists(ScenePath))
                {
                    EditorSceneManager.OpenScene(ScenePath);
                }

                GetWindow<DressUpSetupWindow>()?.Close();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Setup Failed", exception.Message, "OK");
            }
        }
    }
}
#endif
