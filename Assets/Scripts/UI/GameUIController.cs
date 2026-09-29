using DressUpGame.Character;
using DressUpGame.Data;
using DressUpGame.Save;
using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Main UI coordinator. Connects category navigation, item panel, save/reset buttons,
    /// opening wizard, and the CharacterCustomizer.
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterCustomizer customizer;
        [SerializeField] private CharacterSaveManager saveManager;
        [SerializeField] private CategoryBarController categoryBar;
        [SerializeField] private MakeupSubBarController makeupSubBar;
        [SerializeField] private ItemsPanelController itemsPanel;
        [SerializeField] private OpeningWizardController openingWizard;
        [SerializeField] private DoneViewController doneView;

        private void Awake()
        {
            if (saveManager == null)
            {
                saveManager = FindAnyObjectByType<CharacterSaveManager>();
            }

            if (customizer == null && saveManager != null)
            {
                customizer = saveManager.Customizer;
            }

            if (openingWizard == null)
            {
                openingWizard = FindAnyObjectByType<OpeningWizardController>();
            }

            if (doneView == null)
            {
                doneView = FindAnyObjectByType<DoneViewController>();
            }
        }

        private void Start()
        {
            if (saveManager != null)
            {
                saveManager.LoadCharacter();
            }
            else if (customizer != null)
            {
                customizer.InitializeDefaults();
            }

            customizer?.RefreshDisplayScale();

            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            layout?.Apply();

            WireItemsPanelEvents();
            WireWizardEvents();
            WireDoneViewEvents();

            if (saveManager != null && !saveManager.HasCompletedWizard())
            {
                StartOpeningWizard();
            }
            else
            {
                ShowDoneView();
            }
        }

        private void OnDestroy()
        {
            if (categoryBar != null)
            {
                categoryBar.CategoryChanged -= HandleCategoryChanged;
            }

            if (makeupSubBar != null)
            {
                makeupSubBar.TypeChanged -= HandleMakeupTypeChanged;
            }

            if (openingWizard != null)
            {
                openingWizard.WizardCompleted -= HandleWizardCompleted;
            }

            if (doneView != null)
            {
                doneView.EditRequested -= HandleEditLook;
                doneView.StartOverRequested -= HandleStartOver;
            }
        }

        private void WireItemsPanelEvents()
        {
            if (itemsPanel == null)
            {
                return;
            }

            itemsPanel.HairSelected += item => customizer?.SetHair(item);
            itemsPanel.HairColorSelected += preset => customizer?.SetHairColor(preset);
            itemsPanel.DressSelected += item => customizer?.SetDress(item);
            itemsPanel.ShoeSelected += item => customizer?.SetShoe(item);
            itemsPanel.MakeupSelected += HandleMakeupSelected;
            itemsPanel.AccessorySelected += item => customizer?.SetAccessory(item);
        }

        private void WireWizardEvents()
        {
            if (openingWizard == null)
            {
                return;
            }

            openingWizard.WizardCompleted -= HandleWizardCompleted;
            openingWizard.WizardCompleted += HandleWizardCompleted;
        }

        private void WireDoneViewEvents()
        {
            if (doneView == null)
            {
                return;
            }

            doneView.EditRequested -= HandleEditLook;
            doneView.StartOverRequested -= HandleStartOver;
            doneView.EditRequested += HandleEditLook;
            doneView.StartOverRequested += HandleStartOver;
        }

        private void HandleEditLook()
        {
            saveManager?.ClearWizardComplete();
            customizer?.RefreshDisplayScale();
            StartOpeningWizard();
        }

        private void HandleStartOver()
        {
            saveManager?.ResetCharacter();
            customizer?.RefreshDisplayScale();
            StartOpeningWizard();
        }

        private void StartOpeningWizard()
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
            openingWizard?.StartWizard();
        }

        private void ShowDoneView()
        {
            openingWizard?.HideWizard();
            doneView?.Show();
        }

        private void HandleWizardCompleted()
        {
            saveManager?.SaveCharacter();
            saveManager?.MarkWizardComplete();
            ShowDoneView();
        }

        private void HandleCategoryChanged(CustomizationCategory category)
        {
            if (customizer == null || itemsPanel == null)
            {
                return;
            }

            bool isMakeup = category == CustomizationCategory.Makeup;
            makeupSubBar?.SetVisible(isMakeup);

            switch (category)
            {
                case CustomizationCategory.Hair:
                    itemsPanel.ShowHairItems(customizer.Catalog, customizer.SelectedHair, customizer.SelectedHairColor);
                    break;
                case CustomizationCategory.Makeup:
                    makeupSubBar?.SelectType(MakeupType.Lipstick);
                    ShowActiveMakeupItems();
                    break;
                case CustomizationCategory.Dresses:
                    itemsPanel.ShowDressItems(customizer.Catalog, customizer.SelectedDress);
                    break;
                case CustomizationCategory.Shoes:
                    itemsPanel.ShowShoeItems(customizer.Catalog, customizer.SelectedShoe);
                    break;
                case CustomizationCategory.Accessories:
                    itemsPanel.ShowAccessoryItems(
                        customizer.Catalog,
                        customizer.SelectedNecklace,
                        customizer.SelectedEarrings,
                        customizer.SelectedCrown,
                        customizer.SelectedGlasses,
                        customizer.SelectedBag);
                    break;
            }
        }

        private void HandleMakeupTypeChanged(MakeupType type)
        {
            ShowActiveMakeupItems();
        }

        private void ShowActiveMakeupItems()
        {
            if (customizer == null || itemsPanel == null || makeupSubBar == null)
            {
                return;
            }

            MakeupType type = makeupSubBar.ActiveType;
            MakeupItem selected = type switch
            {
                MakeupType.Lipstick => customizer.SelectedLipstick,
                MakeupType.Eyes => customizer.SelectedEyes,
                MakeupType.Eyeshadow => customizer.SelectedEyeshadow,
                MakeupType.Blush => customizer.SelectedBlush,
                _ => null
            };

            itemsPanel.ShowMakeupItems(customizer.Catalog.GetMakeupItems(type), selected);
        }

        private void HandleMakeupSelected(MakeupItem item)
        {
            if (customizer == null || makeupSubBar == null || item == null)
            {
                return;
            }

            switch (makeupSubBar.ActiveType)
            {
                case MakeupType.Lipstick:
                    customizer.SetLipstick(item);
                    break;
                case MakeupType.Eyes:
                    customizer.SetEyes(item);
                    break;
                case MakeupType.Eyeshadow:
                    customizer.SetEyeshadow(item);
                    break;
                case MakeupType.Blush:
                    customizer.SetBlush(item);
                    break;
            }
        }

#if UNITY_EDITOR
        public void AssignReferences(
            CharacterCustomizer characterCustomizer,
            CharacterSaveManager characterSaveManager,
            CategoryBarController categoryBarController,
            MakeupSubBarController makeupSubBarController,
            ItemsPanelController itemsPanelController,
            OpeningWizardController wizardController,
            DoneViewController doneViewController)
        {
            customizer = characterCustomizer;
            saveManager = characterSaveManager;
            categoryBar = categoryBarController;
            makeupSubBar = makeupSubBarController;
            itemsPanel = itemsPanelController;
            openingWizard = wizardController;
            doneView = doneViewController;
        }
#endif
    }
}
