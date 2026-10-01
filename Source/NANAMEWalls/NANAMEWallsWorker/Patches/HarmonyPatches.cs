using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static NanameWalls.ModCompat;

namespace NanameWalls;

[HarmonyPatch(typeof(Designator_Dropdown), "SetupFloatMenu")]
public static class Patch_Designator_Dropdown_SetupFloatMenu
{
  private static readonly AccessTools.FieldRef<FloatMenu, List<FloatMenuOption>> options =
    AccessTools.FieldRefAccess<FloatMenu, List<FloatMenuOption>>("options");

  private static bool Prepare()
  {
    return NanameWalls.Mod.Settings.groupNanameWalls && !MaterialSubMenu.Active;
  }

  public static void Postfix(List<Designator> ___elements, Window __result)
  {
	  if (__result is FloatMenu floatMenu &&
	      ___elements.All(static d => d is Designator_Build { PlacingDef: ThingDef { MadeFromStuff: true } }))
	  {
		  var optionList = options(floatMenu);
		  optionList.Clear();
		  foreach (var element in ___elements)
		  {
			  element.ProcessInput(Event.current);
			  if (Find.WindowStack.TryGetWindow<FloatMenu>(out var floatMenu2))
			  {
				  optionList.AddRange(options(floatMenu2));
				  floatMenu2.Close(false);
			  }
			  else
			  {
				  LongEventHandler.ExecuteWhenFinished(() =>
				  {
					  __result.Close(false);
					  Find.DesignatorManager.Deselect();
				  });
				  return;
			  }
		  }
	  }
  }
}

[HarmonyPatch(typeof(ResearchPrerequisitesUtility),
  nameof(ResearchPrerequisitesUtility.UnlockedDefsGroupedByPrerequisites))]
public static class Patch_ResearchPrerequisitesUtility_UnlockedDefsGroupedByPrerequisites
{
  public static void Postfix(List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>> __result)
  {
    foreach (var pair in __result)
    {
      pair.Second.RemoveAll(def =>
        def is ThingDef thingDef && NanameWalls.Mod.nanameWalls.ContainsValue(thingDef));
    }
  }
}