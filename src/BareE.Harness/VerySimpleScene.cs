﻿using BareE.GameDev;
using BareE.Messages;
using BareE.Systems;

using System.Numerics;

using Veldrid;

using IG = ImGuiNET.ImGui;

namespace BareE.Harness
{
    public class VerySimpleScene : GameSceneBase
    {
        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            this.Systems.Enqueue(new ConsoleSystem(), 1);
            this.Systems.Enqueue(new SoundSystem(), 2);
            this.Systems.Enqueue(new MusicSystem("BareE.Harness.Assets.Def.default.radio"), 3);
        }

        private long p = 0;

        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            State.Messages.EmitMsg<ConsoleInput>(new ConsoleInput($"{p++}"));
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
                State.Messages.EmitMsg<Messages.TransitionScene>(new Messages.TransitionScene()
                {
                    Preloaded = false,
                    Scene = new SceneSelectorScene(),
                    State = new GameState()
                });
            }
            IG.End();
        }
    }
}