using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace OreCrystals
{
    class OreCrystalsCrystal : Block
    {
        public override void OnServerGameTick(IWorldAccessor world, BlockPos pos, object extra = null)
        {
            base.OnServerGameTick(world, pos, extra);

            System.Diagnostics.Debug.WriteLine("Derp");
        }
        //-- If the block that this crystal is butting up against is broken...the crystal should break, too --//
        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            string codeLastPart = LastCodePart(0);

            switch(codeLastPart)
            {
                case "ore_up":
                    if (world.BlockAccessor.GetBlockId(pos.UpCopy(1)) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_down":
                    if(world.BlockAccessor.GetBlockId(pos.DownCopy(1)) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_north":
                    if (world.BlockAccessor.GetBlockId(pos.NorthCopy(1)) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_south":
                    if (world.BlockAccessor.GetBlockId(pos.SouthCopy(1)) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_east":
                    if(world.BlockAccessor.GetBlockId(pos.EastCopy(1)) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_west":
                    if(world.BlockAccessor.GetBlockId(pos.WestCopy(1)) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                default:
                    break;
            }
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            string[] blockCode = this.Code.Path.Split('-');
            string newCode;

            if(blockSel.Face == BlockFacing.UP)
            {
                blockCode[blockCode.Length - 1] = "ore_down";
            }
            else if(blockSel.Face == BlockFacing.DOWN)
            {
                blockCode[blockCode.Length - 1] = "ore_up";
            }
            else if(blockSel.Face == BlockFacing.NORTH)
            {
                blockCode[blockCode.Length - 1] = "ore_south";
            }
            else if(blockSel.Face == BlockFacing.SOUTH)
            {
                blockCode[blockCode.Length - 1] = "ore_north";
            }
            else if(blockSel.Face == BlockFacing.EAST)
            {
                blockCode[blockCode.Length - 1] = "ore_west";
            }
            else if(blockSel.Face == BlockFacing.WEST)
            {
                blockCode[blockCode.Length - 1] = "ore_east";
            }

            newCode = string.Join("-", blockCode);

            int id = world.GetBlock(new AssetLocation("orecrystals", newCode)).Id;

            try
            {
                world.BlockAccessor.SetBlock(id, blockSel.Position);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
