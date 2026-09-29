using DressUpGame.Character;
using DressUpGame.Data;
using UnityEngine;

namespace DressUpGame.Save
{
    /// <summary>
    /// Persists character customization using PlayerPrefs.
    /// </summary>
    public class CharacterSaveManager : MonoBehaviour
    {
        private const string SaveKey = "DressUpGame_CharacterSave";
        private const string WizardCompleteKey = "DressUpGame_WizardComplete";

        [SerializeField] private CharacterCustomizer customizer;

        public CharacterCustomizer Customizer => customizer;

        public bool HasCompletedWizard()
        {
            return PlayerPrefs.GetInt(WizardCompleteKey, 0) == 1;
        }

        public void MarkWizardComplete()
        {
            PlayerPrefs.SetInt(WizardCompleteKey, 1);
            PlayerPrefs.Save();
        }

        public void ClearWizardComplete()
        {
            if (PlayerPrefs.HasKey(WizardCompleteKey))
            {
                PlayerPrefs.DeleteKey(WizardCompleteKey);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Clears saved look and wizard flag so the next MainScene load starts from the opening wizard.
        /// </summary>
        public static void ClearAllGameProgress()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                PlayerPrefs.DeleteKey(SaveKey);
            }

            if (PlayerPrefs.HasKey(WizardCompleteKey))
            {
                PlayerPrefs.DeleteKey(WizardCompleteKey);
            }

            PlayerPrefs.Save();
        }

        private void Awake()
        {
            if (customizer == null)
            {
                customizer = FindAnyObjectByType<CharacterCustomizer>();
            }
        }

        public void SaveCharacter()
        {
            if (customizer == null)
            {
                Debug.LogWarning("CharacterSaveManager: No CharacterCustomizer assigned.");
                return;
            }

            CharacterSaveData data = customizer.CaptureSaveData();
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void LoadCharacter()
        {
            if (customizer == null)
            {
                return;
            }

            if (!PlayerPrefs.HasKey(SaveKey))
            {
                customizer.InitializeDefaults();
                return;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);

            if (data == null)
            {
                customizer.InitializeDefaults();
                return;
            }

            customizer.ApplySaveData(data);
        }

        public void ResetCharacter()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                PlayerPrefs.DeleteKey(SaveKey);
            }

            ClearWizardComplete();
            PlayerPrefs.Save();

            if (customizer != null)
            {
                customizer.InitializeDefaults();
            }
        }

#if UNITY_EDITOR
        public void SetCustomizer(CharacterCustomizer characterCustomizer) => customizer = characterCustomizer;
#endif
    }
}
