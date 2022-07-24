using BareE.DataStructures;
using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using RectangleF = SixLabors.ImageSharp.RectangleF;
using Color = SixLabors.ImageSharp.Color;
namespace BareE.GUI.Widgets
{
    public class Panel : GuiWidgetBase
    {
        public override string WidgetType => "Panel";
        public Panel(AttributeCollection def, GUIContext context):base(def, context)
        {

        }
    }
    public class Border : GuiWidgetBase
    {
        public override string WidgetType => "Border";
        public Border(AttributeCollection def, GUIContext context) : base(def, context)
        {

        }
        
    }
    public class Frame : GuiWidgetBase
    {
        public override string WidgetType => "Frame";
        public Frame(AttributeCollection def, GUIContext context) : base(def, context)
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
        public Window(AttributeCollection def, GUIContext context) : base(def, context)
        {

        }
        int titleBarHeight = 40;
        public override RectangleF GetDisplayArea(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context)
        {
            return new RectangleF(this.FootPrint.X, this.FootPrint.Y, this.FootPrint.Width, this.FootPrint.Height-titleBarHeight);

        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context, RectangleF displayArea)
        {
            switch(RenderStyle)
            {
                case RenderStyle.NineFrame:
                    context.DrawNineFrame("Default_Frame"   , this.FootPrint, this.ZIndex);

                    var titleBarRect = new RectangleF(this.FootPrint.X, this.FootPrint.Y + this.FootPrint.Height - titleBarHeight, this.FootPrint.Width, titleBarHeight);
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
                    context.DrawString(this.Text, new RectangleF(this.FootPrint.X + 20, this.FootPrint.Y + this.FootPrint.Height - titleBarHeight, this.FootPrint.Width - titleBarHeight + xOff, titleBarHeight), this.ZIndex, titleBarHeight * 0.8f, "Neuton", (Vector4)Color.Black);


                    break;
                default:
                    throw new Exception("Windows must be RenderStyle NineFrame");
            }
            base.Render(Instant, GameState, Env, context, this.GetDisplayArea(Instant,GameState,Env,context));
        }

    }

    public class Text : GuiWidgetBase
    {
        public override string WidgetType => "Text";
        public Text(AttributeCollection def, GUIContext context) : base(def, context)
        {
            
        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context, RectangleF displayArea)
        {
            var blX = displayArea.X + FootPrint.X;
            var blY = displayArea.Y + FootPrint.Y;
            var W = Math.Max(displayArea.Right-blX, FootPrint.Width);
            var H = Math.Max(displayArea.Top - blY, FootPrint.Height);
            var textBoxArea = new RectangleF(blX, blY, W, H);
            context.DrawString(this.Text, textBoxArea, this.ZIndex, 16, "neuton", (Vector4)SixLabors.ImageSharp.Color.Black, false);
        }
    }
    public class Button : GuiWidgetBase
    {
        public override string WidgetType => "Button";
        public Button(AttributeCollection def, GUIContext context) : base(def, context)
        {

        }
    }
}
