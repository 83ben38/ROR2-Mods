using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace DarknessExpansion;
[BepInPlugin("com.cybug.DeflationArtifact", "DeflationArtifact","1.1.0")]
[BepInDependency(ItemAPI.PluginGUID)]
[BepInDependency(EliteAPI.PluginGUID)]
[BepInDependency(LanguageAPI.PluginGUID)]
[BepInDependency(DirectorAPI.PluginGUID)] 
[BepInDependency(RecalculateStatsAPI.PluginGUID)]


public class DeflationArtifact : BaseUnityPlugin
{
    public static PluginInfo PInfo;
    public static ConfigEntry<bool> useOldIcon;
    private void Awake()
    {
        useOldIcon = Config.Bind("","UseOldIcon",false,"Whether the icon for the artifact will be the btd6 version (true), or the ror2 version (false).");
        PInfo = Info;
        Log.Init(Logger);
        new Deflation();
    }
    
    
    private void Update()
    {
        // This if statement checks if the player has currently pressed F2.
        // if (Input.GetKeyDown(KeyCode.F2))
        // {
        //     var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
        //     Log.Debug(transform.position);
        //     // 
        //     // itemNum++;
        //     // if (itemNum == DarknessItems.darkItems.Count)
        //     // {
        //     //     itemNum = 0;
        //     // }
        // }
    }

}