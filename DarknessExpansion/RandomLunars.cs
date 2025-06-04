using BepInEx;
using R2API;
using RandomlyGeneratedItems;
using RoR2;
using UnityEngine;
using Random = UnityEngine.Random;


namespace DarknessExpansion;
[BepInPlugin("com.cybug.RandomlyGeneratedLunars", "RandomlyGeneratedLunars","1.0.0")]
[BepInDependency(ItemAPI.PluginGUID)]
[BepInDependency(EliteAPI.PluginGUID)]
[BepInDependency(LanguageAPI.PluginGUID)]
[BepInDependency(DirectorAPI.PluginGUID)]
[BepInDependency(ColorsAPI.PluginGUID)]
[BepInDependency(RecalculateStatsAPI.PluginGUID)]
[BepInDependency(RandomlyGeneratedItems.Main.PluginGuid)]


public class RandomLunars : BaseUnityPlugin
{

    private void Awake()
    {
        On.RoR2.Util.CheckRoll_float_float_CharacterMaster += CalculateDecimalLuck;
        Log.Init(Logger);
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

}