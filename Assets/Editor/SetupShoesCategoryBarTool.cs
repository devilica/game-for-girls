#if UNITY_EDITOR
using DressUpGame.Data;
using DressUpGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DressUpGame.Editor
{
    public static class SetupShoesCategoryBarTool
    {
        [MenuItem("Dress Up Game/Setup Shoes Category Tab")]
        public static void SetupShoesCategoryTab()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before updating the category bar.", "OK");
                return;
            }

            CategoryBarController categoryBar = Object.FindAnyObjectByType<CategoryBarController>();
            if (categoryBar == null)
            {
                EditorUtility.DisplayDialog("No Category Bar", "Open MainScene with a CategoryBar first.", "OK");
                return;
            }

            SerializedObject so = new SerializedObject(categoryBar);
            SerializedProperty list = so.FindProperty("categoryButtons");

            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty entry = list.GetArrayElementAtIndex(i);
                Button btn = entry.FindPropertyRelative("button").objectReferenceValue as Button;
                if (btn != null && btn.name == "AccessoriesButton")
                {
                    entry.FindPropertyRelative("category").enumValueIndex = (int)CustomizationCategory.Accessories;
                }
            }

            if (!HasShoesTab(list))
            {
                Transform bar = categoryBar.transform;
                Transform dressesButton = bar.Find("DressesButton");
                Transform accessoriesButton = bar.Find("AccessoriesButton");
                if (dressesButton == null || accessoriesButton == null)
                {
                    EditorUtility.DisplayDialog("Missing Buttons", "Expected DressesButton and AccessoriesButton under CategoryBar.", "OK");
                    return;
                }

                GameObject shoesButtonGo = Object.Instantiate(dressesButton.gameObject, bar);
                shoesButtonGo.name = "ShoesButton";
                shoesButtonGo.transform.SetSiblingIndex(accessoriesButton.GetSiblingIndex());

                Text label = shoesButtonGo.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = "SHOES";
                    label.fontSize = 22;
                }

                int insertIndex = list.arraySize;
                for (int i = 0; i < list.arraySize; i++)
                {
                    Button btn = list.GetArrayElementAtIndex(i).FindPropertyRelative("button").objectReferenceValue as Button;
                    if (btn != null && btn.name == "AccessoriesButton")
                    {
                        insertIndex = i;
                        break;
                    }
                }

                list.InsertArrayElementAtIndex(insertIndex);
                SerializedProperty shoesEntry = list.GetArrayElementAtIndex(insertIndex);
                shoesEntry.FindPropertyRelative("category").enumValueIndex = (int)CustomizationCategory.Shoes;
                shoesEntry.FindPropertyRelative("button").objectReferenceValue = shoesButtonGo.GetComponent<Button>();
                shoesEntry.FindPropertyRelative("background").objectReferenceValue = shoesButtonGo.GetComponent<Image>();
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(categoryBar);
            EditorSceneManager.MarkSceneDirty(categoryBar.gameObject.scene);
            Debug.Log("Dress Up Game: SHOES category tab configured on CategoryBar.");
        }

        private static bool HasShoesTab(SerializedProperty list)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("category").enumValueIndex == (int)CustomizationCategory.Shoes)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif
