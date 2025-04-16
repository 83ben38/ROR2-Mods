using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DarknessExpansion;

public class DarknessShrine
{
    private GameObject shrine1 = Addressables.LoadAssetAsync<GameObject>("").WaitForCompletion();
    public DarknessShrine()
    {
        
    }
}