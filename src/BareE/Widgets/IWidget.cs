using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.Widgets
{
    [Flags]
    public enum DialogResult
    {
        Cancel = 0,
        Abort = 1,
        Ok = 2
    }
    public interface IWidget
    {
        public void Update(Instant instant, GameState state, GameEnvironment env);
        public void Render(Instant instant, GameState state, GameEnvironment env, Framebuffer outbuffer, CommandList cmds);

        public DialogResult Result { get; }
        public bool Closed { get; }
    }
}
