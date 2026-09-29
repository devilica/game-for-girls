using System;
using System.Collections;
using System.Collections.Generic;
using DressUpGame.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    public enum PetWizardStep
    {
        Pet = 0,
        Hairbow = 1,
        Collar = 2,
        Glasses = 3
    }

    public class PetWizardController : MonoBehaviour
    {
        [SerializeField] private GameObject wizardPanel;
        [SerializeField] private GameObject wizardNavOverlay;
        [SerializeField] private PetWizardItemsGridController itemsGrid;
        [SerializeField] private Button prevArrowButton;
        [SerializeField] private Button nextArrowButton;
        [SerializeField] private PetCustomizer customizer;
        [SerializeField] private BreakScreenController breakScreen;

        private PetWizardStep currentStep = PetWizardStep.Pet;
        private readonly HashSet<PetWizardStep> completedSteps = new HashSet<PetWizardStep>();
        private bool isTransitioning;
        private bool isEditMode;
        private Coroutine nextStepCoroutine;

        public event Action WizardCompleted;
        public event Action<PetItem> ItemApplied;

        private void Awake()
        {
            if (customizer == null)
            {
                customizer = FindAnyObjectByType<PetCustomizer>();
            }

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

        public void StartWizard()
        {
            isEditMode = false;
            SetWizardVisible(true);
            currentStep = PetWizardStep.Pet;
            completedSteps.Clear();
            isTransitioning = false;
            WireGridEvents();
            ShowStep(currentStep);
        }

        public void StartEditMode(PetWizardStep step = PetWizardStep.Pet)
        {
            isEditMode = true;
            SetWizardVisible(true);
            isTransitioning = false;
            WireGridEvents();
            currentStep = step;
            ShowStep(currentStep);
        }

        public void HideWizard()
        {
            isEditMode = false;
            SetWizardVisible(false);
        }

        private void SetWizardVisible(bool visible)
        {
            if (wizardPanel != null)
            {
                wizardPanel.SetActive(visible);
            }

            ResolveWizardNavOverlay();
            if (wizardNavOverlay != null)
            {
                wizardNavOverlay.SetActive(visible);
            }
        }

        private void ResolveWizardNavOverlay()
        {
            if (wizardNavOverlay != null)
            {
                return;
            }

            if (wizardPanel != null)
            {
                wizardNavOverlay = wizardPanel.transform.parent?.Find("WizardNavOverlay")?.gameObject;
            }

            if (wizardNavOverlay == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                wizardNavOverlay = canvas != null
                    ? canvas.transform.Find("WizardNavOverlay")?.gameObject
                    : null;
            }

            if (prevArrowButton == null && wizardNavOverlay != null)
            {
                prevArrowButton = wizardNavOverlay.transform.Find("PrevArrowButton")?.GetComponent<Button>();
            }

            if (nextArrowButton == null && wizardNavOverlay != null)
            {
                nextArrowButton = wizardNavOverlay.transform.Find("NextArrowButton")?.GetComponent<Button>();
            }
        }

        private void WireGridEvents()
        {
            if (itemsGrid == null)
            {
                return;
            }

            itemsGrid.ItemSelected -= HandleItemSelected;
            itemsGrid.ItemSelected += HandleItemSelected;
        }

        private void HandleItemSelected(PetItem item)
        {
            if (item == null || customizer == null)
            {
                return;
            }

            customizer.SetItemForCategory(item.Category, item);
            ItemApplied?.Invoke(item);
            MarkStepCompleted(currentStep);
        }

        private void MarkStepCompleted(PetWizardStep step)
        {
            completedSteps.Add(step);
            UpdateNavigation();
        }

        private void GoToPreviousStep()
        {
            if (isTransitioning || currentStep == PetWizardStep.Pet)
            {
                return;
            }

            currentStep--;
            ShowStep(currentStep);
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
                if (isEditMode)
                {
                    if (currentStep == PetWizardStep.Glasses)
                    {
                        isEditMode = false;
                        WizardCompleted?.Invoke();
                        yield break;
                    }

                    if (currentStep == PetWizardStep.Collar)
                    {
                        yield return ShowTeaBreakRoutine();
                    }

                    currentStep++;
                    ShowStep(currentStep);
                    yield break;
                }

                if (currentStep == PetWizardStep.Glasses)
                {
                    WizardCompleted?.Invoke();
                    yield break;
                }

                if (currentStep == PetWizardStep.Collar)
                {
                    yield return ShowTeaBreakRoutine();
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

        private IEnumerator ShowTeaBreakRoutine()
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

        private BreakScreenController ResolveBreakScreen()
        {
            if (breakScreen != null)
            {
                return breakScreen;
            }

            breakScreen = FindAnyObjectByType<BreakScreenController>();
            if (breakScreen != null)
            {
                return breakScreen;
            }

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                breakScreen = BreakScreenController.EnsureOnCanvas(canvas.transform);
            }

            return breakScreen;
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

        private void ShowStep(PetWizardStep step)
        {
            if (customizer?.Catalog == null || itemsGrid == null)
            {
                return;
            }

            switch (step)
            {
                case PetWizardStep.Pet:
                    itemsGrid.ShowCategory(customizer.Catalog, PetCustomizationCategory.Pet, customizer.SelectedPet);
                    break;
                case PetWizardStep.Hairbow:
                    itemsGrid.ShowCategory(customizer.Catalog, PetCustomizationCategory.Hairbow, customizer.SelectedHairbow);
                    break;
                case PetWizardStep.Collar:
                    itemsGrid.ShowCategory(customizer.Catalog, PetCustomizationCategory.Collar, customizer.SelectedCollar);
                    break;
                case PetWizardStep.Glasses:
                    itemsGrid.ShowCategory(customizer.Catalog, PetCustomizationCategory.Glasses, customizer.SelectedGlasses);
                    break;
            }

            UpdateNavigation();
        }

        private void UpdateNavigation()
        {
            if (prevArrowButton != null)
            {
                prevArrowButton.gameObject.SetActive(currentStep != PetWizardStep.Pet);
                prevArrowButton.interactable = !isTransitioning;
            }

            if (nextArrowButton != null)
            {
                nextArrowButton.interactable = !isTransitioning;
            }
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
    }
}
