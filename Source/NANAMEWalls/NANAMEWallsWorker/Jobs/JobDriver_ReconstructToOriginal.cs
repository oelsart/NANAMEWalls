using Verse;

namespace NanameWalls;

public class JobDriver_ReconstructToOriginal : JobDriver_ReconstructBase
{
    protected override DesignationDef Designation => NAW_DefOf.NAW_ConvertToOriginal;
    protected override ThingDef ConvertTo => NanameWalls.Mod.originalDefs.GetValueOrDefault(Building.def);
}
