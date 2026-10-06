#if UNITY_EDITOR
using UnityEditor;

namespace DressUpGame.Editor
{
    public static class GenerateCollar4ColorVariantsTool
    {
        [MenuItem("Dress Up Game/Generate Collar 4 Color Variants (6)")]
        public static void GenerateCollar4ColorVariants()
        {
            GenerateCollarColorVariantsTool.GenerateFromCollar4();
        }
    }
}
#endif
