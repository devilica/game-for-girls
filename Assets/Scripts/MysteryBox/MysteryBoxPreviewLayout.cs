using DressUpGame.Character;
using DressUpGame.Pet;
using UnityEngine;

namespace DressUpGame.MysteryBox
{
    /// <summary>
    /// Positions girl and pet previews side by side for the mystery box scene.
    /// </summary>
    public class MysteryBoxPreviewLayout : MonoBehaviour
    {
        [SerializeField] private Transform girlRoot;
        [SerializeField] private Transform petRoot;
        [SerializeField] private Vector3 girlLocalPosition = new Vector3(-1.35f, 0.75f, 0f);
        [SerializeField] private Vector3 petLocalPosition = new Vector3(1.2f, 0.75f, 0f);
        [SerializeField] private float girlScale = 0.85f;
        [SerializeField] private float petScale = 0.5f;
        [SerializeField] private bool alignPetFeetToGirl = true;

        private void Awake()
        {
            if (girlRoot == null)
            {
                CharacterCustomizer girl = FindAnyObjectByType<CharacterCustomizer>();
                girlRoot = girl != null ? girl.transform : null;
            }

            if (petRoot == null)
            {
                PetCustomizer pet = FindAnyObjectByType<PetCustomizer>();
                petRoot = pet != null ? pet.transform : null;
            }

            ApplyLayout();
        }

        public void ApplyLayout()
        {
            if (girlRoot != null)
            {
                girlRoot.localPosition = girlLocalPosition;
                girlRoot.localScale = Vector3.one * girlScale;
            }

            if (petRoot != null)
            {
                petRoot.localPosition = petLocalPosition;
                petRoot.localScale = Vector3.one * petScale;
            }

            if (alignPetFeetToGirl)
            {
                AlignPetFeetToGirl();
            }
        }

        private void AlignPetFeetToGirl()
        {
            if (girlRoot == null || petRoot == null)
            {
                return;
            }

            if (!TryGetLowestBoundsY(girlRoot, out float girlFeetY) ||
                !TryGetLowestBoundsY(petRoot, out float petFeetY))
            {
                return;
            }

            float deltaY = girlFeetY - petFeetY;
            if (Mathf.Approximately(deltaY, 0f))
            {
                return;
            }

            petRoot.position += new Vector3(0f, deltaY, 0f);
        }

        private static bool TryGetLowestBoundsY(Transform root, out float lowestY)
        {
            lowestY = float.PositiveInfinity;
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled || renderer.sprite == null)
                {
                    continue;
                }

                lowestY = Mathf.Min(lowestY, renderer.bounds.min.y);
            }

            return !float.IsPositiveInfinity(lowestY);
        }

#if UNITY_EDITOR
        public void SetRoots(Transform girl, Transform pet)
        {
            girlRoot = girl;
            petRoot = pet;
        }
#endif
    }
}
