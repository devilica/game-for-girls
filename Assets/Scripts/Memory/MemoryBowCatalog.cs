using UnityEngine;

namespace DressUpGame.Memory
{
    [CreateAssetMenu(fileName = "MemoryBowCatalog", menuName = "Dress Up Game/Memory Bow Catalog")]
    public sealed class MemoryBowCatalog : ScriptableObject
    {
        public const string ResourcePath = "MemoryBowCatalog";

        [SerializeField] private Sprite[] bows = new Sprite[18];

        public Sprite GetBow(int index)
        {
            if (index < 1 || bows == null || index > bows.Length)
            {
                return null;
            }

            return bows[index - 1];
        }
    }
}
