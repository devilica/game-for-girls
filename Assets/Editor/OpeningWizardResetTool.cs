#if UNITY_EDITOR
using DressUpGame.Save;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class OpeningWizardResetTool
    {
        [MenuItem("Dress Up Game/Reset Opening Wizard")]
        public static void ResetOpeningWizard()
        {
            CharacterSaveManager saveManager = Object.FindAnyObjectByType<CharacterSaveManager>();
            if (saveManager != null)
            {
                saveManager.ClearWizardComplete();
            }
            else
            {
                PlayerPrefs.DeleteKey("DressUpGame_WizardComplete");
                PlayerPrefs.Save();
            }

            Debug.Log("Dress Up Game: Opening wizard flag cleared. Press Play to see the wizard again.");
            EditorUtility.DisplayDialog(
                "Wizard Reset",
                "Opening wizard flag cleared.\n\nPress Play to go through Dress → Hair → Hair color → Eyeshadow → Blush → Lips → Crown → Glasses again.\n\n" +
                "Run Dress Up Game → Fix UI Layout first if the wizard panel is missing.",
                "OK");
        }
    }
}
#endif
