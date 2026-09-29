#if UNITY_EDITOR
using DressUpGame.Character;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class SetupNecklaceLayerTool
    {
        [MenuItem("Dress Up Game/Setup Necklace Layer")]
        public static void SetupNecklaceLayer()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Stop Play before setting up the Necklace layer.",
                    "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog(
                    "No Girl",
                    "No Girl with CharacterCustomizer found in the open scene.",
                    "OK");
                return;
            }

            Transform necklaceTransform = EnsureNecklaceLayerInternal(customizer, out bool changed);
            if (necklaceTransform == null)
            {
                return;
            }

            if (changed)
            {
                EditorSceneManager.SaveOpenScenes();
            }

            Selection.activeGameObject = necklaceTransform.gameObject;
            Debug.Log(
                changed
                    ? "Dress Up Game: Necklace layer added under Girl (scene saved)."
                    : "Dress Up Game: Necklace layer already present and wired.");
        }

        /// <summary>
        /// Ensures Girl/Necklace exists and is wired on CharacterCustomizer. Used by Reload Main Scene.
        /// </summary>
        public static bool EnsureNecklaceLayerInOpenScene()
        {
            if (EditorApplication.isPlaying)
            {
                return false;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                return false;
            }

            EnsureNecklaceLayerInternal(customizer, out bool changed);
            return changed;
        }

        private static Transform EnsureNecklaceLayerInternal(CharacterCustomizer customizer, out bool changed)
        {
            changed = false;
            Transform girl = customizer.transform;
            Transform necklaceTransform = girl.Find("Necklace");
            SpriteRenderer necklaceRenderer;

            if (necklaceTransform == null)
            {
                GameObject necklaceGo = new GameObject("Necklace");
                Undo.RegisterCreatedObjectUndo(necklaceGo, "Create Necklace Layer");
                necklaceGo.transform.SetParent(girl, false);
                necklaceTransform = necklaceGo.transform;
                necklaceRenderer = necklaceGo.AddComponent<SpriteRenderer>();
                necklaceRenderer.enabled = false;
                necklaceRenderer.sortingOrder = 8;
                changed = true;
            }
            else
            {
                necklaceRenderer = necklaceTransform.GetComponent<SpriteRenderer>();
                if (necklaceRenderer == null)
                {
                    necklaceRenderer = necklaceTransform.gameObject.AddComponent<SpriteRenderer>();
                    necklaceRenderer.enabled = false;
                    changed = true;
                }

                if (necklaceRenderer.sortingOrder != 8)
                {
                    necklaceRenderer.sortingOrder = 8;
                    changed = true;
                }
            }

            changed |= UpdateSortingOrder(girl.Find("Eyeshadow"), 4);
            changed |= UpdateSortingOrder(girl.Find("Blush"), 5);
            changed |= UpdateSortingOrder(girl.Find("Hair"), 6);
            changed |= UpdateSortingOrder(girl.Find("Lips"), 7);
            changed |= UpdateSortingOrder(girl.Find("Earrings"), 9);
            changed |= UpdateSortingOrder(girl.Find("Crown"), 10);
            changed |= UpdateSortingOrder(girl.Find("Glasses"), 11);
            changed |= UpdateSortingOrder(girl.Find("Bag"), 12);

            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            if (layout != null)
            {
                SerializedObject layoutSo = new SerializedObject(layout);
                SerializedProperty anchors = layoutSo.FindProperty("layerAnchors");
                bool hasNecklaceAnchor = false;

                for (int i = 0; i < anchors.arraySize; i++)
                {
                    SerializedProperty entry = anchors.GetArrayElementAtIndex(i);
                    if (entry.FindPropertyRelative("layerName").stringValue == "Necklace")
                    {
                        hasNecklaceAnchor = true;
                        break;
                    }
                }

                if (!hasNecklaceAnchor)
                {
                    int insertIndex = -1;
                    for (int i = 0; i < anchors.arraySize; i++)
                    {
                        if (anchors.GetArrayElementAtIndex(i).FindPropertyRelative("layerName").stringValue == "Crown")
                        {
                            insertIndex = i;
                            break;
                        }
                    }

                    if (insertIndex < 0)
                    {
                        insertIndex = anchors.arraySize;
                    }

                    anchors.InsertArrayElementAtIndex(insertIndex);
                    SerializedProperty newEntry = anchors.GetArrayElementAtIndex(insertIndex);
                    newEntry.FindPropertyRelative("layerName").stringValue = "Necklace";
                    newEntry.FindPropertyRelative("localPosition").vector3Value = Vector3.zero;
                    layoutSo.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(layout);
                    changed = true;
                }
            }

            SerializedObject customizerSo = new SerializedObject(customizer);
            SerializedProperty necklaceRendererProperty = customizerSo.FindProperty("necklaceRenderer");
            if (necklaceRendererProperty.objectReferenceValue != necklaceRenderer)
            {
                necklaceRendererProperty.objectReferenceValue = necklaceRenderer;
                changed = true;
            }

            customizerSo.ApplyModifiedPropertiesWithoutUndo();

            customizer.ApplySortingOrders();
            EditorUtility.SetDirty(customizer);
            EditorUtility.SetDirty(necklaceRenderer);
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            }

            AssetDatabase.SaveAssets();
            return necklaceTransform;
        }

        private static bool UpdateSortingOrder(Transform layer, int order)
        {
            if (layer == null)
            {
                return false;
            }

            SpriteRenderer renderer = layer.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sortingOrder != order)
            {
                renderer.sortingOrder = order;
                EditorUtility.SetDirty(renderer);
                return true;
            }

            return false;
        }
    }
}
#endif
