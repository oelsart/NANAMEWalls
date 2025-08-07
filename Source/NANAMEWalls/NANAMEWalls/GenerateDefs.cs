using HarmonyLib;
using RimWorld;
using Verse;

namespace NanameWalls;

[StaticConstructorOnStartup]
public static class GenerateDefs
{
    private delegate void GetGiveShortHash(Def def, Type defType, HashSet<ushort> takenHashes);

    private delegate ThingDef GetNewBlueprintDef_Thing(ThingDef def, bool isInstallBlueprint, ThingDef normalBlueprint = null, bool hotReload = false);

    private delegate ThingDef GetNewFrameDef_Thing(ThingDef def, bool hodReload = false);

    static GenerateDefs()
    {
        static bool IsWallProbably(ThingDef def)
        {
            return (def.IsWall || def.defName.Contains("Wall")) &&
                def.graphicData?.linkType == LinkDrawerType.CornerFiller &&
                def.Size == IntVec2.One &&
                def.passability == Traversability.Impassable &&
                def.BuildableByPlayer;
        }

        var GiveShortHash = AccessTools.MethodDelegate<GetGiveShortHash>(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash"));
        var NewBlueprintDef_Thing = AccessTools.MethodDelegate<GetNewBlueprintDef_Thing>(AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing"));
        var NewFrameDef_Thing = AccessTools.MethodDelegate<GetNewFrameDef_Thing>(AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing"));
        var takenHashes = AccessTools.StaticFieldRefAccess<Dictionary<Type, HashSet<ushort>>>(typeof(ShortHashGiver), "takenHashesPerDeftype");
        foreach (var wallDef in DefDatabase<ThingDef>.AllDefs.Where(IsWallProbably).ToArray())
        {
            var newDef = new ThingDef();
            foreach (var field in typeof(ThingDef).GetFields())
            {
                if (!field.IsLiteral && field.Name != "cachedLabelCap") field.SetValue(newDef, field.GetValue(wallDef));
            }
            newDef.defName += NanameWalls.Suffix;
            newDef.label = "NAW.Diagonal".Translate() + wallDef.LabelCap;
            newDef.graphicData = new GraphicData();
            newDef.graphicData.CopyFrom(wallDef.graphicData);
            newDef.graphicData.linkType = (LinkDrawerType)217;
            newDef.shortHash = 0;
            GiveShortHash(newDef, typeof(ThingDef), takenHashes[typeof(ThingDef)]);
            newDef.modContentPack = NanameWalls.Mod.Content;
            DefGenerator.AddImpliedDef(newDef);
            var bluePrintDef = NewBlueprintDef_Thing(newDef, false);
            bluePrintDef.shortHash = 0;
            GiveShortHash(bluePrintDef, typeof(ThingDef), takenHashes[typeof(ThingDef)]);
            DefGenerator.AddImpliedDef(bluePrintDef);
            var frameDef = NewFrameDef_Thing(newDef, false);
            frameDef.shortHash = 0;
            GiveShortHash(frameDef, typeof(ThingDef), takenHashes[typeof(ThingDef)]);
            DefGenerator.AddImpliedDef(frameDef);

            var meshSettingsDict = NanameWalls.Mod.Settings.meshSettings;
            if (!meshSettingsDict.TryGetValue(wallDef.defName, out var meshSettings))
            {
                meshSettings = meshSettingsDict[wallDef.defName] = MeshSettings.DeepCopyDefaultFor(wallDef);
            }
            if (!meshSettings.enabled) newDef.designationCategory = null;
            NanameWalls.Mod.designationCategories.Add(wallDef.designationCategory);
            NanameWalls.Mod.nanameWalls[wallDef] = newDef;
        }
        foreach (var designationCategory in NanameWalls.Mod.designationCategories)
        {
            designationCategory.ResolveReferences();
        }
    }
}
