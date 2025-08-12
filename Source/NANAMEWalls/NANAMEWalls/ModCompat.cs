using HarmonyLib;
using Verse;
using Verse.Noise;

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
                if (!Active) return;

                RotForPrintCounter = (Func<Rot4>)AccessTools.PropertyGetter("VehicleMapFramework.VehicleMapUtility:RotForPrintCounter")?.CreateDelegate(typeof(Func<Rot4>));
                if (RotForPrintCounter is null)
                {
                    Active = false;
                }
            }
        }

        public static class ViviRace
        {
            public static readonly bool Active = ModsConfig.IsActive("gguake.race.vivi");

            public const string PatchCategory = "Patches_ViviRace";

            public static readonly Type CompProperties_CompWallReplace;

            public static readonly AccessTools.FieldRef<CompProperties, ThingDef> replaceThing;

            static ViviRace()
            {
                if (!Active) return;

                CompProperties_CompWallReplace = GenTypes.GetTypeInAnyAssembly("VVRace.CompProperties_CompWallReplace", "VVRace");
                replaceThing = AccessTools.FieldRefAccess<ThingDef>("VVRace.CompProperties_CompWallReplace:replaceThing");

                if (CompProperties_CompWallReplace is null || replaceThing is null)
                {
                    Active = false;
                }
            }
        }
    }
}
