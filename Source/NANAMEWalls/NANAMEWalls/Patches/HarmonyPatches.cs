﻿using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

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
                        draggable = true,
                        resizeable = true
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

[HarmonyPatch(typeof(Designator_Dropdown), "SetupFloatMenu")]
public static class Patch_Designator_Dropdown_SetupFloatMenu
{
    private readonly static AccessTools.FieldRef<Designator_Build, ThingDef> stuffDef = AccessTools.FieldRefAccess<Designator_Build, ThingDef>("stuffDef");

    private readonly static AccessTools.FieldRef<Designator_Build, bool> writeStuff = AccessTools.FieldRefAccess<Designator_Build, bool>("writeStuff");

    private readonly static AccessTools.FieldRef<Designator_Dropdown, bool> activeDesignatorSet = AccessTools.FieldRefAccess<Designator_Dropdown, bool>("activeDesignatorSet");

    private static bool Prepare()
    {
        return NanameWalls.Mod.Settings.groupNanameWalls;
    }

    public static bool Prefix(Designator_Dropdown __instance, List<Designator> ___elements, ref Window __result)
    {
        List<FloatMenuOption> list = null;
        Designator_Build designator = null;
        var flag = false;
        for (var i = 0; i < 2; i++)
        {
            if (___elements.ElementAtOrDefault(i) is Designator_Build designator_Build)
            {
                if (designator_Build.PlacingDef is not ThingDef thingDef || !thingDef.MadeFromStuff) continue;

                if (NanameWalls.Mod.nanameWalls.ContainsKey(thingDef) || NanameWalls.Mod.nanameWalls.ContainsValue(thingDef))
                {
                    flag = true;
                    list ??= [];
                    designator ??= designator_Build;
                    foreach (ThingDef item in from d in designator_Build.Map.resourceCounter.AllCountedAmounts.Keys
                                              orderby d.stuffProps?.commonality ?? float.PositiveInfinity descending, d.BaseMarketValue
                                              select d)
                    {
                        if (item.IsStuff && item.stuffProps.CanMake(thingDef) && (DebugSettings.godMode || designator_Build.Map.listerThings.ThingsOfDef(item).Count > 0))
                        {
                            ThingDef localStuffDef = item;
                            string str = designator_Build.sourcePrecept == null ? GenLabel.ThingLabel(thingDef, localStuffDef) : ((string)"ThingMadeOfStuffLabel".Translate(localStuffDef.LabelAsStuff, designator_Build.sourcePrecept.Label));
                            str = str.CapitalizeFirst();
                            FloatMenuOption floatMenuOption = new(str, () =>
                            {
                                if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(designator_Build.TutorTagSelect))
                                {
                                    return;
                                }
                                designator_Build.CurActivateSound?.PlayOneShotOnCamera(null);
                                Find.DesignatorManager.Select(designator_Build);
                                stuffDef(designator_Build) = localStuffDef;
                                writeStuff(designator_Build) = true;
                                __instance.SetActiveDesignator(designator_Build);
                            }, item)
                            {
                                tutorTag = "SelectStuff-" + thingDef.defName + "-" + localStuffDef.defName
                            };
                            list.Add(floatMenuOption);
                        }
                    }
                }
            }
        }
        if (flag)
        {
            __result = new FloatMenu(list)
            {
                onCloseCallback = () =>
                {
                    activeDesignatorSet(__instance) = true;
                    writeStuff(designator) = true;
                }
            };
            return false;
        }
        return true;
    }
}