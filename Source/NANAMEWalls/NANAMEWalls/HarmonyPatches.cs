using HarmonyLib;
using RimWorld;
using Verse;

namespace NanameWalls;

[HarmonyPatch(typeof(GraphicUtility), nameof(GraphicUtility.WrapLinked))]
public static class Patch_GraphicUtility_WrapLinked
{
    public static bool Prefix(Graphic subGraphic, LinkDrawerType linkDrawerType, ref Graphic_Linked __result)
    {
        if (linkDrawerType == Graphic_LinkedDiagonal.LinkerTypeStatic)
        {
            __result = new Graphic_LinkedDiagonal(subGraphic);
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Building), nameof(Building.GetGizmos))]
public static class Patch_Building_GetGizmos
{
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Building __instance)
    {
        foreach (var gizmo in values)
        {
            yield return gizmo;
        }
        if (DebugSettings.godMode && __instance.Graphic is Graphic_LinkedDiagonal)
        {
            yield return new Command_Action()
            {
                defaultLabel = "NAW.NanameSettings".Translate(),
                Order = 10000,
                action = () =>
                {
                    Find.WindowStack.Add(new Dialog_ModSettings(NanameWalls.Mod)
                    {
                        draggable = true
                    });
                    NanameWalls.Mod.selDef = NanameWalls.Mod.nanameWalls.FirstOrDefault(pair => pair.Value == __instance.def).Key;
                    NanameWalls.Mod.selThing = __instance;
                    Find.Selector.Deselect(__instance);
                }
            };
            yield return new Command_Action()
            {
                defaultLabel = "NAW.UpdateGraphic".Translate(),
                Order = 10001,
                action = () =>
                {
                    __instance.DirtyMapMesh(__instance.Map);
                }
            };
        }
    }
}
