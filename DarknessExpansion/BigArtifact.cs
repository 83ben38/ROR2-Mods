using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DarknessExpansion;

public class BigArtifact
{
    public static ArtifactDef bigArtifact;
    public static Sprite iconDisabled = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texBuffAffixEarth.tif").WaitForCompletion();
    public static Sprite iconEnabled = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
    public BigArtifact()
    {
        bigArtifact = ScriptableObject.CreateInstance<ArtifactDef>();
        bigArtifact.nameToken = "Artifact of Size";
        bigArtifact.descriptionToken = "Everything is Bigger";
        bigArtifact.smallIconDeselectedSprite = iconDisabled;
        bigArtifact.smallIconSelectedSprite = iconEnabled;
        ContentAddition.AddArtifactDef(bigArtifact);
        SpawnCard.onSpawnedServerGlobal += SpawnCardOnonSpawnedServerGlobal;
    }

    private void SpawnCardOnonSpawnedServerGlobal(SpawnCard.SpawnResult obj)
    {
        if (RunArtifactManager.instance.IsArtifactEnabled(bigArtifact))
        {
            obj.spawnedInstance.transform.localScale *= 3;
            CharacterMaster cm = obj.spawnedInstance.GetComponent<CharacterMaster>();
            if (cm != null)
            {
                cm.GetBody().transform.localScale *= 3;
            }
        }
    }
}