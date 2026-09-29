using System;

namespace DressUpGame.Data
{
    /// <summary>
    /// Serializable snapshot of the player's current customization choices.
    /// </summary>
    [Serializable]
    public class CharacterSaveData
    {
        public string hairId;
        public string hairColorPreset;
        public string dressId;
        public string lipstickId;
        public string eyesId;
        public string eyeshadowId;
        public string blushId;
        public string necklaceId;
        public string earringsId;
        public string crownId;
        public string glassesId;
        public string bagId;
        public string shoesId;
        public string accessoryId;

        public CharacterSaveData Clone()
        {
            return new CharacterSaveData
            {
                hairId = hairId,
                hairColorPreset = hairColorPreset,
                dressId = dressId,
                lipstickId = lipstickId,
                eyesId = eyesId,
                eyeshadowId = eyeshadowId,
                blushId = blushId,
                necklaceId = necklaceId,
                earringsId = earringsId,
                crownId = crownId,
                glassesId = glassesId,
                bagId = bagId,
                shoesId = shoesId,
                accessoryId = accessoryId
            };
        }
    }
}
