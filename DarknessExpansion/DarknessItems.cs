using R2API;
using RoR2;
using UnityEngine;

namespace DarknessExpansion;

public class DarknessItems
{
    private ItemTierDef darkTier;
    private ItemDef darkGolemItem;
    public DarknessItems()
    {
        darkTier = ScriptableObject.CreateInstance<ItemTierDef>();
        darkTier.tier = ItemTier.AssignedAtRuntime;
        darkTier.canScrap = false;
        ContentAddition.AddItemTierDef(darkTier);
        
        darkGolemItem = ScriptableObject.CreateInstance<ItemDef>();
        darkGolemItem._itemTierDef = darkTier;
        darkGolemItem.descriptionToken = "DARK_GOLEM_DESC";
        darkGolemItem.nameToken = "DARK_GOLEM_NAME";
        darkGolemItem.loreToken = "DARK_GOLEM_LORE";
        darkGolemItem.pickupToken = "DARK_GOLEM_PICKUP";

    }
}