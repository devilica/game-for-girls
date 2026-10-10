using System;
using System.Collections.Generic;
using UnityEngine;

namespace DressUpGame.MysteryBox
{
    [Serializable]
    public class MysteryUnlockSaveData
    {
        public List<string> unlockedItemIds = new List<string>();
    }

    /// <summary>
    /// Persistent mystery unlock inventory (PlayerPrefs). Separate from equipped look save data.
    /// </summary>
    public static class MysteryUnlockStore
    {
        public const string SaveKey = "DressUpGame_MysteryUnlocks";

        public static bool IsUnlocked(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return false;
            }

            return LoadSet().Contains(itemId);
        }

        public static void Unlock(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            HashSet<string> set = LoadSet();
            if (!set.Add(itemId))
            {
                return;
            }

            SaveSet(set);
        }

        public static HashSet<string> LoadSet()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return new HashSet<string>(StringComparer.Ordinal);
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            MysteryUnlockSaveData data = JsonUtility.FromJson<MysteryUnlockSaveData>(json);
            if (data?.unlockedItemIds == null)
            {
                return new HashSet<string>(StringComparer.Ordinal);
            }

            return new HashSet<string>(data.unlockedItemIds, StringComparer.Ordinal);
        }

        public static void ClearAll()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
            }
        }

        public static void RestoreSet(IEnumerable<string> itemIds)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.Ordinal);
            if (itemIds != null)
            {
                foreach (string itemId in itemIds)
                {
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        set.Add(itemId);
                    }
                }
            }

            SaveSet(set);
        }

        private static void SaveSet(HashSet<string> set)
        {
            MysteryUnlockSaveData data = new MysteryUnlockSaveData
            {
                unlockedItemIds = new List<string>(set)
            };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
