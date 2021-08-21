using BareE.GameDev;
using BareE.Harness.Scenes;
using BareE.Messages;

using Veldrid;

using IG = ImGuiNET.ImGui;

namespace BareE.Harness
{
    public class SceneSelectorScene : GameSceneBase
    {
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
            IG.End();
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