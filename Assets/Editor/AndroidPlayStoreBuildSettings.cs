#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Forces Android release (non-debuggable) settings used for Google Play uploads.
    /// </summary>
    public static class AndroidPlayStoreBuildSettings
    {
        private const string AndroidBuildProfilePath = "Assets/Settings/Build Profiles/Android\u2122.asset";
        private const string SharedBuildProfilePath = "Library/BuildProfiles/SharedProfile.asset";

        [MenuItem("Dress Up Game/Fix Play Store Build Settings", false, -99)]
        [MenuItem("File/Dress Up Game/Fix Play Store Build Settings", false, 201)]
        public static void FixPlayStoreBuildSettings()
        {
            ApplyForReleaseBuild();

            EditorUtility.DisplayDialog(
                "Play Store Build Settings",
                "Applied release settings:\n\n" +
                "- Development Build: OFF\n" +
                "- Script Debugging: OFF\n" +
                "- Android Build Type: Release\n" +
                "- Build App Bundle: ON\n\n" +
                "Now run: Dress Up Game → Build Release AAB (Play Store)",
                "OK");
        }

        public static void ApplyForReleaseBuild()
        {
            ApplyReleaseEditorSettings();

            ForceReleaseOnAllProjectBuildProfiles();

            BuildProfile sharedProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>(SharedBuildProfilePath);
            ForceReleaseOnProfile(sharedProfile);

            AssetDatabase.SaveAssets();
        }

        public static void ApplyReleaseEditorSettings()
        {
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
            EditorUserBuildSettings.buildScriptsOnly = false;
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
        }

        public static void ForceReleaseOnProfile(BuildProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty iterator = serializedProfile.GetIterator();
            if (!iterator.Next(true))
            {
                return;
            }

            do
            {
                if (iterator.propertyType == SerializedPropertyType.Boolean && iterator.name == "m_Development")
                {
                    iterator.boolValue = false;
                }

                if (iterator.propertyType == SerializedPropertyType.Boolean && iterator.name == "m_AllowDebugging")
                {
                    iterator.boolValue = false;
                }

                if (iterator.propertyType == SerializedPropertyType.Boolean && iterator.name == "m_ConnectProfiler")
                {
                    iterator.boolValue = false;
                }

                if (iterator.propertyType == SerializedPropertyType.Integer && iterator.name == "m_BuildType")
                {
                    iterator.intValue = (int)AndroidBuildType.Release;
                }

                if (iterator.propertyType == SerializedPropertyType.Boolean && iterator.name == "m_BuildAppBundle")
                {
                    iterator.boolValue = true;
                }
            }
            while (iterator.Next(true));

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ForceReleaseOnAllProjectBuildProfiles()
        {
            string profilesFolder = "Assets/Settings/Build Profiles";
            if (!Directory.Exists(profilesFolder))
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:BuildProfile", new[] { profilesFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(path);
                ForceReleaseOnProfile(profile);
            }
        }
    }
}
#endif
