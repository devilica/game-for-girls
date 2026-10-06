#if UNITY_EDITOR
using System.IO;
using DressUpGame.Pet;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class CopyBow6LayoutsToVariantsTool
    {
        private const string Bow6AssetPath = "Assets/Resources/Items/Pet/Hairbows/bow_6.asset";

        [MenuItem("Dress Up Game/Copy Bow 6 Layouts To Bow 8–20")]
        public static void CopyBow6LayoutsToVariants()
        {
            PetItem bow6 = AssetDatabase.LoadAssetAtPath<PetItem>(Bow6AssetPath);
            if (bow6 == null)
            {
                EditorUtility.DisplayDialog("Missing bow_6", "Could not load bow_6 PetItem asset.", "OK");
                return;
            }

            int copied = 0;
            for (int index = 8; index <= 20; index++)
            {
                string path = $"Assets/Resources/Items/Pet/Hairbows/bow_{index}.asset";
                if (!File.Exists(path))
                {
                    continue;
                }

                PetItem target = AssetDatabase.LoadAssetAtPath<PetItem>(path);
                if (target == null)
                {
                    continue;
                }

                target.CopyLayerLayoutsFrom(bow6);
                foreach (string basePetId in PetRecolorPalette.RecolorBasePetIds)
                {
                    target.EnsureColorVariantPetLayouts(basePetId);
                }

                EditorUtility.SetDirty(target);
                copied++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Dress Up Game: Copied bow_6 per-pet layouts to {copied} bow variant(s) (bow_8–bow_20).");
        }
    }
}
#endif
