using RimWorld;
using UnityEngine;
using Verse;
using static NanameWalls.MeshSettings.SettingItem;
using static NanameWalls.ModCompat;

namespace NanameWalls;

[StaticConstructorOnStartup]
public class Graphic_LinkedDiagonal(Graphic subGraphic) : Graphic_LinkedCornerFiller(subGraphic)
{
    private static readonly Dictionary<Material, Material> materialCache = [];
    private const float AltitudeOffset = 0.015f;
    private static readonly int[] TrisIndex = [0, 1, 2, 0, 2, 3];
    private static readonly int[] TrisIndexFlipped = [0, 2, 1, 0, 3, 2];
    
    private static readonly Vector2[] CornerFillerUVs =
    [
        new(0.5f, 0.6f),
        new(0.5f, 0.6f),
        new(0.5f, 0.6f),
        new(0.5f, 0.6f)
    ];
    
    private static readonly Color32[] DefaultColors =
    [
        new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
        new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
        new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
        new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
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
        return ShouldLinkWith(c, parent);
    }

    public bool ShouldLinkWith(IntVec3 c, Thing parent, bool linkWithNormal = true)
    {
        var flag = base.ShouldLinkWith(c, parent);
        if (VehicleMapFramework.Active)
        {
            var offset = c - parent.Position;
            var rotated = offset.RotatedBy(VehicleMapFramework.RotForPrintCounter());
            c = rotated + parent.Position;
        }

        var map = parent.Map;
        var edifice = c.GetEdificeSafe(map);
        if (edifice is null)
            return flag;
        if (!linkWithNormal &&
            !NanameWalls.Mod.nanameWalls.ContainsValue(edifice.def.IsBlueprint
                ? edifice.def.entityDefToBuild as ThingDef ?? edifice.def
                : edifice.def))
        {
            return false;
        }
        if (NanameWalls.Mod.Settings.linkWithDifferentWall)
        {
            return flag;
        }
        return flag && edifice.def == parent.def && edifice.Stuff == parent.Stuff;
    }

    protected override Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
    {
        var entityDef = parent.def.IsBlueprint ? parent.def.entityDefToBuild as ThingDef ?? parent.def : parent.def;
        var originalDef = NanameWalls.Mod.originalDefs.GetValueOrDefault(entityDef, entityDef);
        var defName = originalDef.defName;
        if (!NanameWalls.Mod.Settings.meshSettings.TryGetValue(defName, out var settings))
        {
            settings = NanameWalls.Mod.Settings.meshSettings[defName] = MeshSettings.DeepCopyDefaultFor(entityDef);
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
        var southEast = ShouldLinkWith(pos + IntVec3.SouthEast, parent, false);
        var southWest = ShouldLinkWith(pos + IntVec3.SouthWest, parent, false);
        var northEast = ShouldLinkWith(pos + IntVec3.NorthEast, parent, false);
        var northWest = ShouldLinkWith(pos + IntVec3.NorthWest, parent, false);
        if ((diagonalFlag & Diagonals.NorthEast) > Diagonals.None && northEast)
        {
            linkDirections |= LinkDirections.Up;
        }
        if ((diagonalFlag & Diagonals.NorthWest) > Diagonals.None && northWest)
        {
            linkDirections |= LinkDirections.Up;
        }
        if ((diagonalFlag & Diagonals.SouthEast) > Diagonals.None && southEast)
        {
            linkDirections |= LinkDirections.Right;
        }
        if ((diagonalFlag & Diagonals.SouthWest) > Diagonals.None && southWest)    
        {
            linkDirections |= LinkDirections.Left;
        }

        var east2 = !settings.allowVShaped && ShouldLinkWith(pos + IntVec3.East * 2, parent);
        var west2 = !settings.allowVShaped && ShouldLinkWith(pos + IntVec3.West * 2, parent);
        var north2 = !settings.allowVShaped && ShouldLinkWith(pos + IntVec3.North * 2, parent);
        var south2 = !settings.allowVShaped && ShouldLinkWith(pos + IntVec3.South * 2, parent);
        if ((linkDirections & LinkDirections.Right) <= LinkDirections.None && !east2 && southEast ^ northEast)
        {
            if (southEast ? ClearFor(settings, Rot4.East, Rot4.North, parent) : ClearFor(settings, Rot4.East, Rot4.South, parent))
            {
                linkDirections |= LinkDirections.Right;
            }
        }
        if ((linkDirections & LinkDirections.Left) <= LinkDirections.None && !west2 && southWest ^ northWest)
        {
            if (southWest ? ClearFor(settings, Rot4.West, Rot4.North, parent) : ClearFor(settings, Rot4.West, Rot4.South, parent))
            {
                linkDirections |= LinkDirections.Left;
            }
        }
        if ((linkDirections & LinkDirections.Down) <= LinkDirections.None && southEast ^ southWest)
        {
            if (!south2 && (southEast ? ClearFor(settings, Rot4.South, Rot4.West, parent) : ClearFor(settings, Rot4.South, Rot4.East, parent)))
            {
                linkDirections |= LinkDirections.Down;
            }
        }
        if ((linkDirections & LinkDirections.Up) <= LinkDirections.None && northEast ^ northWest)
        {
            if (!north2 && (northEast ? ClearFor(settings, Rot4.North, Rot4.West, parent) : ClearFor(settings, Rot4.North, Rot4.East, parent)))
            {
                linkDirections |= LinkDirections.Up;
            }
        }
        return MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingleFor(parent), linkDirections);
    }

