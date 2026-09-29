using UnityEngine;

namespace DressUpGame.Data
{
    [CreateAssetMenu(fileName = "DressItem", menuName = "Dress Up Game/Dress Item")]
    public class DressItem : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color tintColor = Color.white;
        [SerializeField] private CustomizationCategory category = CustomizationCategory.Dresses;
        [SerializeField] private Vector3 layerOffset = Vector3.zero;
        [SerializeField] private Vector3 layerScale = Vector3.one;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public Color TintColor => tintColor;
        public CustomizationCategory Category => category;
        public Vector3 LayerOffset => layerOffset;
        public Vector3 LayerScale => layerScale;

#if UNITY_EDITOR
        public void SetLayerLayout(Vector3 offset, Vector3 scale)
        {
            layerOffset = offset;
            layerScale = scale;
        }
#endif

        public void SetRuntimeData(string itemId, string name, Sprite itemSprite, Color? color = null)
        {
            id = itemId;
            displayName = name;
            sprite = itemSprite;
            if (color.HasValue)
            {
                tintColor = color.Value;
            }
        }
    }
}
