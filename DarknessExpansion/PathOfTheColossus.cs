using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using SceneDirector = On.RoR2.SceneDirector;
using SceneExitController = On.RoR2.SceneExitController;
using SceneExitControllerColossusPortal = On.RoR2.SceneExitControllerColossusPortal;

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
        SceneExitControllerColossusPortal.Begin += SceneExitControllerColossusPortalOnBegin;
        On.RoR2.Run.PickNextStageScene += (orig, self, choices) =>
        {
            Log.Debug("Here2");
            orig(self, choices);
        };
        SceneExitController.Begin += (orig, self) =>
        {
            Log.Debug("Here3");
            orig(self);
        };
    }
    

    private void SceneExitControllerColossusPortalOnBegin(SceneExitControllerColossusPortal.orig_Begin orig, RoR2.SceneExitControllerColossusPortal self)
    {
        
        Log.Debug("Here");
        SceneDef nextStageScene = null;
        if (!startOverAfterLooping.Value && Run.instance.stageClearCount >= 5)
        {
            nextStageScene = SceneCatalog.FindSceneDef("meridian");
        }
        else
        {
            int n = Run.instance.stageClearCount % 5;
            if (n == 0)
            {
                nextStageScene = SceneCatalog.FindSceneDef(goAfterStage1.Value);
            }
            if (n == 1)
            {
                nextStageScene = SceneCatalog.FindSceneDef(goAfterStage2.Value);
            }
            if (n == 2)
            {
                nextStageScene = SceneCatalog.FindSceneDef(goAfterStage3.Value);
            }
            if (n == 3)
            {
                nextStageScene = SceneCatalog.FindSceneDef(goAfterStage4.Value);
            }
            if (n == 4)
            {
                nextStageScene = SceneCatalog.FindSceneDef(goAfterStage5.Value);
            }
        }

        WeightedSelection<SceneDef> toPick = new WeightedSelection<SceneDef>();
        toPick.AddChoice(nextStageScene,1);
        Run.instance.PickNextStageScene(toPick);
        SceneCatalog.mostRecentSceneDef = Run.instance.nextStageScene;
        orig(self);
    }
}