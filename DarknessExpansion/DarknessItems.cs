using System;
using System.Collections.Generic;
using EntityStates.TitanMonster;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using CharacterMaster = On.RoR2.CharacterMaster;
using HealthComponent = On.RoR2.HealthComponent;
using Object = UnityEngine.Object;


namespace DarknessExpansion;

public class DarknessItems
{
    public static ItemTierDef darkTier;
    public static ItemDef testItem;
    public static List<ItemDef> darkItems = new();
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
        darkTier.dropletDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/Tier1Orb.prefab")
            .WaitForCompletion().InstantiateClone("Darkness Orb");
        darkTier.dropletDisplayPrefab.GetComponentInChildren<Light>().color = Color.black;
        darkTier.dropletDisplayPrefab.GetComponentInChildren<TrailRenderer>().startColor= Color.black;
        darkTier.dropletDisplayPrefab.GetComponentInChildren<TrailRenderer>().endColor= new Color(0,0,0,0);
        darkTier.highlightPrefab.GetComponent<HighlightRect>().highlightColor = Color.black;
        darkTier.isDroppable = true;
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
        new DarkClayItem();
        new DarkConstructItem();
        new DarkCoreItem();
        new DarkParentItem();
        new DarkStacksItem();
        Inventory.onServerItemGiven += InventoryOnonServerItemGiven;
        onKillDarknessEnemy += body => body.inventory.GiveItem(stackingDarkItem);
        On.RoR2.CharacterBody.RecalculateStats += CharacterBodyOnRecalculateStats; 
        CharacterMaster.OnInventoryChanged += CharacterMasterOnOnInventoryChanged;
        HealthComponent.Heal += HealthComponentOnHeal;
    }

    private void CharacterMasterOnOnInventoryChanged(CharacterMaster.orig_OnInventoryChanged orig, RoR2.CharacterMaster self)
    {
        int numDarknessStacks = self.inventory.GetItemCount(stackingDarkItem);
        int numDarkConstructItems = self.inventory.GetItemCount(DarkConstructItem.darkConstructItem);
        orig(self);
        self.luck += numDarknessStacks * numDarkConstructItems * .03f;
    }

    private float HealthComponentOnHeal(HealthComponent.orig_Heal orig, RoR2.HealthComponent self, float amount, ProcChainMask procchainmask, bool nonregen)
    {
        if (self.body.inventory)
        {
            int numDarkClay = self.body.inventory.GetItemCount(DarkClayItem.darkClayItem);
            int numDarknessStacks = self.body.inventory.GetItemCount(stackingDarkItem);
            float healMultiplier = 1 + (numDarknessStacks * numDarkClay * .03f);
            return orig(self, amount * healMultiplier, procchainmask, nonregen);
        }

        return orig(self, amount, procchainmask, nonregen);
    }

    private void CharacterBodyOnRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
    {
        if (self.inventory)
        {
            int numDarknessStacks = self.inventory.GetItemCount(stackingDarkItem);
            int numDarkGolems = self.inventory.GetItemCount(DarkGolemItem.darkGolemItem);
            int numDarkBeetles = self.inventory.GetItemCount(DarkBeetleItem.darkBeetleItem);
            int numDarkPearls = self.inventory.GetItemCount(DarkPearlItem.darkPearlItem);
            int numDarkBetterPearls = self.inventory.GetItemCount(DarkPearlItem2.darkPearlItem);
            int numDarkJellyfish = self.inventory.GetItemCount(DarkJellyfishItem.darkJellyfishItem);
            int numDarkWisps = self.inventory.GetItemCount(DarkWispItem.darkWispItem);
            int numDarkBleedItems = self.inventory.GetItemCount(DarkBleedItem.darkBleedItem);
            int numDarkCoreItems = self.inventory.GetItemCount(DarkCoreItem.darkCoreItem);
            int numDarkParentItems = self.inventory.GetItemCount(DarkParentItem.darkParentItem);
            orig(self);
            if (numDarkBleedItems > 0)
            {
                self.crit += 20f;
            }

            self.maxHealth += (numDarkGolems * 100) + (numDarkGolems * 5 * numDarknessStacks);
            self.healthComponent.Heal((numDarkGolems * 100) + (numDarkGolems * 5 * numDarknessStacks),
                default, false);
            self.regen += (numDarkGolems * 10) + (numDarkGolems * 1 * numDarknessStacks);
            self.critMultiplier += (numDarkBleedItems * numDarknessStacks * .03f);
            self.armor += (numDarkParentItems * numDarknessStacks * 1.5f);
            self.attackSpeed *= 1 + (numDarkBeetles * numDarknessStacks * .03f);
            self.moveSpeed *= 1 + (numDarkWisps * numDarknessStacks * .03f);
            float prevMaxHealth = self.maxHealth;
            self.maxHealth *= 1 + (numDarkPearls * .5f);
            self.maxHealth *= 1 + (numDarkPearls * numDarknessStacks * .02f);
            self.healthComponent.Heal(self.maxHealth-prevMaxHealth,
                default, false);
            float darkBetterPearlMultiplier = 1 + (numDarkBetterPearls * .5f);
            darkBetterPearlMultiplier *= 1 + (numDarkBetterPearls * numDarknessStacks * .01f);
            self.maxHealth *= darkBetterPearlMultiplier;
            self.regen *= darkBetterPearlMultiplier;
            self.moveSpeed *= darkBetterPearlMultiplier;
            self.damage *= darkBetterPearlMultiplier;
            self.crit *= darkBetterPearlMultiplier;
            self.attackSpeed *= darkBetterPearlMultiplier;
            self.armor *= darkBetterPearlMultiplier;
            float cooldownMult = Mathf.Pow(1-(.01f*numDarkJellyfish),numDarknessStacks);
            if (self.skillLocator.primary)
            {
                self.skillLocator.primary.cooldownScale = cooldownMult;
            }
            if (self.skillLocator.secondaryBonusStockSkill)
            {
                self.skillLocator.secondaryBonusStockSkill.cooldownScale = cooldownMult;
            }
            if (self.skillLocator.utilityBonusStockSkill)
            {
                self.skillLocator.utilityBonusStockSkill.cooldownScale = cooldownMult;
            }
            if (self.skillLocator.specialBonusStockSkill)
            {
                self.skillLocator.specialBonusStockSkill.cooldownScale = cooldownMult;
            }
        }
        else
        {
            orig(self);
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
            if (obj.victimBody.inventory.GetEquipmentIndex() ==
                Darkness.DarknessEquipment.equipmentIndex)
            {
                onKillDarknessEnemy.Invoke(obj.attackerBody);
            }
        }
    }

    private void InventoryOnonServerItemGiven(Inventory arg1, ItemIndex arg2, int arg3)
    {
        if (darkItems.Contains(ItemCatalog.GetItemDef(arg2)))
        {
            Darkness.DarknessLevel += arg3;
            Darkness.UpdateDarkness();
        }
    }

    public class DarkStacksItem
    {

        private Sprite darkGolemSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Knurl/texKnurlIcon.png").WaitForCompletion();

        private GameObject darkGolemPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/Knurl/PickupKnurl.prefab")
            .WaitForCompletion();

        public DarkStacksItem()
        {
            stackingDarkItem = ScriptableObject.CreateInstance<ItemDef>();
            stackingDarkItem.name = "DARK_STACK_NAME";
            stackingDarkItem.descriptionToken = "DARK_STACK_DESCRIPTION";
            stackingDarkItem.nameToken = "DARK_STACK_NAME";
            stackingDarkItem.loreToken = "DARK_STACK_LORE";
            stackingDarkItem.pickupToken = "DARK_STACK_PICKUP";
            stackingDarkItem.pickupIconSprite = darkGolemSprite;
            stackingDarkItem.pickupModelPrefab = darkGolemPickup;
            stackingDarkItem.canRemove = false;
            stackingDarkItem.hidden = true;
            stackingDarkItem._itemTierDef = darkTier;
            var displayRules = new ItemDisplayRuleDict(null);
            stackingDarkItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(stackingDarkItem, displayRules));
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
            var displayRules = new ItemDisplayRuleDict(null);
            darkGolemItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkGolemItem, displayRules));
            HealthComponent.TakeDamageProcess += HealthComponentOnTakeDamageProcess;
            LanguageAPI.Add("DARK_GOLEM_NAME", "Titanic Boulder");
            LanguageAPI.Add("DARK_GOLEM_DESCRIPTION",
                "Gives 100 (+100 per stack) health and 10 (+10 per stack) regen. Upon taking damage, 20% chance to summon a fist for 200% (+200% per stack) damage + 100% damage (+100% per stack) per 500 health. Gives 5 (+5 per stack) health and 1 (+1 per stack) regen upon killing a dark enemy.");
            LanguageAPI.Add("DARK_GOLEM_PICKUP",
                "Increases health and regen. Upon taking damage, chance to summon a fist. Fist damage scales with health. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkGolemItem);
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
            var displayRules = new ItemDisplayRuleDict(null);
            darkBeetleItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkBeetleItem, displayRules));
            LanguageAPI.Add("DARK_BEETLE_NAME", "King's Gland");
            LanguageAPI.Add("DARK_BEETLE_DESCRIPTION",
                "Every 30 seconds, summon a Beetle Guard with 300% (+300% per stack) damage and 300% (+300% per stack) health. Beetle Guards apply 1 (+1 per stack) debuff on hit. Can have up to 1 (+1 per stack) beetle guard at a time. Give your beetles your attack speed. Upon killing a dark enemy, gain 3% (+3% per stack) attack speed.");
            LanguageAPI.Add("DARK_BEETLE_PICKUP",
                "Summon a beetle guard which applies random debuffs on hit. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkBeetleItem);
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
            var displayRules = new ItemDisplayRuleDict(null);
            darkPearlItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkPearlItem, displayRules));
            LanguageAPI.Add("DARK_PEARL_NAME", "Dark Pearl");
            LanguageAPI.Add("DARK_PEARL_DESCRIPTION",
                "Increases maximum health by 50% (+50% per stack). Upon killing a dark enemy, increases health by 2% (+2% per stack).");
            LanguageAPI.Add("DARK_PEARL_PICKUP",
                "Increases health. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkPearlItem);
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
            var displayRules = new ItemDisplayRuleDict(null);
            darkPearlItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkPearlItem, displayRules));
            LanguageAPI.Add("DARK_PEARL_NAME2", "Dark Irradient Pearl");
            LanguageAPI.Add("DARK_PEARL_DESCRIPTION2",
                "Increases all stats by 50% (+50% per stack). Upon killing a dark enemy, increases all stats by 1% (+1% per stack).");
            LanguageAPI.Add("DARK_PEARL_PICKUP2",
                "Increases all stats. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkPearlItem);
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
            var displayRules = new ItemDisplayRuleDict(null);
            darkJellyfishItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkJellyfishItem, displayRules));
            LanguageAPI.Add("DARK_JELLYFISH_NAME", "Omega Loop");
            LanguageAPI.Add("DARK_JELLYFISH_DESCRIPTION",
                "When below 50% health, every 30 / 2 (+1 per stack) seconds, charge an explosion, dealing 6000% damage (+6000% per stack). Additionally, gain 3 (+3 per stack) charges. Upon using your secondary, release a ball of lightning that deaals 500% base damage (+500% per stack). Upon killing a dark enemy, gain 1% (+1% per stack) cooldown reduction, which affects this item.");
            LanguageAPI.Add("DARK_JELLYFISH_PICKUP",
                "Upon reaching low health, explode in an area. Upon using your secondary, release a ball of lightning. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkJellyfishItem);
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
            var displayRules = new ItemDisplayRuleDict(null);
            darkWispItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkWispItem, displayRules));
            LanguageAPI.Add("DARK_WISP_NAME", "Large Disciple");
            LanguageAPI.Add("DARK_WISP_DESCRIPTION",
                "Fire 3 (+3 per stack) tracking wisps for 300% (+300% per stack) base damage. Wisps have 3.0 (+3 per stack) proc coefficient. Fires every 1.6 seconds while sprinting. Fire rate increases with movement speed. Upon killing a dark enemy, gain 3% (+3% per stack) movement speed.");
            LanguageAPI.Add("DARK_WISP_PICKUP",
                "Fire 3 tracking wisps while sprinting. Fire rate scales with move speed. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkWispItem);
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
            var displayRules = new ItemDisplayRuleDict(null);
            darkBleedItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkBleedItem, displayRules));
            LanguageAPI.Add("DARK_BLEED_NAME", "Dark Shatterspleen");
            LanguageAPI.Add("DARK_BLEED_DESCRIPTION",
                "Gain 20% critical chance. All strikes apply 1 stack of bleed. Crits apply bonus bleed based on crit damage. Bleeding enemies explode on death for 100% damage (+100% per stack) damage per bleed stack + 15% (+15% per stack) of their max health. Upon killing a dark enemy, gain 3% (+3% per stack) crit damage.");
            LanguageAPI.Add("DARK_BLEED_PICKUP",
                "All hits apply bleed, and crits apply extra. Bleeding enemies explode, dealing damage to nearby enemies. Grows stronger as it absorbs darkness.");
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManagerOnProcessHitEnemy;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManagerOnonCharacterDeathGlobal;
            testItem = darkBleedItem;
            darkItems.Add(darkBleedItem);
        }

        private void GlobalEventManagerOnonCharacterDeathGlobal(DamageReport obj)
        {
            if (!obj.attacker || !obj.attackerBody)
            {
                return;
            }

            if (obj.attackerBody.inventory)
            {
                int numDarkBleedItems = obj.attackerBody.inventory.GetItemCount(darkBleedItem);
                if (numDarkBleedItems > 0 && obj.victimBody.HasBuff(RoR2Content.Buffs.Bleeding) || obj.victimBody.HasBuff(RoR2Content.Buffs.SuperBleed))
                {
                    Util.PlaySound("Play_bleedOnCritAndExplode_explode", obj.victimBody.gameObject);
                    Vector3 position = obj.victimBody.transform.position;
                    float damageCoefficient = 1f * numDarkBleedItems * (obj.victimBody.GetBuffCount(RoR2Content.Buffs.Bleeding) + 3);
                    float num = 0.15f * numDarkBleedItems;
                    float baseDamage = Util.OnKillProcDamage(obj.attackerBody.damage, damageCoefficient) + obj.victimBody.maxHealth * num;
                    GameObject gameObject = Object.Instantiate(GlobalEventManager.CommonAssets.bleedOnHitAndExplodeBlastEffect, position, Quaternion.identity);
                    DelayBlast component = gameObject.GetComponent<DelayBlast>();
                    component.position = position;
                    component.baseDamage = baseDamage;
                    component.baseForce = 0f;
                    component.radius = 16f;
                    component.attacker = obj.attacker;
                    component.inflictor = null;
                    component.crit = Util.CheckRoll(obj.attackerBody.crit, obj.attackerMaster);
                    component.maxTimer = 0f;
                    component.damageColorIndex = DamageColorIndex.Item;
                    component.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                    gameObject.GetComponent<TeamFilter>().teamIndex = obj.attackerTeamIndex;
                    NetworkServer.Spawn(gameObject);
                }
            }
        }

        private void GlobalEventManagerOnProcessHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageinfo, GameObject victim)
        {
            orig(self, damageinfo, victim);
            if (damageinfo.attacker && damageinfo.procCoefficient > 0f)
            {
                CharacterBody component2 = damageinfo.attacker.GetComponent<CharacterBody>();
                if (component2)
                {
                    var master = component2.master;
                    if (master)
                    {
                        if (!damageinfo.procChainMask.HasProc(ProcType.BleedOnHit))
                        {
                            Inventory inventory = master.inventory;
                            int numDarkBleedItems = inventory.GetItemCount(darkBleedItem);
                            if (numDarkBleedItems > 0)
                            {
                                ProcChainMask procChainMask2 = damageinfo.procChainMask;
                                procChainMask2.AddProc(ProcType.BleedOnHit);
                                int numBleeds = 1;
                                if (damageinfo.crit)
                                {
                                    numBleeds *= (int)(component2.critMultiplier);
                                }
                                for (int i = 0; i < numBleeds; i++)
                                {
                                    DotController.InflictDot(victim, damageinfo.attacker, DotController.DotIndex.Bleed, 3f * damageinfo.procCoefficient, 1f);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public class DarkClayItem
    {
        public static ItemDef darkClayItem;

        private Sprite darkClaySprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/SiphonOnLowHealth/texSiphonOnLowHealthIcon.png").WaitForCompletion();

        private GameObject darkClayPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/SiphonOnLowHealth/PickupSiphonOnLowHealth.prefab")
            .WaitForCompletion();
        

        public DarkClayItem()
        {
            darkClayItem = ScriptableObject.CreateInstance<ItemDef>();
            darkClayItem.name = "DARK_CLAY_NAME";
            darkClayItem.descriptionToken = "DARK_CLAY_DESCRIPTION";
            darkClayItem.nameToken = "DARK_CLAY_NAME";
            darkClayItem.loreToken = "DARK_CLAY_LORE";
            darkClayItem.pickupToken = "DARK_CLAY_PICKUP";
            darkClayItem.pickupIconSprite = darkClaySprite;
            darkClayItem.pickupModelPrefab = darkClayPickup;
            darkClayItem.canRemove = true;
            darkClayItem.hidden = false;
            darkClayItem._itemTierDef = darkTier;
            var displayRules = new ItemDisplayRuleDict(null);
            darkClayItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkClayItem, displayRules));
            LanguageAPI.Add("DARK_CLAY_NAME", "Polished Urn");
            LanguageAPI.Add("DARK_CLAY_DESCRIPTION", "The nearest 1 (+1 per stack) enemies to you within 13m (+8m per stack) will be 'tethered' to you, applying tar. Deal 15% (+15% per stack) additional damage to enemies with tar applied, and heal for 5% (+5% per stack) of the damage dealt. Upon killing a dark enemy, gain 3% (+3% per stack) healing multiplier.");
            LanguageAPI.Add("DARK_CLAY_PICKUP",
                "Tether yourself to nearby enemies, dealing bonus damage and healing for a portion of the damage dealt. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkClayItem);
        }
    }
    
    public class DarkConstructItem
    {
        public static ItemDef darkConstructItem;

        private Sprite darkConstructSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/MinorConstructOnKill/texMinorConstructOnKillIcon.png").WaitForCompletion();

        private GameObject darkConstructPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/DLC1/MinorConstructOnKill/PickupDefenseNucleus.prefab")
            .WaitForCompletion();
        

        public DarkConstructItem()
        {
            darkConstructItem = ScriptableObject.CreateInstance<ItemDef>();
            darkConstructItem.name = "DARK_CONSTRUCT_NAME";
            darkConstructItem.descriptionToken = "DARK_CONSTRUCT_DESCRIPTION";
            darkConstructItem.nameToken = "DARK_CONSTRUCT_NAME";
            darkConstructItem.loreToken = "DARK_CONSTRUCT_LORE";
            darkConstructItem.pickupToken = "DARK_CONSTRUCT_PICKUP";
            darkConstructItem.pickupIconSprite = darkConstructSprite;
            darkConstructItem.pickupModelPrefab = darkConstructPickup;
            darkConstructItem.canRemove = true;
            darkConstructItem.hidden = false;
            darkConstructItem._itemTierDef = darkTier;
            var displayRules = new ItemDisplayRuleDict(null);
            darkConstructItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkConstructItem, displayRules));
            LanguageAPI.Add("DARK_CONSTRUCT_NAME", "Defense Cell");
            LanguageAPI.Add("DARK_CONSTRUCT_DESCRIPTION", "Killing an elite enemy spawns an Alpha Construct that attaches to you with 1000% (+1000% per stack) health. On hit, all Constructs attached to you have a 5% chance to fire at the enemy hit for 300% (+300% per stack) damage. Limit of 4 (+4 per stack) constructs. Upon killing a dark enemy, gain .03 luck.");
            LanguageAPI.Add("DARK_CONSTRUCT_PICKUP",
                "Upon killing an elite, gain an alpha construct that attaches to you and fires at enemies you fire at. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkConstructItem);
        }
    }
    
    public class DarkCoreItem
    {
        public static ItemDef darkCoreItem;

        private Sprite darkCoreSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/RoboBallBuddy/texEmpathyChip.png").WaitForCompletion();

        private GameObject darkCorePickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/RoboBallBuddy/PickupEmpathyChip.prefab")
            .WaitForCompletion();
        

        public DarkCoreItem()
        {
            darkCoreItem = ScriptableObject.CreateInstance<ItemDef>();
            darkCoreItem.name = "DARK_CORE_NAME";
            darkCoreItem.descriptionToken = "DARK_CORE_DESCRIPTION";
            darkCoreItem.nameToken = "DARK_CORE_NAME";
            darkCoreItem.loreToken = "DARK_CORE_LORE";
            darkCoreItem.pickupToken = "DARK_CORE_PICKUP";
            darkCoreItem.pickupIconSprite = darkCoreSprite;
            darkCoreItem.pickupModelPrefab = darkCorePickup;
            darkCoreItem.canRemove = true;
            darkCoreItem.hidden = false;
            darkCoreItem._itemTierDef = darkTier;
            var displayRules = new ItemDisplayRuleDict(null);
            darkCoreItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkCoreItem, displayRules));
            LanguageAPI.Add("DARK_CORE_NAME", "Sympathy Cores");
            LanguageAPI.Add("DARK_CORE_DESCRIPTION", "Every 10 seconds, summon two Solus Probes. All allies gain +100% (+100% per stack) health and damage per ally on your team. Upon killing a dark enemy, increase all of your allies stats by 2% (+2% per stack).");
            LanguageAPI.Add("DARK_CORE_PICKUP",
                "Summon probes. All allies gain stats per ally on your team. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkCoreItem);
        }
    }
    
    public class DarkParentItem
    {
        public static ItemDef darkParentItem;

        private Sprite darkParentSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ParentEgg/texParentEggIcon.png").WaitForCompletion();

        private GameObject darkParentPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/ParentEgg/PickupParentEgg.prefab")
            .WaitForCompletion();
        

        public DarkParentItem()
        {
            darkParentItem = ScriptableObject.CreateInstance<ItemDef>();
            darkParentItem.name = "DARK_PARENT_NAME";
            darkParentItem.descriptionToken = "DARK_PARENT_DESCRIPTION";
            darkParentItem.nameToken = "DARK_PARENT_NAME";
            darkParentItem.loreToken = "DARK_PARENT_LORE";
            darkParentItem.pickupToken = "DARK_PARENT_PICKUP";
            darkParentItem.pickupIconSprite = darkParentSprite;
            darkParentItem.pickupModelPrefab = darkParentPickup;
            darkParentItem.canRemove = true;
            darkParentItem.hidden = false;
            darkParentItem._itemTierDef = darkTier;
            var displayRules = new ItemDisplayRuleDict(null);
            darkParentItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkParentItem, displayRules));
            LanguageAPI.Add("DARK_PARENT_NAME", "Dark Planula");
            LanguageAPI.Add("DARK_PARENT_DESCRIPTION", "Heal from incoming damage equal to 100% (+100% per stack) armor. On taking damage, ignite enemies within a 13m (+8m per stack) radius. Upon killing a dark enemy, gain 1.5 (+1.5 per stack) armor.");
            LanguageAPI.Add("DARK_PARENT_PICKUP",
                "Heal from incoming damage and ignite nearby enemies. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkParentItem);
        }
    }
}