#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Writes the AdMob app ID into Google Mobile Ads plugin settings.
    /// </summary>
    public static class ConfigureAdMobSettingsTool
    {
        private const string AppId = "ca-app-pub-5452343649745884~2916744238";
        private const string SettingsTypeName = "GoogleMobileAds.Editor.GoogleMobileAdsSettings, GoogleMobileAds.Editor";

        [MenuItem("Dress Up Game/Configure AdMob App ID")]
        public static void ConfigureAdMobSettings()
        {
            Type settingsType = Type.GetType(SettingsTypeName);
            if (settingsType == null)
            {
                EditorUtility.DisplayDialog(
                    "Google Mobile Ads Not Installed",
                    "Install the Google Mobile Ads Unity package first.\n\n" +
                    "Open Window > Package Manager, select My Registries, and install " +
                    "\"Google Mobile Ads for Unity\" (com.google.ads.mobile).\n\n" +
                    "Then run this menu again.",
                    "OK");
                return;
            }

            object settings = LoadSettingsInstance(settingsType);
            if (settings == null)
            {
                EditorUtility.DisplayDialog(
                    "AdMob Settings Missing",
                    "Could not open Google Mobile Ads settings asset.",
                    "OK");
                return;
            }

            PropertyInfo androidAppId = settingsType.GetProperty("GoogleMobileAdsAndroidAppId");
            PropertyInfo iosAppId = settingsType.GetProperty("GoogleMobileAdsIOSAppId");
            if (androidAppId == null)
            {
                EditorUtility.DisplayDialog(
                    "AdMob Settings API Changed",
                    "Could not find GoogleMobileAdsAndroidAppId on GoogleMobileAdsSettings.",
                    "OK");
                return;
            }

            androidAppId.SetValue(settings, AppId);
            iosAppId?.SetValue(settings, AppId);
            EditorUtility.SetDirty(settings as UnityEngine.Object);
            AssetDatabase.SaveAssets();

            Debug.Log($"Dress Up Game: AdMob app ID configured ({AppId}).");
        }

        private static object LoadSettingsInstance(Type settingsType)
        {
            MethodInfo loadInstance = settingsType.GetMethod(
                "LoadInstance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return loadInstance?.Invoke(null, null);
        }
    }
}
#endif
