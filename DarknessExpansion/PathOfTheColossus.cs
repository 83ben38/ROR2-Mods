using BepInEx;
using BepInEx.Configuration;

namespace DarknessExpansion;

public class PathOfTheColossus : BaseUnityPlugin
{
    private ConfigEntry<string> goAfterStage1;
    private ConfigEntry<string> goAfterStage2;
    private ConfigEntry<string> goAfterStage3;
    private ConfigEntry<string> goAfterStage4;
    private ConfigEntry<string> goAfterStage5;
    private ConfigEntry<bool> startOverAfterLooping;
    public PathOfTheColossus()
    {
        goAfterStage1 = Config.Bind("", "Stage 1 Leads to", "", "Where the green portal leads after stage 1.");
        goAfterStage2 = Config.Bind("", "Stage 2 Leads to", "", "Where the green portal leads after stage 2.");
        goAfterStage3 = Config.Bind("", "Stage 3 Leads to", "", "Where the green portal leads after stage 3.");
        goAfterStage4 = Config.Bind("", "Stage 4 Leads to", "", "Where the green portal leads after stage 4.");
        goAfterStage5 = Config.Bind("", "Stage 5 Leads to", "", "Where the green portal leads after stage 5.");
        startOverAfterLooping = Config.Bind("", "Start over After Looping", false,
            "Whether the stage number resets after stage 5. If set to false, all green portals after looping will lead to prime meridian.");
    }
    

}