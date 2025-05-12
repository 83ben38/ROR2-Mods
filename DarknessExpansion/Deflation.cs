using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Path = System.IO.Path;

namespace DarknessExpansion;

public class Deflation
{
    public static ArtifactDef deflationArtifact;
    public Deflation(PluginInfo Info)
    {
        deflationArtifact = ScriptableObject.CreateInstance<ArtifactDef>();
        deflationArtifact.nameToken = "Artifact of Deflation";
        deflationArtifact.descriptionToken = "Start with items, but you can't get any more.";
        deflationArtifact.smallIconDeselectedSprite = Addressables.LoadAssetAsync<Sprite>(Path.Combine(Path.GetDirectoryName(Info.Location), "Assets","DeflationDisabled.png")).WaitForCompletion();
        deflationArtifact.smallIconSelectedSprite =  Addressables.LoadAssetAsync<Sprite>(Path.Combine(Path.GetDirectoryName(Info.Location), "Assets","Deflation.png")).WaitForCompletion();
        ContentAddition.AddArtifactDef(deflationArtifact);
        Run.onRunStartGlobal += RunOnonRunStartGlobal;
    }
    


    private void RunOnonRunStartGlobal(Run obj)
    {
        if (RunArtifactManager.instance.IsArtifactEnabled(deflationArtifact))
        {
            
        }
    }

}