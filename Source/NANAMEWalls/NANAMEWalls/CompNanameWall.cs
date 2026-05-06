using RimWorld;
using UnityEngine;
using Verse;

namespace NanameWalls;

[StaticConstructorOnStartup]
public class CompNanameWall : ThingComp
{
    private static readonly Texture2D iconTex = ContentFinder<Texture2D>.Get("NanameWalls/UI/ForceDent");
    
    private bool dentRoofed;

    public bool DentRoofed => dentRoofed;
    
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Toggle
        {
            defaultLabel = "NAW.ForceDentRoofed".Translate(),
            defaultDesc = "NAW.ForceDentRoofedDesc".Translate(),
            icon = iconTex,
            Order = 51f,
            isActive = () => dentRoofed,
            toggleAction = () =>
            {
                dentRoofed = !dentRoofed;
                var mapDrawer = parent.Map.mapDrawer;
                for (var i = 0; i < 4; i++)
                {
                    var c = parent.Position + GenAdj.DiagonalDirections[i];
                    mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Things);
                }
            }
        };
        
        if (!DebugSettings.godMode || parent.Graphic is not Graphic_LinkedDiagonal) yield break;
        yield return new Command_Action
        {
            defaultLabel = "NAW.NanameSettings".Translate(),
            Order = 10000f,
            action = () =>
            {
                Find.WindowStack.Add(new Dialog_ModSettings(NanameWalls.Mod)
                {
                    draggable = true,
                    resizeable = true
                });
                NanameWalls.Mod.selDef = NanameWalls.Mod.nanameWalls.FirstOrDefault(pair => pair.Value == parent.def).Key;
                NanameWalls.Mod.selThing = parent;
                Find.Selector.Deselect(parent);
            }
        };
        yield return new Command_Action()
        {
            defaultLabel = "NAW.UpdateGraphic".Translate(),
            Order = 10001f,
            action = () =>
            {
                parent.DirtyMapMesh(parent.Map);
            }
        };
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref dentRoofed, nameof(dentRoofed));
    }
}
