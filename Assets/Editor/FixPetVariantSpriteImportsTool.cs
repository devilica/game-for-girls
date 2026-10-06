#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class FixPetVariantSpriteImportsTool
    {
        [MenuItem("Dress Up Game/Fix Bow/Collar Variant Sprite Imports")]
        public static void FixBowCollarVariantSpriteImports()
        {
            int importCount = PetVariantSpriteImportUtility.AlignAllBowAndCollarVariantImports();
            CopyBow6LayoutsToVariantsTool.CopyBow6LayoutsToVariants();
            CopyCollarLayoutsToVariantsTool.CopyCollarLayoutsToVariants();
            AssetDatabase.SaveAssets();
            Debug.Log(
                $"Dress Up Game: Aligned sprite imports for {importCount} bow/collar variant PNG(s) " +
                "and refreshed per-pet layouts from bow_6, collar_4, and collar_2.");
        }
    }
}
#endif
