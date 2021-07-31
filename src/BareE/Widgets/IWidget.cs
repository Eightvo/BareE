using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.Widgets
{
    public interface IWidget
    {
        public void Update(Instant instant, GameState state, GameEnvironment env);
        public void Render(Instant instant, GameState state, GameEnvironment env, Framebuffer outbuffer, CommandList cmds);
        public bool Closed { get; }
    }
}
