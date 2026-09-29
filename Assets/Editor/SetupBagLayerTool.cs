#if UNITY_EDITOR
using DressUpGame.Character;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class SetupBagLayerTool
    {
        [MenuItem("Dress Up Game/Setup Bag Layer")]
        public static void SetupBagLayer()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Stop Play before setting up the Bag layer.",
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

            Transform girl = customizer.transform;
            Transform bagTransform = girl.Find("Bag");
            SpriteRenderer bagRenderer;

            if (bagTransform == null)
            {
                GameObject bagGo = new GameObject("Bag");
                bagGo.transform.SetParent(girl, false);
                bagTransform = bagGo.transform;
                bagRenderer = bagGo.AddComponent<SpriteRenderer>();
                bagRenderer.enabled = false;
                bagRenderer.sortingOrder = 9;
            }
            else
            {
                bagRenderer = bagTransform.GetComponent<SpriteRenderer>();
                if (bagRenderer == null)
                {
                    bagRenderer = bagTransform.gameObject.AddComponent<SpriteRenderer>();
                    bagRenderer.enabled = false;
                }

                bagRenderer.sortingOrder = 9;
            }

            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            if (layout != null)
            {
                SerializedObject layoutSo = new SerializedObject(layout);
                SerializedProperty anchors = layoutSo.FindProperty("layerAnchors");
                bool hasBagAnchor = false;

                for (int i = 0; i < anchors.arraySize; i++)
                {
                    SerializedProperty entry = anchors.GetArrayElementAtIndex(i);
                    if (entry.FindPropertyRelative("layerName").stringValue == "Bag")
                    {
                        hasBagAnchor = true;
                        break;
                    }
                }

                if (!hasBagAnchor)
                {
                    anchors.InsertArrayElementAtIndex(anchors.arraySize);
                    SerializedProperty newEntry = anchors.GetArrayElementAtIndex(anchors.arraySize - 1);
                    newEntry.FindPropertyRelative("layerName").stringValue = "Bag";
                    newEntry.FindPropertyRelative("localPosition").vector3Value = Vector3.zero;
                    layoutSo.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(layout);
                }
            }

            SerializedObject customizerSo = new SerializedObject(customizer);
            customizerSo.FindProperty("bagRenderer").objectReferenceValue = bagRenderer;
            customizerSo.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(customizer);
            EditorUtility.SetDirty(bagRenderer);
            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = bagTransform.gameObject;
            Debug.Log("Dress Up Game: Bag layer added under Girl.");
        }
    }
}
#endif
