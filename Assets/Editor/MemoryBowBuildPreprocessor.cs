#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DressUpGame.Editor
{
    public class MemoryBowBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            int copied = SyncMemoryBowSpritesTool.SyncMemoryBowSpritesFromSource(regenerateCatalog: true);
            if (copied < SyncMemoryBowSpritesTool.LastBowIndex)
            {
                Debug.LogWarning(
                    $"Memory bow pre-build sync: copied {copied}/{SyncMemoryBowSpritesTool.LastBowIndex} sprites. " +
                    $"Ensure {SyncMemoryBowSpritesTool.SourceFolder} has bow_1..{SyncMemoryBowSpritesTool.LastBowIndex}.png.");
            }
            else
            {
                Debug.Log(
                    $"Memory bow pre-build sync: {copied} sprites copied from {SyncMemoryBowSpritesTool.SourceFolder} to {SyncMemoryBowSpritesTool.DestFolder}.");
            }
        }
    }
}
#endif
