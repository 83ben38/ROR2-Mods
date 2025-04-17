using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace DarknessExpansion;

public class DarknessShrine
{
    private GameObject shrine1 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineBoss/mdlShrineBoss.fbx").WaitForCompletion();
    private Material darkMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/ShrineBlood/matShrineBlood.mat").WaitForCompletion();
    public DarknessShrine()
    {
        shrine1.name = "Shrine of Darkness";
        shrine1.transform.localScale *= 1.5f;
        shrine1.AddComponent<NetworkIdentity>();
        shrine1.transform.GetComponent<MeshRenderer>().sharedMaterial = darkMaterial;
        shrine1.AddComponent<MeshCollider>();
        PurchaseInteraction interaction = shrine1.AddComponent<PurchaseInteraction>();
        interaction.contextToken = "Reckon with the powers of Darkness (E)";
        interaction.NetworkdisplayNameToken = "Shrine of Darkness";
        interaction.enabled = true;
        DarknessShrineManager dsm = shrine1.AddComponent<DarknessShrineManager>();
        dsm.purchaseInteraction = interaction;
        shrine1.GetComponent<Highlight>().targetRenderer = shrine1.GetComponentInChildren<MeshRenderer>();

        GameObject trigger = Object.Instantiate(new GameObject("Trigger"), shrine1.transform);
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
        card.skipSpawnWhenSacrificeArtifactEnabled = false;
        DirectorCard dc = new DirectorCard();
        dc.selectionWeight = 500;
        dc.spawnCard = card;
        DirectorAPI.DirectorCardHolder cardHolder = new DirectorAPI.DirectorCardHolder();
        cardHolder.Card = dc;
        cardHolder.InteractableCategory = DirectorAPI.InteractableCategory.Shrines;
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
            purchaseInteraction.SetAvailable(true);
            purchaseInteraction.onPurchase.AddListener(OnPurchase);
        }

        private void Update()
        {
            purchaseInteraction.SetAvailableTrue();
        }

        public void OnPurchase(Interactor interactor)
        {
            EffectManager.SpawnEffect(shrineUseEffect,new EffectData(){
                origin = gameObject.transform.position,
                rotation = Quaternion.identity,
                scale = 1f,
                color = Color.black
            },true);
        }
    }
    
}