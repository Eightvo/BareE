using BareE.GameDev;
using BareE.Messages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

using IG = ImGuiNET.ImGui;

namespace BareE.Harness
{
    public class SceneSelectorScene:GameSceneBase
    {
        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            IG.Begin("Scenes");
            if (IG.Button("Very Simple Scene"))
            {
                State.Messages.AddMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new VerySimpleScene(),
                    State = new GameState()
                });
            }
            if (IG.Button("Lighting Test Scene"))
            {
                State.Messages.AddMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new TestGameScene(),
                    State = new GameState()
                });
            }
            IG.End();
        }
    }
}
