using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace OreCrystals
{
    class EntityCrystalGrenade: Entity
    {
        private const double grenadeVerticalSpeed = 5;
        private const double grenadeHorizontalSpeed = 0.125;
        private const float grenadeParticleVelocityModifier = 6;
        private const int grenadeDamage = 10;
        private const int grenadeRange = 3;

        protected CollisionTester collTester = new CollisionTester();
        protected EntityPos grenadeTransforms = new EntityPos();
        protected SimpleParticleProperties grenadeAngerParticles;
        protected SimpleParticleProperties grenadeBreakParticles;

        private bool isTriggered = false;
        private long triggeredTime;
        private int fuseTime = 5000;

        private Random particleRand;
        private string grenadeVariant;

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            grenadeTransforms = ServerPos;
            Pos.SetFrom(ServerPos);

            particleRand = new Random((int)this.EntityId);
            grenadeVariant = this.LastCodePart();

            this.LightHsv = CrystalColour.GetLight(grenadeVariant);
        }
        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);

            if(isTriggered == false)
            {
                if(!collTester.IsColliding(World.BlockAccessor, this.CollisionBox, this.ServerPos.XYZ))
                {
                    this.ServerPos.SetFrom(CalculatePosition(dt));
                }
                else
                {
                    isTriggered = true;
                    triggeredTime = World.ElapsedMilliseconds;

                    this.ServerPos.SetPos(this.ServerPos.XYZ - (this.ServerPos.Motion.Normalize() * 0.25));
                }
            }
            else
            {
                if (this.Api.Side == EnumAppSide.Client)
                {
                    InitAngerParticles();
                    this.Api.World.SpawnParticles(grenadeAngerParticles);
                }

                if(World.ElapsedMilliseconds >= triggeredTime + fuseTime)
                {
                    if (this.Api.Side == EnumAppSide.Client)
                    {
                        InitBreakParticles();
                        this.Api.World.SpawnParticles(grenadeBreakParticles);
                    }
                    else
                    {
                        Api.World.PlaySoundAt(new AssetLocation("orecrystals", "sounds/block/glass"), ServerPos.X, ServerPos.Y, ServerPos.Z, null, true, 32, 1.0f);

                        DamageEntities();
                    }

                    this.Die();
                }
            }
        }
        private void InitAngerParticles()
        {
            Vec3f velocityRand = new Vec3f((float)(particleRand.Next(0, 1) + particleRand.NextDouble()), (float)(particleRand.Next(0, 1) + particleRand.NextDouble()), (float)(particleRand.Next(0, 1) + particleRand.NextDouble()));

            grenadeAngerParticles = new SimpleParticleProperties()
            {
                MinPos = new Vec3d(this.Pos.X, this.Pos.Y + .25, this.Pos.Z),

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

                Color = CrystalColour.GetColour(grenadeVariant),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                VertexFlags = 150,

                ParticleModel = EnumParticleModel.Quad
            };
        }
        private void InitBreakParticles()
        {
            Vec3f velocityRand = new Vec3f((float)particleRand.NextDouble(), (float)particleRand.NextDouble(), (float)particleRand.NextDouble()) * grenadeParticleVelocityModifier;

            grenadeBreakParticles = new SimpleParticleProperties()
            {
                MinPos = new Vec3d(this.Pos.X, this.Pos.Y + .25, this.Pos.Z),

                MinVelocity = new Vec3f(velocityRand.X, velocityRand.Y, velocityRand.Z),
                AddVelocity = new Vec3f(-velocityRand.X, -velocityRand.Y, -velocityRand.Z) * 2,

                GravityEffect = 0.2f,

                MinSize = 0.1f,
                MaxSize = 0.4f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.1f),

                MinQuantity = 40,
                AddQuantity = 80,

                LifeLength = 0.6f,
                addLifeLength = 2.0f,

                ShouldDieInLiquid = true,

                WithTerrainCollision = true,

                Color = CrystalColour.GetColour(grenadeVariant),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                VertexFlags = 150,

                ParticleModel = EnumParticleModel.Cube
            };
        }
        private EntityPos CalculatePosition(float dt)
        {
            this.grenadeTransforms.SetAngles(0, (World.ElapsedMilliseconds / 200.0f) % GameMath.TWOPI, (World.ElapsedMilliseconds / 150.0f) % GameMath.TWOPI);
            
            this.grenadeTransforms.SetPos(this.grenadeTransforms.XYZ + this.grenadeTransforms.Motion.Normalize());

            if (this.grenadeTransforms.Motion.Y < 0)
                this.grenadeTransforms.Motion = this.grenadeTransforms.Motion.AddCopy(-this.grenadeTransforms.Motion.X * grenadeHorizontalSpeed * dt, this.grenadeTransforms.Motion.Y * grenadeVerticalSpeed * dt, -this.grenadeTransforms.Motion.Z * grenadeHorizontalSpeed * dt);
            else if (this.grenadeTransforms.Motion.Y <= grenadeVerticalSpeed / 2)
                this.grenadeTransforms.Motion = this.grenadeTransforms.Motion.AddCopy(-this.grenadeTransforms.Motion.X * grenadeHorizontalSpeed * dt, -1 * grenadeVerticalSpeed * dt, -this.grenadeTransforms.Motion.Z * grenadeHorizontalSpeed * dt);
            else
                this.grenadeTransforms.Motion = this.grenadeTransforms.Motion.AddCopy(-this.grenadeTransforms.Motion.X * grenadeHorizontalSpeed * dt, -this.grenadeTransforms.Motion.Y * grenadeVerticalSpeed * dt, -this.grenadeTransforms.Motion.Z * grenadeHorizontalSpeed * dt);
            return this.grenadeTransforms;
        }
        private void DamageEntities()
        {
            World.GetEntitiesInsideCuboid(new BlockPos((int)Math.Ceiling(ServerPos.X - grenadeRange), (int)Math.Ceiling(ServerPos.Y - grenadeRange), (int)Math.Ceiling(ServerPos.Z - grenadeRange)), 
                new BlockPos((int)Math.Ceiling(ServerPos.X + grenadeRange), (int)Math.Ceiling(ServerPos.Y + grenadeRange), (int)Math.Ceiling(ServerPos.Z + grenadeRange)), 
                (entity) =>
                {
                    entity.ReceiveDamage(new DamageSource() { SourceEntity = this }, grenadeDamage);

                    return true;
                });
        }
    }
}
