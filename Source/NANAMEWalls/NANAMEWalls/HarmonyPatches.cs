using HarmonyLib;
using Verse;

namespace NanameWalls;

[HarmonyPatch(typeof(GraphicUtility), nameof(GraphicUtility.WrapLinked))]
public static class Patch_GraphicUtility_WrapLinked
{
    public static bool Prefix(Graphic subGraphic, LinkDrawerType linkDrawerType, ref Graphic_Linked __result)
    {
        if ((byte)linkDrawerType == 217)
        {
            __result = new Graphic_LinkedDiagonal(subGraphic);
            return false;
        }
        return true;
    }
}
