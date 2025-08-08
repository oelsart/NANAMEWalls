using RimWorld;
using UnityEngine;
using Verse;

namespace NanameWalls;

internal class SettingsTab_WallSettings : SettingsTabDrawer
{
    private const float PreviewSizeRatio = 0.3f;

    private Vector2 scrollPosition;

    private Vector2 scrollPosition2;

    private readonly Dictionary<string, string[]> buffers = [];

    private List<Vector2> texCoords;

    private int repeat = 1;

    private List<Vector3> selectedList;

    private Vector2? selectedPoint;

    private string prevFocusedControl = "";

    private bool designationCategoryChanged;

    private string defaultRequest;

    private float viewRectYMax;

    public override int Index => 0;

    public override string Label => "NAW.Walls".Translate();

    public override bool DrawDefaultButton => false;

    private void Clear()
    {
        buffers.Clear();
        repeat = 1;
        selectedList = null;
        texCoords = null;
        selectedPoint = null;
        prevFocusedControl = "";
    }

    public override void PreClose()
    {
        Clear();

        if (designationCategoryChanged)
        {
            designationCategoryChanged = false;
            foreach (var designationCategory in NanameWalls.Mod.designationCategories)
            {
                designationCategory.ResolveReferences();
            }
        }
        NanameWalls.Mod.selThing = null;
    }

    public override void Draw(Rect inRect)
    {
        base.Draw(inRect);
        Widgets.DrawMenuSection(inRect);
        inRect.SplitVertically(200f, out var left, out var right);
        DoDefNameList(left);
        Widgets.DrawLineVertical(left.xMax, inRect.y, inRect.height);

        if (NanameWalls.Mod.selDef != null)
        {
            DoMeshSettings(right.ContractedBy(7f));
        }
    }

    private void DoDefNameList(Rect rect)
    {
        ref var selDef = ref NanameWalls.Mod.selDef;
        var outRect = rect;
        var defs = NanameWalls.Mod.nanameWalls.Keys.GroupBy(def => def.modContentPack).ToList();
        var height = Text.LineHeight * (defs.Count + defs.SelectMany(group => group).Count());
        var viewRect = new Rect(rect.x, rect.y, rect.width, height);
        Widgets.AdjustRectsForScrollView(rect, ref outRect, ref viewRect);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        var curY = viewRect.y;
        foreach (var group in defs)
        {
            if (group?.Key != null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                using (new TextBlock(TextAnchor.MiddleCenter))
                {
                    var name = group.Key.Name ?? "";
                    var rect2 = new Rect(viewRect.x, curY, viewRect.width, Text.LineHeight);
                    Widgets.DrawBoxSolidWithOutline(rect2, Widgets.InactiveColor, Color.white, 1);
                    if (Text.CalcSize(name).x > rect2.width)
                    {
                        Text.Font = GameFont.Tiny;
                    }
                    Widgets.LabelEllipses(rect2, name);
                }
                Text.Font = GameFont.Small;
                curY += Text.LineHeight;
            }

            foreach (var def in group)
            {
                var rect3 = new Rect(viewRect.x, curY, viewRect.width, Text.LineHeight);
                WidgetsEx.DefLabelEllipsesWithIcon(rect3, def, 5f);
                if (selDef == def)
                {
                    Widgets.DrawHighlightSelected(rect3);
                }
                else if (Widgets.ButtonInvisible(rect3))
                {
                    selDef = def;
                    Clear();
                }
                curY += Text.LineHeight;
            }
        }
        Widgets.EndScrollView();
    }

