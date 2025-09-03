using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static NanameWalls.ModCompat;
using static RimWorld.SectionLayer_GravshipHull;

namespace NanameWalls;

[HarmonyPatchCategory(Odyssey.PatchCategory)]
[HarmonyPatch(typeof(SectionLayer_GravshipHull), nameof(ShouldDrawCornerPiece))]
public static class Patch_SectionLayer_GravshipHull_ShouldDrawCornerPiece
{
    private static bool Prepare()
    {
        return NanameWalls.Mod.Settings.renderSubstructure;
    }

    public static void Postfix(IntVec3 pos, Map map, TerrainGrid terrGrid, IntVec3[] ___Directions, bool[] ___tmpChecks, ref CornerType cornerType, ref Color color, ref bool __result)
    {
        if (__result || pos.GetEdifice(map) != null)
        {
            return;
        }
        TerrainDef terrainDef = terrGrid.FoundationAt(pos);
        if (terrainDef != null && terrainDef.IsSubstructure)
        {
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            var c = pos + ___Directions[i];
            if (!c.InBounds(map)) continue;
            var edifice = c.GetEdifice(map);
            var substructure = terrGrid.FoundationAt(pos + ___Directions[i]);
            if (edifice is null || substructure is null) continue;
            ___tmpChecks[i] = NanameWalls.Mod.nanameWalls.ContainsValue(edifice.def) && substructure.IsSubstructure;
        }

        if (___tmpChecks[0])
        {
            if (___tmpChecks[3] && !___tmpChecks[2] && !___tmpChecks[1])
            {
                cornerType = CornerType.Corner_NW;
            }
            else if (___tmpChecks[1] && !___tmpChecks[2] && !___tmpChecks[3])
            {
                cornerType = CornerType.Corner_NE;
            }
        }
        __result = cornerType != CornerType.None;
        color = color.WithAlpha(0f);
    }
}