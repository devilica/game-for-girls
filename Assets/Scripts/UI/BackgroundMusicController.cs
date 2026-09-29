using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Plays looping background music for the dress-up game.
    /// </summary>
    public class BackgroundMusicController : MonoBehaviour
    {
        public const string MusicResourcePath = "UI/game_music_dress_up";
        private const string MutePreferenceKey = "DressUpGame.MusicMuted";

        public static BackgroundMusicController Instance { get; private set; }

        private static bool sessionSoundInitialized;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] [Range(0f, 1f)] private float volume = 0.55f;

        public bool IsMuted { get; private set; }

        public static bool IsMutedInPreferences()
        {
            return PlayerPrefs.GetInt(MutePreferenceKey, 0) == 1;
        }

        public static BackgroundMusicController EnsureExists()
        {
            if (Instance != null)
            {
                return Instance;
            }

            if (!Application.isPlaying)
            {
                return null;
            }

            BackgroundMusicController existing = FindAnyObjectByType<BackgroundMusicController>();
            if (existing != null)
            {
                Instance = existing;
                existing.InitializeIfNeeded();
                return existing;
            }

            GameObject musicObject = new GameObject("BackgroundMusic");
            BackgroundMusicController controller = musicObject.AddComponent<BackgroundMusicController>();
            DontDestroyOnLoad(musicObject);
            controller.InitializeIfNeeded();
            return controller;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializeIfNeeded();
        }

        private void InitializeIfNeeded()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (audioSource.clip == null)
            {
                audioSource.clip = Resources.Load<AudioClip>(MusicResourcePath);
            }

            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = volume;

            if (!sessionSoundInitialized)
            {
                sessionSoundInitialized = true;
                IsMuted = false;
                PlayerPrefs.SetInt(MutePreferenceKey, 0);
            }
            else
            {
                IsMuted = PlayerPrefs.GetInt(MutePreferenceKey, 0) == 1;
            }

            ApplyMuteState(startPlayback: true);
        }

        public void SetMuted(bool muted)
        {
            IsMuted = muted;
            PlayerPrefs.SetInt(MutePreferenceKey, muted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMuteState(startPlayback: true);
        }

        public void ToggleMuted()
        {
            SetMuted(!IsMuted);
        }

        private void ApplyMuteState(bool startPlayback)
        {
            if (audioSource == null || audioSource.clip == null)
            {
                return;
            }

            AudioListener.pause = false;
            audioSource.mute = IsMuted;

            if (!IsMuted && startPlayback && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
