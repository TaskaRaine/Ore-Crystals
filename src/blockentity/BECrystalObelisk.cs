using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace OreCrystals
{
    class BlockEntityCrystalObelisk: BlockEntity
    {
        protected ICoreServerAPI sApi;
        protected ICoreClientAPI cApi;

        protected string variant;

        public BlockEntityCrystalObelisk() : base()
        {

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            this.variant = this.Block.LastCodePart();

            if (api is ICoreServerAPI)
            {
                sApi = api as ICoreServerAPI;
            }
            else
            {
                cApi = api as ICoreClientAPI;
            }
        }
    }
}
