using RimWorld;
using UnityEngine;
using Verse;
using static NanameWalls.MeshSettings.SettingItem;
using static NanameWalls.ModCompat;

namespace NanameWalls;

[StaticConstructorOnStartup]
[HotSwap]
public class Graphic_LinkedDiagonal(Graphic subGraphic) : Graphic_LinkedCornerFiller(subGraphic)
{
    private static readonly Dictionary<Material, Material> materialCache = [];

    private static readonly int[] TrisIndex = [0, 1, 2, 0, 2, 3];

    private static readonly int[] TrisIndexFlipped = [0, 2, 1, 0, 3, 2];

    private static readonly Vector2[] CornerFillerUVs =
    [
        new(0.5f, 0.6f),
        new(0.5f, 0.6f),
        new(0.5f, 0.6f),
        new(0.5f, 0.6f)
    ];

    protected Diagonals diagonalFlag;

    public const int LinkerNumber = 217;

    public static LinkDrawerType LinkerTypeStatic => (LinkDrawerType)LinkerNumber;

    public override LinkDrawerType LinkerType => (LinkDrawerType)LinkerNumber;

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new Graphic_LinkedDiagonal(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
        {
            data = data
        };
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        var flag = base.ShouldLinkWith(c, parent);
        if (NanameWalls.Mod.Settings.linkWithDifferentWall)
        {
            return flag;
        }
        if (VehicleMapFramework.Active)
        {
            var offset = c - parent.Position;
            var rotated = offset.RotatedBy(VehicleMapFramework.RotForPrintCounter());
            c = rotated + parent.Position;
        }
        var edifice = c.GetEdificeSafe(parent.Map);
        if (edifice is null)
        {
            return false;
        }
        return flag && edifice.def == parent.def && edifice.Stuff == parent.Stuff;
    }

