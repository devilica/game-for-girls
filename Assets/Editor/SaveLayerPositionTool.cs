#if UNITY_EDITOR
using System.Text.RegularExpressions;
using DressUpGame.Character;
using DressUpGame.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DressUpGame.Editor
{
    /// <summary>
    /// Saves layer positions from the scene into ScriptableObject items and resets corrupted transforms.
    /// </summary>
    public static class SaveLayerPositionTool
    {
        public static void AutoSyncLayerPositions()
        {
            int blushCount = SyncAllBlushLayouts();
            int eyeshadowCount = SyncAllEyeshadowLayouts();
            int lipstickCount = SyncAllLipstickShapeLayouts();
            int necklaceCount = SyncAllNecklaceShapeLayouts();
            int earringsCount = SyncAllEarringShapeLayouts();
            int crownCount = SyncAllCrownShapeLayouts();
            int glassesCount = SyncAllGlassesShapeLayouts();
            int dressCount = SyncAllDressShapeLayouts();
            int shoesCount = SyncAllShoesShapeLayouts();

            AssetDatabase.SaveAssets();

            string summary =
                $"Blush: {blushCount} item(s)\n" +
                $"Eyeshadow: {eyeshadowCount} item(s)\n" +
                $"Lipstick: {lipstickCount} item(s)\n" +
                $"Necklace: {necklaceCount} item(s)\n" +
                $"Earrings: {earringsCount} item(s)\n" +
                $"Crown: {crownCount} item(s)\n" +
                $"Glasses: {glassesCount} item(s)\n" +
                $"Dresses: {dressCount} item(s)\n" +
                $"Shoes: {shoesCount} item(s)";

            Debug.Log($"Dress Up Game: Auto-synced layer layouts.\n{summary}");
            EditorUtility.DisplayDialog("Auto-Sync Complete",
                "Copied reference layouts to all color/shape variants:\n\n" + summary,
                "OK");
        }

        [MenuItem("Dress Up Game/Reset Layer Positions")]
        public static void ResetLayerPositions()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before resetting layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            if (layout == null)
            {
                layout = customizer.gameObject.AddComponent<CharacterLayerLayout>();
            }

            customizer.transform.localPosition = Vector3.zero;
            customizer.transform.localScale = Vector3.one;

            string[] layerNames = { "Body", "Eyes", "Dress", "Shoes", "Hair", "Blush", "Eyeshadow", "Lips", "Necklace", "Earrings", "Crown", "Glasses", "Bag" };
            foreach (string layerName in layerNames)
            {
                Transform layer = FindLayerTransform(customizer.transform, layerName);
                if (layer == null)
                {
                    continue;
                }

                layer.localPosition = layout.GetBasePosition(layerName);
                layer.localScale = Vector3.one;
            }

            ResetAllItemOffsets<HairItem>("Assets/Resources/Items/Hair");
            ResetAllItemOffsets<DressItem>("Assets/Resources/Items/Dresses");
            ResetAllShoesOffsets("Assets/Resources/Items/Shoes");
            ResetAllMakeupOffsets("Assets/Resources/Items/Makeup", MakeupType.Lipstick);
            ResetAllMakeupOffsets("Assets/Resources/Items/Makeup", MakeupType.Eyes);
            ResetAllMakeupOffsets("Assets/Resources/Items/Makeup", MakeupType.Eyeshadow);
            ResetAllMakeupOffsets("Assets/Resources/Items/Makeup", MakeupType.Blush);
            ResetAllNecklaceOffsets("Assets/Resources/Items/Accessories");
            ResetAllEarringOffsets("Assets/Resources/Items/Accessories");
            ResetAllCrownOffsets("Assets/Resources/Items/Accessories");
            ResetAllGlassesOffsets("Assets/Resources/Items/Accessories");
            ResetAllBagOffsets("Assets/Resources/Items/Accessories");

            EditorUtility.SetDirty(layout);
            EditorUtility.SetDirty(customizer);
            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log("Dress Up Game: Layer positions and item offsets reset.");
            EditorUtility.DisplayDialog("Reset Complete",
                "All layers reset to base position (0,0,0) with scale (1,1,1).\n\n" +
                "Hair, Dress, Lipstick, Eyes, Eyeshadow, Blush, Crown, Glasses, and Bag item offsets cleared.\n\n" +
                "Press Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Preview Hair Item In Scene")]
        public static void PreviewHairItemInScene()
        {
            PreviewHairItem(Selection.activeObject as HairItem, "Hair");
        }

        [MenuItem("Dress Up Game/Preview Dress Item In Scene")]
        public static void PreviewDressItemInScene()
        {
            PreviewDressItem(Selection.activeObject as DressItem, "Dress");
        }

        [MenuItem("Dress Up Game/Preview Shoe Item In Scene")]
        public static void PreviewShoeItemInScene()
        {
            PreviewShoeItem(Selection.activeObject as ShoeItem, "Shoes");
        }

        [MenuItem("Dress Up Game/Preview Eye Item In Scene")]
        public static void PreviewEyeItemInScene()
        {
            PreviewMakeupItem(Selection.activeObject as MakeupItem, "Eyes", "eye", applyEyeTint: true);
        }

        [MenuItem("Dress Up Game/Preview Lipstick Item In Scene")]
        public static void PreviewLipstickItemInScene()
        {
            PreviewMakeupItem(Selection.activeObject as MakeupItem, "Lips", "lipstick");
        }

        [MenuItem("Dress Up Game/Preview Eyeshadow Item In Scene")]
        public static void PreviewEyeshadowItemInScene()
        {
            PreviewMakeupItem(Selection.activeObject as MakeupItem, "Eyeshadow", "eyeshadow");
        }

        [MenuItem("Dress Up Game/Preview Blush Item In Scene")]
        public static void PreviewBlushItemInScene()
        {
            PreviewMakeupItem(Selection.activeObject as MakeupItem, "Blush", "blush");
        }

        [MenuItem("Dress Up Game/Preview Necklace Item In Scene")]
        public static void PreviewNecklaceItemInScene()
        {
            PreviewAccessoryItem(Selection.activeObject as AccessoryItem, "Necklace", "necklace");
        }

        [MenuItem("Dress Up Game/Preview Earring Item In Scene")]
        public static void PreviewEarringItemInScene()
        {
            PreviewAccessoryItem(Selection.activeObject as AccessoryItem, "Earrings", "ear");
        }

        [MenuItem("Dress Up Game/Preview Crown Item In Scene")]
        public static void PreviewCrownItemInScene()
        {
            PreviewAccessoryItem(Selection.activeObject as AccessoryItem, "Crown", "crown");
        }

        [MenuItem("Dress Up Game/Preview Glasses Item In Scene")]
        public static void PreviewGlassesItemInScene()
        {
            PreviewAccessoryItem(Selection.activeObject as AccessoryItem, "Glasses", "glasses");
        }

        [MenuItem("Dress Up Game/Preview Bag Item In Scene")]
        public static void PreviewBagItemInScene()
        {
            PreviewAccessoryItem(Selection.activeObject as AccessoryItem, "Bag", "bag");
        }

        [MenuItem("Dress Up Game/Save Hair Position To Hair Item")]
        public static void SaveHairPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Stop Play Mode First",
                    "Unity does not keep scene changes made while the game is running.\n\n" +
                    "1. Stop Play (press Play button again)\n" +
                    "2. Move Girl → Hair in the Scene view\n" +
                    "3. Run this menu again",
                    "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform hairTransform = customizer.transform.Find("Hair");
            if (hairTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Hair in the scene.", "OK");
                return;
            }

            HairItem hairItem = ResolveHairItem(hairTransform);
            if (hairItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Hair Item",
                    "Could not find which hair style to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select Hair_1.asset in Assets/Resources/Items/Hair/\n" +
                    "• Or select hair_1.png in Assets/Sprites/Character/Hair/\n" +
                    "• Or assign the sprite on Girl → Hair first\n\n" +
                    "Note: hair_1.png is the picture. Hair_1.asset is the game item that stores position.",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, hairTransform);
            Vector3 scale = hairTransform.localScale;
            hairItem.SetLayerLayout(offset, scale);
            // Keep the same look in Scene view — only the .asset file stores the offset for Play mode.
            ApplyLayerLayoutPreview(customizer, hairTransform, offset, scale);

            EditorUtility.SetDirty(hairItem);
            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved hair layout to {hairItem.name}. Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {hairItem.name}:\n\nOffset: {offset}\nScale: {scale}\n\n" +
                "Hair should stay where you put it.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Lipstick Position To Lipstick Item")]
        public static void SaveLipstickPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform lipsTransform = customizer.transform.Find("Lips");
            if (lipsTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Lips in the scene.", "OK");
                return;
            }

            MakeupItem lipstickItem = ResolveLipstickItem(lipsTransform);
            if (lipstickItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Lipstick Item",
                    "Could not find which lipstick to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select lipstick_1_01.asset in Assets/Resources/Items/Makeup/\n" +
                    "• Or select lipstic_1.png in Assets/Sprites/Character/Makeup/\n" +
                    "• Or assign the sprite on Girl → Lips first",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, lipsTransform);
            Vector3 scale = lipsTransform.localScale;
            int updatedCount = ApplyLipstickLayoutToShapeVariants(lipstickItem, offset, scale);
            ApplyLayerLayoutPreview(customizer, lipsTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved lipstick layout to {updatedCount} lipstick item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} lipstick item(s) (same shape as {lipstickItem.name}):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Lips should stay where you put them.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Eyes Position To Eye Item")]
        public static void SaveEyesPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform eyesTransform = customizer.transform.Find("Eyes");
            if (eyesTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Eyes in the scene.", "OK");
                return;
            }

            MakeupItem eyeItem = ResolveEyeItem(eyesTransform);
            if (eyeItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Eye Item",
                    "Could not find which eye style to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select eyes_2_01.asset in Assets/Resources/Items/Makeup/\n" +
                    "• Or select eyes_2.png in Assets/Sprites/Character/Eyes/\n" +
                    "• Or assign the sprite on Girl → Eyes first",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, eyesTransform);
            Vector3 scale = eyesTransform.localScale;
            int updatedCount = ApplyEyeLayoutToAllColorVariants(eyeItem, offset, scale);
            ApplyLayerLayoutPreview(customizer, eyesTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved eyes layout to {updatedCount} eye item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} eye color item(s) (same shape as {eyeItem.name}):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Eyes should stay where you put them.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Eyeshadow Position To Eyeshadow Item")]
        public static void SaveEyeshadowPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform eyeshadowTransform = customizer.transform.Find("Eyeshadow");
            if (eyeshadowTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Eyeshadow in the scene.", "OK");
                return;
            }

            MakeupItem eyeshadowItem = ResolveEyeshadowItem(eyeshadowTransform);
            if (eyeshadowItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Eyeshadow Item",
                    "Could not find which eyeshadow to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select eyeshadow_2_01.asset in Assets/Resources/Items/Makeup/\n" +
                    "• Or select eyeshadow_2.png in Assets/Sprites/Character/Makeup/\n" +
                    "• Or assign the sprite on Girl → Eyeshadow first",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, eyeshadowTransform);
            Vector3 scale = eyeshadowTransform.localScale;
            int updatedCount = SyncAllEyeshadowLayouts(offset, scale);
            ApplyLayerLayoutPreview(customizer, eyeshadowTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved eyeshadow layout to {updatedCount} eyeshadow item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} eyeshadow item(s):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Eyeshadow should stay where you put it.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Blush Position To Blush Item")]
        public static void SaveBlushPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform blushTransform = customizer.transform.Find("Blush");
            if (blushTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Blush in the scene.", "OK");
                return;
            }

            MakeupItem blushItem = ResolveBlushItem(blushTransform);
            if (blushItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Blush Item",
                    "Could not find which blush to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select blush_2_01.asset in Assets/Resources/Items/Makeup/\n" +
                    "• Or select blush_2.png in Assets/Sprites/Character/Makeup/\n" +
                    "• Or assign the sprite on Girl → Blush first",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, blushTransform);
            Vector3 scale = blushTransform.localScale;
            int updatedCount = SyncAllBlushLayouts(offset, scale);
            ApplyLayerLayoutPreview(customizer, blushTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved blush layout to {updatedCount} blush item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} blush item(s):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Blush should stay where you put it.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Necklace Position To Necklace Item")]
        public static void SaveNecklacePosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform necklaceTransform = FindLayerTransform(customizer.transform, "Necklace");
            if (necklaceTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Necklace in the scene. Run Dress Up Game → Setup Necklace Layer.", "OK");
                return;
            }

            AccessoryItem necklaceItem = ResolveNecklaceItem(necklaceTransform);
            if (necklaceItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Necklace Item",
                    "Could not find which necklace to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select necklace_1_01.asset in Assets/Resources/Items/Accessories/\n" +
                    "• Or select necklace_1.png in Assets/Sprites/Character/Necklace/\n" +
                    "• Or assign the sprite on Girl → Necklace first",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, necklaceTransform);
            Vector3 scale = necklaceTransform.localScale;
            int updatedCount = ApplyNecklaceLayoutToShapeVariants(necklaceItem, offset, scale);
            ApplyLayerLayoutPreview(customizer, necklaceTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved necklace layout to {updatedCount} necklace item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} necklace item(s) (same shape as {necklaceItem.name}):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Necklace should stay where you put it.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Earrings Position To Earring Item")]
        public static void SaveEarringsPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform earringsTransform = FindLayerTransform(customizer.transform, "Earrings");
            if (earringsTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Earrings. Run Dress Up Game → Setup Earrings Layer.", "OK");
                return;
            }

            AccessoryItem earringItem = ResolveEarringItem(earringsTransform);
            if (earringItem == null && !EarringCatalogItemsExist())
            {
                GenerateEarringColorsTool.GenerateEarringColors();
                SpriteAssetLinker.ApplySpritesFromFolder();
                earringItem = ResolveEarringItem(earringsTransform);
            }

            if (earringItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Earring Item",
                    "Could not find which earring shape to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Run Dress Up Game → Generate Earring Colors (creates ear_1, ear_1_01, …)\n" +
                    "• Select ear_6.png (or ear_6.asset) in the Project window, then run Save again\n" +
                    "• Or put a sprite on Girl → Earrings in the scene, then run Save again",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, earringsTransform);
            Vector3 scale = earringsTransform.localScale;
            int updatedCount = ApplyEarringLayoutToShapeVariants(earringItem, offset, scale);
            ApplyLayerLayoutPreview(customizer, earringsTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved earring layout to {updatedCount} earring item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} earring item(s) (same shape as {earringItem.name}):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Earrings should stay where you put it.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Crown Position To Crown Item")]
        public static void SaveCrownPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform crownTransform = FindLayerTransform(customizer.transform, "Crown");
            if (crownTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Crown in the scene.", "OK");
                return;
            }

            AccessoryItem crownItem = ResolveCrownItem(crownTransform);
            if (crownItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Crown Item",
                    "Could not find which crown to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select crown_1_01.asset in Assets/Resources/Items/Accessories/\n" +
                    "• Or select crown_1.png in Assets/Sprites/Character/Crown/\n" +
                    "• Or assign the sprite on Girl → Crown first",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, crownTransform);
            Vector3 scale = crownTransform.localScale;
            int updatedCount = ApplyCrownLayoutToShapeVariants(crownItem, offset, scale);
            ApplyLayerLayoutPreview(customizer, crownTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved crown layout to {updatedCount} crown item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} crown item(s) (same shape as {crownItem.name}):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Crown should stay where you put it.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Glasses Position To Glasses Item")]
        public static void SaveGlassesPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform glassesTransform = FindLayerTransform(customizer.transform, "Glasses");
            if (glassesTransform == null)
            {
                EditorUtility.DisplayDialog(
                    "Not Found",
                    "Could not find Girl → Glasses in the scene.\n\n" +
                    "Run Dress Up Game → Reload Main Scene if you just added the layer.",
                    "OK");
                return;
            }

            AccessoryItem glassesItem = ResolveGlassesItem(glassesTransform);
            if (glassesItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Glasses Item",
                    "Could not find which glasses to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select glasses_1.asset in Assets/Resources/Items/Accessories/\n" +
                    "• Or select glasses_1.png in Assets/Sprites/Character/Glasses/\n" +
                    "• Or assign the sprite on Girl → Glasses first",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, glassesTransform);
            Vector3 scale = glassesTransform.localScale;
            int updatedCount = ApplyGlassesLayoutToShapeVariants(glassesItem, offset, scale);
            ApplyLayerLayoutPreview(customizer, glassesTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved glasses layout to {updatedCount} glasses item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} glasses item(s) (same shape as {glassesItem.name}):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Glasses should stay where you put them.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Bag Position To Bag Item")]
        public static void SaveBagPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform bagTransform = FindLayerTransform(customizer.transform, "Bag");
            if (bagTransform == null)
            {
                EditorUtility.DisplayDialog(
                    "Not Found",
                    "Could not find Girl → Bag in the scene.\n\n" +
                    "Run Dress Up Game → Reload Main Scene if you just added the layer.",
                    "OK");
                return;
            }

            AccessoryItem bagItem = ResolveBagItem(bagTransform);
            if (bagItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Bag Item",
                    "Could not find which bag to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select bag_1.asset in Assets/Resources/Items/Accessories/\n" +
                    "• Or select bag_1.png in Assets/Sprites/Character/Bags/\n" +
                    "• Or assign the sprite on Girl → Bag first",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, bagTransform);
            Vector3 scale = bagTransform.localScale;
            int updatedCount = ApplyBagLayoutToAllVariants(bagItem, offset, scale);
            ApplyLayerLayoutPreview(customizer, bagTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved bag layout to {updatedCount} bag item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} bag item(s) (same shape as {bagItem.name}):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Bag should stay where you put it.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Shoes Position To Shoe Item")]
        public static void SaveShoesPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Not Found", "No Girl with CharacterCustomizer found in the open scene.", "OK");
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform shoesTransform = FindLayerTransform(customizer.transform, "Shoes");
            if (shoesTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Shoes. Run Dress Up Game → Setup Shoes Layer.", "OK");
                return;
            }

            ShoeItem shoeItem = ResolveShoeItem(shoesTransform);
            if (shoeItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Shoe Item",
                    "Could not find which shoe to save to.\n\n" +
                    "• Select shoes_1.asset or shoes_1_01.asset in Assets/Resources/Items/Shoes/\n" +
                    "• Or select shoes_1.png in Assets/Sprites/Character/Shoes/",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, shoesTransform);
            Vector3 scale = shoesTransform.localScale;
            int updatedCount = ApplyShoesLayoutToShapeVariants(shoeItem, offset, scale);
            ApplyLayerLayoutPreview(customizer, shoesTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved shoes layout to {updatedCount} shoe item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} shoe item(s) (same shape as {shoeItem.name}):\n\n" +
                $"Offset: {offset}\nScale: {scale}",
                "OK");
        }

        [MenuItem("Dress Up Game/Save Dress Position To Dress Item")]
        public static void SaveDressPosition()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode First", "Stop Play before saving layer positions.", "OK");
                return;
            }

            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                return;
            }

            if (!EnsureBodyAtBase(customizer))
            {
                return;
            }

            Transform dressTransform = customizer.transform.Find("Dress");
            if (dressTransform == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find Girl → Dress in the scene.", "OK");
                return;
            }

            DressItem dressItem = ResolveDressItem(dressTransform);
            if (dressItem == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Dress Item",
                    "Could not find which dress to save to.\n\n" +
                    "Do ONE of these:\n" +
                    "• Select Dress_1.asset in Assets/Resources/Items/Dresses/\n" +
                    "• Or select the dress PNG in Assets/Sprites/Character/Dress/\n" +
                    "• Or assign the sprite on Girl → Dress first",
                    "OK");
                return;
            }

            Vector3 offset = CalculateRelativeOffset(customizer, dressTransform);
            Vector3 scale = dressTransform.localScale;
            int updatedCount = ApplyDressLayoutToShapeVariants(dressItem, offset, scale);
            ApplyLayerLayoutPreview(customizer, dressTransform, offset, scale);

            EditorSceneManager.MarkSceneDirty(customizer.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved dress layout to {updatedCount} dress item(s). Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog("Saved",
                $"Saved to {updatedCount} dress item(s) (same shape as {dressItem.name}):\n\n" +
                $"Offset: {offset}\nScale: {scale}\n\n" +
                "Dress should stay where you put it.\nPress Play to test, then Ctrl+S to save the scene.",
                "OK");
        }

        private static HairItem ResolveHairItem(Transform hairTransform)
        {
            if (Selection.activeObject is HairItem selectedHairItem)
            {
                return selectedHairItem;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                HairItem fromSprite = FindHairItemBySprite(spriteFromSelection);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer hairRenderer = hairTransform.GetComponent<SpriteRenderer>();
            if (hairRenderer != null && hairRenderer.sprite != null)
            {
                return FindHairItemBySprite(hairRenderer.sprite);
            }

            return null;
        }

        private static MakeupItem ResolveBlushItem(Transform blushTransform)
        {
            if (Selection.activeObject is MakeupItem selectedMakeup
                && selectedMakeup.MakeupType == MakeupType.Blush)
            {
                return selectedMakeup;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                MakeupItem fromSprite = FindMakeupItemBySprite(spriteFromSelection, MakeupType.Blush);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer blushRenderer = blushTransform.GetComponent<SpriteRenderer>();
            if (blushRenderer != null && blushRenderer.sprite != null)
            {
                return FindMakeupItemBySprite(blushRenderer.sprite, MakeupType.Blush);
            }

            return null;
        }

        private static MakeupItem ResolveEyeItem(Transform eyesTransform)
        {
            if (Selection.activeObject is MakeupItem selectedMakeup
                && selectedMakeup.MakeupType == MakeupType.Eyes)
            {
                return selectedMakeup;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                MakeupItem fromSprite = FindMakeupItemBySprite(spriteFromSelection, MakeupType.Eyes);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer eyesRenderer = eyesTransform.GetComponent<SpriteRenderer>();
            if (eyesRenderer != null && eyesRenderer.sprite != null)
            {
                return FindMakeupItemBySprite(eyesRenderer.sprite, MakeupType.Eyes);
            }

            return null;
        }

        private static MakeupItem ResolveEyeshadowItem(Transform eyeshadowTransform)
        {
            if (Selection.activeObject is MakeupItem selectedMakeup
                && selectedMakeup.MakeupType == MakeupType.Eyeshadow)
            {
                return selectedMakeup;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                MakeupItem fromSprite = FindMakeupItemBySprite(spriteFromSelection, MakeupType.Eyeshadow);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer eyeshadowRenderer = eyeshadowTransform.GetComponent<SpriteRenderer>();
            if (eyeshadowRenderer != null && eyeshadowRenderer.sprite != null)
            {
                return FindMakeupItemBySprite(eyeshadowRenderer.sprite, MakeupType.Eyeshadow);
            }

            return null;
        }

        private static MakeupItem ResolveLipstickItem(Transform lipsTransform)
        {
            if (Selection.activeObject is MakeupItem selectedMakeup
                && selectedMakeup.MakeupType == MakeupType.Lipstick)
            {
                return selectedMakeup;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                MakeupItem fromSprite = FindMakeupItemBySprite(spriteFromSelection, MakeupType.Lipstick);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer lipsRenderer = lipsTransform.GetComponent<SpriteRenderer>();
            if (lipsRenderer != null && lipsRenderer.sprite != null)
            {
                return FindMakeupItemBySprite(lipsRenderer.sprite, MakeupType.Lipstick);
            }

            return null;
        }

        private static AccessoryItem ResolveBagItem(Transform bagTransform)
        {
            if (Selection.activeObject is AccessoryItem selectedAccessory && selectedAccessory.IsBagVariant)
            {
                return selectedAccessory;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                AccessoryItem fromSprite = FindBagItemBySprite(spriteFromSelection);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer bagRenderer = bagTransform.GetComponent<SpriteRenderer>();
            if (bagRenderer != null && bagRenderer.sprite != null)
            {
                return FindBagItemBySprite(bagRenderer.sprite);
            }

            return null;
        }

        private static AccessoryItem ResolveGlassesItem(Transform glassesTransform)
        {
            if (Selection.activeObject is AccessoryItem selectedAccessory && selectedAccessory.IsGlassesVariant)
            {
                return selectedAccessory;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                AccessoryItem fromSprite = FindGlassesItemBySprite(spriteFromSelection);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer glassesRenderer = glassesTransform.GetComponent<SpriteRenderer>();
            if (glassesRenderer != null && glassesRenderer.sprite != null)
            {
                return FindGlassesItemBySprite(glassesRenderer.sprite);
            }

            return null;
        }

        private static AccessoryItem ResolveNecklaceItem(Transform necklaceTransform)
        {
            if (Selection.activeObject is AccessoryItem selectedAccessory && selectedAccessory.IsNecklaceVariant)
            {
                return selectedAccessory;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                AccessoryItem fromSprite = FindNecklaceItemBySprite(spriteFromSelection);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer necklaceRenderer = necklaceTransform.GetComponent<SpriteRenderer>();
            if (necklaceRenderer != null && necklaceRenderer.sprite != null)
            {
                return FindNecklaceItemBySprite(necklaceRenderer.sprite);
            }

            return null;
        }

        private static AccessoryItem ResolveEarringItem(Transform earringsTransform)
        {
            if (Selection.activeObject is AccessoryItem selectedAccessory && selectedAccessory.IsEarringVariant)
            {
                return selectedAccessory;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                AccessoryItem fromSprite = FindEarringItemBySprite(spriteFromSelection);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer earringsRenderer = earringsTransform.GetComponent<SpriteRenderer>();
            if (earringsRenderer != null && earringsRenderer.sprite != null)
            {
                return FindEarringItemBySprite(earringsRenderer.sprite);
            }

            return null;
        }

        private static AccessoryItem ResolveCrownItem(Transform crownTransform)
        {
            if (Selection.activeObject is AccessoryItem selectedAccessory && selectedAccessory.IsCrownVariant)
            {
                return selectedAccessory;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                AccessoryItem fromSprite = FindCrownItemBySprite(spriteFromSelection);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer crownRenderer = crownTransform.GetComponent<SpriteRenderer>();
            if (crownRenderer != null && crownRenderer.sprite != null)
            {
                return FindCrownItemBySprite(crownRenderer.sprite);
            }

            return null;
        }

        private static DressItem ResolveDressItem(Transform dressTransform)
        {
            if (Selection.activeObject is DressItem selectedDressItem)
            {
                return selectedDressItem;
            }

            Sprite spriteFromSelection = GetSpriteFromSelection();
            if (spriteFromSelection != null)
            {
                DressItem fromSprite = FindDressItemBySprite(spriteFromSelection);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer dressRenderer = dressTransform.GetComponent<SpriteRenderer>();
            if (dressRenderer != null && dressRenderer.sprite != null)
            {
                return FindDressItemBySprite(dressRenderer.sprite);
            }

            return null;
        }

        private static Sprite GetSpriteFromSelection()
        {
            if (Selection.activeObject is Sprite sprite)
            {
                return sprite;
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            Sprite fromPath = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (fromPath != null)
            {
                return fromPath;
            }

            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object subAsset in subAssets)
            {
                if (subAsset is Sprite subSprite)
                {
                    return subSprite;
                }
            }

            return null;
        }

        private static HairItem FindHairItemBySprite(Sprite sprite)
        {
            string[] guids = AssetDatabase.FindAssets("t:HairItem", new[] { "Assets/Resources/Items/Hair" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                HairItem item = AssetDatabase.LoadAssetAtPath<HairItem>(path);
                if (item != null && item.Sprite == sprite)
                {
                    return item;
                }
            }

            return null;
        }

        private static DressItem FindDressItemBySprite(Sprite sprite)
        {
            DressItem fallback = null;
            string[] guids = AssetDatabase.FindAssets("t:DressItem", new[] { "Assets/Resources/Items/Dresses" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DressItem item = AssetDatabase.LoadAssetAtPath<DressItem>(path);
                if (item == null || item.Sprite != sprite)
                {
                    continue;
                }

                if (Regex.IsMatch(item.Id ?? string.Empty, @"^dress_\d+$"))
                {
                    return item;
                }

                fallback ??= item;
            }

            return fallback;
        }

        private static AccessoryItem FindNecklaceItemBySprite(Sprite sprite)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { "Assets/Resources/Items/Accessories" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.IsNecklaceVariant && item.Sprite == sprite)
                {
                    return item;
                }
            }

            return null;
        }

        private static AccessoryItem FindEarringItemBySprite(Sprite sprite)
        {
            if (sprite == null)
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { "Assets/Resources/Items/Accessories" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.IsEarringVariant && item.Sprite == sprite)
                {
                    return item;
                }
            }

            return LoadEarringItemForShapeSprite(sprite);
        }

        private static AccessoryItem LoadEarringItemForShapeSprite(Sprite sprite)
        {
            int? shapeIndex = TryParseEarringShapeIndexFromSprite(sprite);
            if (!shapeIndex.HasValue)
            {
                return null;
            }

            string shapeId = $"ear_{shapeIndex.Value}";
            return LoadAccessoryReference(shapeId) ?? LoadAccessoryReference($"{shapeId}_01");
        }

        private static int? TryParseEarringShapeIndexFromSprite(Sprite sprite)
        {
            string assetPath = AssetDatabase.GetAssetPath(sprite);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            Match match = Regex.Match(assetPath, @"[/\\]ear_(\d+)\.png$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                match = Regex.Match(sprite.name ?? string.Empty, @"^ear_(\d+)$", RegexOptions.IgnoreCase);
            }

            if (!match.Success)
            {
                return null;
            }

            return int.Parse(match.Groups[1].Value);
        }

        private static bool EarringCatalogItemsExist()
        {
            return LoadAccessoryReference("earring_none") != null
                || LoadAccessoryReference("ear_1") != null
                || LoadAccessoryReference("ear_1_01") != null;
        }

        private static AccessoryItem FindCrownItemBySprite(Sprite sprite)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { "Assets/Resources/Items/Accessories" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.IsCrownVariant && item.Sprite == sprite)
                {
                    return item;
                }
            }

            return null;
        }

        private static AccessoryItem FindGlassesItemBySprite(Sprite sprite)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { "Assets/Resources/Items/Accessories" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.IsGlassesVariant && item.Sprite == sprite)
                {
                    return item;
                }
            }

            return null;
        }

        private static AccessoryItem FindBagItemBySprite(Sprite sprite)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { "Assets/Resources/Items/Accessories" });
            AccessoryItem fallback = null;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item == null || !item.IsBagVariant || item.Sprite != sprite)
                {
                    continue;
                }

                if (Regex.IsMatch(item.Id ?? string.Empty, @"^bag_\d+$"))
                {
                    return item;
                }

                fallback ??= item;
            }

            return fallback;
        }

        private static int SyncAllBlushLayouts(Vector3? offset = null, Vector3? scale = null)
        {
            MakeupItem reference = LoadMakeupReference("blush_2_01", MakeupType.Blush);
            if (reference == null)
            {
                return 0;
            }

            Vector3 resolvedOffset = offset ?? reference.LayerOffset;
            Vector3 resolvedScale = scale ?? reference.LayerScale;
            return ApplyMakeupLayoutToIdPrefix("blush_2", MakeupType.Blush, resolvedOffset, resolvedScale, skipNoneId: "blush_none");
        }

        private static int SyncAllEyeshadowLayouts(Vector3? offset = null, Vector3? scale = null)
        {
            MakeupItem reference = LoadMakeupReference("eyeshadow_2_01", MakeupType.Eyeshadow);
            if (reference == null)
            {
                return 0;
            }

            Vector3 resolvedOffset = offset ?? reference.LayerOffset;
            Vector3 resolvedScale = scale ?? reference.LayerScale;
            return ApplyMakeupLayoutToIdPrefix("eyeshadow_2", MakeupType.Eyeshadow, resolvedOffset, resolvedScale, skipNoneId: "eyeshadow_none");
        }

        private static int SyncAllLipstickShapeLayouts()
        {
            int updatedCount = 0;
            updatedCount += ApplyLipstickLayoutToShapeVariants(LoadMakeupReference("lipstick_1_01", MakeupType.Lipstick), null, null);
            updatedCount += ApplyLipstickLayoutToShapeVariants(LoadMakeupReference("lipstick_2_01", MakeupType.Lipstick), null, null);

            MakeupItem fallback = LoadMakeupReference("lipstick_1_01", MakeupType.Lipstick);
            if (fallback != null)
            {
                updatedCount += ApplyMakeupLayoutToExactIds(
                    new[] { "lipstick_3", "lipstick_4", "lipstick_5" },
                    MakeupType.Lipstick,
                    fallback.LayerOffset,
                    fallback.LayerScale);
            }

            return updatedCount;
        }

        private static int SyncAllNecklaceShapeLayouts()
        {
            int updatedCount = 0;
            updatedCount += ApplyNecklaceLayoutToShapeVariants(
                LoadAccessoryReference("necklace_1") ?? LoadAccessoryReference("necklace_1_01"), null, null);
            updatedCount += ApplyNecklaceLayoutToShapeVariants(
                LoadAccessoryReference("necklace_2") ?? LoadAccessoryReference("necklace_2_01"), null, null);
            updatedCount += ApplyNecklaceLayoutToShapeVariants(
                LoadAccessoryReference("necklace_3") ?? LoadAccessoryReference("necklace_3_01"), null, null);
            updatedCount += ApplyNecklaceLayoutToShapeVariants(
                LoadAccessoryReference("necklace_4") ?? LoadAccessoryReference("necklace_4_01"), null, null);
            return updatedCount;
        }

        private static int SyncAllEarringShapeLayouts()
        {
            int updatedCount = 0;
            for (int shape = 1; shape <= 6; shape++)
            {
                updatedCount += ApplyEarringLayoutToShapeVariants(
                    LoadAccessoryReference($"ear_{shape}") ?? LoadAccessoryReference($"ear_{shape}_01"),
                    null,
                    null);
            }

            return updatedCount;
        }

        private static int SyncAllCrownShapeLayouts()
        {
            int updatedCount = 0;
            updatedCount += ApplyCrownLayoutToShapeVariants(LoadAccessoryReference("crown_1_01"), null, null);
            updatedCount += ApplyCrownLayoutToShapeVariants(LoadAccessoryReference("crown_2_01"), null, null);
            return updatedCount;
        }

        private static int SyncAllGlassesShapeLayouts()
        {
            int updatedCount = 0;
            updatedCount += ApplyGlassesLayoutToShapeVariants(LoadAccessoryReference("glasses_1"), null, null);
            updatedCount += ApplyGlassesLayoutToShapeVariants(LoadAccessoryReference("glasses_2_01"), null, null);
            return updatedCount;
        }

        private static int SyncAllDressShapeLayouts()
        {
            int updatedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:DressItem", new[] { "Assets/Resources/Items/Dresses" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DressItem item = AssetDatabase.LoadAssetAtPath<DressItem>(path);
                if (item == null || !Regex.IsMatch(item.Id ?? string.Empty, @"^dress_\d+$"))
                {
                    continue;
                }

                updatedCount += ApplyDressLayoutToShapeVariants(item, null, null);
            }

            return updatedCount;
        }

        private static int SyncAllShoesShapeLayouts()
        {
            int updatedCount = 0;
            updatedCount += ApplyShoesLayoutToShapeVariants(LoadShoeReference("shoes_1") ?? LoadShoeReference("shoes_1_01"), null, null);
            updatedCount += ApplyShoesLayoutToShapeVariants(LoadShoeReference("shoes_2") ?? LoadShoeReference("shoes_2_01"), null, null);
            return updatedCount;
        }

        private static int ApplyLipstickLayoutToShapeVariants(MakeupItem sourceItem, Vector3? offset, Vector3? scale)
        {
            if (sourceItem == null)
            {
                return 0;
            }

            Match match = Regex.Match(sourceItem.Id ?? string.Empty, @"^(lipstick_\d+)(?:_\d+)?$");
            if (!match.Success)
            {
                return 0;
            }

            Vector3 resolvedOffset = offset ?? sourceItem.LayerOffset;
            Vector3 resolvedScale = scale ?? sourceItem.LayerScale;
            return ApplyMakeupLayoutToIdPrefix(match.Groups[1].Value, MakeupType.Lipstick, resolvedOffset, resolvedScale);
        }

        private static int ApplyNecklaceLayoutToShapeVariants(AccessoryItem sourceItem, Vector3? offset, Vector3? scale)
        {
            if (sourceItem == null)
            {
                return 0;
            }

            Match match = Regex.Match(sourceItem.Id ?? string.Empty, @"^(necklace_\d+)(?:_\d+)?$");
            if (!match.Success)
            {
                return 0;
            }

            Vector3 resolvedOffset = offset ?? sourceItem.LayerOffset;
            Vector3 resolvedScale = scale ?? sourceItem.LayerScale;
            return ApplyAccessoryLayoutToIdPrefix(match.Groups[1].Value, item => item.IsNecklaceVariant && !item.IsNecklaceNone, resolvedOffset, resolvedScale);
        }

        private static int ApplyEarringLayoutToShapeVariants(AccessoryItem sourceItem, Vector3? offset, Vector3? scale)
        {
            if (sourceItem == null)
            {
                return 0;
            }

            Match match = Regex.Match(sourceItem.Id ?? string.Empty, @"^(ear_\d+)(?:_\d+)?$");
            if (!match.Success)
            {
                return 0;
            }

            Vector3 resolvedOffset = offset ?? sourceItem.LayerOffset;
            Vector3 resolvedScale = scale ?? sourceItem.LayerScale;
            return ApplyAccessoryLayoutToIdPrefix(match.Groups[1].Value, item => item.IsEarringVariant && !item.IsEarringNone, resolvedOffset, resolvedScale);
        }

        private static int ApplyCrownLayoutToShapeVariants(AccessoryItem sourceItem, Vector3? offset, Vector3? scale)
        {
            if (sourceItem == null)
            {
                return 0;
            }

            Match match = Regex.Match(sourceItem.Id ?? string.Empty, @"^(crown_\d+)(?:_\d+)?$");
            if (!match.Success)
            {
                return 0;
            }

            Vector3 resolvedOffset = offset ?? sourceItem.LayerOffset;
            Vector3 resolvedScale = scale ?? sourceItem.LayerScale;
            return ApplyAccessoryLayoutToIdPrefix(match.Groups[1].Value, item => item.IsCrownVariant && !item.IsCrownNone, resolvedOffset, resolvedScale);
        }

        private static int ApplyGlassesLayoutToShapeVariants(AccessoryItem sourceItem, Vector3? offset, Vector3? scale)
        {
            if (sourceItem == null)
            {
                return 0;
            }

            Match match = Regex.Match(sourceItem.Id ?? string.Empty, @"^(glasses_\d+)(?:_\d+)?$");
            if (!match.Success)
            {
                return 0;
            }

            Vector3 resolvedOffset = offset ?? sourceItem.LayerOffset;
            Vector3 resolvedScale = scale ?? sourceItem.LayerScale;
            return ApplyAccessoryLayoutToIdPrefix(match.Groups[1].Value, item => item.IsGlassesVariant && !item.IsGlassesNone, resolvedOffset, resolvedScale);
        }

        private static int ApplyMakeupLayoutToIdPrefix(
            string idPrefix,
            MakeupType makeupType,
            Vector3 offset,
            Vector3 scale,
            string skipNoneId = null)
        {
            int updatedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:MakeupItem", new[] { "Assets/Resources/Items/Makeup" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MakeupItem item = AssetDatabase.LoadAssetAtPath<MakeupItem>(path);
                if (item == null || item.MakeupType != makeupType || item.IsNoneOption)
                {
                    continue;
                }

                string itemId = item.Id ?? string.Empty;
                if (!string.IsNullOrEmpty(skipNoneId) && itemId == skipNoneId)
                {
                    continue;
                }

                if (itemId != idPrefix && !itemId.StartsWith(idPrefix + "_"))
                {
                    continue;
                }

                item.SetLayerLayout(offset, scale);
                EditorUtility.SetDirty(item);
                updatedCount++;
            }

            return updatedCount;
        }

        private static int ApplyMakeupLayoutToExactIds(
            string[] ids,
            MakeupType makeupType,
            Vector3 offset,
            Vector3 scale)
        {
            int updatedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:MakeupItem", new[] { "Assets/Resources/Items/Makeup" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MakeupItem item = AssetDatabase.LoadAssetAtPath<MakeupItem>(path);
                if (item == null || item.MakeupType != makeupType || item.IsNoneOption)
                {
                    continue;
                }

                string itemId = item.Id ?? string.Empty;
                bool matches = false;
                foreach (string id in ids)
                {
                    if (itemId == id)
                    {
                        matches = true;
                        break;
                    }
                }

                if (!matches)
                {
                    continue;
                }

                item.SetLayerLayout(offset, scale);
                EditorUtility.SetDirty(item);
                updatedCount++;
            }

            return updatedCount;
        }

        private static int ApplyAccessoryLayoutToIdPrefix(
            string idPrefix,
            System.Func<AccessoryItem, bool> predicate,
            Vector3 offset,
            Vector3 scale)
        {
            int updatedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { "Assets/Resources/Items/Accessories" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item == null || !predicate(item))
                {
                    continue;
                }

                string itemId = item.Id ?? string.Empty;
                if (itemId != idPrefix && !itemId.StartsWith(idPrefix + "_"))
                {
                    continue;
                }

                item.SetLayerLayout(offset, scale);
                EditorUtility.SetDirty(item);
                updatedCount++;
            }

            return updatedCount;
        }

        private static MakeupItem LoadMakeupReference(string id, MakeupType makeupType)
        {
            string[] guids = AssetDatabase.FindAssets("t:MakeupItem", new[] { "Assets/Resources/Items/Makeup" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MakeupItem item = AssetDatabase.LoadAssetAtPath<MakeupItem>(path);
                if (item != null && item.MakeupType == makeupType && item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        private static AccessoryItem LoadAccessoryReference(string id)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { "Assets/Resources/Items/Accessories" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        private static int ApplyBagLayoutToAllVariants(AccessoryItem sourceItem, Vector3 offset, Vector3 scale)
        {
            if (sourceItem == null)
            {
                return 0;
            }

            Match match = Regex.Match(sourceItem.Id ?? string.Empty, @"^(bag_\d+)(?:_\d+)?$");
            if (!match.Success)
            {
                return 0;
            }

            string shapePrefix = match.Groups[1].Value;
            int updatedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { "Assets/Resources/Items/Accessories" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item == null || !item.IsBagVariant || item.IsBagNone)
                {
                    continue;
                }

                if (item.Id != shapePrefix && !(item.Id ?? string.Empty).StartsWith(shapePrefix + "_"))
                {
                    continue;
                }

                item.SetLayerLayout(offset, scale);
                EditorUtility.SetDirty(item);
                updatedCount++;
            }

            return updatedCount;
        }

        private static int ApplyDressLayoutToShapeVariants(DressItem sourceItem, Vector3? offset, Vector3? scale)
        {
            if (sourceItem == null)
            {
                return 0;
            }

            Match match = Regex.Match(sourceItem.Id ?? string.Empty, @"^(dress_\d+)(?:_\d+)?$");
            if (!match.Success)
            {
                return 0;
            }

            Vector3 resolvedOffset = offset ?? sourceItem.LayerOffset;
            Vector3 resolvedScale = scale ?? sourceItem.LayerScale;
            return ApplyDressLayoutToIdPrefix(match.Groups[1].Value, resolvedOffset, resolvedScale);
        }

        private static int ApplyShoesLayoutToShapeVariants(ShoeItem sourceItem, Vector3? offset, Vector3? scale)
        {
            if (sourceItem == null)
            {
                return 0;
            }

            Match match = Regex.Match(sourceItem.Id ?? string.Empty, @"^(shoes_\d+)(?:_\d+)?$");
            if (!match.Success)
            {
                return 0;
            }

            Vector3 resolvedOffset = offset ?? sourceItem.LayerOffset;
            Vector3 resolvedScale = scale ?? sourceItem.LayerScale;
            return ApplyShoesLayoutToIdPrefix(match.Groups[1].Value, resolvedOffset, resolvedScale);
        }

        private static int ApplyDressLayoutToIdPrefix(string idPrefix, Vector3 offset, Vector3 scale)
        {
            int updatedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:DressItem", new[] { "Assets/Resources/Items/Dresses" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DressItem item = AssetDatabase.LoadAssetAtPath<DressItem>(path);
                if (item == null)
                {
                    continue;
                }

                string itemId = item.Id ?? string.Empty;
                if (itemId != idPrefix && !itemId.StartsWith(idPrefix + "_"))
                {
                    continue;
                }

                item.SetLayerLayout(offset, scale);
                EditorUtility.SetDirty(item);
                updatedCount++;
            }

            return updatedCount;
        }

        private static int ApplyShoesLayoutToIdPrefix(string idPrefix, Vector3 offset, Vector3 scale)
        {
            int updatedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:ShoeItem", new[] { "Assets/Resources/Items/Shoes" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ShoeItem item = AssetDatabase.LoadAssetAtPath<ShoeItem>(path);
                if (item == null || item.IsShoesNone)
                {
                    continue;
                }

                string itemId = item.Id ?? string.Empty;
                if (itemId != idPrefix && !itemId.StartsWith(idPrefix + "_"))
                {
                    continue;
                }

                item.SetLayerLayout(offset, scale);
                EditorUtility.SetDirty(item);
                updatedCount++;
            }

            return updatedCount;
        }

        private static ShoeItem LoadShoeReference(string id)
        {
            return AssetDatabase.LoadAssetAtPath<ShoeItem>($"Assets/Resources/Items/Shoes/{id}.asset");
        }

        private static ShoeItem ResolveShoeItem(Transform shoesTransform)
        {
            if (Selection.activeObject is ShoeItem selected && selected.IsShoesVariant)
            {
                return selected;
            }

            if (Selection.activeObject is Sprite spriteFromSelection)
            {
                ShoeItem fromSprite = FindShoeItemBySprite(spriteFromSelection);
                if (fromSprite != null)
                {
                    return fromSprite;
                }
            }

            SpriteRenderer renderer = shoesTransform.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite != null)
            {
                return FindShoeItemBySprite(renderer.sprite);
            }

            return null;
        }

        private static ShoeItem FindShoeItemBySprite(Sprite sprite)
        {
            string[] guids = AssetDatabase.FindAssets("t:ShoeItem", new[] { "Assets/Resources/Items/Shoes" });
            foreach (string guid in guids)
            {
                ShoeItem item = AssetDatabase.LoadAssetAtPath<ShoeItem>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null && item.IsShoesVariant && item.Sprite == sprite)
                {
                    return item;
                }
            }

            return null;
        }

        private static void ResetAllShoesOffsets(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:ShoeItem", new[] { folder });
            foreach (string guid in guids)
            {
                ShoeItem item = AssetDatabase.LoadAssetAtPath<ShoeItem>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null && item.IsShoesVariant)
                {
                    item.SetLayerLayout(Vector3.zero, Vector3.one);
                    EditorUtility.SetDirty(item);
                }
            }
        }

        private static MakeupItem FindMakeupItemBySprite(Sprite sprite, MakeupType makeupType)
        {
            string[] guids = AssetDatabase.FindAssets("t:MakeupItem", new[] { "Assets/Resources/Items/Makeup" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MakeupItem item = AssetDatabase.LoadAssetAtPath<MakeupItem>(path);
                if (item != null
                    && item.MakeupType == makeupType
                    && item.Sprite == sprite)
                {
                    return item;
                }
            }

            return null;
        }

        private static bool EnsureBodyAtBase(CharacterCustomizer customizer)
        {
            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            Transform bodyTransform = customizer.transform.Find("Body");
            if (bodyTransform == null)
            {
                return true;
            }

            Vector3 basePosition = layout != null
                ? layout.GetBasePosition("Body")
                : Vector3.zero;

            if (Vector3.Distance(bodyTransform.localPosition, basePosition) > 0.001f
                || Vector3.Distance(bodyTransform.localScale, Vector3.one) > 0.001f)
            {
                EditorUtility.DisplayDialog(
                    "Move Body First",
                    "Body must stay at position (0,0,0) with scale (1,1,1).\n\n" +
                    "Only move Hair, Dress, Lips, Eyeshadow, Blush, Crown, or Glasses — not Body, and not the whole Girl object.\n\n" +
                    "Run Dress Up Game → Reset Layer Positions if things look wrong.",
                    "OK");
                return false;
            }

            return true;
        }

        private static Vector3 CalculateRelativeOffset(CharacterCustomizer customizer, Transform layerTransform)
        {
            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            Vector3 basePosition = layout != null
                ? layout.GetBasePosition(layerTransform.name)
                : Vector3.zero;

            return layerTransform.localPosition - basePosition;
        }

        private static void ApplyLayerLayoutPreview(
            CharacterCustomizer customizer,
            Transform layerTransform,
            Vector3 offset,
            Vector3 scale)
        {
            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            Vector3 basePosition = layout != null
                ? layout.GetBasePosition(layerTransform.name)
                : Vector3.zero;

            layerTransform.localPosition = basePosition + offset;
            layerTransform.localScale = scale == Vector3.zero ? Vector3.one : scale;
        }

        private static bool EnsureNotPlayingForPreview(string layerLabel)
        {
            if (!EditorApplication.isPlaying)
            {
                return true;
            }

            EditorUtility.DisplayDialog(
                "Stop Play Mode First",
                $"Stop Play before previewing a {layerLabel} item in the Scene view.",
                "OK");
            return false;
        }

        private static CharacterCustomizer FindCustomizerForPreview()
        {
            CharacterCustomizer customizer = Object.FindAnyObjectByType<CharacterCustomizer>();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog(
                    "Not Found",
                    "No Girl with CharacterCustomizer found in the open scene.",
                    "OK");
            }

            return customizer;
        }

        private static bool TryGetLayerRenderer(
            CharacterCustomizer customizer,
            string layerName,
            out Transform layerTransform,
            out SpriteRenderer renderer)
        {
            layerTransform = FindLayerTransform(customizer.transform, layerName)
                ?? customizer.transform.Find(layerName);
            renderer = layerTransform != null ? layerTransform.GetComponent<SpriteRenderer>() : null;

            if (layerTransform != null && renderer != null)
            {
                return true;
            }

            EditorUtility.DisplayDialog(
                "Not Found",
                $"Could not find Girl → {layerName} SpriteRenderer in the scene.",
                "OK");
            return false;
        }

        private static void PromptSelectItem(string itemLabel, string examples)
        {
            EditorUtility.DisplayDialog(
                $"Select {itemLabel} Item",
                $"Select a {itemLabel} item asset first (e.g. {examples}),\nthen run this menu again.",
                "OK");
        }

        private static void FinishPreview(
            CharacterCustomizer customizer,
            Transform layerTransform,
            string itemName,
            Vector3 offset,
            Vector3 scale)
        {
            customizer.ApplySortingOrders();
            Selection.activeGameObject = layerTransform.gameObject;
            SceneView.RepaintAll();
            Debug.Log($"Previewing {itemName} in Scene view. Offset: {offset}, Scale: {scale}");
        }

        private static void PreviewHairItem(HairItem hairItem, string layerName)
        {
            if (!EnsureNotPlayingForPreview("hair"))
            {
                return;
            }

            if (hairItem == null)
            {
                PromptSelectItem("Hair", "Hair_5.asset in Assets/Resources/Items/Hair/");
                return;
            }

            CharacterCustomizer customizer = FindCustomizerForPreview();
            if (customizer == null || !TryGetLayerRenderer(customizer, layerName, out Transform layerTransform, out SpriteRenderer renderer))
            {
                return;
            }

            renderer.sprite = hairItem.Sprite;
            renderer.enabled = hairItem.Sprite != null;
            renderer.color = Color.white;
            ApplyLayerLayoutPreview(customizer, layerTransform, hairItem.LayerOffset, hairItem.LayerScale);
            FinishPreview(customizer, layerTransform, hairItem.name, hairItem.LayerOffset, hairItem.LayerScale);
        }

        private static void PreviewDressItem(DressItem dressItem, string layerName)
        {
            if (!EnsureNotPlayingForPreview("dress"))
            {
                return;
            }

            if (dressItem == null)
            {
                PromptSelectItem("Dress", "Dress_1.asset in Assets/Resources/Items/Dresses/");
                return;
            }

            CharacterCustomizer customizer = FindCustomizerForPreview();
            if (customizer == null || !TryGetLayerRenderer(customizer, layerName, out Transform layerTransform, out SpriteRenderer renderer))
            {
                return;
            }

            renderer.sprite = dressItem.Sprite;
            renderer.color = dressItem.TintColor;
            renderer.enabled = dressItem.Sprite != null;
            ApplyLayerLayoutPreview(customizer, layerTransform, dressItem.LayerOffset, dressItem.LayerScale);
            FinishPreview(customizer, layerTransform, dressItem.name, dressItem.LayerOffset, dressItem.LayerScale);
        }

        private static void PreviewShoeItem(ShoeItem shoeItem, string layerName)
        {
            if (!EnsureNotPlayingForPreview("shoe"))
            {
                return;
            }

            if (shoeItem == null)
            {
                PromptSelectItem("Shoe", "shoes_1.asset in Assets/Resources/Items/Shoes/");
                return;
            }

            CharacterCustomizer customizer = FindCustomizerForPreview();
            if (customizer == null || !TryGetLayerRenderer(customizer, layerName, out Transform layerTransform, out SpriteRenderer renderer))
            {
                return;
            }

            if (shoeItem.IsShoesNone || shoeItem.Sprite == null)
            {
                renderer.sprite = null;
                renderer.enabled = false;
            }
            else
            {
                renderer.sprite = shoeItem.Sprite;
                renderer.color = shoeItem.TintColor;
                renderer.enabled = true;
            }

            ApplyLayerLayoutPreview(customizer, layerTransform, shoeItem.LayerOffset, shoeItem.LayerScale);
            FinishPreview(customizer, layerTransform, shoeItem.name, shoeItem.LayerOffset, shoeItem.LayerScale);
        }

        private static void PreviewMakeupItem(
            MakeupItem makeupItem,
            string layerName,
            string itemLabel,
            bool applyEyeTint = false)
        {
            if (!EnsureNotPlayingForPreview(itemLabel))
            {
                return;
            }

            if (makeupItem == null)
            {
                PromptSelectItem(itemLabel, "an item in Assets/Resources/Items/Makeup/");
                return;
            }

            CharacterCustomizer customizer = FindCustomizerForPreview();
            if (customizer == null || !TryGetLayerRenderer(customizer, layerName, out Transform layerTransform, out SpriteRenderer renderer))
            {
                return;
            }

            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            if (makeupItem.IsNoneOption || makeupItem.Sprite == null)
            {
                renderer.sprite = null;
                renderer.enabled = false;
                renderer.material = null;
                renderer.color = Color.white;
                layout?.ApplyBasePosition(layerTransform);
                FinishPreview(customizer, layerTransform, makeupItem.name, Vector3.zero, Vector3.one);
                return;
            }

            renderer.sprite = makeupItem.Sprite;
            renderer.color = makeupItem.TintColor;
            renderer.enabled = true;
            if (applyEyeTint)
            {
                EyeIrisTintUtility.ApplyToRenderer(renderer, makeupItem);
            }
            else
            {
                renderer.material = null;
            }

            ApplyLayerLayoutPreview(customizer, layerTransform, makeupItem.LayerOffset, makeupItem.LayerScale);
            FinishPreview(customizer, layerTransform, makeupItem.name, makeupItem.LayerOffset, makeupItem.LayerScale);
        }

        private static void PreviewAccessoryItem(
            AccessoryItem accessoryItem,
            string layerName,
            string itemLabel)
        {
            if (!EnsureNotPlayingForPreview(itemLabel))
            {
                return;
            }

            if (accessoryItem == null)
            {
                PromptSelectItem(itemLabel, "an item in Assets/Resources/Items/Accessories/");
                return;
            }

            CharacterCustomizer customizer = FindCustomizerForPreview();
            if (customizer == null || !TryGetLayerRenderer(customizer, layerName, out Transform layerTransform, out SpriteRenderer renderer))
            {
                return;
            }

            CharacterLayerLayout layout = customizer.GetComponent<CharacterLayerLayout>();
            bool isNone = accessoryItem.IsNoneOption
                || accessoryItem.IsNecklaceNone
                || accessoryItem.IsEarringNone
                || accessoryItem.IsCrownNone
                || accessoryItem.IsGlassesNone
                || accessoryItem.IsBagNone
                || accessoryItem.Sprite == null;

            if (isNone)
            {
                renderer.sprite = null;
                renderer.enabled = false;
                renderer.color = Color.white;
                layout?.ApplyBasePosition(layerTransform);
                FinishPreview(customizer, layerTransform, accessoryItem.name, Vector3.zero, Vector3.one);
                return;
            }

            renderer.sprite = accessoryItem.Sprite;
            renderer.color = accessoryItem.TintColor;
            renderer.enabled = true;
            ApplyLayerLayoutPreview(customizer, layerTransform, accessoryItem.LayerOffset, accessoryItem.LayerScale);
            FinishPreview(customizer, layerTransform, accessoryItem.name, accessoryItem.LayerOffset, accessoryItem.LayerScale);
        }

        private static void ResetAllItemOffsets<T>(string folder) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T item = AssetDatabase.LoadAssetAtPath<T>(path);
                if (item is HairItem hairItem)
                {
                    hairItem.SetLayerLayout(Vector3.zero, Vector3.one);
                    EditorUtility.SetDirty(hairItem);
                }
                else if (item is DressItem dressItem)
                {
                    dressItem.SetLayerLayout(Vector3.zero, Vector3.one);
                    EditorUtility.SetDirty(dressItem);
                }
            }
        }

        private static int ApplyEyeLayoutToAllColorVariants(MakeupItem sourceItem, Vector3 offset, Vector3 scale)
        {
            if (sourceItem == null)
            {
                return 0;
            }

            Match match = Regex.Match(sourceItem.Id ?? string.Empty, @"^(eyes_\d+)_\d+$");
            string shapePrefix = match.Success ? match.Groups[1].Value : sourceItem.Id;
            int updatedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:MakeupItem", new[] { "Assets/Resources/Items/Makeup" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MakeupItem item = AssetDatabase.LoadAssetAtPath<MakeupItem>(path);
                if (item == null || item.MakeupType != MakeupType.Eyes || item.IsNoneOption)
                {
                    continue;
                }

                if (item.Id != shapePrefix && !(item.Id ?? string.Empty).StartsWith(shapePrefix + "_"))
                {
                    continue;
                }

                item.SetLayerLayout(offset, scale);
                EditorUtility.SetDirty(item);
                updatedCount++;
            }

            return updatedCount;
        }

        private static void ResetAllMakeupOffsets(string folder, MakeupType makeupType)
        {
            string[] guids = AssetDatabase.FindAssets("t:MakeupItem", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MakeupItem item = AssetDatabase.LoadAssetAtPath<MakeupItem>(path);
                if (item != null && item.MakeupType == makeupType)
                {
                    item.SetLayerLayout(Vector3.zero, Vector3.one);
                    EditorUtility.SetDirty(item);
                }
            }
        }

        private static void ResetAllNecklaceOffsets(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.IsNecklaceVariant)
                {
                    item.SetLayerLayout(Vector3.zero, Vector3.one);
                    EditorUtility.SetDirty(item);
                }
            }
        }

        private static void ResetAllEarringOffsets(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.IsEarringVariant)
                {
                    item.SetLayerLayout(Vector3.zero, Vector3.one);
                    EditorUtility.SetDirty(item);
                }
            }
        }

        private static void ResetAllCrownOffsets(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.IsCrownVariant)
                {
                    item.SetLayerLayout(Vector3.zero, Vector3.one);
                    EditorUtility.SetDirty(item);
                }
            }
        }

        private static void ResetAllGlassesOffsets(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.IsGlassesVariant)
                {
                    item.SetLayerLayout(Vector3.zero, Vector3.one);
                    EditorUtility.SetDirty(item);
                }
            }
        }

        private static void ResetAllBagOffsets(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:AccessoryItem", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AccessoryItem item = AssetDatabase.LoadAssetAtPath<AccessoryItem>(path);
                if (item != null && item.IsBagVariant)
                {
                    item.SetLayerLayout(Vector3.zero, Vector3.one);
                    EditorUtility.SetDirty(item);
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
    }
}
#endif
