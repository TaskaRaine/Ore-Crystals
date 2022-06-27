﻿using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace OreCrystals
{
    class CrystalObeliskBlock : Block
    {
        private const int OBELISK_DURABILITY_DAMAGE = 5;

        WorldInteraction[] interactions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            interactions = ObjectCacheUtil.GetOrCreate(api, "crystalObeliskInteractions", () =>
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
                    else if (item.Tool == EnumTool.Pickaxe)
                    {
                        pickaxeStackList.Add(new ItemStack(item));
                    }
                }

                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "orecrystals:blockhelp-crystal-break",
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
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);

            //-- When this block is broken, break any obelisk blocks that are positioned alongside it --//
            world.BlockAccessor.WalkBlocks(new BlockPos(pos.X - 1, pos.Y - 1, pos.Z - 1), new BlockPos(pos.X + 1, pos.Y + 1, pos.Z + 1), (block, xPos, yPos, zPos) =>
            {
                if(block is CrystalObeliskBlock)
                {
                    world.BlockAccessor.BreakBlock(new BlockPos(xPos, yPos, zPos), byPlayer, dropQuantityMultiplier);

                    if(byPlayer.InventoryManager.ActiveTool == EnumTool.Chisel)
                    {
                        if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
                            byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.DamageItem(world, byPlayer.Entity, byPlayer.InventoryManager.ActiveHotbarSlot, OBELISK_DURABILITY_DAMAGE);
                    }
                    else if (byPlayer.InventoryManager.ActiveTool == EnumTool.Pickaxe)
                    {
                        //-- Vintage Story applies durability damage on block break with a pickaxe. If it's already 0, it causes an exception --//
                        if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Durability - OBELISK_DURABILITY_DAMAGE > 0)
                            byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.DamageItem(world, byPlayer.Entity, byPlayer.InventoryManager.ActiveHotbarSlot, OBELISK_DURABILITY_DAMAGE - 1);
                        else
                            byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Durability = 1;
                    }
                }
            }, true);

            world.BlockAccessor.Commit();
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions;
        }
    }
}
