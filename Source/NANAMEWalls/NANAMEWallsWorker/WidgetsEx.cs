using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NanameWalls;

[HotSwapAll]
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

        if (texCoords is { Count: 4 })
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
        Widgets.DefIcon(rect2, def, null, 1f, null, true);
        Rect rect3 = new(rect2.xMax + textOffsetX, 0f, rect.width, rect.height);
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.WordWrap = false;
        Widgets.LabelEllipses(rect3, def.LabelCap);
        Text.Anchor = TextAnchor.UpperLeft;
        Text.WordWrap = true;
        Widgets.EndGroup();
    }

    public static void DrawTabs<TTabRecord>(Rect baseRect, List<TTabRecord> tabs, float tabHeight = 32f, float maxTabWidth = 200f) where TTabRecord : TabRecord
    {
        TTabRecord val = null;
        var val2 = tabs.Find(t => t.Selected);
        var num = baseRect.width + ((tabs.Count - 1) * 10f);
        var tabWidth = num / tabs.Count;
        if (tabWidth > maxTabWidth)
        {
            tabWidth = maxTabWidth;
        }
        var rect = new Rect(baseRect);
        rect.y -= tabHeight;
        rect.height = 9999f;
        Widgets.BeginGroup(rect);
        Text.Anchor = TextAnchor.MiddleCenter;
        Text.Font = GameFont.Small;
        var list = tabs.ListFullCopy();
        if (val2 != null)
        {
            list.Remove(val2);
            list.Add(val2);
        }
        TabRecord tabRecord = null;
        var list2 = list.ListFullCopy();
        list2.Reverse();
        for (var num2 = 0; num2 < list2.Count; num2++)
        {
            var val3 = list2[num2];
            var rect2 = Func(val3);
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
        foreach (var item in list)
        {
            var rect3 = Func(item);
            item.Draw(rect3);
        }
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.EndGroup();
        if (val != null && val != val2)
        {
            SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
            val.clickedAction?.Invoke();
        }
        return;
        Rect Func(TTabRecord tab) => new(tabs.IndexOf(tab) * (tabWidth - 10f), 1f, tabWidth, tabHeight);
    }

    public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, float height = 24f, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, bool placeCheckboxNearText = false, bool paintable = false)
    {
        var anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        if (placeCheckboxNearText)
        {
            rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + height + 10f);
        }
        var rect2 = rect;
        rect2.xMax -= height;
        Widgets.Label(rect2, label);
        if (!disabled)
        {
            Widgets.ToggleInvisibleDraggable(rect, ref checkOn, true, paintable);
        }
        Widgets.CheckboxDraw(rect.x + rect.width - height, rect.y + (rect.height - height) / 2f, checkOn, disabled, height, texChecked, texUnchecked);
        Text.Anchor = anchor;
    }
    
    public static void TextFieldVector(Rect rect, ref Vector3 vector, ref string[] buffer, float min = 0f, float max = 1E+09f)
    {
        buffer ??= new string[3];
        var num = rect.width / 3f - 4f;
        var rect2 = rect.LeftPartPixels(num);
        var rect3 = rect2;
        var rect4 = rect3;
        rect3.x = rect2.xMax + 4f;
        rect4.x = rect3.xMax + 4f;
        TextFieldNumeric(rect2, ref vector.x, ref buffer[0], min, max);
        TextFieldNumeric(rect3, ref vector.y, ref buffer[1], min, max);
        TextFieldNumeric(rect4, ref vector.z, ref buffer[2], min, max);
    }
    
    public static void TextFieldNumeric<T>(Rect rect, ref T val, ref string buffer, float min = 0f, float max = 1E+09f)
    {
        buffer ??= val.ToString();
        var controlName = "TextField" + rect.y.ToString("F0") + rect.x.ToString("F0");
        var pressed = GUI.GetNameOfFocusedControl() == controlName && Event.current.type == EventType.KeyDown &&
                      (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
        GUI.SetNextControlName(controlName);
        var text = Widgets.TextField(rect, buffer);
        if (text != buffer)
        {
            buffer = text;
            if (IsPartiallyOrFullyTypedNumber(text))
            {
                if (IsFullyTypedNumber(text))
                {
                    ResolveParseNow(text, ref val, ref buffer, min, max, false);
                }
            }
        }
        if (pressed)
        {
            ResolveParseNow(text, ref val, ref buffer, min, max, true);
        }
        return;
        
        static bool IsPartiallyOrFullyTypedNumber(string s)
        {
            return s == "" || ((s.Length <= 1 || s[^1] != '-') && s != "00" && s.Length <= 12 &&
                               ((typeof(T) == typeof(float) && s.Count(t => t == '.') <= 1 &&
                                 ContainsOnlyCharacters(s, "-.0123456789")) || IsFullyTypedNumber(s)));
        }
        
        static bool ContainsOnlyCharacters(string s, string allowedChars)
        {
            return s.All(allowedChars.Contains);
        }
        
        static bool IsFullyTypedNumber(string s)
        {
            if (s == "")
            {
                return false;
            }
            if (typeof(T) == typeof(float))
            {
                var array = s.Split('.');
                if (array.Length is > 2 or < 1)
                {
                    return false;
                }
                if (!ContainsOnlyCharacters(array[0], "-0123456789"))
                {
                    return false;
                }
                if (array.Length == 2 && (array[1].Length == 0 || !ContainsOnlyCharacters(array[1], "0123456789")))
                {
                    return false;
                }
            }
            return !(typeof(T) == typeof(int)) || ContainsOnlyCharacters(s, "-0123456789");
        }
    }

    private static void ResolveParseNow<T>(string text, ref T val, ref string buffer, float min, float max, bool force)
    {
        try
        {
            val = val switch
            {
                float => (T)(object)Mathf.Clamp(Evaluate<float>(text), min, max),
                int => (T)(object)Mathf.RoundToInt(Mathf.Clamp(Evaluate<int>(text), min, max)),
                _ => throw new NotSupportedException()
            };
            var str = ToStringTypedIn(val);
            if (force || buffer == str)
                buffer = str;
        }
        catch
        {   
            ResetValue(out val, out buffer, min, max);
        }
        return;
        
        static void ResetValue(out T val, out string buffer, float min, float max)
        {
            val = default;
            if (min > 0f)
            {
                val = (T)(object)Mathf.RoundToInt(min);
            }
            if (max < 0f)
            {
                val = (T)(object)Mathf.RoundToInt(max);
            }
            buffer = ToStringTypedIn(val);
        }
    }
    
    private static string ToStringTypedIn<T>(T val)
    {
        return typeof(T) == typeof(float) ? ((float)(object)val).ToString("0.##########") : val.ToString();
    }
    
    public static T Evaluate<T>(string expression)
    {
        var text = expression.Replace(" ", "");
        var pos = 0;
        return (T)(object)ParseExpression(ref text, ref pos);
        
    }

    // Expression: Term ((+|-) Term)*
    private static float ParseExpression(ref string text, ref int pos)
    {
        var result = ParseTerm(ref text, ref pos);
        while (pos < text.Length)
        {
            var op = text[pos];
            if (op != '+' && op != '-') break;
            pos++;

            if (op == '+') result += ParseTerm(ref text, ref pos);
            else result -= ParseTerm(ref text, ref pos);
        }
        return result;
    }

    // Term: Factor ((*|/) Factor)*
    private static float ParseTerm(ref string text, ref int pos)
    {
        var result = ParseFactor(ref text, ref pos);
        while (pos < text.Length)
        {
            var op = text[pos];
            if (op != '*' && op != '/') break;
            pos++;

            if (op == '*') result *= ParseFactor(ref text, ref pos);
            else result /= ParseFactor(ref text, ref pos);
        }
        return result;
    }

    // Factor: Number or ( Expression )
    private static float ParseFactor(ref string text, ref int pos)
    {
        if (text[pos] == '(')
        {
            pos++; // '(' を飛ばす
            var result = ParseExpression(ref text, ref pos);
            pos++; // ')' を飛ばす
            return result;
        }

        var start = pos;
        if (text[pos] == '-') pos++;
        while (pos < text.Length && (char.IsDigit(text[pos]) || text[pos] == '.'))
        {
            pos++;
        }
        var slice = text.Substring(start, pos - start);
        return float.TryParse(slice, out var res) ? res : 0f;
    }
}