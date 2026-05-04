using RimWorld;
using Verse;

namespace NanameWalls;

[DefOf]
public static class NAW_DefOf
{
    public static DesignationDef NAW_ConvertToOriginal;
    public static DesignationDef NAW_ConvertToNaname;

    public static JobDef NAW_ReconstructToOriginal;
    public static JobDef NAW_ReconstructToNaname;
    
    static NAW_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(NAW_DefOf));
    }
}