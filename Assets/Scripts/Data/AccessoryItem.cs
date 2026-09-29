using UnityEngine;

namespace DressUpGame.Data
{
    [CreateAssetMenu(fileName = "AccessoryItem", menuName = "Dress Up Game/Accessory Item")]
    public class AccessoryItem : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color tintColor = Color.white;
        [SerializeField] private CustomizationCategory category = CustomizationCategory.Accessories;
        [SerializeField] private bool isNoneOption;
        [SerializeField] private Vector3 layerOffset = Vector3.zero;
        [SerializeField] private Vector3 layerScale = Vector3.one;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public Color TintColor => tintColor;
        public CustomizationCategory Category => category;
        public bool IsNoneOption => isNoneOption;
        public bool IsCrownNone => id == "crown_none";
        public bool IsGlassesNone => id == "glasses_none";
        public bool IsBagNone => id == "bag_none";
        public bool IsNecklaceNone => id == "necklace_none";
        public bool IsEarringNone => id == "earring_none";
        public bool IsCrownVariant => !isNoneOption && id != null && id.StartsWith("crown_");
        public bool IsGlassesVariant => !isNoneOption && id != null && id.StartsWith("glasses_");
        public bool IsBagVariant => !isNoneOption && id != null && id.StartsWith("bag_");
        public bool IsNecklaceVariant => !isNoneOption && id != null && id.StartsWith("necklace_") && !IsNecklaceNone;
        public bool IsEarringVariant => !isNoneOption && id != null && id.StartsWith("ear_");
        public Vector3 LayerOffset => layerOffset;
        public Vector3 LayerScale => layerScale;

#if UNITY_EDITOR
        public void SetLayerLayout(Vector3 offset, Vector3 scale)
        {
            layerOffset = offset;
            layerScale = scale;
        }
#endif

        public void SetRuntimeData(string itemId, string name, Sprite itemSprite, bool noneOption, Color? color = null)
        {
            id = itemId;
            displayName = name;
            sprite = itemSprite;
            isNoneOption = noneOption;
            if (color.HasValue)
            {
                tintColor = color.Value;
            }
            else if (noneOption)
            {
                tintColor = Color.white;
            }
        }
    }
}
