using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
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
        }
    }
}
