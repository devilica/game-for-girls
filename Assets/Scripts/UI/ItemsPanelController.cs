using System;
using System.Collections.Generic;
using DressUpGame.Data;
using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Builds and manages the horizontal scrollable item buttons for the active category.
    /// </summary>
    public class ItemsPanelController : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private ItemSlotButton itemButtonPrefab;
        [SerializeField] private GameObject hairColorRow;
        [SerializeField] private Transform hairColorContentRoot;
        [SerializeField] private ItemSlotButton hairColorButtonPrefab;

        private readonly List<ItemSlotButton> spawnedButtons = new List<ItemSlotButton>();
        private readonly List<ItemSlotButton> spawnedColorButtons = new List<ItemSlotButton>();
        private int nextCardIndex;
        private int nextColorCardIndex;
        private HairColorPreset activeHairColor;

        public event Action<HairItem> HairSelected;
        public event Action<HairColorPreset> HairColorSelected;
        public event Action<DressItem> DressSelected;
        public event Action<ShoeItem> ShoeSelected;
        public event Action<MakeupItem> MakeupSelected;
        public event Action<AccessoryItem> AccessorySelected;

        public void ShowHairItems(GameCatalog catalog, HairItem selectedHair, HairColorPreset selectedColor)
        {
            ClearButtons();
            activeHairColor = selectedColor;

            if (catalog == null)
            {
                return;
            }

            foreach (HairItem item in catalog.HairItems)
            {
                if (item == null || item.Sprite == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateItemButton(item.Id, item.DisplayName, item.Sprite);
                button.SetSelected(selectedHair != null && selectedHair.Id == item.Id);
                WireCardClick(button, () =>
                {
                    HairSelected?.Invoke(item);
                    RefreshSelection(button);
                    ShowHairColorRow(catalog, item, activeHairColor);
                });
            }

            ShowHairColorRow(catalog, selectedHair, selectedColor);
        }

        public void ShowMakeupItems(IEnumerable<MakeupItem> items, MakeupItem selectedItem)
        {
            ClearButtons();
            HideHairColorRow();

            foreach (MakeupItem item in items)
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateItemButton(item.Id, item.DisplayName, item.Sprite, item.TintColor);
                button.SetSelected(selectedItem != null && selectedItem.Id == item.Id);
                WireCardClick(button, () =>
                {
                    MakeupSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }
        }

        public void ShowDressItems(GameCatalog catalog, DressItem selectedDress)
        {
            ClearButtons();
            HideHairColorRow();

            if (catalog == null)
            {
                return;
            }

            foreach (DressItem item in catalog.DressItems)
            {
                if (item == null || item.Sprite == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateItemButton(item.Id, item.DisplayName, item.Sprite, item.TintColor);
                button.SetSelected(selectedDress != null && selectedDress.Id == item.Id);
                WireCardClick(button, () =>
                {
                    DressSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }
        }

        public void ShowShoeItems(GameCatalog catalog, ShoeItem selectedShoe)
        {
            ClearButtons();
            HideHairColorRow();

            if (catalog == null)
            {
                return;
            }

            foreach (ShoeItem item in catalog.GetShoesItems())
            {
                if (item == null)
                {
                    continue;
                }

                bool isNoneOption = item.IsShoesNone || item.IsNoneOption;
                if (!isNoneOption && item.Sprite == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateItemButton(
                    item.Id,
                    item.DisplayName,
                    item.Sprite,
                    item.TintColor,
                    isNoneOption: isNoneOption);
                button.SetSelected(selectedShoe != null && selectedShoe.Id == item.Id);
                WireCardClick(button, () =>
                {
                    ShoeSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }
        }

        public void ShowAccessoryItems(
            GameCatalog catalog,
            AccessoryItem selectedNecklace,
            AccessoryItem selectedEarrings,
            AccessoryItem selectedCrown,
            AccessoryItem selectedGlasses,
            AccessoryItem selectedBag)
        {
            ClearButtons();
            HideHairColorRow();

            if (catalog == null)
            {
                return;
            }

            foreach (AccessoryItem item in catalog.AccessoryItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (!item.IsNecklaceNone && !item.IsNecklaceVariant
                    && !item.IsEarringNone && !item.IsEarringVariant
                    && !item.IsCrownNone && !item.IsCrownVariant
                    && !item.IsGlassesNone && !item.IsGlassesVariant
                    && !item.IsBagNone && !item.IsBagVariant)
                {
                    continue;
                }

                bool isSelected = (selectedNecklace != null && selectedNecklace.Id == item.Id)
                    || (selectedEarrings != null && selectedEarrings.Id == item.Id)
                    || (selectedCrown != null && selectedCrown.Id == item.Id)
                    || (selectedGlasses != null && selectedGlasses.Id == item.Id)
                    || (selectedBag != null && selectedBag.Id == item.Id);
                bool isNoneOption = item.IsNecklaceNone || item.IsEarringNone || item.IsCrownNone || item.IsGlassesNone || item.IsBagNone || item.IsNoneOption;
                ItemSlotButton button = CreateItemButton(
                    item.Id,
                    item.DisplayName,
                    item.Sprite,
                    item.TintColor,
                    isNoneOption: isNoneOption);
                button.SetSelected(isSelected);
                WireCardClick(button, () =>
                {
                    AccessorySelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }
        }

        private ItemSlotButton CreateItemButton(
            string id,
            string displayName,
            Sprite sprite,
            Color? tint = null,
            bool isNoneOption = false)
        {
            ItemSlotButton button = Instantiate(itemButtonPrefab, contentRoot);
            if (isNoneOption)
            {
                button.SetupNone(id);
            }
            else
            {
                button.Setup(id, displayName, sprite, tint);
            }

            button.SetCardIndex(nextCardIndex++);
            spawnedButtons.Add(button);
            return button;
        }

        private void ShowHairColorRow(GameCatalog catalog, HairItem selectedHair, HairColorPreset selectedColor)
        {
            if (hairColorRow != null)
            {
                hairColorRow.SetActive(true);
            }

            ClearColorButtons();

            if (catalog?.HairColorLibrary == null || hairColorContentRoot == null || hairColorButtonPrefab == null)
            {
                return;
            }

            Sprite hairPreviewSprite = GetHairPreviewSprite(catalog, selectedHair);
            if (hairPreviewSprite == null)
            {
                return;
            }

            foreach (HairColorLibrary.ColorEntry entry in catalog.HairColorLibrary.Colors)
            {
                ItemSlotButton button = Instantiate(hairColorButtonPrefab, hairColorContentRoot);
                button.Setup(entry.preset.ToString(), entry.displayName, hairPreviewSprite, entry.color);
                button.SetLabelVisible(false);
                button.SetSelected(entry.preset == selectedColor);
                button.SetCardIndex(nextColorCardIndex++);
                WireCardClick(button, () =>
                {
                    activeHairColor = entry.preset;
                    HairColorSelected?.Invoke(entry.preset);
                    RefreshColorSelection(button);
                });
                spawnedColorButtons.Add(button);
            }
        }

        private void HideHairColorRow()
        {
            if (hairColorRow != null)
            {
                hairColorRow.SetActive(false);
            }

            ClearColorButtons();
        }

        private void ClearButtons()
        {
            foreach (ItemSlotButton button in spawnedButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            spawnedButtons.Clear();
            nextCardIndex = 0;
        }

        private void ClearColorButtons()
        {
            foreach (ItemSlotButton button in spawnedColorButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            spawnedColorButtons.Clear();
            nextColorCardIndex = 0;
        }

        private void WireCardClick(ItemSlotButton button, Action applySelection)
        {
            button.Clicked += _ => AdGatedSelectionHelper.Run(this, button.CardIndex, button.ItemId, applySelection);
        }

        private void RefreshSelection(ItemSlotButton selectedButton)
        {
            foreach (ItemSlotButton button in spawnedButtons)
            {
                button.SetSelected(button == selectedButton);
            }
        }

        private void RefreshColorSelection(ItemSlotButton selectedButton)
        {
            foreach (ItemSlotButton button in spawnedColorButtons)
            {
                button.SetSelected(button == selectedButton);
            }
        }

        private static Sprite GetHairPreviewSprite(GameCatalog catalog, HairItem selectedHair)
        {
            if (selectedHair?.Sprite != null)
            {
                return selectedHair.Sprite;
            }

            if (catalog?.HairItems == null)
            {
                return null;
            }

            foreach (HairItem item in catalog.HairItems)
            {
                if (item?.Sprite != null)
                {
                    return item.Sprite;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public void AssignReferences(
            Transform content,
            ItemSlotButton itemPrefab,
            GameObject colorRow,
            Transform colorContent,
            ItemSlotButton colorPrefab)
        {
            contentRoot = content;
            itemButtonPrefab = itemPrefab;
            hairColorRow = colorRow;
            hairColorContentRoot = colorContent;
            hairColorButtonPrefab = colorPrefab;
        }
#endif
    }
}
