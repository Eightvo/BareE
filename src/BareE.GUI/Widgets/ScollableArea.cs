using System;
using System.IO;
using System.Numerics;

using Veldrid.Sdl2;

using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace BareE.GUI.Widgets
{
    public class ScollableArea:WidgetBase
    {
        String ScrollTop;
        String ScrollBottom;
        String ScrollLeft;
        String ScrollRight;

        Vector4 ScrollTopColor;
        Vector4 ScrollBottomColor;
        Vector4 ScrollLeftColor;
        Vector4 ScrollRightColor;

        String ScrollHorizontal;
        Vector4 ScrollHorizontalColor;
        String ScrollHorizontalKnob;
        Vector4 ScrollHorizontalKnobColor;

        String ScrollVertical;
        Vector4 ScrollVerticalColor;
        String ScrollVerticalKnob;
        Vector4 ScrollVerticalKnobColor;

        Vector2 ScrollAmount=new Vector2(0,0);
        Vector2 MaxScroll=new Vector2(0,0);
        Vector2 ContentSize;
        Vector2 Margin;
        public override void ReadStyle(GUIContext renderTo)
        {
            base.ReadStyle(renderTo);
            ScrollTop= (String)ResolveStyle(renderTo, StyleElement.Scroll_Top);
            ScrollTopColor = (Vector4)ResolveStyle(renderTo, StyleElement.Scroll_TopColor);
            ScrollVertical= (String)ResolveStyle(renderTo, StyleElement.Scroll_Vertical);
            ScrollVerticalColor= (Vector4)ResolveStyle(renderTo, StyleElement.Scroll_VerticalColor);
            ScrollBottom= (String)ResolveStyle(renderTo, StyleElement.Scroll_Bottom);
            ScrollBottomColor= (Vector4)ResolveStyle(renderTo, StyleElement.Scroll_BottomColor);

            ScrollVerticalKnob= (String)ResolveStyle(renderTo, StyleElement.Scroll_VerticalKnob);
            ScrollVerticalKnobColor= (Vector4)ResolveStyle(renderTo, StyleElement.Scroll_VerticalKnobColor);



            ScrollLeft = (String)ResolveStyle(renderTo, StyleElement.Scroll_Left);
            ScrollLeftColor = (Vector4)ResolveStyle(renderTo, StyleElement.Scroll_LeftColor);
            ScrollHorizontal = (String)ResolveStyle(renderTo, StyleElement.Scroll_Horizontal);
            ScrollHorizontalColor = (Vector4)ResolveStyle(renderTo, StyleElement.Scroll_HorizontalColor);
            ScrollRight = (String)ResolveStyle(renderTo, StyleElement.Scroll_Right);
            ScrollRightColor = (Vector4)ResolveStyle(renderTo, StyleElement.Scroll_RightColor);

            ScrollHorizontalKnob = (String)ResolveStyle(renderTo, StyleElement.Scroll_HorizontalKnob);
            ScrollHorizontalKnobColor = (Vector4)ResolveStyle(renderTo, StyleElement.Scroll_HorizontalKnobColor);

            Margin = new Vector2((int)ResolveStyle(renderTo, StyleElement.MarginHorizontal), (int)ResolveStyle(renderTo, StyleElement.MarginVertical)); 
        }
        bool isScrollDraggingVert = false;
        bool isScrollDraggingHorizontal = false;
        public override void OnMouseButtonEvent(SDL_MouseButtonEvent args)
        {
            if (args.type== SDL_EventType.MouseButtonUp)
            {
                isScrollDraggingVert = false;
                isScrollDraggingHorizontal = false;
            }
            if (args.type == SDL_EventType.MouseButtonDown)
            {
                if (horizontalScrollRegion.Contains(args.x, args.y))
                {
                    var ScrollToPercentage = (args.x - horizontalScrollRegion.X) / (float)verticalScrollRegion.Width;
                    ScrollToPercentage = Math.Clamp(ScrollToPercentage, 0.0f, 1.0f);
                    ScrollAmount.X = ScrollToPercentage * MaxScroll.X;
                    isScrollDraggingHorizontal = true;
                }
                if (verticalScrollRegion.Contains(args.x, args.y))
                {
                    var ScrollToPercentage = (args.y - verticalScrollRegion.Y) / (float)horizontalScrollRegion.Height;
                    ScrollToPercentage = Math.Clamp(ScrollToPercentage, 0.0f, 1.0f);
                    ScrollAmount.Y = ScrollToPercentage * MaxScroll.Y;
                    isScrollDraggingVert = true;
                }
                return;
            }
            base.OnMouseButtonEvent(args);
        }
        public override void OnLostFocus()
        {
            isScrollDraggingVert = false;
            base.OnLostFocus();
        }
        public override void OnMouseMoved(SDL_MouseMotionEvent args)
        {
            if (isScrollDraggingVert) 
           //f (verticalScrollRegion.Contains(args.x, args.y))
            {
                var ScrollToPercentage = (args.y - verticalScrollRegion.Y) / (float)verticalScrollRegion.Height;
                ScrollToPercentage = Math.Clamp(ScrollToPercentage, 0.0f, 1.0f);
                ScrollAmount.Y = ScrollToPercentage * MaxScroll.Y;
                Dirty = true;
                return;
            }
            if (isScrollDraggingHorizontal)
            {
                var ScrollToPercentage = (args.x - horizontalScrollRegion.X) / (float)horizontalScrollRegion.Width;
                ScrollToPercentage = Math.Clamp(ScrollToPercentage, 0.0f, 1.0f);
                ScrollAmount.X = ScrollToPercentage * MaxScroll.X;
                Dirty = true;
            }
            base.OnMouseMoved(args);
        }
        public override void OnMouseWheelMoved(SDL_MouseWheelEvent args)
        {
            if (args.y > 0)
                ScrollAmount.Y += 10;
            else ScrollAmount.Y -= 10;
            if (ScrollAmount.Y > 0) ScrollAmount.Y =0;
            if (ScrollAmount.Y < MaxScroll.Y) ScrollAmount.Y = MaxScroll.Y;
            Dirty = true;
            base.OnMouseWheelMoved(args);
        }
        Rectangle verticalScrollRegion;
        Rectangle horizontalScrollRegion;
        public override Rectangle Render(GUIContext renderTo, Rectangle contentRegion, Vector2 offset)
        {
            Dirty = false;
            offset = offset+=ScrollAmount;
            var ss = renderTo.GetOriginalSize(ScrollTop);

            int widthReduction = 0;
            int heightReduction = 0;
            if (ContentSize.Y>contentRegion.Height)
            {
                int VerticalScrollBarLength = (int)(contentRegion.Height - 3 * ss.Y);
                int scrollIconX = (int)(contentRegion.X + contentRegion.Width - ss.X);
                renderTo.AddImage(ScrollTop, new Vector2(scrollIconX, contentRegion.Y), ScrollTopColor);
                verticalScrollRegion = renderTo.AddScaledImage(ScrollVertical, new Vector2(scrollIconX, (int)(contentRegion.Y + ss.Y)), new Vector2((int)ss.X, VerticalScrollBarLength), ScrollBottomColor);
                renderTo.AddImage(ScrollBottom, new Vector2(scrollIconX, contentRegion.Y + contentRegion.Height - 2 * ss.Y), ScrollBottomColor);
                widthReduction = (int)(ss.X+Margin.X);

                var scollableDistance = ContentSize.Y - contentRegion.Height/2.0f;

                var scrollPercent = -ScrollAmount.Y / scollableDistance;
                var scrollKnobDist = scrollPercent * (VerticalScrollBarLength-ss.Y);
                MaxScroll.Y=-scollableDistance;
                if (MaxScroll.Y > 0) MaxScroll.Y = 0;
                if (ScrollAmount.Y < MaxScroll.Y) ScrollAmount.Y = MaxScroll.Y;
                renderTo.AddImage(ScrollVerticalKnob, new Vector2(scrollIconX, (int)(contentRegion.Y + ss.Y + scrollKnobDist)), ScrollVerticalKnobColor);
            }
            if (ContentSize.X>contentRegion.Width)
            {
                int HorizontalScrollBarLength = (int)(contentRegion.Width - 3 * ss.X);
                var scrollImgY = (int)(contentRegion.Y + contentRegion.Height - ss.Y);
                renderTo.AddImage(ScrollLeft, new Vector2(contentRegion.X, scrollImgY), ScrollLeftColor);
                horizontalScrollRegion=renderTo.AddScaledImage(ScrollHorizontal, new Vector2((int)(contentRegion.X + ss.X), scrollImgY), new Vector2(HorizontalScrollBarLength, (int)(ss.Y)), ScrollHorizontalColor);
                renderTo.AddImage(ScrollRight, new Vector2(contentRegion.X + contentRegion.Width - 2 * ss.X, scrollImgY), ScrollRightColor);
                heightReduction = (int)(ss.Y+Margin.Y);

                var scollableDistance = ContentSize.X - contentRegion.Width;

                var scrollPercent = -ScrollAmount.X / scollableDistance;
                var scrollKnobDist = scrollPercent * (HorizontalScrollBarLength - ss.X);
                MaxScroll.X = -scollableDistance;
                if (MaxScroll.X > 0) MaxScroll.X = 0;
                if (ScrollAmount.X<MaxScroll.X) ScrollAmount.X=MaxScroll.X;
                renderTo.AddImage(ScrollHorizontalKnob, new Vector2((int)(contentRegion.X+ss.X+scrollKnobDist), scrollImgY), ScrollHorizontalKnobColor);
            }

            renderTo.EndVertSet(contentRegion, true);
            contentRegion.Width -= widthReduction;
            contentRegion.Height -= heightReduction;
            
            var totalHeight = 0;
            var totalWidth = 0;
            foreach (var child in Children)
            {
                var childSz = child.Render(renderTo, contentRegion, offset);
                var childWidth = childSz.Width;
                if (childWidth>totalWidth)
                    totalWidth=childWidth;
                var childHeight =childSz.Height;
                if (childHeight > totalHeight)
                    totalHeight = childHeight;
            }
            if (ContentSize.X != totalWidth || ContentSize.Y != totalHeight)
            {
                Dirty = true;
                ContentSize = new Vector2(totalWidth, totalHeight);
            }
            return contentRegion;
        }
    }

}
