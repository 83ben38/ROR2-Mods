using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace DarknessExpansion;
[BepInPlugin("com.cybug.DarknessExpansion", "DarknessExpansion","1.1.0")]
[BepInDependency(ItemAPI.PluginGUID)]
[BepInDependency(EliteAPI.PluginGUID)]
[BepInDependency(LanguageAPI.PluginGUID)]
[BepInDependency(DirectorAPI.PluginGUID)]
[BepInDependency(ColorsAPI.PluginGUID)]
[BepInDependency(RecalculateStatsAPI.PluginGUID)]


public class DarknessExpansion : BaseUnityPlugin
{
    public static ConfigEntry<int> startingDarkness;
    public static ConfigEntry<int> maximumDarknessLevel;
    public static ConfigEntry<int> darknessGainedFromShrine;
    public static ConfigEntry<int> darknessGainedFromItem;
    public static ConfigEntry<int> darknessGainedFromArtifact;
    public static ConfigEntry<bool> linearDarknessEliteItemScaling;
    public static ConfigEntry<float> maximumItemChance;
    public static ConfigEntry<int> maximumBonusItems;
    public static ConfigEntry<bool> linearDarknessEliteChanceScaling;
    public static ConfigEntry<bool> linearDarknessEliteStatsScaling;
    public static ConfigEntry<float> healthBoostAmount;
    public static ConfigEntry<float> damageBoostAmount;
    public static ConfigEntry<int> creditCost;
    public static ConfigEntry<int> selectionWeight;
    public static ConfigEntry<int> maxDarknessShrines;
    public static ConfigEntry<int> numPotentialsPerShrine;
    public static ConfigEntry<float> baseShrineCredits;
    public static ConfigEntry<int> numWhiteItemsGiven;
    public static ConfigEntry<int> numGreenItemsGiven;
    public static ConfigEntry<int> numRedItemsGiven;
    public static ConfigEntry<int> numYellowItemsGiven;
    public static ConfigEntry<float> bonusStatsGiven;
    public static ConfigEntry<int> numWhitesPerRed;
    public static ConfigEntry<bool> logStacking;
    public static ConfigEntry<bool> sqrtStacking;
    public static ConfigEntry<int> golemHealth;
    public static ConfigEntry<int> golemRegen;
    public static ConfigEntry<float> golemChance;
    public static ConfigEntry<int> golemBaseDamage;
    public static ConfigEntry<int> golemDamagePerHealth;
    public static ConfigEntry<int> golemStackingHealth;
    public static ConfigEntry<int> golemStackingRegen;
    public static ConfigEntry<float> golemStacking;
    public static ConfigEntry<float> pearlHealthPercent;
    public static ConfigEntry<float> pearlOnKillPercent;
    public static ConfigEntry<float> pearlStacking;
    public static ConfigEntry<float> pearl2AllStatsPercent;
    public static ConfigEntry<float> pearl2OnKillPercent;
    public static ConfigEntry<float> pearl2Stacking;
    public static ConfigEntry<float> bleedCritChancePercent;     
    public static ConfigEntry<int>   bleedStacksPerHit;
    public static ConfigEntry<float> explosionBaseDamagePercent; 
    public static ConfigEntry<float> explosionHealthPercent;     
    public static ConfigEntry<float> onKillCritDamagePercent;    
    public static ConfigEntry<float> bleedStackingMultiplier;
    public static ConfigEntry<float> beetleSpawnInterval;
    public static ConfigEntry<float> beetleBaseDamagePercent;
    public static ConfigEntry<float> beetleBaseHealthPercent;
    public static ConfigEntry<int>   beetleDebuffStacks;
    public static ConfigEntry<int>   beetleMaxGuards;
    public static ConfigEntry<float> beetleOnKillASPercent;
    public static ConfigEntry<float> coreSpawnInterval;
    public static ConfigEntry<float> coreAllyDamagePerAlly;
    public static ConfigEntry<float> coreOnKillAllyStatPct;
    public static ConfigEntry<float> jellyLowHealthThreshold;
    public static ConfigEntry<float> jellyRechargeInterval;
    public static ConfigEntry<int>   jellyBaseCharges;
    public static ConfigEntry<float> jellySecondaryBasePct;
    public static ConfigEntry<float> jellyOnKillCdrPct;
    public static ConfigEntry<float> jellyStackingMultiplier;
    public static ConfigEntry<int>   wispCount;            
    public static ConfigEntry<int> wispBaseDamageMult;   
    public static ConfigEntry<float> wispProcCoeff;        
    public static ConfigEntry<float> wispOnKillMoveSpeedPct;
    public static ConfigEntry<float> wispStackingMultiplier;
    public static ConfigEntry<int>   clayTetherCount;
    public static ConfigEntry<float> clayTarBonusDamagePct;
    public static ConfigEntry<float> clayHealPct;
    public static ConfigEntry<float> clayDarkKillHealingBonusPct;
    public static ConfigEntry<float> clayStackingMultiplier;
    public static ConfigEntry<float> parentHealPerArmorPct;
    public static ConfigEntry<float> parentIgniteRadiusBase;
    public static ConfigEntry<float> parentDarkKillArmorBonus;
    public static ConfigEntry<float> parentStackingMultiplier;
    public static ConfigEntry<float> lightningProcChance; 
    public static ConfigEntry<int>   lightningExtraTargets;
    public static ConfigEntry<float> lightningRangeBase;
    public static ConfigEntry<float> lightningDamagePct;
    public static ConfigEntry<float> lightningOnKillDamageBonus;
    public static ConfigEntry<float> lightningStackingMultiplier;
    public static ConfigEntry<float> fireProcChance;              
    public static ConfigEntry<int>   fireBaseBalls;               
    public static ConfigEntry<float> fireDamagePct;               
    public static ConfigEntry<float> fireOnKillDamageBonus;    
    public static ConfigEntry<float> fireStackingMultiplier;     

