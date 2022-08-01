using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Size = System.Drawing.Size;
namespace BareE
{
    public static class StringHelper
    {
        private static bool TryParsePosStr(String posStr, int contextSize, out int pos)
        {
            pos = 0;
            if (posStr == null) return false;
            if (posStr.EndsWith("%"))
            {
                posStr = posStr.Substring(0, posStr.Length - 1);
                float p = 0;
                if (!float.TryParse(posStr,out p))
                    return false;
                pos = (int)(contextSize*(p/100.0f));
                return true;
            }
            if (posStr.Equals("px"))
                posStr = posStr.Substring(0, posStr.Length - 2);
            if (!int.TryParse(posStr, out pos))
                return false;
            return true;
        }
        public static bool TryParseSize(String szStr, Size contextSize, out Size size)
        {
            size = new System.Drawing.Size(0, 0);
            if (szStr == null) return false;
            var parts = szStr.Split(",");
            if (parts.Count() != 2) return false;
            parts[0]=parts[0].Trim();
            parts[1]=parts[1].Trim();

            int w;
            int h;
            if (!TryParsePosStr(parts[0], contextSize.Width, out w))
                return false;
            if (!TryParsePosStr(parts[1], contextSize.Height, out h))
                return false;
            size = new Size(w, h);
            return true;
        }
        public static bool TryParsePosition(String posStr,Size WidgetSize, Size contextSize, out System.Drawing.Point pos)
        {
            pos = new System.Drawing.Point(0, 0);
            if (posStr == null) return false;

            switch (posStr.Trim().ToLower())
            {
                case "bottomleft": pos = new System.Drawing.Point(0, 0);break;
                case "bottom":
                case "bottomcenter": pos = new System.Drawing.Point((int)((contextSize.Width - WidgetSize.Width) / 2.0f), 0); break;
                case "bottomright": pos = new System.Drawing.Point((int)((contextSize.Width - WidgetSize.Width)), 0); break;

                case "left":
                case "centerleft": pos = new System.Drawing.Point(0, (int)((contextSize.Height - WidgetSize.Height) / 2)); break;
                case "centered":
                case "centercenter": pos = new System.Drawing.Point((int)((contextSize.Width - WidgetSize.Width) / 2.0f), (int)((contextSize.Height - WidgetSize.Height) / 2)); break;
                case "right":
                case "centerright": pos = new System.Drawing.Point((int)((contextSize.Width - WidgetSize.Width)), (int)((contextSize.Height - WidgetSize.Height) / 2)); break;

                case "topleft": pos = new System.Drawing.Point(0, (int)(contextSize.Height - WidgetSize.Height)); break;
                case "top":
                case "topcenter": pos = new System.Drawing.Point((int)((contextSize.Width - WidgetSize.Width) / 2.0f), (int)(contextSize.Height - WidgetSize.Height)); break;
                case "topright": pos = new System.Drawing.Point((int)((contextSize.Width - WidgetSize.Width) ), (int)(contextSize.Height - WidgetSize.Height)); break;

                    default:
                    {
                        Size sz;
                        if (!TryParseSize(posStr, contextSize, out sz))
                            return false;
                        pos = new System.Drawing.Point(sz.Width, sz.Height);
                    }
                  
                    break;

            }
            return true;

        }
    }
}
