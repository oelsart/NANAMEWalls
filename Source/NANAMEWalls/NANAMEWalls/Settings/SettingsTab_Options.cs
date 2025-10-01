using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NanameWalls;

[HotSwap]
internal class SettingsTab_Options : SettingsTabDrawer
{
    public override int Index => 1;

    public override string Label => "NAW.Settings.Options".Translate();

    public override bool DrawDefaultButton => true;

    public override void ResetSettings()
    {
        NanameWalls.Mod.Settings.groupNanameWalls = Settings.Default.groupNanameWalls;
        base.ResetSettings();
    }

    public override void Draw(Rect inRect)
    {
        base.Draw(inRect);
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);
        listing_Standard.CheckboxLabeled("NAW.Settings.GroupNanameWalls".Translate(), ref NanameWalls.Mod.Settings.groupNanameWalls);
        listing_Standard.CheckboxLabeled("NAW.Settings.LinkWithDifferentWall".Translate(), ref NanameWalls.Mod.Settings.linkWithDifferentWall);
        listing_Standard.CheckboxLabeled("NAW.Settings.RenderSubstructure".Translate(), ref NanameWalls.Mod.Settings.renderSubstructure);
        if (listing_Standard.ButtonText("NAW.Settings.ReloadDefaultSettings".Translate(), widthPct: 0.3f))
        {
            ReloadDefaultSettings();
        }
        listing_Standard.End();
    }

    private void ReloadDefaultSettings()
    {
        MeshSettings.Init();
        SoundDefOf.Click.PlayOneShotOnCamera(null);
    }
}