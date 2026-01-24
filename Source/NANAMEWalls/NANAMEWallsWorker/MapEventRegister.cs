using Verse;

namespace NanameWalls
{
    public class MapEventRegister(Map map) : MapComponent(map)
    {
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            map.events.BuildingSpawned += UpdateAdjacentWalls;
            map.events.BuildingDespawned += UpdateAdjacentWalls;
        }

        public void UpdateAdjacentWalls(Building building)
        {
            foreach (var c in GenAdj.CellsAdjacentCardinal(building))
            {
                var edifice = c.GetEdificeSafe(map);
                if (edifice?.def?.graphicData?.linkType == Graphic_LinkedDiagonal.LinkerTypeStatic)
                {
                    edifice.DirtyMapMesh(map);
                }
            }
        }
    }
}
