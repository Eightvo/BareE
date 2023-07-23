using BareE.DataStructures;

using System;
using System.Collections.Generic;
using System.Numerics;

using Veldrid;

using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace BareE.GUI.Widgets
{

    public class Text:WidgetBase
    {
        String FontName;
        Vector4 FontColor;
        int FontSize;
        
        public override void ReadAttributes(AttributeCollection def)
        {
            base.ReadAttributes(def);
        }
        public override void ReadStyle(GUIContext renderTo)
        {
            FontName = (String)ResolveStyle(renderTo, StyleElement.Font);
            FontColor = (Vector4)ResolveStyle(renderTo, StyleElement.FontColor);
            FontSize = (int)ResolveStyle(renderTo, StyleElement.FontSize);
            base.ReadStyle(renderTo);
        }
        public override Rectangle Render(GUIContext renderTo, Rectangle contentRegion, Vector2 offset)
        {
            Dirty = false;
            base.Render(renderTo, contentRegion, offset);
            var szX = Size.X;
            if (szX == 0)
                szX = contentRegion.X + contentRegion.Width - (Position.X + contentRegion.X);
            var textArea = new Rectangle((int)(contentRegion.X+Position.X+offset.X), (int)(contentRegion.Y+Position.Y+offset.Y),(int)szX,(int)Size.Y);

            var ret = renderTo.AddTextBlock(textArea, contentRegion, 0, Text, FontName, FontSize, FontColor);
            ret.Width = textArea.Width;
            renderTo.EndVertSet(contentRegion);
            return ret;
        }
    }

}
