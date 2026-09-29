using UnityEngine;

namespace DressUpGame.Pet
{
    public class PetSaveManager : MonoBehaviour
    {
        private const string SaveKey = "DressUpGame_PetSave";
        private const string WizardCompleteKey = "DressUpGame_PetWizardComplete";

        [SerializeField] private PetCustomizer customizer;

        public PetCustomizer Customizer => customizer;

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

        public static void ClearAllPetProgress()
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
                customizer = FindAnyObjectByType<PetCustomizer>();
            }
        }

        public void SavePet()
        {
            if (customizer == null)
            {
                return;
            }

            PetSaveData data = customizer.CaptureSaveData();
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void LoadPet()
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

            string json = PlayerPrefs.GetString(SaveKey);
            PetSaveData data = JsonUtility.FromJson<PetSaveData>(json);
            customizer.ApplySaveData(data);
        }

        public void ResetPet()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                PlayerPrefs.DeleteKey(SaveKey);
            }

            ClearWizardComplete();
            PlayerPrefs.Save();
            customizer?.InitializeDefaults();
        }
    }
}
