using System;
using System.Collections.Generic;
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
    public static List<ItemIndex> darkItems = new();
    public static Action<CharacterBody> onKillDarknessEnemy;
    public static ItemDef stackingDarkItem;
    
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
        GlobalEventManager.onCharacterDeathGlobal += GlobalEventManagerOnonCharacterDeathGlobal;
        new DarkGolemItem();
        new DarkBeetleItem();
        new DarkPearlItem();
        new DarkPearlItem2();
        new DarkJellyfishItem();
        new DarkWispItem();
        new DarkBleedItem();
        Inventory.onServerItemGiven += InventoryOnonServerItemGiven;

        stackingDarkItem = ScriptableObject.CreateInstance<ItemDef>();
        stackingDarkItem.hidden = true;
        stackingDarkItem.canRemove = false;
        stackingDarkItem.tier = ItemTier.NoTier;
        stackingDarkItem.itemIndex = ItemIndex.Count;
        onKillDarknessEnemy += body => body.inventory.GiveItem(stackingDarkItem);
        ContentAddition.AddItemDef(stackingDarkItem);
        
        On.RoR2.CharacterBody.RecalculateStats += CharacterBodyOnRecalculateStats;
    }

    private void CharacterBodyOnRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
    {
        int numDarknessStacks = self.inventory.GetItemCount(stackingDarkItem);
        int numDarkGolems = self.inventory.GetItemCount(DarkGolemItem.darkGolemItem);
        int numDarkBeetles = self.inventory.GetItemCount(DarkBeetleItem.darkBeetleItem);
        int numDarkPearls = self.inventory.GetItemCount(DarkPearlItem.darkPearlItem);
        int numDarkBetterPearls = self.inventory.GetItemCount(DarkPearlItem2.darkPearlItem);
        int numDarkJellyfish = self.inventory.GetItemCount(DarkJellyfishItem.darkJellyfishItem);
        int numDarkWisps = self.inventory.GetItemCount(DarkWispItem.darkWispItem);
        int numDarkBleedItems = self.inventory.GetItemCount(DarkBleedItem.darkBleedItem);
        self.maxHealth += (numDarkGolems * 100) + (numDarkGolems * 5 * numDarknessStacks);
        self.regen += (numDarkGolems * 10) + (numDarkGolems * 1 * numDarknessStacks);
        self.critMultiplier += (numDarkBleedItems * numDarknessStacks * .03f);
        orig(self);
        self.attackSpeed += 1 + (numDarkBeetles * numDarknessStacks * .03f);
        self.moveSpeed *= 1 + (numDarkWisps * numDarknessStacks * .03f);
        self.maxHealth *= 1 + (numDarkPearls * .5f);
        self.maxHealth *= 1 + (numDarkPearls * numDarknessStacks * .02f);
        float darkBetterPearlMultiplier = 1 + (numDarkBetterPearls * .5f);
        darkBetterPearlMultiplier *= 1 + (numDarkBetterPearls * numDarknessStacks * .01f);
        self.maxHealth *= darkBetterPearlMultiplier;
        self.regen *= darkBetterPearlMultiplier;
        self.moveSpeed *= darkBetterPearlMultiplier;
        self.damage *= darkBetterPearlMultiplier;
        self.crit *= darkBetterPearlMultiplier;
        self.attackSpeed *= darkBetterPearlMultiplier;
        self.armor *= darkBetterPearlMultiplier;
        
    }

    private void GlobalEventManagerOnonCharacterDeathGlobal(DamageReport obj)
    {
        if (!obj.attacker || !obj.attackerBody)
        {
            return;
        }

        if (obj.attackerBody.inventory)
        {
            if (obj.victimBody.inventory.GetEquipmentIndex() ==
                Darkness.DarknessEquipment.equipmentIndex)
            {
                onKillDarknessEnemy.Invoke(obj.attackerBody);
            }
        }
    }

    private void InventoryOnonServerItemGiven(Inventory arg1, ItemIndex arg2, int arg3)
    {
        if (darkItems.Contains(arg2))
        {
            Darkness.DarknessLevel += arg3;
            Darkness.UpdateDarkness();
        }
    }

    public class DarkGolemItem
    {
        public static ItemDef darkGolemItem;
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
            Inventory.onServerItemGiven += InventoryOnonServerItemGiven;
            onKillDarknessEnemy += OnKillDarknessEnemy;

            

            LanguageAPI.Add("DARK_GOLEM_NAME","Titanic Boulder");
            LanguageAPI.Add("DARK_GOLEM_DESCRIPTION","Gives 100 (+100 per stack) health and 10 (+10 per stack) regen. Upon taking damage, 20% chance to summon a fist for 200% (+200% per stack) damage + 100% damage (+100% per stack) per 500 health. Gives 5 (+5 per stack) health and 1 (+1 per stack) regen upon killing a dark enemy.");
            LanguageAPI.Add("DARK_GOLEM_PICKUP","Increases health and regen. Upon taking damage, chance to summon a fist. Fist damage scales with health. Grows stronger as it absorbs darkness.");
            testItem = darkGolemItem;
            darkItems.Add(darkGolemItem.itemIndex);
        }
        private void OnKillDarknessEnemy(CharacterBody obj)
        {
            int numDarkGolems = obj.inventory.GetItemCount(darkGolemItem);
            if (numDarkGolems > 0)
            {
                obj.inventory.beadAppliedHealth += numDarkGolems * 5f;
                obj.inventory.beadAppliedRegen += numDarkGolems;
            }
        }

        private void InventoryOnonServerItemGiven(Inventory arg1, ItemIndex arg2, int arg3)
        {
            if (arg2 == darkGolemItem.itemIndex)
            {
                arg1.beadAppliedHealth += arg3 * 100;
                arg1.beadAppliedRegen += arg3 * 10;
                //figure out how to add armor
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

    public class DarkBeetleItem
    {
        public static ItemDef darkBeetleItem;

        private Sprite darkBeetleSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/BeetleGland/texBeetleGlandIcon.png").WaitForCompletion();

        private GameObject darkBeetlePickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/BeetleGland/PickupBeetleGland.prefab")
            .WaitForCompletion();
        

        public DarkBeetleItem()
        {
            darkBeetleItem = ScriptableObject.CreateInstance<ItemDef>();
            darkBeetleItem.name = "DARK_BEETLE_NAME";
            darkBeetleItem.descriptionToken = "DARK_BEETLE_DESCRIPTION";
            darkBeetleItem.nameToken = "DARK_BEETLE_NAME";
            darkBeetleItem.loreToken = "DARK_BEETLE_LORE";
            darkBeetleItem.pickupToken = "DARK_BEETLE_PICKUP";
            darkBeetleItem.pickupIconSprite = darkBeetleSprite;
            darkBeetleItem.pickupModelPrefab = darkBeetlePickup;
            darkBeetleItem.canRemove = true;
            darkBeetleItem.hidden = false;
            darkBeetleItem._itemTierDef = darkTier;
            darkBeetleItem.tier = (ItemTier)11;
            var displayRules = new ItemDisplayRuleDict(null);
            darkBeetleItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkBeetleItem, displayRules));
            LanguageAPI.Add("DARK_BEETLE_NAME", "King's Gland");
            LanguageAPI.Add("DARK_BEETLE_DESCRIPTION",
                "Every 30 seconds, summon a Beetle Guard with 300% (+300% per stack) damage and 300% (+300% per stack) health. Beetle Guards apply 1 (+1 per stack) debuff on hit. Can have up to 1 (+1 per stack) beetle guard at a time. Give your beetles your attack speed. Upon killing a dark enemy, gain 3% (+3% per stack) attack speed.");
            LanguageAPI.Add("DARK_BEETLE_PICKUP",
                "Summon a beetle guard which applies random debuffs on hit. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkBeetleItem.itemIndex);
        }
    }
    public class DarkPearlItem
    {
        public static ItemDef darkPearlItem;

        private Sprite darkPearlSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Pearl/texPearlIcon.png").WaitForCompletion();

        private GameObject darkPearlPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/Pearl/PickupPearl.prefab")
            .WaitForCompletion();
        

        public DarkPearlItem()
        {
            darkPearlItem = ScriptableObject.CreateInstance<ItemDef>();
            darkPearlItem.name = "DARK_PEARL_NAME";
            darkPearlItem.descriptionToken = "DARK_PEARL_DESCRIPTION";
            darkPearlItem.nameToken = "DARK_PEARL_NAME";
            darkPearlItem.loreToken = "DARK_PEARL_LORE";
            darkPearlItem.pickupToken = "DARK_PEARL_PICKUP";
            darkPearlItem.pickupIconSprite = darkPearlSprite;
            darkPearlItem.pickupModelPrefab = darkPearlPickup;
            darkPearlItem.canRemove = true;
            darkPearlItem.hidden = false;
            darkPearlItem._itemTierDef = darkTier;
            darkPearlItem.tier = (ItemTier)11;
            var displayRules = new ItemDisplayRuleDict(null);
            darkPearlItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkPearlItem, displayRules));
            LanguageAPI.Add("DARK_PEARL_NAME", "Dark Pearl");
            LanguageAPI.Add("DARK_PEARL_DESCRIPTION",
                "Increases maximum health by 50% (+50% per stack). Upon killing a dark enemy, increases health by 2% (+2% per stack).");
            LanguageAPI.Add("DARK_PEARL_PICKUP",
                "Increases health. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkPearlItem.itemIndex);
        }
    }
    public class DarkPearlItem2
    {
        public static ItemDef darkPearlItem;

        private Sprite darkPearlSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ShinyPearl/texShinyPearlIcon.png").WaitForCompletion();

        private GameObject darkPearlPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/ShinyPearl/PickupShinyPearl.prefab")
            .WaitForCompletion();
        

        public DarkPearlItem2()
        {
            darkPearlItem = ScriptableObject.CreateInstance<ItemDef>();
            darkPearlItem.name = "DARK_PEARL_NAME2";
            darkPearlItem.descriptionToken = "DARK_PEARL_DESCRIPTION2";
            darkPearlItem.nameToken = "DARK_PEARL_NAME2";
            darkPearlItem.loreToken = "DARK_PEARL_LORE2";
            darkPearlItem.pickupToken = "DARK_PEARL_PICKUP2";
            darkPearlItem.pickupIconSprite = darkPearlSprite;
            darkPearlItem.pickupModelPrefab = darkPearlPickup;
            darkPearlItem.canRemove = true;
            darkPearlItem.hidden = false;
            darkPearlItem._itemTierDef = darkTier;
            darkPearlItem.tier = (ItemTier)11;
            var displayRules = new ItemDisplayRuleDict(null);
            darkPearlItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkPearlItem, displayRules));
            LanguageAPI.Add("DARK_PEARL_NAME2", "Dark Irradient Pearl");
            LanguageAPI.Add("DARK_PEARL_DESCRIPTION2",
                "Increases all stats by 50% (+50% per stack). Upon killing a dark enemy, increases all stats by 1% (+1% per stack).");
            LanguageAPI.Add("DARK_PEARL_PICKUP2",
                "Increases all stats. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkPearlItem.itemIndex);
        }
    }
    public class DarkJellyfishItem
    {
        public static ItemDef darkJellyfishItem;

        private Sprite darkJellyfishSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/NovaOnLowHealth/texJellyGutsIcon.png").WaitForCompletion();

        private GameObject darkJellyfishPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/NovaOnLowHealth/PickupJellyGuts.prefab")
            .WaitForCompletion();
        

        public DarkJellyfishItem()
        {
            darkJellyfishItem = ScriptableObject.CreateInstance<ItemDef>();
            darkJellyfishItem.name = "DARK_JELLYFISH_NAME";
            darkJellyfishItem.descriptionToken = "DARK_JELLYFISH_DESCRIPTION";
            darkJellyfishItem.nameToken = "DARK_JELLYFISH_NAME";
            darkJellyfishItem.loreToken = "DARK_JELLYFISH_LORE";
            darkJellyfishItem.pickupToken = "DARK_JELLYFISH_PICKUP";
            darkJellyfishItem.pickupIconSprite = darkJellyfishSprite;
            darkJellyfishItem.pickupModelPrefab = darkJellyfishPickup;
            darkJellyfishItem.canRemove = true;
            darkJellyfishItem.hidden = false;
            darkJellyfishItem._itemTierDef = darkTier;
            darkJellyfishItem.tier = (ItemTier)11;
            var displayRules = new ItemDisplayRuleDict(null);
            darkJellyfishItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkJellyfishItem, displayRules));
            LanguageAPI.Add("DARK_JELLYFISH_NAME", "Omega Loop");
            LanguageAPI.Add("DARK_JELLYFISH_DESCRIPTION",
                "When below 50% health, every 30 / 2 (+1 per stack) seconds, charge an explosion, dealing 6000% damage (+6000% per stack). Additionally, gain 3 (+3 per stack) charges. Upon using your secondary, release a ball of lightning that deaals 500% base damage (+500% per stack). Upon killing a dark enemy, gain 1% (+1% per stack) cooldown reduction, which affects this item.");
            LanguageAPI.Add("DARK_JELLYFISH_PICKUP",
                "Upon reaching low health, explode in an area. Upon using your secondary, release a ball of lightning. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkJellyfishItem.itemIndex);
        }
    }
    public class DarkWispItem
    {
        public static ItemDef darkWispItem;

        private Sprite darkWispSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/SprintWisp/texBrokenMaskIcon.png").WaitForCompletion();

        private GameObject darkWispPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/SprintWisp/PickupBrokenMask.prefab")
            .WaitForCompletion();
        

        public DarkWispItem()
        {
            darkWispItem = ScriptableObject.CreateInstance<ItemDef>();
            darkWispItem.name = "DARK_WISP_NAME";
            darkWispItem.descriptionToken = "DARK_WISP_DESCRIPTION";
            darkWispItem.nameToken = "DARK_WISP_NAME";
            darkWispItem.loreToken = "DARK_WISP_LORE";
            darkWispItem.pickupToken = "DARK_WISP_PICKUP";
            darkWispItem.pickupIconSprite = darkWispSprite;
            darkWispItem.pickupModelPrefab = darkWispPickup;
            darkWispItem.canRemove = true;
            darkWispItem.hidden = false;
            darkWispItem._itemTierDef = darkTier;
            darkWispItem.tier = (ItemTier)11;
            var displayRules = new ItemDisplayRuleDict(null);
            darkWispItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkWispItem, displayRules));
            LanguageAPI.Add("DARK_WISP_NAME", "Large Disciple");
            LanguageAPI.Add("DARK_WISP_DESCRIPTION",
                "Fire 3 (+3 per stack) tracking wisps for 300% (+300% per stack) base damage. Wisps have 3.0 (+3 per stack) proc coefficient. Fires every 1.6 seconds while sprinting. Fire rate increases with movement speed. Upon killing a dark enemy, gain 3% (+3% per stack) movement speed.");
            LanguageAPI.Add("DARK_WISP_PICKUP",
                "Fire 3 tracking wisps while sprinting. Fire rate scales with move speed. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkWispItem.itemIndex);
        }
    }
    public class DarkBleedItem
    {
        public static ItemDef darkBleedItem;

        private Sprite darkBleedSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/BleedOnHitAndExplode/texBleedOnHitAndExplodeIcon.png").WaitForCompletion();

        private GameObject darkBleedPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/DisplayBleedOnHitAndExplode.prefab")
            .WaitForCompletion();
        

        public DarkBleedItem()
        {
            darkBleedItem = ScriptableObject.CreateInstance<ItemDef>();
            darkBleedItem.name = "DARK_BLEED_NAME";
            darkBleedItem.descriptionToken = "DARK_BLEED_DESCRIPTION";
            darkBleedItem.nameToken = "DARK_BLEED_NAME";
            darkBleedItem.loreToken = "DARK_BLEED_LORE";
            darkBleedItem.pickupToken = "DARK_BLEED_PICKUP";
            darkBleedItem.pickupIconSprite = darkBleedSprite;
            darkBleedItem.pickupModelPrefab = darkBleedPickup;
            darkBleedItem.canRemove = true;
            darkBleedItem.hidden = false;
            darkBleedItem._itemTierDef = darkTier;
            darkBleedItem.tier = (ItemTier)11;
            var displayRules = new ItemDisplayRuleDict(null);
            darkBleedItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkBleedItem, displayRules));
            LanguageAPI.Add("DARK_BLEED_NAME", "Dark Shatterspleen");
            LanguageAPI.Add("DARK_BLEED_DESCRIPTION",
                "Gain 20% critical chance. All strikes apply 1 stack of bleed. Crits apply bonus bleed based on crit damage. Bleeding enemies explode on death for 100% damage (+100% per stack) damage per bleed stack + 15% (+15% per stack) of their max health. 10% (+10% per stack) of bleed is applied to hit enemies. Upon killing a dark enemy, gain 3% (+3% per stack) crit damage.");
            LanguageAPI.Add("DARK_BLEED_PICKUP",
                "All hits apply bleed, and crits apply extra. Bleeding enemies explode, dealing damage and applying bleed to nearby enemies. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkBleedItem.itemIndex);
        }
    }
}