    private void DoMeshSettings(Rect rect)
    {
        ref var selDef = ref NanameWalls.Mod.selDef;
        ref var selThing = ref NanameWalls.Mod.selThing;
        var previewSize = rect.height * PreviewSizeRatio;
        if (!NanameWalls.Mod.nanameWalls.TryGetValue(selDef, out var nanameWall))
        {
            using (new TextBlock(TextAnchor.MiddleCenter))
            {
                Widgets.Label(rect, "NAW.NotGenerated".Translate());
            }
            return;
        }
        if (!NanameWalls.Mod.Settings.meshSettings.TryGetValue(selDef.defName, out var meshSettings))
        {
            meshSettings = NanameWalls.Mod.Settings.meshSettings[selDef.defName] = MeshSettings.DeepCopyDefaultFor(selDef);
        }

        rect.SplitHorizontally(previewSize, out var top, out var bottom);
        top.SplitVertically(previewSize, out var left, out var right);

        //壁のプレビュー
        Widgets.DrawBoxSolid(left, new Color(0.1f, 0.1f, 0.1f));
        var mat = selDef.graphic?.MatSingle;
        if (mat != null)
        {
            Widgets.DrawTextureFitted(left.LeftHalf().BottomHalf(), mat.mainTexture, 1f, new(previewSize / 2f, previewSize / 2f), new(0.03125f, 0.03125f, 0.1875f, 0.1875f), 0f);
            Widgets.DrawTextureFitted(left.RightHalf().TopHalf(), mat.mainTexture, 1f, new(previewSize / 2f, previewSize / 2f), new(0.03125f, 0.03125f, 0.1875f, 0.1875f), 0f);
        }
        if (selectedList != null)
        {
            Widgets.BeginGroup(left);
            var mat2 = texCoords != null ? mat : null;
            var color = texCoords != null ? Color.white : new Color(1f, 0.94f, 0.5f, 0.18f);
            WidgetsEx.DrawQuadFilled(selectedList, color, mat2, texCoords, repeat);
            Widgets.EndGroup();
            if (selectedPoint != null)
            {
                var point = selectedPoint.Value + left.position;
                point = new(Mathf.Floor(point.x), Mathf.Floor(point.y));
                var tmpColor = GUI.color;
                GUI.color = Color.red;
                GUI.DrawTexture(new Rect(point.x - 2f, point.y, 5f, 1f), BaseContent.WhiteTex);
                GUI.DrawTexture(new Rect(point.x, point.y - 2f, 1f, 5f), BaseContent.WhiteTex);
                GUI.color = tmpColor;
            }
        }

        //壁の説明
        right = right.RightPartPixels(right.width - 10f);
        Text.Font = GameFont.Medium;
        Widgets.Label(right.TopPartPixels(Text.LineHeight), nanameWall.LabelCap);
        Text.Font = GameFont.Small;
        right = right.BottomPartPixels(right.height - Text.LineHeight);
        Widgets.Label(right.TopPartPixels(Text.LineHeight), $"defName: {nanameWall.defName}");
        right = right.BottomPartPixels(right.height - Text.LineHeight);
        var buttonOffset = selThing != null ? Text.LineHeight : 0f;
        Widgets.Label(right.TopPartPixels(right.height - Text.LineHeight - buttonOffset), nanameWall.description);
        var enabled = meshSettings.enabled;
        right = right.BottomPartPixels(Text.LineHeight + buttonOffset);
        Widgets.CheckboxLabeled(right.TopPartPixels(Text.LineHeight), "NAW.Settings.EnableNaname".Translate(), ref enabled);
        if (selThing != null)
        {
            if (Widgets.ButtonText(right.BottomPartPixels(buttonOffset), "NAW.UpdateGraphic".Translate()))
            {
                selThing.DirtyMapMesh(selThing.Map);
            }
        }
        if (meshSettings.enabled != enabled)
        {
            nanameWall.designationCategory = enabled ? selDef.designationCategory : null;
            designationCategoryChanged = true;
            meshSettings.enabled = enabled;
        }

        //デフォルトボタン
        var item = bottom.TopPartPixels(Text.LineHeight);
        item.y += 5f;
        defaultRequest = "";
        var lastRowRect = bottom.BottomPartPixels(Text.LineHeight);
        var leftButtonRect = item.LeftPartPixels((item.width / 2f) - 2f);
        var rightButtonRect = item.RightPartPixels((item.width / 2f) - 2f);
        if (Widgets.ButtonText(leftButtonRect, "Reset".Translate()))
        {
            defaultRequest = GUI.GetNameOfFocusedControl();
            Clear();
        }
        if (Widgets.ButtonText(rightButtonRect, "ResetAll".Translate()))
        {
            meshSettings = NanameWalls.Mod.Settings.meshSettings[selDef.defName] = MeshSettings.DeepCopyDefaultFor(selDef);
            Clear();
        }
        item.y += Text.LineHeight + 5f;

        //数値の設定
        var outRect = item;
        outRect.height = rect.yMax - item.y;
        var viewRect = outRect;
        item.xMax -= GenUI.ScrollBarWidth;
        viewRect.xMax = item.xMax;
        viewRect.height = viewRectYMax - item.y;
        //Widgets.AdjustRectsForScrollView(outRect, ref outRect, ref viewRect);
        Widgets.BeginScrollView(outRect, ref scrollPosition2, viewRect);
        var labelPct = 0.2f;
        var rect2 = item.RightPart(1f - labelPct).AtZero();
        rect2.SplitVerticallyWithMargin(out var left2, out var right2, 5f);
        left2.SplitVerticallyWithMargin(out var rect3, out var rect4, 5f);
        right2.SplitVerticallyWithMargin(out var rect5, out var rect6, 5f);
        List<Rect> rects = [rect3, rect4, rect5, rect6];
        Text.Font = GameFont.Tiny;
        var defaultSettings = MeshSettings.DefaultSettingsFor(selDef);
        if (SettingItem(ref item, labelPct, "RepeatNorth", ref meshSettings.repeatNorth, defaultSettings.repeatSouth, rects[0]))
        {
            repeat = meshSettings.repeatNorth;
            texCoords = [.. meshSettings.northUVs.Select(uv => ((Vector2)uv * 0.1875f) + new Vector2(0.03125f, 0.03125f))];
            selectedList = [.. meshSettings.northVerts.Select(v => Invert((v.ToVector2() + new Vector2(0.5f, 0.5f)) * previewSize / 2f, previewSize))];
            selectedPoint = null;
        }
        SettingItem(ref item, previewSize, labelPct, "NorthUVs", meshSettings.northUVs, defaultSettings.northUVs, rects);
        SettingItem(ref item, previewSize, labelPct, "NorthVerts", meshSettings.northVerts, defaultSettings.northVerts, rects, meshSettings.northUVs, meshSettings.repeatNorth);
        SettingItem(ref item, previewSize, labelPct, "NorthVertsFiller", meshSettings.northVertsFiller, defaultSettings.northVertsFiller, rects, meshSettings.topFillerUVs);
        SettingItem(ref item, previewSize, labelPct, "NorthUVsFinish", meshSettings.northUVsFinish, defaultSettings.northUVsFinish, rects);
        SettingItem(ref item, previewSize, labelPct, "NorthVertsFinish", meshSettings.northVertsFinish, defaultSettings.northVertsFinish, rects, meshSettings.northUVsFinish);
        SettingItem(ref item, previewSize, labelPct, "BorderFillerUVs", meshSettings.borderFillerUVs, defaultSettings.borderFillerUVs, rects);
        SettingItem(ref item, previewSize, labelPct, "NorthVertsFinishBorder", meshSettings.northVertsFinishBorder, defaultSettings.northVertsFinishBorder, rects, meshSettings.borderFillerUVs);
        if (SettingItem(ref item, labelPct, "RepeatSouth", ref meshSettings.repeatSouth, defaultSettings.repeatSouth, rects[0]))
        {
            repeat = meshSettings.repeatSouth;
            texCoords = [.. meshSettings.southUVs.Select(uv => ((Vector2)uv * 0.1875f) + new Vector2(0.03125f, 0.03125f))];
            selectedList = [.. meshSettings.southVerts.Select(v => Invert((v.ToVector2() + new Vector2(0.5f, 0.5f)) * previewSize / 2f, previewSize))];
            selectedPoint = null;
        }
        SettingItem(ref item, previewSize, labelPct, "SouthUVs", meshSettings.southUVs, defaultSettings.southUVs, rects);
        SettingItem(ref item, previewSize, labelPct, "SouthVerts", meshSettings.southVerts, defaultSettings.southVerts, rects, meshSettings.southUVs, meshSettings.repeatSouth);
        SettingItem(ref item, previewSize, labelPct, "SouthVertsFiller", meshSettings.southVertsFiller, defaultSettings.southVertsFiller, rects, meshSettings.topFillerUVs);
        SettingItem(ref item, previewSize, labelPct, "SouthUVsFinish", meshSettings.southUVsFinish, defaultSettings.southUVsFinish, rects);
        SettingItem(ref item, previewSize, labelPct, "SouthVertsFinish", meshSettings.southVertsFinish, defaultSettings.southVertsFinish, rects, meshSettings.southUVsFinish);
        SettingItem(ref item, previewSize, labelPct, "TopFillerUVs", meshSettings.topFillerUVs, defaultSettings.topFillerUVs, rects);
        Widgets.EndScrollView();

        viewRectYMax = item.y;
        Text.Font = GameFont.Small;
    }

