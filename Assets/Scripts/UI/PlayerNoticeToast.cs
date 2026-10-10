using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Short-lived on-screen message for connectivity / ad unlock feedback.
    /// </summary>
    public sealed class PlayerNoticeToast : MonoBehaviour
    {
        public const string AdUnlockRequiresInternetMessage = "Connect to the internet to unlock this item.";
        public const string AdUnlockFailedMessage = "Could not show the ad. Try again when you're online.";

        private const float DisplaySeconds = 2.2f;

        private static PlayerNoticeToast instance;

        private Text messageText;
        private CanvasGroup canvasGroup;
        private Coroutine hideRoutine;

        public static void ShowAdUnlockBlocked()
        {
            Show(AdUnlockRequiresInternetMessage);
        }

        public static void ShowAdUnlockFailed()
        {
            Show(AdUnlockFailedMessage);
        }

        public static void Show(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            EnsureExists();
            instance?.Display(message);
        }

        private static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            GameObject host = new GameObject("PlayerNoticeToast");
            instance = host.AddComponent<PlayerNoticeToast>();
            DontDestroyOnLoad(host);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            BuildUi();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void BuildUi()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;
            gameObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(transform, false);
            RectTransform panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 220f);
            panelRect.sizeDelta = new Vector2(920f, 120f);

            Image background = panelGo.AddComponent<Image>();
            background.color = new Color(0.12f, 0.12f, 0.14f, 0.92f);
            background.raycastTarget = false;

            canvasGroup = panelGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(panelGo.transform, false);
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 12f);
            textRect.offsetMax = new Vector2(-24f, -12f);

            messageText = textGo.AddComponent<Text>();
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            messageText.fontSize = 34;
            messageText.alignment = TextAnchor.MiddleCenter;
            messageText.color = Color.white;
            messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            messageText.verticalOverflow = VerticalWrapMode.Truncate;
            messageText.raycastTarget = false;
        }

        private void Display(string message)
        {
            if (messageText == null)
            {
                BuildUi();
            }

            messageText.text = message;

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(ShowThenHideRoutine());
        }

        private IEnumerator ShowThenHideRoutine()
        {
            canvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(DisplaySeconds);
            canvasGroup.alpha = 0f;
            hideRoutine = null;
        }
    }
}
