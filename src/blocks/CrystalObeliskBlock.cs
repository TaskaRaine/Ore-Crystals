using System;
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
            //-- When this block is broken, break any obelisk blocks that are positioned alongside it --//
            world.BlockAccessor.WalkBlocks(new BlockPos(pos.X - 1, pos.Y - 1, pos.Z - 1), new BlockPos(pos.X + 1, pos.Y + 1, pos.Z + 1), (block, xPos, yPos, zPos) =>
            {
                if(block is CrystalObeliskBlock)
                {
                    world.BlockAccessor.SetBlock(0, new BlockPos(xPos, yPos, zPos));

                    if(byPlayer != null)
                    {
                        ItemStack[] drops = this.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);

                        if (byPlayer.InventoryManager.ActiveTool == EnumTool.Chisel)
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

                        if (drops != null)
                        {
                            for (int i = 0; i < drops.Length; i++)
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
                    if(api.Side == EnumAppSide.Server)
                        world.PlaySoundAt(Sounds.GetBreakSound(byPlayer), pos.X, pos.Y, pos.Z, byPlayer, true, 32, 1);
                    else
                        BreakParticles(pos);
                }
            }, true);

            world.BlockAccessor.Commit();
        }
        // -- Break particles have been moved from the heart to the block to allow glowing break particles to occur with just normal block breaking methods --//
        private SimpleParticleProperties InitBreakParticles(Vec3d pos)
        {
            Random rand = new Random();
            Vec3f velocityRand = new Vec3f((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()) * 6;

            return new SimpleParticleProperties()
            {
                MinPos = new Vec3d(pos.X, pos.Y + .25, pos.Z),
                AddPos = new Vec3d(1, 1, 1),

                MinVelocity = new Vec3f(velocityRand.X, velocityRand.Y, velocityRand.Z),
                AddVelocity = new Vec3f(-velocityRand.X, -velocityRand.Y, -velocityRand.Z) * 2,

                GravityEffect = 0.2f,

                MinSize = 0.3f,
                MaxSize = 0.6f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.1f),

                MinQuantity = 30,
                AddQuantity = 60,

                LifeLength = 1.2f,
                addLifeLength = 1.4f,

                ShouldDieInLiquid = false,

                WithTerrainCollision = true,

                Color = CrystalColour.GetColour(FirstCodePart(3)),
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
