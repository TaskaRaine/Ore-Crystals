using Vintagestory.API.Common;

namespace OreCrystals
{
    class RegisterEntities : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntity("EntityCrystalLocust", typeof(EntityCrystalLocust));
            api.RegisterEntity("EntityCrystalHeart", typeof(EntityCrystalHeart));
            api.RegisterEntity("EntityCrystalGrenade", typeof(EntityCrystalGrenade));
        }
    }
}
