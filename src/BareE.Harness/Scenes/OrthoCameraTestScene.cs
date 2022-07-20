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
using BareE.EZRend.VertexTypes;
using BareE.EZRend.Novelty;

namespace BareE.Harness.Scenes
{
    public class OrthoCamTestScene : GameSceneBase 
    {


        ColoredLineShader linesShader;
        VoronoiShader vshade;
        Vector4 Color1=new Vector4(0,1,0,1);
        Vector4 Color2 = new Vector4(0, 0, 1,1);

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {

            Systems.Enqueue(new BareE.Systems.ConsoleSystem(),1);
            Env.WorldCamera = new BareE.Rendering.LookAtQuaternionCamera(new Vector2(800, 600));
            ((LookAtQuaternionCamera)Env.WorldCamera).FarPlane = 1024.0f * 10.0f;
            linesShader = new ColoredLineShader();
            linesShader.SetOutputDescription(Env.LeftEyeBackBuffer.OutputDescription);
            linesShader.CreateResources(Env.Window.Device);

            vshade = new VoronoiShader();
            vshade.SetOutputDescription(Env.LeftEyeBackBuffer.OutputDescription);
            vshade.CreateResources(Env.Window.Device);

        }


        public override void Initialize(Instant Instant, GameState State, GameEnvironment Env)
        {
            Env.WorldCamera = new BareE.Rendering.OrthographicCamera(30,21,1024);
            Env.WorldCamera.Set(new Vector3(0, 0, -10), Vector3.Zero, Vector3.UnitY);
            State.Input = InputHandler.Build("System", "Cam", "Test");
            linesShader.Update(Env.Window.Device);
            vshade.Update(Env.Window.Device);
        }

        bool AB = false;
        bool isMouseLook = false;
        float speed = 10.0f;
        float turnspeed = 8.0f;
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            Env.WorldCamera.Move(new Vector3(State.Input["Truck"] * (Instant.TickDelta / (1000.0f / speed)),
                                             State.Input["Dolly"] * -(Instant.TickDelta / (1000.0f / speed)),
                                              0));
            var z = -State.Input.ReadOnce("Zoom") * 0.20f;
            Env.WorldCamera.Zoom(z);
//            if (isMouseLook)
//            {
//                Env.WorldCamera.Pitch((State.Input.ReadOnce("Tilt")) * -(Instant.TickDelta / (1000.0f / turnspeed)));
//                Env.WorldCamera.Yaw((State.Input.ReadOnce("Pan")) * -(Instant.TickDelta / (1000.0f / turnspeed)));
//            }
            if (State.Input.ReadOnce("Button1") > 0)
                speed *= 2.0f;
            if (State.Input.ReadOnce("Button2") > 0)
                speed *= 0.5f;
            if (State.Input.ReadOnce("CycleMode") > 0)
            {
                isMouseLook = !isMouseLook;
                Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(isMouseLook);
            }
            Vector4 Tint1 = Color1;
            Vector4 Tint2 = Color2;

            linesShader.Clear();
             linesShader.AddVertex(new EZRend.VertexTypes.Float3_Float4(new Vector3(0, 0, -1), Color1));
              linesShader.AddVertex(new EZRend.VertexTypes.Float3_Float4(new Vector3(1, 1, -1), Color1));
              linesShader.AddVertex(new EZRend.VertexTypes.Float3_Float4(new Vector3(0, 0, -1), Color2));
              linesShader.AddVertex(new EZRend.VertexTypes.Float3_Float4(new Vector3(0, 1, -1), Color2));

            linesShader.Update(Env.Window.Device);
            if (State.Input.ReadOnce("Cancel") > 0)
            {
                Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(false);
                State.Messages.EmitMsg(new TransitionScene(new SceneSelectorScene(), new GameState()));
            }

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

           // vshade.Render(outbuffer, cmds, null, eyeMat, Matrix4x4.Identity);
            linesShader.Render(outbuffer, cmds, null, eyeMat, Matrix4x4.Identity);
        }
    }
}
