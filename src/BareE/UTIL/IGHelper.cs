using System;
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
            IG.SameLine();
            var b = IG.GetCursorScreenPos();
            var lh = IG.GetTextLineHeight();
            //ImGui::GetWindowDrawList()->AddLine(lineStart, lineEnd, segs[i].colour);
            IG.GetWindowDrawList().AddLine(
                new System.Numerics.Vector2(a.X, a.Y + lh),
                new System.Numerics.Vector2(b.X, b.Y + lh),
                clr, 1);
            IG.PopStyleColor();
        }
    }
}