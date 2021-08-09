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
        public override void OnEntitySpawn()
        {
            base.OnEntitySpawn();

            //-- Prevent locusts from attacking in passive/never hostile mode by removing their aggressive AI Tasks --//
            PreventHostilities();
        }
        public override void OnEntityLoaded()
        {
            base.OnEntityLoaded();

            //-- Prevent locusts from attacking in passive/never hostile mode by removing their aggressive AI Tasks --//
            PreventHostilities();

            if(Attributes.GetBool("exploded") == true)
            {
                AnimManager.StartAnimation(new AnimationMetaData() { Animation = bombAnimation, Code = bombAnimation }.Init());
            }
        }
        //-- Prevent locusts from attacking in passive/never hostile mode by removing their aggressive AI Tasks --//
        private void PreventHostilities()
        {
            if (Api.Side == EnumAppSide.Server)
            {
                if (Api.World.Config.GetString("creatureHostility") == "off" || Api.World.Config.GetString("creatureHostility") == "passive")
                {
                    EntityBehaviorTaskAI taskAi = null;

                    foreach (EntityBehavior behavior in this.Properties.Server.Behaviors)
                    {
                        if (behavior is EntityBehaviorTaskAI)
                            taskAi = behavior as EntityBehaviorTaskAI;
                    }

                    if (taskAi != null)
                    {
                        AiTaskMeleeAttack meleeAttack = taskAi.taskManager.GetTask<AiTaskMeleeAttack>();
                        AiTaskCrystalLocustBomb bombAttack = taskAi.taskManager.GetTask<AiTaskCrystalLocustBomb>();
                        AiTaskSeekEntity seekEntity = taskAi.taskManager.GetTask<AiTaskSeekEntity>();

                        if (meleeAttack != null)
                            taskAi.taskManager.RemoveTask(meleeAttack);

                        if (bombAttack != null)
                            taskAi.taskManager.RemoveTask(bombAttack);

                        if (seekEntity != null)
                            taskAi.taskManager.RemoveTask(seekEntity);
                    }
                }
            }
        }
    }
}
