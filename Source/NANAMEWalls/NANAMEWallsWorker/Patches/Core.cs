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
                    if (!patchClass.Category.NullOrEmpty() &&
                        (!ViviRace.Active || patchClass.Category != ViviRace.PatchCategory) &&
                        (!Odyssey.Active || patchClass.Category != Odyssey.PatchCategory)) return;
                    patchClass.Patch();
                }
                catch (Exception ex)
                {
                    Log.Error($"[NanameWalls] Error while apply patching: {ex}");
                }
            });
    }
}
