using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }
    }
}
