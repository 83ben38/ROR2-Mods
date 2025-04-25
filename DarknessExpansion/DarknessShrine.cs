using System;
using BepInEx;
using On.EntityStates.VoidInfestor;
using R2API;
using Rewired;
using RoR2;
using RoR2.Navigation;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using CameraRigController = On.RoR2.CameraRigController;
using Object = System.Object;
using PlayerController = UnityEngine.Networking.PlayerController;
using Random = UnityEngine.Random;
using RunReport = On.RoR2.RunReport;


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
    public DarknessShrine()
    {
        shrine1.name = "Shrine of Darkness";
        shrine1.transform.localScale *= 1.5f;
        shrine1.AddComponent<NetworkIdentity>();
        shrine1.GetComponent<MeshRenderer>().sharedMaterial = darkMaterial;
        shrine1.AddComponent<MeshCollider>();

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
        card.directorCreditCost = 20;
        card.occupyPosition = true;
        card.orientToFloor = false;
        card.skipSpawnWhenSacrificeArtifactEnabled = false;
        card.maxSpawnsPerStage = 1;

        DirectorCard dc = new DirectorCard()
        {
            selectionWeight = 500,
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
    }

    private void SpawnCardOnonSpawnedServerGlobal(SpawnCard.SpawnResult obj)
    {
        DarknessShrineManager dsm = obj.spawnedInstance.GetComponent<DarknessShrineManager>();
        if (dsm != null)
        {
            Log.Debug("Darkness Shrine Found");
            dsm.transform.Rotate(-90,-90,-90);
            dsm.purchaseInteraction.SetAvailable(true);
            dsm.createPotentials(shrine2,spawnCard);
        }

        DarknessPotentialManager dpm = obj.spawnedInstance.GetComponent<DarknessPotentialManager>();
        if (dpm != null)
        {
            Log.Debug("Darkness Potential Found");
            dpm.transform.Rotate(-90,-90,-90);
            dpm.purchaseInteraction.SetAvailable(true);
        }
    }


    public class DarknessShrineManager : NetworkBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        private GameObject shrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab").WaitForCompletion();
        private GameObject[] terminalGameObjects;
        public void Start()
        {
            purchaseInteraction.SetAvailableTrue();
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
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage(){baseToken = "<style=cEvent><color=#000000>Darkness Increased!</color></style>"});
            purchaseInteraction.SetAvailable(false);
            Darkness.DarknessLevel++;
            Darkness.UpdateDarkness();
        }
    }

    public class DarknessPotentialManager : NetworkBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public DarknessShrineManager parent;
        public NetworkUIPromptController networkUIPromptController;
        public GameObject UIObject;
        public InspectPanelController panelController;
        public int frames = 0;
        public void Start()
        {
            purchaseInteraction.SetAvailableTrue();
            purchaseInteraction.onPurchase.AddListener(OnPurchase);
            networkUIPromptController = gameObject.AddComponent<NetworkUIPromptController>();
            networkUIPromptController.onDisplayBegin += onDisplayBegin;
            networkUIPromptController.onDisplayEnd += onDisplayEnd;
        }

        private void onDisplayEnd(NetworkUIPromptController arg1, LocalUser arg2, RoR2.CameraRigController arg3)
        {
            Log.Debug("Display Ending");
            Destroy(UIObject);
            UIObject = null;
            panelController = null;
        }

        private void onDisplayBegin(NetworkUIPromptController arg1, LocalUser arg2, RoR2.CameraRigController arg3)
        {
            Log.Debug("Display Starting");
            UIObject = Instantiate(itemSelectionScreen, arg3.hud.mainContainer.transform);
            panelController = UIObject.GetComponent<ScrapperInfoPanelHelper>().inspectPanelController;
            frames = 0;
            foreach (var item in arg2.cachedMaster.inventory.itemAcquisitionOrder)
            {
                Log.Debug("Attempting to add an item to the screen.");
                //figure out how to add the items
                panelController.Show(PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(item)),WithSidecar: false, incomingUserProfile: null);
            }
        }

        public void OnPurchase(Interactor interactor)
        { 
            networkUIPromptController.SetParticipantMasterFromInteractor(interactor);
        }

        private void FixedUpdate()
        {
            CharacterMaster currentParticipantMaster = networkUIPromptController.currentParticipantMaster;
            if (currentParticipantMaster)
            {
                CharacterBody body = currentParticipantMaster.GetBody();
                if (!body || (body.inputBank.aimOrigin - transform.position).sqrMagnitude > 10f)
                {
                    networkUIPromptController.SetParticipantMaster(null);
                }
            }

            if (!UIObject)
            {
                networkUIPromptController.SetParticipantMaster(null);
            }
        }
    }

}