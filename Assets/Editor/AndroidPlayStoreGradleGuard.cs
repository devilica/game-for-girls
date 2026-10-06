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

            if (EditorUserBuildSettings.androidBuildType == AndroidBuildType.Release)
            {
                AndroidPlayStoreBuildSettings.ApplyAndroidReleasePlayerSettings();
                EditorUserBuildSettings.development = false;
                EditorUserBuildSettings.allowDebugging = false;
                EditorUserBuildSettings.connectProfiler = false;
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
            string gradleRoot = Directory.GetParent(path)?.FullName;
            if (string.IsNullOrEmpty(gradleRoot))
            {
                return;
            }

            EnsureLauncherProguardRules(gradleRoot);
            EnsureAdsMobileSdkDependency(gradleRoot);
            EnsureNextGenAdsExclusions(gradleRoot);

            string launcherManifestPath = Path.Combine(gradleRoot, "launcher", "src", "main", "AndroidManifest.xml");
            StripDebuggableFromManifest(launcherManifestPath);
            EnsureAdMobApplicationIdInManifest(launcherManifestPath);

            string unityManifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            StripDebuggableFromManifest(unityManifestPath);
            EnsureAdMobApplicationIdInManifest(unityManifestPath);
        }

        private const string AdMobApplicationIdMetaName = "com.google.android.gms.ads.APPLICATION_ID";
        private const string DefaultAdMobAndroidAppId = "ca-app-pub-5452343649745884~2916744238";

        private static void EnsureAdMobApplicationIdInManifest(string manifestPath)
        {
            if (!File.Exists(manifestPath))
            {
                return;
            }

            string appId = LoadAdMobAndroidAppId();
            if (string.IsNullOrEmpty(appId))
            {
                return;
            }

            string manifest = File.ReadAllText(manifestPath);
            if (manifest.Contains(AdMobApplicationIdMetaName))
            {
                manifest = Regex.Replace(
                    manifest,
                    "(<meta-data\\s+android:name=\"" + Regex.Escape(AdMobApplicationIdMetaName) +
                    "\"\\s+android:value=\")[^\"]*(\")",
                    "${1}" + appId + "${2}",
                    RegexOptions.IgnoreCase);
            }
            else
            {
                const string applicationClose = "</application>";
                int closeIndex = manifest.IndexOf(applicationClose, System.StringComparison.OrdinalIgnoreCase);
                if (closeIndex < 0)
                {
                    return;
                }

                string metaData =
                    "    <meta-data android:name=\"" + AdMobApplicationIdMetaName +
                    "\" android:value=\"" + appId + "\" />" + System.Environment.NewLine;
                manifest = manifest.Insert(closeIndex, metaData);
            }

            File.WriteAllText(manifestPath, manifest);
        }

        private static string LoadAdMobAndroidAppId()
        {
            string settingsPath = Path.Combine(Application.dataPath, "GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");
            if (!File.Exists(settingsPath))
            {
                return DefaultAdMobAndroidAppId;
            }

            string settingsText = File.ReadAllText(settingsPath);
            Match match = Regex.Match(settingsText, @"adMobAndroidAppId:\s*(\S+)");
            if (match.Success && match.Groups[1].Value.Contains("~"))
            {
                return match.Groups[1].Value;
            }

            return DefaultAdMobAndroidAppId;
        }

        private const string AdsMobileSdkGradleLine =
            "    implementation 'com.google.android.libraries.ads.mobile.sdk:ads-mobile-sdk:1.4.0'";

        private static void EnsureAdsMobileSdkDependency(string gradleRoot)
        {
            EnsureAdsMobileSdkInBuildGradle(Path.Combine(gradleRoot, "unityLibrary", "build.gradle"));
            EnsureAdsMobileSdkInBuildGradle(Path.Combine(gradleRoot, "launcher", "build.gradle"));
        }

        private static void EnsureAdsMobileSdkInBuildGradle(string buildGradlePath)
        {
            if (!File.Exists(buildGradlePath))
            {
                return;
            }

            string content = File.ReadAllText(buildGradlePath);
            if (content.Contains("ads-mobile-sdk"))
            {
                return;
            }

            const string marker = "dependencies {";
            int idx = content.IndexOf(marker, System.StringComparison.Ordinal);
            if (idx < 0)
            {
                return;
            }

            int insertAt = idx + marker.Length;
            while (insertAt < content.Length && (content[insertAt] == '\r' || content[insertAt] == '\n'))
            {
                insertAt++;
            }

            content = content.Insert(insertAt, System.Environment.NewLine + AdsMobileSdkGradleLine);
            File.WriteAllText(buildGradlePath, content);
        }

        private const string NextGenAdsExclusionsBlock =
            "configurations.configureEach {\r\n" +
            "    exclude group: 'com.google.android.gms', module: 'play-services-ads'\r\n" +
            "    exclude group: 'com.google.android.gms', module: 'play-services-ads-lite'\r\n" +
            "}";

        private static void EnsureNextGenAdsExclusions(string gradleRoot)
        {
            EnsureNextGenAdsExclusionsInBuildGradle(Path.Combine(gradleRoot, "unityLibrary", "build.gradle"));
            EnsureNextGenAdsExclusionsInBuildGradle(Path.Combine(gradleRoot, "launcher", "build.gradle"));
        }

        private static void EnsureNextGenAdsExclusionsInBuildGradle(string buildGradlePath)
        {
            if (!File.Exists(buildGradlePath))
            {
                return;
            }

            string content = File.ReadAllText(buildGradlePath);
            if (content.Contains("module: 'play-services-ads-lite'"))
            {
                return;
            }

            const string marker = "dependencies {";
            int idx = content.IndexOf(marker, System.StringComparison.Ordinal);
            if (idx < 0)
            {
                return;
            }

            int closeIdx = content.IndexOf("\n}", idx, System.StringComparison.Ordinal);
            if (closeIdx < 0)
            {
                return;
            }

            content = content.Insert(closeIdx + 2, System.Environment.NewLine + System.Environment.NewLine + NextGenAdsExclusionsBlock);
            File.WriteAllText(buildGradlePath, content);
        }

        private static void EnsureLauncherProguardRules(string gradleRoot)
        {
            string sourceRules = Path.Combine(Application.dataPath, "Plugins/Android/proguard-user.txt");
            if (!File.Exists(sourceRules))
            {
                return;
            }

            string launcherRules = Path.Combine(gradleRoot, "launcher", "proguard-user.txt");
            File.Copy(sourceRules, launcherRules, true);
        }

        private static void StripDebuggableFromManifest(string manifestPath)
        {
            if (!File.Exists(manifestPath))
            {
                return;
            }

            string manifest = File.ReadAllText(manifestPath);
            manifest = Regex.Replace(
                manifest,
                "\\s*android:debuggable=\"[^\"]*\"",
                string.Empty,
                RegexOptions.IgnoreCase);

            File.WriteAllText(manifestPath, manifest);
        }
    }
}
#endif
