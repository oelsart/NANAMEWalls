using Verse;

namespace NanameWalls;

public class JobDriver_ReconstructToNaname : JobDriver_ReconstructBase
{
    protected override DesignationDef Designation => NAW_DefOf.NAW_ConvertToNaname;
    protected override ThingDef ConvertTo => NanameWalls.Mod.nanameWalls.GetValueOrDefault(Building.def);
}
