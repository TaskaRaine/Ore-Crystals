using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace OreCrystals
{
    class ItemCrystalHeart: Item
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null) return;

            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer)
                byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
            else
                return;

            SpawnHeart(slot, byPlayer, blockSel);

            handling = EnumHandHandling.PreventDefault;
        }
        private void SpawnHeart(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
        {
            EntityPos entityPos = new EntityPos(blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z);
            entityPos = OffsetHeartPosition(entityPos, blockSel.Face);

            string variant = this.LastCodePart();

            EntityProperties entityType = api.World.GetEntityType(new AssetLocation("orecrystals", "crystal_heart-" + variant));
            Entity entity = api.World.ClassRegistry.CreateEntity(entityType);

            entity.ServerPos.SetPos(entityPos);

            entity.Pos.SetFrom(entity.ServerPos);

            api.World.SpawnEntity(entity);

            if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                slot.TakeOut(1);
                slot.MarkDirty();
            }
        }
        //-- Returns the centered position in the neighbour block space --//
        private EntityPos OffsetHeartPosition(EntityPos pos, BlockFacing face)
        {
            if (face == BlockFacing.UP)
            {
                pos.Y += 1;
            }
            else if (face == BlockFacing.DOWN)
            {
                pos.Y += -1;
            }
            else if (face == BlockFacing.SOUTH)
            {
                pos.Z += 1;
            }
            else if (face == BlockFacing.NORTH)
            {
                pos.Z -= 1;
            }
            else if (face == BlockFacing.EAST)
            {
                pos.X += 1;
            }
            else if (face == BlockFacing.WEST)
            {
                pos.X -= 1;
            }

            pos.X += 0.5;
            pos.Z += 0.5;

            return pos;
        }
    }
}
