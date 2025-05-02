using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DarknessExpansion;

public class BigArtifact
{
    public static ArtifactDef bigArtifact;
    public static Sprite iconDisabled = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteOnly/texArtifactEliteOnlyDisabled.png").WaitForCompletion();
    public static Sprite iconEnabled = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteOnly/texArtifactEliteOnlyEnabled.png").WaitForCompletion();
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
        if (obj.spawnedInstance)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(bigArtifact))
            {
                obj.spawnedInstance.transform.localScale *= 3;
                CharacterMaster cm = obj.spawnedInstance.GetComponent<CharacterMaster>();
                if (cm)
                {
                    if (cm.GetBody())
                    {
                        cm.GetBody().transform.localScale *= 3;
                    }
                }
            }
        }
    }
}