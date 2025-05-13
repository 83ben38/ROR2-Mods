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
        Run.onRunStartGlobal += RunOnonRunStartGlobal;
    }
    


    private void RunOnonRunStartGlobal(Run obj)
    {
        if (RunArtifactManager.instance.IsArtifactEnabled(deflationArtifact))
        {
            
        }
    }

}