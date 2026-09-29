using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Mobile ad SDKs pause Unity via timeScale; this restores normal gameplay input afterward.
    /// </summary>
    public static class GameplayPauseGuard
    {
        public static void EnsureUnpaused()
        {
            MainThreadDispatcher.Run(() =>
            {
                if (Time.timeScale != 1f)
                {
                    Time.timeScale = 1f;
                }
            });
        }
    }
}
