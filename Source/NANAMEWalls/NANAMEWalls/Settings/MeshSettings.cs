using UnityEngine;
using Verse;

namespace NanameWalls;

public class MeshSettings : IExposable
{
    private const string DefaultName = "NAW_DefaultWall";

    private static Dictionary<string, MeshSettings> defaultSettings = [];

    private static MeshSettings commonDefaultSettings;

    public bool enabled;

    public int repeatNorth;

    public List<Vector3> northUVs;

    public List<Vector3> northVerts;

    public List<Vector3> northVertsFiller;

    public List<Vector3> northUVsFinish;

    public List<Vector3> northVertsFinish;

    public List<Vector3> borderFillerUVs;

    public List<Vector3> northVertsFinishBorder;

    public int repeatSouth;

    public List<Vector3> southUVs;

    public List<Vector3> southVerts;

    public List<Vector3> southVertsFiller;

    public List<Vector3> southUVsFinish;

    public List<Vector3> southVertsFinish;

    public List<Vector3> topFillerUVs;

    private MeshSettings() { }

    public static void Init()
    {
        var settingsFilename = Path.Combine(NanameWalls.Mod.Content.RootDir, "DefaultSettings.xml");
        try
        {
            if (File.Exists(settingsFilename))
            {
                Scribe.loader.InitLoading(settingsFilename);
                try
                {
                    Scribe_Deep.Look(ref commonDefaultSettings, "commonDefaultSettings");
                    Scribe_StringKeyDictionary.Look(ref defaultSettings, "meshSettings", LookMode.Deep);
                }
                finally
                {
                    Scribe.loader.FinalizeLoading();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"[NANAME Walls] Caught exception while loading default settings data. Generating fresh settings. The exception was: {ex}");
        }
        defaultSettings ??= [];
    }

    public static MeshSettings DefaultSettingsFor(ThingDef def)
    {
        var defName = def?.defName;
        return DefaultSettingsFor(defName);
    }

    public static MeshSettings DefaultSettingsFor(string defName)
    {
        if (!defName.NullOrEmpty() && defaultSettings.TryGetValue(defName, out var settings))
        {
            return settings;
        }
        return commonDefaultSettings;
    }

    public static MeshSettings DeepCopyDefaultFor(ThingDef def)
    {
        static bool IsWallProbably(ThingDef def)
        {
            return (def.IsWall || (def.defName.Contains("Wall"))) &&
                def.passability == Traversability.Impassable;
        }

        var defaultSettings = DefaultSettingsFor(def);
        var enabled = defaultSettings == commonDefaultSettings ? IsWallProbably(def) : defaultSettings.enabled;
        return new MeshSettings
        {
            enabled = enabled,
            repeatNorth = defaultSettings.repeatNorth,
            northUVs = [.. defaultSettings.northUVs],
            northVerts = [.. defaultSettings.northVerts],
            northVertsFiller = [.. defaultSettings.northVertsFiller],
            northUVsFinish = [.. defaultSettings.northUVsFinish],
            northVertsFinish = [.. defaultSettings.northVertsFinish],
            borderFillerUVs = [.. defaultSettings.borderFillerUVs],
            northVertsFinishBorder = [.. defaultSettings.northVertsFinishBorder],
            repeatSouth = defaultSettings.repeatSouth,
            southUVs = [.. defaultSettings.southUVs],
            southVerts = [.. defaultSettings.southVerts],
            southVertsFiller = [.. defaultSettings.southVertsFiller],
            southUVsFinish = [.. defaultSettings.southUVsFinish],
            southVertsFinish = [.. defaultSettings.southVertsFinish],
            topFillerUVs = [.. defaultSettings.topFillerUVs]
        };
    }

    public void ExposeData()
    {
        static void CheckAndLook(ref List<Vector3> value, List<Vector3> defaultValue, string label)
        {
            if (Scribe.mode != LoadSaveMode.Saving || (defaultValue != null && value != null && !value.SequenceEqual(defaultValue)))
            {
                Scribe_Collections.Look(ref value, label, LookMode.Value);
            }
            value ??= [.. defaultValue ?? []];
        }

        var key = Scribe_StringKeyDictionary.ProcessingKey;
        var curDefault = key != DefaultName ? DefaultSettingsFor(Scribe_StringKeyDictionary.ProcessingKey) : default;
        Scribe_Values.Look(ref enabled, "enabled", curDefault?.enabled ?? default);
        Scribe_Values.Look(ref repeatNorth, "repeatNorth", curDefault?.repeatNorth ?? default);
        Scribe_Values.Look(ref repeatSouth, "repeatSouth", curDefault?.repeatSouth ?? default);

        CheckAndLook(ref northUVs, curDefault?.northUVs, "northUVs");
        CheckAndLook(ref northVerts, curDefault?.northVerts, "northVerts");
        CheckAndLook(ref northVertsFiller, curDefault?.northVertsFiller, "northVertsFiller");
        CheckAndLook(ref northUVsFinish, curDefault?.northUVsFinish, "northUVsFinish");
        CheckAndLook(ref northVertsFinish, curDefault?.northVertsFinish, "northVertsFinish");
        CheckAndLook(ref borderFillerUVs, curDefault?.borderFillerUVs, "borderFillerUVs");
        CheckAndLook(ref northVertsFinishBorder, curDefault?.northVertsFinishBorder, "northVertsFinishBorder");
        CheckAndLook(ref southUVs, curDefault?.southUVs, "southUVs");
        CheckAndLook(ref southVerts, curDefault?.southVerts, "southVerts");
        CheckAndLook(ref southVertsFiller, curDefault?.southVertsFiller, "southVertsFiller");
        CheckAndLook(ref southUVsFinish, curDefault?.southUVsFinish, "southUVsFinish");
        CheckAndLook(ref southVertsFinish, curDefault?.southVertsFinish, "southVertsFinish");
        CheckAndLook(ref topFillerUVs, curDefault?.topFillerUVs, "topFillerUVs");
    }
}
