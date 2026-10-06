using System.Collections.Generic;

namespace OniExtract
{
    public class OutMutantPlant
    {
        public Tag speciesID;
        public Tag cachedSubspeciesID;
        public List<string> mutationIDs;

        public OutMutantPlant(MutantPlant obj)
        {
            this.speciesID = obj.SpeciesID;
            this.cachedSubspeciesID = obj.SubSpeciesID;
            this.mutationIDs = obj.MutationIDs;
        }
    }
}
