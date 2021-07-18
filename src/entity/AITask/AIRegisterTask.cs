using OreCrystals;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

class AIRegisterTask: ModSystem
{
    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);

        AiTaskRegistry.Register<AiTaskCrystalLocustBomb>("crystalbomb");
    }
}
