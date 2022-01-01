using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace OreCrystals
{
    class BlockEntityCrystalObeliskSpawner: BlockEntityCrystalObelisk
    {
        private const int HEART_SPAWN_INTERVAL = 8000;
        private const int MIN_HEART_SPAWN_RANGE = 50;
        private const int CRYSTAL_LOCUST_COUNT = 3;

        private Entity crystalHeart = null;
        private List<Entity> crystalLocusts = null;

        private bool heartTaken;
        private bool heartSpawned;

        public BlockEntityCrystalObeliskSpawner() : base()
        {

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if(api is ICoreServerAPI)
            {
                crystalLocusts = new List<Entity>(CRYSTAL_LOCUST_COUNT);

                if (!heartTaken)
                {
                    if (heartSpawned)
                    {
                        //-- If a heart has been spawned then associate the nearest heart to this spawner block. This is to ensure that an obelisk has a reference to its heart on load if it already exists in world --//
                        sApi.World.GetEntitiesInsideCuboid(new BlockPos(Pos.X - 5, Pos.Y - 5, Pos.Z - 5), new BlockPos(Pos.X + 5, Pos.Y + 5, Pos.Z + 5), (entity) =>
                        {
                            if(entity is EntityCrystalHeart)
                            {
                                if (this.crystalHeart == null || entity.Pos.DistanceTo(new Vec3d(Pos.X, Pos.Y, Pos.Z)) < this.crystalHeart.Pos.DistanceTo(new Vec3d(Pos.X, Pos.Y, Pos.Z)))
                                {
                                    this.crystalHeart = entity;

                                    return true;
                                }
                                else
                                    return false;
                            }
                            else if(entity is EntityCrystalLocust)
                            {
                                crystalLocusts.Add(entity);

                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        });
                    }

                    RegisterGameTickListener(CheckShouldSpawnHeart, HEART_SPAWN_INTERVAL);
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
        private void SpawnCrystalLocusts()
        {
            EntityProperties entityType = sApi.World.GetEntityType(new AssetLocation("orecrystals", "crystal_locust-" + variant));

            if (crystalLocusts.Count < CRYSTAL_LOCUST_COUNT)
            {
                for(int i = crystalLocusts.Count; i < CRYSTAL_LOCUST_COUNT; i ++)
                {
                    Entity locust = sApi.World.ClassRegistry.CreateEntity(entityType);
                    locust.ServerPos.SetPos(FindOpenSpace(this.Pos.ToVec3i(), i));
                    locust.Pos.SetFrom(locust.ServerPos);

                    sApi.World.SpawnEntity(locust);

                    crystalLocusts.Add(locust);
                }
            }
        }
        //-- Serverside Only --//
        private void SpawnCrystalHeart()
        {
            EntityProperties entityType = sApi.World.GetEntityType(new AssetLocation("orecrystals", "crystal_heart-" + variant));

            if(entityType != null)
            {
                crystalHeart = sApi.World.ClassRegistry.CreateEntity(entityType);

                crystalHeart.ServerPos.SetPos(new EntityPos(this.Pos.X + 1, this.Pos.Y + 1, this.Pos.Z + 1));

                crystalHeart.Pos.SetFrom(crystalHeart.ServerPos);

                sApi.World.SpawnEntity(crystalHeart);

                this.heartSpawned = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Heart not spawned. Variant: " + variant);
            }
        }

        //-- Serverside Only --//
        private void CheckShouldSpawnHeart(float dt)
        {
            bool playerInRange = false;

            //-- Checks to see if a player is within range of this spawner --//
            foreach (IPlayer player in sApi.World.AllOnlinePlayers)
            {
                if (player.Entity.ServerPos.DistanceTo(this.Pos.ToVec3d()) < MIN_HEART_SPAWN_RANGE)
                {
                    playerInRange = true;
                }
            }

            if (playerInRange)
            {
                if(crystalHeart == null && heartSpawned == false && heartTaken == false)
                {
                    SpawnCrystalHeart();
                    SpawnCrystalLocusts();
                }
            }
            else if(heartSpawned == true)
            {
                //-- If no player is in range, then unload the heart and locusts and set heartSpawned to false. --//
                //-- Another heart can spawn if a player again comes within range --//

                if(!playerInRange)
                {
                    if(crystalHeart != null)
                    {    
                        crystalHeart.Die(EnumDespawnReason.OutOfRange);

                        crystalHeart = null;

                        heartSpawned = false;

                        foreach(Entity locust in crystalLocusts)
                        {
                            locust.Die(EnumDespawnReason.OutOfRange);
                        }

                        crystalLocusts.Clear();
                    }
                }
            }
        }
        //-- Finds an open space within a 3x3 area of the spawner to spawn the crystal locust when a heart is spawned --//
        private EntityPos FindOpenSpace(Vec3i spawnerPos, int locustIndex)
        {
            IBlockAccessor blockAccessor = sApi.World.GetBlockAccessor(false, false, false);

            List<Vec3i> possiblePositions = new List<Vec3i>();
            Random randomSpace = new Random((int)this.Api.World.ElapsedMilliseconds + locustIndex);

            Block spaceBlock;

            for(int x = spawnerPos.X - 3; x <= spawnerPos.X + 3; x++)
            {
                for(int y = spawnerPos.Y - 3; y <= spawnerPos.Y + 3; y++)
                {
                    for(int z = spawnerPos.Z - 3; z <= spawnerPos.Z + 3; z++)
                    {
                        spaceBlock = blockAccessor.GetBlock(x, y, z);

                        if (spaceBlock.BlockMaterial == EnumBlockMaterial.Air)
                            possiblePositions.Add(new Vec3i(x, y, z));
                    }
                }
            }

            if (possiblePositions.Count == 0)
                return new EntityPos(spawnerPos.X + 0.5, spawnerPos.Y + 0.5, spawnerPos.Z + 0.5);

            int randomPositionIndex = randomSpace.Next(0, possiblePositions.Count);

            //-- Adding 0.5 to the position forces the entity to spawn within the center of the block position, not on the bottom corner, preventing it from getting stuck in a wall --//
            return new EntityPos(possiblePositions[randomPositionIndex].X + 0.5, possiblePositions[randomPositionIndex].Y + 0.5, possiblePositions[randomPositionIndex].Z + 0.5);
        }
    }
}
