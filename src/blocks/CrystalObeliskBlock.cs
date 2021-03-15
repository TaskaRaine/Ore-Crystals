using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace OreCrystals
{
    class CrystalObeliskBlock : Block
    {
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);

            world.BlockAccessor.WalkBlocks(new BlockPos(pos.X - 1, pos.Y - 1, pos.Z - 1), new BlockPos(pos.X + 1, pos.Y + 1, pos.Z + 1), (block, bpos) =>
            {
                if(block is CrystalObeliskBlock)
                {
                    world.BlockAccessor.BreakBlock(bpos, byPlayer, dropQuantityMultiplier);
                }
            }, true);

            world.BlockAccessor.Commit();
        }
    }
}
