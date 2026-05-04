using RimWorld;
using UnityEngine;
using Verse;

namespace NanameWalls;

public abstract class JobDriver_ReconstructBase : JobDriver_RemoveBuilding
{
    protected override float TotalNeededWork => Mathf.Clamp(Building.GetStatValue(StatDefOf.WorkToBuild) / 2f, 10f, 1500f);
    protected override EffecterDef WorkEffecter => null;
    
    protected abstract ThingDef ConvertTo { get; }

    protected override void FinishedRemoving()
    {
        ThingConvertUtility.ConvertThingWithDef(Target, ConvertTo);
        pawn.records.Increment(RecordDefOf.ThingsConstructed);
    }
}