    protected override Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
    {
        if (!NanameWalls.Mod.originalDefs.TryGetValue(parent.def, out var originalDef))
        {
            originalDef = parent.def;
        }
        var defName = originalDef.defName;
        if (!NanameWalls.Mod.Settings.meshSettings.TryGetValue(defName, out var settings))
        {
            settings = NanameWalls.Mod.Settings.meshSettings[defName] = MeshSettings.DeepCopyDefaultFor(parent.def);
        }
        if (settings.noChangeLinkState)
        {
            return base.LinkedDrawMatFrom(parent, cell);
        }

        var num = 0;
        var num2 = 1;
        for (var i = 0; i < 4; i++)
        {
            var intVec = cell + GenAdj.CardinalDirections[i];
            if (ShouldLinkWith(intVec, parent))
            {
                num += num2;
            }
            num2 *= 2;
        }
        var linkDirections = (LinkDirections)num;

        var pos = parent.Position;
        if (diagonalFlag.HasFlag(Diagonals.NorthEast))
        {
            linkDirections |= LinkDirections.Up;
        }
        if (diagonalFlag.HasFlag(Diagonals.NorthWest))
        {
            linkDirections |= LinkDirections.Up;
        }
        if (diagonalFlag.HasFlag(Diagonals.SouthEast))
        {
            linkDirections |= LinkDirections.Right;
        }
        if (diagonalFlag.HasFlag(Diagonals.SouthWest))
        {
            linkDirections |= LinkDirections.Left;
        }

        var southEast = ShouldLinkWith(pos + IntVec3.SouthEast, parent);
        var southWest = ShouldLinkWith(pos + IntVec3.SouthWest, parent);
        var northEast = ShouldLinkWith(pos + IntVec3.NorthEast, parent);
        var northWest = ShouldLinkWith(pos + IntVec3.NorthWest, parent);
        var east2 = !settings.allowVShaped && ShouldLinkWith(pos + (IntVec3.East * 2), parent);
        var west2 = !settings.allowVShaped && ShouldLinkWith(pos + (IntVec3.West * 2), parent);
        var north2 = !settings.allowVShaped && ShouldLinkWith(pos + (IntVec3.North * 2), parent);
        var south2 = !settings.allowVShaped && ShouldLinkWith(pos + (IntVec3.South * 2), parent);
        if (!linkDirections.HasFlag(LinkDirections.Right) && !east2 && (southEast ^ northEast))
        {
            if (southEast ? ClearFor(Rot4.East, Rot4.North, parent) : ClearFor(Rot4.East, Rot4.South, parent))
            {
                linkDirections |= LinkDirections.Right;
            }
        }
        if (!linkDirections.HasFlag(LinkDirections.Left) && !west2 && (southWest ^ northWest))
        {
            if (southWest ? ClearFor(Rot4.West, Rot4.North, parent) : ClearFor(Rot4.West, Rot4.South, parent))
            {
                linkDirections |= LinkDirections.Left;
            }
        }
        if (!linkDirections.HasFlag(LinkDirections.Down) && (southEast ^ southWest))
        {
            if (!south2 && (southEast ? ClearFor(Rot4.South, Rot4.West, parent) : ClearFor(Rot4.South, Rot4.East, parent)))
            {
                linkDirections |= LinkDirections.Down;
            }
        }
        // ReSharper disable once InvertIf
        if (!linkDirections.HasFlag(LinkDirections.Up) && (northEast ^ northWest))
        {
            if (!north2 && (northEast ? ClearFor(Rot4.North, Rot4.West, parent) : ClearFor(Rot4.North, Rot4.East, parent)))
            {
                linkDirections |= LinkDirections.Up;
            }
        }
        return MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingleFor(parent), linkDirections);
    }

    private Material GetMaterial(Thing thing)
    {
        var baseMat = MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingleFor(thing), LinkDirections.None);
        if (materialCache.TryGetValue(baseMat, out var mat))
        {
            return mat;
        }
        mat = new Material(baseMat);
        // ZWriteがOffであろう半透明シェーダーのキューの場合はいっこ後にしとく
        if (mat.renderQueue >= 3000)
        {
            mat.renderQueue++;
        }
        materialCache[baseMat] = mat;
        return mat;
    }

    private static bool ClearFor(Rot4 rot, Rot4 rot2, Thing thing)
    {
        if (VehicleMapFramework.Active)
        {
            var rotForPrintCounter = VehicleMapFramework.RotForPrintCounter();
            rot.AsInt += rotForPrintCounter.AsInt;
            rot2.AsInt += rotForPrintCounter.AsInt;
        }
        var pos = thing.Position + rot.AsIntVec3;
        if (!pos.InBounds(thing.Map)) return false;
        
        // Substructureの境目では壁はへこまない: Propsの表示がうまくできないため
        if (ModsConfig.OdysseyActive)
        {
            var terrainDef = thing.Map.terrainGrid.FoundationAt(pos);
            var flag = terrainDef is { IsSubstructure: true };
            terrainDef = thing.Map.terrainGrid.FoundationAt(thing.Position);
            if (flag != terrainDef is { IsSubstructure: true })
            {
                return true;
            }
        }
        
        if (pos.GetEdifice(thing.Map) != null) return false;
        var opposite = rot.Opposite;
        var opposite2 = rot2.Opposite;
        return !pos.GetThingList(thing.Map).Any(t => (t.def.building?.isAttachment ?? false) && (t.Rotation == opposite || t.Rotation == opposite2));
    }

    public override void Print(SectionLayer layer, Thing thing, float extraRotation)
    {
        if (!NanameWalls.Mod.originalDefs.TryGetValue(thing.def, out var originalDef))
        {
            originalDef = thing.def;
        }
        var defName = originalDef.defName;
        if (!NanameWalls.Mod.Settings.meshSettings.TryGetValue(defName, out var settings))
        {
            settings = NanameWalls.Mod.Settings.meshSettings[defName] = MeshSettings.DeepCopyDefaultFor(thing.def);
        }

        var pos = thing.Position;
        var north = ShouldLinkWith(pos + IntVec3.North, thing);
        var north2 = !settings.allowVShaped && ShouldLinkWith(pos + (IntVec3.North * 2), thing);
        var east = ShouldLinkWith(pos + IntVec3.East, thing);
        var east2 = !settings.allowVShaped && ShouldLinkWith(pos + (IntVec3.East * 2), thing);
        var west = ShouldLinkWith(pos + IntVec3.West, thing);
        var west2 = !settings.allowVShaped && ShouldLinkWith(pos + (IntVec3.West * 2), thing);
        var south = ShouldLinkWith(pos + IntVec3.South, thing);
        var northEast = ShouldLinkWith(pos + IntVec3.NorthEast, thing);
        var northWest = ShouldLinkWith(pos + IntVec3.NorthWest, thing);
        var southEast = ShouldLinkWith(pos + IntVec3.SouthEast, thing);
        var southWest = ShouldLinkWith(pos + IntVec3.SouthWest, thing);
        var flag = Diagonals.None;
        if (northEast)
        {
            if (!east && !east2 && (settings.allowVShaped || !southEast) && ClearFor(Rot4.East, Rot4.South, thing))
            {
                flag |= Diagonals.SouthEast;
                if (north)
                {
                    flag |= Diagonals.NoFinalize;
                }
            }
            if (!north && !north2 && (settings.allowVShaped || !northWest) && ClearFor(Rot4.North, Rot4.West, thing))
            {
                flag |= Diagonals.NorthWest;
                if (east)
                {
                    flag |= Diagonals.NoFinalize;
                }
            }
        }
        if (northWest)
        {
            if (!west && !west2 && (settings.allowVShaped || !southWest) && ClearFor(Rot4.West, Rot4.South, thing))
            {
                flag |= Diagonals.SouthWest;
                if (north)
                {
                    flag |= Diagonals.NoFinalize;
                }
            }
            if (!north && !north2 && (settings.allowVShaped || !northEast) && ClearFor(Rot4.North, Rot4.East, thing))
            {
                flag |= Diagonals.NorthEast;
                if (west)
                {
                    flag |= Diagonals.NoFinalize;
                }
            }
        }
        diagonalFlag = flag;

        var mat = GetMaterial(thing);
        var subMesh = layer.GetSubMesh(mat);
        var center = thing.TrueCenter().WithYOffset(Altitudes.AltInc);

        // Original printing
        var anyLinked = north || south || east || west;
        var halfLinked = !north && !south && (east ^ west);
        var cornerFiller = originalDef.graphicData?.linkType == LinkDrawerType.CornerFiller;
        if (!settings.skipOriginalPrint || anyLinked && !halfLinked)
        {
            if (cornerFiller)
            {
                base.Print(layer, thing, extraRotation);
            }
            else
            {
                var material = LinkedDrawMatFrom(thing, thing.Position);
                Printer_Plane.PrintPlane(layer, thing.TrueCenter(), new Vector2(1f, 1f), material, extraRotation);
                if (ShadowGraphic != null && thing != null)
                {
                    ShadowGraphic.Print(layer, thing, 0f);
                }
            }
        }

        // Half-linked printing
        if (halfLinked)
            PrintConditional(layer.GetSubMesh(MatSingle), settings, center, extraRotation, east, Condition.HalfLinked);
        if (!anyLinked)
            PrintConditional(layer.GetSubMesh(MatSingle), settings, center, extraRotation, false, Condition.NoLinked);

        // Diagonal printing
        foreach (Diagonals direction in Enum.GetValues(typeof(Diagonals)))
        {
            if (direction == Diagonals.None || direction == Diagonals.NoFinalize) continue;
            if (flag.HasFlag(direction))
            {
                PrintDiagonal(subMesh, settings, center, extraRotation, direction);
            }
        }

        if (flag != Diagonals.None && (flag & (flag - 1)) == 0)
        {
            PrintDiagonal(subMesh, settings, center, extraRotation, flag, true);
        }
        if (flag.HasFlag(Diagonals.SouthEast | Diagonals.SouthWest) && !north)
        {
            PrintDiagonal(subMesh, settings, center, extraRotation, Diagonals.SouthEast, true);
            PrintDiagonal(subMesh, settings, center, extraRotation, Diagonals.SouthWest, true);
        }

        if (!cornerFiller) return;

        //CornerFillers
        if (north)
        {
            if (flag.HasFlag(Diagonals.SouthEast))
            {
                Printer_Plane.PrintPlane(layer, center + new Vector3(0.4f, 0f, 0.75f).RotatedBy(extraRotation), Vector2.one * 0.75f, mat, extraRotation, false, CornerFillerUVs);
            }
            if (flag.HasFlag(Diagonals.SouthWest))
            {
                Printer_Plane.PrintPlane(layer, center + new Vector3(-0.4f, 0f, 0.75f).RotatedBy(extraRotation), Vector2.one * 0.75f, mat, extraRotation, false, CornerFillerUVs);
            }
        }
        if (east && flag.HasFlag(Diagonals.NorthWest))
        {
            Printer_Plane.PrintPlane(layer, center + new Vector3(0.55f, 0f, 0.5f).RotatedBy(extraRotation), Vector2.one * 0.75f, mat, extraRotation, false, CornerFillerUVs);
            Printer_Plane.PrintPlane(layer, center + new Vector3(0.75f, 0f, 0.75f).RotatedBy(extraRotation), Vector2.one * 0.5f, mat, extraRotation, false, CornerFillerUVs);
        }
        if (west && flag.HasFlag(Diagonals.NorthEast))
        {
            Printer_Plane.PrintPlane(layer, center + new Vector3(-0.55f, 0f, 0.5f).RotatedBy(extraRotation), Vector2.one * 0.75f, mat, extraRotation, false, CornerFillerUVs);
            Printer_Plane.PrintPlane(layer, center + new Vector3(-0.75f, 0f, 0.75f).RotatedBy(extraRotation), Vector2.one * 0.5f, mat, extraRotation, false, CornerFillerUVs);
        }
    }

    private void PrintConditional(LayerSubMesh subMesh, MeshSettings settings, Vector3 center, float extraRotation, bool flipped, Condition condition)
    {
        var index = flipped ? TrisIndexFlipped : TrisIndex;
        var count = subMesh.verts.Count;
        var count2 = count;

        foreach (var vertsItem in settings.settingItems.Values.Where(item => item.condition == condition))
        {
            if (!settings.settingItems.TryGetValue(vertsItem.link, out var linkUVs)) continue;
            var verts = vertsItem.vectors;
            var uvs = linkUVs.vectors;
            var repeat = settings.settingItems.Values.FirstOrDefault(item => item.link == vertsItem.label)?.repeat ?? 1;

            for (var i = 0; i < repeat; i++)
            {
                var num = i / (float)repeat;
                var num2 = (i + 1) / (float)repeat;
                subMesh.verts.Add(verts[0] + ((verts[3] - verts[0]) * num));
                subMesh.verts.Add(verts[1] + ((verts[2] - verts[1]) * num));
                subMesh.verts.Add(verts[1] + ((verts[2] - verts[1]) * num2));
                subMesh.verts.Add(verts[0] + ((verts[3] - verts[0]) * num2));
                subMesh.uvs.AddRange(uvs);
                for (var j = 0; j < 6; j++)
                {
                    subMesh.tris.Add(count2 + index[j]);
                }
                count2 += 4;
            }
        }

        FinalizeVerts(subMesh, count, center, flipped, extraRotation);
    }

    private void PrintDiagonal(LayerSubMesh subMesh, MeshSettings settings, Vector3 center, float extraRotation, Diagonals direction, bool finishPrint = false)
    {
        var north = direction == Diagonals.NorthEast || direction == Diagonals.NorthWest;
        var flipped = direction == Diagonals.NorthEast || direction == Diagonals.SouthWest;
        var index = flipped ? TrisIndexFlipped : TrisIndex;
        var count = subMesh.verts.Count;
        var count2 = count;

        var vertsDirection = north ? finishPrint ? Condition.SouthFinish : Condition.North : finishPrint ? Condition.NorthFinish : Condition.South;
        foreach (var vertsItem in settings.settingItems.Values.Where(item => item.condition == vertsDirection))
        {
            if (!settings.settingItems.TryGetValue(vertsItem.link, out var linkUVs)) continue;
            var verts = vertsItem.vectors;
            var uvs = linkUVs.vectors;
            var repeat = settings.settingItems.Values.FirstOrDefault(item => item.link == vertsItem.label)?.repeat ?? 1;

            for (var i = 0; i < repeat; i++)
            {
                var num = i / (float)repeat;
                var num2 = (i + 1) / (float)repeat;
                subMesh.verts.Add(verts[0] + ((verts[3] - verts[0]) * num));
                subMesh.verts.Add(verts[1] + ((verts[2] - verts[1]) * num));
                subMesh.verts.Add(verts[1] + ((verts[2] - verts[1]) * num2));
                subMesh.verts.Add(verts[0] + ((verts[3] - verts[0]) * num2));
                subMesh.uvs.AddRange(uvs);
                for (var j = 0; j < 6; j++)
                {
                    subMesh.tris.Add(count2 + index[j]);
                }
                count2 += 4;
            }
        }

        FinalizeVerts(subMesh, count, center, flipped, extraRotation);
    }

    private void FinalizeVerts(LayerSubMesh subMesh, int skip, Vector3 center, bool flipped, float rot)
    {
        for (var i = skip; i < subMesh.verts.Count; i++)
        {
            var vert = subMesh.verts[i];
            if (rot != 0)
            {
                vert = vert.RotatedBy(flipped ? -rot : rot);
            }
            if (flipped)
            {
                vert.x = -vert.x;
            }
            vert += center;
            subMesh.verts[i] = vert;
        }
    }

    [Flags]
    protected enum Diagonals
    {
        None = 0,
        NorthEast = 1 << 0,
        NorthWest = 1 << 1,
        SouthEast = 1 << 2,
        SouthWest = 1 << 3,
        NoFinalize = 1 << 4
    }
}
