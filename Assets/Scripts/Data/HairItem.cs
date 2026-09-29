using UnityEngine;

namespace DressUpGame.Data
{
    [CreateAssetMenu(fileName = "HairItem", menuName = "Dress Up Game/Hair Item")]
    public class HairItem : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;
        [SerializeField] private CustomizationCategory category = CustomizationCategory.Hair;
        [SerializeField] private Vector3 layerOffset = Vector3.zero;
        [SerializeField] private Vector3 layerScale = Vector3.one;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public CustomizationCategory Category => category;
        public Vector3 LayerOffset => layerOffset;
        public Vector3 LayerScale => layerScale;

        public void SetRuntimeData(string itemId, string name, Sprite itemSprite)
        {
            id = itemId;
            displayName = name;
            sprite = itemSprite;
        }

#if UNITY_EDITOR
        public void SetLayerLayout(Vector3 offset, Vector3 scale)
        {
            layerOffset = offset;
            layerScale = scale;
        }
#endif
    }
}
