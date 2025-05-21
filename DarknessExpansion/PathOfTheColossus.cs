using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SceneExitController = On.RoR2.SceneExitController;
using TeleporterInteraction = On.RoR2.TeleporterInteraction;

namespace DarknessExpansion;
[BepInPlugin("com.cybug.ColossusPath", "ColossusPath","1.0.0")]
[BepInDependency(ItemAPI.PluginGUID)]
[BepInDependency(EliteAPI.PluginGUID)]
[BepInDependency(LanguageAPI.PluginGUID)]
[BepInDependency(DirectorAPI.PluginGUID)] 
[BepInDependency(RecalculateStatsAPI.PluginGUID)]

public class PathOfTheColossus : BaseUnityPlugin
{
    private ConfigEntry<string> goAfterStage1;
    private ConfigEntry<string> goAfterStage2;
    private ConfigEntry<string> goAfterStage3;
    private ConfigEntry<string> goAfterStage4;
    private ConfigEntry<string> goAfterStage5;
    private ConfigEntry<bool> startOverAfterLooping;
    private ConfigEntry<bool> blacklistStages;
    private InteractableSpawnCard portalSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/DLC2/iscColossusPortal.asset").WaitForCompletion();
    private List<SceneDef> stages = new ();
    public PathOfTheColossus()
    {
        Log.Init(Logger);
        goAfterStage1 = Config.Bind("", "Stage 1 Leads to", "lemuriantemple", "Where the green portal leads after stage 1.");
        goAfterStage2 = Config.Bind("", "Stage 2 Leads to", "habitat", "Where the green portal leads after stage 2.");
        goAfterStage3 = Config.Bind("", "Stage 3 Leads to", "goldshores", "Where the green portal leads after stage 3.");
        goAfterStage4 = Config.Bind("", "Stage 4 Leads to", "helminthroost", "Where the green portal leads after stage 4.");
        goAfterStage5 = Config.Bind("", "Stage 5 Leads to", "meridian", "Where the green portal leads after stage 5.");
        startOverAfterLooping = Config.Bind("", "Start over After Looping", false,
            "Whether the stage number resets after stage 5. If set to false, all green portals after looping will lead to prime meridian.");
        blacklistStages = Config.Bind("", "Blacklist Stages", false,
            "Whether chosen stages will not be able to show up outside of the path.");
        SceneExitController.SetState += SceneExitControllerOnSetState;
        TeleporterInteraction.AttemptToSpawnAllEligiblePortals += TeleporterInteractionOnAttemptToSpawnAllEligiblePortals;
        On.RoR2.Run.PickNextStageSceneFromCurrentSceneDestinations += RunOnPickNextStageSceneFromCurrentSceneDestinations;
        stages.Add(SceneCatalog.FindSceneDef(goAfterStage1.Value));
        stages.Add(SceneCatalog.FindSceneDef(goAfterStage2.Value));
        stages.Add(SceneCatalog.FindSceneDef(goAfterStage3.Value));
        stages.Add(SceneCatalog.FindSceneDef(goAfterStage4.Value));
        stages.Add(SceneCatalog.FindSceneDef(goAfterStage5.Value));
        On.RoR2.Run.AdvanceStage += RunOnAdvanceStage;
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            PlayerCharacterMasterController.instances[0].master.GiveMoney(100);
        }
    }

    private void RunOnAdvanceStage(On.RoR2.Run.orig_AdvanceStage orig, Run self, SceneDef nextscene)
    {
        if (SceneCatalog.GetSceneDefForCurrentScene()?.baseSceneName == "goldshores")
        {
            Log.Debug("Here");
            self.NetworkstageClearCount = self.stageClearCount + 1;
        }

        orig(self, nextscene);
    }

    private void RunOnPickNextStageSceneFromCurrentSceneDestinations(On.RoR2.Run.orig_PickNextStageSceneFromCurrentSceneDestinations orig, Run self)
    {
        
        WeightedSelection<SceneDef> weightedSelection = new WeightedSelection<SceneDef>(8);
        SceneCatalog.mostRecentSceneDef.AddDestinationsToWeightedSelection(weightedSelection, def => CanPickStage(def,self));
        self.PickNextStageScene(weightedSelection);
    }

    public bool CanPickStage(SceneDef sceneDef, Run self)
    {
        if (!self.CanPickStage(sceneDef))
        {
            return false;
        }

        if (blacklistStages.Value && stages.Contains(sceneDef))
        {
            return false;
        }

        return true;
    }

    private void TeleporterInteractionOnAttemptToSpawnAllEligiblePortals(TeleporterInteraction.orig_AttemptToSpawnAllEligiblePortals orig, RoR2.TeleporterInteraction self)
    {
        orig(self);
        if (shouldSpawnPortal)
        {
            Log.Debug("Spawning portal");
            self.AttemptSpawnPortal(portalSpawnCard, 10f, 40f, "PORTAL_COLOSSUS_OPEN");
            shouldSpawnPortal = false;
        }
    }
    

    private bool shouldSpawnPortal = false;
    private bool shouldIncrementStageCount = false;
    
    private void SceneExitControllerOnSetState(SceneExitController.orig_SetState orig, RoR2.SceneExitController self, RoR2.SceneExitController.ExitState newstate)
    {
        orig(self, newstate);
        if (newstate == RoR2.SceneExitController.ExitState.Finished)
        {
            if (self.isColossusPortal)
            {
                string nextStageScene = null;
                if (!startOverAfterLooping.Value && Run.instance.stageClearCount >= 5)
                {
                    nextStageScene = "meridian";
                }
                else
                {
                    int n = Run.instance.stageClearCount % 5;
                    if (n == 0)
                    {
                        nextStageScene = goAfterStage1.Value;
                    }
                    if (n == 1)
                    {
                        nextStageScene = goAfterStage2.Value;
                    }
                    if (n == 2)
                    {
                        nextStageScene = goAfterStage3.Value;
                    }
                    if (n == 3)
                    {
                        nextStageScene = goAfterStage4.Value;
                    }
                    if (n == 4)
                    {
                        nextStageScene = goAfterStage5.Value;
                    }
                }
                Log.Debug("Switching Stage to " + nextStageScene);
                if (nextStageScene == "gildedcoast")
                {
                    shouldIncrementStageCount = true;
                }
                self.destinationScene = SceneCatalog.FindSceneDef(nextStageScene);
                Log.Debug(self.useRunNextStageScene);
                if (self.useRunNextStageScene)
                {
                    self.useRunNextStageScene = false;
                }

                shouldSpawnPortal = true;
                Stage.instance.BeginAdvanceStage(self.destinationScene);
            }
        }
    }

    
}