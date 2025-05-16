using System;
using System.Collections.Generic;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using BossGroup = On.RoR2.BossGroup;
using Path = System.IO.Path;
using Random = UnityEngine.Random;
using SceneDirector = On.RoR2.SceneDirector;
using SceneExitController = On.RoR2.SceneExitController;

namespace DarknessExpansion;

public class Deflation
{
    public static String[] yellowItemNames = new[]
    {
        "BeetleGland",
        "Knurl",
        "NovaOnLowHealth",
        "SprintWisp",
        "BleedOnHitAndExplode",
        "SiphonOnLowHealth",
        "MinorConstructOnKill",
        "ParentEgg",
        "LightningStrikeOnHit",
        "FireballsOnHit",
        "RoboBallBuddy"
    };
    public static ArtifactDef deflationArtifact;

    public static AssetBundle ab =
        AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(DeflationArtifact.PInfo.Location), "assets"));

    public static Sprite deselected1 = ab.LoadAsset<Sprite>("DeflationDisabled1.png");
    public static Sprite selected1 = ab.LoadAsset<Sprite>("Deflation1.png");
    public static Sprite deselected2 = ab.LoadAsset<Sprite>("DeflationDisabled2.png");
    public static Sprite selected2 = ab.LoadAsset<Sprite>("Deflation2.png");
    public Deflation()
    {
        deflationArtifact = ScriptableObject.CreateInstance<ArtifactDef>();
        deflationArtifact.nameToken = "Artifact of Deflation";
        deflationArtifact.descriptionToken = "Start with items, but you can't get any more.";
        deflationArtifact.smallIconDeselectedSprite = DeflationArtifact.useOldIcon.Value ? deselected1 : deselected2;
        deflationArtifact.smallIconSelectedSprite = DeflationArtifact.useOldIcon.Value ? selected1 : selected2;
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
                Run.instance.NetworkstageClearCount = DeflationArtifact.stagesToSkip.Value;
                Run.instance.runStopwatch.offsetFromFixedTime += DeflationArtifact.timeToSkip.Value;
                for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
                {
                    PlayerCharacterMasterController.instances[i].master.GiveExperience(DeflationArtifact.xpToGet.Value);
                }
                WeightedSelection<SceneDef> weightedSelection = new WeightedSelection<SceneDef>();
                Run.instance.startingSceneGroup.AddToWeightedSelection(weightedSelection, Run.instance.CanPickStage);
                Run.instance.PickNextStageScene(weightedSelection);
                for (int i = 0; i < Run.instance.NetworkstageClearCount; i++)
                {
                    SceneCatalog.mostRecentSceneDef = Run.instance.nextStageScene;
                    Run.instance.PickNextStageSceneFromCurrentSceneDestinations();
                }
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
                if (interactableSpawnCard.name.Contains("Halcyonite") || interactableSpawnCard.name.Contains("Chest"))
                {
                    Log.Debug("Blocked One");
                }
                return !(interactableSpawnCard.name.Contains("Halcyonite") || interactableSpawnCard.name.Contains("Chest"));
            });
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
            obj.onPopulateCreditMultiplier *= DeflationArtifact.creditMultiplier.Value;
        }
    }

    private void SceneDirectorOnStart(SceneDirector.orig_Start orig, RoR2.SceneDirector self)
    {
        if (fillWithItems)
        {
            createItem(new List<EquipmentIndex>(){EquipmentCatalog.FindEquipmentIndex("Recycle")},new Vector3(-70,-20,-2));
            List<ItemIndex> whiteItems = ItemCatalog.tier1ItemList;
            int j = 0;
            for (int i = 0; i < DeflationArtifact.whitesToGive.Value; i++)
            {
                createItem(whiteItems,new Vector3(-85 - (j/5) * 5,-20, -10 - (j%5) * 5));
                j++;
            }
            List<ItemIndex> greenItems = ItemCatalog.tier2ItemList;
            for (int i = 0; i < DeflationArtifact.greensToGive.Value; i++)
            {
                createItem(greenItems,new Vector3(-85 - (j/5) * 5,-20, -10 - (j%5) * 5));
                j++;
            }
            List<ItemIndex> redItems = ItemCatalog.tier3ItemList;
            for (int i = 0; i < DeflationArtifact.redsToGive.Value; i++)
            {
                createItem(redItems,new Vector3(-85 - (j/5) * 5,-20, -10 - (j%5) * 5));
                j++;
            }
            List<ItemIndex> yellowItems = new List<ItemIndex>();
            for (int i = 0; i < yellowItemNames.Length; i++)
            {
                yellowItems.Add(ItemCatalog.FindItemIndex(yellowItemNames[i]));
            }
            for (int i = 0; i < DeflationArtifact.yellowsToGive.Value; i++)
            {
                createItem(yellowItems,new Vector3(-85 - (j/5) * 5,-20, -10 - (j%5) * 5));
                j++;
            }

            ItemDef.Pair[] pairs = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem];
            List<ItemIndex> tier1VoidItems = new List<ItemIndex>();
            for (int i = 0; i < pairs.Length; i++)
            {
                if (whiteItems.Contains(pairs[i].itemDef1.itemIndex))
                {
                    tier1VoidItems.Add(pairs[i].itemDef2.itemIndex);
                }
            }
            for (int i = 0; i < DeflationArtifact.voidTier1ToGive.Value; i++)
            {
                createItem(tier1VoidItems,new Vector3(-85 - (j/5) * 5,-20, -10 - (j%5) * 5));
                j++;
            }
            List<ItemIndex> tier2VoidItems = new List<ItemIndex>();
            for (int i = 0; i < pairs.Length; i++)
            {
                if (greenItems.Contains(pairs[i].itemDef1.itemIndex))
                {
                    tier2VoidItems.Add(pairs[i].itemDef2.itemIndex);
                }
            }
            for (int i = 0; i < DeflationArtifact.voidTier2ToGive.Value; i++)
            {
                createItem(tier2VoidItems,new Vector3(-85 - (j/5) * 5,-20, -10 - (j%5) * 5));
                j++;
            }
            List<EquipmentIndex> equipments = EquipmentCatalog.equipmentList;
            for (int i = 0; i < DeflationArtifact.equipmentToGive.Value; i++)
            {
                createItem(equipments,new Vector3(-85 - (j/5) * 5,-20, -10 - (j%5) * 5));
                j++;
            }
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
            baazarVisitsLeft = NetworkUser.instancesList.Count;
            RunArtifactManager.instance.SetArtifactEnabled(RoR2Content.Artifacts.Sacrifice,false);
            RoR2Content.Equipment.Recycle.cooldown = 0f;
        }
        else
        {
            baazarVisitsLeft = 0;
            RoR2Content.Equipment.Recycle.cooldown = 45f;
        }
        orig(self);
    }

    private void createItem(List<ItemIndex> options, Vector3 position)
    {
        ItemIndex option = options[Random.RandomRangeInt(0,options.Count)];
        while (ItemCatalog.GetItemDef(option).tags.Contains(ItemTag.Scrap))
        {
            option = options[Random.RandomRangeInt(0,options.Count)];
        }
        PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(option),position,Vector3.zero);
    }
    private void createItem(List<EquipmentIndex> options, Vector3 position)
    {
        EquipmentIndex option = options[Random.RandomRangeInt(0,options.Count)];
        while (EquipmentCatalog.GetEquipmentDef(option).isLunar)
        {
            option = options[Random.RandomRangeInt(0,options.Count)];
        }
        PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(option),position,Vector3.zero);
    }
    private int baazarVisitsLeft = 0;
    private bool fillWithItems = false;

}