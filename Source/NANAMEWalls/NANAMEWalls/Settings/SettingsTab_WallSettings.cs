using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static NanameWalls.MeshSettings;

namespace NanameWalls;

[HotSwap]
internal class SettingsTab_WallSettings : SettingsTabDrawer
{
    private const float PreviewSizeRatio = 0.315f;

    private Vector2 scrollPosition;

    private Vector2 scrollPosition2;

    private Vector2 scrollPosition3;

    private readonly Dictionary<string, string[]> buffers = [];

    private List<Vector2> texCoords;

    private int repeat = 1;

    private List<Vector3> selectedList;

    private Vector2? selectedPoint;

    private string prevFocusedControl = "";

    private bool designationCategoryChanged;

    private string defaultRequest;

    private float viewRectYMax;

    private List<TabRecord> tabs;

    private string settingMode = "Values";

    public override int Index => 0;

    public override string Label => "NAW.Settings.Walls".Translate();

    public override bool DrawDefaultButton => false;

    private void InitializeTabs()
    {
        tabs = [];
        tabs.Add(new TabRecord("NAW.Settings.Values".Translate(), () => settingMode = "Values", () => settingMode == "Values"));
        tabs.Add(new TabRecord("NAW.Settings.Items".Translate(), () => settingMode = "Items", () => settingMode == "Items"));
        tabs.Add(new TabRecord("NAW.Settings.Options".Translate(), () => settingMode = "Options", () => settingMode == "Options"));
    }

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
                    NanameWalls.Mod.selThing = null;
                    Clear();
                }
                curY += Text.LineHeight;
            }
        }
        Widgets.EndScrollView();
    }

    private void DoMeshSettings(Rect rect)
    {
        var selDef = NanameWalls.Mod.selDef;
        var selThing = NanameWalls.Mod.selThing;
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
            meshSettings = NanameWalls.Mod.Settings.meshSettings[selDef.defName] = DeepCopyDefaultFor(selDef);
        }

        rect.SplitHorizontally(previewSize, out var top, out var bottom);
        top.SplitVertically(previewSize, out var left, out var right);

        //壁のプレビュー
        Widgets.DrawBoxSolid(left, new Color(0.1f, 0.1f, 0.1f));
        var mat = selThing != null ? selThing.Graphic.MatSingle : selDef.graphicData?.Graphic?.MatSingle;
        if (mat != null)
        {
            var scale = mat.mainTextureScale;
            var offset = mat.mainTextureOffset;
            Widgets.DrawTextureFitted(left.LeftHalf().BottomHalf(), mat.mainTexture, 1f, new(previewSize / 2f, previewSize / 2f), new(0.03125f, 0.03125f, 0.1875f, 0.1875f), 0f, mat);
            Widgets.DrawTextureFitted(left.RightHalf().TopHalf(), mat.mainTexture, 1f, new(previewSize / 2f, previewSize / 2f), new(0.03125f, 0.03125f, 0.1875f, 0.1875f), 0f, mat);
            mat.mainTextureScale = scale;
            mat.mainTextureOffset = offset;
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
        using (new TextBlock(GameFont.Medium))
        {
            Widgets.Label(right.TopPartPixels(Text.LineHeight), nanameWall.LabelCap);
            right = right.BottomPartPixels(right.height - Text.LineHeight + 4f);
        }

        var buttonOffset = selThing != null ? Text.LineHeight : 0f;
        var settingsCount = 1;
        var settingItemHeight = 18f;
        var settingsHeight = settingItemHeight * settingsCount;
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft))
        {
            Widgets.Label(right.TopPartPixels(Text.LineHeight), $"defName: {nanameWall.defName}");
            right = right.BottomPartPixels(right.height - Text.LineHeight + 4f);

            var outRect = right.TopPartPixels(right.height - settingsHeight - buttonOffset);
            var viewRect = outRect;
            viewRect.height = Text.CalcHeight(nanameWall.description, viewRect.width);
            if (viewRect.height >= outRect.height)
            {
                viewRect.width -= 20f;
                viewRect.height = Text.CalcHeight(nanameWall.description, viewRect.width);
            }
            Widgets.BeginScrollView(outRect, ref scrollPosition3, viewRect);
            Widgets.Label(viewRect, nanameWall.description);
            Widgets.EndScrollView();
        }

        //基本設定
        right = right.BottomPartPixels(settingsHeight + buttonOffset);
        var enabled = meshSettings.enabled;
        var curY = right.y;
        Rect CurRect()
        {
            Rect rect = new(right.x, curY, right.width, Text.LineHeight);
            curY += Text.LineHeight;
            return rect;
        }
        WidgetsEx.CheckboxLabeled(CurRect(), "NAW.Settings.EnableNaname".Translate(), ref enabled, settingItemHeight);
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

        //タブドロワー
        var tabRect = bottom.BottomPartPixels(bottom.height - Text.LineHeight - 5f);
        if (tabs is null)
        {
            InitializeTabs();
        }
        Widgets.DrawMenuSection(tabRect);
        WidgetsEx.DrawTabs(tabRect, tabs, Text.LineHeight, tabRect.width / 2f / tabs.Count);

        //デフォルトボタン
        var buttonRect = bottom.TopPartPixels(Text.LineHeight).RightPartPixels((bottom.width / 2f) - 2f);
        buttonRect.y = tabRect.y - Text.LineHeight;
        var rightButtonRect = buttonRect.RightPartPixels((buttonRect.width / 2f) - 1f);
        if (Widgets.ButtonText(rightButtonRect, "ResetAll".Translate()))
        {
            Find.WindowStack.Add(new Dialog_Confirm("NAW.Settings.DefaultAll".Translate(), () =>
            {
                meshSettings = NanameWalls.Mod.Settings.meshSettings[selDef.defName] = DeepCopyDefaultFor(selDef);
                Clear();
            }));
        }

        tabRect = tabRect.ContractedBy(2f);
        switch (settingMode)
        {
            case "Items":
                DoItemsSection(tabRect, meshSettings);
                break;
            case "Options":
                DoOptionsSection(tabRect, meshSettings);
                break;
            case "Values":
            default:
                defaultRequest = "";
                var leftButtonRect = buttonRect.LeftPartPixels((buttonRect.width / 2f) - 1f);
                if (Widgets.ButtonText(leftButtonRect, "Reset".Translate()))
                {
                    defaultRequest = GUI.GetNameOfFocusedControl();
                    Clear();
                }
                DoValuesSection(tabRect, previewSize, meshSettings);
                break;
        }
    }

    private void DoValuesSection(Rect rect, float previewSize, MeshSettings meshSettings)
    {
        var viewRect = rect;
        viewRect.height = viewRectYMax - rect.y;
        Widgets.AdjustRectsForScrollView(rect, ref rect, ref viewRect);
        var itemRect = viewRect.TopPartPixels(Text.LineHeight);
        Widgets.BeginScrollView(rect, ref scrollPosition2, viewRect);
        var labelPct = 0.2f;
        var rect2 = itemRect.RightPart(1f - labelPct).AtZero();
        rect2.SplitVerticallyWithMargin(out var left2, out var right2, 5f);
        left2.SplitVerticallyWithMargin(out var rect3, out var rect4, 5f);
        right2.SplitVerticallyWithMargin(out var rect5, out var rect6, 5f);
        List<Rect> rects = [rect3, rect4, rect5, rect6];
        Text.Font = GameFont.Tiny;

        var defaultSettings = DefaultSettingsFor(NanameWalls.Mod.selDef);
        SettingItem newUVs = null;
        foreach (var item in meshSettings.settingItems.Values)
        {
            if (!defaultSettings.settingItems.TryGetValue(item.label, out var defaultItem))
            {
                defaultItem = item.DeepCopy();
            }
            switch (item.type)
            {
                case SettingItem.SettingType.Repeat:
                    if (DoItem(ref itemRect, labelPct, item.label, ref item.repeat, defaultItem.repeat, rects[0]))
                    {
                        repeat = item.repeat;
                        selectedPoint = null;
                        if (!meshSettings.settingItems.TryGetValue(item.link, out var linkVerts)) continue;
                        selectedList = [.. linkVerts.vectors.Select(v => Invert((v.ToVector2() + new Vector2(0.5f, 0.5f)) * previewSize / 2f, previewSize))];

                        if (!meshSettings.settingItems.TryGetValue(linkVerts.link, out var linkUVs)) continue;
                        texCoords = [.. linkUVs.vectors];
                    }
                    break;

                case SettingItem.SettingType.UVs:
                case SettingItem.SettingType.Verts:
                    DoItem(ref itemRect, previewSize, labelPct, item.label, item, defaultItem, rects, meshSettings);
                    break;
            }
        }
        if (newUVs is not null)
        {
            meshSettings.settingItems[newUVs.label] = newUVs;
        }
        Widgets.EndScrollView();

        viewRectYMax = itemRect.y;
        Text.Font = GameFont.Small;
    }

    private bool DoItem(ref Rect rect, float labelPct, string label, ref int value, int defaultValue, Rect textFieldRect)
    {
        Widgets.LabelEllipses(rect.LeftPart(labelPct), label);
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

    private void DoItem(ref Rect rect, float previewSize, float labelPct, string label, SettingItem item, SettingItem defaultItem, List<Rect> textFieldRects, MeshSettings meshSettings)
    {
        Vector3 ConvertVert(Vector3 v) => Invert((v.ToVector2() + new Vector2(0.5f, 0.5f)) * previewSize / 2f, previewSize);
        Vector3 ConvertUV(Vector3 v) => Invert(v * previewSize / 2f, previewSize);

        var isVerts = item.type == SettingItem.SettingType.Verts;
        Widgets.DrawRectFast(rect, isVerts ? new Color(0f, 0.1f, 0.85f, 0.05f) : new Color(0.85f, 0.8f, 0f, 0.05f));
        Widgets.LabelEllipses(rect.LeftPart(labelPct), label);
        var rect2 = rect.RightPart(1f - labelPct);
        for (var i = 0; i < item.vectors.Count; i++)
        {
            var value = item.vectors[i];
            var rect3 = textFieldRects[i];
            rect3.position += rect2.position;
            if (!buffers.TryGetValue(label + i, out var buffer))
            {
                buffer = buffers[label + i] = new string[3];
            }
            var prevValue = value;
            Widgets.TextFieldVector(rect3, ref value, ref buffer, isVerts ? -0.75f : 0f, isVerts ? 1.75f : 1f);
            item.vectors[i] = value;
            if (prevValue != value)
            {
                if (isVerts)
                {
                    selectedPoint = ConvertVert(value);
                    selectedList = [.. item.vectors.Select(ConvertVert)];
                    if (meshSettings.settingItems.TryGetValue(item.link, out var uvs))
                    {
                        texCoords = [.. uvs.vectors];
                    }
                }
                else
                {
                    selectedPoint = ConvertUV(value);
                    selectedList = [.. item.vectors.Select(ConvertUV)];
                }
            }

            if (i != item.vectors.Count - 1)
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
                        repeat = meshSettings.settingItems.Values.FirstOrDefault(item2 => item2.link == item.label)?.repeat ?? 1;
                        selectedPoint = ConvertVert(value);
                        selectedList = [.. item.vectors.Select(ConvertVert)];
                        if (meshSettings.settingItems.TryGetValue(item.link, out var uvs))
                        {
                            texCoords = [.. uvs.vectors];
                        }
                    }
                    else
                    {
                        selectedPoint = ConvertUV(value);
                        selectedList = [.. item.vectors.Select(ConvertUV)];
                        texCoords = null;
                    }
                }
            }
            if (defaultRequest == controlName1 || defaultRequest == controlName2 || defaultRequest == controlName3)
            {
                item.vectors[i] = defaultItem.vectors[i];
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

    private void DoItemsSection(Rect rect, MeshSettings meshSettings)
    {
        var labelPct = 0.3f;

        rect.SplitHorizontally(Text.LineHeight, out var top, out var bottom);
        var viewRect = bottom;
        viewRect.height = viewRectYMax - bottom.y;
        Widgets.AdjustRectsForScrollView(bottom, ref bottom, ref viewRect);

        using (new TextBlock(TextAnchor.MiddleCenter))
        {
            top.width = viewRect.width;
            top.SplitVertically(top.width * labelPct, out var left, out var right);
            Widgets.DrawBox(left);
            Widgets.Label(left, "NAW.Settings.Label".Translate());
            right.SplitVertically(right.width / 3f, out var rect2, out var rect3);
            Widgets.DrawBox(rect2);
            Widgets.Label(rect2, "NAW.Settings.Type".Translate());
            rect3.SplitVertically(rect3.width / 2f, out var rect4, out var rect5);
            Widgets.DrawBox(rect4);
            Widgets.Label(rect4, "NAW.Settings.Related".Translate());
            Widgets.DrawBox(rect5);
            Widgets.Label(rect5, "NAW.Settings.Direction".Translate());
        }

        var itemRect = viewRect.TopPartPixels(Text.LineHeight);
        Widgets.BeginScrollView(bottom, ref scrollPosition2, viewRect);
        Text.Font = GameFont.Tiny;
        foreach (var item in meshSettings.settingItems.Values)
        {
            DoItem(ref itemRect, labelPct, item, meshSettings);
        }
        if (Widgets.ButtonImageWithBG(itemRect.LeftPart(labelPct), TexUI.CopyTex, new(itemRect.height - 4f, itemRect.height - 4f)))
        {
            var item = new SettingItem();
            Find.WindowStack.Add(new Dialog_RenameItem(item, meshSettings, () =>
            {
                meshSettings.settingItems[item.label] = item;
            }));
        }
        itemRect.y += Text.LineHeightOf(GameFont.Small);
        Widgets.EndScrollView();

        viewRectYMax = itemRect.y;
        Text.Font = GameFont.Small;
    }

    private void DoItem(ref Rect rect, float labelPct, SettingItem item, MeshSettings meshSettings)
    {
        var labelRect = rect.LeftPart(labelPct);
        var lineHeight = Text.LineHeightOf(GameFont.Small);
        labelRect.SplitVertically(labelRect.width - (lineHeight * 2f), out var left, out var right);
        Widgets.LabelEllipses(left, item.label);

        var buttonRect = right.LeftHalf().ContractedBy(1f);
        TooltipHandler.TipRegionByKey(buttonRect, "Rename");
        if (Widgets.ButtonImageFitted(buttonRect, TexUI.RenameTex))
        {
            var prevName = item.label;
            Find.WindowStack.Add(new Dialog_RenameItem(item, meshSettings, () =>
            {
                meshSettings.settingItems.Remove(prevName);
                meshSettings.settingItems[item.label] = item;
                meshSettings.settingItems.Values.Where(i => i.link == prevName).Do(i => i.link = item.label);
            }));
        }
        var buttonRect2 = right.RightHalf().ContractedBy(1f);
        TooltipHandler.TipRegionByKey(buttonRect2, "Delete");
        if (Widgets.ButtonImageFitted(buttonRect2, TexUI.DismissTex))
        {
            Delay.AfterNSeconds(0, () =>
            {
                meshSettings.settingItems.Remove(item.label);
                meshSettings.settingItems.Values.Where(i => i.link == item.label).Do(i => i.link = "");
            });
        }

        var rect2 = rect.RightPartPixels(rect.xMax - right.xMax - 2f);
        Widgets.Dropdown(rect2.LeftPart(0.33f), item, item => item.type, item =>
        {
            return ((IEnumerable<SettingItem.SettingType>)Enum.GetValues(typeof(SettingItem.SettingType))).Select(type =>
            {
                return new Widgets.DropdownMenuElement<SettingItem.SettingType>()
                {
                    option = new FloatMenuOption(type.ToString(), () =>
                    {
                        item.type = type;
                    }),
                    payload = type
                };
            });
        }, item.type.ToString());
        if (item.type == SettingItem.SettingType.Repeat)
        {
            if (meshSettings.settingItems.Values.Any(item2 => item2.type == SettingItem.SettingType.Verts))
            {
                Widgets.Dropdown(rect2.MiddlePart(0.33f, 1f), item, item => item.link, item =>
                {
                    return meshSettings.settingItems.Values.Where(item2 => item2.type == SettingItem.SettingType.Verts).Select(item2 =>
                    {
                        return new Widgets.DropdownMenuElement<string>()
                        {
                            option = new FloatMenuOption(item2.label, () =>
                            {
                                item.link = item2.label;
                            }),
                            payload = item2.label
                        };
                    });
                }, item.link);
            }
        }
        if (item.type == SettingItem.SettingType.Verts)
        {
            if (meshSettings.settingItems.Values.Any(item2 => item2.type == SettingItem.SettingType.UVs))
            {
                Widgets.Dropdown(rect2.MiddlePart(0.33f, 1f), item, item => item.link, item =>
                {
                    return meshSettings.settingItems.Values.Where(item2 => item2.type == SettingItem.SettingType.UVs).Select(item2 =>
                    {
                        return new Widgets.DropdownMenuElement<string>()
                        {
                            option = new FloatMenuOption(item2.label, () =>
                            {
                                item.link = item2.label;
                            }),
                            payload = item2.label
                        };
                    });
                }, item.link);
            }
            Widgets.Dropdown(rect2.RightPart(0.33f), item, item => item.condition, item =>
            {
                return ((IEnumerable<SettingItem.Condition>)Enum.GetValues(typeof(SettingItem.Condition))).Where(d => d != SettingItem.Condition.None).Select(direction =>
                {
                    return new Widgets.DropdownMenuElement<SettingItem.Condition>()
                    {
                        option = new FloatMenuOption(direction.ToString(), () =>
                        {
                            item.condition = direction;
                        }),
                        payload = direction
                    };
                });
            }, item.condition.ToString());
        }

        rect.y += lineHeight;
    }

    private void DoOptionsSection(Rect rect, MeshSettings meshSettings)
    {
        var listing_standard = new Listing_Standard();
        listing_standard.Begin(rect);
        listing_standard.CheckboxLabeled("NAW.Settings.NoChangeLinkState".Translate(), ref meshSettings.noChangeLinkState);
        listing_standard.CheckboxLabeled("NAW.Settings.SkipOriginalPrint".Translate(), ref meshSettings.skipOriginalPrint);
        listing_standard.CheckboxLabeled("NAW.Settings.AllowVShaped".Translate(), ref meshSettings.allowVShaped);
        listing_standard.End();
    }

    public class Dialog_RenameItem(SettingItem item, MeshSettings meshSettings, Action action) : Dialog_Rename<SettingItem>(item)
    {
        private readonly MeshSettings meshSettings = meshSettings;

        private readonly Action action = action;

        protected override AcceptanceReport NameIsValid(string name)
        {
            AcceptanceReport acceptanceReport = base.NameIsValid(name);
            if (!acceptanceReport.Accepted)
            {
                return acceptanceReport;
            }
            if (name != renaming.label && meshSettings.settingItems.TryGetValue(name, out _))
            {
                return "NameIsInUse".Translate();
            }
            return true;
        }

        protected override void OnRenamed(string name) => action();
    }
}
