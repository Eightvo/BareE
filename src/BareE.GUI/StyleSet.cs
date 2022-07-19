using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.GUI
{
    public class StyleTree
    {
        Dictionary<String,Stack<StyleNode>> StylePriority = new Dictionary<String,Stack<StyleNode>>(StringComparer.CurrentCultureIgnoreCase);
        Stack<Stack<StyleNode>> opOrder = new Stack<Stack<StyleNode>>();
        public void PushStyles(params StyleNode[] styles)
        {
            foreach (var style in styles)
            {
                if (!StylePriority.ContainsKey(style.Element))
                    StylePriority.Add(style.Element, new Stack<StyleNode>());
                StylePriority[style.Element].Push(style);
                opOrder.Push(StylePriority[style.Element]);
           }
        }
        public void PopStyles(int stylesToPop=1)
        {
            int popped = 0;
            while(opOrder.Count > 0 && popped<stylesToPop)
            {
                (opOrder.Pop()).Pop();
            }
        }
        public String GetStyle(String Element)
        {
            if (StylePriority.ContainsKey(Element) && StylePriority[Element].Count>0)
                return StylePriority[Element].Peek().Style;
            return "Default";
        }
        public void ClearStyles()
        {
            while (opOrder.Count > 0)
                (opOrder.Pop()).Pop();
        }
        public String this[String Element] { get { return GetStyle(Element); } }
    }

    public struct StyleNode
    {
        public String Element { get; set; }
        public String Style { get; set; }
    }
}
