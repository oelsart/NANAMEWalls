using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;
using static NanameWalls.ModCompat;

namespace NanameWalls;

public class NanameWalls : Mod
{
    public static NanameWalls Mod { get; private set; }

    public Settings Settings { get; private set; }

    internal Harmony Harmony { get; private set; }

    public const string Suffix = "_NAWDiagonal";

    public readonly HashSet<DesignationCategoryDef> designationCategories = [];

    public readonly Dictionary<ThingDef, ThingDef> originalDefs = [];

    public readonly Dictionary<ThingDef, ThingDef> nanameWalls = [];

    private readonly List<TabRecord> tabs = [];

    private readonly List<SettingsTabDrawer> tabDrawers = [];

    public Thing selThing;

    public ThingDef selDef;

    internal SettingsTabDrawer CurrentTab { get; set; }

    public NanameWalls(ModContentPack content) : base(content)
    {
        Mod = this;
        MeshSettings.Init();
        Settings = GetSettings<Settings>();
        Harmony = new Harmony("OELS.NanameWalls");

        var assembly = Assembly.GetExecutingAssembly();
        GenTypes.AllTypes.Where(t => t.Assembly == assembly).Select(Harmony.CreateClassProcessor)
            .Do(patchClass =>
            {
                try
                {
                    if (patchClass.Category.NullOrEmpty())
                    {
                        patchClass.Patch();
                    }
                    if (ViviRace.Active && patchClass.Category == ViviRace.PatchCategory)
                    {
                        patchClass.Patch();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[NanameWalls] Error while apply patch: {ex}");
                }
            });
    }

    public override string SettingsCategory()
    {
        return "NANAME Walls";
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        tabDrawers.Do(tab => tab.PreClose());
    }

    public void InitializeTabs()
    {
        tabs.Clear();
        tabDrawers.AddRange(typeof(SettingsTabDrawer).AllSubclassesNonAbstract()
            .Select(Activator.CreateInstance).Cast<SettingsTabDrawer>()
            .OrderBy(tab => tab.Index));
        CurrentTab = tabDrawers[0];
        tabs.AddRange(tabDrawers.Select(tab => new TabRecord(tab.Label, () => CurrentTab = tab, () => CurrentTab == tab)));
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        if (CurrentTab == null)
        {
            InitializeTabs();
        }

        base.DoSettingsWindowContents(inRect);
        var rect = new Rect(inRect.x, inRect.y + TabDrawer.TabHeight, inRect.width, inRect.height - TabDrawer.TabHeight);
        Widgets.DrawMenuSection(rect);
        TabDrawer.DrawTabs(rect, tabs);
        CurrentTab.Draw(rect.ContractedBy(10f));
    }
}