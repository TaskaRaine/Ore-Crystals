using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace OreCrystals
{
    class OreCrystalsRegisterCommands : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            api.RegisterCommand("cleanworld", "Reverts ? blocks that have an ID but do not have an associated block code to air. Only works on chunks that have been loaded. Requires world save and restart.", "",
            (IServerPlayer player, int groupId, CmdArgs args) =>
            {
                IBlockAccessor worldBlockAccessor = api.World.BlockAccessor;
                IServerChunk currentChunk = api.WorldManager.GetChunk(player.Entity.Pos.AsBlockPos);
                int chunkSize = worldBlockAccessor.ChunkSize;
                Dictionary<long, IServerChunk> loadedChunks = api.WorldManager.AllLoadedChunks;

                for(int i = 0; i < loadedChunks.Count; i++)
                {
                    currentChunk = loadedChunks.ElementAt(i).Value;

                    for (int x = 0; x < chunkSize; x++)
                    {
                        for (int y = 0; y < chunkSize; y++)
                        {
                            for (int z = 0; z < chunkSize; z++)
                            {
                                if (currentChunk.Blocks != null)
                                {
                                    if (api.World.GetBlock(currentChunk.Blocks[(y * chunkSize + z) * chunkSize + x]).Code == null)
                                    {
                                        currentChunk.Blocks[(y * chunkSize + z) * chunkSize + x] = 0;
                                    }
                                }
                            }
                        }
                    }
                    currentChunk.MarkModified();
                }
            }, Privilege.controlserver);

            api.RegisterCommand("stone", "Spawns a test stone", "/stone basalt, etc.",
            (IServerPlayer player, int groupId, CmdArgs args) =>
            {
                try
                {
                    EntityProperties entityType = api.World.GetEntityType(new AssetLocation("game", "thrownstone-" + args[0]));
                    Entity entity = api.World.ClassRegistry.CreateEntity(entityType);
                    EntityPos entityPos = new EntityPos(player.Entity.ServerPos.X, player.Entity.ServerPos.Y, player.Entity.ServerPos.Z);

                    entity.ServerPos.SetPos(new Vec3d(entityPos.X - 1, entityPos.Y - 1, entityPos.Z - 1));
                    entity.Pos.SetFrom(entity.ServerPos);
                    api.World.SpawnEntity(entity);
                }
                catch
                {
                    api.Server.LogWarning("A stone could not be spawned. " + args[0] + " is not a spawnable variant.");
                }
            }, Privilege.controlserver);
            api.RegisterCommand("locust", "Spawns a test locust", "/locust bismutinite, etc.",
            (IServerPlayer player, int groupId, CmdArgs args) =>
            {
                try
                {
                    EntityProperties entityType = api.World.GetEntityType(new AssetLocation("orecrystals", "crystal_locust-" + args[0]));
                    Entity entity = api.World.ClassRegistry.CreateEntity(entityType);
                    EntityPos entityPos = new EntityPos(player.Entity.ServerPos.X, player.Entity.ServerPos.Y, player.Entity.ServerPos.Z);

                    entity.ServerPos.SetPos(entityPos);
                    entity.Pos.SetFrom(entity.ServerPos);

                    api.World.SpawnEntity(entity);
                }
                catch
                {
                    api.Server.LogWarning("A crystal locust could not be spawned. " + args[0] + " is not a spawnable variant.");
                }
            }, Privilege.controlserver);
        }
    }
}