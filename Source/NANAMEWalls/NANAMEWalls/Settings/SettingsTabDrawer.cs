using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NanameWalls;

internal abstract class SettingsTabDrawer
{
    public abstract int Index { get; }

    public abstract string Label { get; }

    protected abstract bool DrawDefaultButton { get; }

    protected virtual void ResetSettings()
    {
        SoundDefOf.Click.PlayOneShotOnCamera();
    }

    protected readonly Vector2 ResetButtonSize = new(150f, 35f);

    public virtual void Draw(Rect inRect)
    {
        if (DrawDefaultButton)
        {
            var rect = new Rect(inRect.xMax - ResetButtonSize.x, inRect.yMax - ResetButtonSize.y, ResetButtonSize.x, ResetButtonSize.y);
            if (Widgets.ButtonText(rect, "Default".Translate()))
            {
                ResetSettings();
            }
        }
    }

    public virtual void PreClose() { }
}