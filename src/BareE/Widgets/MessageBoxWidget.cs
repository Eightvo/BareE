using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

using IG = ImGuiNET.ImGui;
namespace BareE.Widgets
{
    public struct MessageBoxWidgetOptions
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DialogResult Selections { get; set; }

        public MessageBoxWidgetOptions(String msg) : this(String.Empty, msg, DialogResult.Ok)
        {

        }
        public MessageBoxWidgetOptions(String title, String msg, DialogResult sel)
        {
            Title = title;
            Message = msg;
            Selections = sel;
        }

    }
    public class MessageBoxWidget : IWidget
    {
        static int _id;
        int id = _id++;
        string mangle = $"##MSWPWdgt{_id}";

        public DialogResult Result { get; private set; } = DialogResult.Cancel;

        private bool Closing = false;
        public bool Closed { get; private set; }

        Action<Instant, GameState, GameEnvironment, MessageBoxWidget> Callback;
        MessageBoxWidgetOptions Options;
        public MessageBoxWidget(Action<Instant, GameState, GameEnvironment, MessageBoxWidget> callback, MessageBoxWidgetOptions opts)
        {
            Callback = callback;
            Options = opts;
        }
        public void Render(Instant instant, GameState state, GameEnvironment env, Framebuffer outbuffer, CommandList cmds)
        {
            if (Closing || Closed)
                return;

            IG.Begin($"{Options.Title}{mangle}");
            IG.Text($"{Options.Message}");
            foreach (var v in Enum.GetNames(typeof(DialogResult)))
            {
                var val = (int)Enum.Parse(typeof(DialogResult), v);
                if (((int)Options.Selections & val) == val)
                {
                    if (IG.Button($"{v}{mangle}{val}"))
                    {
                        Result = (DialogResult)val;
                        Closing = true;
                    }
                    IG.SameLine();
                }
            }
            IG.End();
        }
        public void Update(Instant instant, GameState state, GameEnvironment env)
        {
            if (Closing)
            {
                Closing = false;
                Closed = true;
                if (Callback != null)
                    Callback(instant, state, env, this);
            }
        }
    }
}
