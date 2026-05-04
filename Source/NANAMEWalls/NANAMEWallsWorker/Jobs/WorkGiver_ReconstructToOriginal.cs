using RimWorld;
using Verse;

namespace NanameWalls;

public class WorkGiver_ReconstructToOriginal : WorkGiver_RemoveBuilding
{
    protected override DesignationDef Designation => NAW_DefOf.NAW_ConvertToOriginal;
    protected override JobDef RemoveBuildingJob => NAW_DefOf.NAW_ReconstructToOriginal;
}
