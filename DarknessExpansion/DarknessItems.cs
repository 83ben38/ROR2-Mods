using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DarknessExpansion;

public class DarknessItems
{
    private ItemTierDef darkTier;
    private ItemDef darkGolemItem;

    private Sprite darkGolemSprite =
        Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Knurl/texKnurlIcon.png").WaitForCompletion();

    private GameObject darkGolemPickup = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Knurl/PickupKnurl.prefab")
        .WaitForCompletion();
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
        darkGolemItem.pickupIconSprite = darkGolemSprite;
        darkGolemItem.pickupModelPrefab = darkGolemPickup;
    }
}