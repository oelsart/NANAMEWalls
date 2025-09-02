using Verse;

namespace NanameWalls;

public class Settings : ModSettings
{
    public Dictionary<string, MeshSettings> meshSettings = [];

    public bool groupNanameWalls = Default.groupNanameWalls;

    public bool linkWithDifferentWall = Default.linkWithDifferentWall;

    public bool renderSubstructure = Default.renderSubstructure;
    public override void ExposeData()
    {
        Scribe_StringKeyDictionary.Look(ref meshSettings, "meshSettings", LookMode.Deep);
        Scribe_Values.Look(ref groupNanameWalls, "groupNanameWalls", Default.groupNanameWalls);
        Scribe_Values.Look(ref linkWithDifferentWall, "linkWithDifferentWall", Default.linkWithDifferentWall);
        Scribe_Values.Look(ref renderSubstructure, "renderSubstructure", Default.renderSubstructure);
    }

    public static class Default
    {
        public const bool groupNanameWalls = true;

        public const bool linkWithDifferentWall = true;

        public const bool renderSubstructure = true;
    }
}