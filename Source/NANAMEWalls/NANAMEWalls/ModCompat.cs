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
                if (!Active) return;

                try
                {
                    RotForPrintCounter = (Func<Rot4>)AccessTools.PropertyGetter("VehicleMapFramework.VehicleSectionLayerManager:RotForPrintCounter")?.CreateDelegate(typeof(Func<Rot4>));
                }
                finally
                {                    if (RotForPrintCounter is null)
                    {
                        Log.Error("[NanameWalls] VehicleMapFramework compatibility is broken.");
                        Active = false;
                    }
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

                try
                {
                    CompProperties_CompWallReplace = GenTypes.GetTypeInAnyAssembly("VVRace.CompProperties_CompWallReplace", "VVRace");
                    replaceThing = AccessTools.FieldRefAccess<ThingDef>("VVRace.CompProperties_CompWallReplace:replaceThing");
                }
                finally
                {
                    if (CompProperties_CompWallReplace is null || replaceThing is null)
                    {
                        Log.Error("[NanameWalls] ViviRace compatibility is broken.");
                        Active = false;
                    }
                }
            }
        }

        public static class MaterialSubMenu
        {
            public static readonly bool Active = ModsConfig.IsActive("cedaro.material.submenu") || ModsConfig.IsActive("WSP.GroupedBuildings");
        }

        public static class ReplaceContextMenu
        {
            public static readonly bool Active = ModsConfig.IsActive("Nebulae.NoCrowdedContextMenu");

            public const string PatchCategory = "Patches_ReplaceContextMenu";
        }

        public static class Odyssey
        {
            public static readonly bool Active = ModsConfig.OdysseyActive;

            public const string PatchCategory = "Patches_Odyssey";
        }

        public static class ShowBuildableMaterialCount
        {
            public static readonly bool Active = ModsConfig.IsActive("BP.ShowBuildableMaterialCount");
            public static readonly AccessTools.FieldRef<bool> buildableClicked;
            public static readonly AccessTools.FieldRef<BuildableDef> desBuildable;

            static ShowBuildableMaterialCount()
            {
                if (!Active) return;

                try
                {
                    var type = GenTypes.GetTypeInAnyAssembly("ShowBuildableMaterialCount.ShowBuildableMaterialCountMod", "ShowBuildableMaterialCount");
                    var f_buildableClicked = AccessTools.Field(type, "buildableClicked");
                    if (f_buildableClicked is not null)
                        buildableClicked = AccessTools.StaticFieldRefAccess<bool>(f_buildableClicked);

                    var f_desBuildable = AccessTools.Field(type, "desBuildable");
                    if (f_desBuildable is not null)
                        desBuildable = AccessTools.StaticFieldRefAccess<BuildableDef>(f_desBuildable);
                }
                finally
                {
                    if (buildableClicked is null || desBuildable is null)
                    {
                        Log.Error("[NanameWalls] ShowBuildableMaterialCount compatibility is broken.");
                        Active = false;
                    }
                }
            }
        }

        public static class ArgonicCore
        {
            public static readonly bool Active = ModsConfig.IsActive("Argon.CoreLib");
            public static readonly Func<Def, DefModExtension> GetModExtension_ThingDefExtension_CoatableWall;
            public static readonly AccessTools.FieldRef<DefModExtension, ThingDef> coatedThingDef;

            static ArgonicCore()
            {
                if (!Active) return;

                try
                {
                    var t_ThingDefExtension_CoatableWall = GenTypes.GetTypeInAnyAssembly("ArgonicCore.ModExtensions.ThingDefExtension_CoatableWall", "ArgonicCore.ModExtensions");
                    if (t_ThingDefExtension_CoatableWall is not null)
                    {
                        var m_GetModExtension_ThingDefExtension_CoatableWall = AccessTools.Method(typeof(Def), nameof(Def.GetModExtension))
                            .MakeGenericMethod(t_ThingDefExtension_CoatableWall);
                        GetModExtension_ThingDefExtension_CoatableWall =
                            AccessTools.MethodDelegate<Func<Def, DefModExtension>>(m_GetModExtension_ThingDefExtension_CoatableWall);
                        coatedThingDef = AccessTools.FieldRefAccess<ThingDef>(t_ThingDefExtension_CoatableWall, "coatedThingDef");
                    }
                }
                finally
                {
                    if (GetModExtension_ThingDefExtension_CoatableWall is null || coatedThingDef is null)
                    {
                        Log.Error("[NanameWalls] ArgonicCore compatibility is broken.");
                        Active = false;
                    }
                }
            }
        }
    }
}
