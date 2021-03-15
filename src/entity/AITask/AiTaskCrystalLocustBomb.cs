using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace OreCrystals
{
    class AiTaskCrystalLocustBomb : AiTaskBase
    {
        enum TaskState
        {
            MOVING = 0,
            BOMBING = 1,
            STUCK = 2
        }

        EntityBehaviorHealth behaviorEntityHealth;

        Entity nearestPlayer;
        double nearestDistance;

        string crouchAnimation;
        string bombAnimation;

        int damage;
        int damageRange;
        int bombDelay;
        float particleVelocity;

        public SimpleParticleProperties bombParticles;

        private TaskState taskState;

        public AiTaskCrystalLocustBomb(EntityAgent entity) : base(entity)
        {
            behaviorEntityHealth = entity.GetBehavior<EntityBehaviorHealth>();
        }

        private void InitBombParticles()
        {
            Vec3f velocityVector = new Vec3f(particleVelocity, particleVelocity, particleVelocity);

            this.bombParticles = new SimpleParticleProperties
            {
                MinPos = new Vec3d(entity.ServerPos.X, entity.ServerPos.Y + .5, entity.ServerPos.Z),

                GravityEffect = 0.3f,

                MinVelocity = new Vec3f(-velocityVector.X, -velocityVector.Y, -velocityVector.Z),
                AddVelocity = velocityVector * 2,

                MinSize = 0.5f,
                MaxSize = 0.8f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.1f),

                MinQuantity = 75,
                AddQuantity = 125,

                LifeLength = 1.0f,
                addLifeLength = 2.0f,

                ShouldDieInLiquid = false,

                WithTerrainCollision = true,

                Color = CrystalColour.GetColour(this.entity.LastCodePart()),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                VertexFlags = 50,

                ParticleModel = EnumParticleModel.Cube
            };
        }

        public override void LoadConfig(JsonObject taskConfig, JsonObject aiConfig)
        {
            base.LoadConfig(taskConfig, aiConfig);

            if(taskConfig["crouchAnimation"].Exists)
            {
                crouchAnimation = taskConfig["crouchAnimation"].AsString();
            }

            if(taskConfig["bombAnimation"].Exists)
            {
                bombAnimation = taskConfig["bombAnimation"].AsString();
            }

            if(taskConfig["bombDelay"].Exists)
            {
                bombDelay = taskConfig["bombDelay"].AsInt(600);
            }

            if(taskConfig["particleVelocity"].Exists)
            {
                particleVelocity = taskConfig["particleVelocity"].AsFloat(1.0f);
            }

            if(taskConfig["damage"].Exists)
            {
                damage = taskConfig["damage"].AsInt(5);
            }

            if(taskConfig["damageRange"].Exists)
            {
                damageRange = taskConfig["damageRange"].AsInt(1);
            }
        }
        public override bool ShouldExecute()
        {
            if (behaviorEntityHealth.Health >= this.behaviorEntityHealth.MaxHealth * 0.80)
                return false;
            else
                return true;
        }
        public override void StartExecute()
        {
            base.StartExecute();

            this.nearestPlayer = this.entity.World.AllOnlinePlayers[0].Entity;
            this.nearestDistance = nearestPlayer.ServerPos.DistanceTo(this.entity.ServerPos.XYZ);

            foreach (IPlayer player in this.entity.World.AllOnlinePlayers)
            {
                if(player.Entity.ServerPos.DistanceTo(this.entity.ServerPos.XYZ) < this.nearestDistance)
                {
                    this.nearestPlayer = player.Entity;
                    this.nearestDistance = nearestPlayer.ServerPos.DistanceTo(this.entity.ServerPos.XYZ);
                }
            }

            pathTraverser.WalkTowards(nearestPlayer.ServerPos.XYZ, 0.030f, 0.25f, OnGoalReached, OnStuck);
            taskState = TaskState.MOVING;
        }
        public override bool ContinueExecute(float dt)
        {
            if(taskState == TaskState.MOVING)
            {
                pathTraverser.CurrentTarget.X = nearestPlayer.ServerPos.X;
                pathTraverser.CurrentTarget.Y = nearestPlayer.ServerPos.Y;
                pathTraverser.CurrentTarget.Z = nearestPlayer.ServerPos.Z;
            }

            return base.ContinueExecute(dt);
        }
        private void OnGoalReached()
        {
            entity.AnimManager.StopAnimation("run");
            entity.AnimManager.StartAnimation(new AnimationMetaData() { Animation = crouchAnimation, Code = crouchAnimation }.Init());

            entity.Api.Event.RegisterCallback(ExplodeCrystalLocust, this.bombDelay);

            taskState = TaskState.BOMBING;
        }
        private void OnStuck()
        {
            taskState = TaskState.STUCK;
        }
        private void ExplodeCrystalLocust(float deltaTime)
        {
            InitBombParticles();
            Entity[] nearbyEntities = this.world.GetEntitiesAround(this.entity.ServerPos.XYZ, damageRange, damageRange);

            foreach (RunningAnimation animation in this.entity.AnimManager.Animator.RunningAnimations)
            {
                this.entity.AnimManager.StopAnimation(animation.Animation.Code);
            }

            entity.AnimManager.StartAnimation(new AnimationMetaData() { Animation = bombAnimation, Code = bombAnimation }.Init());

            entity.World.PlaySoundAt(new AssetLocation("game", "sounds/block/glass"), this.entity, null);
            entity.World.PlaySoundAt(new AssetLocation("game", "sounds/block/glass"), this.entity, null);
            entity.World.PlaySoundAt(new AssetLocation("game", "sounds/block/glass"), this.entity, null);

            entity.World.SpawnParticles(bombParticles);

            this.entity.Attributes.SetBool("exploded", true);

            this.behaviorEntityHealth.Health = 0;
            this.entity.Alive = false;

            foreach (Entity nearbyEntity in nearbyEntities)
            {
                if (nearbyEntity != entity)
                {
                    nearbyEntity.ReceiveDamage(new DamageSource
                    {
                        Source = EnumDamageSource.Explosion,
                        Type = EnumDamageType.PiercingAttack
                    }, damage);
                }
            }
        }
    }
}
