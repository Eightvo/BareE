using BareE.GameDev;
using BareE.Harness.Scenes;
using BareE.Messages;

using System;
using System.Collections.Generic;

using Veldrid;

using IG = ImGuiNET.ImGui;

namespace BareE.Harness
{
    public class SceneSelectorScene : GameSceneBase
    {

        List<Exception> _exceptions = new List<Exception>();
        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            State.Messages.AddListener<EmitException>(CollectExceptions);
            base.Load(Instant, State, Env);
        }

        private bool CollectExceptions(EmitException msg, GameState state, Instant instant)
        {
            _exceptions.Add(msg.Exception);
            return true;
        }

        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            IG.Begin("Scenes");
            if (IG.Button("Very Simple Scene"))
            {
                State.Messages.EmitMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new VerySimpleScene(),
                    State = new GameState()
                });
            }
            if (IG.Button("Lighting Test Scene"))
            {
                State.Messages.EmitMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new LightingTestScene(),
                    State = new GameState()
                });
            }
            if (IG.Button("Transvoxel Test Scene"))
            {
                State.Messages.EmitMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new TransvoxelScene(),
                    State = new GameState()
                });
            }
            if (IG.Button("SDF Test Scene"))
            {
                State.Messages.EmitMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new TextTestScene(),
                    State = new GameState()
                });
            }
            if (IG.Button("AdvSpriteBatch Test Scene"))
            {
                State.Messages.EmitMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new AdvSpriteBatchTestScene(),
                    State = new GameState()
                });
            }

            if (IG.Button("Line Shader Test"))
            {
                State.Messages.EmitMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new LineShaderTestScene(),
                    State = new GameState()
                });
            }

            if (IG.Button("Ortho camera Test"))
            {
                State.Messages.EmitMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new OrthoCamTestScene(),
                    State = new GameState()
                });
            }
            

            if (IG.Button("EZ Text Test"))
            {
                State.Messages.EmitMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new EZFontTestScene(),
                    State = new GameState()
                });
            }
            if (IG.Button("EZ GUI Test"))
            {
                State.Messages.EmitMsg<TransitionScene>(new TransitionScene()
                {
                    Preloaded = false,
                    Scene = new EZGUITestScene(),
                    State = new GameState()
                });
            }
            IG.End();

            if (_exceptions.Count>0)
            {
                IG.Begin("Exceptions");
                foreach(var v in _exceptions)
                    IG.Text(v.Message);
                if (IG.Button("Clear"))
                    _exceptions.Clear();
                IG.End();
            }


        }

        void RenderSceneButton<T>(string text)
            where T:GameSceneBase,new()
        {
            if (IG.Button(text))
            {
            }

        }

    }
}