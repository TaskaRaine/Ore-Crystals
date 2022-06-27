using Vintagestory.API;
using Vintagestory.API.Common.Entities;

namespace OreCrystals
{
    class BehaviorIlluminate : EntityBehavior
    {
        private byte[] lightHsv = new byte[] { 0, 0, 6 };

        public override string PropertyName()
        {
            return "illuminate";
        }
        public BehaviorIlluminate(Entity entity) : base(entity)
        {
            //-- A hack way to force only crystal arrows to illuminate. Other arrows are given the behaviour, but won't emit light. --//
            //-- This makes it so that Ore Crystals won't overwrite behaviour changes from other mods --//
            if(entity.LastCodePart() == "crystal")
                entity.LightHsv = lightHsv;
        }
        
    }
}
