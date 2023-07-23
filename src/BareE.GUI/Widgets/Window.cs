using BareE.DataStructures;

using SixLabors.ImageSharp;

using System;
using System.Net.Http.Headers;
using System.Numerics;

using Veldrid.Sdl2;

using Vortice.Direct3D11;

using Vulkan;

using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace BareE.GUI.Widgets
{

    public class Window:WidgetBase
    {
        string longText = System.IO.File.ReadAllText(@"C:\TestData\RatsInTheWalls.txt");
        Vector2 ogVect;
        Vector2 mousedownVect;
        bool isResizing = false;
        bool isDragging = false;

        bool AllowResize;
        bool AllowDrag;


        Rectangle titleContentRegion;
        Rectangle childContentRegion;
        Rectangle resizeRegion;
        Rectangle scrollJumpRegion;
        public override Vector2 Position {
            get {
                if (isDragging)
                {
                    return base.Position + ( mousedownVect- ogVect);
                }
                
                return base.Position;
            }
            
            set{
                base.Position = value;
            }
        }
        public override Vector2 Size
        {
            get
            {
                if (isResizing)
                    return base.Size + (mousedownVect - ogVect);

                return base.Size;
            }

            set
            {
                base.Size = value;
            }
        }

        public override bool Dirty
        {
            get
            {
                return base.Dirty;
            }
            set
            {
                base.Dirty = value;
            }
        }

        public Window()
        {
            //Children = new System.Collections.Generic.List<WidgetBase>();
            //AddChild(new IconButton() { Position = new Vector2(Size.X-11, 3), Size=new Vector2(8,8) });
        }
        public override void OnMouseButtonEvent(SDL_MouseButtonEvent args)
        {
            if ((args.button & SDL_MouseButton.Left)!= 0)
            {
                if (args.type == SDL_EventType.MouseButtonDown)
                {
                    if (titleContentRegion.Contains(args.x, args.y))
                        isDragging = true & AllowDrag;
                    if (resizeRegion.Contains(args.x, args.y))
                        isResizing = true & AllowResize;
                    if (scrollJumpRegion.Contains(args.x,args.y))
                    {
                        scrolledAmount.Y = -(int)((args.y - scrollJumpRegion.Y)/(float)scrollJumpRegion.Height*ScrollAreaHeight);
                    }
                    mousedownVect = new Vector2(args.x, args.y);
                    ogVect = mousedownVect;
                    Dirty = true;
                }
                else if (args.type == SDL_EventType.MouseButtonUp)
                {
                    if (isDragging)
                    {
                        isDragging = false;
                        Position = Position + mousedownVect - ogVect;
                        Dirty = true;
                    }
                    if (isResizing)
                    {
                        isResizing = false;
                        Size = Size + mousedownVect - ogVect;
                        Dirty = true;
                    }
                }
            }
            base.OnMouseButtonEvent(args);
        }
        public override void OnMouseMoved(SDL_MouseMotionEvent args)
        {
            mousedownVect = new Vector2(args.x,args.y);
            if (isDragging || isResizing)
                Dirty = true;
            base.OnMouseMoved(args);
        }

        public override void OnMouseWheelMoved(SDL_MouseWheelEvent args)
        {

            base.OnMouseWheelMoved(args);
        }
        Point scrolledAmount = new Point(0, 0);
        int ScrollAreaHeight;

        String FrameImage;
        Vector4 FrameColor;
        int MarginH;
        int MarginV;
        String FontName;
        Vector4 FontColor;
        int FontSize;
        String Resize_Button;
        Vector4 Resize_ButtonColor;

        String outterFrameName;
        Vector4 outterFrameColor;

        public override void ReadStyle(GUIContext renderTo)
        {
            FrameImage = (String)ResolveStyle(renderTo, StyleElement.Frame);
            FrameColor = (Vector4)ResolveStyle(renderTo, StyleElement.FrameColor);
            MarginH = (int)ResolveStyle(renderTo, StyleElement.MarginHorizontal);
            MarginV = (int)ResolveStyle(renderTo, StyleElement.MarginVertical);
            FontName = (String)ResolveStyle(renderTo, StyleElement.Font);
            FontColor = (Vector4)ResolveStyle(renderTo, StyleElement.FontColor);
            FontSize = (int)ResolveStyle(renderTo, StyleElement.FontSize);
            Resize_Button = (String)ResolveStyle(renderTo, StyleElement.ResizeButton_Normal);
            Resize_ButtonColor = (Vector4)ResolveStyle(renderTo, StyleElement.ResizeButton_NormalColor);
            outterFrameName = (String)ResolveStyle(renderTo, StyleElement.TitleFrame);
            outterFrameColor = (Vector4)ResolveStyle(renderTo, StyleElement.TitleFrameColor);

            AllowResize = (bool)ResolveStyle(renderTo, "AllowResize");
            AllowDrag = (bool)ResolveStyle(renderTo, "AllowDrag");
            
            base.ReadStyle(renderTo);
        }
        public override void ReadAttributes(AttributeCollection def)
        {
            base.ReadAttributes(def);
            CustomStyle.Add("AllowResize", def.DataAs<Boolean>("AllowResize"));
            CustomStyle.Add("AllowDrag", def.DataAs<bool>("AllowDrag"));
        }
        public override Rectangle Render(GUIContext renderTo, Rectangle contentRegion, Vector2 offset)
        {

            Dirty = false;
            renderTo.BeginPass($"{ID}",ZIndex, null);
           
            

            var TPosition = Position + offset;

            titleContentRegion = renderTo.AddBanner(FrameImage, TPosition, (int)Size.X, FrameColor);
            var titleHeight = titleContentRegion.Height;


            childContentRegion = renderTo.AddFrame(FrameImage, new Vector2(TPosition.X, TPosition.Y + titleHeight), new Vector2(Size.X, Size.Y - titleHeight), FrameColor);
            childContentRegion.Inflate(-MarginH, -MarginV);
            var r = titleContentRegion;
            r.Inflate(-MarginH, -MarginV);
            renderTo.AddTextBlock(r,r, 0, Text, FontName, FontSize, FontColor);

            renderTo.EndVertSet(new Rectangle((int)TPosition.X, (int)TPosition.Y, (int)Size.X, (int)Size.Y), true);
            base.Render(renderTo, childContentRegion, offset);
            if (AllowResize)
            {
                var ResizeBtnSize = renderTo.GetOriginalSize(Resize_Button);
                resizeRegion = new Rectangle((int)(TPosition.X + Size.X - ResizeBtnSize.X), (int)(TPosition.Y + Size.Y - ResizeBtnSize.Y), (int)ResizeBtnSize.X, (int)ResizeBtnSize.Y);
                renderTo.AddImage(Resize_Button, new Vector2(resizeRegion.X,resizeRegion.Y), Resize_ButtonColor);
                renderTo.EndVertSet(resizeRegion);
            }
           
           
           return new Rectangle((int)TPosition.X, (int)TPosition.Y, (int)Size.X, (int)Size.Y);
        }
    }

}
