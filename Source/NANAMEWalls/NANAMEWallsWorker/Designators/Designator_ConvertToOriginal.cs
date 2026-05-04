using RimWorld;
using UnityEngine;
using Verse;

namespace NanameWalls;

public sealed class Designator_ConvertToOriginal : Designator_Cells
{
    public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;
    
    public Designator_ConvertToOriginal()
    {
        defaultLabel = "NAW.ConvertToOriginal".Translate();
        defaultDesc = "NAW.ConvertToOriginalDesc".Translate();
        Order = 12;
        icon = ContentFinder<Texture2D>.Get("NanameWalls/UI/ConvertToOriginal");
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        useMouseIcon = true;
        soundSucceeded = SoundDefOf.Designate_Claim;
        showReverseDesignatorDisabledReason = true;
    }
    
    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        if (!loc.InBounds(Map)) return false;
        foreach (var t in loc.GetThingList(Map))
        {
            if (CanDesignateThing(t)) return true;
        }
        return "NAW.MustDesignateNanameWall".Translate();
    }

    public override AcceptanceReport CanDesignateThing(Thing t)
    {
        return Map.designationManager.DesignationOn(t) is null &&
               NanameWalls.Mod.originalDefs.ContainsKey(t.def);
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        var thingList = c.GetThingList(Map);
        for (var i = thingList.Count - 1; i >= 0; i--)
        {
            var thing = thingList[i];
            if (CanDesignateThing(thing))
            {
                DesignateThing(thing);
            }
        }
    }

    public override void DesignateThing(Thing t)
    {
        if (DebugSettings.godMode)
            ThingConvertUtility.ConvertThingWithDef(t, NanameWalls.Mod.originalDefs[t.def]);
        else
        {
            Map.designationManager.AddDesignation(new Designation(t, NAW_DefOf.NAW_ConvertToOriginal));
        }
    }
}
