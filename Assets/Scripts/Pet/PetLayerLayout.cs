using UnityEngine;

namespace DressUpGame.Pet
{
    public class PetLayerLayout : MonoBehaviour
    {
        [System.Serializable]
        public struct LayerAnchor
        {
            public string layerName;
            public Vector3 localPosition;
        }

        [SerializeField] private LayerAnchor[] layerAnchors =
        {
            new LayerAnchor { layerName = "Body", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Hairbow", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Collar", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Glasses", localPosition = Vector3.zero }
        };

        public Vector3 GetBasePosition(string layerName)
        {
            foreach (LayerAnchor anchor in layerAnchors)
            {
                if (anchor.layerName == layerName)
                {
                    return anchor.localPosition;
                }
            }

            return Vector3.zero;
        }

        public void ApplyBasePosition(Transform layerTransform)
        {
            if (layerTransform == null)
            {
                return;
            }

            layerTransform.localPosition = GetBasePosition(layerTransform.name);
            layerTransform.localScale = Vector3.one;
        }

        public void ApplyAllBasePositions(Transform petRoot)
        {
            if (petRoot == null)
            {
                return;
            }

            foreach (LayerAnchor anchor in layerAnchors)
            {
                Transform child = petRoot.Find(anchor.layerName);
                if (child != null)
                {
                    child.localPosition = anchor.localPosition;
                    child.localScale = Vector3.one;
                }
            }
        }
    }
}
