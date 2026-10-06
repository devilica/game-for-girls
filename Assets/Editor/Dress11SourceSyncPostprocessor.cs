#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public class Dress11SourceSyncPostprocessor : AssetPostprocessor
    {
        private const string Dress11SourcePath = "Assets/Sprites/Character/Dresses/dress_11.png";

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (GenerateDress11ColorSpritesTool.IsSyncInProgress)
            {
                return;
            }

            if (!ContainsDress11Source(importedAssets)
                && !ContainsDress11Source(deletedAssets)
                && !ContainsDress11Source(movedAssets)
                && !ContainsDress11Source(movedFromAssetPaths))
            {
                return;
            }

            Sprite original = AssetDatabase.LoadAssetAtPath<Sprite>(Dress11SourcePath);
            if (original == null)
            {
                Debug.LogWarning("Dress 11: source dress_11.png missing; skipping variant sync.");
                return;
            }

            GenerateDress11ColorSpritesTool.DressLayout layout = ReadLayout();
            GenerateDress11ColorSpritesTool.EnsureVariantTexturesAndItems(layout, original);
            AssetDatabase.SaveAssets();
        }

        private static bool ContainsDress11Source(string[] paths)
        {
            if (paths == null)
            {
                return false;
            }

            foreach (string path in paths)
            {
                if (path == Dress11SourcePath)
                {
                    return true;
                }
            }

            return false;
        }

        private static GenerateDress11ColorSpritesTool.DressLayout ReadLayout()
        {
            DressUpGame.Data.DressItem existing = AssetDatabase.LoadAssetAtPath<DressUpGame.Data.DressItem>(
                "Assets/Resources/Items/Dresses/Dress_11.asset");
            if (existing != null)
            {
                return new GenerateDress11ColorSpritesTool.DressLayout(
                    existing.LayerOffset,
                    existing.LayerScale);
            }

            return GenerateDress11ColorSpritesTool.DressLayout.Default;
        }
    }
}
#endif
