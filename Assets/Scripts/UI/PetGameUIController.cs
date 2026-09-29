using DressUpGame.Pet;
using UnityEngine;

namespace DressUpGame.UI
{
    public class PetGameUIController : MonoBehaviour
    {
        [SerializeField] private PetCustomizer customizer;
        [SerializeField] private PetSaveManager saveManager;
        [SerializeField] private PetCategoryBarController categoryBar;
        [SerializeField] private PetItemsPanelController itemsPanel;
        [SerializeField] private PetWizardController wizard;
        [SerializeField] private PetDoneViewController doneView;

        private void Awake()
        {
            if (saveManager == null)
            {
                saveManager = FindAnyObjectByType<PetSaveManager>();
            }

            if (customizer == null && saveManager != null)
            {
                customizer = saveManager.Customizer;
            }

            if (wizard == null)
            {
                wizard = FindAnyObjectByType<PetWizardController>();
            }

            if (doneView == null)
            {
                doneView = FindAnyObjectByType<PetDoneViewController>();
            }
        }

        private void Start()
        {
            MobileAdsInitializer.EnsureInitialized();
            RewardedAdController.EnsureExists();
            BottomBannerAdController.EnsureExists();

            saveManager?.LoadPet();
            customizer?.RefreshDisplayScale();

            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            layout?.Apply();

            Canvas canvas = FindAnyObjectByType<Canvas>();
            HomeMenuButton.EnsureOnCanvas(canvas != null ? canvas.transform : null)?.RefreshVisibility();

            WireEvents();

            if (saveManager != null && !saveManager.HasCompletedWizard())
            {
                StartWizard();
            }
            else
            {
                ShowDoneView();
            }
        }

        private void OnDestroy()
        {
            if (wizard != null)
            {
                wizard.WizardCompleted -= HandleWizardCompleted;
                wizard.ItemApplied -= HandleItemSelected;
            }

            if (doneView != null)
            {
                doneView.EditRequested -= HandleEditLook;
                doneView.StartOverRequested -= HandleStartOver;
            }

            if (categoryBar != null)
            {
                categoryBar.CategoryChanged -= HandleCategoryChanged;
            }

            if (itemsPanel != null)
            {
                itemsPanel.ItemSelected -= HandleItemSelected;
            }
        }

        private void WireEvents()
        {
            if (wizard != null)
            {
                wizard.WizardCompleted -= HandleWizardCompleted;
                wizard.WizardCompleted += HandleWizardCompleted;
                wizard.ItemApplied -= HandleItemSelected;
                wizard.ItemApplied += HandleItemSelected;
            }

            if (doneView != null)
            {
                doneView.EditRequested -= HandleEditLook;
                doneView.StartOverRequested -= HandleStartOver;
                doneView.EditRequested += HandleEditLook;
                doneView.StartOverRequested += HandleStartOver;
            }

            if (categoryBar != null)
            {
                categoryBar.CategoryChanged -= HandleCategoryChanged;
                categoryBar.CategoryChanged += HandleCategoryChanged;
            }

            if (itemsPanel != null)
            {
                itemsPanel.ItemSelected -= HandleItemSelected;
                itemsPanel.ItemSelected += HandleItemSelected;
            }
        }

        private void StartWizard()
        {
            if (categoryBar != null)
            {
                categoryBar.gameObject.SetActive(false);
            }

            if (itemsPanel != null)
            {
                itemsPanel.gameObject.SetActive(false);
            }

            doneView?.HideForWizard();
            wizard?.StartWizard();
        }

        private void ShowDoneView()
        {
            wizard?.HideWizard();
            doneView?.Show();
        }

        private void HandleWizardCompleted()
        {
            saveManager?.SavePet();
            saveManager?.MarkWizardComplete();
            ShowDoneView();
        }

        private void HandleEditLook()
        {
            doneView?.ShowEditMode();
            wizard?.StartEditMode(PetWizardStep.Pet);

            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            layout?.Apply();

            Canvas canvas = FindAnyObjectByType<Canvas>();
            HomeMenuButton.EnsureOnCanvas(canvas != null ? canvas.transform : null)?.RefreshVisibility();
        }

        private void HandleStartOver()
        {
            saveManager?.ResetPet();
            customizer?.RefreshDisplayScale();

            if (categoryBar != null)
            {
                categoryBar.gameObject.SetActive(false);
            }

            if (itemsPanel != null)
            {
                itemsPanel.gameObject.SetActive(false);
            }

            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            layout?.Apply();

            Canvas canvas = FindAnyObjectByType<Canvas>();
            HomeMenuButton.EnsureOnCanvas(canvas != null ? canvas.transform : null)?.RefreshVisibility();

            StartWizard();
        }

        private void HandleCategoryChanged(PetCustomizationCategory category)
        {
            ShowActiveCategoryItems();
        }

        private void HandleItemSelected(PetItem item)
        {
            if (item == null)
            {
                return;
            }

            customizer?.SetItemForCategory(item.Category, item);
            saveManager?.SavePet();
        }

        private void ShowActiveCategoryItems()
        {
            if (itemsPanel == null || customizer?.Catalog == null || categoryBar == null)
            {
                return;
            }

            PetCustomizationCategory category = categoryBar.ActiveCategory;
            PetItem selected = category switch
            {
                PetCustomizationCategory.Pet => customizer.SelectedPet,
                PetCustomizationCategory.Hairbow => customizer.SelectedHairbow,
                PetCustomizationCategory.Collar => customizer.SelectedCollar,
                _ => customizer.SelectedGlasses
            };

            itemsPanel.ShowCategory(customizer.Catalog, category, selected);
        }
    }
}
