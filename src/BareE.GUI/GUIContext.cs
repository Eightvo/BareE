using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using BareE.GUI.Widgets;
using BareE.GameDev;

namespace BareE.GUI
{
    public class GUIContext
    {
        public StyleSet Styles { get; set; }
        Dictionary<String, GuiWidgetBase> Widgets { get; set; }

        public Bitmap Canvas;
        private Graphics CGraphics;
        public GUIContext(Size size)
        {
            Canvas = new Bitmap(size.Width, size.Height);
            CGraphics = Graphics.FromImage(Canvas);
        }

        public void Render(Instant Instant, GameState State, GameEnvironment Env)
        {
            foreach(var widget in Widgets.Values.OrderBy(x=>x.ZIndex))
            {
                widget.Render(Instant, State, Env);
            }
        }

        ~GUIContext()
        {
            CGraphics.Dispose();
            Canvas.Dispose();
        }

    }
}
