using System;
using System.Collections.Generic;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Navigation;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using Random = UnityEngine.Random;


namespace DarknessExpansion;

public class DarknessShrine
{
    private GameObject shrine1 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineBoss/mdlShrineBoss.fbx").WaitForCompletion().InstantiateClone("Darkness Shrine");
    private Material darkMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/ShrineBlood/matShrineBlood.mat").WaitForCompletion();

    private GameObject shrine2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineBlood/mdlShrineBlood.fbx")
        .WaitForCompletion().InstantiateClone("Darkness Item");

    private static GameObject itemSelectionScreen = Addressables
        .LoadAssetAsync<GameObject>("RoR2/Base/Scrapper/ScrapperPickerPanel.prefab").WaitForCompletion()
        .InstantiateClone("Darkness Item Selector");

    private InteractableSpawnCard spawnCard;

    public static String[] bossLocations = new[]
    {
        "RoR2/Base/Beetle/cscBeetleQueen.asset",
        "RoR2/Base/Titan/cscTitanGolemPlains.asset",
        "RoR2/Base/Vagrant/cscVagrant.asset",
        "RoR2/Base/Gravekeeper/cscGravekeeper.asset",
        "RoR2/Base/ImpBoss/cscImpBoss.asset",
        "RoR2/Base/ClayBoss/cscClayBoss.asset",
        "RoR2/DLC1/MajorAndMinorConstruct/cscMegaConstruct.asset",
        "RoR2/Base/RoboBallBoss/cscRoboBallBoss.asset",
        "RoR2/Base/Grandparent/cscGrandparent.asset",
        "RoR2/Base/ElectricWorm/cscElectricWorm.asset",
        "RoR2/Base/MagmaWorm/cscMagmaWorm.asset",
        "RoR2/Base/Scav/cscScavBoss.asset"
    };

    public static String[] yellowItemNames = new[]
    {
        "BeetleGland",
        "Knurl",
        "NovaOnLowHealth",
        "SprintWisp",
        "BleedOnHitAndExplode",
        "SiphonOnLowHealth",
        "MinorConstructOnKill",
        "RoboBallBuddy",
        "ParentEgg",
        "LightningStrikeOnHit",
        "FireballsOnHit",
        "Pearl",
        "ShinyPearl"
    };


    public static SpawnCard[] bosses;
    public DarknessShrine()
    {
        shrine1.name = "Shrine of Darkness";
        shrine1.transform.localScale *= 1.5f;
        shrine1.AddComponent<NetworkIdentity>();
        shrine1.GetComponent<MeshRenderer>().sharedMaterial = darkMaterial;
        shrine1.AddComponent<MeshCollider>();
        shrine1.AddComponent<CombatDirector>().enabled = false;
        
        DarknessShrineManager dsm = shrine1.AddComponent<DarknessShrineManager>();
        PurchaseInteraction interaction = shrine1.AddComponent<PurchaseInteraction>();
        interaction.contextToken = "Reckon with the powers of Darkness (E)";
        interaction.NetworkdisplayNameToken = "Shrine of Darkness";
        dsm.purchaseInteraction = interaction;
        shrine1.GetComponent<Highlight>().targetRenderer = shrine1.GetComponent<MeshRenderer>();
        GameObject trigger = BaseUnityPlugin.Instantiate(new GameObject(), shrine1.transform);
        trigger.AddComponent<BoxCollider>().isTrigger = true;
        trigger.AddComponent<EntityLocator>().entity = shrine1;

        InteractableSpawnCard card = ScriptableObject.CreateInstance<InteractableSpawnCard>();
        card.name = "iscDarknessShrine";
        card.prefab = shrine1;
        card.sendOverNetwork = true;
        card.hullSize = HullClassification.Golem;
        card.nodeGraphType = MapNodeGroup.GraphType.Ground;
        card.requiredFlags = NodeFlags.None;
        card.forbiddenFlags = NodeFlags.NoShrineSpawn;
        card.directorCreditCost = 10;
        card.occupyPosition = true;
        card.orientToFloor = false;
        card.skipSpawnWhenSacrificeArtifactEnabled = false;
        card.maxSpawnsPerStage = 1;

        DirectorCard dc = new DirectorCard()
        {
            selectionWeight = 100,
            spawnCard = card
        };

        DirectorAPI.DirectorCardHolder cardHolder = new DirectorAPI.DirectorCardHolder()
        {
            Card = dc,
            InteractableCategory = DirectorAPI.InteractableCategory.Shrines
        };
        DirectorAPI.Helpers.AddNewInteractable(cardHolder);
        SpawnCard.onSpawnedServerGlobal += SpawnCardOnonSpawnedServerGlobal;
        
        shrine2.name = "Darkness Potential";
        shrine2.transform.localScale *= 0.005f;
        shrine2.AddComponent<NetworkIdentity>();
        shrine2.GetComponent<Renderer>().sharedMaterial = darkMaterial;
        shrine2.AddComponent<MeshCollider>();
        
        spawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
        spawnCard.name = "iscDarknessPotential";
        spawnCard.prefab = shrine2;
        spawnCard.sendOverNetwork = true;
        spawnCard.hullSize = HullClassification.Human;
        spawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
        spawnCard.requiredFlags = NodeFlags.None;
        spawnCard.forbiddenFlags = NodeFlags.NoShrineSpawn;
        spawnCard.directorCreditCost = 0;
        spawnCard.occupyPosition = true;
        spawnCard.orientToFloor = false;
        spawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;
        spawnCard.maxSpawnsPerStage = 3;

        DarknessPotentialManager dpm = shrine2.AddComponent<DarknessPotentialManager>();
        PurchaseInteraction interaction2 = shrine2.AddComponent<PurchaseInteraction>();
        
        interaction2.contextToken = "Offer a Sacrifice to the Darkness (E)";
        interaction2.NetworkdisplayNameToken = "Darkness Potential";
        dpm.purchaseInteraction = interaction2;
        shrine2.GetComponent<Highlight>().targetRenderer = shrine2.GetComponent<Renderer>();
        GameObject trigger2 = BaseUnityPlugin.Instantiate(new GameObject(), shrine2.transform);
        trigger2.AddComponent<BoxCollider>().isTrigger = true;
        trigger2.AddComponent<EntityLocator>().entity = shrine2;

        bosses = new SpawnCard[bossLocations.Length];
        for (int i = 0; i < bosses.Length; i++)
        {
            bosses[i] = Addressables.LoadAssetAsync<SpawnCard>(bossLocations[i]).WaitForCompletion();
        }
    }

