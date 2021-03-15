using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace OreCrystals
{
    class EntityCrystalHeart : Entity
    {
        private const int CRYSTAL_GROWTH_CHANCE = 1;
        private const int CRYSTAL_BREAK_CHANCE = 5;
        private const int CRYSTAL_GROWTH_RANGE = 9;

        private ICoreServerAPI sApi;
        private ICoreClientAPI cApi;

        IBlockAccessor blockAccessor;

        public SimpleParticleProperties heartAreaParticles;
        public SimpleParticleProperties obeliskAreaParticles;
        public SimpleParticleProperties crystalParticlesGrow;
        public SimpleParticleProperties heartAngerParticles;
        public SimpleParticleProperties crystalBreakParticles;

        private Random growthRand;

        private string heartVariant;
        private bool heartAngered = false;

        public EnumEntityActivity CurrentControls;

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            growthRand = new Random((int)this.EntityId);
            heartVariant = this.LastCodePart();

            this.LightHsv = CrystalColour.GetLight(heartVariant);

            if (api is ICoreServerAPI)
            {
                sApi = api as ICoreServerAPI;

                blockAccessor = sApi.World.GetBlockAccessorBulkUpdate(true, true);

                InitGrowParticles();
                InitBreakParticles();
                InitAngerParticles();
            }
            else
            {
                cApi = api as ICoreClientAPI;

                InitHeartAreaParticles();
                InitObeliskAreaParticles();
            }
        }
        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);

            if (Alive && !heartAngered)
            {
                if (Api.Side == EnumAppSide.Server)
                {
                    if (growthRand.Next(1, 101) <= CRYSTAL_GROWTH_CHANCE)
                    {
                        BlockPos crystalToGrowPos = GetNearbyCrystal(false);

                        if (crystalToGrowPos != null)
                            GrowCrystal(crystalToGrowPos);
                    }
                }
                else
                {
                    HandleAnimation();

                    this.World.SpawnParticles(heartAreaParticles);
                    this.World.SpawnParticles(obeliskAreaParticles);
                }
            }
            else if(Alive && heartAngered)
            {
                if (Api.Side == EnumAppSide.Server)
                {
                    this.World.SpawnParticles(heartAngerParticles);

                    if (growthRand.Next(1, 101) <= CRYSTAL_BREAK_CHANCE)
                    {
                        BlockPos crystalToBreakPos = GetNearbyCrystal(true);

                        if (crystalToBreakPos != null)
                        {
                            Block crystal = blockAccessor.GetBlock(crystalToBreakPos);

                            SetBreakParticlePosDirCol(crystalToBreakPos.ToVec3d(), crystal.LastCodePart(), crystal.FirstCodePart(1));
                            this.World.SpawnParticles(crystalBreakParticles);

                            BreakCrystal(crystalToBreakPos);

                            World.GetEntitiesInsideCuboid(crystalToBreakPos, new BlockPos(crystalToBreakPos.X + 1, crystalToBreakPos.Y + 1, crystalToBreakPos.Z + 1), (entity) =>
                            {
                                entity.ReceiveDamage(new DamageSource(), 2.0f);

                                return true;
                            });
                        }
                        else
                        {
                            DestroyHeart();
                        }
                    }
                }
                else
                {
                    HandleAnimation();
                }
            }
        }

        public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
        {
            base.OnInteract(byEntity, itemslot, hitPosition, mode);

            if(mode == EnumInteractMode.Attack)
            {
                DamageHeart(byEntity, itemslot, hitPosition);

                AngerHeart();
            }
            else
            {
                AttemptGiveHeart(byEntity, itemslot);
            }
        }
        public override void OnEntityDespawn(EntityDespawnReason despawn)
        {
            base.OnEntityDespawn(despawn);

            if(despawn.reason == EnumDespawnReason.Death)
            {
                BlockEntityCrystalObeliskSpawner heartSpawner = Api.World.BlockAccessor.GetBlockEntity(this.ServerPos.AsBlockPos - new BlockPos(1, 1, 1)) as BlockEntityCrystalObeliskSpawner;
                
                if(heartSpawner != null)
                    heartSpawner.SetHeartTaken();
            }
        }

        public void AngerHeart()
        {
            heartAngered = true;
        }

        private void HandleAnimation()
        {
            if (World.Side == EnumAppSide.Client)
            {
                if (Alive)
                {
                    if (heartAngered)
                        CurrentControls = EnumEntityActivity.Fly;
                    else
                        CurrentControls = EnumEntityActivity.Idle;
                }
                else
                    CurrentControls = EnumEntityActivity.Dead;

                AnimationMetaData defaultAnim = null;
                bool anyAverageAnimActive = false;
                bool skipDefaultAnim = false;

                AnimationMetaData[] animations = Properties.Client.Animations;
                for (int i = 0; animations != null && i < animations.Length; i++)
                {
                    AnimationMetaData anim = animations[i];
                    bool wasActive = AnimManager.ActiveAnimationsByAnimCode.ContainsKey(anim.Animation);
                    bool nowActive = anim.Matches((int)CurrentControls);
                    bool isDefaultAnim = anim.TriggeredBy.DefaultAnim == true;

                    anyAverageAnimActive |= nowActive || (wasActive && !anim.WasStartedFromTrigger);
                    skipDefaultAnim |= (nowActive || (wasActive && !anim.WasStartedFromTrigger)) && anim.SupressDefaultAnimation;

                    if (isDefaultAnim)
                    {
                        defaultAnim = anim;
                    }

                    if (!wasActive && nowActive)
                    {
                        anim.WasStartedFromTrigger = true;
                        AnimManager.StartAnimation(anim);
                    }

                    if (wasActive && !nowActive && (anim.WasStartedFromTrigger || isDefaultAnim))
                    {
                        anim.WasStartedFromTrigger = false;
                        AnimManager.StopAnimation(anim.Animation);
                    }
                }

                if (!anyAverageAnimActive && defaultAnim != null && Alive && !skipDefaultAnim)
                {
                    defaultAnim.WasStartedFromTrigger = true;
                    AnimManager.StartAnimation(defaultAnim);
                }

                if ((!Alive || skipDefaultAnim) && defaultAnim != null)
                {
                    AnimManager.StopAnimation(defaultAnim.Code);
                }
            }
        }
        private void DamageHeart(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition)
        {
            float damage = itemslot.Itemstack == null ? 0.5f : itemslot.Itemstack.Collectible.GetAttackPower(itemslot.Itemstack);
            int damagetier = itemslot.Itemstack == null ? 0 : itemslot.Itemstack.Collectible.ToolTier;

            damage *= byEntity.Stats.GetBlended("meleeWeaponsDamage");

            DamageSource dmgSource = new DamageSource()
            {
                Source = (byEntity as EntityPlayer).Player == null ? EnumDamageSource.Entity : EnumDamageSource.Player,
                SourceEntity = byEntity,
                Type = EnumDamageType.BluntAttack,
                HitPosition = hitPosition,
                DamageTier = damagetier
            };

            if (!itemslot.Empty)
                itemslot.Itemstack.Collectible.OnAttackingWith(byEntity.World, byEntity, this, itemslot);

            ReceiveDamage(dmgSource, damage);
        }
        private void DestroyHeart()
        {
            this.Die();

            Block hostBlock = World.BlockAccessor.GetBlock(ServerPos.AsBlockPos);

            if(hostBlock is CrystalObeliskBlock)
            {
                World.BlockAccessor.BreakBlock(ServerPos.AsBlockPos, null, 0);
            }
        }
        private void AttemptGiveHeart(EntityAgent byEntity, ItemSlot itemslot)
        {
            if(itemslot.Empty)
            {
                ItemStack heart = new ItemStack(World.GetItem(new AssetLocation("orecrystals", "crystal_heart-" + heartVariant)));
                itemslot.Itemstack = heart;

                itemslot.MarkDirty();

                BlockEntityCrystalObeliskSpawner heartSpawner = Api.World.BlockAccessor.GetBlockEntity(this.ServerPos.AsBlockPos - new BlockPos(1, 1, 1)) as BlockEntityCrystalObeliskSpawner;
                
                if(heartSpawner != null)
                    heartSpawner.SetHeartTaken();

                this.Die(EnumDespawnReason.PickedUp);
            }
        }
        private BlockPos GetNearbyCrystal(bool findBountiful)
        {
            List<Vec3i> possibleCrystals = new List<Vec3i>();
            Vec3i heartPos = new Vec3i((int)this.ServerPos.X, (int)this.ServerPos.Y, (int)this.ServerPos.Z);
            Block checkBlock;

            for (int x = heartPos.X - CRYSTAL_GROWTH_RANGE; x <= heartPos.X + CRYSTAL_GROWTH_RANGE; x++)
            {
                for (int y = heartPos.Y - CRYSTAL_GROWTH_RANGE; y <= heartPos.Y + CRYSTAL_GROWTH_RANGE; y++)
                {
                    for (int z = heartPos.Z - CRYSTAL_GROWTH_RANGE; z <= heartPos.Z + CRYSTAL_GROWTH_RANGE; z++)
                    {
                        checkBlock = blockAccessor.GetBlock(x, y, z);

                        if (checkBlock is OreCrystalsCrystal)
                        {
                            if(findBountiful == false)
                            {
                                if(checkBlock.FirstCodePart() != "orecrystals_crystal_bountiful")
                                {
                                    possibleCrystals.Add(new Vec3i(x, y, z));
                                }
                            }
                            else
                            {
                                possibleCrystals.Add(new Vec3i(x, y, z));
                            }
                        }
                    }
                }
            }
            if (possibleCrystals.Count == 0)
                return null;
            else
                return possibleCrystals[growthRand.Next(0, possibleCrystals.Count)].AsBlockPos;
        }
        private void GrowCrystal(BlockPos crystalPos)
        {
            Block crystal = blockAccessor.GetBlock(crystalPos);
            int newBlockId;

            switch(crystal.FirstCodePart())
            {
                case "orecrystals_crystal_poor":
                    newBlockId = sApi.WorldManager.GetBlockId(new AssetLocation("orecrystals", "orecrystals_crystal_medium-" + crystal.FirstCodePart(1) + "-" + crystal.LastCodePart()));

                    blockAccessor.SetBlock(newBlockId, crystalPos);
                    break;
                case "orecrystals_crystal_medium":
                    newBlockId = sApi.WorldManager.GetBlockId(new AssetLocation("orecrystals", "orecrystals_crystal_rich-" + crystal.FirstCodePart(1) + "-" + crystal.LastCodePart()));

                    blockAccessor.SetBlock(newBlockId, crystalPos);
                    break;
                case "orecrystals_crystal_rich":
                    newBlockId = sApi.WorldManager.GetBlockId(new AssetLocation("orecrystals", "orecrystals_crystal_bountiful-" + crystal.FirstCodePart(1) + "-" + crystal.LastCodePart()));
                    
                    blockAccessor.SetBlock(newBlockId, crystalPos);
                    break;
            }

            blockAccessor.Commit();

            sApi.World.PlaySoundAt(new AssetLocation("orecrystals", "sounds/effect/crystal_ping"), crystalPos.X, crystalPos.Y, crystalPos.Z, null, true, 32, .25f);

            SetGrowParticlePosDirCol(crystalPos.ToVec3d(), crystal.LastCodePart(), crystal.FirstCodePart(1));
            World.SpawnParticles(crystalParticlesGrow);
        }
        private void BreakCrystal(BlockPos crystalPos)
        {
            Block crystal = blockAccessor.GetBlock(crystalPos);

            this.World.BlockAccessor.BreakBlock(crystalPos, null, 0);

            this.World.BlockAccessor.Commit();
        }
        private void InitGrowParticles()
        {
            crystalParticlesGrow = new SimpleParticleProperties()
            {
                GravityEffect = 0.1f,

                MinSize = 0.1f,
                MaxSize = 0.4f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.1f),

                MinQuantity = 5,
                AddQuantity = 15,

                LifeLength = 0.2f,
                addLifeLength = 0.4f,

                ShouldDieInLiquid = true,

                WithTerrainCollision = true,

                Color = CrystalColour.GetColour(heartVariant),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                VertexFlags = 150,

                ParticleModel = EnumParticleModel.Cube
            };
        }
        private void InitAngerParticles()
        {
            Vec3f velocityRand = new Vec3f((float)(growthRand.Next(0, 1) + growthRand.NextDouble()), (float)(growthRand.Next(0, 1) + growthRand.NextDouble()), (float)(growthRand.Next(0, 1) + growthRand.NextDouble()));

            heartAngerParticles = new SimpleParticleProperties()
            {
                MinPos = new Vec3d(this.ServerPos.X, this.ServerPos.Y + .5, this.ServerPos.Z),

                MinVelocity = new Vec3f(-velocityRand.X, -velocityRand.Y, -velocityRand.Z),
                AddVelocity = velocityRand * 2,

                GravityEffect = 0.0f,

                MinSize = 0.1f,
                MaxSize = 0.4f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.1f),

                MinQuantity = 5,
                AddQuantity = 15,

                LifeLength = 0.6f,
                addLifeLength = 1.4f,

                ShouldDieInLiquid = true,

                WithTerrainCollision = true,

                Color = CrystalColour.GetColour(heartVariant),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                VertexFlags = 150,

                ParticleModel = EnumParticleModel.Quad
            };
        }
        private void InitHeartAreaParticles()
        {
            heartAreaParticles = new SimpleParticleProperties()
            {
                MinPos = new Vec3d(this.ServerPos.X - CRYSTAL_GROWTH_RANGE, this.ServerPos.Y - CRYSTAL_GROWTH_RANGE, this.ServerPos.Z - CRYSTAL_GROWTH_RANGE),
                AddPos = new Vec3d(CRYSTAL_GROWTH_RANGE * 2, CRYSTAL_GROWTH_RANGE * 2, CRYSTAL_GROWTH_RANGE * 2),

                GravityEffect = -0.1f,

                MinSize = 0.1f,
                MaxSize = 0.4f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.1f),

                MinQuantity = 1,

                LifeLength = 0.6f,
                addLifeLength = 1.4f,

                ShouldDieInLiquid = true,

                WithTerrainCollision = true,

                Color = CrystalColour.GetColour(heartVariant),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                VertexFlags = 150,

                ParticleModel = EnumParticleModel.Quad
            };
        }
        private void InitObeliskAreaParticles()
        {
            obeliskAreaParticles = new SimpleParticleProperties()
            {
                MinPos = new Vec3d(this.ServerPos.X - 1, this.ServerPos.Y - 1, this.ServerPos.Z - 1),
                AddPos = new Vec3d(2, 2, 2),

                GravityEffect = -0.1f,

                MinSize = 0.1f,
                MaxSize = 0.4f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.1f),

                MinQuantity = 1,

                LifeLength = 0.6f,
                addLifeLength = 1.4f,

                ShouldDieInLiquid = true,

                WithTerrainCollision = true,

                Color = CrystalColour.GetColour(heartVariant),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                VertexFlags = 150,

                ParticleModel = EnumParticleModel.Quad
            };
        }
        private void InitBreakParticles()
        {
            crystalBreakParticles = new SimpleParticleProperties()
            {
                GravityEffect = 0.1f,

                MinSize = 0.4f,
                MaxSize = 0.8f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 0.5f),

                MinQuantity = 15,
                AddQuantity = 30,

                LifeLength = 1.2f,
                addLifeLength = 1.4f,

                ShouldDieInLiquid = true,

                WithTerrainCollision = true,

                Color = CrystalColour.GetColour(heartVariant),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                VertexFlags = 150,

                ParticleModel = EnumParticleModel.Cube
            };
        }
        private void SetGrowParticlePosDirCol(Vec3d crystalPos, string crystalDirection, string crystalColour)
        {
            crystalParticlesGrow.MinPos = crystalPos;
            crystalParticlesGrow.AddPos = new Vec3d(1, 1, 1);

            crystalParticlesGrow.MinVelocity = GetParticleVelocity(crystalDirection, 0.5f, 0.0f);
            crystalParticlesGrow.AddVelocity = GetParticleVelocity(crystalDirection, 0.5f, 0.0f);

            crystalParticlesGrow.Color = CrystalColour.GetColour(crystalColour);
        }
        private void SetBreakParticlePosDirCol(Vec3d crystalPos, string crystalDirection, string crystalColour)
        {
            crystalBreakParticles.MinPos = crystalPos;
            crystalBreakParticles.AddPos = new Vec3d(1, 1, 1);

            crystalBreakParticles.MinVelocity = GetParticleVelocity(crystalDirection, 0.5f, -0.5f);
            crystalBreakParticles.AddVelocity = crystalBreakParticles.MinVelocity * 2;

            crystalBreakParticles.Color = CrystalColour.GetColour(crystalColour);
        }
        private Vec3f GetParticleVelocity(string direction, float parallelSpeed, float perpendicularSpeed)
        {
            switch(direction)
            {
                case "ore_down":
                    return new Vec3f(perpendicularSpeed, parallelSpeed, perpendicularSpeed);
                case "ore_up":
                    return new Vec3f(perpendicularSpeed, -parallelSpeed, perpendicularSpeed);
                case "ore_west":
                    return new Vec3f(parallelSpeed, perpendicularSpeed, perpendicularSpeed);
                case "ore_east":
                    return new Vec3f(-parallelSpeed, perpendicularSpeed, perpendicularSpeed);
                case "ore_north":
                    return new Vec3f(perpendicularSpeed, perpendicularSpeed, parallelSpeed);
                case "ore_south":
                    return new Vec3f(perpendicularSpeed, perpendicularSpeed, -parallelSpeed);
                default:
                    return new Vec3f(0, 0, 0);
            }
        }
    }
}
