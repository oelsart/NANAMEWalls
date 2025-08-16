using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NanameWalls;

public class WidgetsEx
{
    public static void DrawQuadFilled(List<Vector3> points, Color color, Material mat = null, List<Vector2> texCoords = null, int repeat = 1)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }
        if (points.Count != 4)
        {
            return;
        }

        mat ??= SolidColorMaterials.SimpleSolidColorMaterial(color);
        mat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(color);

        if (texCoords != null && texCoords.Count == 4)
        {
            for (var i = 0; i < repeat; i++)
            {
                var num = i / (float)repeat;
                var num2 = (i + 1) / (float)repeat;
                GL.TexCoord2(texCoords[0].x, texCoords[0].y);
                GL.Vertex(points[0] + ((points[3] - points[0]) * num));
                GL.TexCoord2(texCoords[1].x, texCoords[1].y);
                GL.Vertex(points[1] + ((points[2] - points[1]) * num));
                GL.TexCoord2(texCoords[2].x, texCoords[2].y);
                GL.Vertex(points[1] + ((points[2] - points[1]) * num2));
                GL.TexCoord2(texCoords[3].x, texCoords[3].y);
                GL.Vertex(points[0] + ((points[3] - points[0]) * num2));
            }
        }
        else
        {
            for (var i = 0; i < repeat; i++)
            {
                var num = i / (float)repeat;
                var num2 = (i + 1) / (float)repeat;
                GL.Vertex(points[0] + ((points[3] - points[0]) * num));
                GL.Vertex(points[1] + ((points[2] - points[1]) * num));
                GL.Vertex(points[1] + ((points[2] - points[1]) * num2));
                GL.Vertex(points[0] + ((points[3] - points[0]) * num2));
            }
        }
        GL.End();
    }

    public static void DefLabelEllipsesWithIcon(Rect rect, Def def, float iconMargin = 2f, float textOffsetX = 6f)
    {
        Widgets.DrawHighlightIfMouseover(rect);
        TooltipHandler.TipRegion(rect, def.description);
        Widgets.BeginGroup(rect);
        Rect rect2 = new(0f, 0f, rect.height, rect.height);
        if (iconMargin != 0f)
        {
            rect2 = rect2.ContractedBy(iconMargin);
        }
        Widgets.DefIcon(rect2, def, null, 1f, null, true, null, null, null, 1f);
        Rect rect3 = new(rect2.xMax + textOffsetX, 0f, rect.width, rect.height);
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.WordWrap = false;
        Widgets.LabelEllipses(rect3, def.LabelCap);
        Text.Anchor = TextAnchor.UpperLeft;
        Text.WordWrap = true;
        Widgets.EndGroup();
    }

    public static TTabRecord DrawTabs<TTabRecord>(Rect baseRect, List<TTabRecord> tabs, float tabHeight = 32f, float maxTabWidth = 200f) where TTabRecord : TabRecord
    {
        TTabRecord val = null;
        TTabRecord val2 = tabs.Find((TTabRecord t) => t.Selected);
        float num = baseRect.width + ((tabs.Count - 1) * 10f);
        float tabWidth = num / tabs.Count;
        if (tabWidth > maxTabWidth)
        {
            tabWidth = maxTabWidth;
        }
        Rect rect = new Rect(baseRect);
        rect.y -= tabHeight;
        rect.height = 9999f;
        Widgets.BeginGroup(rect);
        Text.Anchor = TextAnchor.MiddleCenter;
        Text.Font = GameFont.Small;
        Rect Func(TTabRecord tab) => new(tabs.IndexOf(tab) * (tabWidth - 10f), 1f, tabWidth, tabHeight);
        List<TTabRecord> list = tabs.ListFullCopy();
        if (val2 != null)
        {
            list.Remove(val2);
            list.Add(val2);
        }
        TabRecord tabRecord = null;
        List<TTabRecord> list2 = list.ListFullCopy();
        list2.Reverse();
        for (int num2 = 0; num2 < list2.Count; num2++)
        {
            TTabRecord val3 = list2[num2];
            Rect rect2 = Func(val3);
            if (tabRecord == null && Mouse.IsOver(rect2))
            {
                tabRecord = val3;
            }
            MouseoverSounds.DoRegion(rect2, SoundDefOf.Mouseover_Tab);
            if (Mouse.IsOver(rect2) && !val3.GetTip().NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect2, val3.GetTip());
            }
            if (Widgets.ButtonInvisible(rect2))
            {
                val = val3;
            }
        }
        foreach (TTabRecord item in list)
        {
            Rect rect3 = Func(item);
            item.Draw(rect3);
        }
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.EndGroup();
        if (val != null && val != val2)
        {
            SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
            val.clickedAction?.Invoke();
        }
        return val;
    }
}
