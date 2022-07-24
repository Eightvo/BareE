using BareE.GameDev;
using BareE.Rendering;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using System.Numerics;
using Veldrid.Sdl2;

namespace BareE.GUI
{
    public class BareEGUISystem: GameSystem
    {
        GUIContext Context;

        public BareEGUISystem(System.Drawing.Size Resolution)
        {
            Context = new GUIContext(Resolution);
        }

         
        public void AddWindow(String name, String src)
        {
            var result = BareE.DataStructures.AttributeCollectionDeserializer.FromSrc(src);
            Context.AddWindow(name, result);
            Console.WriteLine(result);
        }

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            Context.Load(Instant, State, Env);
        }

        public void SetResolution(Vector2 vector2)
        {
            Context.SetResolution(vector2);
        }

        //int t = 0;
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            Context.Update(Instant, State, Env);


        }
        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            Context.Render(Instant, State, Env, outbuffer, cmds);
            
        }
        public override bool HandleMouseButtonEvent(SDL_MouseButtonEvent mouseButtonEvent)
        {
            if (Context.HandleMouseButtonEvent(mouseButtonEvent))
                return true;
            return base.HandleMouseButtonEvent(mouseButtonEvent);
        }
    }
}
