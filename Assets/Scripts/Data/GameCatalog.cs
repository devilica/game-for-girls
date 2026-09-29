using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DressUpGame.Data
{
    /// <summary>
    /// Central catalog of all customization items. Assign assets in the Inspector
    /// or let the editor generator populate this asset automatically.
    /// </summary>
    [CreateAssetMenu(fileName = "GameCatalog", menuName = "Dress Up Game/Game Catalog")]
    public class GameCatalog : ScriptableObject
    {
        [Header("Libraries")]
        [SerializeField] private HairColorLibrary hairColorLibrary;

        [Header("Items")]
        [SerializeField] private List<HairItem> hairItems = new List<HairItem>();
        [SerializeField] private List<DressItem> dressItems = new List<DressItem>();
        [SerializeField] private List<ShoeItem> shoeItems = new List<ShoeItem>();
        [SerializeField] private List<MakeupItem> makeupItems = new List<MakeupItem>();
        [SerializeField] private List<AccessoryItem> accessoryItems = new List<AccessoryItem>();

        public HairColorLibrary HairColorLibrary => hairColorLibrary;
        public IReadOnlyList<HairItem> HairItems => hairItems;
        public IReadOnlyList<DressItem> DressItems => dressItems;
        public IReadOnlyList<ShoeItem> ShoeItems => shoeItems;
        public IReadOnlyList<MakeupItem> MakeupItems => makeupItems;
        public IReadOnlyList<AccessoryItem> AccessoryItems => accessoryItems;

        public HairItem GetHairById(string id) => hairItems.FirstOrDefault(item => item != null && item.Id == id);
        public DressItem GetDressById(string id)
        {
            DressItem item = dressItems.FirstOrDefault(entry => entry != null && entry.Id == id);
            if (item != null || string.IsNullOrEmpty(id))
            {
                return item;
            }

            Match legacyMatch = Regex.Match(id, @"^dress_(\d+)$");
            if (legacyMatch.Success)
            {
                string colorVariantId = $"dress_{legacyMatch.Groups[1].Value}_01";
                return dressItems.FirstOrDefault(entry => entry != null && entry.Id == colorVariantId);
            }

            return null;
        }
        public MakeupItem GetMakeupById(string id)
        {
            MakeupItem item = makeupItems.FirstOrDefault(entry => entry != null && entry.Id == id);
            if (item != null || string.IsNullOrEmpty(id))
            {
                return item;
            }

            Match legacyLipstick = Regex.Match(id, @"^lipstick_(\d+)$");
            if (legacyLipstick.Success)
            {
                string colorVariantId = $"lipstick_{legacyLipstick.Groups[1].Value}_01";
                return makeupItems.FirstOrDefault(entry => entry != null && entry.Id == colorVariantId);
            }

            if (id == "eyes_1" || id == "eyes_2")
            {
                return makeupItems.FirstOrDefault(entry => entry != null && entry.Id == "eyes_2_01");
            }

            Match legacyEyeshadow = Regex.Match(id, @"^eyeshadow_(\d+)$");
            if (legacyEyeshadow.Success)
            {
                string index = legacyEyeshadow.Groups[1].Value;
                string colorVariantId = int.Parse(index) < 10
                    ? $"eyeshadow_2_0{index}"
                    : $"eyeshadow_2_{index}";
                return makeupItems.FirstOrDefault(entry => entry != null && entry.Id == colorVariantId);
            }

            Match legacyBlush = Regex.Match(id, @"^blush_(\d+)$");
            if (legacyBlush.Success)
            {
                string index = legacyBlush.Groups[1].Value;
                string colorVariantId = int.Parse(index) < 10
                    ? $"blush_2_0{index}"
                    : $"blush_2_{index}";
                return makeupItems.FirstOrDefault(entry => entry != null && entry.Id == colorVariantId);
            }

            return null;
        }

        public ShoeItem GetShoeById(string id) => shoeItems.FirstOrDefault(item => item != null && item.Id == id);

        public AccessoryItem GetAccessoryById(string id)
        {
            AccessoryItem item = accessoryItems.FirstOrDefault(entry => entry != null && entry.Id == id);
            if (item != null || string.IsNullOrEmpty(id))
            {
                return item;
            }

            Match legacyCrown = Regex.Match(id, @"^crown_(\d+)$");
            if (legacyCrown.Success)
            {
                string colorVariantId = $"crown_{legacyCrown.Groups[1].Value}_01";
                return accessoryItems.FirstOrDefault(entry => entry != null && entry.Id == colorVariantId);
            }

            if (id == "accessory_crown")
            {
                return accessoryItems.FirstOrDefault(entry => entry != null && entry.Id == "crown_1_01");
            }

            if (id == "accessory_glasses")
            {
                return accessoryItems.FirstOrDefault(entry => entry != null && entry.Id == "glasses_1");
            }

            Match legacyGlasses = Regex.Match(id, @"^glasses_(\d+)$");
            if (legacyGlasses.Success)
            {
                string shapeIndex = legacyGlasses.Groups[1].Value;
                if (shapeIndex == "2")
                {
                    return accessoryItems.FirstOrDefault(entry => entry != null && entry.Id == "glasses_2_01");
                }

                return accessoryItems.FirstOrDefault(entry => entry != null && entry.Id == id);
            }

            Match legacyBag = Regex.Match(id, @"^bag_(\d+)$");
            if (legacyBag.Success)
            {
                return accessoryItems.FirstOrDefault(entry => entry != null && entry.Id == id);
            }

            if (id == "accessory_handbag")
            {
                return accessoryItems.FirstOrDefault(entry => entry != null && entry.Id == "bag_1");
            }

            return null;
        }

        public IEnumerable<ShoeItem> GetShoesItems()
        {
            return shoeItems.Where(item => item != null && (item.IsShoesNone || item.IsShoesVariant));
        }

        public IEnumerable<AccessoryItem> GetNecklaceItems()
        {
            return accessoryItems.Where(item => item != null && (item.IsNecklaceNone || item.IsNecklaceVariant));
        }

        public IEnumerable<AccessoryItem> GetEarringItems()
        {
            return accessoryItems.Where(item => item != null && (item.IsEarringNone || item.IsEarringVariant));
        }

        public IEnumerable<AccessoryItem> GetCrownItems()
        {
            return accessoryItems.Where(item => item != null && (item.IsCrownNone || item.IsCrownVariant));
        }

        public IEnumerable<AccessoryItem> GetGlassesItems()
        {
            return accessoryItems.Where(item => item != null && (item.IsGlassesNone || item.IsGlassesVariant));
        }

        public IEnumerable<AccessoryItem> GetBagItems()
        {
            return accessoryItems.Where(item => item != null && (item.IsBagNone || item.IsBagVariant));
        }

        public ShoeItem GetDefaultShoe()
        {
            return shoeItems.FirstOrDefault(item => item != null && item.IsShoesNone)
                ?? shoeItems.FirstOrDefault(item => item != null && item.IsShoesVariant);
        }

        public AccessoryItem GetDefaultNecklace()
        {
            return accessoryItems.FirstOrDefault(item => item != null && item.IsNecklaceNone)
                ?? accessoryItems.FirstOrDefault(item => item != null && item.IsNecklaceVariant);
        }

        public AccessoryItem GetDefaultEarring()
        {
            return accessoryItems.FirstOrDefault(item => item != null && item.IsEarringNone)
                ?? accessoryItems.FirstOrDefault(item => item != null && item.IsEarringVariant);
        }

        public AccessoryItem GetDefaultCrown()
        {
            return accessoryItems.FirstOrDefault(item => item != null && item.IsCrownNone)
                ?? accessoryItems.FirstOrDefault(item => item != null && item.IsCrownVariant);
        }

        public AccessoryItem GetDefaultGlasses()
        {
            return accessoryItems.FirstOrDefault(item => item != null && item.IsGlassesNone)
                ?? accessoryItems.FirstOrDefault(item => item != null && item.IsGlassesVariant);
        }

        public AccessoryItem GetDefaultBag()
        {
            return accessoryItems.FirstOrDefault(item => item != null && item.IsBagNone)
                ?? accessoryItems.FirstOrDefault(item => item != null && item.IsBagVariant);
        }

        public IEnumerable<MakeupItem> GetMakeupItems(MakeupType type)
        {
            return makeupItems.Where(item => item != null && item.MakeupType == type);
        }

        public HairItem GetDefaultHair() =>
            GetHairById("hair_3") ?? hairItems.FirstOrDefault(item => item != null);
        public DressItem GetDefaultDress() => dressItems.FirstOrDefault(item => item != null);
        public AccessoryItem GetDefaultAccessory() => accessoryItems.FirstOrDefault(item => item != null && item.IsNoneOption)
            ?? accessoryItems.FirstOrDefault(item => item != null);

        public MakeupItem GetDefaultMakeup(MakeupType type)
        {
            return GetMakeupItems(type).FirstOrDefault(item => item.IsNoneOption)
                ?? GetMakeupItems(type).FirstOrDefault();
        }

#if UNITY_EDITOR
        public void SetHairColorLibrary(HairColorLibrary library) => hairColorLibrary = library;

        public void SetHairItems(List<HairItem> items) => hairItems = items;
        public void SetDressItems(List<DressItem> items) => dressItems = items;
        public void SetShoeItems(List<ShoeItem> items) => shoeItems = items;
        public void SetMakeupItems(List<MakeupItem> items) => makeupItems = items;
        public void SetAccessoryItems(List<AccessoryItem> items) => accessoryItems = items;
#endif
    }
}