    private Material GetMaterial(Thing thing, UVSource source)
    {
        var baseMat = source == UVSource.Whole
            ? subGraphic.MatSingleFor(thing)
            : MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingleFor(thing), (LinkDirections)source);
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

    private static bool ClearFor(MeshSettings settings, Rot4 rot, Rot4 rot2, Thing thing)
    {
        if (VehicleMapFramework.Active)
        {
            var rotForPrintCounter = VehicleMapFramework.RotForPrintCounter();
            rot.AsInt += rotForPrintCounter.AsInt;
            rot2.AsInt += rotForPrintCounter.AsInt;
        }

        if (settings.forceDent && thing.Rotation == rot || settings.forceDentOpposite && thing.Rotation == rot.Opposite)
            return false;
        
        var pos = thing.Position + rot.AsIntVec3;
        var map = thing.Map;
        if (!pos.InBounds(map)) return false;
        
        // Substructureの境目では壁はへこまない: Propsの表示がうまくできないため
        if (ModsConfig.OdysseyActive)
        {
            var terrainDef = map.terrainGrid.FoundationAt(pos);
            var flag = terrainDef is { IsSubstructure: true };
            terrainDef = map.terrainGrid.FoundationAt(thing.Position);
            if (flag != terrainDef is { IsSubstructure: true })
            {
                return true;
            }
        }

        var forceDentPossibly = (pos + rot2.Opposite.FacingCell).GetEdificeSafe(map);
        if (forceDentPossibly is not null &&
            NanameWalls.Mod.originalDefs.TryGetValue(forceDentPossibly.def, out var originalDef) &&
            NanameWalls.Mod.Settings.meshSettings.TryGetValue(originalDef.defName, out var settings2) &&
            (settings2.forceDent && forceDentPossibly.Rotation == rot2 ||
             settings2.forceDentOpposite && forceDentPossibly.Rotation == rot2.Opposite))
            return false;

        if (pos.Roofed(map) &&
            (thing.TryGetComp<CompNanameWall>() is { DentRoofed: true } ||
            forceDentPossibly?.TryGetComp<CompNanameWall>() is { DentRoofed: true }))
            return false;

        if (pos.GetEdifice(map) is not null) return false;
        var opposite = rot.Opposite;
        var opposite2 = rot2.Opposite;
        foreach (var t in pos.GetThingList(map))
        {
            if (t.def.building is { isAttachment: true } && (t.Rotation == opposite || t.Rotation == opposite2))
                return false;
        }
        return true;
    }

