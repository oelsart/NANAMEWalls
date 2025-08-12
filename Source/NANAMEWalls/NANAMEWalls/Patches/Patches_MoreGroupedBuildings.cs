using HarmonyLib;
using RimWorld;
using Verse;
using static NanameWalls.ModCompat;

namespace NanameWalls;

[HarmonyPatchCategory(MoreGroupedBuildings.PatchCategory)]
[HarmonyPatch("MaterialSubMenu.Patcher", "Postfix")]
public static class Patch_Patcher_Postfix
{
    private static bool Prepare()
    {
        return NanameWalls.Mod.Settings.groupNanameWalls;
    }

    public static bool Prefix(List<Designator> __1)
    {
        if (__1.ElementAtOrDefault(0) is Designator_Build designator_Build && __1.ElementAtOrDefault(1) is Designator_Build designator_Build2)
        {
            if (designator_Build.PlacingDef is not ThingDef thingDef) return true;

            if (NanameWalls.Mod.nanameWalls.TryGetValue(thingDef, out var thingDef2) && designator_Build2.PlacingDef == thingDef2)
            {
                return !thingDef.MadeFromStuff;
            }
        }
        return true;
    }
}
