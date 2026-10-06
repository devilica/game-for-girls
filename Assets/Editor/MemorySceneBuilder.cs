#if UNITY_EDITOR
using DressUpGame.Memory;
using DressUpGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DressUpGame.Editor
{
    public static class MemorySceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MemoryScene.unity";

        [MenuItem("Dress Up Game/Setup Memory Scene")]
        public static void SetupMemoryScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before setting up MemoryScene.", "OK");
                return;
            }

            GenerateMemoryBowCatalogTool.RelocateMemoryGameResources();
            SyncMemoryBowSpritesTool.SyncMemoryBowSpritesToResources();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = CreateMainCamera();
            CreateGameBackground(camera);

            Canvas canvas = CreateCanvas(out MemoryGameController controller);

            EnsureEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            SetupMainMenuSceneTool.ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"Dress Up Game: MemoryScene created at {ScenePath}.");
        }

        private static Camera CreateMainCamera()
        {
            GameObject cameraGo = new GameObject("Main Camera");
            Camera camera = cameraGo.AddComponent<Camera>();
            cameraGo.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.98f, 0.92f, 0.96f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraGo.AddComponent<AudioListener>();
            return camera;
        }

        private static void CreateGameBackground(Camera camera)
        {
            GameObject backgroundObject = new GameObject("GameBackground");
            GameBackgroundDisplay display = backgroundObject.AddComponent<GameBackgroundDisplay>();
            display.SetBackgroundSources("UI/menu/background", "Assets/Resources/UI/menu/background.png");
            display.Apply(camera);
        }

        private static Canvas CreateCanvas(out MemoryGameController controller)
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            GameObject boardGo = new GameObject("MemoryBoard");
            boardGo.transform.SetParent(canvasRect, false);
            RectTransform boardRect = boardGo.AddComponent<RectTransform>();
            boardRect.anchorMin = Vector2.zero;
            boardRect.anchorMax = Vector2.one;
            boardRect.offsetMin = Vector2.zero;
            boardRect.offsetMax = Vector2.zero;

            GridLayoutGroup grid = boardGo.AddComponent<GridLayoutGroup>();

            const float startAgainWidth = 920f;
            const float startAgainHeight = 240f;

            GameObject startAgainGo = new GameObject("StartAgainButton");
            startAgainGo.transform.SetParent(canvasRect, false);
            RectTransform startAgainRect = startAgainGo.AddComponent<RectTransform>();
            startAgainRect.anchorMin = new Vector2(0.5f, 0.5f);
            startAgainRect.anchorMax = new Vector2(0.5f, 0.5f);
            startAgainRect.pivot = new Vector2(0.5f, 0.5f);
            startAgainRect.anchoredPosition = Vector2.zero;
            startAgainRect.sizeDelta = new Vector2(startAgainWidth, startAgainHeight);

            Image startAgainImage = startAgainGo.AddComponent<Image>();
            Sprite startAgainSprite = Resources.Load<Sprite>("UI/startagain");
            startAgainImage.sprite = startAgainSprite;
            startAgainImage.type = Image.Type.Simple;
            startAgainImage.preserveAspect = true;
            startAgainImage.color = Color.white;

            Button restartButton = startAgainGo.AddComponent<Button>();
            restartButton.targetGraphic = startAgainImage;

            GameObject winPanelGo = startAgainGo;
            startAgainGo.SetActive(false);

            BreakNativeAdSlotView nativeAdSlot = BreakNativeAdSlotView.EnsureForMemory(canvasRect);

            controller = canvasGo.AddComponent<MemoryGameController>();
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("boardRoot").objectReferenceValue = boardRect;
            so.FindProperty("grid").objectReferenceValue = grid;
            so.FindProperty("winPanel").objectReferenceValue = winPanelGo;
            so.FindProperty("restartButton").objectReferenceValue = restartButton;
            so.FindProperty("nativeAdSlot").objectReferenceValue = nativeAdSlot;
            MemoryBowCatalog catalog = AssetDatabase.LoadAssetAtPath<MemoryBowCatalog>(GenerateMemoryBowCatalogTool.CatalogPath);
            so.FindProperty("bowCatalog").objectReferenceValue = catalog;
            so.ApplyModifiedPropertiesWithoutUndo();

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }
}
#endif
