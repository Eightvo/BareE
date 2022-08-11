using BareE.DataStructures;
using BareE.GameDev;

using System;
using System.Numerics;
using RectangleF = SixLabors.ImageSharp.RectangleF;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Color = SixLabors.ImageSharp.Color;
namespace BareE.GUI.Widgets
{
    public class Window: GuiWidgetBase
    {
        public override string WidgetType => "Window";

        public bool AllowClose { get; set; } = true;
        public bool AllowExpand { get; set; } = true;
        public bool AllowCollapse { get; set; } = true;



        public RectangleF RenderedTitleBarRect;
        

        public override RenderStyle RenderStyle { get; protected set; } = RenderStyle.NineFrame;
        public Window(AttributeCollection def, GUIContext context, GuiWidgetBase parent) : base(def, context, parent)
        {

        }
        int titleBarHeight = 40;
        public override Rectangle GetDisplayArea(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context)
        {

            var t = new Rectangle(this.FootPrint.X, this.FootPrint.Y+titleBarHeight, this.FootPrint.Width, this.FootPrint.Height-titleBarHeight);
            if (!_isDragging) return t;
            Vector2 _delta = context.MousePosition - _dragPt;
            t.X += (int)_delta.X;
            t.Y += (int)_delta.Y;
            return t;


        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context, Rectangle displayArea)
        {
            if (!Visible) return;
            Rectangle trueFootprint = this.FootPrint;
            if (_isDragging)
            {
                Vector2 _delta = context.MousePosition - _dragPt;
                trueFootprint.X += (int)_delta.X;
                trueFootprint.Y += (int)_delta.Y;
            }
            switch(RenderStyle)
            {
                case RenderStyle.NineFrame:
                    context.DrawNineFrame("Default_Frame"   , trueFootprint, this.ZIndex);

                    RenderedTitleBarRect = new RectangleF(trueFootprint.X, trueFootprint.Y, trueFootprint.Width, titleBarHeight);
                    context.DrawNineFrame("Default_Titlebar", RenderedTitleBarRect, this.ZIndex, (Vector4)SixLabors.ImageSharp.Color.CornflowerBlue);
                   
                    var xOff = 0;
                    if (AllowClose)
                    {
                        var closeIconRect = new RectangleF(trueFootprint.X + trueFootprint.Width - titleBarHeight, RenderedTitleBarRect.Y, titleBarHeight, titleBarHeight);
                        context.DrawGlyph("Default_CloseButton", closeIconRect, this.ZIndex, (Vector4)Color.Black);
                        xOff += titleBarHeight;
                    }
                    if (AllowExpand)
                    {
                        var closeIconRect = new RectangleF(trueFootprint.X + trueFootprint.Width - (titleBarHeight + xOff), RenderedTitleBarRect.Y, titleBarHeight, titleBarHeight);
                        context.DrawGlyph("Default_ExpandButton", closeIconRect, this.ZIndex, (Vector4)Color.Black);
                        xOff += titleBarHeight;
                    }
                    if (AllowCollapse)
                    {
                        var closeIconRect = new RectangleF(trueFootprint.X + trueFootprint.Width - (titleBarHeight + xOff), RenderedTitleBarRect.Y, titleBarHeight, titleBarHeight);
                        context.DrawGlyph("Default_CollapseButton", closeIconRect, this.ZIndex, (Vector4)Color.Black);
                        xOff += titleBarHeight;
                    }
                    context.DrawString(this.Text, new RectangleF(trueFootprint.X + 20, trueFootprint.Y , trueFootprint.Width - titleBarHeight + xOff, titleBarHeight), this.ZIndex, titleBarHeight * 0.8f, "Neuton", (Vector4)Color.Black);

                    
                    break;
                default:
                    throw new Exception("Windows must be RenderStyle NineFrame");
            }
            context.EndVertSet(trueFootprint);
            base.Render(Instant, GameState, Env, context, this.GetDisplayArea(Instant,GameState,Env,context));
        }


        bool _isDragging = false;
        Vector2 _dragPt = Vector2.Zero;

        protected internal override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Vector2 _endPt = new Vector2(e.MouseButtonEvent.x, e.MouseButtonEvent.y);
                Vector2 _delta = _endPt - _dragPt;
                Position =new System.Drawing.Point(Position.X + (int)_delta.X, Position.Y+ (int)_delta.Y);
            }
            base.OnMouseUp(e);
        }
        protected internal override void OnMouseDown(MouseButtonEventArgs e)
        {
            //var mP = this.ConvertScreenPtToBeRelativeToWidget();
            
            if (RenderedTitleBarRect.Contains(e.MouseButtonEvent.x, e.MouseButtonEvent.y))
            {
                if (!_isDragging)
                    _dragPt = new Vector2(e.MouseButtonEvent.x, e.MouseButtonEvent.y);
                _isDragging = true;
                
            } else
                base.OnMouseDown(e);
        }
    }
}
