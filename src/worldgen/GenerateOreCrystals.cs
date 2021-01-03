﻿using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace OreCrystals
{
    enum CrystalDirection
    {
        UP = 0,
        DOWN = 1,
        SOUTH = 2,
        NORTH = 3,
        EAST = 4,
        WEST = 5
    }

    class GenerateOreCrystals : ModSystem
    {
        private ICoreServerAPI api;
        private IBlockAccessor worldBlockAccessor;
        private IBlockAccessor chunkBlockAccessor;
        private int chunkSize;

        private LCGRandom worldSeedRand;

        //-- A structure to hold information about an individual crystal. --//
        struct OreCrystal
        {
            public string neighbourOreCode;
            public BlockPos crystalPos;
            public string orientation;
            public string quality;
            public string oreType;

            public OreCrystal(string oreCode, BlockPos pos, string orient)
            {
                neighbourOreCode = oreCode;
                crystalPos = pos;
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

            this.worldSeedRand = new LCGRandom(api.World.Seed);

            this.api.Event.GetWorldgenBlockAccessor(OnWorldGenBlockAccessor);

            this.api.Event.ChunkColumnGeneration(CrystalGen, EnumWorldGenPass.Vegetation, "standard");
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
        private void OnWorldGenBlockAccessor(IChunkProviderThread chunkProvider)
        {
            chunkBlockAccessor = chunkProvider.GetBlockAccessor(true);
        }
        //-- Checks every block in the chunk. If that block is an ore within the oreCodeDict, pass the block position and ore code to the SpawnCrystals method. --// 
        private void CrystalGen(IServerChunk[] chunks, int chunkX, int chunkZ, ITreeAttribute chunkGenParams)
        {
            BlockPos blockPos = new BlockPos();

            for (int x = 0; x < chunkSize; x++)
            {
                blockPos.X = chunkX * chunkSize + x;

                for (int z = 0; z < chunkSize; z++)
                {
                    blockPos.Z = chunkZ * chunkSize + z;

                    for (int y = 0; y < worldBlockAccessor.GetTerrainMapheightAt(blockPos); y++)
                    {
                        blockPos.Y = y;

                        Block block = chunkBlockAccessor.GetBlock(blockPos);
                        if (block is BlockOre)
                        {
                            string code = block.Code.Path;

                            SpawnCrystals(blockPos, code);
                        }
                    }
                }
            }
        }

        //-- Check neighbouring block positions for air blocks --//
        private void SpawnCrystals(BlockPos blockPos, string code)
        {
            EnumBlockMaterial neighbourBlockMaterial;
            BlockPos neighbourPos;

            //-- Check space UP(y + 1) for crystal spawn --//
            if (blockPos.Y < worldBlockAccessor.MapSizeY)
            {
                neighbourPos = GetNeighbour(CrystalDirection.UP, blockPos);
                neighbourBlockMaterial = chunkBlockAccessor.GetBlock(neighbourPos).BlockMaterial;

                if (neighbourBlockMaterial == EnumBlockMaterial.Air || neighbourBlockMaterial == EnumBlockMaterial.Plant)
                {
                    CreateNewCrystal(new OreCrystal(code, neighbourPos, "ore_down"));
                }
            }

            //-- Check space DOWN(y - 1) for crystal spawn --//
            if (blockPos.Y > 0)
            {
                neighbourPos = GetNeighbour(CrystalDirection.DOWN, blockPos);
                neighbourBlockMaterial = chunkBlockAccessor.GetBlock(neighbourPos).BlockMaterial;

                if (neighbourBlockMaterial == EnumBlockMaterial.Air || neighbourBlockMaterial == EnumBlockMaterial.Plant)
                {
                    CreateNewCrystal(new OreCrystal(code, neighbourPos, "ore_up"));
                }
            }
            //-- Check space SOUTH(z + 1) for crystal spawn --//
            if (blockPos.Z < worldBlockAccessor.MapSizeZ)
            {
                neighbourPos = GetNeighbour(CrystalDirection.SOUTH, blockPos);
                neighbourBlockMaterial = chunkBlockAccessor.GetBlock(neighbourPos).BlockMaterial;

                if (neighbourBlockMaterial == EnumBlockMaterial.Air || neighbourBlockMaterial == EnumBlockMaterial.Plant)
                {
                    CreateNewCrystal(new OreCrystal(code, neighbourPos, "ore_north"));
                }
            }
            //-- Check space NORTH(z - 1) for crystal spawn --//
            if (blockPos.Z > 0)
            {
                neighbourPos = GetNeighbour(CrystalDirection.NORTH, blockPos);
                neighbourBlockMaterial = chunkBlockAccessor.GetBlock(neighbourPos).BlockMaterial;

                if (neighbourBlockMaterial == EnumBlockMaterial.Air || neighbourBlockMaterial == EnumBlockMaterial.Plant)
                {
                    CreateNewCrystal(new OreCrystal(code, neighbourPos, "ore_south"));
                }
            }
            //-- Check space EAST(x + 1) for crystal spawn --//
            if (blockPos.X < worldBlockAccessor.MapSizeX)
            {
                neighbourPos = GetNeighbour(CrystalDirection.EAST, blockPos);
                neighbourBlockMaterial = chunkBlockAccessor.GetBlock(neighbourPos).BlockMaterial;

                if (neighbourBlockMaterial == EnumBlockMaterial.Air || neighbourBlockMaterial == EnumBlockMaterial.Plant)
                {
                    CreateNewCrystal(new OreCrystal(code, neighbourPos, "ore_west"));
                }
            }
            //-- Check space WEST(x - 1) for crystal spawn --//
            if (blockPos.X > 0)
            {
                neighbourPos = GetNeighbour(CrystalDirection.WEST, blockPos);
                neighbourBlockMaterial = chunkBlockAccessor.GetBlock(neighbourPos).BlockMaterial;

                if (neighbourBlockMaterial == EnumBlockMaterial.Air || neighbourBlockMaterial == EnumBlockMaterial.Plant)
                {
                    CreateNewCrystal(new OreCrystal(code, neighbourPos, "ore_east"));
                }
            }
        }

        //-- Creates a code string that is then used to search the crystal dictionary for the associated crystal ID, then changes the ID of the air block to that found ID --//  
        private void CreateNewCrystal(OreCrystal crystal)
        {
            string codeName = "orecrystals_crystal_" + crystal.quality + "-" + crystal.oreType + "-" + crystal.orientation;
            int crystalID = api.WorldManager.GetBlockId(new AssetLocation("orecrystals", codeName));

            OreCrystalsCrystal newCrystal = new OreCrystalsCrystal
            {
                BlockId = crystalID
            };

            newCrystal.TryPlaceBlockForWorldGen(chunkBlockAccessor, crystal.crystalPos, BlockFacing.UP, this.worldSeedRand);
        }
        
        private BlockPos GetNeighbour(CrystalDirection direction, BlockPos blockPos)
        {
            BlockPos neighbour = new BlockPos();

            switch(direction)
            {
                case CrystalDirection.UP:
                    neighbour.X = blockPos.X;
                    neighbour.Y = blockPos.Y + 1;
                    neighbour.Z = blockPos.Z;

                    break;
                case CrystalDirection.DOWN:
                    neighbour.X = blockPos.X;
                    neighbour.Y = blockPos.Y - 1;
                    neighbour.Z = blockPos.Z;

                    break;
                case CrystalDirection.SOUTH:
                    neighbour.X = blockPos.X;
                    neighbour.Y = blockPos.Y;
                    neighbour.Z = blockPos.Z + 1;

                    break;
                case CrystalDirection.NORTH:
                    neighbour.X = blockPos.X;
                    neighbour.Y = blockPos.Y;
                    neighbour.Z = blockPos.Z - 1;

                    break;
                case CrystalDirection.EAST:
                    neighbour.X = blockPos.X + 1;
                    neighbour.Y = blockPos.Y;
                    neighbour.Z = blockPos.Z;

                    break;
                case CrystalDirection.WEST:
                    neighbour.X = blockPos.X - 1;
                    neighbour.Y = blockPos.Y;
                    neighbour.Z = blockPos.Z;

                    break;
            }
            return neighbour;
        }
    }
}
