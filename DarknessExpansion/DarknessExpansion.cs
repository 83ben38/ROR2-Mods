using System;
using BepInEx;
using R2API;
using UnityEngine;

namespace DarknessExpansion;
[BepInPlugin("com.cybug.DarknessExpansion", "DarknessExpansion","0.0.1")]
[BepInDependency(ItemAPI.PluginGUID,BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(EliteAPI.PluginGUID,BepInDependency.DependencyFlags.HardDependency)]

public class DarknessExpansion : BaseUnityPlugin
{
    private void Awake()
    {
        Log.Init(Logger);
        new Darkness();
    }
    
}