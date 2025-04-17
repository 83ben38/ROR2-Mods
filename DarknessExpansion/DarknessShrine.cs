using BepInEx;
using R2API;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;


namespace DarknessExpansion;

public class DarknessShrine
{
    private GameObject shrine1 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineBoss/mdlShrineBoss.fbx").WaitForCompletion().InstantiateClone("Darkness Shrine");
    private Material darkMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/ShrineBlood/matShrineBlood.mat").WaitForCompletion();

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
        
    }

    private void SpawnCardOnonSpawnedServerGlobal(SpawnCard.SpawnResult obj)
    {
        DarknessShrineManager dsm = obj.spawnedInstance.GetComponent<DarknessShrineManager>();
        if (dsm != null)
        {
            Log.Debug("Darkness Shrine Found");
            dsm.transform.Rotate(-90,-90,-90);
            dsm.purchaseInteraction.SetAvailable(true);
        }
    }


    public class DarknessShrineManager : NetworkBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        private GameObject shrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab").WaitForCompletion();

        public void Start()
        {
            purchaseInteraction.SetAvailableTrue();
            purchaseInteraction.onPurchase.AddListener(OnPurchase);
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
    
}