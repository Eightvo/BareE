using BareE.DataStructures;
using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.GUI.Widgets
{
    public enum AnchorPoint
    {
        None=0,
        Top=1,
        Left=2,
        Right=4,
        Bottom=8,
    }

    
    public abstract class GuiWidgetBase  {
        static private int _id;
        public int ID { get; set; }
        public virtual string Name { get; protected set; }
        public abstract string WidgetType { get; }
        public virtual string Text { get; set; }
        private bool _dirty = true;
        public virtual bool Dirty { get { if (_dirty) return true; foreach (var c in ChildWidgets) if (c.Dirty) return true; return false; } set { _dirty = value; } } 
        public virtual bool Visible { get;  set; } = false;
        public virtual bool Active { get;  set; } = false;
        public virtual bool AllowDrop { get; protected set; } = false;
        public virtual bool ClickThrough { get; protected set; } = false;
        public virtual bool CanFocus { get; protected set; } = false;

        private bool _focused = false;
        public virtual bool ContainsFocus { get { if (_focused) return true; foreach (var c in ChildWidgets) if (c.ContainsFocus) return true; return false; } protected set { _focused = value; } } 

        public virtual AnchorPoint Anchor { get; protected set; } = AnchorPoint.None;
        public virtual Size Size { get; protected set; }
        public virtual Point Position { get; protected set; }
        public virtual RectangleF FootPrint { get { return new RectangleF(Position.X, Position.Y, Size.Width, Size.Height); } }
        public virtual RectangleF Viewport { get; protected set; }
        
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

        public GuiWidgetBase(BareE.DataStructures.AttributeCollection def, GUIContext context)
        {
            _id++;
            ID = _id;
            Name = def.DataAs<String>("Name") ?? $"{WidgetType}{ID}";
            Text = def.DataAs<String>("Text") ?? "";
            Visible = def.DataAs<bool>("Visible");
            Active = def.DataAs<bool>("Active");
            AllowDrop = def.DataAs<bool>("AllowDrop");
            ClickThrough = def.DataAs<bool>("ClickThrough");
            CanFocus = def.DataAs<bool>("CanFocus");
            ContainsFocus = def.DataAs<bool>("ContainsFocus");
            Anchor = def.DataAs<AnchorPoint>("Anchor");
            switch(def["Size"])
            {
                case null: throw new Exception($"Widget {Name} requires a size.");
                case String szStr:
                    {
                        Size sz;
                        if (!StringHelper.TryParseSize(szStr, context.Resolution, out sz))
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
                case string posStr:
                    {
                        Point pos;
                        if (!StringHelper.TryParsePosition(posStr, Size, context.Resolution, out pos)) 
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
                    var childWidget = context.CreateWidget((AttributeCollection)child);
                    Children.Add(childWidget.Name, childWidget);
                }
            }
        }

        public abstract void Render(Instant Instant, GameState GameState, GameEnvironment Env);


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


        protected virtual void OnUpdate(UpdateEventArgs e) { Updated?.Invoke(this, e); }
        protected virtual void OnInitilalized(UpdateEventArgs e) { Initialized?.Invoke(this, e); }
        protected virtual void OnShown(UpdateEventArgs e) { Shown?.Invoke(this, e); }
        protected virtual void OnHidden(UpdateEventArgs e) { Hidden?.Invoke(this, e); }
        protected virtual void OnClosed(UpdateEventArgs e) { Closed?.Invoke(this, e); }
        protected virtual void OnResized(UpdateEventArgs e) { Resized?.Invoke(this, e); }
        protected virtual void OnKeyDown(UpdateEventArgs e) { KeyDown?.Invoke(this, e); }
        protected virtual void OnKeyUp(UpdateEventArgs e) { KeyUp?.Invoke(this, e); }
        protected virtual void OnMouseDownb(UpdateEventArgs e) { MouseDown?.Invoke(this, e); }
        protected virtual void OnMouseUp(UpdateEventArgs e) { MouseUp?.Invoke(this, e); }
        protected virtual void OnMouseMove(UpdateEventArgs e) { MouseMoved?.Invoke(this, e); }
        protected virtual void OnDragDropped(UpdateEventArgs e) { DragDropped?.Invoke(this, e); }
        public class UpdateEventArgs : EventArgs
        {
            public UpdateEventArgs() { }
        }
    }
}
