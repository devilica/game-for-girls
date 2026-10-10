#if UNITY_EDITOR
using DressUpGame.MysteryBox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DressUpGame.Editor
{
    public static class MysteryBoxRevealEditorPreviewTool
    {
        private const string SinglePreviewResourcePath = "MemoryBows/bow_1";
        private const string DualPrimaryResourcePath = "MemoryBows/bow_2";
        private const string DualSecondaryResourcePath = "MemoryBows/bow_3";

        [MenuItem("Dress Up Game/Preview Mystery Reveal In Editor")]
        public static void PreviewSingleGiftReveal()
        {
            if (!TryGetMysteryBoxUi(out MysteryBoxUI ui))
            {
                return;
            }

            Sprite preview = LoadPreviewSprite(SinglePreviewResourcePath);
            if (preview == null)
            {
                EditorUtility.DisplayDialog(
                    "Preview Sprite Missing",
                    "Could not load Resources/MemoryBows/bow_1 for the fake gift preview.",
                    "OK");
                return;
            }

            ApplyPreview(ui, preview, null);
        }

        [MenuItem("Dress Up Game/Preview Mystery Reveal (Dual Gifts)")]
        public static void PreviewDualGiftReveal()
        {
            if (!TryGetMysteryBoxUi(out MysteryBoxUI ui))
            {
                return;
            }

            Sprite primary = LoadPreviewSprite(DualPrimaryResourcePath);
            Sprite secondary = LoadPreviewSprite(DualSecondaryResourcePath);
            if (primary == null || secondary == null)
            {
                EditorUtility.DisplayDialog(
                    "Preview Sprites Missing",
                    "Could not load bow_2 and bow_3 from Resources/MemoryBows for dual preview.",
                    "OK");
                return;
            }

            ApplyPreview(ui, primary, secondary);
        }

        [MenuItem("Dress Up Game/Reset Mystery Reveal Layout to Defaults")]
        public static void ResetRevealLayoutToDefaults()
        {
            if (!TryGetMysteryBoxUi(out MysteryBoxUI ui))
            {
                return;
            }

            ui.ApplyDefaultRevealLayout();
            MarkSceneDirty();
            SceneView.RepaintAll();
            Debug.Log("Dress Up Game: Mystery reveal layout reset to code defaults.");
        }

        [MenuItem("Dress Up Game/Hide Mystery Reveal Preview")]
        public static void HideRevealPreview()
        {
            if (!TryGetMysteryBoxUi(out MysteryBoxUI ui))
            {
                return;
            }

            ui.HideReveal();
            MarkSceneDirty();
            Debug.Log("Dress Up Game: Mystery reveal preview hidden.");
        }

        [MenuItem("Dress Up Game/Preview Mystery Reveal In Editor", true)]
        [MenuItem("Dress Up Game/Preview Mystery Reveal (Dual Gifts)", true)]
        [MenuItem("Dress Up Game/Hide Mystery Reveal Preview", true)]
        private static bool ValidatePreviewMenus()
        {
            return !EditorApplication.isPlaying && Object.FindAnyObjectByType<MysteryBoxUI>() != null;
        }

        private static bool TryGetMysteryBoxUi(out MysteryBoxUI ui)
        {
            ui = Object.FindAnyObjectByType<MysteryBoxUI>();
            if (ui != null)
            {
                return true;
            }

            EditorUtility.DisplayDialog(
                "Mystery Box UI Not Found",
                "Open Assets/Scenes/MysteryBoxScene.unity in the Editor (not Play Mode), then run this menu again.",
                "OK");
            return false;
        }

        private static void ApplyPreview(MysteryBoxUI ui, Sprite primary, Sprite secondary)
        {
            ui.ShowEditorRevealPreview(primary, secondary);
            MarkSceneDirty();
            SceneView.RepaintAll();
            Debug.Log(
                "Dress Up Game: Mystery reveal preview shown (Game tab = full background). " +
                "Move Apply/Refresh/Preview in the Scene, then save the scene (Ctrl+S). " +
                "Play Mode keeps saved positions. Use Reset Mystery Reveal Layout to Defaults to undo.");
        }

        private static Sprite LoadPreviewSprite(string resourcePath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                return sprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        private static void MarkSceneDirty()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }
    }
}
#endif
