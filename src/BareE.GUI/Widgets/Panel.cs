﻿using BareE.DataStructures;
using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using RectangleF = SixLabors.ImageSharp.RectangleF;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Color = SixLabors.ImageSharp.Color;
namespace BareE.GUI.Widgets
{
    public class Panel : GuiWidgetBase
    {
        public override string WidgetType => "Panel";
        public Panel(AttributeCollection def, GUIContext context, GuiWidgetBase parent):base(def, context, parent)
        {

        }
    }
    public class Border : GuiWidgetBase
    {
        public override string WidgetType => "Border";
        public Border(AttributeCollection def, GUIContext context, GuiWidgetBase parent) : base(def, context, parent)
        {

        }
        
    }
    public class Frame : GuiWidgetBase
    {
        public override string WidgetType => "Frame";
        public Frame(AttributeCollection def, GUIContext context, GuiWidgetBase parent) : base(def, context, parent)
        {

        }

    }
    public class Window: GuiWidgetBase
    {
        public override string WidgetType => "Window";

        public bool AllowClose { get; set; } = true;
        public bool AllowExpand { get; set; } = true;
        public bool AllowCollapse { get; set; } = true;

        public override RenderStyle RenderStyle { get; protected set; } = RenderStyle.NineFrame;
        public Window(AttributeCollection def, GUIContext context, GuiWidgetBase parent) : base(def, context, parent)
        {

        }
        int titleBarHeight = 40;
        public override Rectangle GetDisplayArea(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context)
        {
            return new Rectangle(this.FootPrint.X, this.FootPrint.Y+titleBarHeight, this.FootPrint.Width, this.FootPrint.Height-titleBarHeight);

        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context, Rectangle displayArea)
        {
            switch(RenderStyle)
            {
                case RenderStyle.NineFrame:
                    context.DrawNineFrame("Default_Frame"   , this.FootPrint, this.ZIndex);
                    
                    var titleBarRect = new RectangleF(this.FootPrint.X, this.FootPrint.Y, this.FootPrint.Width, titleBarHeight);
                    context.DrawNineFrame("Default_Titlebar", titleBarRect, this.ZIndex, (Vector4)SixLabors.ImageSharp.Color.CornflowerBlue);
                   
                    var xOff = 0;
                    if (AllowClose)
                    {
                        var closeIconRect = new RectangleF(this.FootPrint.X + this.FootPrint.Width - titleBarHeight, titleBarRect.Y, titleBarHeight, titleBarHeight);
                        context.DrawGlyph("Default_CloseButton", closeIconRect, this.ZIndex, (Vector4)Color.Black);
                        xOff += titleBarHeight;
                    }
                    if (AllowExpand)
                    {
                        var closeIconRect = new RectangleF(this.FootPrint.X + this.FootPrint.Width - (titleBarHeight + xOff), titleBarRect.Y, titleBarHeight, titleBarHeight);
                        context.DrawGlyph("Default_ExpandButton", closeIconRect, this.ZIndex, (Vector4)Color.Black);
                        xOff += titleBarHeight;
                    }
                    if (AllowCollapse)
                    {
                        var closeIconRect = new RectangleF(this.FootPrint.X + this.FootPrint.Width - (titleBarHeight + xOff), titleBarRect.Y, titleBarHeight, titleBarHeight);
                        context.DrawGlyph("Default_CollapseButton", closeIconRect, this.ZIndex, (Vector4)Color.Black);
                        xOff += titleBarHeight;
                    }
                    context.DrawString(this.Text, new RectangleF(this.FootPrint.X + 20, this.FootPrint.Y , this.FootPrint.Width - titleBarHeight + xOff, titleBarHeight), this.ZIndex, titleBarHeight * 0.8f, "Neuton", (Vector4)Color.Black);

                    
                    break;
                default:
                    throw new Exception("Windows must be RenderStyle NineFrame");
            }
            context.EndVertSet(FootPrint);
            base.Render(Instant, GameState, Env, context, this.GetDisplayArea(Instant,GameState,Env,context));
        }

    }

    public class Text : GuiWidgetBase
    {
        public override string WidgetType => "Text";
        public Text(AttributeCollection def, GUIContext context, GuiWidgetBase parent) : base(def, context, parent)
        {
            
        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context, Rectangle displayArea)
        {
           // return;
            var blX = displayArea.X + FootPrint.X;
            var blY = FootPrint.Y;// (displayArea.Y-displayArea.Height) + FootPrint.Y;
            var W = Math.Max(displayArea.Right-blX, FootPrint.Width);
            var H = Math.Max(displayArea.Top - blY, FootPrint.Height);
            blY = 0;

            var textBoxArea = new Rectangle((int)blX, (int)blY, (int)W, (int)H);

            Rectangle finalArea =new Rectangle(displayArea.X+FootPrint.X, displayArea.Y, displayArea.Width, displayArea.Height);

           // context.DrawGlyph("Default_Mouse_Cursor_Normal", displayArea, ZIndex, new Vector4(1, 0, 0, 1));
           // context.DrawGlyph("Default_Mouse_Cursor_Normal", FootPrint, ZIndex, new Vector4(0, 1, 0, 1));
           // context.DrawGlyph("Default_Mouse_Cursor_Normal", finalArea, ZIndex, new Vector4(0, 0, 1, 1));
            context.DrawString(this.Text, finalArea, this.ZIndex, 16, "neuton", (Vector4)SixLabors.ImageSharp.Color.Black, false);
           // context.DrawString(this.Text, textBoxArea, this.ZIndex + 1, 16, "neuton", (Vector4)SixLabors.ImageSharp.Color.Black, false);
            context.EndVertSet(displayArea);
            //context.
        }
    }
    public class Button : GuiWidgetBase
    {
        public override string WidgetType => "Button";
        public Button(AttributeCollection def, GUIContext context, GuiWidgetBase parent) : base(def, context, parent)
        {

        }
    }
}
