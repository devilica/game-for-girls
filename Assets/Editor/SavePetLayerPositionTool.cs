#if UNITY_EDITOR
using System.Text.RegularExpressions;
using DressUpGame.Pet;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class SavePetLayerPositionTool
    {
        [MenuItem("Dress Up Game/Save Pet Body Position To Pet Item")]
        public static void SavePetBodyPosition() => SaveLayer("Body", PetCustomizationCategory.Pet, @"^pet_(\d+)$");

        [MenuItem("Dress Up Game/Save Hairbow Position To Hairbow Item")]
        public static void SaveHairbowPosition() => SaveLayer("Hairbow", PetCustomizationCategory.Hairbow, @"^bow_(\d+)$");

        [MenuItem("Dress Up Game/Save Collar Position To Collar Item")]
        public static void SaveCollarPosition() => SaveLayer("Collar", PetCustomizationCategory.Collar, @"^collar_(\d+)$");

        [MenuItem("Dress Up Game/Save Pet Glasses Position To Glasses Item")]
        public static void SaveGlassesPosition() => SaveLayer("Glasses", PetCustomizationCategory.Glasses, @"^glasses_(\d+)$");

        [MenuItem("Dress Up Game/Save Pet Accessory Anchors To Current Pet Item")]
        public static void SavePetAccessoryAnchors()
        {
            PetCustomizer customizer = FindPetCustomizer();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Pet Scene Required", "Open PetScene with a pet selected.", "OK");
                return;
            }

            PetItem pet = customizer.SelectedPet;
            if (!TryGetActivePetId(customizer, out string petId, out PetItem petItem))
            {
                return;
            }

            PetLayerLayout layout = customizer.GetComponent<PetLayerLayout>();
            if (layout == null)
            {
                EditorUtility.DisplayDialog("Missing Layout", "Pet root needs PetLayerLayout.", "OK");
                return;
            }

            SaveAnchorForLayer(customizer, petItem, layout, "Hairbow", customizer.SelectedHairbow, petId);
            SaveAnchorForLayer(customizer, petItem, layout, "Collar", customizer.SelectedCollar, petId);
            SaveAnchorForLayer(customizer, petItem, layout, "Glasses", customizer.SelectedGlasses, petId);

            customizer.ApplyAllSelections();
            EditorUtility.SetDirty(petItem);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Saved",
                $"Saved Hairbow / Collar / Glasses anchor offsets on {petItem.name} for {petId}.\n\n" +
                "These shift every accessory on this pet shape (use per-item saves for one bow/collar/glasses).",
                "OK");
        }

        [MenuItem("Dress Up Game/Reset Pet Layer Positions")]
        public static void ResetPetLayerPositions()
        {
            PetCustomizer customizer = FindPetCustomizer();
            if (customizer == null)
            {
                return;
            }

            PetLayerLayout layout = customizer.GetComponent<PetLayerLayout>();
            layout?.ApplyAllBasePositions(customizer.transform);
            customizer.ApplyAllSelections();
            Debug.Log("Dress Up Game: Reset pet layer transforms to base anchors.");
        }

        private static void SaveAnchorForLayer(
            PetCustomizer customizer,
            PetItem petItem,
            PetLayerLayout layout,
            string layerName,
            PetItem accessory,
            string petId)
        {
            Transform layer = customizer.transform.Find(layerName);
            if (layer == null || accessory == null || accessory.IsNoneOption)
            {
                return;
            }

            accessory.GetEffectiveLayout(petId, out Vector3 accessoryOffset, out _);
            Vector3 globalBase = layout.GetBasePosition(layerName);
            Vector3 anchorAdjust = layer.localPosition - globalBase - accessoryOffset;
            petItem.SetLayerAnchorAdjust(layerName, anchorAdjust);
        }

        private static void SaveLayer(string layerName, PetCustomizationCategory category, string idPattern)
        {
            PetCustomizer customizer = FindPetCustomizer();
            if (customizer == null)
            {
                EditorUtility.DisplayDialog("Pet Scene Required", "Open PetScene and select a pet item layer to save.", "OK");
                return;
            }

            Transform layer = customizer.transform.Find(layerName);
            if (layer == null)
            {
                EditorUtility.DisplayDialog("Layer Missing", $"Could not find '{layerName}' under Pet.", "OK");
                return;
            }

            PetItem item = ResolveItem(customizer, layer, category, idPattern);
            if (item == null)
            {
                if (!System.IO.File.Exists("Assets/Resources/PetCatalog.asset"))
                {
                    GeneratePetItemsTool.GeneratePetItems();
                }

                item = ResolveItem(customizer, layer, category, idPattern);
            }

            if (item == null)
            {
                EditorUtility.DisplayDialog(
                    "Select Pet Item",
                    $"Could not determine which {category} item to save to.\n\n" +
                    "Select a PetItem asset in Assets/Resources/Items/Pet/ or preview a sprite on the layer.",
                    "OK");
                return;
            }

            Vector3 offset = layer.localPosition;
            PetLayerLayout layout = customizer.GetComponent<PetLayerLayout>();
            if (layout != null)
            {
                offset -= layout.GetBasePosition(layerName);
                if (category != PetCustomizationCategory.Pet)
                {
                    PetItem layoutPet = ResolveActivePetShape(customizer);
                    if (layoutPet != null && layoutPet.IsPetVariant)
                    {
                        offset -= layoutPet.GetLayerAnchorAdjust(layerName);
                    }
                }
            }

            Vector3 scale = layer.localScale;

            string petContext = string.Empty;
            if (category == PetCustomizationCategory.Pet)
            {
                item.SetLayerLayout(offset, scale);
            }
            else
            {
                if (!TryGetActivePetId(customizer, out string petId, out _))
                {
                    return;
                }

                item.SetLayoutForPet(petId, offset, scale);
                petContext = $" (for {petId})";
            }

            customizer.ApplyAllSelections();
            EditorUtility.SetDirty(item);
            AssetDatabase.SaveAssets();

            Debug.Log($"Saved {category} layout to {item.name}{petContext}. Offset: {offset}, Scale: {scale}");
            EditorUtility.DisplayDialog(
                "Saved",
                $"Saved {category} layout on {item.name}{petContext}.\n\nOffset: {offset}\nScale: {scale}",
                "OK");
        }

        private static bool TryGetActivePetId(PetCustomizer customizer, out string petId, out PetItem petItem)
        {
            petItem = ResolveActivePetShape(customizer);
            petId = petItem != null ? PetLayoutIds.ResolveBasePetId(petItem.Id) : string.Empty;
            if (petItem == null || petItem.IsPetNone)
            {
                EditorUtility.DisplayDialog(
                    "Select a pet shape",
                    "Tell the tool which pet you are laying out:\n\n" +
                    "• In Scene view: put pet_1 / pet_2 / … on the Body sprite, or\n" +
                    "• In Project: select Assets/Resources/Items/Pet/Pets/pet_1.asset, or\n" +
                    "• Press Play → Menu → Game 2 → pick a pet in the bottom item grid (that is the wizard).\n\n" +
                    "Then run the save menu again.",
                    "OK");
                petItem = null;
                petId = string.Empty;
                return false;
            }

            return true;
        }

        private static PetItem ResolveActivePetShape(PetCustomizer customizer)
        {
            PetItem selected = customizer.SelectedPet;
            if (selected != null && selected.IsPetVariant)
            {
                return selected;
            }

            if (Selection.activeObject is PetItem petAsset && petAsset.IsPetVariant)
            {
                return petAsset;
            }

            Transform body = customizer.transform.Find("Body");
            SpriteRenderer bodyRenderer = body != null ? body.GetComponent<SpriteRenderer>() : null;
            if (bodyRenderer != null && bodyRenderer.sprite != null)
            {
                Match match = Regex.Match(bodyRenderer.sprite.name, @"^pet_(\d+)$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    PetCatalog catalog = GetCatalog(customizer);
                    PetItem fromBody = catalog?.GetById(bodyRenderer.sprite.name);
                    if (fromBody != null)
                    {
                        return fromBody;
                    }
                }
            }

            PetCatalog fallbackCatalog = GetCatalog(customizer);
            return fallbackCatalog?.GetById("pet_1") ?? fallbackCatalog?.GetDefault(PetCustomizationCategory.Pet);
        }

        private static PetCatalog GetCatalog(PetCustomizer customizer)
        {
            PetCatalog catalog = customizer.Catalog;
            if (catalog == null)
            {
                catalog = Resources.Load<PetCatalog>("PetCatalog");
            }

            return catalog;
        }

        private static PetItem ResolveItem(
            PetCustomizer customizer,
            Transform layer,
            PetCustomizationCategory category,
            string idPattern)
        {
            if (Selection.activeObject is PetItem selectedItem && selectedItem.Category == category)
            {
                return selectedItem;
            }

            SpriteRenderer renderer = layer.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite != null)
            {
                string spriteName = renderer.sprite.name;
                Match match = Regex.Match(spriteName, idPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string id = spriteName;
                    PetCatalog catalog = customizer.Catalog;
                    if (catalog == null)
                    {
                        catalog = Resources.Load<PetCatalog>("PetCatalog");
                    }

                    return catalog?.GetById(id);
                }
            }

            return category switch
            {
                PetCustomizationCategory.Pet => customizer.SelectedPet,
                PetCustomizationCategory.Hairbow => customizer.SelectedHairbow,
                PetCustomizationCategory.Collar => customizer.SelectedCollar,
                _ => customizer.SelectedGlasses
            };
        }

        private static PetCustomizer FindPetCustomizer()
        {
            if (Selection.activeGameObject != null)
            {
                PetCustomizer onSelection = Selection.activeGameObject.GetComponentInParent<PetCustomizer>();
                if (onSelection != null)
                {
                    return onSelection;
                }
            }

            return Object.FindAnyObjectByType<PetCustomizer>();
        }
    }
}
#endif
