using EntityStates.TitanMonster;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using HealthComponent = On.RoR2.HealthComponent;
using Object = UnityEngine.Object;


namespace DarknessExpansion;

public class DarknessItems
{
    public static ItemTierDef darkTier;
    public static ItemDef testItem;

    
    public DarknessItems()
    {
        ColorCatalog.ColorIndex ci = ColorsAPI.RegisterColor(Color.black);
        darkTier = ScriptableObject.CreateInstance<ItemTierDef>();
        darkTier.tier = (ItemTier)11;
        darkTier.darkColorIndex = ci;
        //figure out how to change the color
        darkTier.colorIndex = ci;
        darkTier.highlightPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/UI/HighlightMisc.prefab")
            .WaitForCompletion().InstantiateClone("Dark Item Highlight");
        darkTier.dropletDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/PickupDroplet.prefab")
            .WaitForCompletion();
        darkTier.highlightPrefab.GetComponent<HighlightRect>().highlightColor = Color.black;
        darkTier.isDroppable = true;
        foreach (var item in darkTier.dropletDisplayPrefab.GetComponents<Object>())
        {
            Log.Debug(item.GetType());
        }
        darkTier.canScrap = false;
        darkTier.canRestack = false;
        ContentAddition.AddItemTierDef(darkTier);
        new DarkGolemItem();
    }

    public class DarkGolemItem
    {
        private ItemDef darkGolemItem;
        private Sprite darkGolemSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Knurl/texKnurlIcon.png").WaitForCompletion();

        private GameObject darkGolemPickup = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Knurl/PickupKnurl.prefab")
            .WaitForCompletion();

        private GameObject fistProjectilePrefab = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/Titan/TitanPreFistProjectile.prefab").WaitForCompletion()
            .InstantiateClone("Dark Fist");
        public DarkGolemItem()
        {
            darkGolemItem = ScriptableObject.CreateInstance<ItemDef>();
            darkGolemItem.name = "DARK_GOLEM_NAME";
            darkGolemItem.descriptionToken = "DARK_GOLEM_DESCRIPTION";
            darkGolemItem.nameToken = "DARK_GOLEM_NAME";
            darkGolemItem.loreToken = "DARK_GOLEM_LORE";
            darkGolemItem.pickupToken = "DARK_GOLEM_PICKUP";
            darkGolemItem.pickupIconSprite = darkGolemSprite;
            darkGolemItem.pickupModelPrefab = darkGolemPickup;
            darkGolemItem.canRemove = true;
            darkGolemItem.hidden = false;
            darkGolemItem._itemTierDef = darkTier;
            darkGolemItem.tier = (ItemTier)11;
            var displayRules = new ItemDisplayRuleDict(null);
            darkGolemItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkGolemItem, displayRules));
            HealthComponent.TakeDamageProcess += HealthComponentOnTakeDamageProcess;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManagerOnonCharacterDeathGlobal;
            Inventory.onServerItemGiven += InventoryOnonServerItemGiven;
            LanguageAPI.Add("DARK_GOLEM_NAME","Titanic Boulder");
            LanguageAPI.Add("DARK_GOLEM_DESCRIPTION","Gives 100 (+100 per stack) health and 10 (+10 per stack) regen. Upon taking damage, 20% chance to summon a fist for 200% (+200% per stack) damage + 100% damage (+100% per stack) per 500 health. Gives 10 (+10 per stack) health and 1 (+1 per stack) regen upon killing a dark enemy.");
            LanguageAPI.Add("DARK_GOLEM_PICKUP","Increases health and regen. Upon taking damage, chance to summon a fist. Fist damage scales with health. Grows stronger as it absorbs darkness.");
            testItem = darkGolemItem;
        }

        private void InventoryOnonServerItemGiven(Inventory arg1, ItemIndex arg2, int arg3)
        {
            if (arg2 == darkGolemItem.itemIndex)
            {
                Darkness.DarknessLevel += arg3;
                Darkness.UpdateDarkness();
                arg1.beadAppliedHealth += arg3 * 100;
                arg1.beadAppliedRegen += arg3 * 10;
                //figure out how to add armor
            }
        }
        

        private void GlobalEventManagerOnonCharacterDeathGlobal(DamageReport obj)
        {
            if (!obj.attacker || !obj.attackerBody)
            {
                return;
            }

            if (obj.attackerBody.inventory)
            {
                int numDarkGolems = obj.attackerBody.inventory.GetItemCount(darkGolemItem);
                if (numDarkGolems > 0 && obj.victimBody.inventory.GetEquipmentIndex() ==
                    Darkness.DarknessEquipment.equipmentIndex)
                {
                    obj.attackerBody.inventory.beadAppliedHealth += numDarkGolems * 10f;
                    obj.attackerBody.inventory.beadAppliedRegen += numDarkGolems;
                }
            }
        }

        private void HealthComponentOnTakeDamageProcess(HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, DamageInfo damageinfo)
        {
            if (self.body.inventory)
            {
                int numDarkGolems = self.body.inventory.GetItemCount(darkGolemItem);
                if (numDarkGolems > 0)
                {
                    if (Util.CheckRoll(20, self.body._master))
                    {
                        bool isCrit = self.body.RollCrit();
                        float damageValue = self.body.damage * (self.body.maxHealth + 1000f) * numDarkGolems / 500f;
                        FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                        fireProjectileInfo.projectilePrefab = fistProjectilePrefab;
                        fireProjectileInfo.position = damageinfo.attacker.transform.position;
                        fireProjectileInfo.rotation = Quaternion.identity;
                        fireProjectileInfo.owner = self.body.gameObject;
                        fireProjectileInfo.damage = damageValue;
                        fireProjectileInfo.force = FireFist.fistForce;
                        fireProjectileInfo.crit = isCrit;
                        fireProjectileInfo._fuseOverride = FireFist.entryDuration - FireFist.trackingDuration + 2f;
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    }
                }
            }

            orig(self, damageinfo);
        }
    }
}