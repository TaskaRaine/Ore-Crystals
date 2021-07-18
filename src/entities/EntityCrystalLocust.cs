using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace OreCrystals
{
    class EntityCrystalLocust : EntityLocust
    {
        private string bombAnimation = "crystalbomb";

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);
        }
        public override void OnEntityLoaded()
        {
            base.OnEntityLoaded();

            if(Attributes.GetBool("exploded") == true)
            {
                AnimManager.StartAnimation(new AnimationMetaData() { Animation = bombAnimation, Code = bombAnimation }.Init());
            }
        }
    }
}
