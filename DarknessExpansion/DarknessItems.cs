using System;
using System.Collections.Generic;
using System.Linq;
using EntityStates.TitanMonster;
using EntityStates.VagrantNovaItem;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Items;
using RoR2.Navigation;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using BaseVagrantNovaItemState = On.EntityStates.VagrantNovaItem.BaseVagrantNovaItemState;
using HealthComponent = On.RoR2.HealthComponent;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using ReadyState = On.EntityStates.VagrantNovaItem.ReadyState;
using RechargeState = On.EntityStates.VagrantNovaItem.RechargeState;


namespace DarknessExpansion;

public class DarknessItems
{
    public static ItemTierDef darkTier;
    public static ItemDef testItem;
    public static List<ItemDef> darkItems = new();
    public static Action<CharacterBody> onKillDarknessEnemy;
    public static ItemDef stackingDarkItem;

    public static int darknessGained;
    public static bool logStacking;
    public static bool sqrtStacking;
    
    public DarknessItems()
    {
        ColorCatalog.ColorIndex ci = ColorsAPI.RegisterColor(Color.black);
        darkTier = ScriptableObject.CreateInstance<ItemTierDef>();
        darkTier.tier = ItemTier.AssignedAtRuntime;
        darkTier.darkColorIndex = ci;
        darkTier.colorIndex = ci;
        darkTier.highlightPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/UI/HighlightMisc.prefab")
            .WaitForCompletion().InstantiateClone("Dark Item Highlight");
        CreateDropletPrefab();
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
        new DarkParentItem();       
        new DarkLightningItem();
        new DarkFireItem();
        new DarkStacksItem();
        new DarkCoreItem();
        Inventory.onServerItemGiven += InventoryOnonServerItemGiven;
        onKillDarknessEnemy += body => body.inventory.GiveItem(stackingDarkItem);
        On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMasterOnOnInventoryChanged;
        HealthComponent.Heal += HealthComponentOnHeal;
        RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPIOnGetStatCoefficients;
        On.RoR2.Util.CheckRoll_float_float_CharacterMaster += CalculateDecimalLuck;
        On.RoR2.Items.BaseItemBodyBehavior.Init += BaseItemBodyBehaviorOnInit;
        darknessGained = DarknessExpansion.darknessGainedFromItem.Value;
        logStacking = DarknessExpansion.logStacking.Value;
        sqrtStacking = DarknessExpansion.sqrtStacking.Value;
    }
    private bool CalculateDecimalLuck(On.RoR2.Util.orig_CheckRoll_float_float_CharacterMaster orig, float percentChance, float luck, CharacterMaster effectOriginMaster)
    {
        if (percentChance <= 0.0f)
        {
            return false;
        }
        float newChance = 1 - Mathf.Pow(1 - (percentChance/100f), luck+1);
        if (luck < 0.0f)
        {
            newChance = Mathf.Pow(percentChance / 100f,-luck+1);
        }
        bool rolled = Random.value < newChance;
        if (!rolled)
        {
            return false;
        }
        if (luck > 0) if (effectOriginMaster)
        {
            GameObject bodyObject = effectOriginMaster.GetBodyObject();
            if (bodyObject)
            {
                CharacterBody component = bodyObject.GetComponent<CharacterBody>();
                if (component)
                {
                    component.wasLucky = true;
                }
            }
        }
        return true;
    }



