#if UNITY_EDITOR
using System.IO;
using DressUpGame.Pet;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class CopyCollarLayoutsToVariantsTool
    {
        [MenuItem("Dress Up Game/Copy Collar Layouts To Color Variants")]
        public static void CopyCollarLayoutsToVariants()
        {
            PetItem collar4 = AssetDatabase.LoadAssetAtPath<PetItem>(
                "Assets/Resources/Items/Pet/Collar/collar_4.asset");
            PetItem collar2 = AssetDatabase.LoadAssetAtPath<PetItem>(
                "Assets/Resources/Items/Pet/Collar/collar_2.asset");

            int copied = 0;
            if (collar4 != null)
            {
                copied += CopyRange(collar4, 7, 12);
            }

            if (collar2 != null)
            {
                copied += CopyRange(collar2, 13, 17);
            }

            AssetDatabase.SaveAssets();
            Debug.Log(
                $"Dress Up Game: Copied collar layouts to {copied} variant(s) " +
                "(collar_7–12 from collar_4, collar_13–17 from collar_2).");
        }

        private static int CopyRange(PetItem source, int firstIndex, int lastIndex)
        {
            int count = 0;
            for (int index = firstIndex; index <= lastIndex; index++)
            {
                string path = $"Assets/Resources/Items/Pet/Collar/collar_{index}.asset";
                if (!File.Exists(path))
                {
                    continue;
                }

                PetItem target = AssetDatabase.LoadAssetAtPath<PetItem>(path);
                if (target == null)
                {
                    continue;
                }

                target.CopyLayerLayoutsFrom(source);
                foreach (string basePetId in PetRecolorPalette.RecolorBasePetIds)
                {
                    target.EnsureColorVariantPetLayouts(basePetId);
                }

                EditorUtility.SetDirty(target);
                count++;
            }

            return count;
        }
    }
}
#endif
