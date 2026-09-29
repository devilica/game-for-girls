using System.Collections.Generic;
using UnityEngine;

namespace DressUpGame.Pet
{
    [CreateAssetMenu(fileName = "PetItem", menuName = "Dress Up Game/Pet Item")]
    public class PetItem : ScriptableObject
    {
        [System.Serializable]
        public struct PetSpecificLayerLayout
        {
            public string petId;
            public Vector3 layerOffset;
            public Vector3 layerScale;
        }

        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color tintColor = Color.white;
        [SerializeField] private PetCustomizationCategory category;
        [SerializeField] private bool isNoneOption;
        [SerializeField] private Vector3 layerOffset = Vector3.zero;
        [SerializeField] private Vector3 layerScale = Vector3.one;
        [SerializeField] private List<PetSpecificLayerLayout> perPetLayouts = new List<PetSpecificLayerLayout>();
        [SerializeField] private Vector3 hairbowLayerAnchorOffset = Vector3.zero;
        [SerializeField] private Vector3 collarLayerAnchorOffset = Vector3.zero;
        [SerializeField] private Vector3 glassesLayerAnchorOffset = Vector3.zero;
        [SerializeField] private string layoutBasePetId;

        public string Id => id;
        public string LayoutBasePetId => layoutBasePetId;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public Color TintColor => tintColor;
        public PetCustomizationCategory Category => category;
        public bool IsNoneOption => isNoneOption;

        public bool IsPetNone => id == "pet_none";
        public bool IsHairbowNone => id == "bow_none";
        public bool IsCollarNone => id == "collar_none";
        public bool IsGlassesNone => id == "glasses_none";

        public bool IsPetVariant => !isNoneOption && id != null && id.StartsWith("pet_") && !IsPetNone;
        public bool IsHairbowVariant => !isNoneOption && id != null && id.StartsWith("bow_") && !IsHairbowNone;
        public bool IsCollarVariant => !isNoneOption && id != null && id.StartsWith("collar_") && !IsCollarNone;
        public bool IsGlassesVariant => !isNoneOption && id != null && id.StartsWith("glasses_") && !IsGlassesNone;

        public Vector3 LayerOffset => layerOffset;
        public Vector3 LayerScale => layerScale;

        public string GetLayoutPetId()
        {
            if (!string.IsNullOrEmpty(layoutBasePetId))
            {
                return layoutBasePetId;
            }

            if (IsPetVariant)
            {
                return PetLayoutIds.ResolveBasePetId(id);
            }

            return id;
        }

        public void GetEffectiveLayout(string activePetId, out Vector3 offset, out Vector3 scale)
        {
            if (TryGetLayoutForPet(activePetId, out offset, out scale))
            {
                return;
            }

            string basePetId = PetLayoutIds.ResolveBasePetId(activePetId);
            if (basePetId != activePetId && TryGetLayoutForPet(basePetId, out offset, out scale))
            {
                return;
            }

            offset = layerOffset;
            scale = layerScale;
        }

        private bool TryGetLayoutForPet(string petId, out Vector3 offset, out Vector3 scale)
        {
            offset = layerOffset;
            scale = layerScale;
            if (string.IsNullOrEmpty(petId))
            {
                return false;
            }

            foreach (PetSpecificLayerLayout entry in perPetLayouts)
            {
                if (entry.petId == petId)
                {
                    offset = entry.layerOffset;
                    scale = entry.layerScale;
                    return true;
                }
            }

            return false;
        }

        public Vector3 GetLayerAnchorAdjust(string layerName)
        {
            if (!IsPetVariant)
            {
                return Vector3.zero;
            }

            return layerName switch
            {
                "Hairbow" => hairbowLayerAnchorOffset,
                "Collar" => collarLayerAnchorOffset,
                "Glasses" => glassesLayerAnchorOffset,
                _ => Vector3.zero
            };
        }

#if UNITY_EDITOR
        public void SetLayerLayout(Vector3 offset, Vector3 scale)
        {
            layerOffset = offset;
            layerScale = scale;
        }

        public void SetLayoutForPet(string petId, Vector3 offset, Vector3 scale)
        {
            if (string.IsNullOrEmpty(petId))
            {
                SetLayerLayout(offset, scale);
                return;
            }

            for (int i = 0; i < perPetLayouts.Count; i++)
            {
                if (perPetLayouts[i].petId == petId)
                {
                    PetSpecificLayerLayout entry = perPetLayouts[i];
                    entry.layerOffset = offset;
                    entry.layerScale = scale;
                    perPetLayouts[i] = entry;
                    return;
                }
            }

            perPetLayouts.Add(new PetSpecificLayerLayout
            {
                petId = petId,
                layerOffset = offset,
                layerScale = scale
            });
        }

        public void SetLayerAnchorAdjust(string layerName, Vector3 adjust)
        {
            switch (layerName)
            {
                case "Hairbow":
                    hairbowLayerAnchorOffset = adjust;
                    break;
                case "Collar":
                    collarLayerAnchorOffset = adjust;
                    break;
                case "Glasses":
                    glassesLayerAnchorOffset = adjust;
                    break;
            }
        }

        public void SetEditorData(
            string itemId,
            string name,
            PetCustomizationCategory itemCategory,
            Sprite itemSprite,
            bool noneOption)
        {
            id = itemId;
            displayName = name;
            category = itemCategory;
            sprite = itemSprite;
            isNoneOption = noneOption;
            tintColor = Color.white;
        }

        public void SetColorVariantFromBase(PetItem basePet, string variantId, string variantDisplayName, Color tint)
        {
            if (basePet == null)
            {
                return;
            }

            id = variantId;
            displayName = variantDisplayName;
            category = PetCustomizationCategory.Pet;
            sprite = basePet.Sprite;
            isNoneOption = false;
            tintColor = tint;
            layoutBasePetId = basePet.Id;
            layerOffset = basePet.LayerOffset;
            layerScale = basePet.LayerScale;
            hairbowLayerAnchorOffset = basePet.hairbowLayerAnchorOffset;
            collarLayerAnchorOffset = basePet.collarLayerAnchorOffset;
            glassesLayerAnchorOffset = basePet.glassesLayerAnchorOffset;
        }

        public void EnsureColorVariantPetLayouts(string basePetId)
        {
            if (!TryGetLayoutForPet(basePetId, out Vector3 offset, out Vector3 scale))
            {
                return;
            }

            foreach (string colorSuffix in PetLayoutIds.ColorSuffixes)
            {
                string variantPetId = PetLayoutIds.BuildColorVariantId(basePetId, colorSuffix);
                SetLayoutForPet(variantPetId, offset, scale);
            }
        }
#endif

        public void SetRuntimeData(string itemId, string name, Sprite itemSprite, bool noneOption)
        {
            id = itemId;
            displayName = name;
            sprite = itemSprite;
            isNoneOption = noneOption;
            tintColor = Color.white;
        }
    }
}
