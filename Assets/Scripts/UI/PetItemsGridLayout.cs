using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    internal static class PetItemsGridLayout
    {
        public static void Apply(Transform contentRoot, ScrollRect scrollRect, CanvasScaler canvasScaler)
        {
            if (contentRoot == null)
            {
                return;
            }

            ItemsGridLayoutUtility.DisableContentSizeFitter(contentRoot);

            GridLayoutGroup grid = contentRoot.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                return;
            }

            ItemsGridLayoutUtility.ApplyResponsiveGridLayout(grid, scrollRect, canvasScaler);
        }

        public static void UpdateContentHeight(
            Transform contentRoot,
            ScrollRect scrollRect,
            int itemCount)
        {
            if (contentRoot is not RectTransform contentRect)
            {
                return;
            }

            GridLayoutGroup grid = contentRoot.GetComponent<GridLayoutGroup>();
            ItemsGridLayoutUtility.UpdateContentHeight(contentRect, grid, scrollRect, itemCount);
        }
    }
}
