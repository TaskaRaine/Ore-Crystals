using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace OreCrystals
{
    class OreCrystalsRegisterItems : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterItemClass("orecrystals_crystalshard", typeof(OreCrystalsCrystalShard));
            api.RegisterItemClass("ItemCrystalHeart", typeof(ItemCrystalHeart));
        }
    }
}
