#if UNITY_EDITOR
using DressUpGame.UI;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class ReloadMainMenuLayoutTool
    {
        [MenuItem("Dress Up Game/Reload Main Menu Layout")]
        public static void ReloadMainMenuLayoutFromMenu()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Play Menu Scene",
                    "Enter Play Mode on MenuScene (or run from Splash), then use this menu again.\n\n" +
                    "Or use Dress Up Game → Setup Main Menu Scene and press Play.",
                    "OK");
                return;
            }

            MainMenuScreenController controller = Object.FindAnyObjectByType<MainMenuScreenController>();
            if (controller == null)
            {
                EditorUtility.DisplayDialog(
                    "Main Menu Not Found",
                    "Open MenuScene and enter Play Mode, then run Reload Main Menu Layout again.",
                    "OK");
                return;
            }

            controller.ForceReloadMenuLayout();
            Debug.Log("Dress Up Game: Main menu layout reloaded.");
        }
    }
}
#endif
