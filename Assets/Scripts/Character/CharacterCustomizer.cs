using DressUpGame.Data;
using DressUpGame.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace DressUpGame.Character
{
    /// <summary>
    /// Applies customization choices to layered SpriteRenderers on the Girl character.
    /// Stores the currently selected item for every category.
    /// </summary>
    public class CharacterCustomizer : MonoBehaviour
    {
        [Header("Character Layers")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer hairRenderer;
        [SerializeField] private SpriteRenderer eyesRenderer;
        [SerializeField] private SpriteRenderer eyeshadowRenderer;
        [SerializeField] private SpriteRenderer blushRenderer;
        [SerializeField] private SpriteRenderer lipsRenderer;
        [SerializeField] private SpriteRenderer dressRenderer;
        [SerializeField] private SpriteRenderer shoesRenderer;
        [SerializeField] private SpriteRenderer necklaceRenderer;
        [SerializeField] private SpriteRenderer earringsRenderer;
        [FormerlySerializedAs("accessoriesRenderer")]
        [SerializeField] private SpriteRenderer crownRenderer;
        [SerializeField] private SpriteRenderer glassesRenderer;
        [SerializeField] private SpriteRenderer bagRenderer;

        [Header("Data")]
        [SerializeField] private GameCatalog catalog;
        [SerializeField] private CharacterLayerLayout layerLayout;

        private HairItem selectedHair;
        private HairColorPreset selectedHairColor;
        private DressItem selectedDress;
        private ShoeItem selectedShoe;
        private MakeupItem selectedLipstick;
        private MakeupItem selectedEyes;
        private MakeupItem selectedEyeshadow;
        private MakeupItem selectedBlush;
        private AccessoryItem selectedNecklace;
        private AccessoryItem selectedEarrings;
        private AccessoryItem selectedCrown;
        private AccessoryItem selectedGlasses;
        private AccessoryItem selectedBag;

        public HairItem SelectedHair => selectedHair;
        public HairColorPreset SelectedHairColor => selectedHairColor;
        public DressItem SelectedDress => selectedDress;
        public ShoeItem SelectedShoe => selectedShoe;
        public MakeupItem SelectedLipstick => selectedLipstick;
        public MakeupItem SelectedEyes => selectedEyes;
        public MakeupItem SelectedEyeshadow => selectedEyeshadow;
        public MakeupItem SelectedBlush => selectedBlush;
        public AccessoryItem SelectedNecklace => selectedNecklace;
        public AccessoryItem SelectedEarrings => selectedEarrings;
        public AccessoryItem SelectedCrown => selectedCrown;
        public AccessoryItem SelectedGlasses => selectedGlasses;
        public AccessoryItem SelectedBag => selectedBag;

        public GameCatalog Catalog => catalog;

        private void OnEnable()
        {
            EnsureShoesRenderer();
            EnsureAccessoryRenderers();
            ApplySortingOrders();
        }

        private void Awake()
        {
            if (catalog == null)
            {
                catalog = Resources.Load<GameCatalog>("GameCatalog");
            }

            if (layerLayout == null)
            {
                layerLayout = GetComponent<CharacterLayerLayout>();
            }
        }

        private void EnsureShoesRenderer()
        {
            if (shoesRenderer != null)
            {
                return;
            }

            Transform shoesLayer = transform.Find("Shoes");
            if (shoesLayer == null)
            {
                GameObject shoesGo = new GameObject("Shoes");
                shoesGo.transform.SetParent(transform, false);
                shoesRenderer = shoesGo.AddComponent<SpriteRenderer>();
                shoesRenderer.enabled = false;
            }
            else
            {
                shoesRenderer = shoesLayer.GetComponent<SpriteRenderer>();
            }
        }

        private void EnsureAccessoryRenderers()
        {
            if (necklaceRenderer == null)
            {
                Transform necklaceLayer = transform.Find("Necklace");
                if (necklaceLayer == null)
                {
                    GameObject necklaceGo = new GameObject("Necklace");
                    necklaceGo.transform.SetParent(transform, false);
                    necklaceRenderer = necklaceGo.AddComponent<SpriteRenderer>();
                    necklaceRenderer.enabled = false;
                }
                else
                {
                    necklaceRenderer = necklaceLayer.GetComponent<SpriteRenderer>();
                }
            }

            if (earringsRenderer == null)
            {
                Transform earringsLayer = transform.Find("Earrings");
                if (earringsLayer == null)
                {
                    GameObject earringsGo = new GameObject("Earrings");
                    earringsGo.transform.SetParent(transform, false);
                    earringsRenderer = earringsGo.AddComponent<SpriteRenderer>();
                    earringsRenderer.enabled = false;
                }
                else
                {
                    earringsRenderer = earringsLayer.GetComponent<SpriteRenderer>();
                }
            }

            if (crownRenderer == null)
            {
                Transform crownLayer = transform.Find("Crown") ?? transform.Find("Accessories");
                crownRenderer = crownLayer != null ? crownLayer.GetComponent<SpriteRenderer>() : null;
            }

            if (glassesRenderer == null)
            {
                Transform glassesLayer = transform.Find("Glasses");
                if (glassesLayer == null)
                {
                    GameObject glassesGo = new GameObject("Glasses");
                    glassesGo.transform.SetParent(transform, false);
                    glassesRenderer = glassesGo.AddComponent<SpriteRenderer>();
                    glassesRenderer.enabled = false;
                }
                else
                {
                    glassesRenderer = glassesLayer.GetComponent<SpriteRenderer>();
                }
            }

            if (bagRenderer == null)
            {
                Transform bagLayer = transform.Find("Bag");
                if (bagLayer == null)
                {
                    GameObject bagGo = new GameObject("Bag");
                    bagGo.transform.SetParent(transform, false);
                    bagRenderer = bagGo.AddComponent<SpriteRenderer>();
                    bagRenderer.enabled = false;
                }
                else
                {
                    bagRenderer = bagLayer.GetComponent<SpriteRenderer>();
                }
            }

        }

        public void ApplySortingOrders()
        {
            SetOrder(bodyRenderer, 0);
            SetOrder(eyesRenderer, 1);
            SetOrder(dressRenderer, 2);
            SetOrder(shoesRenderer, 3);
            SetOrder(eyeshadowRenderer, 4);
            SetOrder(blushRenderer, 5);
            SetOrder(hairRenderer, 6);
            SetOrder(lipsRenderer, 7);
            SetOrder(necklaceRenderer, 8);
            SetOrder(earringsRenderer, 9);
            SetOrder(crownRenderer, 10);
            SetOrder(glassesRenderer, 11);
            SetOrder(bagRenderer, 12);
        }

        private static void SetOrder(SpriteRenderer renderer, int order)
        {
            if (renderer == null)
            {
                return;
            }

            EnsureDefaultSpriteMaterial(renderer);
            renderer.sortingOrder = order;
        }

        private static void EnsureDefaultSpriteMaterial(SpriteRenderer renderer)
        {
            if (renderer.sharedMaterial != null)
            {
                return;
            }

            Material defaultMaterial = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
            if (defaultMaterial != null)
            {
                renderer.sharedMaterial = defaultMaterial;
            }
        }

        public void InitializeDefaults()
        {
            if (catalog == null)
            {
                Debug.LogError("CharacterCustomizer: GameCatalog is not assigned.");
                return;
            }

            selectedHair = catalog.GetDefaultHair();
            selectedHairColor = catalog.HairColorLibrary != null
                ? catalog.HairColorLibrary.GetDefaultPreset()
                : HairColorPreset.Original;
            selectedDress = catalog.GetDefaultDress();
            selectedShoe = catalog.GetDefaultShoe();
            selectedLipstick = catalog.GetDefaultMakeup(MakeupType.Lipstick);
            selectedEyes = catalog.GetDefaultMakeup(MakeupType.Eyes);
            selectedEyeshadow = catalog.GetDefaultMakeup(MakeupType.Eyeshadow);
            selectedBlush = catalog.GetDefaultMakeup(MakeupType.Blush);
            selectedNecklace = catalog.GetDefaultNecklace();
            selectedEarrings = catalog.GetDefaultEarring();
            selectedCrown = catalog.GetDefaultCrown();
            selectedGlasses = catalog.GetDefaultGlasses();
            selectedBag = catalog.GetDefaultBag();

            ApplyAllSelections();
        }

        public void ApplyAllSelections()
        {
            layerLayout?.ApplyAllBasePositions(transform);

            ApplyHair(selectedHair);
            ApplyHairColor(selectedHairColor);
            ApplyDress(selectedDress);
            ApplyShoe(selectedShoe);
            ApplyEye(selectedEyes);
            ApplyMakeup(selectedLipstick, lipsRenderer);
            ApplyMakeup(selectedEyeshadow, eyeshadowRenderer);
            ApplyMakeup(selectedBlush, blushRenderer);
            ApplyAccessory(selectedNecklace, necklaceRenderer);
            ApplyAccessory(selectedEarrings, earringsRenderer);
            ApplyAccessory(selectedCrown, crownRenderer);
            ApplyAccessory(selectedGlasses, glassesRenderer);
            ApplyAccessory(selectedBag, bagRenderer);
        }

        public void RefreshDisplayScale()
        {
            CharacterDisplayScaler scaler = GetComponent<CharacterDisplayScaler>();
            if (scaler != null)
            {
                scaler.Apply();
            }

            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            layout?.Apply();
        }

        public void RefreshDisplayScaleForDoneView()
        {
            CharacterDisplayScaler scaler = GetComponent<CharacterDisplayScaler>();
            if (scaler != null)
            {
                scaler.ApplyForDoneView();
            }

            PortraitUILayout layout = FindAnyObjectByType<PortraitUILayout>();
            layout?.ApplyForDoneView();

            Camera camera = Camera.main;
            if (camera != null)
            {
                GameBackgroundDisplay.EnsureExists(camera);
            }
        }

        public void SetHair(HairItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedHair = item;
            ApplyHair(item);
        }

        public void SetHairColor(HairColorPreset preset)
        {
            selectedHairColor = preset;
            ApplyHairColor(preset);
        }

        public void SetDress(DressItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedDress = item;
            ApplyDress(item);
        }

        public void SetShoe(ShoeItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedShoe = item;
            ApplyShoe(item);
        }

        public void SetLipstick(MakeupItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedLipstick = item;
            ApplyMakeup(item, lipsRenderer);
        }

        public void SetEyes(MakeupItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedEyes = item;
            ApplyEye(item);
        }

        public void SetEyeshadow(MakeupItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedEyeshadow = item;
            ApplyMakeup(item, eyeshadowRenderer);
        }

        public void SetBlush(MakeupItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedBlush = item;
            ApplyMakeup(item, blushRenderer);
        }

        public void SetNecklace(AccessoryItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedNecklace = item;
            ApplyAccessory(item, necklaceRenderer);
        }

        public void SetEarrings(AccessoryItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedEarrings = item;
            ApplyAccessory(item, earringsRenderer);
        }

        public void SetCrown(AccessoryItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedCrown = item;
            ApplyAccessory(item, crownRenderer);
        }

        public void SetGlasses(AccessoryItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedGlasses = item;
            ApplyAccessory(item, glassesRenderer);
        }

        public void SetBag(AccessoryItem item)
        {
            if (item == null)
            {
                return;
            }

            selectedBag = item;
            ApplyAccessory(item, bagRenderer);
        }

        public void SetAccessory(AccessoryItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item.IsNecklaceNone || item.IsNecklaceVariant)
            {
                SetNecklace(item);
                return;
            }

            if (item.IsEarringNone || item.IsEarringVariant)
            {
                SetEarrings(item);
                return;
            }

            if (item.IsCrownNone || item.IsCrownVariant)
            {
                SetCrown(item);
                return;
            }

            if (item.IsGlassesNone || item.IsGlassesVariant)
            {
                SetGlasses(item);
                return;
            }

            if (item.IsBagNone || item.IsBagVariant)
            {
                SetBag(item);
            }
        }

        public CharacterSaveData CaptureSaveData()
        {
            return new CharacterSaveData
            {
                hairId = selectedHair != null ? selectedHair.Id : string.Empty,
                hairColorPreset = selectedHairColor.ToString(),
                dressId = selectedDress != null ? selectedDress.Id : string.Empty,
                lipstickId = selectedLipstick != null ? selectedLipstick.Id : string.Empty,
                eyesId = selectedEyes != null ? selectedEyes.Id : string.Empty,
                eyeshadowId = selectedEyeshadow != null ? selectedEyeshadow.Id : string.Empty,
                blushId = selectedBlush != null ? selectedBlush.Id : string.Empty,
                necklaceId = selectedNecklace != null ? selectedNecklace.Id : string.Empty,
                earringsId = selectedEarrings != null ? selectedEarrings.Id : string.Empty,
                crownId = selectedCrown != null ? selectedCrown.Id : string.Empty,
                glassesId = selectedGlasses != null ? selectedGlasses.Id : string.Empty,
                bagId = selectedBag != null ? selectedBag.Id : string.Empty,
                shoesId = selectedShoe != null ? selectedShoe.Id : string.Empty,
                accessoryId = selectedCrown != null ? selectedCrown.Id : string.Empty
            };
        }

        public void ApplySaveData(CharacterSaveData data)
        {
            if (data == null || catalog == null)
            {
                InitializeDefaults();
                return;
            }

            HairItem hair = catalog.GetHairById(data.hairId) ?? catalog.GetDefaultHair();
            DressItem dress = catalog.GetDressById(data.dressId) ?? catalog.GetDefaultDress();
            MakeupItem lipstick = catalog.GetMakeupById(data.lipstickId) ?? catalog.GetDefaultMakeup(MakeupType.Lipstick);
            MakeupItem eyes = catalog.GetMakeupById(data.eyesId) ?? catalog.GetDefaultMakeup(MakeupType.Eyes);
            MakeupItem eyeshadow = catalog.GetMakeupById(data.eyeshadowId) ?? catalog.GetDefaultMakeup(MakeupType.Eyeshadow);
            MakeupItem blush = catalog.GetMakeupById(data.blushId) ?? catalog.GetDefaultMakeup(MakeupType.Blush);

            AccessoryItem necklace = catalog.GetAccessoryById(data.necklaceId) ?? catalog.GetDefaultNecklace();
            if (necklace != null && !necklace.IsNecklaceNone && !necklace.IsNecklaceVariant)
            {
                necklace = catalog.GetDefaultNecklace();
            }

            AccessoryItem earrings = catalog.GetAccessoryById(data.earringsId) ?? catalog.GetDefaultEarring();
            if (earrings != null && !earrings.IsEarringNone && !earrings.IsEarringVariant)
            {
                earrings = catalog.GetDefaultEarring();
            }

            string crownLookupId = !string.IsNullOrEmpty(data.crownId) ? data.crownId : data.accessoryId;
            AccessoryItem crown = catalog.GetAccessoryById(crownLookupId) ?? catalog.GetDefaultCrown();
            if (crown != null && !crown.IsCrownNone && !crown.IsCrownVariant)
            {
                crown = catalog.GetDefaultCrown();
            }

            AccessoryItem glasses = catalog.GetAccessoryById(data.glassesId) ?? catalog.GetDefaultGlasses();
            if (glasses != null && !glasses.IsGlassesNone && !glasses.IsGlassesVariant)
            {
                glasses = catalog.GetDefaultGlasses();
            }

            AccessoryItem bag = catalog.GetAccessoryById(data.bagId) ?? catalog.GetDefaultBag();
            if (bag != null && !bag.IsBagNone && !bag.IsBagVariant)
            {
                bag = catalog.GetDefaultBag();
            }

            ShoeItem shoe = catalog.GetShoeById(data.shoesId) ?? catalog.GetDefaultShoe();
            if (shoe != null && !shoe.IsShoesNone && !shoe.IsShoesVariant)
            {
                shoe = catalog.GetDefaultShoe();
            }

            selectedHair = hair;
            selectedDress = dress;
            selectedShoe = shoe;
            selectedLipstick = lipstick;
            selectedEyes = eyes;
            selectedEyeshadow = eyeshadow;
            selectedBlush = blush;
            selectedNecklace = necklace;
            selectedEarrings = earrings;
            selectedCrown = crown;
            selectedGlasses = glasses;
            selectedBag = bag;

            if (System.Enum.TryParse(data.hairColorPreset, out HairColorPreset colorPreset))
            {
                selectedHairColor = colorPreset;
            }
            else
            {
                selectedHairColor = catalog.HairColorLibrary != null
                    ? catalog.HairColorLibrary.GetDefaultPreset()
                    : HairColorPreset.Original;
            }

            ApplyAllSelections();
        }

        private void ApplyHair(HairItem item)
        {
            if (hairRenderer == null || item == null)
            {
                return;
            }

            hairRenderer.sprite = item.Sprite;
            hairRenderer.enabled = item.Sprite != null;
            ApplyLayerTransform(hairRenderer.transform, item.LayerOffset, item.LayerScale);
        }

        private void ApplyHairColor(HairColorPreset preset)
        {
            if (hairRenderer == null || catalog?.HairColorLibrary == null)
            {
                return;
            }

            hairRenderer.color = catalog.HairColorLibrary.GetColor(preset);
        }

        private void ApplyDress(DressItem item)
        {
            if (dressRenderer == null || item == null)
            {
                return;
            }

            dressRenderer.sprite = item.Sprite;
            dressRenderer.color = item.TintColor;
            dressRenderer.enabled = item.Sprite != null;
            ApplyLayerTransform(dressRenderer.transform, item.LayerOffset, item.LayerScale);
        }

        private void ApplyShoe(ShoeItem item)
        {
            if (shoesRenderer == null || item == null)
            {
                return;
            }

            if (item.IsShoesNone || item.IsNoneOption || item.Sprite == null)
            {
                shoesRenderer.sprite = null;
                shoesRenderer.enabled = false;
                if (layerLayout != null)
                {
                    layerLayout.ApplyBasePosition(shoesRenderer.transform);
                }

                return;
            }

            shoesRenderer.sprite = item.Sprite;
            shoesRenderer.color = item.TintColor;
            shoesRenderer.enabled = true;
            ApplyLayerTransform(shoesRenderer.transform, item.LayerOffset, item.LayerScale);
        }

        private void ApplyLayerTransform(Transform layerTransform, Vector3 itemOffset, Vector3 itemScale)
        {
            if (layerTransform == null)
            {
                return;
            }

            Vector3 basePosition = layerLayout != null
                ? layerLayout.GetBasePosition(layerTransform.name)
                : Vector3.zero;

            layerTransform.localPosition = basePosition + itemOffset;
            layerTransform.localScale = itemScale == Vector3.zero ? Vector3.one : itemScale;
        }

        private void ApplyEye(MakeupItem item)
        {
            if (eyesRenderer == null || item == null)
            {
                return;
            }

            if (item.IsNoneOption || item.Sprite == null)
            {
                eyesRenderer.sprite = null;
                eyesRenderer.enabled = false;
                EyeIrisTintUtility.ApplyOriginal(eyesRenderer);
                if (layerLayout != null)
                {
                    layerLayout.ApplyBasePosition(eyesRenderer.transform);
                }

                return;
            }

            eyesRenderer.sprite = item.Sprite;
            eyesRenderer.enabled = true;
            ApplyLayerTransform(eyesRenderer.transform, item.LayerOffset, item.LayerScale);
            EyeIrisTintUtility.ApplyToRenderer(eyesRenderer, item);
        }

        private void ApplyMakeup(MakeupItem item, SpriteRenderer renderer)
        {
            if (renderer == null || item == null)
            {
                return;
            }

            if (item.IsNoneOption || item.Sprite == null)
            {
                renderer.sprite = null;
                renderer.enabled = false;
                if (layerLayout != null)
                {
                    layerLayout.ApplyBasePosition(renderer.transform);
                }

                return;
            }

            renderer.sprite = item.Sprite;
            renderer.color = item.TintColor;
            renderer.enabled = true;
            ApplyLayerTransform(renderer.transform, item.LayerOffset, item.LayerScale);
        }

        private void ApplyAccessory(AccessoryItem item, SpriteRenderer renderer)
        {
            if (renderer == null || item == null)
            {
                return;
            }

            if (item.IsNoneOption || item.IsNecklaceNone || item.IsEarringNone || item.IsCrownNone || item.IsGlassesNone || item.IsBagNone || item.Sprite == null)
            {
                renderer.sprite = null;
                renderer.enabled = false;
                if (layerLayout != null)
                {
                    layerLayout.ApplyBasePosition(renderer.transform);
                }

                return;
            }

            renderer.sprite = item.Sprite;
            renderer.color = item.TintColor;
            renderer.enabled = true;
            ApplyLayerTransform(renderer.transform, item.LayerOffset, item.LayerScale);
        }

#if UNITY_EDITOR
        public void SetCatalog(GameCatalog gameCatalog) => catalog = gameCatalog;

        public void AssignRenderers(
            SpriteRenderer body,
            SpriteRenderer hair,
            SpriteRenderer eyes,
            SpriteRenderer eyeshadow,
            SpriteRenderer blush,
            SpriteRenderer lips,
            SpriteRenderer dress,
            SpriteRenderer shoes,
            SpriteRenderer necklace,
            SpriteRenderer crown,
            SpriteRenderer glasses,
            SpriteRenderer bag)
        {
            bodyRenderer = body;
            hairRenderer = hair;
            eyesRenderer = eyes;
            eyeshadowRenderer = eyeshadow;
            blushRenderer = blush;
            lipsRenderer = lips;
            dressRenderer = dress;
            shoesRenderer = shoes;
            necklaceRenderer = necklace;
            crownRenderer = crown;
            glassesRenderer = glasses;
            bagRenderer = bag;
        }
#endif
    }
}
