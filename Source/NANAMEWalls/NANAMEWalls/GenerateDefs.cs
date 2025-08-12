using HarmonyLib;
using RimWorld;
using Verse;
using static NanameWalls.ModCompat;

namespace NanameWalls;

[StaticConstructorOnStartup]
public static class GenerateDefs
{
    private delegate void GetGiveShortHash(Def def, Type defType, HashSet<ushort> takenHashes);

    private delegate ThingDef GetNewBlueprintDef_Thing(ThingDef def, bool isInstallBlueprint, ThingDef normalBlueprint = null, bool hotReload = false);

    private delegate ThingDef GetNewFrameDef_Thing(ThingDef def, bool hodReload = false);

    static GenerateDefs()
    {
        static bool IsLinkedThing(ThingDef def)
        {
            var linkType = def.graphicData?.linkType;
            return def.Size == IntVec2.One && (linkType == LinkDrawerType.CornerFiller || linkType == LinkDrawerType.Basic);
        }

        static bool IsWallProbably(ThingDef def)
        {
            return (def.IsWall || (def.defName.Contains("Wall"))) &&
                def.passability == Traversability.Impassable;
        }

        var GiveShortHash = AccessTools.MethodDelegate<GetGiveShortHash>(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash"));
        var NewBlueprintDef_Thing = AccessTools.MethodDelegate<GetNewBlueprintDef_Thing>(AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing"));
        var NewFrameDef_Thing = AccessTools.MethodDelegate<GetNewFrameDef_Thing>(AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing"));
        var takenHashes = AccessTools.StaticFieldRefAccess<Dictionary<Type, HashSet<ushort>>>(typeof(ShortHashGiver), "takenHashesPerDeftype");
        foreach (var wallDef in DefDatabase<ThingDef>.AllDefs.Where(def => IsLinkedThing(def) && def.BuildableByPlayer).ToArray())
        {
            if (NanameWalls.Mod.nanameWalls.ContainsKey(wallDef)) continue;

            var newDef = GenerateInner(wallDef, GiveShortHash, takenHashes, NewBlueprintDef_Thing, NewFrameDef_Thing);
            if (newDef.IsSmoothable)
            {
                newDef.building = Gen.MemberwiseClone(newDef.building);
                ref var smoothedThing = ref newDef.building.smoothedThing;
                if (!IsWallProbably(smoothedThing)) continue;

                smoothedThing = GenerateInner(smoothedThing, GiveShortHash, takenHashes, NewBlueprintDef_Thing, NewFrameDef_Thing);
                smoothedThing.building.unsmoothedThing = newDef;

                if (!ViviRace.Active) continue;
                var index = smoothedThing.comps.FindIndex(c => ViviRace.CompProperties_CompWallReplace.IsAssignableFrom(c.GetType()));
                if (index != -1)
                {
                    smoothedThing.comps = [.. smoothedThing.comps];
                    smoothedThing.comps[index] = Gen.MemberwiseClone(smoothedThing.comps[index]);
                    ref var replaceThing = ref ViviRace.replaceThing(smoothedThing.comps[index]);
                    replaceThing = GenerateInner(replaceThing, GiveShortHash, takenHashes, NewBlueprintDef_Thing, NewFrameDef_Thing);
                }
            }
        }
        if (ViviRace.Active)
        {
            var VV_ViviHardenHoneycombWall = DefDatabase<ThingDef>.GetNamedSilentFail("VV_ViviHardenHoneycombWall");
            if (VV_ViviHardenHoneycombWall != null)
            {
                GenerateInner(VV_ViviHardenHoneycombWall, GiveShortHash, takenHashes, NewBlueprintDef_Thing, NewFrameDef_Thing);
            }
        }
        foreach (var designationCategory in NanameWalls.Mod.designationCategories)
        {
            designationCategory.ResolveReferences();
        }
    }

    private static ThingDef GenerateInner(ThingDef wallDef, GetGiveShortHash GiveShortHash, Dictionary<Type, HashSet<ushort>> takenHashes, GetNewBlueprintDef_Thing NewBlueprintDef_Thing, GetNewFrameDef_Thing NewFrameDef_Thing)
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
        var buildableByPlayer = wallDef.BuildableByPlayer;
        if (buildableByPlayer)
            NanameWalls.Mod.designationCategories.Add(wallDef.designationCategory);
        NanameWalls.Mod.nanameWalls[wallDef] = newDef;
        NanameWalls.Mod.originalDefs[newDef] = wallDef;
        if (NanameWalls.Mod.Settings.groupNanameWalls && buildableByPlayer && wallDef.designatorDropdown is null)
        {
            var dropdown = new DesignatorDropdownGroupDef()
            {
                defName = wallDef.defName
            };
            wallDef.designatorDropdown = dropdown;
            newDef.designatorDropdown = dropdown;
        }
        return newDef;
    }
}
