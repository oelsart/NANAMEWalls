using HarmonyLib;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using static NanameWalls.ModCompat;

namespace NanameWalls;

[HarmonyPatchCategory(ViviRace.PatchCategory)]
[HarmonyPatch]
public static class Patch_JobDriver_FortifyHoneycombWall_MakeNewToils
{
    public static MethodBase TargetMethod()
    {
        Type[] localTypes = [typeof(float), typeof(Thing), typeof(float), typeof(Map), typeof(IntVec3), typeof(Rot4), typeof(Faction), typeof(Thing), typeof(LocalTargetInfo)];
        return AccessTools.FindIncludingInnerTypes(GenTypes.GetTypeInAnyAssembly("VVRace.JobDriver_FortifyHoneycombWall", "VVRace"), type =>
        {
            return type.GetDeclaredMethods().FirstOrDefault(method =>
            {
                return method.Name.Contains("<MakeNewToils>") && method.GetMethodBody().LocalVariables.Select(l => l.LocalType).SequenceEqual(localTypes);
            });
        });
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new CodeMatcher(instructions);
        var f_VV_ViviHardenHoneycombWall = AccessTools.Field("VVRace.VVThingDefOf:VV_ViviHardenHoneycombWall");
        codes.MatchStartForward(new CodeMatch(OpCodes.Ldsfld, f_VV_ViviHardenHoneycombWall));
        codes.InsertAfter(
            CodeInstruction.LoadLocal(1),
            CodeInstruction.Call(typeof(Patch_JobDriver_FortifyHoneycombWall_MakeNewToils), nameof(ReplaceThingDef)));
        return codes.Instructions();
    }

    private static ThingDef ReplaceThingDef(ThingDef thingDef, Thing thing)
    {
        if (NanameWalls.Mod.nanameWalls.ContainsValue(thing.def) && NanameWalls.Mod.nanameWalls.TryGetValue(thingDef, out var thingDef2))
        {
            return thingDef2;
        }
        return thingDef;
    }
}

[HarmonyPatchCategory(ViviRace.PatchCategory)]
[HarmonyPatch]
public static class Patch_Designator_FortifyHoneycombWall_DesignateThing
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        var type = GenTypes.GetTypeInAnyAssembly("VVRace.Designator_FortifyHoneycombWall", "VVRace");
        if (type != null)
        {
            yield return AccessTools.Method(type, nameof(Designator.DesignateThing));
        }
        var type2 = GenTypes.GetTypeInAnyAssembly("VVRace.HarmonyPatches.ViviRacePatch", "VVRace.HarmonyPatches");
        if (type2 != null)
        {
            var method = AccessTools.Method(type2, "Designator_Cancel_DesignateThing_Postfix");
            if (method != null)
            {
                yield return method;
            }
        }
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new CodeMatcher(instructions);
        codes.MatchStartForward(new CodeMatch(c => c.opcode == OpCodes.Bne_Un_S || c.opcode == OpCodes.Bne_Un));
        codes.Insert(
            CodeInstruction.Call(typeof(Patch_Designator_FortifyHoneycombWall_DesignateThing), nameof(IsWallDefOrNanameWallDef)),
            new CodeInstruction(OpCodes.Ldc_I4_1));
        return codes.Instructions();
    }

    public static bool IsWallDefOrNanameWallDef(ThingDef thingDef, ThingDef wallDef)
    {
        if (thingDef == wallDef) return true;
        if (NanameWalls.Mod.nanameWalls.TryGetValue(wallDef, out var nanameDef) )
        {
            return thingDef == nanameDef;
        }
        return false;
    }
}

[HarmonyPatchCategory(ViviRace.PatchCategory)]
[HarmonyPatch("VVRace.Designator_FortifyHoneycombWall", "CanDesignateThing")]
public static class Patch_Designator_FortifyHoneycombWall_CanDesignateThing
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new CodeMatcher(instructions);
        codes.MatchEndForward(
            new CodeMatch(OpCodes.Ldarg_1),
            new CodeMatch(OpCodes.Ldfld),
            new CodeMatch(OpCodes.Ldsfld),
            new CodeMatch(c => c.opcode == OpCodes.Beq_S || c.opcode == OpCodes.Beq));
        codes.Insert(
            CodeInstruction.Call(typeof(Patch_Designator_FortifyHoneycombWall_DesignateThing), nameof(Patch_Designator_FortifyHoneycombWall_DesignateThing.IsWallDefOrNanameWallDef)),
            new CodeInstruction(OpCodes.Ldc_I4_1));
        return codes.Instructions();
    }
}