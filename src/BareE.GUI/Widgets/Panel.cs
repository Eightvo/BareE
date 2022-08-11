using BareE.DataStructures;
using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Rectangle = SixLabors.ImageSharp.Rectangle;
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

    public class Text : GuiWidgetBase
    {
        public override string WidgetType => "Text";
        public override bool ClickThrough { get; protected set; } = true;
        public Text(AttributeCollection def, GUIContext context, GuiWidgetBase parent) : base(def, context, parent)
        {
            ClickThrough = true;
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
