using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
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
        var GiveShortHash = AccessTools.MethodDelegate<GetGiveShortHash>(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash"));
        var NewBlueprintDef_Thing = AccessTools.MethodDelegate<GetNewBlueprintDef_Thing>(AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing"));
        var NewFrameDef_Thing = AccessTools.MethodDelegate<GetNewFrameDef_Thing>(AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing"));
        var takenHashes = AccessTools.StaticFieldRefAccess<Dictionary<Type, HashSet<ushort>>>(typeof(ShortHashGiver), "takenHashesPerDeftype");
        var f_thingDef = AccessTools.FieldRefAccess<ThingDefStyle, ThingDef>("thingDef");
        var f_styleDef = AccessTools.FieldRefAccess<ThingDefStyle, ThingStyleDef>("styleDef");
        foreach (var wallDef in DefDatabase<ThingDef>.AllDefs.Where(def => IsLinkedThing(def) && def.BuildableByPlayer).ToArray())
        {
            try
            {
                if (NanameWalls.Mod.nanameWalls.ContainsKey(wallDef)) continue;

                var newDef = GenerateInner(wallDef);
                if (!newDef.IsSmoothable) continue;
                
                newDef.building = Gen.MemberwiseClone(newDef.building);
                ref var smoothedThing = ref newDef.building.smoothedThing;
                if (!IsLinkedThing(smoothedThing)) continue;

                smoothedThing = GenerateInner(smoothedThing);
                smoothedThing.building.unsmoothedThing = newDef;

                if (!ViviRace.Active) continue;
                var index = smoothedThing.comps.FindIndex(c => ViviRace.CompProperties_CompWallReplace.IsAssignableFrom(c.GetType()));
                if (index == -1) continue;
                smoothedThing.comps = [.. smoothedThing.comps];
                smoothedThing.comps[index] = Gen.MemberwiseClone(smoothedThing.comps[index]);
                ref var replaceThing = ref ViviRace.replaceThing(smoothedThing.comps[index]);
                replaceThing = GenerateInner(replaceThing);
            }
            catch (Exception ex)
            {
                Log.Error($"[NANAME Walls] Error while generating Naname wall def from {wallDef.defName}: {ex}");
            }
        }
        if (ViviRace.Active)
        {
            var VV_ViviHardenHoneycombWall = DefDatabase<ThingDef>.GetNamedSilentFail("VV_ViviHardenHoneycombWall");
            if (VV_ViviHardenHoneycombWall != null)
            {
                GenerateInner(VV_ViviHardenHoneycombWall);
            }
        }
        foreach (var designationCategory in NanameWalls.Mod.designationCategories)
        {
            designationCategory.ResolveReferences();
        }

        return;

        static bool IsLinkedThing(ThingDef def)
        {
            var linkType = def.graphicData?.linkType;
            return def.Size == IntVec2.One && linkType is LinkDrawerType.Basic or LinkDrawerType.CornerFiller or LinkDrawerType.Asymmetric;
        }

        ThingDef GenerateInner(ThingDef wallDef)
        {
            var newDef = MakeShallowCopy(wallDef, "cachedLabelCap", "designationHotKey");
            newDef.defName += NanameWalls.Suffix;
            newDef.label = "NAW.Diagonal".Translate() + wallDef.LabelCap;
            newDef.graphicData = new GraphicData();
            newDef.graphicData.CopyFrom(wallDef.graphicData);
            newDef.graphicData.linkType = Graphic_LinkedDiagonal.LinkerTypeStatic;
            newDef.shortHash = 0;
            GiveShortHash(newDef, typeof(ThingDef), takenHashes[typeof(ThingDef)]);
            newDef.modContentPack = NanameWalls.Mod.Content;
            DefGenerator.AddImpliedDef(newDef);
            DefDatabase<BuildableDef>.Add(newDef);
            var bluePrintDef = NewBlueprintDef_Thing(newDef, false);
            bluePrintDef.shortHash = 0;
            GiveShortHash(bluePrintDef, typeof(ThingDef), takenHashes[typeof(ThingDef)]);
            DefGenerator.AddImpliedDef(bluePrintDef);
            var frameDef = NewFrameDef_Thing(newDef);
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

            if (meshSettings.enabled && NanameWalls.Mod.Settings.groupNanameWalls && buildableByPlayer)
            {
                if (wallDef.designatorDropdown is null)
                {
                    var dropdown = new DesignatorDropdownGroupDef
                    {
                        defName = wallDef.defName
                    };
                    wallDef.designatorDropdown = dropdown;
                    newDef.designatorDropdown = dropdown;
                }
                else if (MaterialSubMenu.Active)
                {
                    newDef.designatorDropdown = wallDef.designatorDropdown;
                }
            }
            foreach (var styleCategory in wallDef.RelevantStyleCategories)
            {
                var thingDefStyle = styleCategory.thingDefStyles.FirstOrDefault(t => t.ThingDef == wallDef);
                if (thingDefStyle is null)
                    continue;
                var thingDefStyle2 = new ThingDefStyle();
                f_thingDef(thingDefStyle2) = newDef;
                var newStyleDef = MakeShallowCopy(thingDefStyle.StyleDef, "cachedCategory");
                newStyleDef.defName += NanameWalls.Suffix;
                if (!newStyleDef.overrideLabel.NullOrEmpty())
                    newStyleDef.overrideLabel = "NAW.Diagonal".Translate() + newStyleDef.overrideLabel;
                newStyleDef.graphicData = new GraphicData();
                newStyleDef.graphicData.CopyFrom(thingDefStyle.StyleDef.graphicData);
                newStyleDef.graphicData.linkType = Graphic_LinkedDiagonal.LinkerTypeStatic;
                newStyleDef.shortHash = 0;
                GiveShortHash(newStyleDef, typeof(ThingStyleDef), takenHashes[typeof(ThingStyleDef)]);
                newStyleDef.modContentPack = NanameWalls.Mod.Content;
                DefGenerator.AddImpliedDef(newStyleDef);
                f_styleDef(thingDefStyle2) = newStyleDef;
                styleCategory.thingDefStyles.Add(thingDefStyle2);
            }
            return newDef;
        }
    }

    private static T MakeShallowCopy<T>(T from, params string[] exceptFields)
    {
        var to = Activator.CreateInstance(from.GetType());
        foreach (var fieldInfo in from.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (exceptFields.Contains(fieldInfo.Name))
                continue;
            fieldInfo.SetValue(to, fieldInfo.GetValue(from));
        }
        return (T)to;
    }
}
