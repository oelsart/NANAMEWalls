using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace NanameWalls;

public class NanameWalls : Mod
{
    public static NanameWalls Mod { get; private set; }

    public Settings Settings { get; private set; }

    public readonly List<ThingDef> walls = [];

    private const float PreviewSize = 180f;

    private ThingDef selDef;

    private Vector2 scrollPosition;

    private readonly Dictionary<string, string[]> buffers = [];

    private List<Vector3> selectedList;

    private Vector2? selectedPoint;

    private string prevFocusedControl = "";

    public NanameWalls(ModContentPack content) : base(content)
    {
        Mod = this;
        Settings = GetSettings<Settings>();
        var harmony = new Harmony("OELS.NanameWalls");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    public override string SettingsCategory()
    {
        return "NANAME Walls";
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        Clear();
    }

    private void Clear()
    {
        buffers.Clear();
        selectedList = null;
        selectedPoint = null;
        prevFocusedControl = "";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Widgets.DrawMenuSection(inRect);
        inRect.SplitVertically(200f, out var left, out var right);
        DoDefNameList(left);
        Widgets.DrawLineVertical(left.xMax, inRect.y, inRect.height);

        if (selDef != null)
        {
            DoMeshSettings(right.ContractedBy(10f));
        }
    }

    private void DoDefNameList(Rect rect)
    {
        var outRect = rect;
        var defs = walls.GroupBy(def => def.modContentPack).ToList();
        var height = Text.LineHeight * (defs.Count + defs.SelectMany(group => group).Count());
        var viewRect = new Rect(rect.x, rect.y, rect.width, height);
        Widgets.AdjustRectsForScrollView(rect, ref outRect, ref viewRect);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        var curY = viewRect.y;
        foreach (var group in defs)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            var rect2 = new Rect(viewRect.x, curY, viewRect.width, Text.LineHeight);
            Widgets.DrawBoxSolidWithOutline(rect2, Widgets.InactiveColor, Color.white, 1);
            if (Text.CalcSize(group.Key.Name).x > rect2.width)
            {
                Text.Font = GameFont.Tiny;
            }
            Widgets.Label(rect2, group.Key.Name);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;
            curY += Text.LineHeight;

            foreach (var def in group)
            {
                var rect3 = new Rect(viewRect.x, curY, viewRect.width, Text.LineHeight);
                Widgets.DefLabelWithIcon(rect3, def, 5f);
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
        rect.SplitHorizontally(200f, out var top, out var bottom);
        top.SplitVertically(200f, out var left, out var right);

        //壁の説明
        right = right.RightPartPixels(right.width - 10f);
        Text.Font = GameFont.Medium;
        Widgets.Label(right.TopPartPixels(Text.LineHeight), selDef.LabelCap);
        Text.Font = GameFont.Small;
        right = right.BottomPartPixels(right.height - Text.LineHeight);
        Widgets.Label(right.TopPartPixels(Text.LineHeight), $"defName: {selDef.defName}");
        right = right.BottomPartPixels(right.height - Text.LineHeight);
        Widgets.Label(right.TopPartPixels(right.height - Text.LineHeight), selDef.description);
        if (!Settings.enabled.TryGetValue(selDef.defName, out var enabled))
        {
            enabled = Settings.enabled[selDef.defName] = true;
        }
        Widgets.CheckboxLabeled(right.BottomPartPixels(Text.LineHeight), "NAW.Settings.EnableNaname".Translate(), ref enabled);
        Settings.enabled[selDef.defName] = enabled;

        //壁のプレビュー
        Widgets.DrawBoxSolid(left, new Color(0.1f, 0.1f, 0.1f));
        left = left.ContractedBy((200f - PreviewSize) / 2f);
        var mat = selDef.graphic?.MatSingle;
        if (mat != null)
        {
            Widgets.DrawTextureFitted(left.LeftHalf().BottomHalf(), mat.mainTexture, 1f, new(PreviewSize / 2f, PreviewSize / 2f), new(0.03125f, 0.03125f, 0.1875f, 0.1875f), 0f, mat);
            Widgets.DrawTextureFitted(left.RightHalf().TopHalf(), mat.mainTexture, 1f, new(PreviewSize / 2f, PreviewSize / 2f), new(0.03125f, 0.03125f, 0.1875f, 0.1875f), 0f, mat);
        }
        if (selectedList != null)
        {
            Widgets.BeginGroup(left);
            WidgetsEx.DrawPolygonFilled(selectedList, new Color(1f, 0.94f, 0.5f, 0.18f));
            Widgets.EndGroup();
            if (selectedPoint != null)
            {
                var point = selectedPoint.Value + left.position;
                point = new(Mathf.Floor(point.x), Mathf.Floor(point.y));
                var color = GUI.color;
                GUI.color = Color.red;
                GUI.DrawTexture(new Rect(point.x - 2f, point.y, 5f, 1f), BaseContent.WhiteTex);
                GUI.DrawTexture(new Rect(point.x, point.y - 2f, 1f, 5f), BaseContent.WhiteTex);
                GUI.color = color;
            }
        }

        //デフォルトボタン
        if (Widgets.ButtonText(bottom.BottomPartPixels(Text.LineHeight).RightPartPixels(350f), "Default".Translate()))
        {
            Settings.meshSettings[selDef.defName] = new();
            Clear();
        }

        //数値の設定
        var item = bottom;
        item.y += 10f;
        item.height = Text.LineHeight;
        if (!Settings.meshSettings.TryGetValue(selDef.defName, out var meshSettings))
        {
            meshSettings = Settings.meshSettings[selDef.defName] = new MeshSettings();
        }
        var labelPct = 0.2f;
        var rect2 = item.RightPart(1f - labelPct).AtZero();
        rect2.SplitVerticallyWithMargin(out var left2, out var right2, 5f);
        left2.SplitVerticallyWithMargin(out var rect3, out var rect4, 5f);
        right2.SplitVerticallyWithMargin(out var rect5, out var rect6, 5f);
        List<Rect> rects = [rect3, rect4, rect5, rect6];
        Text.Font = GameFont.Tiny;
        if (SettingItem(ref item, labelPct, "RepeatNorth", ref meshSettings.repeatNorth, rects[0]))
        {
            selectedList = [.. meshSettings.northVerts.Select(v => Invert((v.ToVector2() + new Vector2(0.5f, 0.5f)) * PreviewSize / 2f))];
            selectedPoint = null;
        }
        SettingItem(ref item, labelPct, "NorthUVs", meshSettings.northUVs, rects, false);
        SettingItem(ref item, labelPct, "NorthVerts", meshSettings.northVerts, rects, true);
        SettingItem(ref item, labelPct, "NorthVertsFiller", meshSettings.northVertsFiller, rects, true);
        SettingItem(ref item, labelPct, "NorthUVsFinish", meshSettings.northUVsFinish, rects, false);
        SettingItem(ref item, labelPct, "NorthVertsFinish", meshSettings.northVertsFinish, rects, true);
        SettingItem(ref item, labelPct, "NorthVertsFinishBorder", meshSettings.northVertsFinishBorder, rects, true);
        if (SettingItem(ref item, labelPct, "RepeatSouth", ref meshSettings.repeatSouth, rects[0]))
        {
            selectedList = [.. meshSettings.southVerts.Select(v => Invert((v.ToVector2() + new Vector2(0.5f, 0.5f)) * PreviewSize / 2f))];
            selectedPoint = null;
        }
        SettingItem(ref item, labelPct, "SouthUVs", meshSettings.southUVs, rects, false);
        SettingItem(ref item, labelPct, "SouthVerts", meshSettings.southVerts, rects, true);
        SettingItem(ref item, labelPct, "SouthVertsFiller", meshSettings.southVertsFiller, rects, true);
        SettingItem(ref item, labelPct, "SouthUVsFinish", meshSettings.southUVsFinish, rects, false);
        SettingItem(ref item, labelPct, "SouthVertsFinish", meshSettings.southVertsFinish, rects, true);
        Text.Font = GameFont.Small;
    }

    private bool SettingItem(ref Rect rect, float labelPct, string label, ref int value, Rect textFieldRect)
    {
        Widgets.Label(rect.LeftPart(labelPct), label);
        var rect2 = rect.RightPart(1f - labelPct);
        if (!buffers.TryGetValue(label, out var buffer))
        {
            buffer = buffers[label] = new string[1];
        }
        var rect3 = textFieldRect;
        rect3.position += rect2.position;
        Widgets.TextFieldNumeric(rect3, ref value, ref buffer[0], 1, 10);

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
        rect.y += Text.LineHeightOf(GameFont.Small);
        return flag;
    }

    private void SettingItem(ref Rect rect, float labelPct, string label, List<Vector3> values, List<Rect> textFieldRects, bool verts)
    {
        Vector3 ConvertVert(Vector3 v) => Invert((v.ToVector2() + new Vector2(0.5f, 0.5f)) * PreviewSize / 2f);
        Vector3 ConvertUV(Vector3 v) => Invert(v * PreviewSize / 2f);

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
            Widgets.TextFieldVector(rect3, ref value, ref buffer, verts ? -0.5f : 0f, verts ? 1.5f : 1f);
            values[i] = value;
            if (prevValue != value)
            {
                selectedPoint = verts ? ConvertVert(value) : ConvertUV(value);
                selectedList = verts ? [.. values.Select(ConvertVert)] : [.. values.Select(ConvertUV)];
            }

            if (i != values.Count - 1)
            {
                Widgets.Label(new(rect3.xMax - 2f, rect3.y, 5f, rect3.height), ",");
            }

            var offset = rect3.width / 3f;
            var controlName1 = "TextField" + rect3.y.ToString("F0") + rect3.x.ToString("F0");
            var controlName2 = "TextField" + rect3.y.ToString("F0") + (rect3.x + offset).ToString("F0");
            var controlName3 = "TextField" + rect3.y.ToString("F0") + (rect3.x + offset * 2).ToString("F0");
            var name = GUI.GetNameOfFocusedControl();
            if (name == controlName1 || name == controlName2 || name == controlName3)
            {
                Widgets.DrawHighlightSelected(rect);
                if (prevFocusedControl != name)
                {
                    prevFocusedControl = name;
                    selectedPoint = verts ? ConvertVert(value) : ConvertUV(value);
                    selectedList = verts ? [.. values.Select(ConvertVert)] : [.. values.Select(ConvertUV)];
                }
            }
        }

        rect.y += Text.LineHeightOf(GameFont.Small);
    }

    private Vector3 Invert(Vector3 vector)
    {
        vector.y = PreviewSize - vector.y;
        return vector;
    }
}