    private void SpawnCardOnonSpawnedServerGlobal(SpawnCard.SpawnResult obj)
    {
        if (obj.spawnedInstance) {
            DarknessShrineManager dsm = obj.spawnedInstance.GetComponent<DarknessShrineManager>();
            if (dsm)
            {
                Log.Debug("Darkness Shrine Found");
                dsm.transform.Rotate(-90,-90,-90);
                dsm.createPotentials(shrine2,spawnCard);
            }

            DarknessPotentialManager dpm = obj.spawnedInstance.GetComponent<DarknessPotentialManager>();
            if (dpm)
            {
                Log.Debug("Darkness Potential Found");
                dpm.transform.Rotate(-90,-90,-90);
                dpm.purchaseInteraction.SetAvailable(true);
            }
        }
    }


    public class DarknessShrineManager : NetworkBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        private GameObject shrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab").WaitForCompletion();
        private GameObject[] terminalGameObjects;
        public List<ItemIndex> sacrificedItems = new List<ItemIndex>();
        private ItemIndex bonusItemToGive;
        public void Start()
        {
            purchaseInteraction.SetAvailable(false);
            purchaseInteraction.onPurchase.AddListener(OnPurchase);
        }

        public void createPotentials(GameObject shrine2, SpawnCard spawnCard)
        {
            terminalGameObjects = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                terminalGameObjects[i] = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard,new DirectorPlacementRule(){placementMode = DirectorPlacementRule.PlacementMode.Random},new Xoroshiro128Plus((ulong)(Random.value * ulong.MaxValue))));
                terminalGameObjects[i].GetComponent<DarknessPotentialManager>().parent = this;
            }
            
        }

        public void OnPurchase(Interactor interactor)
        {
            EffectManager.SpawnEffect(shrineUseEffect,new EffectData(){
                origin = gameObject.transform.position,
                rotation = Quaternion.identity,
                scale = 1f,
                color = Color.black
            },true);
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage(){baseToken = "<style=cEvent><color=#000000>The Darkness Increases!</color></style>"});
            purchaseInteraction.SetAvailable(false);
            Darkness.DarknessLevel++;
            Darkness.UpdateDarkness();
            for (int i = 0; i < 3; i++)
            {
                if (terminalGameObjects[i])
                {
                    DarknessPotentialManager dpm = terminalGameObjects[i].GetComponent<DarknessPotentialManager>();
                    dpm.shrinking = 0;
                    dpm.purchaseInteraction.SetAvailable(false);
                }
            }

            bonusItemToGive = ItemIndex.None;
            int bossToSpawn = -1;
            if (sacrificedItems.Count == 3)
            {
                if (sacrificedItems[0] == sacrificedItems[1] && sacrificedItems[0] == sacrificedItems[2])
                {
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["BeetleGland"])
                    {
                        bossToSpawn = 0;
                        bonusItemToGive = DarknessItems.DarkBeetleItem.darkBeetleItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["Knurl"])
                    {
                        bossToSpawn = 1;
                        bonusItemToGive = DarknessItems.DarkGolemItem.darkGolemItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["NovaOnLowHealth"])
                    {
                        bossToSpawn = 2;
                        bonusItemToGive = DarknessItems.DarkJellyfishItem.darkJellyfishItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["SprintWisp"])
                    {
                        bossToSpawn = 3;
                        bonusItemToGive = DarknessItems.DarkWispItem.darkWispItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["BleedOnHitAndExplode"])
                    {
                        bossToSpawn = 4;
                        bonusItemToGive = DarknessItems.DarkBleedItem.darkBleedItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["SiphonOnLowHealth"])
                    {
                        bossToSpawn = 5;
                        bonusItemToGive = DarknessItems.DarkClayItem.darkClayItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["MinorConstructOnKill"])
                    {
                        bossToSpawn = 6;
                        bonusItemToGive = DarknessItems.DarkConstructItem.darkConstructItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["RoboBallBuddy"])
                    {
                        bossToSpawn = 7;
                        bonusItemToGive = DarknessItems.DarkCoreItem.darkCoreItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["ParentEgg"])
                    {
                        bossToSpawn = 8;
                        bonusItemToGive = DarknessItems.DarkParentItem.darkParentItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["LightningStrikeOnHit"])
                    {
                        bossToSpawn = 9;
                        bonusItemToGive = DarknessItems.DarkLightningItem.darkLightningItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["FireballsOnHit"])
                    {
                        bossToSpawn = 10;
                        bonusItemToGive = DarknessItems.DarkFireItem.darkFireItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["Pearl"])
                    {
                        bonusItemToGive = DarknessItems.DarkPearlItem.darkPearlItem.itemIndex;
                    }
                    if (sacrificedItems[0] == ItemCatalog.itemNameToIndex["ShinyPearl"])
                    {
                        bossToSpawn = 11;
                        bonusItemToGive = DarknessItems.DarkPearlItem2.darkPearlItem.itemIndex;
                    }
                }
            }

            CombatDirector bossSpawner = gameObject.GetComponent<CombatDirector>();
            bossSpawner.monsterCardsSelection = new WeightedSelection<DirectorCard>();
            if (bossToSpawn == -1)
            {
                for (int i = 0; i < bosses.Length; i++)
                {
                    bossSpawner.monsterCardsSelection.AddChoice(new DirectorCard(){spawnCard = bosses[i], selectionWeight = 1},1);
                }
            }
            else
            {
                bossSpawner.monsterCardsSelection.AddChoice(new DirectorCard(){spawnCard = bosses[bossToSpawn], selectionWeight = 1},1);
            }
            bossSpawner.rng = new Xoroshiro128Plus((ulong)(Random.value * ulong.MaxValue));
            CombatSquad squad = gameObject.AddComponent<CombatSquad>();
            bossSpawner.combatSquad = squad;
            bossSpawner.teamIndex = TeamIndex.Monster;
            bossSpawner.currentSpawnTarget = gameObject;
            bossSpawner.ignoreTeamSizeLimit = true;
            bossSpawner.shouldSpawnOneWave = true;
            bossSpawner.enabled = true;
            bossSpawner.monsterCredit +=
                (int)(600f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
            bossSpawner.SetNextSpawnAsBoss();
            

            bossSquad = squad;
        }

        public CombatSquad bossSquad;
        public bool changedEnemies = false;

        private void FixedUpdate()
        {
            if (bossSquad)
            {
                if (!changedEnemies && bossSquad.membersList.Count > 0)
                {
                    List<CharacterMaster> boss = bossSquad.membersList;
                    for (int j = 0; j < boss.Count; j++)
                    {
                        Inventory bossInventory = boss[j].inventory;
                        //boss[j].GetBody().AddOrRemoveEliteItemBehavior(Darkness.DarknessBuff,true);
                        //bossInventory.SetEquipmentIndex(Darkness.DarknessEquipment.equipmentIndex);
                        // for (int i = 0; i < sacrificedItems.Count; i++)
                        // {
                        //     ItemIndex ii = sacrificedItems[i];
                        //     int numItems = 1;
                        //     if (ItemCatalog.tier1ItemList.Contains(ii))
                        //     {
                        //         numItems = 5;
                        //     }
                        //
                        //     if (ItemCatalog.tier2ItemList.Contains(ii))
                        //     {
                        //         numItems = 3;
                        //     }
                        //     bossInventory.GiveItem(ii,numItems);
                        // }
                        //
                        // if (bonusItemToGive != ItemIndex.None)
                        // {
                        //     bossInventory.GiveItem(bonusItemToGive);
                        // }
                        //bossInventory.GiveItemString("ShinyPearl",Darkness.DarknessLevel);
                    }
                }
                if (bossSquad.defeatedServer)
                {
                    //win
                    List<ItemIndex> toGive = new List<ItemIndex>();
                    if (bonusItemToGive != ItemIndex.None)
                    {
                        toGive.Add(bonusItemToGive);
                    }
                    else
                    {
                        for (int i = 0; i < sacrificedItems.Count; i++)
                        {
                            ItemIndex ii = sacrificedItems[i];
                            ItemIndex nii;
                            int itemCount = 1;
                            if (ItemCatalog.tier1ItemList.Contains(ii))
                            {
                                nii = ItemCatalog.tier2ItemList[(int)(ItemCatalog.tier2ItemList.Count * Random.value)];
                            }
                            else if (ItemCatalog.tier2ItemList.Contains(ii))
                            {
                                nii = ItemCatalog.itemNameToIndex[
                                    yellowItemNames[(int)(yellowItemNames.Length * Random.value)]];
                            }
                            else if (ItemCatalog.tier3ItemList.Contains(ii))
                            {
                                nii = ItemCatalog.tier1ItemList[(int)(ItemCatalog.tier1ItemList.Count * Random.value)];
                                itemCount = 5;
                            }
                            else
                            {
                                nii = ItemCatalog.tier3ItemList[(int)(ItemCatalog.tier3ItemList.Count * Random.value)];
                            }
                            for (int j = 0; j < itemCount; j++)
                            {
                                toGive.Add(nii);
                            }
                        }
                    }

                    for (int i = 0; i < toGive.Count; i++)
                    {
                        float degrees = i * 2 * Mathf.PI / toGive.Count;
                        Vector3 direction = new Vector3(Mathf.Sin(degrees),1,Mathf.Cos(degrees));
                        PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(toGive[i]),transform.position + (Vector3.up * 5f),direction*20f);
                    }

                    bossSquad = null;
                }
            }
        }
    }

    public class DarknessPotentialManager : NetworkBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public DarknessShrineManager parent;
        public NetworkUIPromptController networkUIPromptController;
        public PickupPickerController ppc;
        private Interactor interactor;
        public int shrinking = -1;
        public void Start()
        {
            purchaseInteraction.SetAvailableTrue();
            purchaseInteraction.onPurchase.AddListener(OnPurchase);
            networkUIPromptController = gameObject.AddComponent<NetworkUIPromptController>();
            ppc = gameObject.AddComponent<PickupPickerController>();
            networkUIPromptController.onDisplayBegin += ppc.OnDisplayBegin;
            networkUIPromptController.onDisplayEnd += ppc.OnDisplayEnd;
            ppc.panelPrefab = itemSelectionScreen;
            ppc.onPickupSelected = new PickupPickerController.PickupIndexUnityEvent();
            ppc.onPickupSelected.AddListener(OnPickupSelected);
            
        }

        private void OnPickupSelected(int arg0)
        {
            FindObjectOfType<PickupPickerPanel>().DestroyIt();
            ItemIndex ii = PickupCatalog.GetPickupDef(new PickupIndex(arg0)).itemIndex;
            parent.GetComponent<DarknessShrineManager>().sacrificedItems.Add(ii);
            purchaseInteraction.SetAvailable(false);
            Inventory inventory = interactor.GetComponent<CharacterBody>().inventory;
            inventory.RemoveItem(ii);
            shrinking = 0;
            parent.purchaseInteraction.SetAvailableTrue();
        }


        public void OnPurchase(Interactor interactor)
        {
            this.interactor = interactor;
            ppc.SetOptionsFromInteractor(interactor);
            networkUIPromptController.SetParticipantMasterFromInteractor(interactor);
        }

        private void FixedUpdate()
        {
            if (shrinking > -1)
            {
                shrinking++;
                gameObject.transform.localScale *= 0.95f;
                if (shrinking > 30)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

}