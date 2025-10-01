using System.Xml;
using UnityEngine;
using Verse;

namespace NanameWalls;

public class MeshSettings : IExposable
{
    private const string DefaultName = "NAW_DefaultWall";

    private static Dictionary<string, MeshSettings> defaultSettings;

    public bool enabled;

    public bool noChangeLinkState;

    public bool skipOriginalPrint;

    public bool allowVShaped;

    public SortedDictionary<string, SettingItem> settingItems = [];

    private static MeshSettings CommonDefaultSettings
    {
        get
        {
            if (defaultSettings.TryGetValue(DefaultName, out var settings))
            {
                return settings;
            }
            return null;
        }
    }

    private MeshSettings() { }

    public static void Init()
    {
        defaultSettings = [];
        var settingsFilename = Path.Combine(NanameWalls.Mod.Content.RootDir, "DefaultSettings.xml");
        try
        {
            if (File.Exists(settingsFilename))
            {
                Scribe.loader.InitLoading(settingsFilename);
                try
                {
                    var childs = Scribe.loader.curXmlParent.ChildNodes;

                    foreach (XmlNode child in childs)
                    {
                        MeshSettings settings = null;
                        Scribe_Deep.Look(ref settings, child.Name);
                        defaultSettings[child.Name] = settings;
                    }
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
        return CommonDefaultSettings;
    }

    public static MeshSettings DeepCopyDefaultFor(ThingDef def)
    {
        static bool IsWallProbably(ThingDef def)
        {
            return (def.IsWall || def.defName.Contains("Wall")) &&
                def.passability == Traversability.Impassable;
        }

        var defaultSettings = DefaultSettingsFor(def);
        var enabled = defaultSettings == CommonDefaultSettings ? IsWallProbably(def) : defaultSettings.enabled;
        var copy = new MeshSettings
        {
            enabled = enabled
        };
        foreach (var item in defaultSettings.settingItems)
        {
            copy.settingItems[item.Key] = item.Value.DeepCopy();
        }
        return copy;
    }

    public void ExposeData()
    {
        var curDefault = DefaultSettingsFor(Scribe_StringKeyDictionary.ProcessingKey);
        Scribe_Values.Look(ref enabled, "enabled", curDefault?.enabled ?? default);
        Scribe_Values.Look(ref noChangeLinkState, "noChangeLinkState", curDefault?.noChangeLinkState ?? default);
        Scribe_Values.Look(ref skipOriginalPrint, "skipOriginalPrint", curDefault?.skipOriginalPrint ?? default);
        Scribe_Values.Look(ref allowVShaped, "allowVShaped", curDefault?.allowVShaped ?? default);
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            settingItems ??= [];
            List<string> ignoreList = [];
            var childs = Scribe.loader.curXmlParent.ChildNodes;
            foreach (XmlNode child in childs)
            {
                if (child.Name == "enabled" || child.Name == "noChangeLinkState" || child.Name == "skipOriginalPrint" || child.Name == "allowVShaped") continue;
                XmlAttribute xmlAttribute = child.Attributes["IsNull"];
                if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    ignoreList.Add(child.Name);
                    continue;
                }

                SettingItem item = new();
                if (!LookBackCompatibility(ref item, child.Name))
                {
                    Scribe_Deep.Look(ref item, child.Name);
                }
                settingItems[item.label] = item;
            }
            if (curDefault != null)
            {
                foreach (var defaultItem in curDefault.settingItems.Values.Where(i => !settingItems.ContainsKey(i.label) && !ignoreList.Contains(i.label)))
                {
                    settingItems[defaultItem.label] = defaultItem.DeepCopy();
                }
            }
        }
        else if (Scribe.mode == LoadSaveMode.Saving)
        {
            foreach (var item in settingItems)
            {
                var value = item.Value;
                if (!curDefault.settingItems.TryGetValue(item.Key, out var item2) || !value.Equals(item2))
                {
                    Scribe_Deep.Look(ref value, $"{item.Key}_{value.type}");
                }
            }
            if (curDefault != null)
            {
                SettingItem item = null;
                foreach (var defaultItem in curDefault.settingItems.Values.Where(i => !settingItems.ContainsKey(i.label)))
                {
                    Scribe_Deep.Look(ref item, defaultItem.label);
                }
            }
        }
    }

    private bool LookBackCompatibility(ref SettingItem item, string label)
    {
        switch (label)
        {
            case "repeatNorth":
                Scribe_Values.Look(ref item.repeat, label);
                item.label = "NorthVertsRepeat";
                item.type = SettingItem.SettingType.Repeat;
                item.link = "NorthVerts";
                return true;

            case "repeatSouth":
                Scribe_Values.Look(ref item.repeat, label);
                item.label = "SouthVertsRepeat";
                item.type = SettingItem.SettingType.Repeat;
                item.link = "SouthVerts";
                return true;

            case "northUVs":
            case "northUVsFinish":
            case "borderFillerUVs":
            case "southUVs":
            case "southUVsFinish":
            case "topFillerUVs":
                Scribe_Collections.Look(ref item.vectors, label);
                item.label = label.CapitalizeFirst();
                item.type = SettingItem.SettingType.UVs;
                return true;

            case "northVerts":
                Scribe_Collections.Look(ref item.vectors, label);
                item.label = label.CapitalizeFirst();
                item.link = "NorthUVs";
                item.type = SettingItem.SettingType.Verts;
                item.condition = SettingItem.Condition.North;
                return true;

            case "northVertsFiller":
                Scribe_Collections.Look(ref item.vectors, label);
                item.label = label.CapitalizeFirst();
                item.link = "TopFillerUVs";
                item.type = SettingItem.SettingType.Verts;
                item.condition = SettingItem.Condition.North;
                return true;

            case "northVertsFinish":
                Scribe_Collections.Look(ref item.vectors, label);
                item.label = label.CapitalizeFirst();
                item.link = "NorthUVsFinish";
                item.type = SettingItem.SettingType.Verts;
                item.condition = SettingItem.Condition.NorthFinish;
                return true;

            case "northVertsFinishBorder":
                Scribe_Collections.Look(ref item.vectors, label);
                item.label = label.CapitalizeFirst();
                item.link = "BorderFillerUVs";
                item.type = SettingItem.SettingType.Verts;
                item.condition = SettingItem.Condition.NorthFinish;
                return true;

            case "southVerts":
                Scribe_Collections.Look(ref item.vectors, label);
                item.label = label.CapitalizeFirst();
                item.link = "SouthUVs";
                item.type = SettingItem.SettingType.Verts;
                item.condition = SettingItem.Condition.South;
                return true;

            case "southVertsFiller":
                Scribe_Collections.Look(ref item.vectors, label);
                item.label = label.CapitalizeFirst();
                item.link = "TopFillerUVs";
                item.type = SettingItem.SettingType.Verts;
                item.condition = SettingItem.Condition.South;
                return true;

            case "southVertsFinish":
                Scribe_Collections.Look(ref item.vectors, label);
                item.label = label.CapitalizeFirst();
                item.link = "SouthUVsFinish";
                item.type = SettingItem.SettingType.Verts;
                item.condition = SettingItem.Condition.SouthFinish;
                return true;

            default:
                return false;
        }
    }

    public class SettingItem : IExposable, IRenameable
    {
        public string label = "";

        public SettingType type;

        public Condition condition;

        public int repeat = 1;

        public List<Vector3> vectors =
        [
            new(0f, 0f, 0f),
            new(0f, 0f, 0f),
            new(0f, 0f, 0f),
            new(0f, 0f, 0f)
        ];

        public string link = "";

        public string RenamableLabel
        {
            get
            {
                return label;
            }
            set
            {
                label = value;
            }
        }

        public string BaseLabel => label;

        public string InspectLabel => label;

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                var name = Scribe.loader.curXmlParent.Name;
                var values = name.Split("_");
                var last = values.Last();
                label = string.Join("_", values.Except(last));
                if (!Enum.IsDefined(typeof(SettingType), last))
                {
                    Log.Error($"[NanameWalls] Invalid element in settings: {name}");
                    return;
                }
                Enum.TryParse(last, true, out type);
            }
            switch (type)
            {
                case SettingType.UVs:
                    Scribe_Collections.Look(ref vectors, "UVs", LookMode.Value);
                    break;

                case SettingType.Verts:
                    Scribe_Values.Look(ref link, "linkUVs");
                    Scribe_Values.Look(ref condition, "condition");
                    LoadWithOldName(ref condition, "direction");
                    Scribe_Collections.Look(ref vectors, "Verts", LookMode.Value);
                    break;

                case SettingType.Repeat:
                    Scribe_Values.Look(ref link, "linkVerts");
                    Scribe_Values.Look(ref repeat, "repeat");
                    break;
            }
        }

