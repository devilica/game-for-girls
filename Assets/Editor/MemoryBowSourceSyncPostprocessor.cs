#if UNITY_EDITOR
using System.IO;
using UnityEditor;

namespace DressUpGame.Editor
{
    public class MemoryBowSourceSyncPostprocessor : AssetPostprocessor
    {
        private const string SourceFolderPrefix = "Assets/Sprites/Memory/Bows/";

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (SyncMemoryBowSpritesTool.IsSyncInProgress)
            {
                return;
            }

            if (!AnyMemoryBowSourceChanged(importedAssets)
                && !AnyMemoryBowSourceChanged(deletedAssets)
                && !AnyMemoryBowSourceChanged(movedAssets)
                && !AnyMemoryBowSourceChanged(movedFromAssetPaths))
            {
                return;
            }

            SyncMemoryBowSpritesTool.SyncMemoryBowSpritesFromSource(regenerateCatalog: true);
        }

        private static bool AnyMemoryBowSourceChanged(string[] paths)
        {
            if (paths == null)
            {
                return false;
            }

            foreach (string path in paths)
            {
                if (IsMemoryBowSourcePng(path))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMemoryBowSourcePng(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)
                || !assetPath.StartsWith(SourceFolderPrefix)
                || !assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            if (string.IsNullOrEmpty(fileName) || !fileName.StartsWith("bow_"))
            {
                return false;
            }

            return int.TryParse(fileName.Substring(4), out int index)
                && index >= SyncMemoryBowSpritesTool.FirstBowIndex
                && index <= SyncMemoryBowSpritesTool.LastBowIndex;
        }
    }
}
#endif
