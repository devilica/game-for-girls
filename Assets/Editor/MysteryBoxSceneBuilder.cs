#if UNITY_EDITOR
using DressUpGame.Character;
using DressUpGame.Data;
using DressUpGame.MysteryBox;
using DressUpGame.Pet;
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
    public static class MysteryBoxSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MysteryBoxScene.unity";
        private const string BoxSpritePath = MysteryBoxSpriteImportTool.BoxSpritePath;
        private const string HeaderSpritePath = MysteryBoxSpriteImportTool.HeaderSpritePath;
        private const string GameCatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string PetCatalogPath = "Assets/Resources/PetCatalog.asset";
        private const string RegistryPath = "Assets/Resources/MysteryLootRegistry.asset";

        [MenuItem("Dress Up Game/Setup Mystery Box Scene")]
        public static void SetupMysteryBoxScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before setting up MysteryBoxScene.", "OK");
                return;
            }

            EnsureMysteryLootRegistryReady();
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(BoxSpritePath);
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(HeaderSpritePath);
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(MysteryBoxSpriteImportTool.RevealBackgroundSpritePath);
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(MysteryBoxSpriteImportTool.ApplyButtonSpritePath);
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(MysteryBoxSpriteImportTool.RefreshButtonSpritePath);
            MysteryBoxSpriteImportTool.SyncResourcesCopy(BoxSpritePath, MysteryBoxSpriteImportTool.BoxResourcesPath);
            MysteryBoxSpriteImportTool.SyncResourcesCopy(HeaderSpritePath, MysteryBoxSpriteImportTool.HeaderResourcesPath);
            MysteryBoxSpriteImportTool.SyncResourcesCopy(
                MysteryBoxSpriteImportTool.RevealBackgroundSpritePath,
                MysteryBoxSpriteImportTool.RevealBackgroundResourcesPath);
            MysteryBoxSpriteImportTool.SyncResourcesCopy(
                MysteryBoxSpriteImportTool.ApplyButtonSpritePath,
                MysteryBoxSpriteImportTool.ApplyButtonResourcesPath);
            MysteryBoxSpriteImportTool.SyncResourcesCopy(
                MysteryBoxSpriteImportTool.RefreshButtonSpritePath,
                MysteryBoxSpriteImportTool.RefreshButtonResourcesPath);
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(MysteryBoxSpriteImportTool.BoxResourcesPath);
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(MysteryBoxSpriteImportTool.HeaderResourcesPath);
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(MysteryBoxSpriteImportTool.RevealBackgroundResourcesPath);
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(MysteryBoxSpriteImportTool.ApplyButtonResourcesPath);
            MysteryBoxSpriteImportTool.ApplySpriteImportSettings(MysteryBoxSpriteImportTool.RefreshButtonResourcesPath);
            AssetDatabase.Refresh();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = CreateMainCamera();
            CreateGameBackground(camera);

            GameObject previewRoot = new GameObject("CharacterPreview");
            GameObject girl = CreateGirlCharacter();
            girl.transform.SetParent(previewRoot.transform, false);
            GameObject pet = CreatePetCharacter();
            pet.transform.SetParent(previewRoot.transform, false);

            MysteryBoxPreviewLayout previewLayout = previewRoot.AddComponent<MysteryBoxPreviewLayout>();
            SerializedObject previewSo = new SerializedObject(previewLayout);
            previewSo.FindProperty("girlRoot").objectReferenceValue = girl.transform;
            previewSo.FindProperty("petRoot").objectReferenceValue = pet.transform;
            previewSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject managers = new GameObject("MysteryBoxManagers");
            CharacterSaveManager girlSave = managers.AddComponent<CharacterSaveManager>();
            PetSaveManager petSave = managers.AddComponent<PetSaveManager>();

            CharacterCustomizer girlCustomizer = girl.GetComponent<CharacterCustomizer>();
            PetCustomizer petCustomizer = pet.GetComponent<PetCustomizer>();
            GameCatalog gameCatalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(GameCatalogPath);
            PetCatalog petCatalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(PetCatalogPath);

            SerializedObject girlCustomizerSo = new SerializedObject(girlCustomizer);
            girlCustomizerSo.FindProperty("catalog").objectReferenceValue = gameCatalog;
            girlCustomizerSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject petCustomizerSo = new SerializedObject(petCustomizer);
            petCustomizerSo.FindProperty("catalog").objectReferenceValue = petCatalog;
            petCustomizerSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject girlSaveSo = new SerializedObject(girlSave);
            girlSaveSo.FindProperty("customizer").objectReferenceValue = girlCustomizer;
            girlSaveSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject petSaveSo = new SerializedObject(petSave);
            petSaveSo.FindProperty("customizer").objectReferenceValue = petCustomizer;
            petSaveSo.ApplyModifiedPropertiesWithoutUndo();

            Sprite boxSprite = MysteryBoxSpriteImportTool.LoadUiSprite(BoxSpritePath);
            Sprite headerSprite = MysteryBoxSpriteImportTool.LoadUiSprite(HeaderSpritePath);
            MysteryBoxUI ui = CreateCanvas(boxSprite, headerSprite, out MysteryBoxController controller);
            MysteryLootRegistry registry = AssetDatabase.LoadAssetAtPath<MysteryLootRegistry>(RegistryPath);

            SerializedObject controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("lootRegistry").objectReferenceValue = registry;
            controllerSo.FindProperty("girlCustomizer").objectReferenceValue = girlCustomizer;
            controllerSo.FindProperty("petCustomizer").objectReferenceValue = petCustomizer;
            controllerSo.FindProperty("girlSaveManager").objectReferenceValue = girlSave;
            controllerSo.FindProperty("petSaveManager").objectReferenceValue = petSave;
            Sprite resourcesBox = MysteryBoxSpriteImportTool.LoadUiSprite(MysteryBoxSpriteImportTool.BoxResourcesPath);
            if (resourcesBox == null)
            {
                resourcesBox = boxSprite;
            }

            controllerSo.FindProperty("headerBannerSprite").objectReferenceValue = headerSprite;
            controllerSo.FindProperty("girlBoxSprite").objectReferenceValue = resourcesBox;
            controllerSo.FindProperty("petBoxSprite").objectReferenceValue = resourcesBox;
            controllerSo.FindProperty("bestiesBoxSprite").objectReferenceValue = resourcesBox;
            controllerSo.FindProperty("mysteryUi").objectReferenceValue = ui;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, ScenePath);
            SetupMainMenuSceneTool.ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"Dress Up Game: MysteryBoxScene created at {ScenePath}.");
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

        private static GameObject CreatePetCharacter()
        {
            GameObject pet = new GameObject("Pet");
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

        private static MysteryBoxUI CreateCanvas(Sprite boxSprite, Sprite headerSprite, out MysteryBoxController controller)
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<MysteryBoxSceneLayout>();
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            CreateHeaderBanner(canvasRect, headerSprite);
            MysteryBoxUI ui = CreateBoxButtons(canvasRect, boxSprite);
            WireRevealPanel(canvasRect, ui);
            controller = canvasGo.AddComponent<MysteryBoxController>();
            return ui;
        }

        private static void CreateHeaderBanner(RectTransform canvasRect, Sprite headerSprite)
        {
            GameObject headerGo = new GameObject("HeaderBanner");
            headerGo.transform.SetParent(canvasRect, false);
            RectTransform rect = headerGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -32f);
            rect.sizeDelta = new Vector2(900f, 200f);
            Image image = headerGo.AddComponent<Image>();
            image.sprite = headerSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private static MysteryBoxUI CreateBoxButtons(RectTransform canvasRect, Sprite boxSprite)
        {
            GameObject rootGo = new GameObject("MysteryBoxUI");
            rootGo.transform.SetParent(canvasRect, false);
            rootGo.AddComponent<RectTransform>();
            rootGo.AddComponent<MysteryBoxBottomUiLayout>();
            MysteryBoxBottomUiLayout bottomLayout = rootGo.GetComponent<MysteryBoxBottomUiLayout>();
            bottomLayout.ApplyLayout();

            HorizontalLayoutGroup layout = rootGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            MysteryBoxUI ui = rootGo.AddComponent<MysteryBoxUI>();
            SerializedObject uiSo = new SerializedObject(ui);

            CreateBoxColumn(rootGo.transform, boxSprite, "GirlFashionBox",
                out Button girlButton, out Image girlImage);
            CreateBoxColumn(rootGo.transform, boxSprite, "PetSurpriseBox",
                out Button petButton, out Image petImage);
            CreateBoxColumn(rootGo.transform, boxSprite, "BestiesBox",
                out Button bestiesButton, out Image bestiesImage);

            uiSo.FindProperty("girlFashionButton").objectReferenceValue = girlButton;
            uiSo.FindProperty("petSurpriseButton").objectReferenceValue = petButton;
            uiSo.FindProperty("bestiesButton").objectReferenceValue = bestiesButton;
            uiSo.FindProperty("girlBoxImage").objectReferenceValue = girlImage;
            uiSo.FindProperty("petBoxImage").objectReferenceValue = petImage;
            uiSo.FindProperty("bestiesBoxImage").objectReferenceValue = bestiesImage;
            uiSo.ApplyModifiedPropertiesWithoutUndo();
            return ui;
        }

        private static void CreateBoxColumn(
            Transform parent,
            Sprite boxSprite,
            string name,
            out Button button,
            out Image boxImage)
        {
            const float boxSize = MysteryBoxUI.BoxTapSizeReference;

            GameObject columnGo = new GameObject(name);
            columnGo.transform.SetParent(parent, false);
            LayoutElement columnLayout = columnGo.AddComponent<LayoutElement>();
            columnLayout.preferredWidth = boxSize + 16f;
            columnLayout.preferredHeight = boxSize;

            GameObject shakeGo = new GameObject(MysteryBoxUI.BoxShakeVisualName);
            shakeGo.transform.SetParent(columnGo.transform, false);
            RectTransform shakeRect = shakeGo.AddComponent<RectTransform>();
            StretchUiRect(shakeRect);

            GameObject imageGo = new GameObject("BoxImage");
            imageGo.transform.SetParent(shakeGo.transform, false);
            RectTransform imageRect = imageGo.AddComponent<RectTransform>();
            StretchUiRect(imageRect);
            boxImage = imageGo.AddComponent<Image>();
            boxImage.sprite = boxSprite;
            boxImage.preserveAspect = true;
            button = imageGo.AddComponent<Button>();
            button.targetGraphic = boxImage;
        }

        private static void WireRevealPanel(RectTransform canvasRect, MysteryBoxUI ui)
        {
            GameObject panelGo = new GameObject("RevealPanel");
            panelGo.transform.SetParent(canvasRect, false);
            RectTransform panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Image panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0f, 0f, 0f, 0.55f);

            GameObject cardGo = new GameObject("Card");
            cardGo.transform.SetParent(panelRect, false);
            RectTransform cardRect = cardGo.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            Image cardBg = cardGo.AddComponent<Image>();
            Sprite revealFrame = MysteryBoxSpriteImportTool.LoadUiSprite(MysteryBoxSpriteImportTool.RevealBackgroundSpritePath);
            if (revealFrame == null)
            {
                revealFrame = MysteryBoxSpriteImportTool.LoadUiSprite(MysteryBoxSpriteImportTool.RevealBackgroundResourcesPath);
            }

            if (revealFrame != null)
            {
                cardBg.sprite = revealFrame;
                cardBg.color = Color.white;
                cardBg.preserveAspect = true;
                cardBg.raycastTarget = false;
                float width = MysteryBoxArt.RevealCardWidthReference;
                float aspect = revealFrame.rect.height / revealFrame.rect.width;
                cardRect.sizeDelta = new Vector2(width, width * aspect);
            }
            else
            {
                cardRect.sizeDelta = new Vector2(860f, 920f);
                cardBg.color = new Color(1f, 0.97f, 0.99f, 0.98f);
            }

            Text headline = CreateCenteredLabel(cardGo.transform, "Headline", string.Empty, 48, -120f);
            Text rewardTitle = CreateCenteredLabel(cardGo.transform, "RewardTitle", string.Empty, 40, -40f);
            Text rewardDetail = CreateCenteredLabel(cardGo.transform, "RewardDetail", string.Empty, 30, 10f);
            Text rarity = CreateCenteredLabel(cardGo.transform, "Rarity", string.Empty, 34, 60f);
            headline.gameObject.SetActive(false);
            rewardTitle.gameObject.SetActive(false);
            rewardDetail.gameObject.SetActive(false);
            rarity.gameObject.SetActive(false);

            GameObject previewGo = new GameObject("Preview");
            previewGo.transform.SetParent(cardGo.transform, false);
            RectTransform previewRect = previewGo.AddComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.anchoredPosition = new Vector2(0f, MysteryBoxArt.RevealPreviewCenterYReference);
            previewRect.sizeDelta = new Vector2(MysteryBoxArt.RevealPreviewSizeReference, MysteryBoxArt.RevealPreviewSizeReference);
            Image previewImage = previewGo.AddComponent<Image>();
            previewImage.preserveAspect = true;

            GameObject previewSecondaryGo = new GameObject("PreviewSecondary");
            previewSecondaryGo.transform.SetParent(cardGo.transform, false);
            RectTransform previewSecondaryRect = previewSecondaryGo.AddComponent<RectTransform>();
            previewSecondaryRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewSecondaryRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewSecondaryRect.anchoredPosition = new Vector2(
                MysteryBoxArt.RevealPreviewDualSizeReference * 0.55f,
                MysteryBoxArt.RevealPreviewCenterYReference);
            previewSecondaryRect.sizeDelta = new Vector2(
                MysteryBoxArt.RevealPreviewDualSizeReference,
                MysteryBoxArt.RevealPreviewDualSizeReference);
            Image previewSecondaryImage = previewSecondaryGo.AddComponent<Image>();
            previewSecondaryImage.preserveAspect = true;
            previewSecondaryGo.SetActive(false);

            GameObject actionRowGo = new GameObject("RevealActionRow");
            actionRowGo.transform.SetParent(cardGo.transform, false);
            RectTransform actionRowRect = actionRowGo.AddComponent<RectTransform>();
            actionRowRect.anchorMin = new Vector2(0.5f, 0.5f);
            actionRowRect.anchorMax = new Vector2(0.5f, 0.5f);
            actionRowRect.anchoredPosition = Vector2.zero;
            actionRowRect.sizeDelta = new Vector2(640f, 420f);

            Sprite applySprite = MysteryBoxSpriteImportTool.LoadUiSprite(MysteryBoxSpriteImportTool.ApplyButtonSpritePath);
            Sprite refreshSprite = MysteryBoxSpriteImportTool.LoadUiSprite(MysteryBoxSpriteImportTool.RefreshButtonSpritePath);
            Button continueButton = CreateSpriteActionButton(
                actionRowGo.transform,
                "ApplyButton",
                applySprite,
                MysteryBoxArt.ApplyButtonWidthReference);
            Button refreshButton = CreateSpriteActionButton(
                actionRowGo.transform,
                "RefreshButton",
                refreshSprite,
                MysteryBoxArt.RefreshButtonWidthReference);
            if (continueButton != null)
            {
                continueButton.gameObject.name = "ApplyButton";
            }

            Button openAnotherButton = refreshButton;

            GameObject emptyGo = new GameObject("EmptyMessage");
            emptyGo.transform.SetParent(cardGo.transform, false);
            RectTransform emptyRect = emptyGo.AddComponent<RectTransform>();
            emptyRect.anchorMin = new Vector2(0.5f, 0.5f);
            emptyRect.anchorMax = new Vector2(0.5f, 0.5f);
            emptyRect.sizeDelta = new Vector2(760f, 200f);
            Text emptyText = emptyGo.AddComponent<Text>();
            emptyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emptyText.fontSize = 34;
            emptyText.alignment = TextAnchor.MiddleCenter;
            emptyText.color = new Color(0.4f, 0.4f, 0.45f);
            emptyGo.SetActive(false);

            panelGo.SetActive(false);

            SerializedObject uiSo = new SerializedObject(ui);
            uiSo.FindProperty("revealPanel").objectReferenceValue = panelGo;
            uiSo.FindProperty("headlineText").objectReferenceValue = headline;
            uiSo.FindProperty("rewardTitleText").objectReferenceValue = rewardTitle;
            uiSo.FindProperty("rewardDetailText").objectReferenceValue = rewardDetail;
            uiSo.FindProperty("rarityText").objectReferenceValue = rarity;
            uiSo.FindProperty("rewardPreviewImage").objectReferenceValue = previewImage;
            uiSo.FindProperty("rewardPreviewSecondaryImage").objectReferenceValue = previewSecondaryImage;
            uiSo.FindProperty("continueButton").objectReferenceValue = continueButton;
            uiSo.FindProperty("openAnotherButton").objectReferenceValue = openAnotherButton;
            uiSo.FindProperty("emptyMessagePanel").objectReferenceValue = emptyGo;
            uiSo.FindProperty("emptyMessageText").objectReferenceValue = emptyText;
            uiSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Text CreateCenteredLabel(Transform parent, string name, string content, int size, float y)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(760f, 80f);
            Text text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.25f, 0.25f, 0.3f);
            return text;
        }

        private static Button CreateSpriteActionButton(
            Transform parent,
            string name,
            Sprite sprite,
            float widthReference)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            Image image = go.AddComponent<Image>();
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
                float aspect = sprite.rect.height / sprite.rect.width;
                rect.sizeDelta = new Vector2(widthReference, widthReference * aspect);
            }
            else
            {
                rect.sizeDelta = new Vector2(widthReference, widthReference * 0.35f);
                image.color = new Color(0.95f, 0.45f, 0.68f);
            }

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private static void StretchUiRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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

        private static void EnsureMysteryLootRegistryReady()
        {
            MysteryLootRegistry registry = AssetDatabase.LoadAssetAtPath<MysteryLootRegistry>(RegistryPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<MysteryLootRegistry>();
                AssetDatabase.CreateAsset(registry, RegistryPath);
            }

            GameCatalog gameCatalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(GameCatalogPath);
            PetCatalog petCatalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(PetCatalogPath);
            registry.SetCatalogs(gameCatalog, petCatalog);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
