using UnityEngine;

namespace DressUpGame.UI
{
    public static class NetworkReachabilityUtility
    {
        public static bool IsInternetAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}
