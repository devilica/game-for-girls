using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Short tap vibration for item cards on mobile devices.
    /// </summary>
    public static class HapticFeedback
    {
        private const int AndroidTapDurationMs = 25;
        private const int AndroidTapAmplitude = 120;

        public static void PlayCardTap()
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            PlayAndroidTap(AndroidTapDurationMs);
#elif UNITY_IOS
            Handheld.Vibrate();
#endif
        }

#if UNITY_ANDROID
        private static void PlayAndroidTap(long durationMs)
        {
            try
            {
                using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (vibrator == null)
                {
                    return;
                }

                if (vibrator.Call<bool>("hasVibrator") == false)
                {
                    return;
                }

                using AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION");
                int sdk = versionClass.GetStatic<int>("SDK_INT");

                if (sdk >= 26)
                {
                    using AndroidJavaClass effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                    using AndroidJavaObject effect = effectClass.CallStatic<AndroidJavaObject>(
                        "createOneShot",
                        durationMs,
                        AndroidTapAmplitude);
                    vibrator.Call("vibrate", effect);
                }
                else
                {
                    vibrator.Call("vibrate", durationMs);
                }
            }
            catch (System.Exception)
            {
                Handheld.Vibrate();
            }
        }
#endif
    }
}
