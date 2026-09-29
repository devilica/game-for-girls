#if UNITY_EDITOR
using DressUpGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Creates or migrates WizardNavOverlay with kid-friendly arrow buttons on the Canvas.
    /// </summary>
    public static class SetupWizardNavTool
    {
        private const string ScenePath = "Assets/Scenes/MainScene.unity";

        [MenuItem("Dress Up Game/Setup Wizard Arrow Buttons")]
        public static void SetupWizardArrowButtons()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Stop Play before setting up wizard arrow buttons.",
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

            Transform canvasTransform = canvas.transform;
            Transform wizardPanel = canvasTransform.Find("WizardPanel");
            Transform legacyNavBar = wizardPanel != null ? wizardPanel.Find("WizardNavBar") : null;

            Transform overlay = canvasTransform.Find("WizardNavOverlay");
            if (overlay == null)
            {
                GameObject overlayGo = new GameObject("WizardNavOverlay");
                overlayGo.transform.SetParent(canvasTransform, false);
                overlay = overlayGo.transform;

                RectTransform overlayRect = overlayGo.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
            }

            overlay.SetAsLastSibling();
            overlay.gameObject.SetActive(false);

            Button prevButton = EnsureArrowButton(overlay, legacyNavBar, "PrevArrowButton", WizardArrowButton.ArrowDirection.Previous);
            Button nextButton = EnsureArrowButton(overlay, legacyNavBar, "NextArrowButton", WizardArrowButton.ArrowDirection.Next);

            PositionArrow(prevButton, true);
            PositionArrow(nextButton, false);

            if (legacyNavBar != null)
            {
                legacyNavBar.gameObject.SetActive(false);
            }

            OpeningWizardController wizard = Object.FindAnyObjectByType<OpeningWizardController>();
            if (wizard != null)
            {
                SerializedObject wizardSo = new SerializedObject(wizard);
                if (wizardPanel != null)
                {
                    wizardSo.FindProperty("wizardPanel").objectReferenceValue = wizardPanel.gameObject;
                }

                wizardSo.FindProperty("wizardNavOverlay").objectReferenceValue = overlay.gameObject;
                wizardSo.FindProperty("prevArrowButton").objectReferenceValue = prevButton;
                wizardSo.FindProperty("nextArrowButton").objectReferenceValue = nextButton;
                wizardSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(wizard);
            }

            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Dress Up Game: Wizard arrow buttons are set up on WizardNavOverlay.");
        }

        private static Button EnsureArrowButton(
            Transform overlay,
            Transform legacyNavBar,
            string buttonName,
            WizardArrowButton.ArrowDirection direction)
        {
            Transform existing = overlay.Find(buttonName);
            if (existing == null && legacyNavBar != null)
            {
                existing = legacyNavBar.Find(buttonName);
                if (existing != null)
                {
                    existing.SetParent(overlay, false);
                }
            }

            GameObject buttonGo;
            if (existing != null)
            {
                buttonGo = existing.gameObject;
            }
            else
            {
                buttonGo = new GameObject(buttonName);
                buttonGo.transform.SetParent(overlay, false);
                buttonGo.AddComponent<RectTransform>();
                buttonGo.AddComponent<Image>();
                buttonGo.AddComponent<Button>();

                GameObject textGo = new GameObject("Text");
                textGo.transform.SetParent(buttonGo.transform, false);
                RectTransform textRect = textGo.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                textGo.AddComponent<Text>();
            }

            WizardArrowButton arrowStyle = buttonGo.GetComponent<WizardArrowButton>();
            if (arrowStyle == null)
            {
                arrowStyle = buttonGo.AddComponent<WizardArrowButton>();
            }

            arrowStyle.Configure(direction);
            return buttonGo.GetComponent<Button>();
        }

        private static void PositionArrow(Button button, bool isLeft)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            const float bodyBottomY = 730f;
            rect.anchorMin = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.anchorMax = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.pivot = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.anchoredPosition = new Vector2(isLeft ? 24f : -24f, bodyBottomY);

            WizardArrowButton arrowStyle = button.GetComponent<WizardArrowButton>();
            if (arrowStyle != null)
            {
                arrowStyle.ApplyStyle();
            }
        }
    }
}
#endif
