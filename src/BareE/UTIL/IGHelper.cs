using System;
using System.Collections.Generic;
using System.Numerics;

using IG = ImGuiNET.ImGui;

namespace BareE.UTIL
{
    public static class IGHelper
    {
        public static void Link(String text, String url, Vector4 color)
        {
            uint clr = IG.ColorConvertFloat4ToU32(color);
            var a = IG.GetCursorScreenPos();
            IG.PushStyleColor(ImGuiNET.ImGuiCol.Text, clr);
            IG.Text(text);
            if (IG.IsItemClicked())
            {
                Util.OpenBrowser(url);
            }
            var b = IG.GetCursorScreenPos();
            var lh = IG.GetTextLineHeight();
            //ImGui::GetWindowDrawList()->AddLine(lineStart, lineEnd, segs[i].colour);
            IG.GetWindowDrawList().AddLine(
                new System.Numerics.Vector2(a.X, a.Y + lh),
                new System.Numerics.Vector2(b.X, b.Y + lh),
                clr, 1);
            IG.PopStyleColor();
        }
        public static bool Link(String text, Vector4 color)
        {
            var ret = false;
            uint clr = IG.ColorConvertFloat4ToU32(color);
            var a = IG.GetCursorScreenPos();
            IG.PushStyleColor(ImGuiNET.ImGuiCol.Text, clr);
            IG.Text(text);
            if (IG.IsItemClicked())
            {
                ret = true;
            }
            var b = IG.GetCursorScreenPos();
            var lh = IG.GetTextLineHeight();
            //ImGui::GetWindowDrawList()->AddLine(lineStart, lineEnd, segs[i].colour);
            //IG.GetWindowDrawList().AddLine(
           //     new System.Numerics.Vector2(a.X, a.Y + lh),
           //     new System.Numerics.Vector2(b.X, b.Y + lh),
           //     clr, 1);
            IG.PopStyleColor();
            return ret;
        }


        public static T ComboFromEnum<T>(String name, T preview)
     where T : struct, Enum
        {
            var ret = preview;
            if (IG.BeginCombo($"##{name}", preview.ToString()))
            {
                foreach (var v in Enum.GetNames<T>())
                    if (IG.Selectable(v))
                        ret = Enum.Parse<T>(v);

                IG.EndCombo();
            }
            return ret;
        }
        public static T ComboFromCollection<T>(String name, T preview, IEnumerable<T> collection)
        {
            var ret = preview;
            if (IG.BeginCombo($"##{name}", preview.ToString()))
            {
                foreach (var v in collection)
                    if (IG.Selectable(v.ToString()))
                        ret = v;
                IG.EndCombo();
            }
            return ret;
        }
    }
}