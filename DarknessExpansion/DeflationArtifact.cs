using BepInEx;
using R2API;
using RoR2;
using UnityEngine;

namespace DarknessExpansion;
[BepInPlugin("com.cybug.DeflationArtifact", "DeflationArtifact","0.0.1")]
[BepInDependency(ItemAPI.PluginGUID)]
[BepInDependency(EliteAPI.PluginGUID)]
[BepInDependency(LanguageAPI.PluginGUID)]
[BepInDependency(DirectorAPI.PluginGUID)]
[BepInDependency(ColorsAPI.PluginGUID)]
[BepInDependency(RecalculateStatsAPI.PluginGUID)]


public class DeflationArtifact : BaseUnityPlugin
{
    public static PluginInfo PInfo;
    private void Awake()
    {
        PInfo = Info;
        Log.Init(Logger);
        new Deflation();
    }
    
    
    private void Update()
    {
        // This if statement checks if the player has currently pressed F2.
        if (Input.GetKeyDown(KeyCode.F2))
        {
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            Log.Debug(transform.position);
            // 
            // itemNum++;
            // if (itemNum == DarknessItems.darkItems.Count)
            // {
            //     itemNum = 0;
            // }
        }
    }

}