using System;
using DressUpGame.Character;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Shows the finished character after the opening wizard completes.
    /// Hides all editing UI and offers Edit / Start Over actions.
    /// </summary>
    public class DoneViewController : MonoBehaviour
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

        private void OnDestroy()
        {
            if (editLookButton != null)
            {
                editLookButton.onClick.RemoveAllListeners();
            }

            if (startOverButton != null)
            {
                startOverButton.onClick.RemoveAllListeners();
            }
        }

        public void Show()
        {
            SetActiveSafe(donePanel, true);
            SetActiveSafe(categoryBar, false);
            SetActiveSafe(itemsPanel, false);
            SetActiveSafe(wizardPanel, false);

            CharacterCustomizer customizer = FindAnyObjectByType<CharacterCustomizer>();
            customizer?.RefreshDisplayScaleForDoneView();
            EnsureDonePanelButtonStyles();
        }

        public void HideForWizard()
        {
            SetActiveSafe(donePanel, false);
            SetActiveSafe(categoryBar, false);
            SetActiveSafe(itemsPanel, false);
            SetActiveSafe(wizardPanel, true);

            CharacterCustomizer customizer = FindAnyObjectByType<CharacterCustomizer>();
            customizer?.RefreshDisplayScale();
        }

        private void TryFindButtonsInPanel()
        {
            if (donePanel == null)
            {
                return;
            }

            if (editLookButton == null)
            {
                editLookButton = donePanel.transform.Find("DoneButtonRow/EditLookButton")?.GetComponent<Button>();
            }

            if (startOverButton == null)
            {
                startOverButton = donePanel.transform.Find("DoneButtonRow/StartOverButton")?.GetComponent<Button>();
            }
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

        private static void SetActiveSafe(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

#if UNITY_EDITOR
        public void AssignReferences(
            GameObject done,
            GameObject category,
            GameObject items,
            GameObject wizard,
            Button editButton,
            Button startOverBtn)
        {
            donePanel = done;
            categoryBar = category;
            itemsPanel = items;
            wizardPanel = wizard;
            editLookButton = editButton;
            startOverButton = startOverBtn;
        }
#endif
    }
}
