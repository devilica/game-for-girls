using System;

namespace DressUpGame.Pet
{
    [Serializable]
    public class PetSaveData
    {
        public string petId;
        public string hairbowId;
        public string collarId;
        public string glassesId;

        public PetSaveData Clone()
        {
            return new PetSaveData
            {
                petId = petId,
                hairbowId = hairbowId,
                collarId = collarId,
                glassesId = glassesId
            };
        }
    }
}