    private bool SettingItem(ref Rect rect, float labelPct, string label, ref int value, int defaultValue, Rect textFieldRect)
    {
        Widgets.Label(rect.LeftPart(labelPct), label);
        var rect2 = rect.RightPart(1f - labelPct);
        if (!buffers.TryGetValue(label, out var buffer))
        {
            buffer = buffers[label] = new string[1];
        }
        var rect3 = textFieldRect;
        rect3.position += rect2.position;
        var prevValue = value;
        Widgets.TextFieldNumeric(rect3, ref value, ref buffer[0], 1, 9);

        using (new TextBlock(GameFont.Small))
        {
            if (Widgets.ButtonText(new Rect(rect3.xMax + 2f, rect.y, rect3.height, rect3.height).ContractedBy(1f), "+"))
            {
                value = Math.Min(value + 1, 9);
                buffer[0] = value.ToStringCached();
            }
            if (Widgets.ButtonText(new Rect(rect3.xMax + rect3.height + 4f, rect.y, rect3.height, rect3.height).ContractedBy(1f), "-"))
            {
                value = Math.Max(value - 1, 1);
                buffer[0] = value.ToStringCached();
            }
        }
        if (prevValue != value)
        {
            repeat = value;
        }

        var flag = false;
        var controlName = "TextField" + rect3.y.ToString("F0") + rect3.x.ToString("F0");
        var name = GUI.GetNameOfFocusedControl();
        if (name == controlName)
        {
            Widgets.DrawHighlightSelected(rect);
            if (prevFocusedControl != name)
            {
                prevFocusedControl = name;
                flag = true;
            }
        }
        if (defaultRequest == controlName)
        {
            value = defaultValue;
            buffer[0] = value.ToStringCached();
        }
        rect.y += Text.LineHeightOf(GameFont.Small);
        return flag;
    }

