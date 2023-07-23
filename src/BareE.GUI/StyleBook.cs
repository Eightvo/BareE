using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BareE.GUI
{
    public enum GUILayer
    {
        Base=0,
        Window=1000,
        FocusedWindow=2000
    }
    /// <summary>
    /// </summary>
    public enum RenderType
    {
        None,
        Frame,
        Banner,
        Column,
        Image,
        Solid
    }
    public enum StyleElement
    {
        RenderType=1,
        LayoutType,

        TitleFrame,
        TitleFrameColor,
        TitleMarginVertical,
        TitleMarginHorizontal,

        Frame,
        FrameColor,
        MarginHorizontal,
        MarginVertical,
        PaddingHorizontal,
        PadidingVertical,

        Font,
        FontSize,
        FontColor,

        CloseButton_Normal,
        CloseButton_NormalColor,
        CloseButton_Hover,
        CloseButton_HoverColor,
        CloseButton_Down,
        CloseButton_DownColor,

        Scroll_Bottom,
        Scroll_BottomColor,
        Scroll_Top,
        Scroll_TopColor,
        Scroll_Left,
        Scroll_LeftColor,
        Scroll_Right,
        Scroll_RightColor,
        Scroll_Horizontal,
        Scroll_HorizontalColor,
        Scroll_Vertical,
        Scroll_VerticalColor,
        Scroll_HorizontalKnob,
        Scroll_HorizontalKnobColor,
        Scroll_VerticalKnob,
        Scroll_VerticalKnobColor,



        MouseCursor_Normal,
        MouseCursor_NormalColor,
        MouseCursor_Down,
        MouseCursor_DownColor,
        MouseCursor_OffsetX,
        MouseCursor_OffsetY,

        ResizeButton_Normal,
        ResizeButton_NormalColor,


    }
    public class StyleDefinition
    {
        Dictionary<String, object> _defs = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        public bool ContainsKey(StyleElement element) { return ContainsKey(element.ToString()); }
        public bool ContainsKey(String key) { return _defs.ContainsKey(key); }

        public object this[StyleElement element] { get { return this[element.ToString()]; } }
        public object this[String elementName] { get { return _defs[elementName]; } }
        public void Add(StyleElement element, object value) { Add(element.ToString(), value); }
        public void Add(String elementName, object value) { _defs.Add(elementName, value); }
    }
    public class StyleBook
    {
        private Dictionary<String, StyleDefinition> _styleDefs;
        public StyleBook()
        {
            
            _styleDefs = new Dictionary<string, StyleDefinition>();
        }
        public void DefineStyle(String name, StyleDefinition def)
        {
            if (!_styleDefs.ContainsKey(name))
                _styleDefs.Add(name, def);
            else _styleDefs[name]= def; 
        }
        public object this[String StyleName, String element]
        {
            get
            {
                if (!_styleDefs.ContainsKey(StyleName)) return null;
                if (!_styleDefs[StyleName].ContainsKey(element)) return null;
                return _styleDefs[StyleName][element];
            }
        }
        public object this[String StyleName, StyleElement element]
        {
            get
            {
                return this[StyleName, element.ToString()];
            }
        }
        public object this[StyleElement element]
        {
            get
            {
                return this["Default", element];
            }
        }
       
    }
}
