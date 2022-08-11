using BareE.DataStructures;
using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid.Sdl2;

using Rectangle = SixLabors.ImageSharp.Rectangle;
using RectangleF = SixLabors.ImageSharp.RectangleF;
namespace BareE.GUI.Widgets
{
    public enum RenderStyle
    {
        Glyph = 0,
        NineFrame = 1,
        VerticalThreeFrame = 2,
        HorizontalThreeFrame = 3
    }
    public enum AnchorPoint
    {
        None = 0,
        Top = 1,
        Left = 2,
        Right = 4,
        Bottom = 8,
    }
    public class MouseButtonEventArgs:EventArgs{
        public SDL_MouseButtonEvent MouseButtonEvent { get; set; }
        public MouseButtonEventArgs(SDL_MouseButtonEvent mouseButtonEvent)
        {
            MouseButtonEvent = mouseButtonEvent;
        }
    }
    
    public abstract class GuiWidgetBase  {
        static private int _id;
        public int ID { get; set; }
        GuiWidgetBase Parent { get; set; }
        public virtual string Name { get; protected set; }
        public abstract string WidgetType { get; }
        public virtual string Text { get; set; }
        private bool _dirty = true;
        public virtual bool Dirty { get { if (_dirty) return true; foreach (var c in ChildWidgets) if (c.Dirty) return true; return false; } set { _dirty = value; } } 
        public virtual bool Visible { get;  set; } = true;
        public virtual bool Active { get;  set; } = false;
        public virtual bool AllowDrop { get; protected set; } = false;
        public virtual bool ClickThrough { get; protected set; } = false;
        public virtual bool CanFocus { get; protected set; } = false;

        public AttributeCollection Style { get; set; } = new AttributeCollection();

        private bool _focused = false;
        public virtual bool ContainsFocus { get { if (_focused) return true; foreach (var c in ChildWidgets) if (c.ContainsFocus) return true; return false; } protected set { _focused = value; } } 

        public virtual AnchorPoint Anchor { get; protected set; } = AnchorPoint.None;
        public virtual RenderStyle RenderStyle { get; protected set; } = RenderStyle.Glyph;
        public virtual Size Size { get; protected set; }
        public virtual Point Position { get; protected internal set; }
        public virtual Rectangle FootPrint { get { return new Rectangle(Position.X, Position.Y, Size.Width, Size.Height); } }
        public virtual Rectangle Viewport { get; protected set; }
        
        public virtual int KeyRefreshRate { get; protected set; }
        public virtual int ZIndex { get; set; } 

        protected Dictionary<String, GuiWidgetBase> Children = new Dictionary<string, GuiWidgetBase>(StringComparer.OrdinalIgnoreCase);

        public virtual IEnumerable<GuiWidgetBase> ChildWidgets
        {
            get
            {
                return Children.Values.ToList();
            }
        }

        public GuiWidgetBase(BareE.DataStructures.AttributeCollection def, GUIContext context, GuiWidgetBase parent)
        {
            _id++;
            ID = _id;
            Parent = parent;
            Name = def.DataAs<String>("Name") ?? $"{WidgetType}{ID}";
            Text = def.DataAs<String>("Text") ?? "";
            Visible =def.HasAttribute("Visible") ? def.DataAs<bool>("Visible"):true;
            Active = def.DataAs<bool>("Active");
            AllowDrop = def.DataAs<bool>("AllowDrop");
            ClickThrough =def.HasAttribute("ClickThrought")? def.DataAs<bool>("ClickThrough"):false;
            CanFocus = def.DataAs<bool>("CanFocus");
            ContainsFocus = def.DataAs<bool>("ContainsFocus");
            Anchor = def.DataAs<AnchorPoint>("Anchor");
            Style = (AttributeCollection)def["Style"];
            if (def.HasAttribute("RenderStyle"))
                RenderStyle = def.DataAs<RenderStyle>("RenderStyle");
            switch(def["Size"])
            {
                case null: throw new Exception($"Widget {Name} requires a size.");
                case Vector2 v2: Size = new Size((int)v2.X, (int)v2.Y);break;
                case String szStr:
                    {
                        Size sz;
                        Size rz = context.Resolution;
                        
                        if (parent != null)
                        {
                            var pSz = parent.GetDisplayArea(new Instant(), null, null, context);
                            rz=new Size(pSz.Width, pSz.Height);
                        }
                            
                        if (!StringHelper.TryParseSize(szStr, rz, out sz))
                            throw new Exception($"Invalid size {szStr}");
                        Size=sz;
                    }
                    break;
                default:
                    {
                        throw new Exception($"Invalid size {def["Size"]}");
                    }
            }
            switch (def["Position"])
            {
                case null: Position = new Point(0, 0); break;

                case Vector2 v2: Position = new Point((int)v2.X, (int)v2.Y); break;
                case string posStr:
                    {
                        Point pos;
                        Size rz = context.Resolution;

                        if (parent != null)
                        {
                            var pSz = parent.GetDisplayArea(new Instant(), null, null, context);
                            rz = new Size(pSz.Width, pSz.Height);
                        }
                        if (!StringHelper.TryParsePosition(posStr, Size, rz, out pos)) 
                          throw new Exception($"Invalid position {posStr}");
                        Position = pos;
                    }
                    break;
                default:
                    throw new Exception($"Invalid position {def["Position"]}");
            }
            if (def["Children"] != null)
            {
                foreach (var child in def.DataAs<Object[]>("Children"))
                {
                    var childWidget = context.CreateWidget((AttributeCollection)child, this);
                    Children.Add(childWidget.Name, childWidget);
                }
            }
        }

