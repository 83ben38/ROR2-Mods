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
[BepInPlugin("com.cybug.DarknessExpansion", "DarknessExpansion","0.0.1")]
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
    private void Awake()
    {
        startingDarkness = Config.Bind("Darkness Level", "Starting Darkness Level", 0, "What the starting darkness level is.");
        maximumDarknessLevel = Config.Bind("Darkness Level", "Maximum Darkness Level", 10, "The point at which the world becomes consumed with darkness.");
        darknessGainedFromShrine = Config.Bind("Darkness Level", "Darkness From Shrine Activation", 1, "How much is added to the darkness level on activating a darkness shrine.");
        darknessGainedFromItem = Config.Bind("Darkness Level", "Darkness From Dark Item", 1, "How much is added to the darkness level per dark item picked up.");
        darknessGainedFromItem = Config.Bind("Darkness Level", "Darkness From Artifact", 1, "How much is added to the darkness level when starting a run with the darkness artifact.");
        
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