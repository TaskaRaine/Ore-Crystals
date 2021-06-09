using Vintagestory.API.Common;

namespace OreCrystals
{
    class RegisterBehaviours : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterEntityBehaviorClass("illuminate", typeof(BehaviourIlluminate));
        }
    }
}
