using UnityEngine;
using Verse;

namespace NanameWalls;

public class MeshSettings : IExposable
{
    public int repeatNorth = 3;

    public List<Vector3> northUVs = [.. Default.northUVs];

    public List<Vector3> northVerts = [.. Default.northVerts];

    public List<Vector3> northVertsFiller = [.. Default.northVertsFiller];

    public List<Vector3> northUVsFinish = [.. Default.northUVsFinish];

    public List<Vector3> northVertsFinish = [.. Default.northVertsFinish];

    public List<Vector3> northVertsFinishBorder = [.. Default.northVertsFinishBorder];

    public int repeatSouth = 3;

    public List<Vector3> southUVs = [.. Default.southUVs];

    public List<Vector3> southVerts = [.. Default.southVerts];

    public List<Vector3> southVertsFiller = [.. Default.southVertsFiller];

    public List<Vector3> southUVsFinish = [..Default.southUVsFinish];

    public List<Vector3> southVertsFinish = [.. Default.southVertsFinish];

    public void ExposeData()
    {
        Scribe_Values.Look(ref repeatNorth, "repeatNorth", Default.repeatNorth);

        Scribe_Collections.Look(ref northUVs, "northUVs", LookMode.Value);
        northUVs ??= [.. Default.northUVs];

        Scribe_Collections.Look(ref northVerts, "northVerts", LookMode.Value);
        northVerts ??= [.. Default.northVerts];

        Scribe_Collections.Look(ref northVertsFiller, "northVertsFiller", LookMode.Value);
        northVertsFiller ??= [.. Default.northVertsFiller];

        Scribe_Collections.Look(ref northUVsFinish, "northUVsFinish", LookMode.Value);
        northUVsFinish ??= [.. Default.northUVsFinish];

        Scribe_Collections.Look(ref northVertsFinish, "northVertsFinish", LookMode.Value);
        northVertsFinish ??= [.. Default.northVertsFinish];

        Scribe_Collections.Look(ref northVertsFinishBorder, "northVertsFinishBorder", LookMode.Value);
        northVertsFinishBorder ??= [.. Default.northVertsFinishBorder];

        Scribe_Values.Look(ref repeatSouth, "repeatSouth", Default.repeatSouth);

        Scribe_Collections.Look(ref southUVs, "southUVs", LookMode.Value);
        southUVs ??= [.. Default.southUVs];

        Scribe_Collections.Look(ref southVerts, "southVerts", LookMode.Value);
        southVerts ??= [.. Default.southVerts];

        Scribe_Collections.Look(ref southVertsFiller, "southVertsFiller", LookMode.Value);
        southVertsFiller ??= [.. Default.southVertsFiller];

        Scribe_Collections.Look(ref southUVsFinish, "southUVsFinish", LookMode.Value);
        southUVsFinish ??= [.. Default.southUVsFinish];

        Scribe_Collections.Look(ref southVertsFinish, "southVertsFinish", LookMode.Value);
        southVertsFinish ??= [.. Default.southVertsFinish];
    }

    private static class Default
    {
        public const int repeatNorth = 3;

        public static readonly List<Vector3> northUVs =
        [
            new(0.283f, 0.47f),
            new(0f, 0.47f),
            new(0f, 0.89f),
            new(0.283f, 0.89f)
        ];

        public static readonly List<Vector3> northVerts =
        [
            new(-0.216f, 0f, 0.45f),
            new(-0.5f, 0.01f, 0.45f),
            new(0.5f, 0.01f, 1.45f),
            new(0.783f, 0f, 1.45f)
        ];

        public static readonly List<Vector3> northVertsFiller =
        [
            new(-0.216f, 0f, 0.45f),
            new(0.708f, 0.01f, 1.375f),
            new(1.005f, 0.01f, 1.195f),
            new(0.08f, 0f, 0.27f)
        ];

        public static readonly List<Vector3> northUVsFinish =
        [
            new(0.29f, 0.55f),
            new(0.29f, 0.945f),
            new(0.71f, 0.945f),
            new(0.71f, 0.55f)
        ];

        public static readonly List<Vector3> northVertsFinish =
        [
            new(0.38f, 0f, 0.097f),
            new(0.201f, 0.01f, 0.433f),
            new(0.701f, 0.01f, 0.933f),
            new(1.217f, 0f, 0.933f)
        ];

        public static readonly List<Vector3> northVertsFinishBorder =
        [
            new(0.202f, 0f, 0.434f),
            new(0.202f, 0.01f, 0.5f),
            new(0.5f, 0.01f, 0.798f),
            new(0.5f, 0f, 0.732f)
        ];

        public const int repeatSouth = 3;

        public static readonly List<Vector3> southUVs =
        [
            new(0.29f, 0f),
            new(0.29f, 0.467f),
            new(0.71f, 0.467f),
            new(0.71f, 0f)
        ];

        public static readonly List<Vector3> southVerts =
        [
            new(0.475f, 0f, -0.5f),
            new(0.25f, 0.01f, -0.033f),
            new(1.25f, 0.01f, 0.967f),
            new(1.475f, 0f, 0.5f)
        ];

        public static readonly List<Vector3> southVertsFiller =
        [
            new(0.25f, 0f, -0.033f),
            new(0.08f, 0f, 0.27f),
            new(1.005f, 0.01f, 1.195f),
            new(1.175f, 0.01f, 0.892f)
        ];

        public static readonly List<Vector3> southUVsFinish =
        [
            new(0.29f, 0f),
            new(0.29f, 0.467f),
            new(0.71f, 0.467f),
            new(0.71f, 0f)
        ];

        public static readonly List<Vector3> southVertsFinish =
        [
            new(0.5f, 0f, 0.227f),
            new(0.217f, 0.01f, 0.407f),
            new(0.777f, 0.01f, 0.967f),
            new(0.777f, 0f, 0.5f)
        ];
    }
}
