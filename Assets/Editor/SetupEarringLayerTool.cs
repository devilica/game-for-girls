#if UNITY_EDITOR
using DressUpGame.Character;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class SetupEarringLayerTool
    {
        [MenuItem("Dress Up Game/Setup Earrings Layer")]
        public static void SetupEarringsLayer()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before setting up the Earrings layer.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("No Girl", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            Transform earringsTransform = EnsureEarringsLayerInternal(customizer, out bool changed);
            if (earringsTransform == null)
            {
                return;
            }

            if (changed)
            {
                EditorSceneManager.SaveOpenScenes();
            }

            Selection.activeGameObject = earringsTransform.gameObject;
            Debug.Log(changed
                ? "Dress Up Game: Earrings layer added under Girl (scene saved)."
                : "Dress Up Game: Earrings layer already present and wired.");
        }

        public static bool EnsureEarringsLayerInOpenScene()
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

            EnsureEarringsLayerInternal(customizer, out bool changed);
            return changed;
        }

        private static Transform EnsureEarringsLayerInternal(CharacterCustomizer customizer, out bool changed)
        {
            changed = false;
            Transform girl = customizer.transform;
            Transform earringsTransform = girl.Find("Earrings");
            SpriteRenderer earringsRendererComponent;

            if (earringsTransform == null)
            {
                GameObject earringsGo = new GameObject("Earrings");
                Undo.RegisterCreatedObjectUndo(earringsGo, "Create Earrings Layer");
                earringsGo.transform.SetParent(girl, false);
                earringsTransform = earringsGo.transform;
                earringsRendererComponent = earringsGo.AddComponent<SpriteRenderer>();
                earringsRendererComponent.enabled = false;
                earringsRendererComponent.sortingOrder = 9;
                changed = true;
            }
            else
            {
                earringsRendererComponent = earringsTransform.GetComponent<SpriteRenderer>();
                if (earringsRendererComponent == null)
                {
                    earringsRendererComponent = earringsTransform.gameObject.AddComponent<SpriteRenderer>();
                    earringsRendererComponent.enabled = false;
                    changed = true;
                }

                if (earringsRendererComponent.sortingOrder != 9)
                {
                    earringsRendererComponent.sortingOrder = 9;
                    changed = true;
                }
            }

            changed |= UpdateSortingOrder(girl.Find("Eyeshadow"), 4);
            changed |= UpdateSortingOrder(girl.Find("Blush"), 5);
            changed |= UpdateSortingOrder(girl.Find("Hair"), 6);
            changed |= UpdateSortingOrder(girl.Find("Lips"), 7);
            changed |= UpdateSortingOrder(girl.Find("Necklace"), 8);
            changed |= UpdateSortingOrder(girl.Find("Crown"), 10);
            changed |= UpdateSortingOrder(girl.Find("Glasses"), 11);
            changed |= UpdateSortingOrder(girl.Find("Bag"), 12);

            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            if (layout != null)
            {
                SerializedObject layoutSo = new SerializedObject(layout);
                SerializedProperty anchors = layoutSo.FindProperty("layerAnchors");
                bool hasEarringsAnchor = false;

                for (int i = 0; i < anchors.arraySize; i++)
                {
                    if (anchors.GetArrayElementAtIndex(i).FindPropertyRelative("layerName").stringValue == "Earrings")
                    {
                        hasEarringsAnchor = true;
                        break;
                    }
                }

                if (!hasEarringsAnchor)
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
                    newEntry.FindPropertyRelative("layerName").stringValue = "Earrings";
                    newEntry.FindPropertyRelative("localPosition").vector3Value = Vector3.zero;
                    layoutSo.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(layout);
                    changed = true;
                }
            }

            SerializedObject customizerSo = new SerializedObject(customizer);
            SerializedProperty earringsRendererProperty = customizerSo.FindProperty("earringsRenderer");
            if (earringsRendererProperty.objectReferenceValue != earringsRendererComponent)
            {
                earringsRendererProperty.objectReferenceValue = earringsRendererComponent;
                changed = true;
            }

            customizerSo.ApplyModifiedPropertiesWithoutUndo();
            customizer.ApplySortingOrders();
            EditorUtility.SetDirty(customizer);
            EditorUtility.SetDirty(earringsRendererComponent);
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            }

            AssetDatabase.SaveAssets();
            return earringsTransform;
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
