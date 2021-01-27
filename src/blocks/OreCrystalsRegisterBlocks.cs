using Vintagestory.API.Common;

namespace OreCrystals
{
    class OreCrystalsRegisterBlocks : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("orecrystals_crystal_poor", typeof(OreCrystalsCrystal));
            api.RegisterBlockClass("orecrystals_crystal_medium", typeof(OreCrystalsCrystal));
            api.RegisterBlockClass("orecrystals_crystal_rich", typeof (OreCrystalsCrystal));
            api.RegisterBlockClass("orecrystals_crystal_bountiful", typeof(OreCrystalsCrystal));

            api.RegisterBlockClass("orecrystals_glass", typeof(OreCrystalsGlass));

            api.RegisterBlockClass("orecrystals_pottedcrystal", typeof(Block));

            api.RegisterBlockClass("CrystalObeliskBottom", typeof(CrystalObeliskBottom));
            api.RegisterBlockClass("CrystalObeliskTop", typeof(CrystalObeliskTop));
        }
    }
}