        public virtual Rectangle GetDisplayArea(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context)
        {
            var padding = (int)(context.Styles.GetStyleDefinition("Default_Padding")??2);
            return new Rectangle(this.FootPrint.X + padding, this.FootPrint.Y + padding, this.FootPrint.Width-2*padding, this.FootPrint.Height-2*padding);    
        }
        protected Vector2 ConvertScreenPtToBeRelativeToWidget(int x, int y)
        {
            if (Parent==null) return new Vector2(x, y) - new Vector2(Position.X, Position.Y);
            return Parent.ConvertScreenPtToBeRelativeToWidget(x, y) - new Vector2(Position.X, Position.Y);
       }
        public virtual void Render(Instant Instant, GameState GameState, GameEnvironment Env, GUIContext context, Rectangle displayArea)
        {
            if (!Visible) return;
            foreach (var widget in Children.OrderBy(x => x.Value.ZIndex))
                widget.Value.Render(Instant, GameState, Env, context, GetDisplayArea(Instant,GameState,Env,context));
        }


        public event EventHandler Updated;
        public event EventHandler Initialized;
        public event EventHandler Shown;
        public event EventHandler Hidden;
        public event EventHandler Closed;
        public event EventHandler Resized;

        public event EventHandler KeyDown;
        public event EventHandler KeyUp;

        public event EventHandler MouseDown;
        public event EventHandler MouseUp;
        public event EventHandler MouseMoved;

        public event EventHandler DragDropped;


        protected internal virtual void OnUpdate(UpdateEventArgs e) { Updated?.Invoke(this, e); }
        protected internal virtual void OnInitilalized(UpdateEventArgs e) { Initialized?.Invoke(this, e); }
        protected internal virtual void OnShown(UpdateEventArgs e) { Shown?.Invoke(this, e); }
        protected internal virtual void OnHidden(UpdateEventArgs e) { Hidden?.Invoke(this, e); }
        protected internal virtual void OnClosed(UpdateEventArgs e) { Closed?.Invoke(this, e); }
        protected internal virtual void OnResized(UpdateEventArgs e) { Resized?.Invoke(this, e); }
        protected internal virtual void OnKeyDown(UpdateEventArgs e) { KeyDown?.Invoke(this, e); }
        protected internal virtual void OnKeyUp(UpdateEventArgs e) { KeyUp?.Invoke(this, e); }
        protected internal virtual void OnMouseDown(MouseButtonEventArgs e) { MouseDown?.Invoke(this, e); }
        protected internal virtual void OnMouseUp(MouseButtonEventArgs e) { MouseUp?.Invoke(this, e); }
        protected internal virtual void OnMouseMove(UpdateEventArgs e) { MouseMoved?.Invoke(this, e); }
        protected internal virtual void OnDragDropped(UpdateEventArgs e) { DragDropped?.Invoke(this, e); }
        public class UpdateEventArgs : EventArgs
        {
            public UpdateEventArgs() { }
        }

        internal bool ContainsPoint(Vector2 loc)
        {
            if (!Visible) return false;
            if (ClickThrough) return false;
            
            if (loc.X < FootPrint.X) return false;
            if (loc.Y < FootPrint.Y) return false;
            if (loc.X > FootPrint.Right) return false;
            if (loc.Y > FootPrint.Bottom) return false;
            return true;
        }

        internal GuiWidgetBase GetWidgetAtPt(Vector2 vector2)
        {
            foreach (var widget in this.ChildWidgets)
            {
                if (widget.ContainsPoint(vector2))
                    return widget.GetWidgetAtPt(vector2 - new Vector2(widget.FootPrint.X, widget.FootPrint.Y));
            }
            return this;
        }
    }
}
