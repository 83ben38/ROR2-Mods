using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
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
    private void Awake()
    {
        Log.Init(Logger);
        new Darkness();
        new DarknessShrine();
        new DarknessArtifact();
        new DarknessItems();
    }
    
    private void Update()
    {
        // This if statement checks if the player has currently pressed F2.
        if (Input.GetKeyDown(KeyCode.F2))
        {
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(DarknessItems.testItem.itemIndex),transform.position,transform.forward*20f);
            for (int i = 0; i < BaseItemBodyBehavior.server.itemTypePairs.Length; i++)
            {
                Log.Debug(BaseItemBodyBehavior.server.itemTypePairs[i].behaviorType);
            }
            
        }
    }

}