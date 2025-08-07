using UnityEngine;
using Verse;

namespace NanameWalls;

public class MeshSettings : IExposable
{
    private static Dictionary<string, MeshSettings> defaultSettings = [];

    private static readonly MeshSettings commonDefaultSettings = new()
    {
        enabled = true,
        repeatNorth = 3,
        northUVs =
        [
            new(0.283f, 0.47f),
            new(0f, 0.47f),
            new(0f, 0.89f),
            new(0.283f, 0.89f)
        ],
        northVerts =
        [
            new(-0.216f, 0f, 0.45f),
            new(-0.5f, 0.01f, 0.45f),
            new(0.5f, 0.01f, 1.45f),
            new(0.783f, 0f, 1.45f)
        ],
        northVertsFiller =
        [
            new(-0.216f, 0f, 0.45f),
            new(0.708f, 0.01f, 1.375f),
            new(1.005f, 0.01f, 1.195f),
            new(0.08f, 0f, 0.27f)
        ],
        northUVsFinish =
        [
            new(0.29f, 0.55f),
            new(0.29f, 0.945f),
            new(0.71f, 0.945f),
            new(0.71f, 0.55f)
        ],
        northVertsFinish =
        [
            new(0.38f, 0f, 0.097f),
            new(0.201f, 0.01f, 0.433f),
            new(0.701f, 0.01f, 0.933f),
            new(1.217f, 0f, 0.933f)
        ],
        borderFillerUVs =
        [
            new Vector3(0.5f, 0.025f),
            new Vector3(0.5f, 0.025f),
            new Vector3(0.5f, 0.025f),
            new Vector3(0.5f, 0.025f)
        ],
        northVertsFinishBorder =
        [
            new(0.202f, 0f, 0.434f),
            new(0.202f, 0.01f, 0.5f),
            new(0.5f, 0.01f, 0.798f),
            new(0.5f, 0f, 0.732f)
        ],
        repeatSouth = 3,
        southUVs =
        [
            new(0.29f, 0f),
            new(0.29f, 0.467f),
            new(0.71f, 0.467f),
            new(0.71f, 0f)
        ],
        southVerts =
        [
            new(0.475f, 0f, -0.5f),
            new(0.25f, 0.01f, -0.033f),
            new(1.25f, 0.01f, 0.967f),
            new(1.475f, 0f, 0.5f)
        ],
        southVertsFiller =
        [
            new(0.25f, 0f, -0.033f),
            new(0.08f, 0f, 0.27f),
            new(1.005f, 0.01f, 1.195f),
            new(1.175f, 0.01f, 0.892f)
        ],
        southUVsFinish =
        [
            new(0.29f, 0f),
            new(0.29f, 0.467f),
            new(0.71f, 0.467f),
            new(0.71f, 0f)
        ],
        southVertsFinish =
        [
            new(0.5f, 0f, 0.227f),
            new(0.217f, 0.01f, 0.407f),
            new(0.777f, 0.01f, 0.967f),
            new(0.777f, 0f, 0.5f)
        ],
        topFillerUVs =
        [
            new Vector3(0.5f, 0.6f),
            new Vector3(0.5f, 0.6f),
            new Vector3(0.5f, 0.6f),
            new Vector3(0.5f, 0.6f)
        ]
    };

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
        //return AccessTools.MakeDeepCopy<MeshSettings>(DefaultSettingsFor(def));
        var defaultSettings = DefaultSettingsFor(def);
        return new MeshSettings
        {
            enabled = defaultSettings.enabled,
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
            value ??= [.. defaultValue];
        }

        var curDefault = DefaultSettingsFor(Scribe_StringKeyDictionary.ProcessingKey);
        Scribe_Values.Look(ref enabled, "enabled", curDefault.enabled);
        Scribe_Values.Look(ref repeatNorth, "repeatNorth", curDefault.repeatNorth);
        Scribe_Values.Look(ref repeatSouth, "repeatSouth", curDefault.repeatSouth);

        CheckAndLook(ref northUVs, curDefault.northUVs, "northUVs");
        CheckAndLook(ref northVerts, curDefault.northVerts, "northVerts");
        CheckAndLook(ref northVertsFiller, curDefault.northVertsFiller, "northVertsFiller");
        CheckAndLook(ref northUVsFinish, curDefault.northUVsFinish, "northUVsFinish");
        CheckAndLook(ref northVertsFinish, curDefault.northVertsFinish, "northVertsFinish");
        CheckAndLook(ref borderFillerUVs, curDefault.borderFillerUVs, "borderFillerUVs");
        CheckAndLook(ref northVertsFinishBorder, curDefault.northVertsFinishBorder, "northVertsFinishBorder");
        CheckAndLook(ref southUVs, curDefault.southUVs, "southUVs");
        CheckAndLook(ref southVerts, curDefault.southVerts, "southVerts");
        CheckAndLook(ref southVertsFiller, curDefault.southVertsFiller, "southVertsFiller");
        CheckAndLook(ref southUVsFinish, curDefault.southUVsFinish, "southUVsFinish");
        CheckAndLook(ref southVertsFinish, curDefault.southVertsFinish, "southVertsFinish");
        CheckAndLook(ref topFillerUVs, curDefault.topFillerUVs, "topFillerUVs");
    }
}
