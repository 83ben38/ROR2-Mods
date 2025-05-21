using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using UnityEngine;
using CharacterBody = On.RoR2.CharacterBody;
using Random = UnityEngine.Random;


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
    private ConfigEntry<bool> workOnEnemies;
    private static List<SkillDef> primaries = new();
    private static List<SkillDef> secondaries = new();
    private static List<SkillDef> utilites = new();
    private static List<SkillDef> specials = new();
    private static List<SkillDef> all = new();
    public AbilityRandomizer()
    {
        Log.Init(Logger);
        keepSlot = Config.Bind("","Stay in slot",true,"Whether the abilities stay in the slot they are in.");
        workOnEnemies = Config.Bind("", "Work On Enemies", false, "Whether enemy abilities are randomized.");
        CharacterBody.OnSkillActivated += CharacterBodyOnOnSkillActivated;
        On.RoR2.SurvivorCatalog.Init += SurvivorCatalogOnInit;
        
        CharacterBody.Start += (orig, self) => {
            orig(self);
            ModelLocator locator = self.GetComponent<ModelLocator>();
            if (locator && locator.modelTransform) {
                GameObject fallback = new("FallbackTransform");
                fallback.transform.SetParent(locator.modelTransform);
            }    
        };
        
        On.ChildLocator.FindChild_string += (orig, self, str) => {
            Transform transform = orig(self, str);
            if (transform) {
                return transform;
            }
        
            List<string> muzzles = new();
            self.transformPairs.ToList().ForEach(x => {
                if (x.name.ToLower().Contains("muzzle")) {
                    muzzles.Add(x.name);
                }
            });
        
            if (muzzles.Count >= 1) {
                string toFind = muzzles[0];
                return orig(self, toFind);
            }
        
            return self.transform.Find("FallbackTransform");
        };
        
        On.ChildLocator.FindChildIndex_string += (orig, self, str) => {
            int c = orig(self, str);
            if (c != -1) {
                return c;
            }
        
            List<string> muzzles = new();
            self.transformPairs.ToList().ForEach(x => {
                if (x.name.ToLower().Contains("muzzle")) {
                    muzzles.Add(x.name);
                }
            });
        
            if (muzzles.Count >= 1) {
                string toFind = muzzles[0];
                return orig(self, toFind);
            }
            return -1;
        };
        CharacterBody.Start += (orig, self) => {
                orig(self);
                ModelLocator locator = self.GetComponent<ModelLocator>();
                if (locator && locator.modelTransform) {
                    GameObject defHitbox = new("DefaultSSHitbox");
                    BoxCollider collider = defHitbox.AddComponent<BoxCollider>();
                    collider.size = new Vector3(240, 180, 240);
                    HitBox hitbox = defHitbox.AddComponent<HitBox>();
                    defHitbox.layer = LayerIndex.triggerZone.intVal;
                    collider.isTrigger = true;
                    defHitbox.transform.SetParent(locator.modelTransform);
                    defHitbox.transform.position = locator.modelTransform.position;
                    defHitbox.transform.localPosition = new Vector3(0, 1, 1.5f);
                    defHitbox.transform.localScale *= 3.5f;
        
        
                    HitBoxGroup group = locator.modelTransform.gameObject.AddComponent<HitBoxGroup>();
                    group.groupName = "DefaultSSGroup";
                    group.hitBoxes = new HitBox[] { hitbox };
                }    
        };
        
            On.EntityStates.BaseState.FindHitBoxGroup += (orig, self, str) => {
                HitBoxGroup group = orig(self, str);
                if (group) {
                    return group;
                }
                
                return orig(self, "DefaultSSGroup");
            };
        
            On.RoR2.OverlapAttack.Fire += (orig, self, res) => {
                if (self.hitBoxGroup == null) {
                    if (self.attacker) {
                        RoR2.CharacterBody body = self.attacker.GetComponent<RoR2.CharacterBody>();
                        if (body && body.modelLocator && body.modelLocator.modelTransform) {
                            HitBoxGroup[] groups = body.modelLocator.modelTransform.GetComponents<HitBoxGroup>();
                            self.hitBoxGroup = Array.Find(groups, x => x.groupName == "DefaultSSGroup");
                            Debug.Log(self.hitBoxGroup.groupName);
                        }
                    }
                }
        
                return orig(self, res);
            };
            CharacterBody.Start += Components;
    }
    

    private void Components(CharacterBody.orig_Start orig, RoR2.CharacterBody self)
    {
        orig(self);
        HuntressTracker tracker = self.GetComponent<HuntressTracker>();
        if (!tracker) {
            tracker = self.gameObject.AddComponent<HuntressTracker>();
            tracker.maxTrackingDistance = 60;
            tracker.maxTrackingAngle = 30;
            tracker.trackerUpdateFrequency = 10;
        }
        
        SeekerController sc = self.GetComponent<SeekerController>();
        if (!sc)
        {
            sc = self.gameObject.AddComponent<SeekerController>();
            sc.skillLocator = self.skillLocator;
            sc.characterBody = self;
        }
    }

    private void SurvivorCatalogOnInit(On.RoR2.SurvivorCatalog.orig_Init orig)
    {
        orig();
        foreach (SurvivorDef survivor in SurvivorCatalog.survivorDefs) {
            if (survivor.survivorIndex == SurvivorCatalog.FindSurvivorIndex("Chef"))
            {
                continue;
            }
            GameObject prefab = survivor.bodyPrefab;
            SkillLocator locator = prefab.GetComponent<SkillLocator>();
            if (survivor.survivorIndex != SurvivorCatalog.FindSurvivorIndex("FalseSon"))
            {
                CollectSkills(locator.primary.skillFamily, ref primaries);
            }

            if (survivor.survivorIndex != SurvivorCatalog.FindSurvivorIndex("Railgunner"))
            {
                CollectSkills(locator.secondary.skillFamily, ref secondaries);
            }

            if (survivor.survivorIndex != SurvivorCatalog.FindSurvivorIndex("Engi") && survivor.survivorIndex != SurvivorCatalog.FindSurvivorIndex("Captain"))
            {
                CollectSkills(locator.utility.skillFamily, ref utilites);
            }

            if (survivor.survivorIndex != SurvivorCatalog.FindSurvivorIndex("Captain") && survivor.survivorIndex != SurvivorCatalog.FindSurvivorIndex("Toolbot") && survivor.survivorIndex != SurvivorCatalog.FindSurvivorIndex("Railgunner"))
            {
                CollectSkills(locator.special.skillFamily, ref specials);
            }
            CollectMachines(survivor.bodyPrefab);
        }

        foreach (SurvivorDef survivor in ContentManager.survivorDefs)
        {
            GameObject prefab = survivor.bodyPrefab;
            SetupStateMachines(prefab);
        }
    }

        internal static void SetupStateMachines(GameObject survivor)
        {
            foreach (StateMachine machine in machines) {
                if (!HasMachine(survivor, machine.name)) {
                    NetworkStateMachine nsm = survivor.GetComponent<NetworkStateMachine>();
                    EntityStateMachine esm = survivor.AddComponent<EntityStateMachine>();
                    esm.customName = machine.name;
                    esm.initialStateType = machine.initial;
                    esm.mainStateType = machine.main;
                    List<EntityStateMachine> esms = nsm.stateMachines.ToList();
                    esms.Add(esm);
                    nsm.stateMachines = esms.ToArray();
                }
            }
        }
        internal static bool HasMachine(GameObject survivor, string name)
        {
            foreach (EntityStateMachine machine in survivor.GetComponents<EntityStateMachine>())
            {
                if (machine.customName != null && machine.customName == name)
                {
                    if (machine.customName == "Body") {
                        machine.initialStateType = new(typeof(EntityStates.Mage.MageCharacterMain));
                        machine.mainStateType = new(typeof(EntityStates.Mage.MageCharacterMain));
                    } 
                    return true;
                }
            }
            return false;
        }
    private void CharacterBodyOnOnSkillActivated(CharacterBody.orig_OnSkillActivated orig, RoR2.CharacterBody self, GenericSkill skill)
    {
        orig(self, skill);
        if (!workOnEnemies.Value && !self.isPlayerControlled)
        {
            return;
        }

        if (self.skillLocator)
        {
            if (self.skillLocator.primary == skill)
            {
                self.skillLocator.primary.skillDef = keepSlot.Value ? primaries[(int)(primaries.Count *Random.value)] : all[(int)(all.Count *Random.value)];
                self.skillLocator.primary.cooldownOverride = self.skillLocator.primary.skillDef.baseRechargeInterval;
                if (self.skillLocator.primary.skillDef is HuntressTrackingSkillDef)
                {
                    self.skillLocator.primary.skillInstanceData = new HuntressTrackingSkillDef.InstanceData()
                    {
                        huntressTracker = self.gameObject.GetComponent<HuntressTracker>()
                    };
                }
            }
            else if (self.skillLocator.secondary == skill)
            {
                self.skillLocator.secondary.skillDef = keepSlot.Value ? secondaries[(int)(secondaries.Count *Random.value)] : all[(int)(all.Count *Random.value)];
                self.skillLocator.secondary.cooldownOverride = self.skillLocator.secondary.skillDef.baseRechargeInterval;
                if (self.skillLocator.secondary.skillDef is HuntressTrackingSkillDef)
                {
                    self.skillLocator.secondary.skillInstanceData = new HuntressTrackingSkillDef.InstanceData()
                    {
                        huntressTracker = self.gameObject.GetComponent<HuntressTracker>()
                    };
                }
            }
            else if (self.skillLocator.utility == skill)
            {
                self.skillLocator.utility.skillDef = keepSlot.Value ? utilites[(int)(utilites.Count *Random.value)] : all[(int)(all.Count *Random.value)];
                self.skillLocator.utility.cooldownOverride = self.skillLocator.utility.skillDef.baseRechargeInterval;
                if (self.skillLocator.utility.skillDef is MercDashSkillDef)
                {
                    self.skillLocator.utility.skillInstanceData = new MercDashSkillDef.InstanceData();
                }
                if (self.skillLocator.utility.skillDef is SteppedSkillDef)
                {
                    self.skillLocator.utility.skillInstanceData = new SteppedSkillDef.InstanceData();
                }
            }
            else if (self.skillLocator.special == skill)
            {
                self.skillLocator.special.skillDef = keepSlot.Value ? specials[(int)(specials.Count *Random.value)] : all[(int)(all.Count *Random.value)];
                self.skillLocator.special.cooldownOverride = self.skillLocator.special.skillDef.baseRechargeInterval;
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
    private struct StateMachine {
        public string name;
        public SerializableEntityStateType initial;
        public SerializableEntityStateType main;
    }
    private static List<StateMachine> machines = new();
    internal static void CollectMachines(GameObject survivor)
    {
        foreach (EntityStateMachine machine in survivor.GetComponents<EntityStateMachine>())
        {
            if (!ContainsMachine(machine))
            {
                StateMachine m = new();
                m.name = machine.customName;
                m.initial = machine.initialStateType;
                m.main = machine.mainStateType;
                if (m.name == "Body")
                {
                    m.initial = new(typeof(EntityStates.Mage.MageCharacterMain));
                    m.main = m.initial;
                }

                machines.Add(m);
            }
            static bool ContainsMachine(EntityStateMachine machine) {
                if (machine.customName == null) {
                    return true;
                }

                foreach (StateMachine m in machines) {
                    if (m.name == machine.customName) {
                        return true;
                    }
                }

                return false;
            }
        }
    }

}