    #region allItems
    private void BaseItemBodyBehaviorOnInit(On.RoR2.Items.BaseItemBodyBehavior.orig_Init orig)
    {
        orig();
        List<BaseItemBodyBehavior.ItemTypePair> itemTypePairs = BaseItemBodyBehavior.server.itemTypePairs.ToList();
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkBeetleItem.DarkBeetleBodyBehavior),
            itemIndex = DarkBeetleItem.darkBeetleItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkCoreItem.DarkCoreBodyBehavior),
            itemIndex = DarkCoreItem.darkCoreItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkJellyfishItem.DarkJellyfishItemBehavior),
            itemIndex = DarkJellyfishItem.darkJellyfishItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkWispItem.DarkWispItemBehavior),
            itemIndex = DarkWispItem.darkWispItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkClayItem.DarkClayItemBehavior),
            itemIndex = DarkClayItem.darkClayItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkConstructItem.DarkConstructItemBehavior),
            itemIndex = DarkConstructItem.darkConstructItem.itemIndex
        });
        BaseItemBodyBehavior.server.SetItemTypePairs(itemTypePairs);
        itemTypePairs = BaseItemBodyBehavior.shared.itemTypePairs.ToList();
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkBeetleItem.DarkBeetleBodyBehavior),
            itemIndex = DarkBeetleItem.darkBeetleItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkJellyfishItem.DarkJellyfishItemBehavior),
            itemIndex = DarkJellyfishItem.darkJellyfishItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkCoreItem.DarkCoreBodyBehavior),
            itemIndex = DarkCoreItem.darkCoreItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkWispItem.DarkWispItemBehavior),
            itemIndex = DarkWispItem.darkWispItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkClayItem.DarkClayItemBehavior),
            itemIndex = DarkClayItem.darkClayItem.itemIndex
        });
        itemTypePairs.Add(new BaseItemBodyBehavior.ItemTypePair()
        {
            behaviorType = typeof(DarkConstructItem.DarkConstructItemBehavior),
            itemIndex = DarkConstructItem.darkConstructItem.itemIndex
        });
        BaseItemBodyBehavior.shared.SetItemTypePairs(itemTypePairs);
        itemTypePairs = BaseItemBodyBehavior.client.itemTypePairs.ToList();
        BaseItemBodyBehavior.client.SetItemTypePairs(itemTypePairs);
    }
    private void CreateDropletPrefab()
    { 
        GameObject Temp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/LunarOrb.prefab").WaitForCompletion().InstantiateClone("Darkness Orb", true);
        Gradient gradient = new Gradient();
        var colors = new GradientColorKey[2]; 
        colors[0] = new GradientColorKey(Color.black, 0.0f); 
        colors[1] = new GradientColorKey(Color.black, 1.0f);
        var alphas = new GradientAlphaKey[2]; 
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f); 
        alphas[1] = new GradientAlphaKey(0.0f, 1.0f);
        gradient.SetKeys(colors, alphas); 
        Color c = Color.black; 
        Temp.transform.GetChild(0).gameObject.GetComponent<TrailRenderer>().startColor = Color.black; 
        Temp.transform.GetChild(0).gameObject.GetComponent<TrailRenderer>().set_startColor_Injected(ref c); 
        Temp.transform.GetChild(0).gameObject.GetComponent<TrailRenderer>().SetColorGradient(gradient);
        Light[] lights = Temp.GetComponentsInChildren<Light>(); 
        foreach (Light thisLight in lights) 
        { 
            thisLight.color = c;
        }
        ParticleSystem[] array = Temp.GetComponentsInChildren<ParticleSystem>(); 
        foreach (ParticleSystem obj in array) 
        { 
            ParticleSystem.MainModule main = obj.main; 
            ParticleSystem.ColorOverLifetimeModule COL = obj.colorOverLifetime; 
            main.startColor = new ParticleSystem.MinMaxGradient(c); 
            COL.color = c;
        } 
        darkTier.dropletDisplayPrefab = Temp;
    }

    private void CharacterMasterOnOnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
    {
        if (self) if (self.inventory)
        {
            int numDarknessStacks = self.inventory.GetItemCount(stackingDarkItem);
            if (logStacking && sqrtStacking)
            {
                numDarknessStacks = 0;
            }
            else if (logStacking)
            {
                numDarknessStacks = (int)Mathf.Log(numDarknessStacks + 1,2);
            }
            else if (sqrtStacking)
            {
                numDarknessStacks = (int)Mathf.Sqrt(numDarknessStacks);
            }
            int numDarkConstructItems = self.inventory.GetItemCount(DarkConstructItem.darkConstructItem);
            orig(self);
            self.luck += numDarknessStacks * numDarkConstructItems * .03f;
        }
    }

    private float HealthComponentOnHeal(HealthComponent.orig_Heal orig, RoR2.HealthComponent self, float amount, ProcChainMask procchainmask, bool nonregen)
    {
        if (self) if (self.body) if (self.body.inventory)
        {
            int numDarkClay = self.body.inventory.GetItemCount(DarkClayItem.darkClayItem); 
            int numDarknessStacks = self.body.inventory.GetItemCount(stackingDarkItem);
            if (logStacking && sqrtStacking)
            {
                numDarknessStacks = 0;
            }
            else if (logStacking)
            {
                numDarknessStacks = (int)Mathf.Log(numDarknessStacks + 1,2);
            }
            else if (sqrtStacking)
            {
                numDarknessStacks = (int)Mathf.Sqrt(numDarknessStacks);
            }
            float healMultiplier = 1 + (numDarknessStacks * numDarkClay * .03f);
            return orig(self, amount * healMultiplier, procchainmask, nonregen);
        }
        return orig(self, amount, procchainmask, nonregen);
    }
    

    private void GlobalEventManagerOnonCharacterDeathGlobal(DamageReport obj)
    {
        if (!obj.attacker || !obj.attackerBody)
        {
            return;
        }

        if (obj.attackerBody.inventory)
        {
            if (obj.victimBody && obj.victimBody.inventory) if (obj.victimBody.inventory.GetEquipmentIndex() ==
                Darkness.DarknessEquipment.equipmentIndex)
            {
                onKillDarknessEnemy.Invoke(obj.attackerBody);
            }
        }
    }

    private void InventoryOnonServerItemGiven(Inventory arg1, ItemIndex arg2, int arg3)
    {
        if (arg1) if (arg1.currentEquipmentIndex != Darkness.DarknessEquipment.equipmentIndex)
        {
            if (darkItems.Contains(ItemCatalog.GetItemDef(arg2)))
            {
                Darkness.DarknessLevel += arg3 * darknessGained;
                Darkness.UpdateDarkness();
            }
        }
    }
    private void RecalculateStatsAPIOnGetStatCoefficients(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (!self)
            return;
        if (self.inventory)
        {
            int numDarknessStacks = self.inventory.GetItemCount(stackingDarkItem);
            if (logStacking && sqrtStacking)
            {
                numDarknessStacks = 0;
            }
            else if (logStacking)
            {
                numDarknessStacks = (int)Mathf.Log(numDarknessStacks + 1,2);
            }
            else if (sqrtStacking)
            {
                numDarknessStacks = (int)Mathf.Sqrt(numDarknessStacks);
            }
            float numDarkGolems = self.inventory.GetItemCount(DarkGolemItem.darkGolemItem);
            numDarkGolems = 1 + (numDarkGolems - 1) * DarkGolemItem.stacking;
            int numDarkBeetles = self.inventory.GetItemCount(DarkBeetleItem.darkBeetleItem);
            float numDarkPearls = self.inventory.GetItemCount(DarkPearlItem.darkPearlItem);
            numDarkPearls = 1 + (numDarkPearls - 1) * DarkPearlItem.stackingMultiplier;
            float numDarkBetterPearls = self.inventory.GetItemCount(DarkPearlItem2.darkPearlItem);
            numDarkBetterPearls = 1 + (numDarkBetterPearls - 1) * DarkPearlItem2.stackingMultiplier;
            int numDarkJellyfish = self.inventory.GetItemCount(DarkJellyfishItem.darkJellyfishItem);
            int numDarkWisps = self.inventory.GetItemCount(DarkWispItem.darkWispItem);
            float numDarkBleedItems = self.inventory.GetItemCount(DarkBleedItem.darkBleedItem);
            numDarkBleedItems = 1 + (numDarkBleedItems - 1) * DarkBleedItem.stackingMultiplier;
            int numDarkCoreStacks = self.inventory.GetItemCount(DarkCoreItem.DarkStacksItem.stackingDarkItem);
            int numDarkParentItems = self.inventory.GetItemCount(DarkParentItem.darkParentItem);
            int numDarkLightningItems = self.inventory.GetItemCount(DarkLightningItem.darkLightningItem);
            int numDarkFires = self.inventory.GetItemCount(DarkFireItem.darkFireItem);
            if (numDarkBleedItems > 0)
            {
                args.critAdd += DarkBleedItem.critChancePercent;
            }
            args.baseDamageAdd += numDarkFires * numDarknessStacks * 0.5f;
            args.baseHealthAdd += (numDarkGolems * DarkGolemItem.baseHealth) + (numDarkGolems * numDarknessStacks * DarkGolemItem.stackingHealth);
            args.baseRegenAdd += (numDarkGolems * DarkGolemItem.baseRegen) + (numDarkGolems * numDarknessStacks * DarkGolemItem.stackingRegen);
            args.critDamageMultAdd += (numDarkBleedItems * numDarknessStacks * DarkBleedItem.onKillCritDmgPercent / 100f);
            args.armorAdd += (numDarkParentItems * numDarknessStacks * 1.5f);
            args.attackSpeedMultAdd += (numDarkBeetles * numDarknessStacks * .03f);
            args.moveSpeedMultAdd += (numDarkWisps * numDarknessStacks * .03f);
            args.healthMultAdd += (1 + numDarkPearls * DarkPearlItem.baseHealthPercent/100f) * (1 + numDarkPearls * numDarknessStacks * DarkPearlItem.onKillHealthPercent/100f)-1;
            args.damageMultAdd += (numDarknessStacks * .04f * numDarkLightningItems);
            float darkBetterPearlMultiplier = 1 + (numDarkBetterPearls * DarkPearlItem2.allStatsPercent/100f);
            darkBetterPearlMultiplier *= 1 + (numDarkBetterPearls * numDarknessStacks * DarkPearlItem2.onKillPercent/100f);
            darkBetterPearlMultiplier -= 1;
            darkBetterPearlMultiplier += .02f * numDarkCoreStacks;
            args.healthMultAdd += darkBetterPearlMultiplier;
            args.regenMultAdd += darkBetterPearlMultiplier;
            args.moveSpeedMultAdd += darkBetterPearlMultiplier;
            args.damageMultAdd += darkBetterPearlMultiplier;
            args.critAdd += darkBetterPearlMultiplier * (self.crit+args.critAdd);
            args.attackSpeedMultAdd += darkBetterPearlMultiplier;
            args.armorAdd += darkBetterPearlMultiplier * (self.armor+args.armorAdd);
            //check if this actually works
            float cooldownMult = Mathf.Pow(1-(.01f*numDarkJellyfish),numDarknessStacks);
            args.cooldownMultAdd =cooldownMult - 1;
        }
    }

    #endregion

    #region completedItems
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

        public static  int   baseHealth;
        public static  int   baseRegen;
        public static  float procChance;
        public static  int   fistBaseDamage;
        public static  int   fistDamagePerHealth;
        public static  int   stackingHealth;
        public static  int   stackingRegen;
        public static  float stacking;
        public DarkGolemItem()
        {
            baseHealth           = DarknessExpansion.golemHealth.Value;
            baseRegen            = DarknessExpansion.golemRegen.Value;
            procChance           = DarknessExpansion.golemChance.Value;
            fistBaseDamage       = DarknessExpansion.golemBaseDamage.Value;
            fistDamagePerHealth  = DarknessExpansion.golemDamagePerHealth.Value;
            stackingHealth       = DarknessExpansion.golemStackingHealth.Value;
            stackingRegen        = DarknessExpansion.golemStackingRegen.Value;
            stacking   = DarknessExpansion.golemStacking.Value;
            
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
                $"Gives {baseHealth} (+{baseHealth*stacking} per stack) health and {baseRegen} (+{baseRegen*stacking} per stack) regen. Upon taking damage, {procChance}% chance to summon a fist for {fistBaseDamage}% (+{fistBaseDamage*stacking}% per stack) damage + {fistDamagePerHealth}% damage (+{fistDamagePerHealth*stacking}% per stack) per 500 health. Gives {stackingHealth} (+{stackingHealth*stacking} per stack) health and {stackingRegen} (+{stackingRegen*stacking} per stack) regen upon killing a dark enemy.");
            LanguageAPI.Add("DARK_GOLEM_PICKUP",
                "Increases health and regen. Upon taking damage, chance to summon a fist. Fist damage scales with health. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkGolemItem);
        }

        
        private void HealthComponentOnTakeDamageProcess(HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, DamageInfo damageinfo)
        {
            if (self) if (self.body) if (self.body.inventory) if (damageinfo.attacker)
            {
                float numDarkGolems = self.body.inventory.GetItemCount(darkGolemItem);
                numDarkGolems = 1 + (numDarkGolems - 1) * stacking;
                if (numDarkGolems > 0)
                {
                    if (Util.CheckRoll(procChance, self.body.master))
                    {
                        bool isCrit = self.body.RollCrit();
                        float damageValue = self.body.damage * self.body.healthComponent.fullCombinedHealth * numDarkGolems * fistDamagePerHealth / 50000f;
                        if (self.body.teamComponent.teamIndex != TeamIndex.Player)
                        {
                            damageValue = self.body.damage * self.body.healthComponent.fullCombinedHealth  * numDarkGolems * fistDamagePerHealth / 500000f;
                        }

                        damageValue += fistBaseDamage / 100f;
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
    
    public class DarkPearlItem
    {
        public static ItemDef darkPearlItem;

        private Sprite darkPearlSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Pearl/texPearlIcon.png").WaitForCompletion();

        private GameObject darkPearlPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/Pearl/PickupPearl.prefab")
            .WaitForCompletion();
        
        public static float baseHealthPercent;
        public static float onKillHealthPercent;
        public static float stackingMultiplier;
        public DarkPearlItem()
        {
            baseHealthPercent    = DarknessExpansion.pearlHealthPercent.Value;
            onKillHealthPercent  = DarknessExpansion.pearlOnKillPercent.Value; 
            stackingMultiplier   = DarknessExpansion.pearlStacking.Value;

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
                $"Increases maximum health by {baseHealthPercent}% " +
                $"(+{baseHealthPercent * stackingMultiplier}% per stack). " +
                $"Upon killing a dark enemy, increases health by {onKillHealthPercent}% " +
                $"(+{onKillHealthPercent * stackingMultiplier}% per stack)."
            );
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
        public static float allStatsPercent;
        public static float onKillPercent;
        public static float stackingMultiplier;

        public DarkPearlItem2()
        {
            allStatsPercent      = DarknessExpansion.pearl2AllStatsPercent.Value;
            onKillPercent        = DarknessExpansion.pearl2OnKillPercent.Value;
            stackingMultiplier   = DarknessExpansion.pearl2Stacking.Value; 
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
                $"Increases all stats by {allStatsPercent}% " +
                $"(+{allStatsPercent * stackingMultiplier}% per stack). " +
                $"Upon killing a dark enemy, increases all stats by {onKillPercent}% " +
                $"(+{onKillPercent * stackingMultiplier}% per stack)."
            );
            LanguageAPI.Add("DARK_PEARL_PICKUP2",
                "Increases all stats. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkPearlItem);
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
        
        public static float critChancePercent;
        public static int   bleedStacksPerHit;
        public static float explosionBasePercent;
        public static float explosionHealthPercent;
        public static float onKillCritDmgPercent;
        public static float stackingMultiplier;
        public DarkBleedItem()
        {
            critChancePercent       = DarknessExpansion.bleedCritChancePercent.Value;
            bleedStacksPerHit        = DarknessExpansion.bleedStacksPerHit.Value;
            explosionBasePercent    = DarknessExpansion.explosionBaseDamagePercent.Value;
            explosionHealthPercent  = DarknessExpansion.explosionHealthPercent.Value;
            onKillCritDmgPercent    = DarknessExpansion.onKillCritDamagePercent.Value;
            stackingMultiplier      = DarknessExpansion.bleedStackingMultiplier.Value;

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
                $"Gain {critChancePercent}% critical chance. " +
                $"All hits apply {bleedStacksPerHit} bleed stack(s). " +
                $"Crits apply bonus bleed. " +
                $"Bleeding enemies explode for {explosionBasePercent}% (+{explosionBasePercent * stackingMultiplier}% per stack) " +
                $"damage per bleed stack + {explosionHealthPercent}% (+{explosionHealthPercent * stackingMultiplier}% per stack) max health. " +
                $"Upon killing a dark enemy, gain {onKillCritDmgPercent}% (+{onKillCritDmgPercent * stackingMultiplier}% per stack) crit damage."
            );LanguageAPI.Add("DARK_BLEED_PICKUP",
                "All hits apply bleed, and crits apply extra. Bleeding enemies explode, dealing damage to nearby enemies. Grows stronger as it absorbs darkness.");
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManagerOnProcessHitEnemy;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManagerOnonCharacterDeathGlobal;
            darkItems.Add(darkBleedItem);
        }

        private void GlobalEventManagerOnonCharacterDeathGlobal(DamageReport obj)
        {
            if (!obj.attacker || !obj.attackerBody)
            {
                return;
            }

            if (obj.attackerBody.inventory && obj.victimBody)
            {
                float numDarkBleedItems = obj.attackerBody.inventory.GetItemCount(darkBleedItem);
                numDarkBleedItems = 1 + (numDarkBleedItems - 1) * stackingMultiplier;
                if (numDarkBleedItems > 0 && obj.victimBody.HasBuff(RoR2Content.Buffs.Bleeding) || obj.victimBody.HasBuff(RoR2Content.Buffs.SuperBleed))
                {
                    Util.PlaySound("Play_bleedOnCritAndExplode_explode", obj.victimBody.gameObject);
                    Vector3 position = obj.victimBody.transform.position;
                    float damageCoefficient = explosionBasePercent * numDarkBleedItems * (obj.victimBody.GetBuffCount(RoR2Content.Buffs.Bleeding) + 3) / 100f;
                    float num = explosionHealthPercent * numDarkBleedItems / 100f;
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
                            float numDarkBleedItems = inventory.GetItemCount(darkBleedItem);
                            numDarkBleedItems = 1 + (numDarkBleedItems - 1) * stackingMultiplier;
                            if (numDarkBleedItems > 0)
                            {
                                ProcChainMask procChainMask2 = damageinfo.procChainMask;
                                procChainMask2.AddProc(ProcType.BleedOnHit);
                                int numBleeds = bleedStacksPerHit;
                                if (damageinfo.crit)
                                {
                                    numBleeds *= (int)component2.critMultiplier;
                                }
                                for (int i = 0; i < numBleeds; i++)
                                {
                                    DotController.InflictDot(victim, damageinfo.attacker, DotController.DotIndex.Bleed, 3f * damageinfo.procCoefficient);
                                }
                            }
                        }
                    }
                }
            }
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

        private static BuffDef debuffApplier;
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

            debuffApplier = ScriptableObject.CreateInstance<BuffDef>();
            debuffApplier.isHidden = true;
            debuffApplier.canStack = true;
            debuffApplier.isCooldown = false;
            ContentAddition.AddBuffDef(debuffApplier);
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManagerOnProcessHitEnemy;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += CharacterMasterOnGetDeployableSameSlotLimit;
            MasterSummon.onServerMasterSummonGlobal += MasterSummonOnonServerMasterSummonGlobal;
        }

        private void MasterSummonOnonServerMasterSummonGlobal(MasterSummon.MasterSummonReport obj)
        {
            if (obj.leaderMasterInstance) if (obj.leaderMasterInstance.inventory) if (obj.leaderMasterInstance.inventory.GetItemCount(darkBeetleItem) > 0)
            {
                float leaderAttackSpeed = obj.leaderMasterInstance.GetBody().attackSpeed;
                int numSyringes = (int)((leaderAttackSpeed - 1) / .15f);
                obj.summonMasterInstance.inventory.GiveItemString("Syringe",numSyringes);
            }
        }

        private int CharacterMasterOnGetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            if (slot == DeployableSlot.BeetleGuardAlly)
            {
                int num = 1;
                if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.swarmsArtifactDef))
                {
                    num = 2;
                }

                return num * (self.inventory.GetItemCount(RoR2Content.Items.BeetleGland) + self.inventory.GetItemCount(darkBeetleItem));
            }

            return orig(self, slot);
        }
        private void GlobalEventManagerOnProcessHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageinfo, GameObject victim)
        {
            if (damageinfo.attacker)
            {
                CharacterBody cb = damageinfo.attacker.GetComponent<CharacterBody>();
                if (cb) if (cb.HasBuff(debuffApplier))
                {
                    int numBuffStacks = cb.GetBuffCount(debuffApplier);
                    for (int i = 0; i < numBuffStacks; i++)
                    {
                        CharacterBody cb2 = victim.GetComponent<CharacterBody>();
                        BuffDef bd = BuffCatalog.buffDefs[(int)(BuffCatalog.buffDefs.Length * Random.value)];
                        while (!bd.isDebuff)
                        {
                            bd = BuffCatalog.buffDefs[(int)(BuffCatalog.buffDefs.Length * Random.value)];
                        }

                        cb2.AddTimedBuff(bd, 10f);
                    }
                }
            }

            orig(self, damageinfo, victim);
        }

        public class DarkBeetleBodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef()
            {
                return darkBeetleItem;
            }

            private void Start()
            {
                cm = body.master;
                guardResummonCooldown = 0f;
            }

            private void FixedUpdate()
            {
                int deployableCount = cm.GetDeployableCount(DeployableSlot.BeetleGuardAlly);
                if (deployableCount < stack)
                {
                    guardResummonCooldown -= Time.fixedDeltaTime;
                    if (guardResummonCooldown <= 0f)
                    {
                        DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscBeetleGuardAlly"), new DirectorPlacementRule
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                            minDistance = 3f,
                            maxDistance = 40f,
                            spawnOnTarget = transform
                        }, RoR2Application.rng);
                        directorSpawnRequest.summonerBodyObject = gameObject;
                        directorSpawnRequest.onSpawnedServer = OnSpawned;
                        DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                        if (deployableCount < stack)
                        {
                            guardResummonCooldown = 1f;
                            return;
                        }
                        guardResummonCooldown = 30f;
                    }
                }
            }

            private void OnSpawned(SpawnCard.SpawnResult obj)
            {
                if (obj.spawnedInstance)
                {
                    Deployable d = obj.spawnedInstance.GetComponent<Deployable>();
                    cm.AddDeployable(d, DeployableSlot.BeetleGuardAlly);
                    obj.spawnedInstance.GetComponent<CharacterMaster>().GetBody().baseDamage *= stack * 3;
                    obj.spawnedInstance.GetComponent<CharacterMaster>().GetBody().baseMaxHealth *= stack * 3;
                    for (int i = 0; i < stack; i++)
                    {
                        obj.spawnedInstance.GetComponent<CharacterMaster>().GetBody().AddBuff(debuffApplier);
                    }
                }
            }
            
            private CharacterMaster cm;
            private float guardResummonCooldown;
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
        
        public class DarkStacksItem
        {
            public static ItemDef stackingDarkItem;
            private Sprite darkGolemSprite =
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Knurl/texKnurlIcon.png").WaitForCompletion();

            private GameObject darkGolemPickup = Addressables
                .LoadAssetAsync<GameObject>("RoR2/Base/Knurl/PickupKnurl.prefab")
                .WaitForCompletion();

            public DarkStacksItem()
            {
                stackingDarkItem = ScriptableObject.CreateInstance<ItemDef>();
                stackingDarkItem.name = "DARK_STACK_NAME2";
                stackingDarkItem.descriptionToken = "DARK_STACK_DESCRIPTION2";
                stackingDarkItem.nameToken = "DARK_STACK_NAME2";
                stackingDarkItem.loreToken = "DARK_STACK_LORE2";
                stackingDarkItem.pickupToken = "DARK_STACK_PICKUP2";
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
            LanguageAPI.Add("DARK_CORE_DESCRIPTION", "Every 10 seconds, summon two Solus Probes. All summons gain +200% (+200% per stack) damage per ally on your team. Upon killing a dark enemy, increase all of your allies stats by 2% (+2% per stack).");
            LanguageAPI.Add("DARK_CORE_PICKUP",
                "Summon probes. All allies gain stats per ally on your team. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkCoreItem);
            new DarkStacksItem();
            MasterSummon.onServerMasterSummonGlobal+= MasterSummonOnonServerMasterSummonGlobal;

        }

        private void MasterSummonOnonServerMasterSummonGlobal(MasterSummon.MasterSummonReport obj)
        {
            if (obj.leaderMasterInstance) if (obj.leaderMasterInstance.inventory) if (obj.leaderMasterInstance.inventory.GetItemCount(darkCoreItem) > 0)
            {
                if (obj.summonMasterInstance)
                {
                    CharacterMaster component = obj.summonMasterInstance;
                    if (component)
                    {
                        Inventory inventory = obj.leaderMasterInstance.GetBody().inventory;
                        Inventory inventory2 = component.inventory;
                        if (inventory)
                        {
                            InventorySync inventorySync = obj.summonMasterInstance.gameObject.AddComponent<InventorySync>();
                            inventorySync.srcInventory = inventory;
                            inventorySync.destInventory = inventory2;
                        }
                    }
                }
            }
        }

        private class InventorySync : MonoBehaviour
        {
            private void FixedUpdate()
            {
                if (srcInventory && destInventory)
                {
                    int itemCount = srcInventory.GetItemCount(darkCoreItem)*2;
                    int num = itemCount - granted;
                    if (num != 0)
                    {
                        destInventory.GiveItem(RoR2Content.Items.TeamSizeDamageBonus,num);
                        granted = itemCount;
                    }

                    itemCount = srcInventory.GetItemCount(darkCoreItem) * srcInventory.GetItemCount(stackingDarkItem);
                    num = itemCount - granted2;
                    if (num != 0)
                    {
                        destInventory.GiveItem(DarkStacksItem.stackingDarkItem,num);
                        granted2 = itemCount;
                    }
                }
            }

            public Inventory srcInventory;
            public Inventory destInventory;
            private int granted;
            private int granted2;
        }
         public class DarkCoreBodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef()
            {
                return darkCoreItem;
            }

            private void Start()
            {
                cm = body.master;
            }

            private void FixedUpdate()
            {
                if (redBuddySpawner == null && isActiveAndEnabled)
                {
                    CreateSpawners();
                }
            }

            private void CreateSpawners()
            {
                CreateSpawner(ref redBuddySpawner,DeployableSlot.RoboBallRedBuddy,Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/RoboBallBuddy/cscRoboBallRedBuddy.asset").WaitForCompletion());
                CreateSpawner(ref greenBuddySpawner,DeployableSlot.RoboBallGreenBuddy,Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/RoboBallBuddy/cscRoboBallGreenBuddy.asset").WaitForCompletion());
            }

            private void CreateSpawner(ref DeployableMinionSpawner dms, DeployableSlot ds, SpawnCard sc)
            {
                dms = new DeployableMinionSpawner(cm, ds, new Xoroshiro128Plus((ulong)Random.value * ulong.MaxValue))
                {
                    respawnInterval = 30f,
                    spawnCard = sc
                };
            }
            
            private DeployableMinionSpawner redBuddySpawner;
            private DeployableMinionSpawner greenBuddySpawner;
            private CharacterMaster cm;
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

        private static BuffDef chargeBuff;

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
            chargeBuff = ScriptableObject.CreateInstance<BuffDef>();
            chargeBuff.canStack = true;
            chargeBuff.isHidden = true;
            BaseVagrantNovaItemState.GetItemStack += BaseVagrantNovaItemStateOnGetItemStack;
            ReadyState.FixedUpdate += ReadyStateOnFixedUpdate;
            RechargeState.FixedUpdate += RechargeStateOnFixedUpdate;
            ContentAddition.AddBuffDef(chargeBuff);
        }

        private void RechargeStateOnFixedUpdate(RechargeState.orig_FixedUpdate orig, EntityStates.VagrantNovaItem.RechargeState self)
        {
            if (self.attachedBody.inventory.GetItemCount(darkJellyfishItem) > 0)
            {
                self.fixedAge += self.GetDeltaTime();
                if (self.isAuthority)
                {
                    int itemStack = self.GetItemStack();
                    float num = EntityStates.VagrantNovaItem.RechargeState.baseDuration / (itemStack + 1);
                    num *= self.attachedBody.skillLocator.primary.cooldownScale;
                    float num2 = self.fixedAge / num;
                    if (num2 >= 1f)
                    {
                        num2 = 1f;
                        self.outer.SetNextState(new EntityStates.VagrantNovaItem.ReadyState());
                    }
                    self.SetChargeSparkEmissionRateMultiplier(EntityStates.VagrantNovaItem.RechargeState.particleEmissionCurve.Evaluate(num2));
                }
            }
            else
            {
                orig(self);
            }
        }

        private void ReadyStateOnFixedUpdate(ReadyState.orig_FixedUpdate orig, EntityStates.VagrantNovaItem.ReadyState self)
        {
            if (self.attachedBody.inventory.GetItemCount(darkJellyfishItem) > 0)
            {
                if (self.isAuthority && (self.attachedHealthComponent.health + self.attachedHealthComponent.shield) /
                    self.attachedHealthComponent.fullCombinedHealth <= 0.5f)
                {
                    self.outer.SetNextState(new ChargeState());
                }
            }
            else
            {
                orig(self);
            }
        }
        

        private int BaseVagrantNovaItemStateOnGetItemStack(BaseVagrantNovaItemState.orig_GetItemStack orig, EntityStates.VagrantNovaItem.BaseVagrantNovaItemState self)
        {
            if (!self.attachedBody || !self.attachedBody.inventory)
            {
                return 1;
            }
            return self.attachedBody.inventory.GetItemCount(RoR2Content.Items.NovaOnLowHealth) + self.attachedBody.inventory.GetItemCount(darkJellyfishItem);
        }

        public class DarkJellyfishItemBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef()
            {
                return darkJellyfishItem;
            }
            
            private void Start()
            {
                attachment = Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BodyAttachments/VagrantNovaItemBodyAttachment")).GetComponent<NetworkedBodyAttachment>();
                attachment.AttachToGameObjectAndSpawn(body.gameObject);
                projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantCannon.prefab").WaitForCompletion();
                if (body)
                {
                    body.onSkillActivatedServer += OnSkillActivated;
                    skillLocator = body.GetComponent<SkillLocator>();
                    inputBank = body.GetComponent<InputBankTest>();
                }
            }
            private void FixedUpdate()
            {
                if (!body.healthComponent.alive)
                {
                    Destroy(this);
                }
                int num = stack * 3;
                if (body.GetBuffCount(chargeBuff) < num)
                {
                    float num2 = 10f * skillLocator.primary.cooldownScale / num;
                    reloadTimer += Time.fixedDeltaTime;
                    while (reloadTimer > num2 && body.GetBuffCount(chargeBuff) < num)
                    {
                        body.AddBuff(chargeBuff);
                        reloadTimer -= num2;
                    }
                }
            }

            private void OnSkillActivated(GenericSkill skill)
            {
                if ((skillLocator  ?   skillLocator.secondary : null) == skill && body.GetBuffCount(chargeBuff) > 0)
                {
                    if (NetworkServer.active)
                    {
                        body.RemoveBuff(chargeBuff);
                    }
                    FireLightning();
                }
            }
    
            private void FireLightning()
            {
                Log.Debug("Firing");
                Ray aimRay = GetAimRay();
                ProjectileManager.instance.FireProjectileWithoutDamageType(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction) * GetRandomRollPitch(), gameObject, body.damage * (5f * stack), 0f, Util.CheckRoll(body.crit, body.master), DamageColorIndex.Item);
            }
            Quaternion GetRandomRollPitch()
            {
                Quaternion lhs = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
                Quaternion rhs = Quaternion.AngleAxis(0f + Random.Range(0f, 1f), Vector3.left);
                return lhs * rhs;
            }

            private Ray GetAimRay()
            {
                if (inputBank)
                {
                    return new Ray(inputBank.aimOrigin, inputBank.aimDirection);
                }
                return new Ray(transform.position, transform.forward);
            }
            private void OnDestroy()
            {
                if (attachment)
                {
                    Destroy(attachment.gameObject);
                    attachment = null;
                }

                if (body)
                {
                    body.onSkillActivatedServer -= OnSkillActivated;
                    while (body.HasBuff(chargeBuff))
                    {
                        body.RemoveBuff(chargeBuff);
                    }
                }
            }

            // Token: 0x04005527 RID: 21799
            private NetworkedBodyAttachment attachment;
            private GameObject projectilePrefab;
            private SkillLocator skillLocator;
            private InputBankTest inputBank;
            private float reloadTimer = 0f;
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
        public class DarkWispItemBehavior : BaseItemBodyBehavior{

            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef()
            {
                return darkWispItem;
            }

            private void FixedUpdate()
            {
                if (body.isSprinting)
                {
                    fireTimer -= Time.fixedDeltaTime;
                    if (fireTimer <= 0f && body.moveSpeed > 0f)
                    {
                        fireTimer += 1f / (0.08571429f * body.moveSpeed);
                        Fire();
                    }
                }
            }

        
            private void Fire()
            {
                for (int i = 0; i < 3 * stack; i++)
                {
                    DevilOrb devilOrb = new DevilOrb
                    {
                        origin = body.corePosition,
                        damageValue = body.damage * 3f * stack,
                        teamIndex = body.teamComponent.teamIndex,
                        attacker = gameObject,
                        damageColorIndex = DamageColorIndex.Item,
                        scale = 3f,
                        effectType = DevilOrb.EffectType.Wisp,
                        procCoefficient = 3f * stack
                    };
                    
                    if (devilOrb.target = devilOrb.PickNextTarget(devilOrb.origin, 40f))
                    {
                        devilOrb.isCrit = Util.CheckRoll(body.crit, body.master);
                        OrbManager.instance.AddOrb(devilOrb);
                    }
                }
            }
        
            private float fireTimer;
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
            LanguageAPI.Add("DARK_CLAY_DESCRIPTION", "The nearest 1 (+1 per stack) enemies to you within 13m will be 'tethered' to you, applying tar. Deal 15% (+15% per stack) additional damage to enemies with tar applied, and heal for 5% (+5% per stack) of the damage dealt. Upon killing a dark enemy, gain 3% (+3% per stack) healing multiplier.");
            LanguageAPI.Add("DARK_CLAY_PICKUP",
                "Tether yourself to nearby enemies, dealing bonus damage and healing for a portion of the damage dealt. Grows stronger as it absorbs darkness.");
            HealthComponent.TakeDamageProcess += HealthComponentOnTakeDamageProcess;
            darkItems.Add(darkClayItem);
        }

        private void HealthComponentOnTakeDamageProcess(HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, DamageInfo damageinfo)
        {
            if (self.body) if (self.body.inventory) if (damageinfo.attacker)
            {
                CharacterBody cb = damageinfo.attacker.GetComponent<CharacterBody>();
                if (cb)
                {
                    int num = cb.inventory.GetItemCount(darkClayItem);
                    if (num > 0)
                    {
                        if (self.body.HasBuff(RoR2Content.Buffs.ClayGoo))
                        {
                            damageinfo.damage *= 1 + (num * 0.15f);
                            cb.healthComponent.Heal(damageinfo.damage * 0.05f * num, default, false);
                        }
                    }
                }
            }

            orig(self, damageinfo);
        }

        public class DarkClayItemBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef()
            {
                return darkClayItem;
            }

            private void OnEnable()
            {
                attachment = Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BodyAttachments/SiphonNearbyBodyAttachment")).GetComponent<NetworkedBodyAttachment>();
                attachment.AttachToGameObjectAndSpawn(body.gameObject);
                siphonNearbyController = attachment.GetComponent<SiphonNearbyController>();
            }

            private void OnDisable()
            {
                DestroyAttachment();
            }
            
            private void FixedUpdate()
            {
                siphonNearbyController.NetworkmaxTargets = (body.healthComponent.alive ? stack : 0);
            }

            private void DestroyAttachment()
            {
                if (attachment)
                {
                    Destroy(attachment.gameObject);
                }
                attachment = null;
                siphonNearbyController = null;
            }

            private NetworkedBodyAttachment attachment;

            private SiphonNearbyController siphonNearbyController;
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
            HealthComponent.TakeDamageProcess += HealthComponentOnTakeDamageProcess;
        }

        private void HealthComponentOnTakeDamageProcess(HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, DamageInfo damageinfo)
        {
            if (self) if (self.body)
                if (self.body.inventory)
                {
                    int numItems = self.body.inventory.GetItemCount(darkParentItem);
                    if (numItems > 0)
                    {
                        self.Heal(self.body.armor * numItems, default, false);
                        float radius = 5f + 8f * numItems;
                        Vector3 corePosition = self.body.corePosition;
                        GlobalEventManager.igniteOnKillSphereSearch.origin = corePosition;
                        GlobalEventManager.igniteOnKillSphereSearch.mask = LayerIndex.entityPrecise.mask;
                        GlobalEventManager.igniteOnKillSphereSearch.radius = radius;
                        GlobalEventManager.igniteOnKillSphereSearch.RefreshCandidates();
                        GlobalEventManager.igniteOnKillSphereSearch.FilterCandidatesByHurtBoxTeam(
                            TeamMask.GetUnprotectedTeams(self.body.teamComponent.teamIndex));
                        GlobalEventManager.igniteOnKillSphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                        GlobalEventManager.igniteOnKillSphereSearch.OrderCandidatesByDistance();
                        GlobalEventManager.igniteOnKillSphereSearch.GetHurtBoxes(GlobalEventManager
                            .igniteOnKillHurtBoxBuffer);
                        GlobalEventManager.igniteOnKillSphereSearch.ClearCandidates();
                        float value = numItems * 0.1f * self.body.armor * self.body.damage;
                        for (int i = 0; i < GlobalEventManager.igniteOnKillHurtBoxBuffer.Count; i++)
                        {
                            HurtBox hurtBox = GlobalEventManager.igniteOnKillHurtBoxBuffer[i];
                            if (hurtBox.healthComponent)
                            {
                                InflictDotInfo inflictDotInfo = new InflictDotInfo
                                {
                                    victimObject = hurtBox.healthComponent.gameObject,
                                    attackerObject = self.gameObject,
                                    totalDamage = value,
                                    dotIndex = DotController.DotIndex.Burn,
                                    damageMultiplier = 1f
                                };
                                StrengthenBurnUtils.CheckDotForUpgrade(self.body.inventory, ref inflictDotInfo);

                                DotController.InflictDot(ref inflictDotInfo);
                            }
                        }

                        GlobalEventManager.igniteOnKillHurtBoxBuffer.Clear();
                    }
                }

            orig(self, damageinfo);
        }
    }
     public class DarkLightningItem
    {
        public static ItemDef darkLightningItem;

        private Sprite darkLightningSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/LightningStrikeOnHit/texLightningStrikeOnHit.png").WaitForCompletion();

        private GameObject darkLightningPickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/LightningStrikeOnHit/PickupChargedPerforator.prefab")
            .WaitForCompletion();
        

        public DarkLightningItem()
        {
            darkLightningItem = ScriptableObject.CreateInstance<ItemDef>();
            darkLightningItem.name = "DARK_LIGHTNING_NAME";
            darkLightningItem.descriptionToken = "DARK_LIGHTNING_DESCRIPTION";
            darkLightningItem.nameToken = "DARK_LIGHTNING_NAME";
            darkLightningItem.loreToken = "DARK_LIGHTNING_LORE";
            darkLightningItem.pickupToken = "DARK_LIGHTNING_PICKUP";
            darkLightningItem.pickupIconSprite = darkLightningSprite;
            darkLightningItem.pickupModelPrefab = darkLightningPickup;
            darkLightningItem.canRemove = true;
            darkLightningItem.hidden = false;
            darkLightningItem._itemTierDef = darkTier;
            var displayRules = new ItemDisplayRuleDict(null);
            darkLightningItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkLightningItem, displayRules));
            LanguageAPI.Add("DARK_LIGHTNING_NAME", "Charged Claw");
            LanguageAPI.Add("DARK_LIGHTNING_DESCRIPTION", "10% chance on hit to down a lightning strike on the enemy and 2 (+2 per stack) enemies within 15m (+8m per stack), dealing 1000% (+1000% per stack) damage. Killing a dark enemy grants 4% (+4% per stack) damage.");
            LanguageAPI.Add("DARK_LIGHTNING_PICKUP",
                "Chance on hit to summon a lightning storm. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkLightningItem);
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManagerOnProcessHitEnemy;
        }

        private void GlobalEventManagerOnProcessHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker)
            {
                CharacterBody cb = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody cb2 = victim.GetComponent<CharacterBody>();
                if (cb && cb2)
                {
                    int numLightningItems = cb.inventory.GetItemCount(darkLightningItem);
                    if (numLightningItems > 0 && !damageInfo.procChainMask.HasProc(ProcType.LightningStrikeOnHit) &&
                        Util.CheckRoll(10f * damageInfo.procCoefficient, cb.master))
                    {
                        float damageValue =
                            Util.OnHitProcDamage(damageInfo.damage, cb.damage, 10f * numLightningItems);
                        ProcChainMask procChainMask = damageInfo.procChainMask;
                        procChainMask.AddProc(ProcType.LightningStrikeOnHit);
                        HurtBox target = cb2.mainHurtBox;
                        if (cb2.hurtBoxGroup)
                        {
                            target = cb2.hurtBoxGroup.hurtBoxes[
                                Random.Range(0, cb2.hurtBoxGroup.hurtBoxes.Length)];
                        }

                        OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
                        {
                            attacker = cb.gameObject,
                            damageColorIndex = DamageColorIndex.Item,
                            damageValue = damageValue,
                            isCrit = Util.CheckRoll(cb.crit, cb.master),
                            procChainMask = procChainMask,
                            procCoefficient = 1f,
                            target = target
                        });
                        BullseyeSearch bullseyeSearch = new BullseyeSearch();
                        bullseyeSearch.searchOrigin = victim.transform.position;
                        bullseyeSearch.searchDirection = Vector3.zero;
                        bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
                        bullseyeSearch.teamMaskFilter.RemoveTeam(cb.teamComponent.teamIndex);
                        bullseyeSearch.filterByLoS = false;
                        bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
                        bullseyeSearch.maxDistanceFilter = 7f + 8f * numLightningItems;
                        bullseyeSearch.RefreshCandidates();
                        List<HurtBox> list = bullseyeSearch.GetResults().ToList();
                        for (int i = 0; i < numLightningItems*2 && list.Count > 0; i++)
                        {
                            int x = Random.Range(0, list.Count);
                            HurtBox hb = list[x];
                            list.RemoveAt(x);
                            OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
                            {
                                attacker = cb.gameObject,
                                damageColorIndex = DamageColorIndex.Item,
                                damageValue = damageValue,
                                isCrit = Util.CheckRoll(cb.crit, cb.master),
                                procChainMask = procChainMask,
                                procCoefficient = 1f,
                                target = hb
                            });
                        }
                    }
                }
            }

            orig(self, damageInfo, victim);
        }
    }
     public class DarkFireItem
    {
        public static ItemDef darkFireItem;

        private Sprite darkFireSprite =
            Addressables.LoadAssetAsync<Sprite>("RoR2/Base/FireballsOnHit/texFireballsOnHitIcon.png").WaitForCompletion();

        private GameObject darkFirePickup = Addressables
            .LoadAssetAsync<GameObject>("RoR2/Base/FireballsOnHit/PickupFireballsOnHit.prefab")
            .WaitForCompletion();
        

        public DarkFireItem()
        {
            darkFireItem = ScriptableObject.CreateInstance<ItemDef>();
            darkFireItem.name = "DARK_FIRE_NAME";
            darkFireItem.descriptionToken = "DARK_FIRE_DESCRIPTION";
            darkFireItem.nameToken = "DARK_FIRE_NAME";
            darkFireItem.loreToken = "DARK_FIRE_LORE";
            darkFireItem.pickupToken = "DARK_FIRE_PICKUP";
            darkFireItem.pickupIconSprite = darkFireSprite;
            darkFireItem.pickupModelPrefab = darkFirePickup;
            darkFireItem.canRemove = true;
            darkFireItem.hidden = false;
            darkFireItem._itemTierDef = darkTier;
            var displayRules = new ItemDisplayRuleDict(null);
            darkFireItem.itemIndex = ItemIndex.Count;
            ItemAPI.Add(new CustomItem(darkFireItem, displayRules));
            LanguageAPI.Add("DARK_FIRE_NAME", "Molten Claw");
            LanguageAPI.Add("DARK_FIRE_DESCRIPTION", "10% chance on hit to call forth 6 (+3 per stack) magma balls from an enemy, dealing (3000% (+3000% per stack) damage)% base damage. Killing a dark enemy grants 0.5 (+0.5 per stack) base damage.");
            LanguageAPI.Add("DARK_FIRE_PICKUP",
                "Chance on hit to summon fireballs. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkFireItem);
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManagerOnProcessHitEnemy;
        }
        private void GlobalEventManagerOnProcessHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker)
            {
                CharacterBody cb = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody cb2 = victim.GetComponent<CharacterBody>();
                if (cb && cb2)
                {
                    int numFireItems = cb.inventory.GetItemCount(darkFireItem);
                    if (numFireItems > 0 && !damageInfo.procChainMask.HasProc(ProcType.Meatball))
                    {
                        Vector3 vector = cb2.characterMotor ? victim.transform.position + Vector3.up * (cb2.characterMotor.capsuleHeight * 0.5f + 2f) : victim.transform.position + Vector3.up * 2f;
                        Vector3 forward =  Vector3.up;
                        float variation = 1f;
                        if (Util.CheckRoll(10f * damageInfo.procCoefficient, cb.master))
                        {
                            EffectData effectData = new EffectData
                            {
                                scale = 1f,
                                origin = vector
                            };
                            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleflashFireMeatBall"), effectData, true);
                            int numFireballs = 3 + 3 * numFireItems;
                            float damageCoefficient = 0.3f * cb.damage * numFireItems;
                            float damage = Util.OnHitProcDamage(damageInfo.damage, cb.damage, damageCoefficient);
                            float minInclusive = 15f;
                            float maxInclusive = 30f;
                            ProcChainMask procChainMask = damageInfo.procChainMask;
                            procChainMask.AddProc(ProcType.Meatball);
                            float speedOverride = Random.Range(minInclusive, maxInclusive);
                            for (int k = 0; k < numFireballs; k++)
                            {
                                float angle = k * 3.1415927f * 2f / numFireballs;
                                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                                {
                                    projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FireMeatBall"),
                                    position = vector + new Vector3( Mathf.Sin(angle), 0f, Mathf.Cos(angle)),
                                    rotation = Util.QuaternionSafeLookRotation(forward),
                                    procChainMask = procChainMask,
                                    target = victim,
                                    owner = cb.gameObject,
                                    damage = damage,
                                    crit = damageInfo.crit,
                                    force = 200f,
                                    damageColorIndex = DamageColorIndex.Item,
                                    speedOverride = speedOverride,
                                    useSpeedOverride = true
                                };
                                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                                forward.x += Mathf.Sin(angle + Random.Range(-variation, variation));
                                forward.z += Mathf.Cos(angle + Random.Range(-variation, variation));
                            }
                        }
                    }
                }
            }

            orig(self, damageInfo, victim);
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
            LanguageAPI.Add("DARK_CONSTRUCT_DESCRIPTION", "Killing an elite enemy spawns an Alpha Construct that attaches to you with 1000% (+1000% per stack) health. On hit, all Constructs attached to you have a 5% chance to fire at the enemy hit for 300% (+300% per stack) total damage. Limit of 4 (+4 per stack) constructs. Upon killing a dark enemy, gain .03 luck.");
            LanguageAPI.Add("DARK_CONSTRUCT_PICKUP",
                "Upon killing an elite, gain an alpha construct that attaches to you and fires at enemies you fire at. Grows stronger as it absorbs darkness.");
            darkItems.Add(darkConstructItem);
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManagerOnonCharacterDeathGlobal;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += CharacterMasterOnGetDeployableSameSlotLimit;
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManagerOnProcessHitEnemy;
            testItem = darkConstructItem;
            spawnCard.prefab.GetComponent<BaseAI>().desiredSpawnNodeGraphType = MapNodeGroup.GraphType.Air;
            AISkillDriver[] skills = spawnCard.prefab.GetComponents<AISkillDriver>();
            for (int i = 0; i < skills.Length; i++)
            {
                skills[i].enabled = false;
            }
        }

        private void GlobalEventManagerOnProcessHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageinfo, GameObject victim)
        {
            if (damageinfo.attacker)
            {
                CharacterBody cb = damageinfo.attacker.GetComponent<CharacterBody>();
                if (cb && cb.inventory)
                {
                    int numItems = cb.inventory.GetItemCount(darkConstructItem);
                    if (numItems > 0)
                    {
                        cb.GetComponent<DarkConstructItemBehavior>().shoot(damageinfo,victim);
                    }
                }
            }

            orig(self, damageinfo, victim);
        }

        private int CharacterMasterOnGetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            if (slot == DeployableSlot.MinorConstructOnKill)
            {
                return 4 * (self.inventory.GetItemCount(DLC1Content.Items.MinorConstructOnKill) +
                            self.inventory.GetItemCount(darkConstructItem));
            }

            return orig(self, slot);
        }

        private void GlobalEventManagerOnonCharacterDeathGlobal(DamageReport obj)
        {
            if (obj.attackerBody && obj.attackerBody.inventory && obj.victimBody && obj.victimBody.isElite && obj.attackerMaster && obj.attacker)
            {
        
                if (!obj.attackerMaster.IsDeployableLimited(DeployableSlot.MinorConstructOnKill) && obj.attackerBody.inventory.GetItemCount(darkConstructItem) > 0)
                {
                    obj.attackerBody.GetComponent<DarkConstructItemBehavior>().spawnChild();
                }
            }
        }
        public class DarkConstructItemBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef()
            {
                return darkConstructItem;
            }

            private void Start()
            {
                master = body.master;
            }

            public CharacterMaster master;

            public void spawnChild()
            {
                Log.Debug("Spawning");
                CharacterMaster characterMaster = master;
                DirectorCore.MonsterSpawnDistance input = DirectorCore.MonsterSpawnDistance.Close;
                DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
                {
                    spawnOnTarget = transform,
                    placementMode = DirectorPlacementRule.PlacementMode.Direct
                };
                DirectorCore.GetMonsterSpawnDistance(input, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, new Xoroshiro128Plus(Run.instance.seed + (ulong)Run.instance.fixedTime));
                directorSpawnRequest.teamIndexOverride = characterMaster.teamIndex;
                directorSpawnRequest.ignoreTeamMemberLimit = false;
                directorSpawnRequest.summonerBodyObject = gameObject;
                directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, new Action<SpawnCard.SpawnResult>(delegate(SpawnCard.SpawnResult result)
                {
                    if (result.success && result.spawnedInstance)
                    {
                        result.spawnedInstance.GetComponent<CharacterMaster>().GetBody().baseMaxHealth *= 10 * stack;
                        Deployable deployable = result.spawnedInstance.AddComponent<Deployable>();
                        NetworkedBodyAttachment nba = result.spawnedInstance.AddComponent<NetworkedBodyAttachment>();
                        nba.AttachToGameObjectAndSpawn(gameObject);
                        characterMaster.AddDeployable(deployable,DeployableSlot.MinorConstructOnKill);
                        children.Add(result.spawnedInstance);
                        masters.Add(result.spawnedInstance.GetComponent<CharacterMaster>());
                        positions.Add(Random.insideUnitSphere * 2f);
                        result.spawnedInstance.GetComponent<CharacterMaster>().GetBodyObject().transform.rotation =
                            Random.rotation;
                        result.spawnedInstance.GetComponent<CharacterMaster>().GetBodyObject().layer =
                            LayerIndex.playerBody.intVal;
                    }
                }));
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }

            public void shoot(DamageInfo di, GameObject victim)
            {
                if (!di.procChainMask.HasProc((ProcType)25))
                {
                    ProcChainMask newMask = di.procChainMask;
                    newMask.AddProc((ProcType)25);
                    for (int i = 0; i < children.Count; i++)
                    {
                        while (!children[i])
                        {
                            children.RemoveAt(i);
                            masters.RemoveAt(i);
                            positions.RemoveAt(i);
                            if (children.Count == i)
                            {
                                return;
                            }
                        }
                        if (Util.CheckRoll(5f * di.procCoefficient, master))
                        {
                            float newDamage = di.damage * 3 * stack;
                            ProjectileManager.instance.FireProjectileWithoutDamageType(projectile, children[i].transform.position, Quaternion.LookRotation(victim.transform.position-children[i].transform.position,Vector3.up), children[i], newDamage, 3f, Util.CheckRoll(body.crit, body.master), DamageColorIndex.Item,victim);
                        }
                    }
                }
            }

            private void FixedUpdate()
            {
                Vector3 currentPos = transform.position;
                for (int i = 0; i < children.Count; i++)
                {
                    while (!children[i])
                    {
                        children.RemoveAt(i);
                        masters.RemoveAt(i);
                        positions.RemoveAt(i);
                        if (children.Count == i)
                        {
                            return;
                        }
                    }

                    masters[i].GetBodyObject().transform.position = currentPos + positions[i];
                }
            }

            private List<GameObject> children = new ();
            private List<CharacterMaster> masters = new();
            private List<Vector3> positions = new();

        }

        private static GameObject projectile = Addressables
            .LoadAssetAsync<GameObject>("RoR2/DLC1/MajorAndMinorConstruct/MinorConstructProjectile.prefab")
            .WaitForCompletion();
        private static SpawnCard spawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/MajorAndMinorConstruct/cscMinorConstructOnKill.asset").WaitForCompletion();
    }
    #endregion
    
    
    
}