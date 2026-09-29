#if UNITY_EDITOR
using DressUpGame.Character;
using DressUpGame.Data;
using DressUpGame.Save;
using DressUpGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Builds MainScene with the required hierarchy, components, and references.
    /// Run after generating placeholder assets.
    /// </summary>
    public static class GameSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainScene.unity";
        private const string ItemButtonPrefabPath = "Assets/Prefabs/ItemButton.prefab";
        private const string CatalogPath = "Assets/Resources/GameCatalog.asset";

        [MenuItem("Dress Up Game/Setup Main Scene")]
        public static void SetupMainScene()
        {
            if (!EnsureEditMode("Setup Main Scene"))
            {
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Rebuild Main Scene?",
                    "This deletes the current MainScene and creates a fresh one.\n\n" +
                    "You will lose manual Scene-view positions for Girl layers that were not saved to item assets.\n\n" +
                    "Use Reload Main Scene if you only want to refresh the open scene.\n" +
                    "Use Setup Bag Layer / Setup Wizard Arrow Buttons for single fixes.",
                    "Rebuild Scene",
                    "Cancel"))
            {
                return;
            }

            if (!System.IO.File.Exists(CatalogPath))
            {
                GameDataGenerator.GenerateAll();
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureProjectForPortraitAndroid();

            GameObject managers = new GameObject("GameManagers");
            CharacterSaveManager saveManager = managers.AddComponent<CharacterSaveManager>();
            GameUIController uiController = managers.AddComponent<GameUIController>();
            GamePlayBootstrap bootstrap = managers.AddComponent<GamePlayBootstrap>();

            Camera mainCamera = CreateMainCamera();
            CreateGameBackground(mainCamera);
            GameObject girl = CreateGirlCharacter();
            CharacterCustomizer customizer = girl.GetComponent<CharacterCustomizer>();
            girl.AddComponent<CharacterDisplayScaler>();

            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            SerializedObject customizerSo = new SerializedObject(customizer);
            customizerSo.FindProperty("catalog").objectReferenceValue = catalog;
            customizerSo.ApplyModifiedPropertiesWithoutUndo();

            Canvas canvas = CreateUI(
                customizer,
                out CategoryBarController categoryBar,
                out MakeupSubBarController makeupSubBar,
                out ItemsPanelController itemsPanel);
            canvas.gameObject.AddComponent<PortraitUILayout>();

            SerializedObject bootstrapSo = new SerializedObject(bootstrap);
            bootstrapSo.FindProperty("mainCamera").objectReferenceValue = mainCamera;
            bootstrapSo.FindProperty("customizer").objectReferenceValue = customizer;
            bootstrapSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject saveSo = new SerializedObject(saveManager);
            saveSo.FindProperty("customizer").objectReferenceValue = customizer;
            saveSo.ApplyModifiedPropertiesWithoutUndo();

            OpeningWizardController openingWizard = managers.GetComponent<OpeningWizardController>();
            if (openingWizard == null)
            {
                openingWizard = managers.AddComponent<OpeningWizardController>();
            }

            DoneViewController doneView = managers.GetComponent<DoneViewController>();
            if (doneView == null)
            {
                doneView = managers.AddComponent<DoneViewController>();
            }

            SerializedObject uiSo = new SerializedObject(uiController);
            uiSo.FindProperty("customizer").objectReferenceValue = customizer;
            uiSo.FindProperty("saveManager").objectReferenceValue = saveManager;
            uiSo.FindProperty("categoryBar").objectReferenceValue = categoryBar;
            uiSo.FindProperty("makeupSubBar").objectReferenceValue = makeupSubBar;
            uiSo.FindProperty("itemsPanel").objectReferenceValue = itemsPanel;
            uiSo.FindProperty("openingWizard").objectReferenceValue = openingWizard;
            uiSo.FindProperty("doneView").objectReferenceValue = doneView;
            uiSo.ApplyModifiedPropertiesWithoutUndo();

            WireWizardAndDoneControllers(canvas, openingWizard, doneView, customizer);

            EnsureEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Dress Up Game: MainScene created at {ScenePath}. Open it and press Play.");
        }

        public static void FullSetup()
        {
            if (!EnsureEditMode("Full Project Setup"))
            {
                return;
            }

            GameDataGenerator.GenerateAll();
            SetupMainScene();
        }

        /// <summary>
        /// Rebuilds UI layout in the open scene without deleting the character or managers.
        /// </summary>
        public static void FixUILayoutInOpenScene()
        {
            if (!EnsureEditMode("Fix UI Layout"))
            {
                return;
            }

            Canvas existingCanvas = Object.FindAnyObjectByType<Canvas>();
            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas.gameObject);
            }

            GameUIController uiController = Object.FindAnyObjectByType<GameUIController>();
            CharacterSaveManager saveManager = Object.FindAnyObjectByType<CharacterSaveManager>();
            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();

            if (uiController == null || saveManager == null || customizer == null)
            {
                Debug.LogError("Dress Up Game: Could not find GameUIController, CharacterSaveManager, or CharacterCustomizer in the open scene.");
                return;
            }

            GameObject girl = customizer.gameObject;
            girl.transform.position = new Vector3(0f, 1.4f, 0f);

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 6f;
            }

            Canvas canvas = CreateUI(
                customizer,
                out CategoryBarController categoryBar,
                out MakeupSubBarController makeupSubBar,
                out ItemsPanelController itemsPanel);
            canvas.gameObject.AddComponent<PortraitUILayout>();

            OpeningWizardController openingWizard = uiController.GetComponent<OpeningWizardController>();
            if (openingWizard == null)
            {
                openingWizard = uiController.gameObject.AddComponent<OpeningWizardController>();
            }

            DoneViewController doneView = uiController.GetComponent<DoneViewController>();
            if (doneView == null)
            {
                doneView = uiController.gameObject.AddComponent<DoneViewController>();
            }

            WireWizardAndDoneControllers(canvas, openingWizard, doneView, customizer);

            if (customizer.GetComponent<CharacterDisplayScaler>() == null)
            {
                customizer.gameObject.AddComponent<CharacterDisplayScaler>();
            }

            CharacterLayerLayout layerLayout = customizer.GetComponent<CharacterLayerLayout>();
            if (layerLayout == null)
            {
                layerLayout = customizer.gameObject.AddComponent<CharacterLayerLayout>();
            }

            SerializedObject customizerSo = new SerializedObject(customizer);
            customizerSo.FindProperty("layerLayout").objectReferenceValue = layerLayout;
            customizerSo.ApplyModifiedPropertiesWithoutUndo();

            GamePlayBootstrap bootstrap = Object.FindAnyObjectByType<GamePlayBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = uiController.gameObject.AddComponent<GamePlayBootstrap>();
            }

            SerializedObject bootstrapSo = new SerializedObject(bootstrap);
            bootstrapSo.FindProperty("mainCamera").objectReferenceValue = camera;
            bootstrapSo.FindProperty("customizer").objectReferenceValue = customizer;
            bootstrapSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject uiSo = new SerializedObject(uiController);
            uiSo.FindProperty("categoryBar").objectReferenceValue = categoryBar;
            uiSo.FindProperty("makeupSubBar").objectReferenceValue = makeupSubBar;
            uiSo.FindProperty("itemsPanel").objectReferenceValue = itemsPanel;
            uiSo.FindProperty("openingWizard").objectReferenceValue = openingWizard;
            uiSo.FindProperty("doneView").objectReferenceValue = doneView;
            uiSo.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Dress Up Game: UI layout fixed. Set Game view aspect ratio to 9:16 Portrait.");
        }

        private static bool EnsureEditMode(string actionName)
        {
            if (!EditorApplication.isPlaying)
            {
                return true;
            }

            EditorUtility.DisplayDialog(
                "Stop Play Mode First",
                "Stop Play before using \"" + actionName + "\".\n\n" +
                "1. Press the Play button to exit play mode\n" +
                "2. Run the menu again",
                "OK");
            return false;
        }

        private static void ConfigureProjectForPortraitAndroid()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
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
            display.Apply(camera);
        }

        private static GameObject CreateGirlCharacter()
        {
            GameObject girl = new GameObject("Girl");
            girl.transform.position = new Vector3(0f, 1.4f, 0f);

            Sprite bodySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Character/body.png");
            SpriteRenderer body = CreateLayer(girl.transform, "Body", bodySprite, 0);
            SpriteRenderer eyes = CreateLayer(girl.transform, "Eyes", null, 1);
            SpriteRenderer dress = CreateLayer(girl.transform, "Dress", null, 2);
            SpriteRenderer shoes = CreateLayer(girl.transform, "Shoes", null, 3);
            SpriteRenderer eyeshadow = CreateLayer(girl.transform, "Eyeshadow", null, 4);
            SpriteRenderer blush = CreateLayer(girl.transform, "Blush", null, 5);
            SpriteRenderer hair = CreateLayer(girl.transform, "Hair", null, 6);
            SpriteRenderer lips = CreateLayer(girl.transform, "Lips", null, 7);
            SpriteRenderer necklace = CreateLayer(girl.transform, "Necklace", null, 8);
            SpriteRenderer earrings = CreateLayer(girl.transform, "Earrings", null, 9);
            SpriteRenderer crown = CreateLayer(girl.transform, "Crown", null, 10);
            SpriteRenderer glasses = CreateLayer(girl.transform, "Glasses", null, 11);
            SpriteRenderer bag = CreateLayer(girl.transform, "Bag", null, 12);

            CharacterCustomizer customizer = girl.AddComponent<CharacterCustomizer>();
            CharacterLayerLayout layerLayout = girl.AddComponent<CharacterLayerLayout>();
            SerializedObject so = new SerializedObject(customizer);
            so.FindProperty("bodyRenderer").objectReferenceValue = body;
            so.FindProperty("hairRenderer").objectReferenceValue = hair;
            so.FindProperty("eyesRenderer").objectReferenceValue = eyes;
            so.FindProperty("eyeshadowRenderer").objectReferenceValue = eyeshadow;
            so.FindProperty("blushRenderer").objectReferenceValue = blush;
            so.FindProperty("lipsRenderer").objectReferenceValue = lips;
            so.FindProperty("dressRenderer").objectReferenceValue = dress;
            so.FindProperty("shoesRenderer").objectReferenceValue = shoes;
            so.FindProperty("necklaceRenderer").objectReferenceValue = necklace;
            so.FindProperty("earringsRenderer").objectReferenceValue = earrings;
            so.FindProperty("crownRenderer").objectReferenceValue = crown;
            so.FindProperty("glassesRenderer").objectReferenceValue = glasses;
            so.FindProperty("bagRenderer").objectReferenceValue = bag;
            so.FindProperty("layerLayout").objectReferenceValue = layerLayout;
            so.ApplyModifiedPropertiesWithoutUndo();

            return girl;
        }

        private static SpriteRenderer CreateLayer(Transform parent, string name, Sprite sprite, int order)
        {
            GameObject layerGo = new GameObject(name);
            layerGo.transform.SetParent(parent, false);
            SpriteRenderer renderer = layerGo.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            if (sprite == null)
            {
                renderer.enabled = false;
            }

            return renderer;
        }

        private static Canvas CreateUI(
            CharacterCustomizer customizer,
            out CategoryBarController categoryBar,
            out MakeupSubBarController makeupSubBar,
            out ItemsPanelController itemsPanel)
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

            // Bottom category tabs — pinned to bottom edge.
            GameObject categoryBarGo = CreateStretchPanel(canvasRect, "CategoryBar",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(16f, 16f), new Vector2(-16f, 136f),
                new Color(0.92f, 0.82f, 0.92f));
            categoryBar = categoryBarGo.AddComponent<CategoryBarController>();

            HorizontalLayoutGroup categoryLayout = categoryBarGo.AddComponent<HorizontalLayoutGroup>();
            categoryLayout.childAlignment = TextAnchor.MiddleCenter;
            categoryLayout.spacing = 12f;
            categoryLayout.padding = new RectOffset(12, 12, 12, 12);
            categoryLayout.childForceExpandWidth = true;
            categoryLayout.childForceExpandHeight = true;

            CreateCategoryButton(categoryBarGo.transform, categoryBar, CustomizationCategory.Hair, "HAIR");
            CreateCategoryButton(categoryBarGo.transform, categoryBar, CustomizationCategory.Makeup, "MAKEUP");
            CreateCategoryButton(categoryBarGo.transform, categoryBar, CustomizationCategory.Dresses, "DRESSES");
            CreateCategoryButton(categoryBarGo.transform, categoryBar, CustomizationCategory.Shoes, "SHOES");
            CreateCategoryButton(categoryBarGo.transform, categoryBar, CustomizationCategory.Accessories, "ACCESSORIES");

            // Item picker — sits above category bar, leaves top of screen clear for the character.
            GameObject itemsPanelGo = CreateStretchPanel(canvasRect, "ItemsPanel",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(16f, 150f), new Vector2(-16f, 520f),
                new Color(0.98f, 0.94f, 0.98f));
            itemsPanel = itemsPanelGo.AddComponent<ItemsPanelController>();

            VerticalLayoutGroup itemsLayout = itemsPanelGo.AddComponent<VerticalLayoutGroup>();
            itemsLayout.childAlignment = TextAnchor.UpperCenter;
            itemsLayout.spacing = 8f;
            itemsLayout.padding = new RectOffset(12, 12, 12, 12);
            itemsLayout.childForceExpandWidth = true;
            itemsLayout.childForceExpandHeight = false;
            itemsLayout.childControlWidth = true;
            itemsLayout.childControlHeight = true;

            GameObject makeupSubBarGo = CreateLayoutRow(itemsPanelGo.transform, "MakeupSubBar", 72f, new Color(0.9f, 0.86f, 0.98f));
            makeupSubBar = makeupSubBarGo.AddComponent<MakeupSubBarController>();
            SerializedObject makeupSo = new SerializedObject(makeupSubBar);
            makeupSo.FindProperty("root").objectReferenceValue = makeupSubBarGo;
            makeupSo.ApplyModifiedPropertiesWithoutUndo();

            HorizontalLayoutGroup makeupLayout = makeupSubBarGo.AddComponent<HorizontalLayoutGroup>();
            makeupLayout.childAlignment = TextAnchor.MiddleCenter;
            makeupLayout.spacing = 8f;
            makeupLayout.padding = new RectOffset(8, 8, 8, 8);
            makeupLayout.childForceExpandWidth = true;
            makeupLayout.childForceExpandHeight = true;

            CreateMakeupSubButton(makeupSubBarGo.transform, makeupSubBar, MakeupType.Lipstick, "LIPSTICK");
            CreateMakeupSubButton(makeupSubBarGo.transform, makeupSubBar, MakeupType.Eyes, "EYES");
            CreateMakeupSubButton(makeupSubBarGo.transform, makeupSubBar, MakeupType.Eyeshadow, "EYESHADOW");
            CreateMakeupSubButton(makeupSubBarGo.transform, makeupSubBar, MakeupType.Blush, "BLUSH");
            makeupSubBarGo.SetActive(false);

            GameObject hairColorRow = CreateLayoutRow(itemsPanelGo.transform, "HairColorRow", 96f, new Color(0.96f, 0.9f, 0.96f));
            hairColorRow.SetActive(false);
            CreateScrollView(hairColorRow.transform, "HairColorScroll", out Transform hairColorContent);

            GameObject itemsScroll = CreateScrollView(itemsPanelGo.transform, "ItemsScroll", out Transform itemsContent);
            LayoutElement itemsScrollLayout = itemsScroll.AddComponent<LayoutElement>();
            itemsScrollLayout.minHeight = 180f;
            itemsScrollLayout.flexibleHeight = 1f;
            RectTransform itemsScrollRect = itemsScroll.GetComponent<RectTransform>();
            itemsScrollRect.anchorMin = new Vector2(0f, 0f);
            itemsScrollRect.anchorMax = new Vector2(1f, 1f);
            itemsScrollRect.offsetMin = Vector2.zero;
            itemsScrollRect.offsetMax = Vector2.zero;

            ItemSlotButton itemPrefab = CreateOrLoadItemButtonPrefab();

            SerializedObject itemsPanelSo = new SerializedObject(itemsPanel);
            itemsPanelSo.FindProperty("contentRoot").objectReferenceValue = itemsContent;
            itemsPanelSo.FindProperty("itemButtonPrefab").objectReferenceValue = itemPrefab;
            itemsPanelSo.FindProperty("hairColorRow").objectReferenceValue = hairColorRow;
            itemsPanelSo.FindProperty("hairColorContentRoot").objectReferenceValue = hairColorContent;
            itemsPanelSo.FindProperty("hairColorButtonPrefab").objectReferenceValue = itemPrefab;
            itemsPanelSo.ApplyModifiedPropertiesWithoutUndo();

            CreateWizardPanel(
                canvasRect,
                itemPrefab,
                customizer,
                categoryBarGo,
                itemsPanelGo,
                out GameObject wizardPanelGo,
                out GameObject donePanelGo);

            wizardPanelGo.SetActive(false);
            donePanelGo.SetActive(false);

            return canvas;
        }

        private static void CreateWizardPanel(
            RectTransform canvasRect,
            ItemSlotButton itemPrefab,
            CharacterCustomizer customizer,
            GameObject categoryBarGo,
            GameObject itemsPanelGo,
            out GameObject wizardPanelGo,
            out GameObject donePanelGo)
        {
            wizardPanelGo = CreateStretchPanel(canvasRect, "WizardPanel",
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

            Button prevArrow = CreateKidFriendlyArrowButton(
                wizardNavOverlayGo.transform,
                "PrevArrowButton",
                WizardArrowButton.ArrowDirection.Previous);
            Button nextArrow = CreateKidFriendlyArrowButton(
                wizardNavOverlayGo.transform,
                "NextArrowButton",
                WizardArrowButton.ArrowDirection.Next);
            PositionWizardArrow(prevArrow, true);
            PositionWizardArrow(nextArrow, false);

            GameObject titleGo = new GameObject("WizardStepTitle");
            titleGo.transform.SetParent(wizardPanelGo.transform, false);
            LayoutElement titleLayout = titleGo.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 48f;
            titleLayout.minHeight = 48f;
            Text titleText = titleGo.AddComponent<Text>();
            titleText.text = "Choose a dress";
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 32;
            titleText.color = new Color(0.35f, 0.15f, 0.35f);
            titleText.fontStyle = FontStyle.Bold;
            titleGo.SetActive(false);

            GameObject wizardScroll = CreateVerticalGridScrollView(wizardPanelGo.transform, "WizardItemsScroll", out Transform wizardContent, out ScrollRect wizardScrollRect);
            LayoutElement scrollLayout = wizardScroll.AddComponent<LayoutElement>();
            scrollLayout.minHeight = 320f;
            scrollLayout.preferredHeight = -1f;
            scrollLayout.flexibleHeight = 1f;

            WizardItemsGridController wizardGrid = wizardPanelGo.AddComponent<WizardItemsGridController>();
            SerializedObject gridSo = new SerializedObject(wizardGrid);
            gridSo.FindProperty("contentRoot").objectReferenceValue = wizardContent;
            gridSo.FindProperty("itemButtonPrefab").objectReferenceValue = itemPrefab;
            gridSo.FindProperty("scrollRect").objectReferenceValue = wizardScrollRect;
            gridSo.ApplyModifiedPropertiesWithoutUndo();

            donePanelGo = CreateTransparentStretchPanel(canvasRect, "DonePanel",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(24f, 144f), new Vector2(-24f, 1124f));

            VerticalLayoutGroup doneLayout = donePanelGo.AddComponent<VerticalLayoutGroup>();
            doneLayout.childAlignment = TextAnchor.LowerCenter;
            doneLayout.reverseArrangement = true;
            doneLayout.spacing = 16f;
            doneLayout.padding = new RectOffset(16, 16, 8, 144);
            doneLayout.childForceExpandWidth = true;
            doneLayout.childForceExpandHeight = false;
            doneLayout.childControlWidth = true;
            doneLayout.childControlHeight = true;

            GameObject doneTextGo = new GameObject("DoneText");
            doneTextGo.transform.SetParent(donePanelGo.transform, false);
            LayoutElement doneTextLayout = doneTextGo.AddComponent<LayoutElement>();
            doneTextLayout.preferredHeight = 64f;
            Text doneText = doneTextGo.AddComponent<Text>();
            doneText.text = "Your look is ready!";
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
            buttonRowLayoutGroup.childForceExpandHeight = false;
            buttonRowLayoutGroup.childControlWidth = true;
            buttonRowLayoutGroup.childControlHeight = true;

            Button editLookButton = CreateDonePanelButton(
                buttonRowGo.transform,
                "EditLookButton",
                DonePanelButton.ButtonKind.EditLook);
            Button startOverButton = CreateDonePanelButton(
                buttonRowGo.transform,
                "StartOverButton",
                DonePanelButton.ButtonKind.StartOver);

            GameUIController uiController = Object.FindAnyObjectByType<GameUIController>();
            OpeningWizardController openingWizard = uiController != null
                ? uiController.GetComponent<OpeningWizardController>()
                : null;
            DoneViewController doneView = uiController != null
                ? uiController.GetComponent<DoneViewController>()
                : null;

            if (openingWizard != null)
            {
                SerializedObject wizardSo = new SerializedObject(openingWizard);
                wizardSo.FindProperty("wizardPanel").objectReferenceValue = wizardPanelGo;
                wizardSo.FindProperty("wizardNavOverlay").objectReferenceValue = wizardNavOverlayGo;
                wizardSo.FindProperty("itemsGrid").objectReferenceValue = wizardGrid;
                wizardSo.FindProperty("prevArrowButton").objectReferenceValue = prevArrow;
                wizardSo.FindProperty("nextArrowButton").objectReferenceValue = nextArrow;
                wizardSo.FindProperty("stepTitleLabel").objectReferenceValue = titleText;
                wizardSo.FindProperty("customizer").objectReferenceValue = customizer;
                wizardSo.ApplyModifiedPropertiesWithoutUndo();
            }

            if (doneView != null)
            {
                SerializedObject doneSo = new SerializedObject(doneView);
                doneSo.FindProperty("donePanel").objectReferenceValue = donePanelGo;
                doneSo.FindProperty("categoryBar").objectReferenceValue = categoryBarGo;
                doneSo.FindProperty("itemsPanel").objectReferenceValue = itemsPanelGo;
                doneSo.FindProperty("wizardPanel").objectReferenceValue = wizardPanelGo;
                doneSo.FindProperty("editLookButton").objectReferenceValue = editLookButton;
                doneSo.FindProperty("startOverButton").objectReferenceValue = startOverButton;
                doneSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static Button CreateDonePanelButton(Transform parent, string name, DonePanelButton.ButtonKind kind)
        {
            GameObject buttonGo = new GameObject(name);
            buttonGo.transform.SetParent(parent, false);
            LayoutElement layout = buttonGo.AddComponent<LayoutElement>();
            layout.flexibleWidth = 1f;
            layout.preferredHeight = 120f;
            layout.minWidth = 0f;

            buttonGo.AddComponent<Image>();
            Button button = buttonGo.AddComponent<Button>();
            DonePanelButton styledButton = buttonGo.AddComponent<DonePanelButton>();
            styledButton.Configure(kind);

            return button;
        }

        private static void WireWizardAndDoneControllers(
            Canvas canvas,
            OpeningWizardController openingWizard,
            DoneViewController doneView,
            CharacterCustomizer customizer)
        {
            Transform canvasTransform = canvas.transform;
            GameObject categoryBar = canvasTransform.Find("CategoryBar")?.gameObject;
            GameObject itemsPanel = canvasTransform.Find("ItemsPanel")?.gameObject;
            GameObject wizardPanel = canvasTransform.Find("WizardPanel")?.gameObject;
            GameObject donePanel = canvasTransform.Find("DonePanel")?.gameObject;

            if (wizardPanel == null)
            {
                return;
            }

            WizardItemsGridController grid = wizardPanel.GetComponent<WizardItemsGridController>();
            GameObject wizardNavOverlay = canvasTransform.Find("WizardNavOverlay")?.gameObject;
            Transform navRoot = wizardNavOverlay != null ? wizardNavOverlay.transform : wizardPanel.transform.Find("WizardNavBar");
            Button prevArrow = navRoot?.Find("PrevArrowButton")?.GetComponent<Button>();
            Button nextArrow = navRoot?.Find("NextArrowButton")?.GetComponent<Button>();
            Text titleText = wizardPanel.transform.Find("WizardStepTitle")?.GetComponent<Text>();

            if (openingWizard != null)
            {
                SerializedObject wizardSo = new SerializedObject(openingWizard);
                wizardSo.FindProperty("wizardPanel").objectReferenceValue = wizardPanel;
                wizardSo.FindProperty("wizardNavOverlay").objectReferenceValue = wizardNavOverlay;
                wizardSo.FindProperty("itemsGrid").objectReferenceValue = grid;
                wizardSo.FindProperty("prevArrowButton").objectReferenceValue = prevArrow;
                wizardSo.FindProperty("nextArrowButton").objectReferenceValue = nextArrow;
                wizardSo.FindProperty("stepTitleLabel").objectReferenceValue = titleText;
                wizardSo.FindProperty("customizer").objectReferenceValue = customizer;
                wizardSo.ApplyModifiedPropertiesWithoutUndo();
            }

            if (doneView != null)
            {
                Button editLookButton = donePanel?.transform.Find("DoneButtonRow/EditLookButton")?.GetComponent<Button>();
                Button startOverButton = donePanel?.transform.Find("DoneButtonRow/StartOverButton")?.GetComponent<Button>();

                SerializedObject doneSo = new SerializedObject(doneView);
                doneSo.FindProperty("donePanel").objectReferenceValue = donePanel;
                doneSo.FindProperty("categoryBar").objectReferenceValue = categoryBar;
                doneSo.FindProperty("itemsPanel").objectReferenceValue = itemsPanel;
                doneSo.FindProperty("wizardPanel").objectReferenceValue = wizardPanel;
                doneSo.FindProperty("editLookButton").objectReferenceValue = editLookButton;
                doneSo.FindProperty("startOverButton").objectReferenceValue = startOverButton;
                doneSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static GameObject CreateVerticalGridScrollView(
            Transform parent,
            string name,
            out Transform content,
            out ScrollRect scrollRect)
        {
            GameObject scrollGo = new GameObject(name);
            scrollGo.transform.SetParent(parent, false);
            RectTransform scrollGoRect = scrollGo.AddComponent<RectTransform>();
            scrollGoRect.anchorMin = Vector2.zero;
            scrollGoRect.anchorMax = Vector2.one;
            scrollGoRect.offsetMin = Vector2.zero;
            scrollGoRect.offsetMax = Vector2.zero;

            Image scrollBg = scrollGo.AddComponent<Image>();
            scrollBg.color = new Color(1f, 1f, 1f, 0.15f);
            scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGo.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            Image viewportMask = viewport.AddComponent<Image>();
            viewportMask.color = Color.white;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = Vector2.zero;
            contentRect.anchoredPosition = Vector2.zero;

            GridLayoutGroup grid = contentGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(195f, 180f);
            grid.spacing = new Vector2(0f, 0f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(0, 0, 0, 56);

            ContentSizeFitter fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
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
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textGo.AddComponent<Text>();

            WizardArrowButton arrowStyle = buttonGo.AddComponent<WizardArrowButton>();
            arrowStyle.Configure(direction);
            return button;
        }

        private static void PositionWizardArrow(Button button, bool isLeft)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.anchorMax = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.pivot = new Vector2(isLeft ? 0f : 1f, 0f);
            rect.anchoredPosition = new Vector2(isLeft ? 24f : -24f, 730f);

            WizardArrowButton arrowStyle = button.GetComponent<WizardArrowButton>();
            if (arrowStyle != null)
            {
                arrowStyle.ApplyStyle();
            }
        }

        private static ItemSlotButton CreateOrLoadItemButtonPrefab()
        {
            ItemSlotButton existing = AssetDatabase.LoadAssetAtPath<ItemSlotButton>(ItemButtonPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject prefabRoot = new GameObject("ItemButton");
            RectTransform rect = prefabRoot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(140f, 160f);

            Sprite cardBorder = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/border.png");
            Sprite adCardBorder = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/borderad.png");
            Image background = prefabRoot.AddComponent<Image>();
            background.sprite = cardBorder;
            background.type = cardBorder != null ? Image.Type.Simple : Image.Type.Sliced;
            background.preserveAspect = cardBorder != null;
            background.color = Color.white;

            Button button = prefabRoot.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.8f, 0.9f);
            colors.pressedColor = new Color(0.95f, 0.65f, 0.8f);
            button.colors = colors;

            GameObject iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(prefabRoot.transform, false);
            RectTransform iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.20f, 0.14f);
            iconRect.anchorMax = new Vector2(0.84f, 0.86f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image icon = iconGo.AddComponent<Image>();
            icon.preserveAspect = true;

            GameObject labelGo = new GameObject("Label");
            labelGo.transform.SetParent(prefabRoot.transform, false);
            RectTransform labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0.3f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            Text label = labelGo.AddComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 22;
            label.color = new Color(0.35f, 0.2f, 0.35f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            ItemSlotButton slot = prefabRoot.AddComponent<ItemSlotButton>();
            SerializedObject slotSo = new SerializedObject(slot);
            slotSo.FindProperty("button").objectReferenceValue = button;
            slotSo.FindProperty("background").objectReferenceValue = background;
            slotSo.FindProperty("icon").objectReferenceValue = icon;
            slotSo.FindProperty("label").objectReferenceValue = label;
            slotSo.FindProperty("defaultBorderSprite").objectReferenceValue = cardBorder;
            slotSo.FindProperty("adBorderSprite").objectReferenceValue = adCardBorder;
            slotSo.ApplyModifiedPropertiesWithoutUndo();

            if (!System.IO.Directory.Exists("Assets/Prefabs"))
            {
                System.IO.Directory.CreateDirectory("Assets/Prefabs");
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, ItemButtonPrefabPath);
            Object.DestroyImmediate(prefabRoot);
            return AssetDatabase.LoadAssetAtPath<ItemSlotButton>(ItemButtonPrefabPath);
        }

        private static GameObject CreateScrollView(Transform parent, string name, out Transform content)
        {
            GameObject scrollGo = new GameObject(name);
            scrollGo.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollGo.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;

            Image scrollBg = scrollGo.AddComponent<Image>();
            scrollBg.color = new Color(1f, 1f, 1f, 0.15f);
            ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGo.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            Image viewportMask = viewport.AddComponent<Image>();
            viewportMask.color = Color.white;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 0.5f);
            contentRect.anchorMax = new Vector2(0f, 0.5f);
            contentRect.pivot = new Vector2(0f, 0.5f);
            contentRect.sizeDelta = new Vector2(1200f, 160f);

            HorizontalLayoutGroup layout = contentGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            ContentSizeFitter fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            content = contentGo.transform;
            return scrollGo;
        }

        private static void CreateCategoryButton(Transform parent, CategoryBarController controller, CustomizationCategory category, string label)
        {
            GameObject buttonGo = new GameObject(category + "Button");
            buttonGo.transform.SetParent(parent, false);
            LayoutElement layout = buttonGo.AddComponent<LayoutElement>();
            layout.minWidth = 140f;
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
            text.fontSize = 26;
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

        private static void CreateMakeupSubButton(Transform parent, MakeupSubBarController controller, MakeupType type, string label)
        {
            GameObject buttonGo = new GameObject(type + "Button");
            buttonGo.transform.SetParent(parent, false);
            Image bg = buttonGo.AddComponent<Image>();
            bg.color = new Color(0.92f, 0.88f, 0.96f);
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
            text.fontSize = 20;
            text.color = new Color(0.3f, 0.2f, 0.45f);
            text.fontStyle = FontStyle.Bold;

            SerializedObject so = new SerializedObject(controller);
            SerializedProperty list = so.FindProperty("subButtons");
            list.InsertArrayElementAtIndex(list.arraySize);
            SerializedProperty element = list.GetArrayElementAtIndex(list.arraySize - 1);
            element.FindPropertyRelative("type").enumValueIndex = (int)type;
            element.FindPropertyRelative("button").objectReferenceValue = button;
            element.FindPropertyRelative("background").objectReferenceValue = bg;
            so.ApplyModifiedPropertiesWithoutUndo();
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
            image.raycastTarget = true;
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

        private static GameObject CreateLayoutRow(Transform parent, string name, float height, Color color)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            row.AddComponent<RectTransform>();
            Image image = row.AddComponent<Image>();
            image.color = color;

            LayoutElement layout = row.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.minHeight = height;
            return row;
        }

        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 anchoredPosition, Color color)
        {
            GameObject buttonGo = new GameObject(name);
            buttonGo.transform.SetParent(parent, false);
            RectTransform rect = buttonGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(200f, 80f);

            Image bg = buttonGo.AddComponent<Image>();
            bg.color = color;
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
            text.fontSize = 28;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;

            return button;
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
