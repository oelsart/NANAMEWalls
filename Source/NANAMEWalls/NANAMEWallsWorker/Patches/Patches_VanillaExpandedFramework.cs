using HarmonyLib;
using RimWorld;
using Verse;

namespace NanameWalls;

[HarmonyPatchCategory(ModCompat.VanillaExpandedFramework.PatchCategory)]
[HarmonyPatch("VEF.Buildings.VanillaExpandedFramework_ResearchProjectDef_UnlockedDefs_Patch", "Postfix")]
public static class Patch_VanillaExpandedFramework_ResearchProjectDef_UnlockedDefs_Patch_Postfix
{
  public static void Postfix(List<Def> __0, HashSet<BuildableDef> ___cachedHiddenDesignators)
  {
    __0.RemoveAll(d => d is ThingDef thingDef &&
                            NanameWalls.Mod.originalDefs.TryGetValue(thingDef, out var originalDef) &&
                            ___cachedHiddenDesignators.Contains(originalDef));
  }
}

[HarmonyPatchCategory(ModCompat.VanillaExpandedFramework.PatchCategory)]
[HarmonyPatch(typeof(DesignationCategoryDef), nameof(DesignationCategoryDef.ResolvedAllowedDesignators), MethodType.Getter)]
public static class Patch_DesignationCategoryDef_ResolvedAllowedDesignators
{
  private static readonly HashSet<BuildableDef> hidden_designators =
    (HashSet<BuildableDef>)AccessTools.Field("VEF.Buildings.StaticCollectionsClass:hidden_designators")?.GetValue(null);

  private static bool Prepare() => hidden_designators is not null;
  
  public static IEnumerable<Designator> Postfix(IEnumerable<Designator> values)
  {
    foreach (var designator in values)
    {
      switch (designator)
      {
        case Designator_Dropdown designatorDropdown when designatorDropdown.Elements
          .OfType<Designator_Build>()
          .Any(d => hidden_designators.Contains(d.PlacingDef)):
        case Designator_Build { PlacingDef: ThingDef thingDef } when
          NanameWalls.Mod.originalDefs.TryGetValue(thingDef, out var originalDef) &&
          hidden_designators.Contains(originalDef):
          continue;
        default:
          yield return designator;
          break;
      }
    }
  }
}