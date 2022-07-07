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
        public virtual string Name { get; protected set; }
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
        public virtual RectangleF FootPrint { get; protected set; }
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
