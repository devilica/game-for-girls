using System;
using System.Collections;
using System.Collections.Generic;
using DressUpGame.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    public class PetItemsPanelController : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private ItemSlotButton itemButtonPrefab;
        [SerializeField] private ScrollRect scrollRect;

        private readonly List<ItemSlotButton> spawnedButtons = new List<ItemSlotButton>();
        private CanvasScaler canvasScaler;
        private int nextCardIndex;

        public event Action<PetItem> ItemSelected;

        private void Awake()
        {
            canvasScaler = GetComponentInParent<CanvasScaler>();
            if (scrollRect == null && contentRoot != null)
            {
                scrollRect = contentRoot.GetComponentInParent<ScrollRect>();
            }

            PetItemsGridLayout.Apply(contentRoot, scrollRect, canvasScaler);
        }

        private void OnRectTransformDimensionsChange()
        {
            PetItemsGridLayout.Apply(contentRoot, scrollRect, canvasScaler);
            PetItemsGridLayout.UpdateContentHeight(contentRoot, scrollRect, spawnedButtons.Count);
        }

        public void ShowCategory(PetCatalog catalog, PetCustomizationCategory category, PetItem selected)
        {
            ClearButtons();
            if (catalog == null)
            {
                return;
            }

            PetItemsGridLayout.Apply(contentRoot, scrollRect, canvasScaler);
            nextCardIndex = 0;

            foreach (PetItem item in catalog.GetItems(category))
            {
                if (item == null)
                {
                    continue;
                }

                bool isNone = item.IsNoneOption;
                if (!isNone && item.Sprite == null)
                {
                    continue;
                }

                ItemSlotButton button = Instantiate(itemButtonPrefab, contentRoot);
                if (isNone)
                {
                    button.SetupNone(item.Id);
                }
                else
                {
                    button.Setup(item.Id, item.DisplayName, item.Sprite, item.TintColor);
                    button.SetLabelVisible(false);
                }

                button.SetSelected(selected != null && selected.Id == item.Id);

                int cardIndex = nextCardIndex++;
                button.SetCardIndex(
                    cardIndex,
                    PetRewardedAdCardPolicy.IsAdGated(category, cardIndex, item.Id));

                WireScrollDragForwarder(button);

                PetItem captured = item;
                ItemSlotButton capturedButton = button;
                button.Clicked += _ => AdGatedSelectionHelper.RunForPet(
                    this,
                    capturedButton.CardIndex,
                    capturedButton.ItemId,
                    category,
                    () =>
                    {
                        ItemSelected?.Invoke(captured);
                        RefreshSelection(capturedButton);
                    });

                spawnedButtons.Add(button);
            }

            StartCoroutine(RefreshLayoutNextFrame());
        }

        private void WireScrollDragForwarder(ItemSlotButton button)
        {
            if (scrollRect == null || button == null)
            {
                return;
            }

            ScrollDragForwarder forwarder = button.GetComponent<ScrollDragForwarder>();
            if (forwarder == null)
            {
                forwarder = button.gameObject.AddComponent<ScrollDragForwarder>();
            }

            forwarder.Initialize(scrollRect);
        }

        private IEnumerator RefreshLayoutNextFrame()
        {
            yield return null;
            PetItemsGridLayout.Apply(contentRoot, scrollRect, canvasScaler);
            PetItemsGridLayout.UpdateContentHeight(contentRoot, scrollRect, spawnedButtons.Count);

            if (contentRoot is RectTransform contentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }
        }

        private void RefreshSelection(ItemSlotButton selectedButton)
        {
            foreach (ItemSlotButton button in spawnedButtons)
            {
                button.SetSelected(button == selectedButton);
            }
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
    }
}
