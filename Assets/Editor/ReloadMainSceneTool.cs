#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Reloads MainScene so hierarchy changes (new Girl layers) appear in the Editor.
    /// </summary>
    public static class ReloadMainSceneTool
    {
        private const string ScenePath = "Assets/Scenes/MainScene.unity";

        [MenuItem("Dress Up Game/Reload Main Scene")]
        public static void ReloadMainScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Stop Play before reloading MainScene.",
                    "OK");
                return;
            }

            if (!System.IO.File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Scene Missing",
                    "MainScene was not found.\n\nRun Dress Up Game → Full Project Setup first.",
                    "OK");
                return;
            }

            UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.isDirty)
            {
                if (!EditorUtility.DisplayDialog(
                        "Reload Main Scene?",
                        "The open scene has unsaved changes.\n\n" +
                        "Reload discards those edits, loads MainScene from disk, and ensures Girl has a Necklace layer.\n\n" +
                        "Cancel to keep editing; save manually first if you need those changes.",
                        "Reload",
                        "Cancel"))
                {
                    return;
                }
            }

            EditorSceneManager.OpenScene(ScenePath);
            GameBackgroundEditorUtility.EnsureInOpenScene();

            bool addedNecklace = SetupNecklaceLayerTool.EnsureNecklaceLayerInOpenScene();
            bool addedEarrings = SetupEarringLayerTool.EnsureEarringsLayerInOpenScene();
            bool addedShoes = SetupShoesLayerTool.EnsureShoesLayerInOpenScene();
            SetupShoesCategoryBarTool.SetupShoesCategoryTab();
            if (addedNecklace || addedEarrings || addedShoes)
            {
                EditorSceneManager.SaveOpenScenes();
            }

            Debug.Log(
                addedNecklace || addedEarrings || addedShoes
                    ? "Dress Up Game: MainScene reloaded; missing Girl layers were added (scene saved)."
                    : "Dress Up Game: MainScene reloaded. Girl should include Shoes, Necklace, and Earrings.");
        }
    }
}
#endif
