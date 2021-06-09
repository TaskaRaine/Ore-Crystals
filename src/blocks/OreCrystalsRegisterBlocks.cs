using Vintagestory.API.Common;

namespace OreCrystals
{
    class OreCrystalsRegisterBlocks : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("OreCrystalsCrystal", typeof(OreCrystalsCrystal));

            api.RegisterBlockClass("CrystalPlanter", typeof(CrystalPlanter));

            api.RegisterBlockClass("CrystalObeliskBlock", typeof(CrystalObeliskBlock));
        }
    }
}
