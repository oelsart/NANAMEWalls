using RimWorld;
using Verse;

namespace NanameWalls;

public class WorkGiver_ReconstructToNaname : WorkGiver_RemoveBuilding
{
    protected override DesignationDef Designation => NAW_DefOf.NAW_ConvertToNaname;
    protected override JobDef RemoveBuildingJob => NAW_DefOf.NAW_ReconstructToNaname;
}
