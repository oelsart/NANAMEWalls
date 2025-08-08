using Verse;

namespace NanameWalls;

public class Settings : ModSettings
{
    public Dictionary<string, MeshSettings> meshSettings = [];

    public bool groupNanameWalls = Default.groupNanameWalls;

    public override void ExposeData()
    {
        Scribe_StringKeyDictionary.Look(ref meshSettings, "meshSettings", LookMode.Deep);
        Scribe_Values.Look(ref groupNanameWalls, "groupNanameWalls", Default.groupNanameWalls);
    }

    public static class Default
    {
        public const bool groupNanameWalls = true;
    }
}