using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace OreCrystals
{
    class CrystalPlanter: Block
    {
        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            ItemSlot oreSlot = null, planterSlot = null;

            foreach(ItemSlot slot in allInputslots)
            {
                if(!slot.Empty)
                {
                    if (slot.Itemstack.Collectible is BlockOre)
                        oreSlot = slot;
                    else if (slot.Itemstack.Collectible is BlockPlantContainer)
                        planterSlot = slot;
                }
            }
            if(oreSlot != null && planterSlot != null)
            {
                Block block = api.World.GetBlock(new AssetLocation("orecrystals", "crystal_planter-" + oreSlot.Itemstack.Collectible.LastCodePart(1) + "-" + oreSlot.Itemstack.Collectible.LastCodePart(0) + "-" + planterSlot.Itemstack.Collectible.LastCodePart()));
                ItemStack outStack = new ItemStack(block);

                outputSlot.Itemstack = outStack;
            }

            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe);
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            //string description = Lang.Get("orecrystals:blockhelp-crystal-planter-desc");

            //dsc.Append(description);
        }
        /*
        public override RichTextComponentBase[] GetHandbookInfo(ItemSlot inSlot, ICoreClientAPI capi, ItemStack[] allStacks, ActionConsumable<string> openDetailPageFor)
        {
            RichTextComponentBase[] oldText = base.GetHandbookInfo(inSlot, capi, allStacks, openDetailPageFor);
            RichTextComponentBase[] derp = new RichTextComponentBase[1] { new RichTextComponent(capi, Lang.Get("orecrystals:blockhelp-crystal-planter-desc"), CairoFont.WhiteSmallText()) };

            oldText[1] = new RichTextComponent(capi, Lang.Get("orecrystals:blockhelp-crystal-planter-title"), CairoFont.WhiteSmallishText());
            return oldText.Concat(derp).ToArray();
        }
        */
    }
}
