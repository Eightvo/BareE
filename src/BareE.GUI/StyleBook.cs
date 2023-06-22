using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BareE.GUI
{
    public enum StyleElement
    {
        TitleFrame=1,
        TitleFrameColor,
        TitleFrameMarginVertical,
        TitleFrameMarginHorizontal,

        InnerFrame,
        InnerFrameColor,
        InnerFrameMarginHorizontal,
        InnerFrameMarginVertical,

        Font,
        FontSize,
        FontColor,

        CloseButton_Normal,
        CloseButton_NormalColor,
        CloseButton_Hover,
        CloseButton_HoverColor,
        CloseButton_Down,
        CloseButton_DownColor,


        MouseCursor_Normal,
        MouseCursor_NormalColor,
        MouseCursor_Down,
        MouseCursor_DownColor,

        ResizeButton_Normal,
        ResizeButton_Hover,
        ResizeButton_HoverDown,

        Button_Normal,
        Button_Hover,
        Button_HoverDown,

        MinButton_Normal,
        MinButton_Hover,
        MinButton_HoverDown,

        MaxButton_Normal,
        MaxButton_Hover,
        MaxButton_HoverDown,
    }
    public class StyleBook
    {
        private Dictionary<StyleElement, Stack<object>> _styles;
        private Dictionary<String, Dictionary<StyleElement, object>> _styleDefs;
        public StyleBook()
        {
            _styles= new Dictionary<StyleElement, Stack<object>>();
            _styleDefs = new Dictionary<string, Dictionary<StyleElement, object>>();
        }
        public void DefineStyle(String name, Dictionary<StyleElement, object> def)
        {
            if (!_styleDefs.ContainsKey(name))
                _styleDefs.Add(name, def);
            else _styleDefs[name]= def; 
        }
        public void PushStlye(StyleElement element, object style)
        {
            if (!_styles.ContainsKey(element)) _styles.Add(element, new Stack<object>());
            _styles[element].Push(style);
        }
        public void PopStyle(StyleElement element)
        {
            if (!_styles.ContainsKey(element) || _styles[element].Count == 0) return;
            _styles[element].Pop();
        }
        public object this[String StyleName, StyleElement element]
        {
            get
            {
                if (!_styleDefs.ContainsKey(StyleName)) return null;
                if (!_styleDefs[StyleName].ContainsKey(element)) return null;
                return _styleDefs[StyleName][element];
            }
        }

        public object this[StyleElement element]
        {
            get
            {
                if (!_styles.ContainsKey(element))
                    return this["Default", element];
                if (_styles[element].Count <= 0)
                    return this["Default", element];
                return _styles[element].Peek();
            }
        }
    }
}
