using BareE.GameDev;
using BareE.Rendering;

using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BareE.GUI.Widgets
{
    public abstract class WidgetBase
    {
        private static int _ID;
        public int ID = ++_ID;
        public Vector2 Position;
        public Vector2 Size;
        public Vector4 Color;
        public List<WidgetBase> Children;
        public WidgetBase Parent;
        public String Text;
        public bool Dirty = true;
        public String Style;
        public abstract void Render(GUIContext renderTo, Rectangle contentRegion);

        protected object ResolveStyle(GUIContext context, StyleElement element)
        {
            object ret;
            if (String.IsNullOrEmpty(Style))
                ret = context.StyleBook[element];
            else
                ret = context.StyleBook[Style, element];
            return ret ?? context.StyleBook["Default", element];
        }
    }
    public class Text:WidgetBase
    {
        public override void Render(GUIContext renderTo, Rectangle contentRegion)
        {
            var fontName = (String)ResolveStyle(renderTo, StyleElement.Font);
            var fontColor = (Vector4)ResolveStyle(renderTo, StyleElement.FontColor);
            var fontSize = (int)ResolveStyle(renderTo, StyleElement.FontSize);
            renderTo.AddTextBlock(contentRegion, 0, Text, fontName, fontSize, fontColor);
        }
    }
    public class Window:WidgetBase
    {
        string longText = System.IO.File.ReadAllText(@"C:\TestData\RatsInTheWalls.txt");
        
        public override void Render(GUIContext renderTo, Rectangle contentRegion)
        {
            if (String.IsNullOrEmpty(Style))
                Style = "Default";

            var innerFrameName = (String)ResolveStyle(renderTo, StyleElement.InnerFrame);
            var innerFrameColor=(Vector4)ResolveStyle(renderTo, StyleElement.InnerFrameColor);

            var outterFrameName = (String)ResolveStyle(renderTo, StyleElement.TitleFrame);
            var outterFrameColor = (Vector4)ResolveStyle(renderTo, StyleElement.TitleFrameColor);

            //var titleHeight = (int)ResolveStyle(renderTo, StyleElement.TitleHeight);
            var imarginH = (int)ResolveStyle(renderTo, StyleElement.InnerFrameMarginHorizontal);
            var imarginV = (int)ResolveStyle(renderTo, StyleElement.InnerFrameMarginVertical);
            var closeBtn = (String)ResolveStyle(renderTo, StyleElement.CloseButton_Normal);
            var closeBtnClr = (Vector4)ResolveStyle(renderTo, StyleElement.CloseButton_NormalColor);

            var srcSz = renderTo.GetOriginalSize(closeBtn);
            var marginH = (int)ResolveStyle(renderTo, StyleElement.TitleFrameMarginHorizontal);
            var marginV = (int)ResolveStyle(renderTo, StyleElement.TitleFrameMarginVertical);
            var fontName = (String)ResolveStyle(renderTo, StyleElement.Font);
            var fontColor = (Vector4)ResolveStyle(renderTo, StyleElement.FontColor);
            var fontSize = (int)ResolveStyle(renderTo, StyleElement.FontSize);


            var titleContentRegion = renderTo.AddBanner(outterFrameName, Position, (int)Size.X, outterFrameColor);
            var titleHeight = titleContentRegion.Height;

            titleContentRegion.X += marginH;
            titleContentRegion.Width -= 2 * marginH;
            titleContentRegion.Y += marginV;
            titleContentRegion.Height -= 2 * marginV;

            var childContentRegion = renderTo.AddFrame(innerFrameName, new Vector2(Position.X, Position.Y + titleHeight), new Vector2(Size.X, Size.Y - titleHeight), innerFrameColor);
            childContentRegion.X += imarginH;
            childContentRegion.Width -= 2 * imarginH;
            childContentRegion.Y += imarginV;
            childContentRegion.Height -= 2 * imarginV;

            renderTo.AddTextBlock(titleContentRegion, 0, Text, fontName, fontSize, fontColor);
            renderTo.AddImage(closeBtn, new Vector2(titleContentRegion.X + titleContentRegion.Width - srcSz.X, titleContentRegion.Y-marginV/2.0f), closeBtnClr);

            renderTo.AddTextBlock(childContentRegion, 0, longText, fontName, fontSize, fontColor);
            renderTo.EndVertSet(new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y));
        }
    }

}
