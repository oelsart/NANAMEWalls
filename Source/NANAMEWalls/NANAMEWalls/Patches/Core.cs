using HarmonyLib;
using System.Reflection;
using Verse;
using static NanameWalls.ModCompat;

namespace NanameWalls;

[StaticConstructorOnStartup]
internal class Core
{
    static Core()
    {
        var assembly = Assembly.GetExecutingAssembly();
        GenTypes.AllTypes.Where(t => t.Assembly == assembly).Select(NanameWalls.Mod.Harmony.CreateClassProcessor)
            .Do(patchClass =>
            {
                try
                {
                    if (patchClass.Category.NullOrEmpty())
                    {
                        patchClass.Patch();
                        return;
                    }
                    if (ViviRace.Active && patchClass.Category == ViviRace.PatchCategory)
                    {
                        patchClass.Patch();
                        return;
                    }
                    if (MoreGroupedBuildings.Active && patchClass.Category == MoreGroupedBuildings.PatchCategory)
                    {
                        patchClass.Patch();
                        return;
                    }
                    if (Odyssey.Active && patchClass.Category == Odyssey.PatchCategory)
                    {
                        patchClass.Patch();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[NanameWalls] Error while apply patching: {ex}");
                }
            });
    }
}
