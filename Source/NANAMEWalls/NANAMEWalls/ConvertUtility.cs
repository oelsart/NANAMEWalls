using RimWorld;
using Verse;

namespace NanameWalls;

public static class ThingConvertUtility
{
    public static void ConvertThingWithDef(Thing thing, ThingDef def)
    {
        if (!thing.Spawned || def is null) return;

        var map = thing.Map;
        var pos = thing.Position;
        var rot = thing.Rotation;

        var newThing = ThingMaker.MakeThing(def, thing.Stuff);
        newThing.SetFaction(thing.Faction);
        newThing.HitPoints = thing.HitPoints;
        newThing.StyleDef = thing.StyleDef;
        newThing.StyleSourcePrecept = thing.StyleSourcePrecept;
        newThing.EverSeenByPlayer = thing.EverSeenByPlayer;
        if (newThing.DrawColor != thing.DrawColor)
            newThing.DrawColor = thing.DrawColor;
        
        if (thing.TryGetComp<CompQuality>(out var compQuality1) && newThing.TryGetComp<CompQuality>(out var compQuality2))
            compQuality2.SetQuality(compQuality1.Quality, null);

        if (thing.TryGetComp<CompPowerBattery>(out var compBattery1) && newThing.TryGetComp<CompPowerBattery>(out var compBattery2))
            compBattery2.SetStoredEnergyPct(compBattery1.StoredEnergyPct);

        if (thing.TryGetComp<CompRefuelable>(out var compRefuelable1) && newThing.TryGetComp<CompRefuelable>(out var compRefuelable2))
            compRefuelable2.Refuel(compRefuelable1.Fuel - compRefuelable2.Fuel);

        List<(Thing attached, IntVec3 pos, Rot4 rot)> attachedThingData = null;
        if (thing.def.building is { supportsWallAttachments: true })
        {
            var attachedBuildings = GenConstruct.GetAttachedBuildings(thing);
            attachedThingData = [];
            foreach (var attached in attachedBuildings)
            {
                attachedThingData.Add((attached, attached.Position, attached.Rotation));
                attached.DeSpawn();
            }
        }
        
        thing.Destroy();
        GenSpawn.Spawn(newThing, pos, map, rot);
        FleckMaker.ThrowAirPuffUp(newThing.DrawPos, map);

        if (attachedThingData is not null)
        {
            foreach (var attachedData in attachedThingData)
            {
                GenSpawn.Spawn(attachedData.attached, attachedData.pos, map, attachedData.rot);
            }
        }
    }
}
