using Vintagestory.API.Common;

namespace OreCrystals
{
    class RegiterBlockEntities: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockEntityClass("BlockEntityCrystalObelisk", typeof(BlockEntityCrystalObelisk));
            api.RegisterBlockEntityClass("BlockEntityCrystalObeliskSpawner", typeof(BlockEntityCrystalObeliskSpawner));
        }
    }
}
