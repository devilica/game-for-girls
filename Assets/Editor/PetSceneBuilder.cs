#if UNITY_EDITOR
using DressUpGame.Pet;
using DressUpGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DressUpGame.Editor
{
    public static class PetSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/PetScene.unity";
        private const string ItemButtonPrefabPath = "Assets/Prefabs/ItemButton.prefab";
        private const string CatalogPath = "Assets/Resources/PetCatalog.asset";

        [MenuItem("Dress Up Game/Setup Pet Scene")]
        public static void SetupPetScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before setting up PetScene.", "OK");
                return;
            }

            if (!System.IO.File.Exists(CatalogPath))
            {
                GeneratePetItemsTool.GeneratePetItems();
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject managers = new GameObject("PetGameManagers");
            PetSaveManager saveManager = managers.AddComponent<PetSaveManager>();
            PetGameUIController uiController = managers.AddComponent<PetGameUIController>();
            PetWizardController wizard = managers.AddComponent<PetWizardController>();
            PetDoneViewController doneView = managers.AddComponent<PetDoneViewController>();

            Camera mainCamera = CreateMainCamera();
            CreateGameBackground(mainCamera);
            GameObject petRoot = CreatePetCharacter();
            PetCustomizer customizer = petRoot.GetComponent<PetCustomizer>();
            petRoot.AddComponent<PetDisplayScaler>();

            PetCatalog catalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(CatalogPath);
            SerializedObject customizerSo = new SerializedObject(customizer);
            customizerSo.FindProperty("catalog").objectReferenceValue = catalog;
            customizerSo.ApplyModifiedPropertiesWithoutUndo();

            Canvas canvas = CreateUI(
                customizer,
                wizard,
                doneView,
                out PetCategoryBarController categoryBar,
                out PetItemsPanelController itemsPanel);

            canvas.gameObject.AddComponent<PortraitUILayout>();

            SerializedObject saveSo = new SerializedObject(saveManager);
            saveSo.FindProperty("customizer").objectReferenceValue = customizer;
            saveSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject uiSo = new SerializedObject(uiController);
            uiSo.FindProperty("customizer").objectReferenceValue = customizer;
            uiSo.FindProperty("saveManager").objectReferenceValue = saveManager;
            uiSo.FindProperty("categoryBar").objectReferenceValue = categoryBar;
            uiSo.FindProperty("itemsPanel").objectReferenceValue = itemsPanel;
            uiSo.FindProperty("wizard").objectReferenceValue = wizard;
            uiSo.FindProperty("doneView").objectReferenceValue = doneView;
            uiSo.ApplyModifiedPropertiesWithoutUndo();

            categoryBar.gameObject.SetActive(false);
            itemsPanel.gameObject.SetActive(false);

            EnsureEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            SetupMainMenuSceneTool.ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"Dress Up Game: PetScene created at {ScenePath}. Run Generate Pet Items if the catalog is empty.");
        }

        [MenuItem("Dress Up Game/Reload Pet Scene")]
        public static void ReloadPetScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before reloading PetScene.", "OK");
                return;
            }

            if (!System.IO.File.Exists(ScenePath))
            {
                SetupPetScene();
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (Object.FindAnyObjectByType<PetCustomizer>() == null)
            {
                bool rebuild = EditorUtility.DisplayDialog(
                    "Pet Scene Not Built Yet",
                    "PetScene has no Pet character (game 2 lives in its own scene — not under Girl in MainScene).\n\n" +
                    "Run Setup Pet Scene now? This creates the Pet root with Body, Hairbow, Collar, and Glasses plus UI.",
                    "Setup Pet Scene",
                    "Cancel");
                if (rebuild)
                {
                    SetupPetScene();
                }

                return;
            }

            Debug.Log("Dress Up Game: PetScene reloaded. Pet is a root object in this scene (not under Girl).");
        }

        public static bool PetSceneHasPlayableHierarchy()
        {
            if (!System.IO.File.Exists(ScenePath))
            {
                return false;
            }

            string yaml = System.IO.File.ReadAllText(ScenePath);
            return yaml.Contains("m_Name: Pet\n") || yaml.Contains("PetCustomizer");
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
            display.UsePetBackground();
            display.Apply(camera);
        }

        private static GameObject CreatePetCharacter()
        {
            GameObject pet = new GameObject("Pet");
            pet.transform.position = new Vector3(0f, 1.4f, 0f);

            SpriteRenderer body = CreateLayer(pet.transform, "Body", null, 0);
            SpriteRenderer hairbow = CreateLayer(pet.transform, "Hairbow", null, 1);
            SpriteRenderer collar = CreateLayer(pet.transform, "Collar", null, 2);
            SpriteRenderer glasses = CreateLayer(pet.transform, "Glasses", null, 3);

            PetCustomizer customizer = pet.AddComponent<PetCustomizer>();
            PetLayerLayout layerLayout = pet.AddComponent<PetLayerLayout>();
            SerializedObject so = new SerializedObject(customizer);
            so.FindProperty("bodyRenderer").objectReferenceValue = body;
            so.FindProperty("hairbowRenderer").objectReferenceValue = hairbow;
            so.FindProperty("collarRenderer").objectReferenceValue = collar;
            so.FindProperty("glassesRenderer").objectReferenceValue = glasses;
            so.FindProperty("layerLayout").objectReferenceValue = layerLayout;
            so.ApplyModifiedPropertiesWithoutUndo();

            return pet;
        }

        private static SpriteRenderer CreateLayer(Transform parent, string name, Sprite sprite, int order)
        {
            GameObject layerGo = new GameObject(name);
            layerGo.transform.SetParent(parent, false);
            SpriteRenderer renderer = layerGo.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            renderer.enabled = sprite != null;
            return renderer;
        }

        private static Canvas CreateUI(
            PetCustomizer customizer,
            PetWizardController wizard,
            PetDoneViewController doneView,
            out PetCategoryBarController categoryBar,
            out PetItemsPanelController itemsPanel)
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
            SoundToggleButton.EnsureOnCanvas(canvasRect);
            BreakScreenController.EnsureOnCanvas(canvasRect);

            GameObject categoryBarGo = CreateStretchPanel(canvasRect, "CategoryBar",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(16f, 16f), new Vector2(-16f, 136f),
                new Color(0.92f, 0.82f, 0.92f));
            categoryBar = categoryBarGo.AddComponent<PetCategoryBarController>();

            HorizontalLayoutGroup categoryLayout = categoryBarGo.AddComponent<HorizontalLayoutGroup>();
            categoryLayout.childAlignment = TextAnchor.MiddleCenter;
            categoryLayout.spacing = 8f;
            categoryLayout.padding = new RectOffset(8, 8, 12, 12);
            categoryLayout.childForceExpandWidth = true;
            categoryLayout.childForceExpandHeight = true;

            CreatePetCategoryButton(categoryBarGo.transform, categoryBar, PetCustomizationCategory.Pet, "PET");
            CreatePetCategoryButton(categoryBarGo.transform, categoryBar, PetCustomizationCategory.Hairbow, "BOW");
            CreatePetCategoryButton(categoryBarGo.transform, categoryBar, PetCustomizationCategory.Collar, "COLLAR");
            CreatePetCategoryButton(categoryBarGo.transform, categoryBar, PetCustomizationCategory.Glasses, "GLASSES");

            GameObject itemsPanelGo = CreateStretchPanel(canvasRect, "ItemsPanel",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(16f, 150f), new Vector2(-16f, 520f),
                new Color(0.98f, 0.94f, 0.98f));
            itemsPanel = itemsPanelGo.AddComponent<PetItemsPanelController>();

            VerticalLayoutGroup itemsLayout = itemsPanelGo.AddComponent<VerticalLayoutGroup>();
            itemsLayout.childAlignment = TextAnchor.UpperCenter;
            itemsLayout.spacing = 8f;
            itemsLayout.padding = new RectOffset(12, 12, 12, 12);
            itemsLayout.childForceExpandWidth = true;
            itemsLayout.childForceExpandHeight = false;
            itemsLayout.childControlWidth = true;
            itemsLayout.childControlHeight = true;

            GameObject itemsScroll = CreateVerticalGridScrollView(itemsPanelGo.transform, "ItemsScroll", out Transform itemsContent, out ScrollRect itemsScrollRect);
            LayoutElement itemsScrollLayout = itemsScroll.AddComponent<LayoutElement>();
            itemsScrollLayout.minHeight = 180f;
            itemsScrollLayout.flexibleHeight = 1f;

            ItemSlotButton itemPrefab = AssetDatabase.LoadAssetAtPath<ItemSlotButton>(ItemButtonPrefabPath);

            SerializedObject itemsPanelSo = new SerializedObject(itemsPanel);
            itemsPanelSo.FindProperty("contentRoot").objectReferenceValue = itemsContent;
            itemsPanelSo.FindProperty("itemButtonPrefab").objectReferenceValue = itemPrefab;
            itemsPanelSo.FindProperty("scrollRect").objectReferenceValue = itemsScrollRect;
            itemsPanelSo.ApplyModifiedPropertiesWithoutUndo();

            CreateWizardAndDoneUI(canvasRect, itemPrefab, customizer, categoryBarGo, itemsPanelGo, wizard, doneView);

            return canvas;
        }

        private static void CreateWizardAndDoneUI(
            RectTransform canvasRect,
            ItemSlotButton itemPrefab,
            PetCustomizer customizer,
            GameObject categoryBarGo,
            GameObject itemsPanelGo,
            PetWizardController wizard,
            PetDoneViewController doneView)
        {
            GameObject wizardPanelGo = CreateStretchPanel(canvasRect, "WizardPanel",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(8f, 8f), new Vector2(-8f, 680f),
                new Color(0.98f, 0.94f, 0.98f));

            VerticalLayoutGroup wizardLayout = wizardPanelGo.AddComponent<VerticalLayoutGroup>();
            wizardLayout.childAlignment = TextAnchor.UpperCenter;
            wizardLayout.spacing = 4f;
            wizardLayout.padding = new RectOffset(4, 4, 4, 4);
            wizardLayout.childForceExpandWidth = true;
            wizardLayout.childForceExpandHeight = false;
            wizardLayout.childControlWidth = true;
            wizardLayout.childControlHeight = true;

            GameObject wizardNavOverlayGo = new GameObject("WizardNavOverlay");
            wizardNavOverlayGo.transform.SetParent(canvasRect, false);
            RectTransform overlayRect = wizardNavOverlayGo.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            wizardNavOverlayGo.SetActive(false);

            Button prevArrow = CreateKidFriendlyArrowButton(wizardNavOverlayGo.transform, "PrevArrowButton", WizardArrowButton.ArrowDirection.Previous);
            Button nextArrow = CreateKidFriendlyArrowButton(wizardNavOverlayGo.transform, "NextArrowButton", WizardArrowButton.ArrowDirection.Next);
            PositionWizardArrow(prevArrow, true);
            PositionWizardArrow(nextArrow, false);

            GameObject wizardScroll = CreateVerticalGridScrollView(wizardPanelGo.transform, "WizardItemsScroll", out Transform wizardContent, out ScrollRect wizardScrollRect);
            LayoutElement scrollLayout = wizardScroll.AddComponent<LayoutElement>();
            scrollLayout.minHeight = 320f;
            scrollLayout.flexibleHeight = 1f;

            PetWizardItemsGridController wizardGrid = wizardPanelGo.AddComponent<PetWizardItemsGridController>();
            SerializedObject gridSo = new SerializedObject(wizardGrid);
            gridSo.FindProperty("contentRoot").objectReferenceValue = wizardContent;
            gridSo.FindProperty("itemButtonPrefab").objectReferenceValue = itemPrefab;
            gridSo.FindProperty("scrollRect").objectReferenceValue = wizardScrollRect;
            gridSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject donePanelGo = CreateTransparentStretchPanel(canvasRect, "DonePanel",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(24f, 144f), new Vector2(-24f, 1124f));

            VerticalLayoutGroup doneLayout = donePanelGo.AddComponent<VerticalLayoutGroup>();
            doneLayout.childAlignment = TextAnchor.LowerCenter;
            doneLayout.reverseArrangement = true;
            doneLayout.spacing = 16f;
            doneLayout.padding = new RectOffset(16, 16, 8, 144);
            doneLayout.childForceExpandWidth = true;
            doneLayout.childForceExpandHeight = false;

            GameObject doneTextGo = new GameObject("DoneText");
            doneTextGo.transform.SetParent(donePanelGo.transform, false);
            LayoutElement doneTextLayout = doneTextGo.AddComponent<LayoutElement>();
            doneTextLayout.preferredHeight = 64f;
            Text doneText = doneTextGo.AddComponent<Text>();
            doneText.text = "Your pet is ready!";
            doneText.alignment = TextAnchor.MiddleCenter;
            doneText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            doneText.fontSize = 42;
            doneText.color = new Color(0.35f, 0.15f, 0.35f);
            doneText.fontStyle = FontStyle.Bold;

            GameObject buttonRowGo = new GameObject("DoneButtonRow");
            buttonRowGo.transform.SetParent(donePanelGo.transform, false);
            buttonRowGo.AddComponent<LayoutElement>();
            HorizontalLayoutGroup buttonRowLayoutGroup = buttonRowGo.AddComponent<HorizontalLayoutGroup>();
            buttonRowLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            buttonRowLayoutGroup.spacing = 20f;
            buttonRowLayoutGroup.childForceExpandWidth = true;

            Button editLookButton = CreateDonePanelButton(buttonRowGo.transform, "EditLookButton", DonePanelButton.ButtonKind.EditLook);
            Button startOverButton = CreateDonePanelButton(buttonRowGo.transform, "StartOverButton", DonePanelButton.ButtonKind.StartOver);

            wizardPanelGo.SetActive(false);
            donePanelGo.SetActive(false);

            SerializedObject wizardSo = new SerializedObject(wizard);
            wizardSo.FindProperty("wizardPanel").objectReferenceValue = wizardPanelGo;
            wizardSo.FindProperty("wizardNavOverlay").objectReferenceValue = wizardNavOverlayGo;
            wizardSo.FindProperty("itemsGrid").objectReferenceValue = wizardGrid;
            wizardSo.FindProperty("prevArrowButton").objectReferenceValue = prevArrow;
            wizardSo.FindProperty("nextArrowButton").objectReferenceValue = nextArrow;
            wizardSo.FindProperty("customizer").objectReferenceValue = customizer;
            wizardSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject doneSo = new SerializedObject(doneView);
            doneSo.FindProperty("donePanel").objectReferenceValue = donePanelGo;
            doneSo.FindProperty("categoryBar").objectReferenceValue = categoryBarGo;
            doneSo.FindProperty("itemsPanel").objectReferenceValue = itemsPanelGo;
            doneSo.FindProperty("wizardPanel").objectReferenceValue = wizardPanelGo;
            doneSo.FindProperty("editLookButton").objectReferenceValue = editLookButton;
            doneSo.FindProperty("startOverButton").objectReferenceValue = startOverButton;
            doneSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreatePetCategoryButton(
            Transform parent,
            PetCategoryBarController controller,
            PetCustomizationCategory category,
            string label)
        {
            GameObject buttonGo = new GameObject(category + "Button");
            buttonGo.transform.SetParent(parent, false);
            LayoutElement layout = buttonGo.AddComponent<LayoutElement>();
            layout.minWidth = 100f;
            layout.preferredHeight = 90f;

            Image bg = buttonGo.AddComponent<Image>();
            bg.color = new Color(0.95f, 0.85f, 0.92f);
            Button button = buttonGo.AddComponent<Button>();

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textGo.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.color = new Color(0.35f, 0.15f, 0.35f);
            text.fontStyle = FontStyle.Bold;

            SerializedObject so = new SerializedObject(controller);
            SerializedProperty list = so.FindProperty("categoryButtons");
            list.InsertArrayElementAtIndex(list.arraySize);
            SerializedProperty element = list.GetArrayElementAtIndex(list.arraySize - 1);
            element.FindPropertyRelative("category").enumValueIndex = (int)category;
            element.FindPropertyRelative("button").objectReferenceValue = button;
            element.FindPropertyRelative("background").objectReferenceValue = bg;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button CreateDonePanelButton(Transform parent, string name, DonePanelButton.ButtonKind kind)
        {
            GameObject buttonGo = new GameObject(name);
            buttonGo.transform.SetParent(parent, false);
            LayoutElement layout = buttonGo.AddComponent<LayoutElement>();
            layout.flexibleWidth = 1f;
            layout.preferredHeight = 120f;

            buttonGo.AddComponent<Image>();
            Button button = buttonGo.AddComponent<Button>();
            DonePanelButton styledButton = buttonGo.AddComponent<DonePanelButton>();
            styledButton.Configure(kind);
            return button;
        }

        private static GameObject CreateStretchPanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static GameObject CreateTransparentStretchPanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject panel = CreateStretchPanel(parent, name, anchorMin, anchorMax, offsetMin, offsetMax, Color.clear);
            Image image = panel.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }

            return panel;
        }

        private static GameObject CreateVerticalGridScrollView(
            Transform parent,
            string name,
            out Transform content,
            out ScrollRect scrollRect)
        {
            GameObject scrollGo = new GameObject(name);
            scrollGo.transform.SetParent(parent, false);
            scrollGo.AddComponent<RectTransform>();
            Image scrollBg = scrollGo.AddComponent<Image>();
            scrollBg.color = new Color(1f, 1f, 1f, 0.15f);
            scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGo.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<Image>().color = Color.white;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);

            GridLayoutGroup grid = contentGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(195f, 180f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            content = contentGo.transform;
            return scrollGo;
        }

        private static Button CreateKidFriendlyArrowButton(
            Transform parent,
            string name,
            WizardArrowButton.ArrowDirection direction)
        {
            GameObject buttonGo = new GameObject(name);
            buttonGo.transform.SetParent(parent, false);
            buttonGo.AddComponent<RectTransform>();
            buttonGo.AddComponent<Image>();
            Button button = buttonGo.AddComponent<Button>();
            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);
            textGo.AddComponent<RectTransform>();
            textGo.AddComponent<Text>();
            WizardArrowButton arrowStyle = buttonGo.AddComponent<WizardArrowButton>();
            arrowStyle.Configure(direction);
            return button;
        }

        private static void PositionWizardArrow(Button button, bool isLeft)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.anchorMax = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.pivot = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.anchoredPosition = new Vector2(isLeft ? 24f : -24f, 730f);
            button.GetComponent<WizardArrowButton>()?.ApplyStyle();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
#endif
