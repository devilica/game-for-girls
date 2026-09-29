#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Keeps Android App Bundle builds non-debuggable for Google Play.
    /// </summary>
    public class AndroidPlayStoreGradleGuard : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 999;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
            {
                return;
            }

            if (!EditorUserBuildSettings.buildAppBundle)
            {
                return;
            }

            AndroidPlayStoreBuildSettings.ApplyForReleaseBuild();

            if (EditorUserBuildSettings.development ||
                EditorUserBuildSettings.allowDebugging ||
                EditorUserBuildSettings.androidBuildType != AndroidBuildType.Release)
            {
                Debug.LogWarning(
                    "Dress Up Game: Android AAB build was not release-safe. Forced Release + non-development settings.");
                AndroidPlayStoreBuildSettings.ApplyReleaseEditorSettings();
            }
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if (!EditorUserBuildSettings.buildAppBundle)
            {
                return;
            }

            string gradleRoot = Directory.GetParent(path)?.FullName;
            if (string.IsNullOrEmpty(gradleRoot))
            {
                return;
            }

            string launcherManifestPath = Path.Combine(gradleRoot, "launcher", "src", "main", "AndroidManifest.xml");
            StripDebuggableFromManifest(launcherManifestPath);

            string unityManifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            StripDebuggableFromManifest(unityManifestPath);
        }

        private static void StripDebuggableFromManifest(string manifestPath)
        {
            if (!File.Exists(manifestPath))
            {
                return;
            }

            string manifest = File.ReadAllText(manifestPath);
            manifest = Regex.Replace(manifest, "\\s*android:debuggable=\"true\"", string.Empty);
            manifest = manifest.Replace("android:debuggable=\"True\"", "android:debuggable=\"false\"");
            if (!manifest.Contains("android:debuggable"))
            {
                manifest = Regex.Replace(
                    manifest,
                    "<application\\s",
                    "<application android:debuggable=\"false\" ");
            }

            File.WriteAllText(manifestPath, manifest);
        }
    }
}
#endif
