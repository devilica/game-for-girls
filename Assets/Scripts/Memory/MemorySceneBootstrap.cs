using DressUpGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DressUpGame.Memory
{
    /// <summary>
    /// Ensures MemoryScene always has a playable host if the scene asset is empty or stale in build settings.
    /// </summary>
    public static class MemorySceneBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.name != MemoryGameController.MemorySceneName)
            {
                return;
            }

            GamePlayBootstrap.EnsureAudioListenerForActiveScene();

            if (Object.FindAnyObjectByType<MemoryGameController>() != null)
            {
                return;
            }

            GameObject host = new GameObject("MemoryGameHost");
            host.AddComponent<MemoryGameController>();
        }
    }
}
