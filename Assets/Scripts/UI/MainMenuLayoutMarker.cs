using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Tracks main-menu UI structure version so runtime can rebuild when layout rules change.
    /// </summary>
    public sealed class MainMenuLayoutMarker : MonoBehaviour
    {
        public const int CurrentLayoutVersion = 6;

        [SerializeField] private int layoutVersion = CurrentLayoutVersion;

        public int LayoutVersion => layoutVersion;

        public static bool HasMoreGamesBanner(Transform mainMenuRoot)
        {
            if (!mainMenuRoot)
            {
                return false;
            }

            Transform bannerRow = mainMenuRoot.Find("MoreGamesBannerRow");
            if (!bannerRow)
            {
                return false;
            }

            Transform card = bannerRow.Find("MoreGamesCard");
            if (!card)
            {
                return false;
            }

            return card.Find("BannerArt") != null;
        }

        public static bool IsCurrent(Transform mainMenuRoot)
        {
            if (!mainMenuRoot)
            {
                return false;
            }

            MainMenuLayoutMarker marker = mainMenuRoot.GetComponent<MainMenuLayoutMarker>();
            if (marker == null || marker.layoutVersion < CurrentLayoutVersion)
            {
                return false;
            }

            return HasMoreGamesBanner(mainMenuRoot);
        }

        public static void Stamp(Transform mainMenuRoot)
        {
            if (!mainMenuRoot)
            {
                return;
            }

            MainMenuLayoutMarker marker = mainMenuRoot.GetComponent<MainMenuLayoutMarker>();
            if (marker == null)
            {
                marker = mainMenuRoot.gameObject.AddComponent<MainMenuLayoutMarker>();
            }

            marker.layoutVersion = CurrentLayoutVersion;
        }
    }
}