    private void Awake()
    {
        startingDarkness = Config.Bind("Darkness Level", "Starting Darkness Level", 0, "What the starting darkness level is.");
        maximumDarknessLevel = Config.Bind("Darkness Level", "Maximum Darkness Level", 10, "The point at which the world becomes consumed with darkness.");
        darknessGainedFromShrine = Config.Bind("Darkness Level", "Darkness From Shrine Activation", 1, "How much is added to the darkness level on activating a darkness shrine.");
        darknessGainedFromItem = Config.Bind("Darkness Level", "Darkness From Dark Item", 1, "How much is added to the darkness level per dark item picked up.");
        darknessGainedFromArtifact = Config.Bind("Darkness Level", "Darkness From Artifact", 1, "How much is added to the darkness level when starting a run with the darkness artifact.");
        
        linearDarknessEliteItemScaling =
            Config.Bind("Darkness Elite", "Darkness Elite Item Chance Scaling Type", true,"Whether the item chance should scale linearly (true) or quadratically (false).");
        maximumItemChance = Config.Bind("Darkness Elite", "Maximum Item Chance", 1f, "The maximum chance that an elite gets bonus items.");
        maximumBonusItems= Config.Bind("Darkness Elite", "Maximum Bonus Items", 3, "The maximum number of bonus yellow items an elite can have.");
        linearDarknessEliteChanceScaling =
            Config.Bind("Darkness Elite", "Darkness Elite Spawn Chance Scaling Type", false,"Whether the spawn chance should scale linearly (true) or quadratically (false).");
        linearDarknessEliteStatsScaling =
            Config.Bind("Darkness Elite", "Darkness Elite Health and Damage Scaling Type", false,"Whether the health and damage should scale linearly (true) or square root (false).");
        healthBoostAmount = Config.Bind("Darkness Elite", "Health Boost Per Stack", 1f, "The amount of health boost an elite gets from 1 darkness level.");
        damageBoostAmount = Config.Bind("Darkness Elite", "Damage Boost Per Stack", 0.5f, "The amount of damage boost an elite gets from 1 darkness level.");

        creditCost = Config.Bind("Darkness Shrine", "Credit Cost", 10, "How many credits the darkness shrine costs.");
        selectionWeight = Config.Bind("Darkness Shrine", "Selection Weight", 100, "The weight of the Darkness Shrine.");
        maxDarknessShrines = Config.Bind("Darkness Shrine", "Maximum Darkness Shrines", 1,
            "How many Darkness Shrines can spawn at once.");
        numPotentialsPerShrine = Config.Bind("Darkness Shrine", "Number Of Potentials Per Shrine", 3,
            "How many Potentials spawn per Darkness Shrine. Also is the number of yellows required to make a dark item.");
        baseShrineCredits = Config.Bind("Darkness Shrine", "Base Shrine Credits", 600f,
            "The base shrine credits of the darkness shrine (scales like the teleporter).");
        numWhiteItemsGiven = Config.Bind("Darkness Shrine", "Number of White Items", 5,
            "The number of each white item sacrificed given to the boss.");
        numGreenItemsGiven = Config.Bind("Darkness Shrine", "Number of Green Items", 3,
            "The number of each green item sacrificed given to the boss.");
        numRedItemsGiven = Config.Bind("Darkness Shrine", "Number of Red Items", 1,
            "The number of each red item sacrificed given to the boss.");
        numYellowItemsGiven = Config.Bind("Darkness Shrine", "Number of Yellow Items", 1,
            "The number of each yellow item sacrificed given to the boss.");
        bonusStatsGiven = Config.Bind("Darkness Shrine", "Amount of Bonus Stats", 1f,
            "The amount of darkness level required for the dark boss to gain 10% bonus stats.");
        numWhitesPerRed = Config.Bind("Darkness Shrine", "Number of Whites Per Red", 5,
            "How many white items a red item upgrades into.");

        logStacking = Config.Bind("Darkness Items", "Logarithmic Stacking", false,
            "Whether the stacking from dark items stacks logarithmically. If this and sqrt stacking are both enabled, stacking is disabled.");
        sqrtStacking = Config.Bind("Darkness Items", "Square Root Stacking", false,
            "Whether the stacking from dark items stacks by square root. If this and log stacking are both enabled, stacking is disabled.");
        golemHealth            = Config.Bind("Titanic Boulder", "Health",              100,  "Base health of each Golem Item.");
        golemRegen             = Config.Bind("Titanic Boulder", "Regen",               10,     "Health regen granted by Golem Item.");
        golemChance            = Config.Bind("Titanic Boulder", "Spawn Chance",        20f, "% chance to spawn a Golem on hit.");
        golemBaseDamage        = Config.Bind("Titanic Boulder", "Base Damage %",         200,    "Base % Damage dealt by Golem item.");
        golemDamagePerHealth   = Config.Bind("Titanic Boulder", "Damage per Health %",   100,     "Additional % damage per 500 health.");
        golemStackingHealth    = Config.Bind("Titanic Boulder", "Stacking Health",     5,   "Additional health per darkness stack per Golem Item.");
        golemStackingRegen     = Config.Bind("Titanic Boulder", "Stacking Regen",      1,     "Additional regen per darkness stack per Golem Item.");
        golemStacking          = Config.Bind("Titanic Boulder", "Stacking Bonus",     1f, "Multiplier applied to per-stack values");
        pearlHealthPercent   = Config.Bind("Dark Pearl", "Base Health %",  50f, "Base % max health");
        pearlOnKillPercent   = Config.Bind("Dark Pearl", "On Kill %",      2f,  "Health % gained on kill");
        pearlStacking        = Config.Bind("Dark Pearl", "Stacking Mult",  1f,  "Multiplier applied to per-stack values");
        pearl2AllStatsPercent = Config.Bind(
            "Dark Irradiant Pearl", "All Stats %", 50f, "Base % increase to all stats per Dark Irradiant Pearl");
        pearl2OnKillPercent = Config.Bind("Dark Irradiant Pearl", "On Kill All Stats %", 1f, "Additional % to all stats on kill per Dark Irradiant Pearl");
        pearl2Stacking = Config.Bind("Dark Irradiant Pearl", "Stacking Multiplier", 1f, "Multiplier applied to per-stack values");
        bleedCritChancePercent       = Config.Bind("Dark Shatterspleen", "Crit Chance %",            20f,  "Base critical chance granted by each item");
        bleedStacksPerHit            = Config.Bind("Dark Shatterspleen", "Bleed Stacks per Hit",     1,    "Bleed stacks applied on each hit");
        explosionBaseDamagePercent   = Config.Bind("Dark Shatterspleen", "Explosion Base %",         100f, "Base % damage per bleed item on death");
        explosionHealthPercent       = Config.Bind("Dark Shatterspleen", "Explosion Health %",       15f,  "Max-health % dealt per bleed item on death");
        onKillCritDamagePercent      = Config.Bind("Dark Shatterspleen", "On-Kill Crit Dmg %",        3f,  "Crit damage % bonus upon killing a dark enemy");
        bleedStackingMultiplier      = Config.Bind("Dark Shatterspleen", "Stacking Multiplier",       1f,  "Multiplier applied to all per-stack values");
        beetleSpawnInterval     = Config.Bind("King's Gland", "Spawn Interval",    30f,  "Seconds between Beetle Guard spawns");
        beetleBaseDamagePercent = Config.Bind("King's Gland", "Guard Damage %",        300f, "Base % damage of each Beetle Guard");
        beetleBaseHealthPercent = Config.Bind("King's Gland", "Guard Health %",        300f, "Base % health of each Beetle Guard");
        beetleDebuffStacks      = Config.Bind("King's Gland", "Debuff Stacks per Hit",1,    "Number of debuff stacks Beetle applies on hit");
        beetleMaxGuards         = Config.Bind("King's Gland", "Max Guards",            1,    "Maximum simultaneous Beetle Guards");
        beetleOnKillASPercent   = Config.Bind("King's Gland", "Attack Speed Gain per Kill",         3f,   "Attack speed % gained on killing a dark enemy");
        coreSpawnInterval     = Config.Bind("Sympathy Cores", "Spawn Interval",       10f, "Seconds between each pair of Solus Probe summons");
        coreAllyDamagePerAlly = Config.Bind("Sympathy Cores", "Damage % per Ally",       200f, "Base % damage bonus per ally on your team");
        coreOnKillAllyStatPct = Config.Bind("Sympathy Cores", "On-Kill Ally Stat %",      2f, "Percent buff to all allies’ stats on killing a dark enemy");
        jellyLowHealthThreshold   = Config.Bind("Omega Loop", "Low Health Threshold", 0.5f, "Fraction of health to trigger charging");
        jellyRechargeInterval     = Config.Bind("Omega Loop", "Charge Interval (s)", 30f,  "Base seconds per explosion");
        jellyBaseCharges          = Config.Bind("Omega Loop", "Base Charges",        3,    "Starting number of charges");
        jellySecondaryBasePct     = Config.Bind("Omega Loop", "Secondary Base %",  500f, "Base secondary projectile damage percent");
        jellyOnKillCdrPct         = Config.Bind("Omega Loop", "On-Kill CDR %",     1f,  "Cooldown reduction percent gained on kill");
        jellyStackingMultiplier   = Config.Bind("Omega Loop", "Stacking Multiplier",1f,  "Multiplier applied to all per-stack values");
        wispCount               = Config.Bind("Large Disciple", "Base Wisps",            3,     "Number of wisps fired per shot");
        wispBaseDamageMult      = Config.Bind("Large Disciple", "Wisp Damage %",   300,    "Damage percent per wisp");
        wispProcCoeff           = Config.Bind("Large Disciple", "Proc Coefficient",     3f,    "Proc coefficient per wisp");
        wispOnKillMoveSpeedPct  = Config.Bind("Large Disciple", "On-Kill MoveSpeed %", 3f,    "Move-speed % gained on killing a dark enemy");
        wispStackingMultiplier  = Config.Bind("Large Disciple", "Stacking Multiplier", 1f,    "Multiplier applied to each per-stack value");
        clayTetherCount               = Config.Bind("Polished Urn", "Tether Count",           1,   "Base number of enemies tethered to you");
        clayTarBonusDamagePct         = Config.Bind("Polished Urn", "Tar Bonus Damage %",    15f, "Bonus damage % against tarred enemies");
        clayHealPct                   = Config.Bind("Polished Urn", "Heal Percent",           5f, "Heal % of damage dealt to tarred enemies");
        clayDarkKillHealingBonusPct  = Config.Bind("Polished Urn", "On-Kill Heal %",        3f, "Healing bonus % gained on dark kill");
        clayStackingMultiplier        = Config.Bind("Polished Urn", "Stacking Multiplier",     1f, "Multiplier applied to all per-stack values");
        parentHealPerArmorPct       = Config.Bind("Dark Planula", "Heal Per Armor %", 100f, "Heal percent of armor when damaged");
        parentIgniteRadiusBase      = Config.Bind("Dark Planula", "Ignite Radius Base", 13f, "Base radius for ignite explosion");
        parentDarkKillArmorBonus    = Config.Bind("Dark Planula", "Dark Kill Armor Bonus", 1.5f, "Armor bonus gained on dark enemy kill");
        parentStackingMultiplier    = Config.Bind("Dark Planula", "Stacking Multiplier", 1f, "Multiplier for all per-stack effects");
        lightningProcChance         = Config.Bind("Charged Claw", "Proc Chance %",           10f,   "Base % chance to trigger lightning on hit");
        lightningExtraTargets       = Config.Bind("Charged Claw", "Extra Targets per Item",    2,   "Number of extra enemies struck per item");
        lightningRangeBase          = Config.Bind("Charged Claw", "Base Range",            15f,   "Base search radius for additional targets");
        lightningDamagePct          = Config.Bind("Charged Claw", "Damage %",             1000f, "Base damage % of lightning strike");
        lightningOnKillDamageBonus  = Config.Bind("Charged Claw", "On-Kill Damage %",      4f,   "Damage % bonus on killing a dark enemy");
        lightningStackingMultiplier = Config.Bind("Charged Claw", "Stacking Multiplier",   1f,   "Multiplier applied to all per-stack values");
        fireProcChance           = Config.Bind("Dark Fire", "Proc Chance %",           10f,   "Chance to summon magma balls on hit");
        fireBaseBalls            = Config.Bind("Dark Fire", "Base Balls",               6,    "Number of magma balls summoned per proc");
        fireDamagePct            = Config.Bind("Dark Fire", "Base Damage Damage %%",              3000f, "Base damage damage percent percent of each magma ball");
        fireOnKillDamageBonus = Config.Bind("Dark Fire", "On-Kill Damage",        0.5f, "Base damage bonus on dark enemy kill");
        fireStackingMultiplier   = Config.Bind("Dark Fire", "Stacking Multiplier",     1f,   "Multiplier applied to all per-stack effects");

        
        Log.Init(Logger);
        new Darkness();
        new DarknessShrine();
        new DarknessArtifact();
        new DarknessItems();
    }

    // private int itemNum = 0;
    //
    // private void Update()
    // {
    //     // This if statement checks if the player has currently pressed F2.
    //     if (Input.GetKeyDown(KeyCode.F2))
    //     {
    //         var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
    //         PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(DarknessItems.darkItems[itemNum].itemIndex),transform.position,transform.forward*20f);
    //         itemNum++;
    //         if (itemNum == DarknessItems.darkItems.Count)
    //         {
    //             itemNum = 0;
    //         }
    //     }
    // }

}