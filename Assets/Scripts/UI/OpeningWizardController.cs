using System;
using System.Collections;
using System.Collections.Generic;
using DressUpGame.Character;
using DressUpGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    public enum WizardStep
    {
        Dress = 0,
        Hair = 1,
        HairColor = 2,
        Eyes = 3,
        Eyeshadow = 4,
        Blush = 5,
        Lips = 6,
        Necklace = 7,
        Earrings = 8,
        Crown = 9,
        Glasses = 10,
        Bag = 11,
        Shoes = 12
    }

    /// <summary>
    /// Guides first-time players through Dress → Hair → … → Necklace → Earrings → … → Shoes.
    /// </summary>
    public class OpeningWizardController : MonoBehaviour
    {
        [SerializeField] private GameObject wizardPanel;
        [SerializeField] private GameObject wizardNavOverlay;
        [SerializeField] private WizardItemsGridController itemsGrid;
        [SerializeField] private Button prevArrowButton;
        [SerializeField] private Button nextArrowButton;
        [SerializeField] private Text stepTitleLabel;
        [SerializeField] private CharacterCustomizer customizer;
        [SerializeField] private BreakScreenController breakScreen;

        private const int BreakEveryNextSteps = 4;

        private WizardStep currentStep = WizardStep.Dress;
        private readonly HashSet<WizardStep> completedSteps = new HashSet<WizardStep>();
        private int nextAdvanceCount;
        private bool isTransitioning;
        private Coroutine nextStepCoroutine;

        public event Action WizardCompleted;

        private void Awake()
        {
            if (customizer == null)
            {
                customizer = FindAnyObjectByType<CharacterCustomizer>();
            }

            ResolveWizardNavOverlay();

            if (prevArrowButton != null)
            {
                prevArrowButton.onClick.AddListener(GoToPreviousStep);
            }

            if (nextArrowButton != null)
            {
                nextArrowButton.onClick.AddListener(GoToNextStep);
            }
        }

        private void OnDestroy()
        {
            if (prevArrowButton != null)
            {
                prevArrowButton.onClick.RemoveListener(GoToPreviousStep);
            }

            if (nextArrowButton != null)
            {
                nextArrowButton.onClick.RemoveListener(GoToNextStep);
            }
        }

        public void StartWizard()
        {
            ResolveWizardNavOverlay();

            if (wizardPanel != null)
            {
                wizardPanel.SetActive(true);
            }

            SetWizardNavVisible(true);

            currentStep = WizardStep.Dress;
            completedSteps.Clear();
            nextAdvanceCount = 0;
            isTransitioning = false;

            WireGridEvents();
            HideStepTitle();
            ShowStep(currentStep);
        }

        public void HideWizard()
        {
            if (wizardPanel != null)
            {
                wizardPanel.SetActive(false);
            }

            SetWizardNavVisible(false);
        }

        private void WireGridEvents()
        {
            if (itemsGrid == null)
            {
                return;
            }

            itemsGrid.DressSelected -= HandleDressSelected;
            itemsGrid.HairSelected -= HandleHairSelected;
            itemsGrid.HairColorSelected -= HandleHairColorSelected;
            itemsGrid.EyesSelected -= HandleEyesSelected;
            itemsGrid.EyeshadowSelected -= HandleEyeshadowSelected;
            itemsGrid.BlushSelected -= HandleBlushSelected;
            itemsGrid.LipstickSelected -= HandleLipstickSelected;
            itemsGrid.NecklaceSelected -= HandleNecklaceSelected;
            itemsGrid.EarringsSelected -= HandleEarringsSelected;
            itemsGrid.CrownSelected -= HandleCrownSelected;
            itemsGrid.GlassesSelected -= HandleGlassesSelected;
            itemsGrid.BagSelected -= HandleBagSelected;
            itemsGrid.ShoesSelected -= HandleShoesSelected;

            itemsGrid.DressSelected += HandleDressSelected;
            itemsGrid.HairSelected += HandleHairSelected;
            itemsGrid.HairColorSelected += HandleHairColorSelected;
            itemsGrid.EyesSelected += HandleEyesSelected;
            itemsGrid.EyeshadowSelected += HandleEyeshadowSelected;
            itemsGrid.BlushSelected += HandleBlushSelected;
            itemsGrid.LipstickSelected += HandleLipstickSelected;
            itemsGrid.NecklaceSelected += HandleNecklaceSelected;
            itemsGrid.EarringsSelected += HandleEarringsSelected;
            itemsGrid.CrownSelected += HandleCrownSelected;
            itemsGrid.GlassesSelected += HandleGlassesSelected;
            itemsGrid.BagSelected += HandleBagSelected;
            itemsGrid.ShoesSelected += HandleShoesSelected;
        }

        private void HandleDressSelected(DressItem item)
        {
            customizer?.SetDress(item);
            MarkStepCompleted(WizardStep.Dress);
        }

        private void HandleHairSelected(HairItem item)
        {
            customizer?.SetHair(item);
            MarkStepCompleted(WizardStep.Hair);
        }

        private void HandleHairColorSelected(HairColorPreset preset)
        {
            customizer?.SetHairColor(preset);
            MarkStepCompleted(WizardStep.HairColor);
        }

        private void HandleEyesSelected(MakeupItem item)
        {
            customizer?.SetEyes(item);
            MarkStepCompleted(WizardStep.Eyes);
        }

        private void HandleEyeshadowSelected(MakeupItem item)
        {
            customizer?.SetEyeshadow(item);
            MarkStepCompleted(WizardStep.Eyeshadow);
        }

        private void HandleBlushSelected(MakeupItem item)
        {
            customizer?.SetBlush(item);
            MarkStepCompleted(WizardStep.Blush);
        }

        private void HandleLipstickSelected(MakeupItem item)
        {
            customizer?.SetLipstick(item);
            MarkStepCompleted(WizardStep.Lips);
        }

        private void HandleNecklaceSelected(AccessoryItem item)
        {
            customizer?.SetNecklace(item);
            MarkStepCompleted(WizardStep.Necklace);
        }

        private void HandleEarringsSelected(AccessoryItem item)
        {
            customizer?.SetEarrings(item);
            MarkStepCompleted(WizardStep.Earrings);
        }

        private void HandleCrownSelected(AccessoryItem item)
        {
            customizer?.SetCrown(item);
            MarkStepCompleted(WizardStep.Crown);
        }

        private void HandleGlassesSelected(AccessoryItem item)
        {
            customizer?.SetGlasses(item);
            MarkStepCompleted(WizardStep.Glasses);
        }

        private void HandleBagSelected(AccessoryItem item)
        {
            customizer?.SetBag(item);
            MarkStepCompleted(WizardStep.Bag);
        }

        private void HandleShoesSelected(ShoeItem item)
        {
            customizer?.SetShoe(item);
            MarkStepCompleted(WizardStep.Shoes);
        }

        private void MarkStepCompleted(WizardStep step)
        {
            completedSteps.Add(step);
            UpdateNavigation();
        }

        private void GoToPreviousStep()
        {
            if (isTransitioning || currentStep == WizardStep.Dress)
            {
                return;
            }

            currentStep--;
            ShowStep(currentStep);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || !Application.isPlaying)
            {
                return;
            }

            RecoverFromStuckAdPause();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused || !Application.isPlaying)
            {
                return;
            }

            RecoverFromStuckAdPause();
        }

        private void RecoverFromStuckAdPause()
        {
            MainThreadDispatcher.Run(RecoverFromStuckAdPauseOnMainThread);
        }

        private void RecoverFromStuckAdPauseOnMainThread()
        {
            bool wasPaused = Time.timeScale == 0f;
            GameplayPauseGuard.EnsureUnpaused();

            if (!wasPaused || !isTransitioning)
            {
                return;
            }

            if (nextStepCoroutine != null)
            {
                StopCoroutine(nextStepCoroutine);
                nextStepCoroutine = null;
            }

            isTransitioning = false;
            SetNavigationInteractable(true);
            UpdateNavigation();
        }

        private void GoToNextStep()
        {
            if (isTransitioning)
            {
                return;
            }

            if (nextStepCoroutine != null)
            {
                StopCoroutine(nextStepCoroutine);
            }

            nextStepCoroutine = StartCoroutine(GoToNextStepRoutine());
        }

        private IEnumerator GoToNextStepRoutine()
        {
            isTransitioning = true;
            SetNavigationInteractable(false);

            try
            {
                nextAdvanceCount++;
                if (nextAdvanceCount % BreakEveryNextSteps == 0)
                {
                    BreakScreenController screen = ResolveBreakScreen();
                    if (screen != null)
                    {
                        yield return screen.ShowRoutine(BreakScreenController.DefaultDurationSeconds);
                    }
                    else
                    {
                        yield return new WaitForSecondsRealtime(BreakScreenController.DefaultDurationSeconds);
                    }

                    yield return BreakInterstitialAdController.WaitForBreakAdRoutine();
                }

                if (currentStep == WizardStep.Shoes)
                {
                    WizardCompleted?.Invoke();
                    yield break;
                }

                currentStep++;
                ShowStep(currentStep);
            }
            finally
            {
                nextStepCoroutine = null;
                isTransitioning = false;
                GameplayPauseGuard.EnsureUnpaused();
                SetNavigationInteractable(true);
                UpdateNavigation();
            }
        }

        private void ShowStep(WizardStep step)
        {
            currentStep = step;

            if (customizer?.Catalog == null || itemsGrid == null)
            {
                UpdateNavigation();
                return;
            }

            switch (step)
            {
                case WizardStep.Dress:
                    itemsGrid.ShowDressItems(customizer.Catalog, customizer.SelectedDress);
                    break;
                case WizardStep.Hair:
                    itemsGrid.ShowHairItems(customizer.Catalog, customizer.SelectedHair);
                    break;
                case WizardStep.HairColor:
                    itemsGrid.ShowHairColorItems(
                        customizer.Catalog,
                        customizer.SelectedHair,
                        customizer.SelectedHairColor);
                    break;
                case WizardStep.Eyes:
                    itemsGrid.ShowEyeItems(customizer.Catalog, customizer.SelectedEyes);
                    break;
                case WizardStep.Eyeshadow:
                    itemsGrid.ShowEyeshadowItems(customizer.Catalog, customizer.SelectedEyeshadow);
                    break;
                case WizardStep.Blush:
                    itemsGrid.ShowBlushItems(customizer.Catalog, customizer.SelectedBlush);
                    break;
                case WizardStep.Lips:
                    itemsGrid.ShowLipstickItems(customizer.Catalog, customizer.SelectedLipstick);
                    break;
                case WizardStep.Necklace:
                    itemsGrid.ShowNecklaceItems(customizer.Catalog, customizer.SelectedNecklace);
                    break;
                case WizardStep.Earrings:
                    itemsGrid.ShowEarringItems(customizer.Catalog, customizer.SelectedEarrings);
                    break;
                case WizardStep.Crown:
                    itemsGrid.ShowCrownItems(customizer.Catalog, customizer.SelectedCrown);
                    break;
                case WizardStep.Glasses:
                    itemsGrid.ShowGlassesItems(customizer.Catalog, customizer.SelectedGlasses);
                    break;
                case WizardStep.Bag:
                    itemsGrid.ShowBagItems(customizer.Catalog, customizer.SelectedBag);
                    break;
                case WizardStep.Shoes:
                    itemsGrid.ShowShoeItems(customizer.Catalog, customizer.SelectedShoe);
                    break;
            }

            UpdateNavigation();
        }

        private void HideStepTitle()
        {
            if (stepTitleLabel != null)
            {
                stepTitleLabel.gameObject.SetActive(false);
                return;
            }

            if (wizardPanel != null)
            {
                Transform title = wizardPanel.transform.Find("WizardStepTitle");
                if (title != null)
                {
                    title.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateNavigation()
        {
            ResolveWizardNavOverlay();

            if (prevArrowButton != null)
            {
                prevArrowButton.gameObject.SetActive(currentStep != WizardStep.Dress);
                prevArrowButton.interactable = !isTransitioning;
            }

            if (nextArrowButton != null)
            {
                nextArrowButton.interactable = !isTransitioning;
            }
        }

        private void ResolveWizardNavOverlay()
        {
            Transform canvas = wizardPanel != null ? wizardPanel.transform.parent : null;
            if (wizardNavOverlay == null && canvas != null)
            {
                wizardNavOverlay = canvas.Find("WizardNavOverlay")?.gameObject;
            }

            if (prevArrowButton == null)
            {
                prevArrowButton = wizardNavOverlay != null
                    ? wizardNavOverlay.transform.Find("PrevArrowButton")?.GetComponent<Button>()
                    : null;
            }

            if (nextArrowButton == null)
            {
                nextArrowButton = wizardNavOverlay != null
                    ? wizardNavOverlay.transform.Find("NextArrowButton")?.GetComponent<Button>()
                    : null;
            }
        }

        private void SetWizardNavVisible(bool visible)
        {
            ResolveWizardNavOverlay();

            if (wizardNavOverlay != null)
            {
                wizardNavOverlay.SetActive(visible);
            }

            if (visible)
            {
                UpdateNavigation();
            }
        }

        private BreakScreenController ResolveBreakScreen()
        {
            if (breakScreen == null)
            {
                breakScreen = FindAnyObjectByType<BreakScreenController>();
            }

            if (breakScreen == null)
            {
                Transform canvas = wizardPanel != null ? wizardPanel.transform.parent : null;
                if (canvas != null)
                {
                    breakScreen = BreakScreenController.EnsureOnCanvas(canvas);
                }
            }

            return breakScreen;
        }

        private void SetNavigationInteractable(bool interactable)
        {
            if (prevArrowButton != null)
            {
                prevArrowButton.interactable = interactable;
            }

            if (nextArrowButton != null)
            {
                nextArrowButton.interactable = interactable;
            }
        }

#if UNITY_EDITOR
        public void AssignReferences(
            GameObject panel,
            GameObject navOverlay,
            WizardItemsGridController grid,
            Button prevButton,
            Button nextButton,
            Text titleLabel,
            CharacterCustomizer characterCustomizer)
        {
            wizardPanel = panel;
            wizardNavOverlay = navOverlay;
            itemsGrid = grid;
            prevArrowButton = prevButton;
            nextArrowButton = nextButton;
            stepTitleLabel = titleLabel;
            customizer = characterCustomizer;
        }
#endif
    }
}
