using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.GUI
{
    public class StyleSet
    {
        Stack<StyleNode> StylePriority = new Stack<StyleNode>();
        public void PushStyles(params StyleNode[] styles)
        {
            foreach(var style in styles)
                StylePriority.Push(style);
        }
        public void PopStyles(int stylesToPop=1)
        {
            for(int i=0;i<stylesToPop;i++)
                if (StylePriority.Count>0)
                    StylePriority.Pop();
        }
        public String GetStyle(String Element)
        {
            foreach (StyleNode style in StylePriority)
                if (String.Compare(Element, style.Element, true) == 0)
                    return style.Style;
            return "Default";
        }
        public void ClearStyles()
        {
            while (StylePriority.Count > 0)
                StylePriority.Pop();
        }
    }

    public struct StyleNode
    {
        public String Element { get; set; }
        public String Style { get; set; }
    }
}
