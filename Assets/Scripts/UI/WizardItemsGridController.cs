using System;
using System.Collections;
using System.Collections.Generic;
using DressUpGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Builds a vertical scrollable card grid for the opening wizard steps.
    /// </summary>
    public class WizardItemsGridController : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private ItemSlotButton itemButtonPrefab;
        [SerializeField] private ScrollRect scrollRect;

        private readonly List<ItemSlotButton> spawnedButtons = new List<ItemSlotButton>();
        private int nextCardIndex;
        private Coroutine scrollLayoutCoroutine;
        private GridLayoutGroup gridLayout;
        private CanvasScaler canvasScaler;

        private void Awake()
        {
            canvasScaler = GetComponentInParent<CanvasScaler>();

            if (contentRoot != null)
            {
                gridLayout = contentRoot.GetComponent<GridLayoutGroup>();
                ItemsGridLayoutUtility.DisableContentSizeFitter(contentRoot);
            }

            ConfigureScrollRect();
            ApplyResponsiveGridLayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyResponsiveGridLayout();
        }

        public event Action<DressItem> DressSelected;
        public event Action<HairItem> HairSelected;
        public event Action<HairColorPreset> HairColorSelected;
        public event Action<MakeupItem> EyesSelected;
        public event Action<MakeupItem> EyeshadowSelected;
        public event Action<MakeupItem> BlushSelected;
        public event Action<MakeupItem> LipstickSelected;
        public event Action<AccessoryItem> NecklaceSelected;
        public event Action<AccessoryItem> EarringsSelected;
        public event Action<AccessoryItem> CrownSelected;
        public event Action<AccessoryItem> GlassesSelected;
        public event Action<AccessoryItem> BagSelected;
        public event Action<ShoeItem> ShoesSelected;

        public void ShowDressItems(GameCatalog catalog, DressItem selectedDress)
        {
            ClearButtons();

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

                ItemSlotButton button = CreateItemButton(item.Id, item.Sprite, item.TintColor);
                button.SetSelected(selectedDress != null && selectedDress.Id == item.Id);
                WireCardClick(button, () =>
                {
                    DressSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowHairItems(GameCatalog catalog, HairItem selectedHair)
        {
            ClearButtons();

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

                ItemSlotButton button = CreateItemButton(item.Id, item.Sprite);
                button.SetSelected(selectedHair != null && selectedHair.Id == item.Id);
                WireCardClick(button, () =>
                {
                    HairSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowHairColorItems(GameCatalog catalog, HairItem selectedHair, HairColorPreset selectedColor)
        {
            ClearButtons();

            if (catalog?.HairColorLibrary == null)
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
                ItemSlotButton button = CreateItemButton(
                    entry.preset.ToString(),
                    hairPreviewSprite,
                    entry.color);
                button.SetSelected(entry.preset == selectedColor);
                WireCardClick(button, () =>
                {
                    HairColorSelected?.Invoke(entry.preset);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowEyeItems(GameCatalog catalog, MakeupItem selectedEyes)
        {
            ClearButtons();

            if (catalog == null)
            {
                return;
            }

            foreach (MakeupItem item in catalog.GetMakeupItems(MakeupType.Eyes))
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateEyeItemButton(item);
                button.SetSelected(selectedEyes != null && selectedEyes.Id == item.Id);
                WireCardClick(button, () =>
                {
                    EyesSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowEyeshadowItems(GameCatalog catalog, MakeupItem selectedEyeshadow)
        {
            ClearButtons();

            if (catalog == null)
            {
                return;
            }

            foreach (MakeupItem item in catalog.GetMakeupItems(MakeupType.Eyeshadow))
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateMakeupItemButton(item);
                button.SetSelected(selectedEyeshadow != null && selectedEyeshadow.Id == item.Id);
                WireCardClick(button, () =>
                {
                    EyeshadowSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowBlushItems(GameCatalog catalog, MakeupItem selectedBlush)
        {
            ClearButtons();

            if (catalog == null)
            {
                return;
            }

            foreach (MakeupItem item in catalog.GetMakeupItems(MakeupType.Blush))
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateMakeupItemButton(item);
                button.SetSelected(selectedBlush != null && selectedBlush.Id == item.Id);
                WireCardClick(button, () =>
                {
                    BlushSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowLipstickItems(GameCatalog catalog, MakeupItem selectedLipstick)
        {
            ClearButtons();

            if (catalog == null)
            {
                return;
            }

            foreach (MakeupItem item in catalog.GetMakeupItems(MakeupType.Lipstick))
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateMakeupItemButton(item);
                button.SetSelected(selectedLipstick != null && selectedLipstick.Id == item.Id);
                WireCardClick(button, () =>
                {
                    LipstickSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowShoeItems(GameCatalog catalog, ShoeItem selectedShoe)
        {
            ClearButtons();

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

                ItemSlotButton button = CreateItemButton(
                    item.Id,
                    item.Sprite,
                    item.TintColor,
                    isNoneOption: item.IsShoesNone);
                button.SetSelected(selectedShoe != null && selectedShoe.Id == item.Id);
                WireCardClick(button, () =>
                {
                    ShoesSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowNecklaceItems(GameCatalog catalog, AccessoryItem selectedNecklace)
        {
            ClearButtons();

            if (catalog == null)
            {
                return;
            }

            foreach (AccessoryItem item in catalog.GetNecklaceItems())
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateItemButton(
                    item.Id,
                    item.Sprite,
                    item.TintColor,
                    isNoneOption: item.IsNecklaceNone);
                button.SetSelected(selectedNecklace != null && selectedNecklace.Id == item.Id);
                WireCardClick(button, () =>
                {
                    NecklaceSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowEarringItems(GameCatalog catalog, AccessoryItem selectedEarrings)
        {
            ClearButtons();

            if (catalog == null)
            {
                return;
            }

            foreach (AccessoryItem item in catalog.GetEarringItems())
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateItemButton(
                    item.Id,
                    item.Sprite,
                    item.TintColor,
                    isNoneOption: item.IsEarringNone);
                button.SetSelected(selectedEarrings != null && selectedEarrings.Id == item.Id);
                WireCardClick(button, () =>
                {
                    EarringsSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowCrownItems(GameCatalog catalog, AccessoryItem selectedCrown)
        {
            ClearButtons();

            if (catalog == null)
            {
                return;
            }

            foreach (AccessoryItem item in catalog.GetCrownItems())
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateItemButton(
                    item.Id,
                    item.Sprite,
                    item.TintColor,
                    isNoneOption: item.IsCrownNone);
                button.SetSelected(selectedCrown != null && selectedCrown.Id == item.Id);
                WireCardClick(button, () =>
                {
                    CrownSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowGlassesItems(GameCatalog catalog, AccessoryItem selectedGlasses)
        {
            ClearButtons();

            if (catalog == null)
            {
                return;
            }

            foreach (AccessoryItem item in catalog.GetGlassesItems())
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateItemButton(
                    item.Id,
                    item.Sprite,
                    item.TintColor,
                    isNoneOption: item.IsGlassesNone);
                button.SetSelected(selectedGlasses != null && selectedGlasses.Id == item.Id);
                WireCardClick(button, () =>
                {
                    GlassesSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        public void ShowBagItems(GameCatalog catalog, AccessoryItem selectedBag)
        {
            ClearButtons();

            if (catalog == null)
            {
                return;
            }

            foreach (AccessoryItem item in catalog.GetBagItems())
            {
                if (item == null)
                {
                    continue;
                }

                ItemSlotButton button = CreateItemButton(
                    item.Id,
                    item.Sprite,
                    item.TintColor,
                    isNoneOption: item.IsBagNone);
                button.SetSelected(selectedBag != null && selectedBag.Id == item.Id);
                WireCardClick(button, () =>
                {
                    BagSelected?.Invoke(item);
                    RefreshSelection(button);
                });
            }

            ScheduleScrollLayout(resetToTop: true);
        }

        private ItemSlotButton CreateMakeupItemButton(MakeupItem item)
        {
            ItemSlotButton button = CreateItemButton(
                item.Id,
                item.Sprite,
                item.TintColor,
                isNoneOption: item.IsNoneOption);
            return button;
        }

        private ItemSlotButton CreateEyeItemButton(MakeupItem item)
        {
            ItemSlotButton button = Instantiate(itemButtonPrefab, contentRoot);
            button.SetupEye(item);
            if (!item.IsNoneOption)
            {
                button.SetLabelVisible(false);
            }

            ScrollDragForwarder forwarder = button.gameObject.GetComponent<ScrollDragForwarder>();
            if (forwarder == null)
            {
                forwarder = button.gameObject.AddComponent<ScrollDragForwarder>();
            }

            forwarder.Initialize(scrollRect);
            spawnedButtons.Add(button);
            AssignCardIndex(button);
            return button;
        }

        private ItemSlotButton CreateItemButton(
            string id,
            Sprite sprite,
            Color? tint = null,
            string displayName = "",
            bool showLabel = false,
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
                button.SetLabelVisible(showLabel);
            }

            ScrollDragForwarder forwarder = button.gameObject.GetComponent<ScrollDragForwarder>();
            if (forwarder == null)
            {
                forwarder = button.gameObject.AddComponent<ScrollDragForwarder>();
            }

            forwarder.Initialize(scrollRect);
            spawnedButtons.Add(button);
            AssignCardIndex(button);
            return button;
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

        private void AssignCardIndex(ItemSlotButton button)
        {
            button.SetCardIndex(nextCardIndex++);
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

        private void ConfigureScrollRect()
        {
            if (scrollRect == null)
            {
                return;
            }

            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 40f;
            scrollRect.inertia = true;
        }

        private void ScheduleScrollLayout(bool resetToTop)
        {
            if (scrollLayoutCoroutine != null)
            {
                StopCoroutine(scrollLayoutCoroutine);
            }

            scrollLayoutCoroutine = StartCoroutine(ApplyScrollAfterLayout(resetToTop));
        }

        private IEnumerator ApplyScrollAfterLayout(bool resetToTop)
        {
            yield return null;

            ApplyResponsiveGridLayout();
            UpdateContentHeight();

            if (contentRoot is RectTransform contentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                scrollRect.velocity = Vector2.zero;

                if (resetToTop)
                {
                    scrollRect.verticalNormalizedPosition = 1f;
                }
            }

            scrollLayoutCoroutine = null;
        }

        private void UpdateContentHeight()
        {
            if (contentRoot is not RectTransform contentRect)
            {
                return;
            }

            ItemsGridLayoutUtility.UpdateContentHeight(
                contentRect,
                gridLayout,
                scrollRect,
                spawnedButtons.Count);
        }

        private void ApplyResponsiveGridLayout()
        {
            ItemsGridLayoutUtility.ApplyResponsiveGridLayout(
                gridLayout,
                scrollRect,
                canvasScaler,
                ItemsGridLayoutUtility.DefaultExtraBottomPadding);
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
        public void AssignReferences(Transform content, ItemSlotButton prefab, ScrollRect scroll)
        {
            contentRoot = content;
            itemButtonPrefab = prefab;
            scrollRect = scroll;
            gridLayout = content != null ? content.GetComponent<GridLayoutGroup>() : null;
        }
#endif
    }
}
