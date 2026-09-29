using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Shared responsive item grid sizing for dress-up and pet selection panels.
    /// </summary>
    internal static class ItemsGridLayoutUtility
    {
        public const int GridColumns = 4;
        private const float GridHorizontalPadding = 0f;
        private const float GridTopPadding = 0f;
        private const float GridSpacing = 0f;
        private const int BaseGridBottomPadding = 8;
        public const float DefaultExtraBottomPadding = 12f;

        public static void DisableContentSizeFitter(Transform contentRoot)
        {
            if (contentRoot == null)
            {
                return;
            }

            ContentSizeFitter fitter = contentRoot.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.enabled = false;
            }
        }

        public static void ApplyResponsiveGridLayout(
            GridLayoutGroup gridLayout,
            ScrollRect scrollRect,
            CanvasScaler canvasScaler,
            float extraBottomPadding = DefaultExtraBottomPadding)
        {
            if (gridLayout == null)
            {
                return;
            }

            float viewportWidth = 1000f;
            if (scrollRect != null && scrollRect.viewport != null)
            {
                viewportWidth = scrollRect.viewport.rect.width;
            }

            if (viewportWidth <= 1f)
            {
                return;
            }

            float totalSpacing = GridSpacing * (GridColumns - 1);
            float totalPadding = GridHorizontalPadding * 2f;
            float cellWidth = (viewportWidth - totalPadding - totalSpacing) / GridColumns;
            cellWidth = Mathf.Max(64f, cellWidth);
            float cellHeight = cellWidth * 0.92f;

            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = GridColumns;
            gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
            gridLayout.spacing = new Vector2(GridSpacing, GridSpacing);
            gridLayout.childAlignment = TextAnchor.UpperCenter;

            float bottomUiInset = SafeAreaInsets.GetBottomUiInset(canvasScaler, extraBottomPadding);
            int bottomPadding = BaseGridBottomPadding + Mathf.RoundToInt(bottomUiInset);
            gridLayout.padding = new RectOffset(
                Mathf.RoundToInt(GridHorizontalPadding),
                Mathf.RoundToInt(GridHorizontalPadding),
                Mathf.RoundToInt(GridTopPadding),
                bottomPadding);
        }

        public static void UpdateContentHeight(
            RectTransform contentRect,
            GridLayoutGroup gridLayout,
            ScrollRect scrollRect,
            int itemCount)
        {
            if (contentRect == null || gridLayout == null)
            {
                return;
            }

            if (itemCount == 0)
            {
                contentRect.sizeDelta = new Vector2(0f, 0f);
                return;
            }

            int columns = Mathf.Max(1, gridLayout.constraintCount);
            int rows = Mathf.CeilToInt(itemCount / (float)columns);
            RectOffset padding = gridLayout.padding;

            float height = padding.top + padding.bottom
                + rows * gridLayout.cellSize.y
                + Mathf.Max(0, rows - 1) * gridLayout.spacing.y;

            float viewportHeight = 100f;
            if (scrollRect != null && scrollRect.viewport != null)
            {
                viewportHeight = scrollRect.viewport.rect.height;
            }

            contentRect.sizeDelta = new Vector2(0f, Mathf.Max(height, viewportHeight + 1f));
        }
    }
}