    private void SettingItem(ref Rect rect, float previewSize, float labelPct, string label, List<Vector3> values, List<Vector3> defaultValue, List<Rect> textFieldRects, List<Vector3> uvs = null, int repeat = 1)
    {
        Vector3 ConvertVert(Vector3 v) => Invert((v.ToVector2() + new Vector2(0.5f, 0.5f)) * previewSize / 2f, previewSize);
        Vector3 ConvertUV(Vector3 v) => Invert(v * previewSize / 2f, previewSize);

        Widgets.Label(rect.LeftPart(labelPct), label);
        var rect2 = rect.RightPart(1f - labelPct);
        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            var rect3 = textFieldRects[i];
            rect3.position += rect2.position;
            if (!buffers.TryGetValue(label + i, out var buffer))
            {
                buffer = buffers[label + i] = new string[3];
            }
            var prevValue = value;
            var isVerts = uvs != null;
            Widgets.TextFieldVector(rect3, ref value, ref buffer, isVerts ? -0.5f : 0f, isVerts ? 1.5f : 1f);
            values[i] = value;
            if (prevValue != value)
            {
                if (isVerts)
                {
                    selectedPoint = ConvertVert(value);
                    selectedList = [.. values.Select(ConvertVert)];
                    texCoords = [.. uvs.Select(uv => ((Vector2)uv * 0.1875f) + new Vector2(0.03125f, 0.03125f))];
                }
                else
                {
                    selectedPoint = ConvertUV(value);
                    selectedList = [.. values.Select(ConvertUV)];
                }
            }

            if (i != values.Count - 1)
            {
                Widgets.Label(new(rect3.xMax - 2f, rect3.y, 5f, rect3.height), ",");
            }

            var offset = rect3.width / 3f;
            var controlName1 = "TextField" + rect3.y.ToString("F0") + rect3.x.ToString("F0");
            var controlName2 = "TextField" + rect3.y.ToString("F0") + (rect3.x + offset).ToString("F0");
            var controlName3 = "TextField" + rect3.y.ToString("F0") + (rect3.x + (offset * 2)).ToString("F0");
            var name = GUI.GetNameOfFocusedControl();
            if (name == controlName1 || name == controlName2 || name == controlName3)
            {
                Widgets.DrawHighlightSelected(rect);
                if (prevFocusedControl != name)
                {
                    prevFocusedControl = name;
                    if (isVerts)
                    {
                        this.repeat = repeat;
                        selectedPoint = ConvertVert(value);
                        selectedList = [.. values.Select(ConvertVert)];
                        texCoords = [.. uvs.Select(uv => ((Vector2)uv * 0.1875f) + new Vector2(0.03125f, 0.03125f))];
                    }
                    else
                    {
                        selectedPoint = ConvertUV(value);
                        selectedList = [.. values.Select(ConvertUV)];
                        texCoords = null;
                    }
                }
            }
            if (defaultRequest == controlName1 || defaultRequest == controlName2 || defaultRequest == controlName3)
            {
                values[i] = defaultValue[i];
                buffers[label + i] = new string[3];
            }
        }

        rect.y += Text.LineHeightOf(GameFont.Small);
    }

    private Vector3 Invert(Vector3 vector, float previewSize)
    {
        vector.y = previewSize - vector.y;
        return vector;
    }
}
