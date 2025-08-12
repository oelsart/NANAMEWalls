using Verse;

namespace NanameWalls;

public class Settings : ModSettings
{
    public Dictionary<string, MeshSettings> meshSettings = [];

    public bool groupNanameWalls = Default.groupNanameWalls;

    public bool linkWithDifferentWall = Default.linkWithDifferentWall;

    public override void ExposeData()
    {
        Scribe_StringKeyDictionary.Look(ref meshSettings, "meshSettings", LookMode.Deep);
        Scribe_Values.Look(ref groupNanameWalls, "groupNanameWalls", Default.groupNanameWalls);
        Scribe_Values.Look(ref linkWithDifferentWall, "linkWithDifferentWall", Default.linkWithDifferentWall);
    }

    public static class Default
    {
        public const bool groupNanameWalls = true;

        public const bool linkWithDifferentWall = true;
    }
}