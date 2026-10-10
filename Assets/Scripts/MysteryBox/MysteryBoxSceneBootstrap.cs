using UnityEngine;
using UnityEngine.SceneManagement;

namespace DressUpGame.MysteryBox
{
    public static class MysteryBoxSceneBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureControllerOnMysteryScene()
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.name != MysteryBoxController.MysteryBoxSceneName)
            {
                return;
            }

            if (Object.FindAnyObjectByType<MysteryBoxController>() != null)
            {
                return;
            }

            GameObject host = new GameObject("MysteryBoxController");
            host.AddComponent<MysteryBoxController>();
        }
    }
}
