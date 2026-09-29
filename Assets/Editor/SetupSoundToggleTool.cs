#if UNITY_EDITOR
using DressUpGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class SetupSoundToggleTool
    {
        [MenuItem("Dress Up Game/Setup Sound Toggle Button")]
        public static void SetupSoundToggleButton()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Stop Play before setting up the sound toggle button.",
                    "OK");
                return;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog(
                    "No Canvas",
                    "Open MainScene with the game Canvas first.",
                    "OK");
                return;
            }

            SoundToggleButton toggle = SoundToggleButton.EnsureOnCanvas(canvas.transform);
            if (toggle != null)
            {
                EditorUtility.SetDirty(toggle);
            }

            Transform overlay = canvas.transform.Find(SoundToggleButton.OverlayName);
            overlay?.SetAsLastSibling();

            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("Dress Up Game: Sound toggle button is set up on SoundToggleOverlay.");
        }
    }
}
#endif
