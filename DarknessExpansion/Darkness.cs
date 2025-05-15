using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using CombatDirector = On.RoR2.CombatDirector;
using Random = UnityEngine.Random;


namespace DarknessExpansion;

public class Darkness
{
    public static EliteDef DarknessElite;
    public static EquipmentDef DarknessEquipment;
    public static BuffDef DarknessBuff;
    public static int DarknessLevel = 0;
    private static Sprite eliteIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteFire/texBuffAffixRed.tif").WaitForCompletion();
    
    public static int maxDarknessLevel;
    public static float maxItemChance;
    public static bool linearItemScaling;
    public static int maximumBonusItems;
    public static bool linearEliteScaling;
    public static bool linearStatsScaling;
    public static float healthBoost;
    public static float damageBoost;
    public Darkness()
    {
        
        DarknessElite = ScriptableObject.CreateInstance<EliteDef>();
        DarknessElite.color = new Color(0, 0, 0, 255);
        EliteRamp.AddRamp(DarknessElite,CreateDarknessTexture());
        DarknessElite.name = "EliteDarkness";
        DarknessElite.modifierToken = "Dark {0}";

        DarknessBuff = ScriptableObject.CreateInstance<BuffDef>();
        DarknessBuff.name = "DarknessBuff";
        DarknessBuff.canStack = false;
        DarknessBuff.isCooldown = false;
        DarknessBuff.isDebuff = false;
        DarknessBuff.buffColor = new Color(0, 0, 0, 255);
        DarknessBuff.iconSprite = eliteIcon;

        DarknessEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
        DarknessEquipment.appearsInMultiPlayer = true;
        DarknessEquipment.appearsInSinglePlayer = true;
        DarknessEquipment.canBeRandomlyTriggered = false;
        DarknessEquipment.canDrop = false;
        DarknessEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
        DarknessEquipment.cooldown = 0.0f;
        DarknessEquipment.isLunar = false;
        DarknessEquipment.isBoss = false;
        DarknessEquipment.passiveBuffDef = DarknessBuff;
        DarknessEquipment.dropOnDeathChance = 0;
        DarknessEquipment.enigmaCompatible = false;

        DarknessElite.eliteEquipmentDef = DarknessEquipment;
        DarknessBuff.eliteDef = DarknessElite;

        ContentAddition.AddEliteDef(DarknessElite);
        ContentAddition.AddBuffDef(DarknessBuff);
        ContentAddition.AddEquipmentDef(DarknessEquipment);
        
        UpdateDarkness();
        CombatDirector.Spawn += CombatDirectorOnSpawn;
        SpawnCard.onSpawnedServerGlobal += SpawnCardOnonSpawnedServerGlobal;
        Run.onRunStartGlobal += run => DarknessLevel = DarknessExpansion.startingDarkness.Value;

        maxDarknessLevel = DarknessExpansion.maximumDarknessLevel.Value;
        maxItemChance = DarknessExpansion.maximumItemChance.Value;
        linearItemScaling = DarknessExpansion.linearDarknessEliteItemScaling.Value;
        maximumBonusItems = DarknessExpansion.maximumBonusItems.Value;
        linearEliteScaling = DarknessExpansion.linearDarknessEliteChanceScaling.Value;
        linearStatsScaling = DarknessExpansion.linearDarknessEliteStatsScaling.Value;
        healthBoost = DarknessExpansion.healthBoostAmount.Value;
        damageBoost = DarknessExpansion.damageBoostAmount.Value;
    }

    private void SpawnCardOnonSpawnedServerGlobal(SpawnCard.SpawnResult obj)
    {
        if (obj.spawnedInstance)
        {
            Inventory i = obj.spawnedInstance.GetComponent<Inventory>();
            if (i)
            {
                if (i.GetEquipmentIndex() == DarknessEquipment.equipmentIndex)
                {
                    List<ItemDef> li = DarknessItems.darkItems;
                    i.GiveItem(li[(int)((li.Count-1) * Random.value)]);
                    float itemChance = DarknessLevel / (float)maxDarknessLevel;
                    if (!linearItemScaling)
                    {
                        itemChance *= itemChance;
                    }
                    itemChance *= maxItemChance;
                    if (Random.value < itemChance)
                    {
                        i.GiveItem(li[(int)((li.Count-1) * Random.value)]);
                    }
                    for (int j = 0; j < maximumBonusItems; j++)
                    {
                        if (Random.value < itemChance)
                        {
                            i.GiveItem(ItemCatalog.itemNameToIndex[
                                DarknessShrine.yellowItemNames[(int)((DarknessShrine.yellowItemNames.Length-1) * Random.value)]]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

    }

    public static Texture2D CreateDarknessTexture()
    {
        Texture2D texture = new Texture2D(256, 8);
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < 8; y++)
        {
            for (int i = 0; i < 50; i++)
            {
                
                texture.SetPixel(i, y, new Color(0,0,i/255f,255));
            }
            for (int i = 0; i < 50; i++)
            {
                texture.SetPixel(i+50, y, new Color(0,0,50/255f-i/255f,255));
            }
            for (int i = 0; i < 50; i++)
            {
                texture.SetPixel(i+100, y, new Color(i/255f,0,i/255f,255));
            }
            for (int i = 0; i < 50; i++)
            {
                texture.SetPixel(i+150, y, new Color(50/255f-i/255f,0,50/255f-i/255f,255));
            }
            for (int i = 0; i < 56; i++)
            {
                texture.SetPixel(i+200, y, new Color(0,i/255f,0,255));
            }
        }
        
        texture.Apply();
        
        return texture;
    }


    private bool CombatDirectorOnSpawn(CombatDirector.orig_Spawn orig, RoR2.CombatDirector self, SpawnCard spawncard, EliteDef elitedef, Transform spawntarget, DirectorCore.MonsterSpawnDistance spawndistance, bool preventoverhead, float valuemultiplier, DirectorPlacementRule.PlacementMode placementmode)
    {
        float chanceToSwap = DarknessLevel / (float)maxDarknessLevel;
        if (!linearEliteScaling)
        {
            chanceToSwap *= chanceToSwap;
        }
        if (Random.value < chanceToSwap)
        {
            elitedef = DarknessElite;
            Log.Debug("Changing Elite Type");
            
        }

        return orig(self,spawncard,elitedef,spawntarget,spawndistance,preventoverhead,valuemultiplier,placementmode);
    }

    public static event Action<int> onDarknessLevelChange;
    public static void UpdateDarkness()
    {
        float value = DarknessLevel;
        if (!linearStatsScaling)
        {
            value = Mathf.Sqrt(value);
        }
        DarknessElite.healthBoostCoefficient = 1f + (value * healthBoost);
        DarknessElite.damageBoostCoefficient = 1f + (value * damageBoost);
        if (onDarknessLevelChange != null)
        {
            onDarknessLevelChange.Invoke(DarknessLevel);
        }
    }
}