using UnityEngine;

namespace DressUpGame.Pet
{
    public class PetCustomizer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer hairbowRenderer;
        [SerializeField] private SpriteRenderer collarRenderer;
        [SerializeField] private SpriteRenderer glassesRenderer;
        [SerializeField] private PetCatalog catalog;
        [SerializeField] private PetLayerLayout layerLayout;

        private PetItem selectedPet;
        private PetItem selectedHairbow;
        private PetItem selectedCollar;
        private PetItem selectedGlasses;

        public PetItem SelectedPet => selectedPet;
        public PetItem SelectedHairbow => selectedHairbow;
        public PetItem SelectedCollar => selectedCollar;
        public PetItem SelectedGlasses => selectedGlasses;
        public PetCatalog Catalog => catalog;

        private void Awake()
        {
            if (catalog == null)
            {
                catalog = Resources.Load<PetCatalog>("PetCatalog");
            }

            if (layerLayout == null)
            {
                layerLayout = GetComponent<PetLayerLayout>();
            }

            EnsureRenderers();
        }

        private void OnEnable()
        {
            EnsureRenderers();
            ApplySortingOrders();
        }

        private void EnsureRenderers()
        {
            bodyRenderer = EnsureLayer(bodyRenderer, "Body");
            hairbowRenderer = EnsureLayer(hairbowRenderer, "Hairbow");
            collarRenderer = EnsureLayer(collarRenderer, "Collar");
            glassesRenderer = EnsureLayer(glassesRenderer, "Glasses");
        }

        private SpriteRenderer EnsureLayer(SpriteRenderer existing, string layerName)
        {
            if (existing != null)
            {
                return existing;
            }

            Transform layer = transform.Find(layerName);
            if (layer == null)
            {
                GameObject go = new GameObject(layerName);
                go.transform.SetParent(transform, false);
                existing = go.AddComponent<SpriteRenderer>();
                existing.enabled = false;
            }
            else
            {
                existing = layer.GetComponent<SpriteRenderer>();
                if (existing == null)
                {
                    existing = layer.gameObject.AddComponent<SpriteRenderer>();
                    existing.enabled = false;
                }
            }

            return existing;
        }

        public void ApplySortingOrders()
        {
            SetOrder(bodyRenderer, 0);
            SetOrder(hairbowRenderer, 1);
            SetOrder(collarRenderer, 2);
            SetOrder(glassesRenderer, 3);
        }

        private static void SetOrder(SpriteRenderer renderer, int order)
        {
            if (renderer != null)
            {
                renderer.sortingOrder = order;
            }
        }

        public void InitializeDefaults()
        {
            if (catalog == null)
            {
                return;
            }

            selectedPet = catalog.GetDefault(PetCustomizationCategory.Pet);
            selectedHairbow = catalog.GetDefault(PetCustomizationCategory.Hairbow);
            selectedCollar = catalog.GetDefault(PetCustomizationCategory.Collar);
            selectedGlasses = catalog.GetDefault(PetCustomizationCategory.Glasses);
            ApplyAllSelections();
        }

        public void ApplyAllSelections()
        {
            layerLayout?.ApplyAllBasePositions(transform);
            ApplyLayerItem(selectedPet, bodyRenderer, true);
            ApplyLayerItem(selectedHairbow, hairbowRenderer, false);
            ApplyLayerItem(selectedCollar, collarRenderer, false);
            ApplyLayerItem(selectedGlasses, glassesRenderer, false);
        }

        /// <summary>
        /// In the Editor, layer selections are often only visible on SpriteRenderers until Play mode runs.
        /// </summary>
        private void SyncSelectionsFromSceneRenderers()
        {
            if (catalog == null)
            {
                return;
            }

            PetItem petFromBody = InferItemFromRenderer(bodyRenderer, PetCustomizationCategory.Pet);
            if (petFromBody != null && petFromBody.IsPetVariant)
            {
                selectedPet = petFromBody;
            }
            else if (selectedPet == null || selectedPet.IsPetNone)
            {
                selectedPet = catalog.GetDefault(PetCustomizationCategory.Pet);
            }

            PetItem hairbowFromScene = InferItemFromRenderer(hairbowRenderer, PetCustomizationCategory.Hairbow);
            if (hairbowFromScene != null)
            {
                selectedHairbow = hairbowFromScene;
            }

            PetItem collarFromScene = InferItemFromRenderer(collarRenderer, PetCustomizationCategory.Collar);
            if (collarFromScene != null)
            {
                selectedCollar = collarFromScene;
            }

            PetItem glassesFromScene = InferItemFromRenderer(glassesRenderer, PetCustomizationCategory.Glasses);
            if (glassesFromScene != null)
            {
                selectedGlasses = glassesFromScene;
            }
        }

        private PetItem InferItemFromRenderer(SpriteRenderer renderer, PetCustomizationCategory category)
        {
            if (renderer == null || renderer.sprite == null || catalog == null)
            {
                return null;
            }

            PetItem item = catalog.GetById(renderer.sprite.name);
            if (item == null || item.Category != category)
            {
                return null;
            }

            return item;
        }

        private PetItem GetLayoutPet()
        {
            if (selectedPet != null && selectedPet.IsPetVariant)
            {
                return selectedPet;
            }

            return InferItemFromRenderer(bodyRenderer, PetCustomizationCategory.Pet);
        }

        private string GetActivePetIdForLayout()
        {
            PetItem pet = GetLayoutPet();
            return pet != null ? pet.GetLayoutPetId() : string.Empty;
        }