        private void LoadWithOldName<T>(ref T value, string name, T defaultValue = default) where T : struct
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars || !value.Equals(defaultValue))
                return;
            var childNodes = Scribe.loader.curXmlParent?.ChildNodes;
            if (childNodes == null)
                return;
            for (var i = 0; i < childNodes.Count; i++)
            {
                var child = childNodes[i];
                if (child?.Name == name)
                {
                    Scribe_Values.Look(ref value, name, defaultValue);
                    return;
                }
            }
        }

        public bool Equals(SettingItem other)
        {
            if (other is null) return false;
            if (type != other.type) return false;
            if (condition != other.condition) return false;
            if (repeat != other.repeat) return false;
            var flag = vectors != null;
            var flag2 = other.vectors != null;
            if ((flag ^ flag2) || (flag && !vectors.SequenceEqual(other.vectors))) return false;
            if (link != other.link) return false;
            return true;
        }

        public SettingItem DeepCopy()
        {
            var copy = new SettingItem()
            {
                label = label,
                type = type,
                condition = condition,
                repeat = repeat,
                link = link,
            };
            if (vectors != null) copy.vectors = [.. vectors];
            return copy;
        }

        public enum SettingType
        {
            UVs,
            Verts,
            Repeat
        }

        public enum Condition
        {
            None,
            North,
            NorthFinish,
            South,
            SouthFinish,
            NoLinked,
            HalfLinked
        }
    }
}
