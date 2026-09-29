#if UNITY_EDITOR
using System.IO;
using DressUpGame.Data;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Ensures eyes_none exists, then delegates color generation to Generate Eye Colors.
    /// </summary>
    public static class GenerateEyesTool
    {
        private const string MakeupFolder = "Assets/Resources/Items/Makeup";

        public static void GenerateEyeItems()
        {
            MakeupItem noneItem = LoadOrCreate<MakeupItem>($"{MakeupFolder}/eyes_none.asset");
            noneItem.SetRuntimeData("eyes_none", "None", null, MakeupType.Eyes, Color.white, true);
            EditorUtility.SetDirty(noneItem);
            AssetDatabase.SaveAssets();
            GenerateEyeColorsTool.GenerateEyeColors();
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
#endif
