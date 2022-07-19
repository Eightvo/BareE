using BareE.DataStructures;
using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.GUI.Widgets
{
    public class Panel : GuiWidgetBase
    {
        public override string WidgetType => "Panel";
        public Panel(AttributeCollection def, GUIContext context):base(def, context)
        {

        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env)
        {
            
        }
    }
    public class Border : GuiWidgetBase
    {
        public override string WidgetType => "Border";
        public Border(AttributeCollection def, GUIContext context) : base(def, context)
        {

        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env)
        {
            throw new NotImplementedException();
        }
    }
    public class Frame : GuiWidgetBase
    {
        public override string WidgetType => "Frame";
        public Frame(AttributeCollection def, GUIContext context) : base(def, context)
        {

        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env)
        {
            throw new NotImplementedException();
        }
    }
    public class Window: GuiWidgetBase
    {
        public override string WidgetType => "Window";
        public Window(AttributeCollection def, GUIContext context) : base(def, context)
        {

        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env)
        {
            throw new NotImplementedException();
        }
    }
    public class Text : GuiWidgetBase
    {
        public override string WidgetType => "Text";
        public Text(AttributeCollection def, GUIContext context) : base(def, context)
        {

        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env)
        {
            throw new NotImplementedException();
        }
    }
    public class Button : GuiWidgetBase
    {
        public override string WidgetType => "Button";
        public Button(AttributeCollection def, GUIContext context) : base(def, context)
        {

        }
        public override void Render(Instant Instant, GameState GameState, GameEnvironment Env)
        {
            throw new NotImplementedException();
        }
    }
}
