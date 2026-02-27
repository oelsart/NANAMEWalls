using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace NanameWalls;

[PublicAPI]
public class NanameWalls : Mod
{
    public static NanameWalls Mod { get; private set; }

    public Settings Settings { get; private set; }

    internal Harmony Harmony { get; private set; }

    public const string Suffix = "_NAWDiagonal";

    public readonly HashSet<DesignationCategoryDef> designationCategories = [];
    
    public readonly HashSet<ThingCategoryDef> thingCategories = [];

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
        Harmony.Patch(
            AccessTools.Method(typeof(GraphicUtility), nameof(GraphicUtility.WrapLinked)),
            prefix: new HarmonyMethod(typeof(Patch_GraphicUtility_WrapLinked), nameof(Patch_GraphicUtility_WrapLinked.Prefix)));
    }

    public override string SettingsCategory()
    {
        return ModsConfig.IsActive("OELS.NanameWalls") ? "NANAME Walls" : "";
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
        CurrentTab?.Draw(rect.ContractedBy(5f));
    }
}

public static class Patch_GraphicUtility_WrapLinked
{
    public static bool Prefix(Graphic subGraphic, LinkDrawerType linkDrawerType, ref Graphic_Linked __result)
    {
        if (linkDrawerType != Graphic_LinkedDiagonal.LinkerTypeStatic) return true;
        __result = new Graphic_LinkedDiagonal(subGraphic);
        return false;
    }
}