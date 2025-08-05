using UnityEngine;
using Verse;

namespace NanameWalls;

public class WidgetsEx
{
    public static void DrawPolygonFilled(List<Vector3> points, Color color)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }
        if (points.Count < 3)
        {
            return;
        }

        var mat = SolidColorMaterials.SimpleSolidColorMaterial(color);
        mat.SetPass(0);
        GL.Begin(GL.TRIANGLES);
        GL.Color(color);

        Vector3 anchor = points[0];
        for (var i = 1; i < points.Count - 1; i++)
        {
            GL.Vertex(anchor);
            GL.Vertex(points[i]);
            GL.Vertex(points[i + 1]);
        }

        GL.End();
    }
}
