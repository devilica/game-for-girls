using UnityEngine;

namespace DressUpGame.Character
{
    /// <summary>
    /// Stores default local positions for each character layer on the Girl object.
    /// Per-item offsets from HairItem/DressItem are added on top of these base positions.
    /// </summary>
    public class CharacterLayerLayout : MonoBehaviour
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
            new LayerAnchor { layerName = "Eyes", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Dress", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Shoes", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Hair", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Blush", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Eyeshadow", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Lips", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Necklace", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Earrings", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Crown", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Glasses", localPosition = Vector3.zero },
            new LayerAnchor { layerName = "Bag", localPosition = Vector3.zero }
        };

        public Vector3 GetBasePosition(string layerName)
        {
            string resolvedName = ResolveLayerName(layerName);

            foreach (LayerAnchor anchor in layerAnchors)
            {
                if (anchor.layerName == resolvedName)
                {
                    return anchor.localPosition;
                }
            }

            return Vector3.zero;
        }

        private static string ResolveLayerName(string layerName)
        {
            if (layerName == "Accessories")
            {
                return "Crown";
            }

            return layerName;
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

        /// <summary>
        /// Resets every known layer under Girl to its base position and scale (1,1,1).
        /// Called before applying per-item offsets so Scene and Play mode stay in sync.
        /// </summary>
        public void ApplyAllBasePositions(Transform girlRoot)
        {
            if (girlRoot == null)
            {
                return;
            }

            foreach (LayerAnchor anchor in layerAnchors)
            {
                Transform child = FindLayerTransform(girlRoot, anchor.layerName);
                if (child != null)
                {
                    child.localPosition = anchor.localPosition;
                    child.localScale = Vector3.one;
                }
            }
        }

        private static Transform FindLayerTransform(Transform girlRoot, string layerName)
        {
            Transform child = girlRoot.Find(layerName);
            if (child != null)
            {
                return child;
            }

            if (layerName == "Shoes")
            {
                return girlRoot.Find("Shoes");
            }

            if (layerName == "Necklace")
            {
                return girlRoot.Find("Necklace");
            }

            if (layerName == "Earrings")
            {
                return girlRoot.Find("Earrings");
            }

            if (layerName == "Crown")
            {
                return girlRoot.Find("Accessories");
            }

            if (layerName == "Glasses")
            {
                return girlRoot.Find("Glasses");
            }

            if (layerName == "Bag")
            {
                return girlRoot.Find("Bag");
            }

            return null;
        }

#if UNITY_EDITOR
        public void CaptureCurrentPositionsFromChildren()
        {
            for (int i = 0; i < layerAnchors.Length; i++)
            {
                Transform child = FindLayerTransform(transform, layerAnchors[i].layerName);
                if (child != null)
                {
                    layerAnchors[i].localPosition = child.localPosition;
                }
            }
        }
#endif
    }
}
