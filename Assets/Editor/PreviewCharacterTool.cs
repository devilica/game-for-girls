#if UNITY_EDITOR
using DressUpGame.Character;
using DressUpGame.Save;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Shows the dressed character in the Scene view without entering Play mode.
    /// </summary>
    public static class PreviewCharacterTool
    {
        [MenuItem("Dress Up Game/Preview Character In Scene")]
        public static void PreviewCharacterInScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Stop Play before previewing the character in the Scene view.",
                    "OK");
                return;
            }

            GameBackgroundEditorUtility.EnsureInOpenScene();

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog(
                    "No Girl",
                    "No Girl with CharacterCustomizer found in the open scene.",
                    "OK");
                return;
            }

            CharacterSaveManager saveManager = Object.FindAnyObjectByType<CharacterSaveManager>();
            if (saveManager != null)
            {
                saveManager.LoadCharacter();
            }
            else
            {
                customizer.InitializeDefaults();
            }

            customizer.ApplySortingOrders();

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            SceneView.RepaintAll();

            Debug.Log(
                "Dress Up Game: Character preview applied in Scene view. " +
                "Build and Play use the same runtime loading — an empty Scene view is normal before preview.");
        }
    }
}
#endif
