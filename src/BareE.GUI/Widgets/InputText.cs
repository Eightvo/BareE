using SixLabors.ImageSharp;

using System;
using System.Numerics;

namespace BareE.GUI.Widgets
{
    public class InputText: WidgetBase 
    {
        String FontName;
        Vector4 FontColor;
        int FontSize;
        String FrameImage;
        Vector4 FrameColor;
        int MarginH;
        int MarginV;

        Rectangle TextCursorPosition;

        public override void ReadStyle(GUIContext renderTo)
        {
            FontName = (String)ResolveStyle(renderTo, StyleElement.Font);
            FontColor = (Vector4)ResolveStyle(renderTo, StyleElement.FontColor);
            FontSize = (int)ResolveStyle(renderTo, StyleElement.FontSize);
            FrameImage = (String)ResolveStyle(renderTo, StyleElement.Frame);
            FrameColor = (Vector4)ResolveStyle(renderTo, StyleElement.FrameColor);
            MarginH = (int)ResolveStyle(renderTo, StyleElement.MarginHorizontal);
            MarginV = (int)ResolveStyle(renderTo, StyleElement.MarginVertical);

            base.ReadStyle(renderTo);
        }
        Vector4 color = new Vector4(1, 1, 1, 1);
        Random rng = new Random();
        long Second;
        public override Rectangle DynamicRender(GUIContext renderTo, Rectangle contentRegion, Vector2 offset)
        {
            if (Second<DateTime.Now.Ticks)
            {
                Second = DateTime.Now.Ticks + 1000;
                color = new Vector4((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble(), 1);
            }
            renderTo.AddQuad(TextCursorPosition, 0, color, false);
            renderTo.EndVertSet(contentRegion, false);
            return contentRegion;
        }
        public override Rectangle Render(GUIContext renderTo, Rectangle contentRegion, Vector2 offset)
        {
            Dirty = false;
            var sPos = new Vector2(contentRegion.X, contentRegion.Y) + Position;// + offset);
            var sSzX = Size.X;
            var sSzY = Size.Y;
            if (sSzX == 0) sSzX = contentRegion.Width;
            if(sSzY==0) Size = new Vector2(Size.X, 2 * MarginV + renderTo.GetLineHeight(FontName, FontSize));

            var sTextArea = new Rectangle((int)sPos.X + MarginH, (int)sPos.Y + MarginV, (int)sSzX, (int)sSzY);

            renderTo.AddFrame(FrameImage, sPos, new Vector2(sSzX,sSzY), FrameColor);

            var t = renderTo.AddTextBlock(sTextArea, 0, Text, FontName, FontSize, FontColor);
            TextCursorPosition = new Rectangle(t.X,t.Y, MarginH, MarginV);
            //renderTo.AddQuad(t, 00, new Vector4(0, 1, 1, 1));

            renderTo.EndVertSet(contentRegion, true);
            return sTextArea;
        }
    }

}
