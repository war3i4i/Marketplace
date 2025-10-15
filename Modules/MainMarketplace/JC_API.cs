using Marketplace.ExternalLoads;
namespace Marketplace.Modules.MainMarketplace;

public static class JC_API_Class
{
    public static void JC_Api_Tooltip(ItemDrop.ItemData item, GameObject JC)
    {
        Jewelcrafting.API.FillItemContainerTooltip(item, JC.transform.parent, false);
    }
}