using Verse;

namespace NanameWalls;

public class Settings : ModSettings
{
    public Dictionary<string, bool> enabled = [];

    public Dictionary<string, MeshSettings> meshSettings = [];

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref enabled, "enabled", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref meshSettings, "meshSettings", LookMode.Value, LookMode.Deep);
    }
}
