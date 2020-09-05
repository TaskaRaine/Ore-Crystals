using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace OreCrystals
{
    class GenerateOreCrystals : ModSystem
    {
        private ICoreServerAPI api;
        private IBlockAccessor worldBlockAccessor;
        private int chunkSize;

        //-- A structure to hold information about an individual crystal. --//
        struct OreCrystal
        {
            public string neighbourOreCode;
            public int crystalIndex;
            public string orientation;
            public string quality;
            public string oreType;

            public OreCrystal(string oreCode, int index, string orient)
            {
                neighbourOreCode = oreCode;
                crystalIndex = index;
                orientation = orient;

                string[] oreSplit = neighbourOreCode.Split('-');

                //-- The ore string is split to extract the variant information for use later --//
                //-- Case 4 includes any ore that has a quality value(bountiful, rich, medium, or poor) --//
                //-- Case 3 includes any ore that doesn't have a quality value. Quartz, for example. Quality is defaulted to 'poor' --//
                switch (oreSplit.Length)
                {
                    case 4:
                        oreType = oreSplit[2];
                        quality = oreSplit[1];
                        break;
                    case 3:
                        oreType = oreSplit[1];
                        quality = "poor";
                        break;
                    default:
                        oreType = null;
                        quality = null;
                        break;
                }
            }
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            this.worldBlockAccessor = api.World.BlockAccessor;
            this.chunkSize = worldBlockAccessor.ChunkSize;

            this.api.Event.ChunkColumnGeneration(CrystalGen, EnumWorldGenPass.TerrainFeatures, "standard");
        }

        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }
        //-- Execute Order 0.3 makes sure that this mod is run AFTER the vanilla game has generated the ore deposits. Vanilla ore gen is set to 0.2(?) --//
        public override double ExecuteOrder()
        {
            return 0.3;
        }
        //-- Checks every block in the chunk. If that block is an ore within the oreCodeDict, add its position to the chunk ore dictionary as its unique ID, and the code, for use later --// 
        private void CrystalGen(IServerChunk[] chunks, int chunkX, int chunkZ, ITreeAttribute chunkGenParams)
        {
            for (int chunkCount = 0; chunkCount < chunks.Length; chunkCount++)
            {
                Dictionary<Vec3i, string> chunkOre = new Dictionary<Vec3i, string>();

                for (int x = 0; x < chunkSize; x++)
                {
                    for (int y = 0; y < chunkSize; y++)
                    {
                        for (int z = 0; z < chunkSize; z++)
                        {
                            int key = chunks[chunkCount].Blocks[(y * chunkSize + z) * chunkSize + x];
                            Block block = api.World.GetBlock(key);

                            if (block is BlockOre)
                            {
                                string code = block.Code.Path;

                                chunkOre.Add(new Vec3i(x, y, z), code);
                            }
                        }
                    }
                }

                int neighbourKey, neighbourIndex;
                
                //-- For every ore within the chunk, check its neighbours for an open space. If it's available, a new crystal is placed --//
                foreach(KeyValuePair<Vec3i, string> pair in chunkOre)
                {
                    //-- Check space UP(y + 1) for crystal spawn --//
                    if (pair.Key.Y < 31)
                    {
                        neighbourIndex = ((pair.Key.Y + 1) * chunkSize + pair.Key.Z) * chunkSize + pair.Key.X;
                        neighbourKey = chunks[chunkCount].Blocks[neighbourIndex];

                        if (neighbourKey == 0)
                        {
                            CreateNewCrystal(new OreCrystal(pair.Value, neighbourIndex, "ore_down"), chunks[chunkCount]);
                        }
                    }
                    //-- Check space DOWN(y - 1) for crystal spawn --//
                    if (pair.Key.Y > 0)
                    {
                        neighbourIndex = ((pair.Key.Y - 1) * chunkSize + pair.Key.Z) * chunkSize + pair.Key.X;
                        neighbourKey = chunks[chunkCount].Blocks[neighbourIndex];

                        if (neighbourKey == 0)
                        {
                            CreateNewCrystal(new OreCrystal(pair.Value, neighbourIndex, "ore_up"), chunks[chunkCount]);
                        }
                    }
                    //-- Check space SOUTH(z + 1) for crystal spawn --//
                    if (pair.Key.Z < 31)
                    {
                        neighbourIndex = (pair.Key.Y * chunkSize + (pair.Key.Z + 1)) * chunkSize + pair.Key.X;
                        neighbourKey = chunks[chunkCount].Blocks[neighbourIndex];

                        if (neighbourKey == 0)
                        {
                            CreateNewCrystal(new OreCrystal(pair.Value, neighbourIndex, "ore_north"), chunks[chunkCount]);
                        }
                    }
                    //-- Check space NORTH(z - 1) for crystal spawn --//
                    if (pair.Key.Z > 0)
                    {
                        neighbourIndex = (pair.Key.Y * chunkSize + (pair.Key.Z - 1)) * chunkSize + pair.Key.X;
                        neighbourKey = chunks[chunkCount].Blocks[neighbourIndex];

                        if (neighbourKey == 0)
                        {
                            CreateNewCrystal(new OreCrystal(pair.Value, neighbourIndex, "ore_south"), chunks[chunkCount]);
                        }
                    }
                    //-- Check space EAST(x + 1) for crystal spawn --//
                    if (pair.Key.X < 31)
                    {
                        neighbourIndex = (pair.Key.Y * chunkSize + pair.Key.Z) * chunkSize + (pair.Key.X + 1);
                        neighbourKey = chunks[chunkCount].Blocks[neighbourIndex];

                        if (neighbourKey == 0)
                        {
                            CreateNewCrystal(new OreCrystal(pair.Value, neighbourIndex, "ore_west"), chunks[chunkCount]);
                        }
                    }
                    //-- Check space WEST(x - 1) for crystal spawn --//
                    if (pair.Key.X > 0)
                    {
                        neighbourIndex = (pair.Key.Y * chunkSize + pair.Key.Z) * chunkSize + (pair.Key.X - 1);
                        neighbourKey = chunks[chunkCount].Blocks[neighbourIndex];

                        if (neighbourKey == 0)
                        {
                            CreateNewCrystal(new OreCrystal(pair.Value, neighbourIndex, "ore_east"), chunks[chunkCount]);
                        }
                    }
                }
                //-- Updates the chunk data after all crystals have been generated --//
                chunks[chunkCount].MarkModified();
            }
        }
        //-- Creates a code string that is then used to search the crystal dictionary for the associated crystal ID, then changes the ID of the air block to that found ID --//  
        private void CreateNewCrystal(OreCrystal crystal, IServerChunk chunk)
        {
            string codeName = "orecrystals_crystal_" + crystal.quality + "-" + crystal.oreType + "-" + crystal.orientation;
            int crystalID = api.WorldManager.GetBlockId(new AssetLocation("orecrystals", codeName));

            chunk.Blocks[crystal.crystalIndex] = crystalID;
        }
    }
}
