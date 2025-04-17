using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DarknessExpansion;

public class DarknessArtifact
{
    public static ArtifactDef darknessArtifact;
    public static Sprite iconDisabled = Addressables.LoadAssetAsync<Sprite>("").WaitForCompletion();
    public static Sprite iconEnabled = Addressables.LoadAssetAsync<Sprite>("").WaitForCompletion();
    public DarknessArtifact()
    {
        darknessArtifact = ScriptableObject.CreateInstance<ArtifactDef>();
        darknessArtifact.nameToken = "Artifact of Darkness";
        darknessArtifact.descriptionToken = "Start with the world partially consumed with darkness.";
        darknessArtifact.smallIconDeselectedSprite = iconDisabled;
        darknessArtifact.smallIconSelectedSprite = iconEnabled;
        ContentAddition.AddArtifactDef(darknessArtifact);
        Run.onRunStartGlobal += RunOnonRunStartGlobal;
    }
    


    private void RunOnonRunStartGlobal(Run obj)
    {
        
        if (RunArtifactManager.instance.IsArtifactEnabled(darknessArtifact))
        {
            Darkness.DarknessLevel += 3;
            Darkness.UpdateDarkness();
            foreach (var cm in CharacterMaster.instancesList)
            {
                List<ItemIndex> li = ItemCatalog.tier3ItemList;
                if (cm._teamIndex == TeamIndex.Player)
                {
                    cm.inventory.GiveItem(li[(int)(li.Count * Random.value)]);
                }
            }
        }
    }

}