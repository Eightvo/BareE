using BareE.DataStructures;
using BareE.GameDev;
using BareE.Rendering;
using BareE.Widgets;

using FFmpeg.AutoGen;

using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Veldrid;
using Veldrid.Sdl2;

using Vulkan;

using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace BareE.GUI.Widgets
{
    public class ControlDroppedEventArgs : EventArgs
    {
        public WidgetBase droppedControl;
        public ControlDroppedEventArgs(WidgetBase cb)
        {
            droppedControl = cb;
        }
    }
    public class KeyEventArgs : EventArgs
    {
        public Key Code;

        public bool Alt;

        public bool Control;

        public bool Shift;

        public bool Down;
        public bool Repeat;

        public KeyEventArgs(KeyEvent e)
        {
            Code = e.Key;
            Alt = (e.Modifiers & ModifierKeys.Alt) != 0;
            Control = (e.Modifiers & ModifierKeys.Control) != 0;
            Shift = (e.Modifiers & ModifierKeys.Shift) != 0;
            Down = e.Down;
            Repeat = e.Repeat;
        }

        public override string ToString()
        {
            return string.Concat("[KeyEventArgs] Code(", Code, ") Alt(", Alt, ") Control(", Control, ") Shift(", Shift, ") Down(", Down, ")", ") Repeat(", Repeat, ")");
        }
    }
    public class MouseButtonEventArgs : EventArgs
    {
        public SDL_MouseButton Button;

        public int X;

        public int Y;
        public MouseButtonEventArgs() { }
        public MouseButtonEventArgs(SDL_MouseButtonEvent e)
        {

            Button = e.button;
            X = e.x;
            Y = e.y;
        }

        public override string ToString()
        {
            return string.Concat("[MouseButtonEventArgs] Button(", Button, ") X(", X, ") Y(", Y, ")");
        }
    }


    public abstract class WidgetBase
    {
        private static int _ID;
        public int ID = ++_ID;

        #region events
        public event EventHandler<Entity> AttachedEntityChanged;
        public event EventHandler<MouseButtonEventArgs> MouseClicked;
        public event EventHandler Hidden;
        public event EventHandler Shown;
        public event EventHandler Focused;
        public event EventHandler LostFocus;
        public event EventHandler<KeyEventArgs> KeyPressed;
        public event EventHandler<SDL_MouseWheelEvent> MouseWheelMoved;
        public event EventHandler<SDL_MouseButtonEvent> MouseButtonEvent;
        public event EventHandler<SDL_MouseMotionEvent> MouseMoved;
        public event EventHandler MouseEntered;
        public event EventHandler MouseExited;
        public event EventHandler<ControlDroppedEventArgs> MouseItemDropped;
        public event EventHandler<Object> ECSUpdated;
        public event EventHandler ControlAdded;

        public virtual void OnAttachedEntityChanged(Entity ent) { if (this.AttachedEntityChanged!=null) this?.AttachedEntityChanged(this,ent); }
        public virtual void OnMouseClicked(MouseButtonEventArgs args) {if (this.MouseClicked!=null) this?.MouseClicked(this, args); }
        public virtual void OnHidden() { if (this.Hidden != null) this?.Hidden(this, null); }
        public virtual void OnShown() { if (this.Shown!=null) this?.Shown(this, null); }
        public virtual void OnFocused() { if (this.Focused!=null) this.Focused(this, null); }
        public virtual void OnLostFocus() { if (this.LostFocus!=null) this?.LostFocus(this, null); }
        public virtual void OnKeyPressed(KeyEventArgs args) { if (this.KeyPressed != null)  this?.KeyPressed(this, args); }
        public virtual void OnMouseWheelMoved(SDL_MouseWheelEvent args) {
            if (this.MouseWheelMoved!=null) this.MouseWheelMoved(this, args);
            else
            {
                if (Children == null) return;
                foreach(var child in Children)
                    child.OnMouseWheelMoved(args);
            }
        }
        public virtual void OnMouseButtonEvent(SDL_MouseButtonEvent args) {
            if (this.MouseButtonEvent!=null) this.MouseButtonEvent(this, args);
            else
            {
                if (Children == null) return;
                foreach(var child in Children)
                {
                    child.OnMouseButtonEvent(args);
                }
                    
            }
        }
        public virtual void OnMouseMoved(SDL_MouseMotionEvent args) {
           if (this.MouseMoved!=null) this.MouseMoved(this, args);
            else
            {
                if (Children == null) return;
                foreach(var child in Children)
                    child.OnMouseMoved(args);
            }
        }
               
        public virtual void OnMouseEntered() {if (this.MouseEntered!=null) this?.MouseEntered(this, null); }
        public virtual void OnMouseExited() {if (this.MouseExited!=null) this?.MouseExited(this, null); }
        public virtual void OnMouseItemDropped(ControlDroppedEventArgs args) { if (this.MouseItemDropped != null) this?.MouseItemDropped(this, args); }
        public virtual void OnECSUpdated(Object args) {if(this.ECSUpdated!=null) this?.ECSUpdated(this, args); }
                
        public virtual void OnControlAdded() {if (this.ControlAdded!=null) this?.ControlAdded(this, null); }
        
        #endregion
        public virtual Vector2 Position { get; set; }
        public virtual Vector2 Size { get; set; }
        public String Text;
        public virtual bool Visible { get; set; } = true;
        public String Style;

        private List<WidgetBase> _Children=new List<WidgetBase>();
        public IEnumerable<WidgetBase> Children { get { return _Children.ToList(); } }
        protected StyleDefinition CustomStyle { get; set; }=new StyleDefinition();
        public WidgetBase Parent;

        private bool _dirty=true;
        public virtual bool Dirty
        {
            get
            {
                if (_dirty) return true;
                if (Children == null) return false;
                foreach (var child in Children)
                {
                    if (child.Dirty) return true;
                }
                return false;
            }
            set { _dirty = value; }
        }
        private bool _styleDirty=true;

        public virtual bool StyleDirty { get { return _styleDirty; } private set { if (value) Dirty = true;_styleDirty = value; } } 
        public int ZIndex;
        public virtual Rectangle Render(GUIContext renderTo, Rectangle contentRegion, Vector2 offset) { foreach (var child in Children) child.Render(renderTo, contentRegion, offset); return contentRegion; }
        public virtual Rectangle DynamicRender(GUIContext renderTo, Rectangle contentRegion, Vector2 offset) 
        {
            if (Children == null)
                return contentRegion;
            foreach (var child in Children)
                child.DynamicRender(renderTo, contentRegion, offset);
            return contentRegion; 
        }
        public virtual void ReadStyle(GUIContext renderTo)
        {
            StyleDirty = false;
            if (Children == null) return;
            foreach(var child in Children)
                child.ReadStyle(renderTo);
        }
        public virtual void ReadAttributes(AttributeCollection def)
        {
            Style = (String)(def["Style"]);
            Position = (Vector2)(def["Position"] ?? new Vector2(0, 0));
            Size = (Vector2)(def["Size"] ?? new Vector2(0, 0));
            Text = (String)(def["Text"] ?? String.Empty);
            Visible = (bool)(def["Visible"] ?? true);


            foreach(string t in Enum.GetNames<StyleElement>())
            {
                if (def.HasAttribute(t))
                    CustomStyle.Add(t, def[t]);
            }

        }

        protected object ResolveStyle(GUIContext context, StyleElement element)
        {
            return ResolveStyle(context, element.ToString());
        }
        protected object ResolveStyle(GUIContext context, String element)
        {
            object ret;
            if (CustomStyle.ContainsKey(element)) return CustomStyle[element];
            if (String.IsNullOrEmpty(Style))
            {
                if (Parent != null)
                    ret = Parent.ResolveStyle(context, element);
                else ret = null;
            }
            else
                ret = context.StyleBook[Style, element];
            return ret ?? context.StyleBook["Default", element];
        }

        public void AddChild(WidgetBase child)
        {
            child.Parent = this;
            _Children.Add(child);
            _dirty = true;
        }

    }

}
