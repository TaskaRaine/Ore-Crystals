﻿using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace OreCrystals
{
    class OreCrystalsCrystal : Block
    {
        private const int CRYSTAL_DURABILITY_DAMAGE = 5;

        WorldInteraction[] interactions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (FirstCodePart() == "orecrystals_crystal_poor") return;

            if (this.FirstCodePart() == "seed_crystals")
                interactions = ObjectCacheUtil.GetOrCreate(api, "crystalSeedsInteractions", () =>
                {
                    return new WorldInteraction[] {
                        new WorldInteraction()
                        {
                            ActionLangCode = "orecrystals:blockhelp-crystal-seed-take",
                            MouseButton = EnumMouseButton.Left
                        }
                    };
                });
            else
                interactions = ObjectCacheUtil.GetOrCreate(api, "crystalBlockInteractions", () =>
                {
                    List<ItemStack> chiselStackList = new List<ItemStack>();
                    List<ItemStack> pickaxeStackList = new List<ItemStack>();

                    foreach (Item item in api.World.Items)
                    {
                        if (item.Code == null) continue;

                        if (item.Tool == EnumTool.Chisel)
                        {
                            chiselStackList.Add(new ItemStack(item));
                        }
                        else if(item.Tool == EnumTool.Pickaxe)
                        {
                            pickaxeStackList.Add(new ItemStack(item));
                        }
                    }
                    return new WorldInteraction[] {
                        new WorldInteraction()
                        {
                            ActionLangCode = "orecrystals:blockhelp-crystal-harvest",
                            MouseButton = EnumMouseButton.Left,
                            Itemstacks = chiselStackList.ToArray()
                        },
                        new WorldInteraction()
                        {
                            ActionLangCode = "orecrystals:blockhelp-crystal-break",
                            MouseButton = EnumMouseButton.Left,
                            Itemstacks = pickaxeStackList.ToArray()
                        }
                    };
                });
        }

        //-- If the block that this crystal is butting up against is broken...the crystal should break, too --//
        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            string codeLastPart = LastCodePart(0);

            switch (codeLastPart)
            {
                case "ore_up":
                    if (world.BlockAccessor.GetBlockId(pos.UpCopy(1), BlockLayersAccess.SolidBlocks) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null, 0);
                    }
                    break;
                case "ore_down":
                    if (world.BlockAccessor.GetBlockId(pos.DownCopy(1), BlockLayersAccess.SolidBlocks) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null, 0);
                    }
                    break;
                case "ore_north":
                    if (world.BlockAccessor.GetBlockId(pos.NorthCopy(1), BlockLayersAccess.SolidBlocks) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null, 0);
                    }
                    break;
                case "ore_south":
                    if (world.BlockAccessor.GetBlockId(pos.SouthCopy(1), BlockLayersAccess.SolidBlocks) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null, 0);
                    }
                    break;
                case "ore_east":
                    if (world.BlockAccessor.GetBlockId(pos.EastCopy(1), BlockLayersAccess.SolidBlocks) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null, 0);
                    }
                    break;
                case "ore_west":
                    if (world.BlockAccessor.GetBlockId(pos.WestCopy(1), BlockLayersAccess.SolidBlocks) == 0)
                    {
                        world.BlockAccessor.BreakBlock(pos, null, 0);
                    }
                    break;
                default:
                    break;
            }
        }

        //-- When a block is placed, ensure that the crystal block is rotated to butt up against the face the player is looking at --//
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            string[] blockCode = this.Code.Path.Split('-');
            string newCode;

            if (blockSel.Face == BlockFacing.UP)
            {
                blockCode[blockCode.Length - 1] = "ore_down";
            }
            else if (blockSel.Face == BlockFacing.DOWN)
            {
                blockCode[blockCode.Length - 1] = "ore_up";
            }
            else if (blockSel.Face == BlockFacing.NORTH)
            {
                blockCode[blockCode.Length - 1] = "ore_south";
            }
            else if (blockSel.Face == BlockFacing.SOUTH)
            {
                blockCode[blockCode.Length - 1] = "ore_north";
            }
            else if (blockSel.Face == BlockFacing.EAST)
            {
                blockCode[blockCode.Length - 1] = "ore_west";
            }
            else if (blockSel.Face == BlockFacing.WEST)
            {
                blockCode[blockCode.Length - 1] = "ore_east";
            }

            newCode = string.Join("-", blockCode);

            int id = world.GetBlock(new AssetLocation("orecrystals", newCode)).Id;

            try
            {
                if (world.BlockAccessor.GetSolidBlock(blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z).IsReplacableBy(this))
                {
                    world.BlockAccessor.SetBlock(id, blockSel.Position);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (byPlayer != null)
            {
                ItemStack[] drops = this.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);

                if (this.FirstCodePart() != "seed_crystals")
                {
                    if (byPlayer.InventoryManager.ActiveTool == EnumTool.Chisel)
                    {
                        //-- If the block is broken with a chisel, return the crystal to its 'poor' state and damage the chisel --//
                        Block harvestedCrystal = world.GetBlock(new AssetLocation("orecrystals", "orecrystals_crystal_poor-" + this.FirstCodePart(1) + "-" + this.LastCodePart()));

                        if (harvestedCrystal.Id != world.BlockAccessor.GetBlockId(pos, BlockLayersAccess.SolidBlocks))
                            world.BlockAccessor.SetBlock(harvestedCrystal.Id, pos);
                        else
                            world.BlockAccessor.SetBlock(0, pos);

                        if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
                            byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.DamageItem(world, byPlayer.Entity, byPlayer.InventoryManager.ActiveHotbarSlot, CRYSTAL_DURABILITY_DAMAGE);
                    }
                    else if (byPlayer.InventoryManager.ActiveTool == EnumTool.Pickaxe)
                    {
                        //-- If the block is broken with a pickaxe, damage the tool and destroy the block --//
                        world.BlockAccessor.SetBlock(0, pos);

                        if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
                        {
                            //-- Vintage Story applies durability damage on block break with a pickaxe. If it's already 0, it causes an exception --//
                            if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Durability - CRYSTAL_DURABILITY_DAMAGE > 0)
                                byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.DamageItem(world, byPlayer.Entity, byPlayer.InventoryManager.ActiveHotbarSlot, CRYSTAL_DURABILITY_DAMAGE - 1);
                            else
                                byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Durability = 1;
                        }
                    }
                    else
                    {
                        world.BlockAccessor.SetBlock(0, pos);
                    }
                }
                else if(this.FirstCodePart() == "seed_crystals")
                {
                    //-- If the block is a seed crystal, broken with anything, remove the block and set the drops to only be the seed crystal --//
                    world.BlockAccessor.SetBlock(0, pos);
                    drops = new ItemStack[1];

                    drops[0] = new ItemStack(this);
                }

                if(drops != null)
                {
                    for(int i = 0; i < drops.Length; i++)
                    {
                        if (SplitDropStacks)
                        {
                            for (int k = 0; k < drops[i].StackSize; k++)
                            {
                                ItemStack stack = drops[i].Clone();
                                stack.StackSize = 1;
                                world.SpawnItemEntity(stack, new Vec3d(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5), null);
                            }
                        }
                    }
                }
            }
            else
            {
                world.BlockAccessor.SetBlock(0, pos);
            }

            if (api.Side == EnumAppSide.Server)
                world.PlaySoundAt(Sounds.GetBreakSound(byPlayer), pos.X, pos.Y, pos.Z, byPlayer, true, 32, 1);
            else
                BreakParticles(pos);
        }
        // -- Break particles have been moved from the heart to the block to allow glowing break particles to occur with just normal block breaking methods --//
        private SimpleParticleProperties InitBreakParticles(Vec3d pos)
        {
            Random rand = new Random();
            Vec3f velocityRand = new Vec3f((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()) * 6;

            return new SimpleParticleProperties()
            {
                MinPos = new Vec3d(pos.X, pos.Y, pos.Z),
                AddPos = new Vec3d(1, 1, 1),

                MinVelocity = new Vec3f(velocityRand.X, velocityRand.Y, velocityRand.Z),
                AddVelocity = new Vec3f(-velocityRand.X, -velocityRand.Y, -velocityRand.Z) * 2,

                GravityEffect = 0.2f,

                MinSize = 0.1f,
                MaxSize = 0.4f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.1f),

                MinQuantity = 20,
                AddQuantity = 40,

                LifeLength = 1.2f,
                addLifeLength = 1.4f,

                ShouldDieInLiquid = false,

                WithTerrainCollision = true,

                Color = CrystalColour.GetColour(FirstCodePart(1)),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                Bounciness = 0.4f,

                VertexFlags = 150,

                ParticleModel = EnumParticleModel.Cube
            };
        }
        public void BreakParticles(BlockPos pos)
        {
            api.World.SpawnParticles(InitBreakParticles(pos.ToVec3d()));
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions;
        }
    }
}