    public override void Print(SectionLayer layer, Thing thing, float extraRotation)
    {
        var entityDef = thing.def.IsBlueprint ? thing.def.entityDefToBuild as ThingDef ?? thing.def : thing.def;
        var originalDef = NanameWalls.Mod.originalDefs.GetValueOrDefault(entityDef, entityDef);
        var defName = originalDef.defName;
        if (!NanameWalls.Mod.Settings.meshSettings.TryGetValue(defName, out var settings))
        {
            settings = NanameWalls.Mod.Settings.meshSettings[defName] = MeshSettings.DeepCopyDefaultFor(entityDef);
        }

        var pos = thing.Position;
        var north = ShouldLinkWith(pos + IntVec3.North, thing);
        var north2 = !settings.allowVShaped && ShouldLinkWith(pos + IntVec3.North * 2, thing);
        var east = ShouldLinkWith(pos + IntVec3.East, thing);
        var east2 = !settings.allowVShaped && ShouldLinkWith(pos + IntVec3.East * 2, thing);
        var west = ShouldLinkWith(pos + IntVec3.West, thing);
        var west2 = !settings.allowVShaped && ShouldLinkWith(pos + IntVec3.West * 2, thing);
        var south = ShouldLinkWith(pos + IntVec3.South, thing);
        var northEast = ShouldLinkWith(pos + IntVec3.NorthEast, thing);
        var northWest = ShouldLinkWith(pos + IntVec3.NorthWest, thing);
        var southEast = ShouldLinkWith(pos + IntVec3.SouthEast, thing);
        var southWest = ShouldLinkWith(pos + IntVec3.SouthWest, thing);
        var flag = Diagonals.None;
        if (northEast)
        {
            if (!east && !east2 && (settings.allowVShaped || !southEast) && ClearFor(settings, Rot4.East, Rot4.South, thing))
            {
                flag |= Diagonals.SouthEast;
                if (north)
                {
                    flag |= Diagonals.NoFinalize;
                }
            }
            if (!north && !north2 && (settings.allowVShaped || !northWest) && ClearFor(settings, Rot4.North, Rot4.West, thing))
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
            if (!west && !west2 && (settings.allowVShaped || !southWest) && ClearFor(settings, Rot4.West, Rot4.South, thing))
            {
                flag |= Diagonals.SouthWest;
                if (north)
                {
                    flag |= Diagonals.NoFinalize;
                }
            }
            if (!north && !north2 && (settings.allowVShaped || !northEast) && ClearFor(settings, Rot4.North, Rot4.East, thing))
            {
                flag |= Diagonals.NorthEast;
                if (west)
                {
                    flag |= Diagonals.NoFinalize;
                }
            }
        }
        diagonalFlag = flag;
        
        var center = thing.TrueCenter().WithYOffset(AltitudeOffset);
        
        // Original printing
        var anyLinked = north || south || east || west;
        var halfLinked = !north && !south && east ^ west;
        var cornerFiller = originalDef.graphicData?.linkType == LinkDrawerType.CornerFiller;
        if (!settings.skipOriginalPrint || anyLinked && !halfLinked)
        {
            if (OpenTheWindows.Active && thing.def.thingClass.SameOrSubclassOf(OpenTheWindows.Building_Window))
            {
                subGraphic.Print(layer, thing, extraRotation);
            }
            else if (cornerFiller)
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
            PrintConditional(layer, thing, settings, center, extraRotation, east, Condition.HalfLinked);
        if (!anyLinked)
            PrintConditional(layer, thing, settings, center, extraRotation, false, Condition.NoLinked);
        
        // 北東方面の斜め接続フラグが両方立っておりかつ別の種類の壁が建っている場合
        if ((flag & (Diagonals.NorthWest | Diagonals.SouthEast)) == (Diagonals.NorthWest | Diagonals.SouthEast) &&
            (pos + IntVec3.NorthEast).GetEdificeSafe(thing.Map) is { } edifice &&
            (edifice.def != thing.def || edifice.Stuff != thing.Stuff))
            PrintConditional(layer, thing, settings, center, extraRotation, false, Condition.LinkedOther);
        if ((flag & (Diagonals.NorthEast | Diagonals.SouthWest)) == (Diagonals.NorthEast | Diagonals.SouthWest) &&
            (pos + IntVec3.NorthWest).GetEdificeSafe(thing.Map) is { } edifice2 &&
            (edifice2.def != thing.def || edifice2.Stuff != thing.Stuff))
            PrintConditional(layer, thing, settings, center, extraRotation, true, Condition.LinkedOther);
        if (southEast && ClearFor(settings, Rot4.South, Rot4.West, thing) &&
            (pos + IntVec3.SouthEast).GetEdificeSafe(thing.Map) is { } edifice3 &&
            (edifice3.def != thing.def || edifice3.Stuff != thing.Stuff))
            PrintConditional(layer, thing, settings, center + new Vector3(-1f, 0f, -1f), extraRotation, true, Condition.LinkedOther);
        if (southWest && ClearFor(settings, Rot4.South, Rot4.East, thing) &&
            (pos + IntVec3.SouthWest).GetEdificeSafe(thing.Map) is { } edifice4 &&
            (edifice4.def != thing.def || edifice4.Stuff != thing.Stuff))
            PrintConditional(layer, thing, settings, center + new Vector3(-1f, 0f, -1f), extraRotation, false, Condition.LinkedOther);

        // Diagonal printing
        for (var i = 0; i < 4; i++)
        {
            var direction = (Diagonals)(1 << i);
            if ((flag & direction) > Diagonals.None)
            {
                PrintDiagonal(layer, thing, settings, center, extraRotation, direction);
            }
        }

        if (flag != Diagonals.None && (flag & flag - 1) == 0) // 立っているフラグが1つであることを確認している
        {
            PrintDiagonal(layer, thing, settings, center, extraRotation, flag, true);
        }
        if ((flag & (Diagonals.SouthEast | Diagonals.SouthWest)) == (Diagonals.SouthEast | Diagonals.SouthWest) && !north)
        {
            PrintDiagonal(layer, thing, settings, center, extraRotation, Diagonals.SouthEast, true);
            PrintDiagonal(layer, thing, settings, center, extraRotation, Diagonals.SouthWest, true);
        }

        if (!cornerFiller) return;

        //CornerFillers
        var mat = GetMaterial(thing, UVSource.None);
        if (north)
        {
            if ((flag & Diagonals.SouthEast) > Diagonals.None)
            {
                Printer_Plane.PrintPlane(layer, center + new Vector3(0.4f, 0f, 0.75f).RotatedBy(extraRotation), Vector2.one * 0.75f, mat, extraRotation, false, CornerFillerUVs);
            }
            if ((flag & Diagonals.SouthWest) > Diagonals.None)
            {
                Printer_Plane.PrintPlane(layer, center + new Vector3(-0.4f, 0f, 0.75f).RotatedBy(extraRotation), Vector2.one * 0.75f, mat, extraRotation, false, CornerFillerUVs);
            }
        }
        if (east && (flag & Diagonals.NorthWest) > Diagonals.None)
        {
            Printer_Plane.PrintPlane(layer, center + new Vector3(0.55f, 0f, 0.5f).RotatedBy(extraRotation), Vector2.one * 0.75f, mat, extraRotation, false, CornerFillerUVs);
            Printer_Plane.PrintPlane(layer, center + new Vector3(0.75f, 0f, 0.75f).RotatedBy(extraRotation), Vector2.one * 0.5f, mat, extraRotation, false, CornerFillerUVs);
        }
        if (west && (flag & Diagonals.NorthEast) > Diagonals.None)
        {
            Printer_Plane.PrintPlane(layer, center + new Vector3(-0.55f, 0f, 0.5f).RotatedBy(extraRotation), Vector2.one * 0.75f, mat, extraRotation, false, CornerFillerUVs);
            Printer_Plane.PrintPlane(layer, center + new Vector3(-0.75f, 0f, 0.75f).RotatedBy(extraRotation), Vector2.one * 0.5f, mat, extraRotation, false, CornerFillerUVs);
        }
    }

    private void PrintConditional(SectionLayer layer, Thing thing, MeshSettings settings, Vector3 center, float extraRotation, bool flipped, Condition condition)
    {
        var index = flipped ? TrisIndexFlipped : TrisIndex;

        foreach (var vertsItem in settings.settingItems.Values)
        {
            if (!vertsItem.visible || vertsItem.condition != condition ||
                !settings.settingItems.TryGetValue(vertsItem.link, out var linkUVs)) continue;

            var mat = GetMaterial(thing, linkUVs.source);
            var subMesh = layer.GetSubMesh(mat);
            var count = subMesh.verts.Count;
            var count2 = count;
            var verts = vertsItem.vectors;
            var uvs = linkUVs.vectors;
            var repeat = RepeatCount(settings, vertsItem.label);

            for (var i = 0; i < repeat; i++)
            {
                var num = i / (float)repeat;
                var num2 = (i + 1) / (float)repeat;
                subMesh.verts.Add(verts[0] + (verts[3] - verts[0]) * num);
                subMesh.verts.Add(verts[1] + (verts[2] - verts[1]) * num);
                subMesh.verts.Add(verts[1] + (verts[2] - verts[1]) * num2);
                subMesh.verts.Add(verts[0] + (verts[3] - verts[0]) * num2);
                subMesh.uvs.AddRange(uvs);
                subMesh.colors.AddRange(DefaultColors);
                
                for (var j = 0; j < 6; j++)
                {
                    subMesh.tris.Add(count2 + index[j]);
                }
                count2 += 4;
            }

            FinalizeVerts(subMesh, count, center, flipped, extraRotation);
        }
    }

    private void PrintDiagonal(SectionLayer layer, Thing thing, MeshSettings settings, Vector3 center, float extraRotation, Diagonals direction, bool finishPrint = false)
    {
        var north = direction is Diagonals.NorthEast or Diagonals.NorthWest;
        var flipped = direction is Diagonals.NorthEast or Diagonals.SouthWest;
        var index = flipped ? TrisIndexFlipped : TrisIndex;

        var vertsDirection = north ? finishPrint ? Condition.SouthFinish : Condition.North : finishPrint ? Condition.NorthFinish : Condition.South;
        foreach (var vertsItem in settings.settingItems.Values)
        {
            if (!vertsItem.visible || vertsItem.condition != vertsDirection ||
                !settings.settingItems.TryGetValue(vertsItem.link, out var linkUVs)) continue;
            
            var mat = GetMaterial(thing, linkUVs.source);
            var subMesh = layer.GetSubMesh(mat);
            var count = subMesh.verts.Count;
            var count2 = count;
            var verts = vertsItem.vectors;
            var uvs = linkUVs.vectors;
            var repeat = RepeatCount(settings, vertsItem.label);

            for (var i = 0; i < repeat; i++)
            {
                var num = i / (float)repeat;
                var num2 = (i + 1) / (float)repeat;
                subMesh.verts.Add(verts[0] + (verts[3] - verts[0]) * num);
                subMesh.verts.Add(verts[1] + (verts[2] - verts[1]) * num);
                subMesh.verts.Add(verts[1] + (verts[2] - verts[1]) * num2);
                subMesh.verts.Add(verts[0] + (verts[3] - verts[0]) * num2);
                subMesh.uvs.AddRange(uvs);
                subMesh.colors.AddRange(DefaultColors);
                for (var j = 0; j < 6; j++)
                {
                    subMesh.tris.Add(count2 + index[j]);
                }
                count2 += 4;
            }

            FinalizeVerts(subMesh, count, center, flipped, extraRotation);
        }
    }

    private static int RepeatCount(MeshSettings settings, string link)
    {
        foreach (var item in settings.settingItems.Values)
        {
            if (item.link == link)
            {
                return item.repeat;
            }
        }
        return 1;
    }

    private static void FinalizeVerts(LayerSubMesh subMesh, int skip, Vector3 center, bool flipped, float rot)
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
