using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace OreCrystals
{
    enum CurrentMotion 
    {
        UP,
        DOWN
    }

    class EntityCrystalHeart : Entity
    {
        public EnumEntityActivity CurrentControls;

        private string heartVariant;

        private EntityPos originalPosition;
        private EntityPos nextPosition;

        private CurrentMotion motion = CurrentMotion.UP;

        private double minHeartOffset = 0.5;
        private double maxHeartOffset = 0.5;

        private double offsetAmount = 0.1;

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            heartVariant = this.LastCodePart();
        }
        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);

            if(Alive)
            {
                if (Api.Side == EnumAppSide.Server)
                {
                    
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
        private void HandleAnimation()
        {
            if (World.Side == EnumAppSide.Client)
            {
                if (Alive)
                    CurrentControls = EnumEntityActivity.Idle;
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
    }
}
