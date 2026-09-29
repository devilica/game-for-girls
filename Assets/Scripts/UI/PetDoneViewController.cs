using System;
using DressUpGame.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    public class PetDoneViewController : MonoBehaviour
    {
        [SerializeField] private GameObject donePanel;
        [SerializeField] private GameObject categoryBar;
        [SerializeField] private GameObject itemsPanel;
        [SerializeField] private GameObject wizardPanel;
        [SerializeField] private Button editLookButton;
        [SerializeField] private Button startOverButton;

        public event Action EditRequested;
        public event Action StartOverRequested;

        private void Awake()
        {
            TryFindButtonsInPanel();
            EnsureTransparentPanel();
            EnsureDonePanelButtonStyles();

            if (editLookButton != null)
            {
                editLookButton.onClick.AddListener(() => EditRequested?.Invoke());
            }

            if (startOverButton != null)
            {
                startOverButton.onClick.AddListener(() => StartOverRequested?.Invoke());
            }
        }

        private void TryFindButtonsInPanel()
        {
            if (donePanel == null)
            {
                return;
            }

            editLookButton ??= donePanel.transform.Find("DoneButtonRow/EditLookButton")?.GetComponent<Button>();
            startOverButton ??= donePanel.transform.Find("DoneButtonRow/StartOverButton")?.GetComponent<Button>();
        }

        private void EnsureTransparentPanel()
        {
            if (donePanel == null)
            {
                return;
            }

            Image panelBackground = donePanel.GetComponent<Image>();
            if (panelBackground != null)
            {
                panelBackground.color = Color.clear;
                panelBackground.raycastTarget = false;
            }
        }

        private void EnsureDonePanelButtonStyles()
        {
            EnsureDonePanelButton(editLookButton, DonePanelButton.ButtonKind.EditLook);
            EnsureDonePanelButton(startOverButton, DonePanelButton.ButtonKind.StartOver);
        }

        private static void EnsureDonePanelButton(Button button, DonePanelButton.ButtonKind kind)
        {
            if (button == null)
            {
                return;
            }

            DonePanelButton styledButton = button.GetComponent<DonePanelButton>();
            if (styledButton == null)
            {
                styledButton = button.gameObject.AddComponent<DonePanelButton>();
            }

            styledButton.Configure(kind);
        }

        public void Show()
        {
            SetActiveSafe(donePanel, true);
            SetActiveSafe(categoryBar, false);
            SetActiveSafe(itemsPanel, false);
            SetActiveSafe(wizardPanel, false);

            PetCustomizer customizer = FindAnyObjectByType<PetCustomizer>();
            customizer?.RefreshDisplayScaleForDoneView();
            EnsureDonePanelButtonStyles();

            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            layout?.ApplyForDoneView();
        }

        public void HideForWizard()
        {
            SetActiveSafe(donePanel, false);
            SetActiveSafe(categoryBar, false);
            SetActiveSafe(itemsPanel, false);
            SetActiveSafe(wizardPanel, true);

            PetCustomizer customizer = FindAnyObjectByType<PetCustomizer>();
            customizer?.RefreshDisplayScale();
        }

        public void ShowEditMode()
        {
            SetActiveSafe(donePanel, false);
            SetActiveSafe(categoryBar, false);
            SetActiveSafe(itemsPanel, false);
            SetActiveSafe(wizardPanel, true);

            PetCustomizer customizer = FindAnyObjectByType<PetCustomizer>();
            customizer?.RefreshDisplayScale();
        }

        private static void SetActiveSafe(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
