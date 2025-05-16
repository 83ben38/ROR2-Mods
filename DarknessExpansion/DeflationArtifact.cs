using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace DarknessExpansion;
[BepInPlugin("com.cybug.DeflationArtifact", "DeflationArtifact","1.2.1")]
[BepInDependency(ItemAPI.PluginGUID)]
[BepInDependency(EliteAPI.PluginGUID)]
[BepInDependency(LanguageAPI.PluginGUID)]
[BepInDependency(DirectorAPI.PluginGUID)] 
[BepInDependency(RecalculateStatsAPI.PluginGUID)]


public class DeflationArtifact : BaseUnityPlugin
{
    public static PluginInfo PInfo;
    public static ConfigEntry<bool> useOldIcon;
    public static ConfigEntry<int> stagesToSkip;
    public static ConfigEntry<float> timeToSkip;
    public static ConfigEntry<ulong> xpToGet;
    public static ConfigEntry<float> creditMultiplier;
    public static ConfigEntry<int> whitesToGive;
    public static ConfigEntry<int> greensToGive;
    public static ConfigEntry<int> redsToGive;
    public static ConfigEntry<int> yellowsToGive;
    public static ConfigEntry<int> voidTier1ToGive;
    public static ConfigEntry<int> voidTier2ToGive;
    public static ConfigEntry<int> equipmentToGive;
    private void Awake()
    {
        useOldIcon = Config.Bind("","Use Old Icon",false,"Whether the icon for the artifact will be the btd6 version (true), or the ror2 version (false).");
        stagesToSkip = Config.Bind("", "Stages To Skip", 2, "How many stages should be skipped when the artifact is enabled.");
        timeToSkip = Config.Bind("", "Time To Skip", 600f,
            "How much time should be skipped when the artifact is enabled in seconds.");
        xpToGet = Config.Bind("", "Starting XP", (ulong)746,
            "How much XP should be granted to the player when the artifact is enabled.");
        creditMultiplier = Config.Bind("","Credit Multiplier",0.5f,"How much credit the director should get while the artifact is enabled.");

        whitesToGive = Config.Bind("", "White Items", 15, "How many whites deflation gives.");
        greensToGive = Config.Bind("", "Green Items", 5, "How many greens deflation gives.");
        redsToGive = Config.Bind("", "Red Items", 1, "How many reds deflation gives.");
        yellowsToGive = Config.Bind("", "Yellow Items", 2, "How many yellows deflation gives.");
        voidTier1ToGive = Config.Bind("", "Void White Items", 1, "How many void whites deflation gives.");
        voidTier2ToGive = Config.Bind("", "Void Green Items", 1, "How many void greens deflation gives.");
        equipmentToGive = Config.Bind("", "Equipment", 2, "How many equipments deflation gives.");
        
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