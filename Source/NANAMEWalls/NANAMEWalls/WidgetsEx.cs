using UnityEngine;
using Verse;

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
}
