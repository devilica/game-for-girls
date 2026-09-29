#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Builds a non-debuggable Android App Bundle for Google Play upload.
    /// </summary>
    public static class BuildReleaseAndroidTool
    {
        private const string OutputPath = "Builds/Android/DressUpMakeupGirls.aab";

        [MenuItem("Dress Up Game/Build Release AAB (Play Store)", false, -100)]
        [MenuItem("File/Dress Up Game/Build Release AAB (Play Store)", false, 200)]
        public static void BuildReleaseAab()
        {
            if (!PlayerSettings.Android.useCustomKeystore)
            {
                EditorUtility.DisplayDialog(
                    "Keystore Not Configured",
                    "Set your release keystore in Edit > Project Settings > Player > Android > Publishing Settings before building.",
                    "OK");
                return;
            }

            AndroidPlayStoreBuildSettings.ApplyForReleaseBuild();

            string outputDirectory = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Scenes In Build",
                    "Add SplashScene and MainScene in File > Build Profiles > Scene List.",
                    "OK");
                return;
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string versionName = PlayerSettings.bundleVersion;
                int versionCode = PlayerSettings.Android.bundleVersionCode;
                EditorUtility.DisplayDialog(
                    "Release AAB Built",
                    "Upload this file to Google Play:\n\n" + Path.GetFullPath(OutputPath) +
                    "\n\nVersion name: " + versionName +
                    "\nVersion code: " + versionCode +
                    "\n\nThis build is non-debuggable (Release).",
                    "OK");
                Debug.Log(
                    "Dress Up Game: Release AAB built at " + Path.GetFullPath(OutputPath) +
                    " (version " + versionName + ", code " + versionCode + ").");
                return;
            }

            EditorUtility.DisplayDialog(
                "Build Failed",
                "Release AAB build failed. Check the Console for details.",
                "OK");
        }

    }
}
#endif
