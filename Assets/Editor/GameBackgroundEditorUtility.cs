#if UNITY_EDITOR
using DressUpGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class GameBackgroundEditorUtility
    {
        public static void EnsureInOpenScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Stop Play before adding the game background.",
                    "OK");
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = Object.FindAnyObjectByType<Camera>();
            }

            if (camera == null)
            {
                EditorUtility.DisplayDialog(
                    "Camera Missing",
                    "Open MainScene with a Main Camera first.",
                    "OK");
                return;
            }

            GameBackgroundDisplay existing = Object.FindAnyObjectByType<GameBackgroundDisplay>();
            if (existing == null)
            {
                GameObject backgroundObject = new GameObject("GameBackground");
                existing = backgroundObject.AddComponent<GameBackgroundDisplay>();
                Undo.RegisterCreatedObjectUndo(backgroundObject, "Create Game Background");
            }

            existing.Apply(camera);
            EditorUtility.SetDirty(existing.gameObject);
            EditorSceneManager.MarkSceneDirty(existing.gameObject.scene);
            Debug.Log("Dress Up Game: Game background added behind the character.");
        }
    }
}
#endif
