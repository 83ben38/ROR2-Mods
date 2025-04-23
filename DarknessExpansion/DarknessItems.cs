using IL.EntityStates.DeepVoidPortalBattery;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DarknessExpansion;

public class DarknessItems
{
    private ItemTierDef darkTier;
    public static ItemDef darkGolemItem;

    private Sprite darkGolemSprite =
        Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Knurl/texKnurlIcon.png").WaitForCompletion();

    private GameObject darkGolemPickup = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Knurl/PickupKnurl.prefab")
        .WaitForCompletion();
    
    public DarknessItems()
    {
        darkTier = ScriptableObject.CreateInstance<ItemTierDef>();
        darkTier.tier = (ItemTier)11;
        darkTier.canScrap = false;
        ContentAddition.AddItemTierDef(darkTier);
        ItemTierDef whiteItemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset")
            .WaitForCompletion();
        darkGolemItem = ScriptableObject.CreateInstance<ItemDef>();
        darkGolemItem.name = "DARK_GOLEM_NAME";
        darkGolemItem.descriptionToken = "DARK_GOLEM_DESC";
        darkGolemItem.nameToken = "DARK_GOLEM_NAME";
        darkGolemItem.loreToken = "DARK_GOLEM_LORE";
        darkGolemItem.pickupToken = "DARK_GOLEM_PICKUP";
        darkGolemItem.pickupIconSprite = darkGolemSprite;
        darkGolemItem.pickupModelPrefab = darkGolemPickup;
        darkGolemItem.canRemove = true;
        darkGolemItem.hidden = false;
        darkGolemItem._itemTierDef = whiteItemTierDef;
        var displayRules = new ItemDisplayRuleDict(null);
        ItemAPI.Add(new CustomItem(darkGolemItem, displayRules));
    }
}