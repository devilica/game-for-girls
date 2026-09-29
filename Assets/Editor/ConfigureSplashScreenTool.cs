#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Keeps splash.png assigned for legacy player settings while SplashScene handles the visible splash.
    /// </summary>
    public static class ConfigureSplashScreenTool
    {
        private const string SplashPath = "Assets/Resources/UI/splash.png";

        [MenuItem("Dress Up Game/Configure Splash Screen")]
        public static void ConfigureSplashScreen()
        {
            Sprite splashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SplashPath);

            if (splashSprite == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Splash Image",
                    "Could not find splash.png at:\n" + SplashPath,
                    "OK");
                return;
            }

            // Use SplashScene for the visible splash; avoid a second smaller Unity splash handoff.
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.backgroundColor = new Color(0.98f, 0.82f, 0.90f);
            PlayerSettings.SplashScreen.backgroundPortrait = splashSprite;
            PlayerSettings.SplashScreen.background = splashSprite;
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
            PlayerSettings.SplashScreen.blurBackgroundImage = false;
            PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFill;

            ConfigureAppIconTool.ConfigureFromSplashSprite(splashSprite);

            AssetDatabase.SaveAssets();
            Debug.Log("Dress Up Game: SplashScene splash configured with splash.png (built-in Unity splash disabled).");
        }
    }
}
#endif
