using HarmonyLib;
using RimWorld;
using Verse;
using static NanameWalls.ModCompat;

namespace NanameWalls;

[HarmonyPatchCategory(ReplaceContextMenu.PatchCategory)]
[HarmonyPatch("NoCrowdedContextMenu.Utilities.MenuOptionUtility", "OnBuildingPickerCreated")]
public static class Patch_MenuOptionUtility_OnBuildingPickerCreated
{
    public static void Prefix(List<Designator> buildings)
    {
        if (buildings.ElementAtOrDefault(0) is not Designator_Build designator_Build ||
            buildings.ElementAtOrDefault(1) is not Designator_Build designator_Build2 ||
            designator_Build.PlacingDef is not ThingDef { MadeFromStuff: true } thingDef ||
            designator_Build2.PlacingDef is not ThingDef { MadeFromStuff: true } thingDef2 ||
            !NanameWalls.Mod.nanameWalls.ContainsKey(thingDef) ||
            !NanameWalls.Mod.nanameWalls.ContainsValue(thingDef2)) return;

        var count = designator_Build.Map.resourceCounter.AllCountedAmounts.Keys
            .Count(item => !item.IsStuff || !item.stuffProps.CanMake(thingDef) || (!DebugSettings.godMode &&
                designator_Build.Map.listerThings.ThingsOfDef(item).Count <= 0));

        for (var i = 0; i < count - 1; i++)
        {
            buildings.Insert(0, designator_Build);
        }
        for (var i = 0; i < count - 1; i++)
        {
            buildings.Add(designator_Build2);
        }
    }
}