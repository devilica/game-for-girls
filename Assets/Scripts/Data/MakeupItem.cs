using UnityEngine;

namespace DressUpGame.Data
{
    [CreateAssetMenu(fileName = "MakeupItem", menuName = "Dress Up Game/Makeup Item")]
    public class MakeupItem : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;
        [SerializeField] private MakeupType makeupType;
        [SerializeField] private Color tintColor = Color.white;
        [SerializeField] private bool isNoneOption;
        [SerializeField] private Vector3 layerOffset = Vector3.zero;
        [SerializeField] private Vector3 layerScale = Vector3.one;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public MakeupType MakeupType => makeupType;
        public Color TintColor => tintColor;
        public bool IsNoneOption => isNoneOption;
        public Vector3 LayerOffset => layerOffset;
        public Vector3 LayerScale => layerScale;

        public void SetRuntimeData(string itemId, string name, Sprite itemSprite, MakeupType type, Color color, bool noneOption)
        {
            id = itemId;
            displayName = name;
            sprite = itemSprite;
            makeupType = type;
            tintColor = color;
            isNoneOption = noneOption;
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
