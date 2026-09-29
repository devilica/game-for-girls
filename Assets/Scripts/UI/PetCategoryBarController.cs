using System;
using System.Collections.Generic;
using DressUpGame.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    public class PetCategoryBarController : MonoBehaviour
    {
        [Serializable]
        private struct CategoryButtonBinding
        {
            public PetCustomizationCategory category;
            public Button button;
            public Image background;
        }

        [SerializeField] private List<CategoryButtonBinding> categoryButtons = new List<CategoryButtonBinding>();
        [SerializeField] private Color normalColor = new Color(0.95f, 0.85f, 0.92f);
        [SerializeField] private Color selectedColor = new Color(0.85f, 0.45f, 0.72f);

        private PetCustomizationCategory activeCategory = PetCustomizationCategory.Pet;

        public PetCustomizationCategory ActiveCategory => activeCategory;
        public event Action<PetCustomizationCategory> CategoryChanged;

        private void Awake()
        {
            foreach (CategoryButtonBinding binding in categoryButtons)
            {
                if (binding.button == null)
                {
                    continue;
                }

                PetCustomizationCategory captured = binding.category;
                binding.button.onClick.AddListener(() => SelectCategory(captured));
            }
        }

        public void SelectCategory(PetCustomizationCategory category)
        {
            activeCategory = category;
            UpdateVisuals();
            CategoryChanged?.Invoke(category);
        }

        private void UpdateVisuals()
        {
            foreach (CategoryButtonBinding binding in categoryButtons)
            {
                if (binding.background != null)
                {
                    binding.background.color = binding.category == activeCategory ? selectedColor : normalColor;
                }
            }
        }
    }
}
