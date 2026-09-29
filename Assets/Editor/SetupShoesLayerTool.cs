#if UNITY_EDITOR
using DressUpGame.Character;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class SetupShoesLayerTool
    {
        [MenuItem("Dress Up Game/Setup Shoes Layer")]
        public static void SetupShoesLayer()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before setting up the Shoes layer.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("No Girl", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            Transform shoesTransform = EnsureShoesLayerInternal(customizer, out bool changed);
            if (shoesTransform == null)
            {
                return;
            }

            if (changed)
            {
                EditorSceneManager.SaveOpenScenes();
            }

            Selection.activeGameObject = shoesTransform.gameObject;
            Debug.Log(changed
                ? "Dress Up Game: Shoes layer added under Girl (scene saved)."
                : "Dress Up Game: Shoes layer already present and wired.");
        }

        public static bool EnsureShoesLayerInOpenScene()
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

            EnsureShoesLayerInternal(customizer, out bool changed);
            return changed;
        }

        private static Transform EnsureShoesLayerInternal(CharacterCustomizer customizer, out bool changed)
        {
            changed = false;
            Transform girl = customizer.transform;
            Transform shoesTransform = girl.Find("Shoes");
            SpriteRenderer shoesRendererComponent;

            if (shoesTransform == null)
            {
                GameObject shoesGo = new GameObject("Shoes");
                Undo.RegisterCreatedObjectUndo(shoesGo, "Create Shoes Layer");
                shoesGo.transform.SetParent(girl, false);
                shoesTransform = shoesGo.transform;
                shoesRendererComponent = shoesGo.AddComponent<SpriteRenderer>();
                shoesRendererComponent.enabled = false;
                shoesRendererComponent.sortingOrder = 3;
                changed = true;
            }
            else
            {
                shoesRendererComponent = shoesTransform.GetComponent<SpriteRenderer>();
                if (shoesRendererComponent == null)
                {
                    shoesRendererComponent = shoesTransform.gameObject.AddComponent<SpriteRenderer>();
                    shoesRendererComponent.enabled = false;
                    changed = true;
                }

                if (shoesRendererComponent.sortingOrder != 3)
                {
                    shoesRendererComponent.sortingOrder = 3;
                    changed = true;
                }
            }

            changed |= UpdateSortingOrder(girl.Find("Eyeshadow"), 4);
            changed |= UpdateSortingOrder(girl.Find("Blush"), 5);
            changed |= UpdateSortingOrder(girl.Find("Hair"), 6);
            changed |= UpdateSortingOrder(girl.Find("Lips"), 7);
            changed |= UpdateSortingOrder(girl.Find("Necklace"), 8);
            changed |= UpdateSortingOrder(girl.Find("Earrings"), 9);
            changed |= UpdateSortingOrder(girl.Find("Crown"), 10);
            changed |= UpdateSortingOrder(girl.Find("Glasses"), 11);
            changed |= UpdateSortingOrder(girl.Find("Bag"), 12);

            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            if (layout != null)
            {
                SerializedObject layoutSo = new SerializedObject(layout);
                SerializedProperty anchors = layoutSo.FindProperty("layerAnchors");
                bool hasShoesAnchor = false;

                for (int i = 0; i < anchors.arraySize; i++)
                {
                    if (anchors.GetArrayElementAtIndex(i).FindPropertyRelative("layerName").stringValue == "Shoes")
                    {
                        hasShoesAnchor = true;
                        break;
                    }
                }

                if (!hasShoesAnchor)
                {
                    int insertIndex = -1;
                    for (int i = 0; i < anchors.arraySize; i++)
                    {
                        if (anchors.GetArrayElementAtIndex(i).FindPropertyRelative("layerName").stringValue == "Hair")
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
                    newEntry.FindPropertyRelative("layerName").stringValue = "Shoes";
                    newEntry.FindPropertyRelative("localPosition").vector3Value = Vector3.zero;
                    layoutSo.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(layout);
                    changed = true;
                }
            }

            SerializedObject customizerSo = new SerializedObject(customizer);
            SerializedProperty shoesRendererProperty = customizerSo.FindProperty("shoesRenderer");
            if (shoesRendererProperty.objectReferenceValue != shoesRendererComponent)
            {
                shoesRendererProperty.objectReferenceValue = shoesRendererComponent;
                changed = true;
            }

            customizerSo.ApplyModifiedPropertiesWithoutUndo();
            customizer.ApplySortingOrders();
            EditorUtility.SetDirty(customizer);
            EditorUtility.SetDirty(shoesRendererComponent);
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            }

            AssetDatabase.SaveAssets();
            return shoesTransform;
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
