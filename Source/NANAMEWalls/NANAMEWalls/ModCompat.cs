using HarmonyLib;
using Verse;

namespace NanameWalls
{
    public static class ModCompat
    {
        public static class VehicleMapFramework
        {
            public static readonly bool Active = ModsConfig.IsActive("OELS.VehicleMapFramework") || ModsConfig.IsActive("OELS.VehicleMapFramework.dev");

            public static readonly Func<Rot4> RotForPrintCounter;

            static VehicleMapFramework()
            {
                if (Active)
                {
                    RotForPrintCounter = (Func<Rot4>)AccessTools.PropertyGetter("VehicleMapFramework.VehicleMapUtility:RotForPrintCounter")?.CreateDelegate(typeof(Func<Rot4>));
                    if (RotForPrintCounter is null)
                    {
                        Active = false;
                    }
                }
            }
        }
    }
}
