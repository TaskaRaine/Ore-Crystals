using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace OreCrystals
{
    class OreCrystalsCrystal : Block
    {
        //-- If the block that this crystal is butting up against is broken...the crystal should break, too --//
        //-- Unexpected pos.DirectionCopy(WestCopy on an 'ore_north' block) is likely due to block rotations? --//
        public override void OnNeighourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            string codeLastPart = LastCodePart(0);

            switch(codeLastPart)
            {
                case "ore_up":
                    if (world.BlockAccessor.GetBlockId(pos.UpCopy(1)) == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Up: " + pos + ", " + pos.UpCopy(1));
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_down":
                    if(world.BlockAccessor.GetBlockId(pos.DownCopy(1)) == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Down: " + pos + ", " + pos.DownCopy(1));
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_north":
                    if (world.BlockAccessor.GetBlockId(pos.WestCopy(1)) == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("North: " + pos + ", " + pos.WestCopy(1));
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_south":
                    if (world.BlockAccessor.GetBlockId(pos.EastCopy(1)) == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("South: " + pos + ", " + pos.EastCopy(1));
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_east":
                    if(world.BlockAccessor.GetBlockId(pos.NorthCopy(1)) == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("East: " + pos + ", " + pos.NorthCopy(1));
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                case "ore_west":
                    if(world.BlockAccessor.GetBlockId(pos.SouthCopy(1)) == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("West: " + pos + ", " + pos.SouthCopy(1));
                        world.BlockAccessor.BreakBlock(pos, null);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
