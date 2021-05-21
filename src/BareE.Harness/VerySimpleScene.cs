using BareE.GameDev;
using BareE.Messages;
using BareE.Systems;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

using IG = ImGuiNET.ImGui;
namespace BareE.Harness
{
    public class VerySimpleScene : GameSceneBase
    {
        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            this.Systems.Push(new ConsoleSystem(), 1);
            this.Systems.Push(new SoundSystem(), 2);
            this.Systems.Push(new MusicSystem("BareE.Harness.Assets.Def.default.radio"), 3);
        }

        long p = 0;
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            //if (Instant.EffectiveDuration-p>1000)
            //{
            //    p = Instant.EffectiveDuration;
                State.Messages.AddMsg<ConsoleInput>(new ConsoleInput($"{p++}"));
            //}
            
        }
        public override void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {
            
        }
        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            IG.Begin("Simple Scene");
            IG.Text("Text");
            if (IG.Button("Home"))
            {
                State.Messages.AddMsg<Messages.TransitionScene>(new Messages.TransitionScene()
                {
                    Preloaded = false,
                    Scene=new SceneSelectorScene(),
                     State=new GameState()
                }) ;
            }
            IG.End();
        }
    }
}
