#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using DressUpGame.Data;
using DressUpGame.MysteryBox;
using DressUpGame.Pet;
using UnityEditor;
using UnityEngine;

namespace DressUpGame.Editor
{
    public static class MysteryLootRegistryTool
    {
        private const string RegistryPath = "Assets/Resources/MysteryLootRegistry.asset";
        private const string GameCatalogPath = "Assets/Resources/GameCatalog.asset";
        private const string PetCatalogPath = "Assets/Resources/PetCatalog.asset";

        [MenuItem("Dress Up Game/Refresh Mystery Loot Registry")]
        public static void RefreshMysteryLootRegistry()
        {
            MysteryLootRegistry registry = LoadOrCreateRegistry();
            GameCatalog gameCatalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(GameCatalogPath);
            PetCatalog petCatalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(PetCatalogPath);
            registry.SetCatalogs(gameCatalog, petCatalog);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            ValidateMysteryLoot();
        }

        [MenuItem("Dress Up Game/Validate Mystery Loot")]
        public static void ValidateMysteryLoot()
        {
            MysteryLootRegistry registry = AssetDatabase.LoadAssetAtPath<MysteryLootRegistry>(RegistryPath);
            StringBuilder report = new StringBuilder();
            report.AppendLine("Mystery loot validation:");

            if (registry == null)
            {
                report.AppendLine("- FAIL: Missing MysteryLootRegistry.asset. Run Refresh Mystery Loot Registry.");
                Debug.LogError(report.ToString());
                return;
            }

            GameCatalog gameCatalog = registry.GameCatalog ?? AssetDatabase.LoadAssetAtPath<GameCatalog>(GameCatalogPath);
            List<MysteryLootEntry> entries = MysteryLootCatalogBuilder.Build(registry);
            report.AppendLine($"- OK: {entries.Count} total loot entries from catalogs.");

            int legacyWarnings = 0;
            if (gameCatalog != null)
            {
                foreach (MysteryLootEntry entry in entries)
                {
                    if (!entry.IsGirl || string.IsNullOrEmpty(entry.itemId))
                    {
                        continue;
                    }

                    if (MysteryLootLegacyFilter.ShouldSkipLegacyGirlLootId(entry.itemId, gameCatalog))
                    {
                        legacyWarnings++;
                        report.AppendLine(
                            $"- WARN: Legacy base id '{entry.itemId}' is still in the mystery loot table while color variants exist in GameCatalog.");
                    }
                }
            }

            Dictionary<MysteryGirlCategory, int> girlCounts = new Dictionary<MysteryGirlCategory, int>();
            Dictionary<MysteryPetCategory, int> petCounts = new Dictionary<MysteryPetCategory, int>();
            foreach (MysteryLootEntry entry in entries)
            {
                if (entry.IsGirl)
                {
                    girlCounts.TryGetValue(entry.girlCategory, out int count);
                    girlCounts[entry.girlCategory] = count + 1;
                }
                else if (entry.IsPet)
                {
                    petCounts.TryGetValue(entry.petCategory, out int count);
                    petCounts[entry.petCategory] = count + 1;
                }
            }

            foreach (KeyValuePair<MysteryGirlCategory, int> pair in girlCounts)
            {
                report.AppendLine($"  Girl {pair.Key}: {pair.Value}");
            }

            foreach (KeyValuePair<MysteryPetCategory, int> pair in petCounts)
            {
                report.AppendLine($"  Pet {pair.Key}: {pair.Value}");
            }

            if (entries.Count == 0)
            {
                report.AppendLine("- FAIL: No eligible mystery loot entries.");
                Debug.LogError(report.ToString());
                return;
            }

            if (legacyWarnings > 0)
            {
                report.AppendLine(
                    $"- WARN: {legacyWarnings} legacy base id(s) should be excluded. Re-run Refresh Mystery Loot Registry after fixing catalog/build.");
                Debug.LogWarning(report.ToString());
                return;
            }

            Debug.Log(report.ToString());
        }

        private static MysteryLootRegistry LoadOrCreateRegistry()
        {
            MysteryLootRegistry registry = AssetDatabase.LoadAssetAtPath<MysteryLootRegistry>(RegistryPath);
            if (registry != null)
            {
                return registry;
            }

            registry = ScriptableObject.CreateInstance<MysteryLootRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryPath);
            return registry;
        }
    }
}
#endif
