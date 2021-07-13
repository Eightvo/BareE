using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BareE;
using BareE.GameDev;
using BareE.Messages;
using BareE.GUI;

using Veldrid;

using IG = ImGuiNET.ImGui;
using BareE.GUI.TextRendering;
using BareE.Components;
using BareE.DataStructures;
using BareE.EZRend.Flat;
using BareE.EZRend.ModelShader.Color;
using BareE.Rendering;
using BareE.EZRend;

namespace BareE.Harness.Scenes
{
    public class LineShaderTestScene : GameSceneBase 
    {


        ColoredLineShader linesShader;

        Vector4 Color1=new Vector4(1,0,0,1);
        Vector4 Color2 = new Vector4(1, 0, 0,1);

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {

            Systems.Enqueue(new BareE.Systems.ConsoleSystem(),1);


            linesShader = new ColoredLineShader();
            linesShader.SetOutputDescription(Env.GetBackbufferOutputDescription());
            linesShader.CreateResources(Env.Window.Device);

        }


        public override void Initialize(Instant Instant, GameState State, GameEnvironment Env)
        {
            InputHandler.Build("System");
            linesShader.Update(Env.Window.Device);
        }

        bool AB = false;
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {

            Vector4 Tint1 = Color1;
            Vector4 Tint2 = Color2;

            linesShader.Clear();
            linesShader.AddVertex(new EZRend.VertexTypes.Float3_Float4(new Vector3(0, 0, -1), Color1));
            linesShader.AddVertex(new EZRend.VertexTypes.Float3_Float4(new Vector3(1, 1, -1), Color1));
            linesShader.AddVertex(new EZRend.VertexTypes.Float3_Float4(new Vector3(0, 0, -1), Color2));
            linesShader.AddVertex(new EZRend.VertexTypes.Float3_Float4(new Vector3(0, 1, -1), Color2));
            linesShader.Update(Env.Window.Device);


            if (State.Input.ReadOnce("Cancel") > 0)
                State.Messages.EmitMsg(new SceneSelectorScene());

        }
        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {

            IG.Begin("Colors");
            IG.ColorEdit4("Color1", ref Color1);
            IG.ColorEdit4("Color2", ref Color2);
            IG.End();
            
        }

        public override void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {

            linesShader.Render(outbuffer, cmds, null, eyeMat, Matrix4x4.Identity);
        }
    }
}
