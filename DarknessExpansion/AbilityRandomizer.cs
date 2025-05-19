using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using UnityEngine;
using CharacterBody = On.RoR2.CharacterBody;


namespace DarknessExpansion;
[BepInPlugin("com.cybug.AbilityRandomizer", "AbilityRandomizer","1.0.0")]
[BepInDependency(ItemAPI.PluginGUID)]
[BepInDependency(EliteAPI.PluginGUID)]
[BepInDependency(LanguageAPI.PluginGUID)]
[BepInDependency(DirectorAPI.PluginGUID)] 
[BepInDependency(RecalculateStatsAPI.PluginGUID)]

public class AbilityRandomizer : BaseUnityPlugin
{
    
    private ConfigEntry<bool> keepSlot;
    private static List<SkillDef> primaries = new();
    private static List<SkillDef> secondaries = new();
    private static List<SkillDef> utilites = new();
    private static List<SkillDef> specials = new();
    private static List<SkillDef> all = new();
    public AbilityRandomizer()
    {
        Log.Init(Logger);
        keepSlot = Config.Bind("","Stay in slot",true,"Whether the abilities stay in the slot they are in.");
        foreach (SurvivorDef survivor in ContentManager.survivorDefs) { // first pass to collect skilldefs
            GameObject prefab = survivor.bodyPrefab;
            SkillLocator locator = prefab.GetComponent<SkillLocator>();
            CollectSkills(locator.primary.skillFamily, ref primaries);
            CollectSkills(locator.secondary.skillFamily, ref secondaries);
            CollectSkills(locator.utility.skillFamily, ref utilites);
            CollectSkills(locator.special.skillFamily, ref specials);
        }
        CharacterBody.OnSkillActivated += CharacterBodyOnOnSkillActivated;
    }

    private void CharacterBodyOnOnSkillActivated(CharacterBody.orig_OnSkillActivated orig, RoR2.CharacterBody self, GenericSkill skill)
    {
        orig(self, skill);
        if (self.skillLocator)
        {
            if (self.skillLocator.primary == skill)
            {
                self.skillLocator.primary.skillDef = keepSlot.Value ? primaries[(int)(primaries.Count *Random.value)] : all[(int)(all.Count *Random.value)];
            }
            else if (self.skillLocator.secondary == skill)
            {
                self.skillLocator.secondary.skillDef = keepSlot.Value ? secondaries[(int)(secondaries.Count *Random.value)] : all[(int)(all.Count *Random.value)];
            }
            else if (self.skillLocator.utility == skill)
            {
                self.skillLocator.utility.skillDef = keepSlot.Value ? utilites[(int)(utilites.Count *Random.value)] : all[(int)(all.Count *Random.value)];
            }
            else if (self.skillLocator.special == skill)
            {
                self.skillLocator.special.skillDef = keepSlot.Value ? specials[(int)(specials.Count *Random.value)] : all[(int)(all.Count *Random.value)];
            }
        }
    }

    private void CollectSkills(SkillFamily secondarySkillFamily, ref List<SkillDef> skillDefs)
    {
        foreach (SkillFamily.Variant variant in secondarySkillFamily.variants)
        {
            skillDefs.Add(variant.skillDef);
            all.Add(variant.skillDef);
        }
    }

   
    
}