        public void SetPet(PetItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item.IsPetNone && catalog != null)
            {
                item = catalog.GetDefault(PetCustomizationCategory.Pet);
            }

            if (item == null || item.IsPetNone)
            {
                return;
            }

            selectedPet = item;
            ApplyLayerItem(item, bodyRenderer, true);
            ReapplyAccessoryLayersForCurrentPet();
        }

        private void ReapplyAccessoryLayersForCurrentPet()
        {
            ApplyLayerItem(selectedHairbow, hairbowRenderer, false);
            ApplyLayerItem(selectedCollar, collarRenderer, false);
            ApplyLayerItem(selectedGlasses, glassesRenderer, false);
        }

        public void SetHairbow(PetItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedHairbow = item;
            ApplyLayerItem(item, hairbowRenderer, false);
        }

        public void SetCollar(PetItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedCollar = item;
            ApplyLayerItem(item, collarRenderer, false);
        }

        public void SetGlasses(PetItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedGlasses = item;
            ApplyLayerItem(item, glassesRenderer, false);
        }

        public void SetItemForCategory(PetCustomizationCategory category, PetItem item)
        {
            switch (category)
            {
                case PetCustomizationCategory.Pet:
                    SetPet(item);
                    break;
                case PetCustomizationCategory.Hairbow:
                    SetHairbow(item);
                    break;
                case PetCustomizationCategory.Collar:
                    SetCollar(item);
                    break;
                case PetCustomizationCategory.Glasses:
                    SetGlasses(item);
                    break;
            }
        }

        public PetSaveData CaptureSaveData()
        {
            return new PetSaveData
            {
                petId = selectedPet != null ? selectedPet.Id : string.Empty,
                hairbowId = selectedHairbow != null ? selectedHairbow.Id : string.Empty,
                collarId = selectedCollar != null ? selectedCollar.Id : string.Empty,
                glassesId = selectedGlasses != null ? selectedGlasses.Id : string.Empty
            };
        }

        public void ApplySaveData(PetSaveData data)
        {
            if (data == null || catalog == null)
            {
                InitializeDefaults();
                return;
            }

            selectedPet = ResolvePetFromSave(data.petId);
            selectedHairbow = catalog.GetById(data.hairbowId) ?? catalog.GetDefault(PetCustomizationCategory.Hairbow);
            selectedCollar = catalog.GetById(data.collarId) ?? catalog.GetDefault(PetCustomizationCategory.Collar);
            selectedGlasses = catalog.GetById(data.glassesId) ?? catalog.GetDefault(PetCustomizationCategory.Glasses);
            ApplyAllSelections();
        }

        public void RefreshDisplayScale()
        {
            PetDisplayScaler scaler = GetComponent<PetDisplayScaler>();
            scaler?.Apply();
        }

        public void RefreshDisplayScaleForDoneView()
        {
            PetDisplayScaler scaler = GetComponent<PetDisplayScaler>();
            scaler?.ApplyForDoneView();
        }

        private void ApplyLayerItem(PetItem item, SpriteRenderer renderer, bool isBody)
        {
            if (renderer == null || item == null)
            {
                return;
            }

            if (item.IsNoneOption || item.Sprite == null)
            {
                renderer.sprite = null;
                renderer.enabled = false;
                layerLayout?.ApplyBasePosition(renderer.transform);
                return;
            }

            renderer.sprite = item.Sprite;
            renderer.color = item.TintColor;
            renderer.enabled = true;
            if (isBody)
            {
                ApplyLayerTransform(renderer.transform, item.LayerOffset, item.LayerScale);
                return;
            }

            item.GetEffectiveLayout(GetActivePetIdForLayout(), out Vector3 offset, out Vector3 scale);
            ApplyLayerTransform(renderer.transform, offset, scale);
        }

        private PetItem ResolvePetFromSave(string petId)
        {
            PetItem pet = catalog.GetById(petId);
            if (pet != null && !pet.IsPetNone && pet.IsPetVariant)
            {
                return pet;
            }

            return catalog.GetDefault(PetCustomizationCategory.Pet);
        }

        private void ApplyLayerTransform(Transform layerTransform, Vector3 offset, Vector3 scale)
        {
            Vector3 basePosition = Vector3.zero;
            if (layerLayout != null)
            {
                basePosition = layerLayout.GetBasePosition(layerTransform.name);
                PetItem layoutPet = GetLayoutPet();
                if (layoutPet != null && layoutPet.IsPetVariant)
                {
                    basePosition += layoutPet.GetLayerAnchorAdjust(layerTransform.name);
                }
            }

            layerTransform.localPosition = basePosition + offset;
            layerTransform.localScale = scale;
        }

#if UNITY_EDITOR
        [SerializeField] [HideInInspector] private string editorLastBodySpriteId = string.Empty;

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EnsureRenderers();
            string bodySpriteId = bodyRenderer != null && bodyRenderer.sprite != null
                ? bodyRenderer.sprite.name
                : string.Empty;
            if (bodySpriteId == editorLastBodySpriteId)
            {
                return;
            }

            editorLastBodySpriteId = bodySpriteId;
            UnityEditor.EditorApplication.delayCall += HandleEditorBodySpriteChanged;
        }

        private void HandleEditorBodySpriteChanged()
        {
            if (this == null || Application.isPlaying)
            {
                return;
            }

            SyncSelectionsFromSceneRenderers();
            ReapplyAccessoryLayersForCurrentPet();
        }
#endif
    }
}
