using Verse;

namespace NanameWalls;

public class Settings : ModSettings
{
    public Dictionary<string, MeshSettings> meshSettings = [];

    public override void ExposeData()
    {
        Scribe_StringKeyDictionary.Look(ref meshSettings, "meshSettings", LookMode.Deep);
    }
}