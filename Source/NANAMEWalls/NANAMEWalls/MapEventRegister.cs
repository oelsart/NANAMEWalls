using Verse;

namespace NanameWalls
{
    public class MapEventRegister : MapComponent
    {
        public MapEventRegister(Map map) : base(map) { }

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
                if (edifice != null && edifice.Graphic is Graphic_LinkedDiagonal)
                {
                    edifice.DirtyMapMesh(map);
                }
            }
        }
    }
}
