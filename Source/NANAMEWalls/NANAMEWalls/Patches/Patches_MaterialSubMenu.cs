using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using static NanameWalls.ModCompat;

namespace NanameWalls;

[HarmonyPatchCategory(MaterialSubMenu.PatchCategory)]
[HarmonyPatch]
public static class Patch_Patcher_Postfix
{
    private static bool Prepare()
    {
        return NanameWalls.Mod.Settings.groupNanameWalls;
    }

    private static IEnumerable<MethodBase> TargetMethods()
    {
        var type = GenTypes.GetTypeInAnyAssembly("MaterialSubMenu.Patcher", "MaterialSubMenu");
        var method = AccessTools.Method(type, "Patch_ProcessInput");
        if (method != null)
        {
            yield return method;
        }
        var method2 = AccessTools.Method(type, "Postfix");
        if (method2 != null)
        {
            yield return method2;
        }
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
