using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace OreCrystals
{
    class BlockEntityCrystalObeliskSpawner: BlockEntityCrystalObelisk
    {
        private Entity crystalHeart = null;

        private bool heartTaken;
        private bool heartSpawned;

        private readonly int minHeartSpawnRange = 25;

        public BlockEntityCrystalObeliskSpawner() : base()
        {

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if(!heartTaken && !heartSpawned)
            {
                if (api is ICoreServerAPI)
                {
                    RegisterGameTickListener(CheckShouldSpawnHeart, 8000);
                }
                else
                {

                }
            }
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            heartTaken = tree.GetBool("heartTaken");
            heartSpawned = tree.GetBool("heartSpawned");
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetBool("heartTaken", heartTaken);
            tree.SetBool("heartSpawned", heartSpawned);
        }

        public void SetHeartTaken()
        {
            this.heartTaken = true;
        }
        //-- Serverside Only --//
        private void SpawnCrystalHeart()
        {
            EntityProperties entityType = sApi.World.GetEntityType(new AssetLocation("orecrystals", "crystal_heart-" + variant));
            crystalHeart = sApi.World.ClassRegistry.CreateEntity(entityType);

            crystalHeart.ServerPos.SetPos(new EntityPos(this.Pos.X + 1, this.Pos.Y + 1, this.Pos.Z + 1));

            crystalHeart.Pos.SetFrom(crystalHeart.ServerPos);

            sApi.World.SpawnEntity(crystalHeart);

            this.heartSpawned = true;
        }

        //-- Serverside Only --//
        private void CheckShouldSpawnHeart(float dt)
        {
            bool playerInRange = false;

            foreach (IPlayer player in sApi.World.AllOnlinePlayers)
            {
                if (player.Entity.ServerPos.DistanceTo(this.Pos.ToVec3d()) < minHeartSpawnRange)
                {
                    playerInRange = true;
                }
            }

            if (playerInRange)
            {
                if(crystalHeart == null && heartSpawned == false && heartTaken == false)
                {
                    SpawnCrystalHeart();
                }
            }
            else if(heartSpawned == true)
            {
                if(!playerInRange)
                {
                    crystalHeart.Die(EnumDespawnReason.OutOfRange);
                    crystalHeart = null;

                    heartSpawned = false;
                }
            }
        }
    }
}
