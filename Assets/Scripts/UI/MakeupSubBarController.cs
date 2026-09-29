using System;
using System.Collections.Generic;
using DressUpGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Secondary bar shown when the Makeup category is active.
    /// </summary>
    public class MakeupSubBarController : MonoBehaviour
    {
        [Serializable]
        private struct SubButtonBinding
        {
            public MakeupType type;
            public Button button;
            public Image background;
        }

        [SerializeField] private GameObject root;
        [SerializeField] private List<SubButtonBinding> subButtons = new List<SubButtonBinding>();

        [Header("Visuals")]
        [SerializeField] private Color normalColor = new Color(0.92f, 0.88f, 0.96f);
        [SerializeField] private Color selectedColor = new Color(0.72f, 0.55f, 0.95f);

        private MakeupType activeType = MakeupType.Lipstick;

        public MakeupType ActiveType => activeType;
        public event Action<MakeupType> TypeChanged;

        private void Awake()
        {
            foreach (SubButtonBinding binding in subButtons)
            {
                if (binding.button == null)
                {
                    continue;
                }

                MakeupType captured = binding.type;
                binding.button.onClick.AddListener(() => SelectType(captured));
            }
        }

        public void SetVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        public void SelectType(MakeupType type)
        {
            activeType = type;
            UpdateVisuals();
            TypeChanged?.Invoke(type);
        }

        private void UpdateVisuals()
        {
            foreach (SubButtonBinding binding in subButtons)
            {
                if (binding.background == null)
                {
                    continue;
                }

                binding.background.color = binding.type == activeType ? selectedColor : normalColor;
            }
        }

#if UNITY_EDITOR
        public void SetRoot(GameObject subBarRoot) => root = subBarRoot;
#endif
    }
}
