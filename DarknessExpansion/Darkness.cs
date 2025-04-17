using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using CombatDirector = On.RoR2.CombatDirector;


namespace DarknessExpansion;

public class Darkness
{
    public static EliteDef DarknessElite;
    public static EquipmentDef DarknessEquipment;
    public static BuffDef DarknessBuff;
    public static int DarknessLevel = 0;
    private static Sprite eliteIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteFire/texBuffAffixRed.tif").WaitForCompletion();

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
        Run.onRunStartGlobal += run => DarknessLevel = 0;
    }

    private void SpawnCardOnonSpawnedServerGlobal(SpawnCard.SpawnResult obj)
    {
        Inventory i = obj.spawnedInstance.GetComponent<Inventory>();
        if (i != null)
        {
            if (i.GetEquipmentIndex() == DarknessEquipment.equipmentIndex)
            {
                float itemChance = DarknessLevel / 10f;
                if (Random.value < itemChance)
                {
                    List<ItemIndex> li = ItemCatalog.tier3ItemList;
                    i.GiveItem(li[(int)(li.Count * Random.value)]);
                    Log.Debug("Giving Item");
                    
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
        float chanceToSwap = DarknessLevel * DarknessLevel / 100f; 
        if (Random.value < chanceToSwap)
        {
            elitedef = DarknessElite;
            Log.Debug("Changing Elite Type");
            
        }

        return orig(self,spawncard,elitedef,spawntarget,spawndistance,preventoverhead,valuemultiplier,placementmode);
    }
    
    public static void UpdateDarkness()
    {
        DarknessElite.healthBoostCoefficient = 1f + Mathf.Sqrt(DarknessLevel);
        DarknessElite.damageBoostCoefficient = 1f + (DarknessElite.healthBoostCoefficient-1) / 2f;
    }
}