using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Artifacts;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using BossGroup = On.RoR2.BossGroup;
using Path = System.IO.Path;
using SceneDirector = On.RoR2.SceneDirector;
using SceneExitController = On.RoR2.SceneExitController;

namespace DarknessExpansion;

public class Deflation
{
    public static ArtifactDef deflationArtifact;

    public static AssetBundle ab =
        AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(DeflationArtifact.PInfo.Location), "assets"));

    public static Sprite deselected = ab.LoadAsset<Sprite>("DeflationDisabled.png");
    public static Sprite selected = ab.LoadAsset<Sprite>("Deflation.png");
    public Deflation()
    {
        deflationArtifact = ScriptableObject.CreateInstance<ArtifactDef>();
        deflationArtifact.nameToken = "Artifact of Deflation";
        deflationArtifact.descriptionToken = "Start with items, but you can't get any more.";
        deflationArtifact.smallIconDeselectedSprite = deselected;
        deflationArtifact.smallIconSelectedSprite = selected;
        ContentAddition.AddArtifactDef(deflationArtifact);
        On.RoR2.Run.Start += RunOnStart;
        On.RoR2.Run.PickNextStageScene += RunOnPickNextStageScene;
        SceneDirector.Start += SceneDirectorOnStart;
        RoR2.SceneDirector.onPrePopulateSceneServer += SceneDirectorOnonPrePopulateSceneServer;
        RoR2.SceneDirector.onGenerateInteractableCardSelection += SceneDirectorOnonGenerateInteractableCardSelection;
        BossGroup.DropRewards += BossGroupOnDropRewards;
        SceneExitController.Begin += SceneExitControllerOnBegin;
    }

    private void SceneExitControllerOnBegin(SceneExitController.orig_Begin orig, RoR2.SceneExitController self)
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if (baazarVisitsLeft > 0)
        {
            baazarVisitsLeft--;
            if (baazarVisitsLeft == 0)
            {
                WeightedSelection<SceneDef> weightedSelection = new WeightedSelection<SceneDef>();
                if (Run.instance.startingSceneGroup)
                {
                    Run.instance.startingSceneGroup.AddToWeightedSelection(weightedSelection, Run.instance.CanPickStage);
                }
                Run.instance.PickNextStageScene(weightedSelection);
            }
        }
        orig(self);
    }

    private void BossGroupOnDropRewards(BossGroup.orig_DropRewards orig, RoR2.BossGroup self)
    {
        if (RunArtifactManager.instance.IsArtifactEnabled(deflationArtifact))
        {
            return;
        }

        orig(self);
    }

    private void SceneDirectorOnonGenerateInteractableCardSelection(RoR2.SceneDirector arg1,
        DirectorCardCategorySelection arg2)
    {
        if (RunArtifactManager.instance.IsArtifactEnabled(deflationArtifact))
        {
            arg2.RemoveCardsThatFailFilter(card =>
            {
                InteractableSpawnCard interactableSpawnCard = card.spawnCard as InteractableSpawnCard;
                return interactableSpawnCard == null || !interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled;
            });
        }
    }

    private void SceneDirectorOnonPrePopulateSceneServer(RoR2.SceneDirector obj)
    {
        if (RunArtifactManager.instance.IsArtifactEnabled(deflationArtifact))
        {
            obj.onPopulateCreditMultiplier *= 0.2f;
        }
    }

    private void SceneDirectorOnStart(SceneDirector.orig_Start orig, RoR2.SceneDirector self)
    {
        if (fillWithItems)
        {
            
        }
        orig(self);
    }

    private void RunOnPickNextStageScene(On.RoR2.Run.orig_PickNextStageScene orig, Run self, WeightedSelection<SceneDef> choices)
    {
        if (baazarVisitsLeft > 0)
        {
            SceneDef nextStageScene = SceneCatalog.FindSceneDef("bazaar"); 
            self.nextStageScene = nextStageScene;
            fillWithItems = true;
            return;
        }
        fillWithItems = false;
        orig(self, choices);
    }

    private void RunOnStart(On.RoR2.Run.orig_Start orig, Run self)
    {
        if (RunArtifactManager.instance.IsArtifactEnabled(deflationArtifact))
        {
            baazarVisitsLeft = 1;
            RunArtifactManager.instance.SetArtifactEnabled(RoR2Content.Artifacts.Sacrifice,false);
        }
        else
        {
            baazarVisitsLeft = 0;
        }
        orig(self);
    }

    private int baazarVisitsLeft = 0;
    private bool fillWithItems = false;

}