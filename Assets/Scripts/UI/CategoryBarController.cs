using System;
using System.Collections.Generic;
using DressUpGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Bottom navigation bar for HAIR / MAKEUP / DRESSES / ACCESSORIES.
    /// </summary>
    public class CategoryBarController : MonoBehaviour
    {
        [Serializable]
        private struct CategoryButtonBinding
        {
            public CustomizationCategory category;
            public Button button;
            public Image background;
        }

        [SerializeField] private List<CategoryButtonBinding> categoryButtons = new List<CategoryButtonBinding>();

        [Header("Visuals")]
        [SerializeField] private Color normalColor = new Color(0.95f, 0.85f, 0.92f);
        [SerializeField] private Color selectedColor = new Color(0.85f, 0.45f, 0.72f);

        private CustomizationCategory activeCategory = CustomizationCategory.Hair;

        public CustomizationCategory ActiveCategory => activeCategory;
        public event Action<CustomizationCategory> CategoryChanged;

        private void Awake()
        {
            foreach (CategoryButtonBinding binding in categoryButtons)
            {
                if (binding.button == null)
                {
                    continue;
                }

                CustomizationCategory captured = binding.category;
                binding.button.onClick.AddListener(() => SelectCategory(captured));
            }
        }

        public void SelectCategory(CustomizationCategory category)
        {
            activeCategory = category;
            UpdateVisuals();
            CategoryChanged?.Invoke(category);
        }

        private void UpdateVisuals()
        {
            foreach (CategoryButtonBinding binding in categoryButtons)
            {
                if (binding.background == null)
                {
                    continue;
                }

                binding.background.color = binding.category == activeCategory ? selectedColor : normalColor;
            }
        }
    }
}
