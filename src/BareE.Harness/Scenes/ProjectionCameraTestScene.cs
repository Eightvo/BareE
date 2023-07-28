using BareE.EZRend.ModelShader.Color;
using BareE.GameDev;
using BareE.Messages;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.Harness.Scenes
{
    internal class ProjectionCameraTestScene:GameDev.GameSceneBase
    {
        ColorShader Colors;
        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            Colors = new ColorShader();
            Colors.SetOutputDescription(Env.LeftEyeBackBuffer.OutputDescription);
            Colors.CreateResources(Env.Window.Device);
            Colors.Update(Env.Window.Device);

            Env.WorldCamera = new BareE.Rendering.ProspectiveCam(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY, Env.Window.Size, 0.1f, 100f, 45f); 
            Env.WorldCamera.Set(new Vector3(0, 0, 4), -Vector3.UnitZ, Vector3.UnitY);
            //Env.WorldCamera.Move(new Vector3(0, 0, 4));
        }
        public override void Initialize(Instant Instant, GameState State, GameEnvironment Env)
        {
            Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(isMouseLook);
            State.Input = InputHandler.Build("System", "Cam","Test");
            base.Initialize(Instant, State, Env);

        }
        bool IgnorePitch = false;
        bool IgnoreYaw = false;
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            UpdateFromControls(Instant, State, Env);
            Colors.Clear();
            AddSquare(new Vector3(0,0,0));
            AddSquare(Env.WorldCamera.Position + Env.WorldCamera.Forward * 3);


            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 0, 0, 0),     new Vector4(0.0f, 0, 0.25f, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(100, 0, 0, 0), new Vector4(0.0f, 0, 0.25f, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(100, 0, 100, 0), new Vector4(0.0f, 0, 0.25f, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 0, 0, 0),     new Vector4(0.0f, 0.25f, 0, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(100, 0, 100, 0), new Vector4(0.0f, 0.25f, 0, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 0, 100, 0),   new Vector4(0.0f, 0.25f, 0, 0.25f)));



            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 0, 0, 0), new Vector4(0.5f, 0, 0, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(100, 100, 0, 0), new Vector4(0.5f, 0, 0, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(100, 0, 0, 0), new Vector4(0.5f, 0, 0, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 0, 0, 0), new Vector4(0.5f, 0, 0, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(100, 0, 0, 0), new Vector4(0.5f, 0, 0, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(100, 100, 0, 0), new Vector4(0.5f, 0, 0, 0.25f)));

            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 0, 0, 0), new Vector4(0.0f, 0, 00.5f, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 100, 100, 0), new Vector4(0.0f, 0, 00.5f, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 0, 100, 0), new Vector4(0.0f, 0, 0.5f, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 0, 0, 0), new Vector4(0.0f, 0, 00.5f, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 0, 100, 0), new Vector4(0.0f, 0, 0.5f, 0.25f)));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(new Vector4(0, 100, 100, 0), new Vector4(0.0f, 0, 00.5f, 0.25f)));



            Colors.Update(Env.Window.Device);

        }

        private void AddSquare(Vector3 pos)
        {
            float hw = 0.5f;
            float hh = 0.5f;
            float hd = 0.5f;
            Vector4 p = new Vector4(pos, 0);
            Vector4 FrontColor = new Vector4(0, 0, 1, 1);
            Vector4 BackColor = new Vector4(0.25f, 0.25f, 0.85f, 1);
            Vector4 RightColor = new Vector4(1, 0, 0, 1);
            Vector4 LeftColor = new Vector4(0.85f, 0.25f, 0.25f, 1f);

            Vector4 TopColor = new Vector4(0, 1, 0, 1);
            Vector4 BottomColor = new Vector4(0.25f, 0.85f, 0.25f, 1f);
            //Front
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(-hw, -hh, hd, 1), FrontColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(hw, hh, hd, 1), FrontColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(hw, -hh, hd, 1), FrontColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(-hw, -hh, hd, 1), FrontColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(-hw, hh, hd, 1), FrontColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(hw, hh, hd, 1), FrontColor));

            //Back
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, -hh, -hd, 1), BackColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, -hh, -hd, 1), BackColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, hh, -hd, 1), BackColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, -hh, -hd, 1), BackColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, hh, -hd, 1), BackColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(-hw, hh, -hd, 1), BackColor));


            //Right
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, -hh, hd, 1), RightColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, hh, hd, 1), RightColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, hh, -hd, 1), RightColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, -hh, hd, 1), RightColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, hh, -hd, 1), RightColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(hw, -hh, -hd, 1), RightColor));


            //Left
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, -hh, hd, 1), LeftColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, hh, -hd, 1), LeftColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, hh, hd, 1), LeftColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, -hh, hd, 1), LeftColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, -hh, -hd, 1), LeftColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(-hw, hh, -hd, 1), LeftColor));

            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, hh, hd, 1), TopColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, hh, -hd, 1), TopColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, hh, -hd, 1), TopColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, hh, hd, 1), TopColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, hh, -hd, 1), TopColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(hw, hh, hd, 1), TopColor));

            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, -hh, hd, 1), BottomColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, -hh, -hd, 1), BottomColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, -hh, -hd, 1), BottomColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(-hw, -hh, hd, 1), BottomColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p+new Vector4(hw, -hh, hd, 1), BottomColor));
            Colors.AddVertex(new EZRend.VertexTypes.Float4_Float4(p + new Vector4(hw, -hh, -hd, 1), BottomColor));
        }

        float moveSpeed = 50;
        bool isMouseLook = true;
        float turnSpeed = 90;
        public void UpdateFromControls(Instant Instant, GameState State, GameEnvironment Env)
        {
            var truckamount = State.Input["Truck"];
            var ttruckamount = truckamount * (Instant.TickDelta / (1000.0f / moveSpeed));
            var dollyamount = State.Input["Dolly"];
            var tdollyamount = dollyamount * -(Instant.TickDelta / (1000.0f / moveSpeed));
            var boomamount = State.Input["Boom"];
            var tboomamount = boomamount * (Instant.TickDelta / (1000.0f / moveSpeed));
            Env.WorldCamera.Move(new Vector3(ttruckamount,
                                 tboomamount,
                                 tdollyamount));

            var zoom = Math.Sign(State.Input.ReadOnce("Zoom"));
            Env.WorldCamera.Zoom(zoom);
            
            
            
            
            Env.WorldCamera.Roll(State.Input["Roll"] * (Instant.TickDelta / (1000.0f / turnSpeed)));
            if (isMouseLook)
            {
                var tpa = Math.Sign(State.Input.ReadOnce("Tilt"));

                var ttpitchamount = tpa * -(Instant.TickDelta / (1000.0f / turnSpeed));
                if (tpa != 0)
                {
                    var pitchamount = tpa;
                    var tpitchamount = ttpitchamount * 1000f;
                }


                var panamount = Math.Sign(State.Input.ReadOnce("Pan"));
                var tpanamount = (panamount) * -(Instant.TickDelta / (1000.0f / turnSpeed));
                if (!IgnorePitch)
                Env.WorldCamera.Pitch(-tpa);
                if (!IgnoreYaw)
                Env.WorldCamera.Yaw(panamount);
            }
            if (State.Input.ReadOnce("Key1")>0)
                IgnorePitch = !IgnorePitch;
            if (State.Input.ReadOnce("Key2") > 0)
                IgnoreYaw = !IgnoreYaw;
            if (State.Input.ReadOnce("Cancel") > 0)
                State.Messages.EmitMsg<ExitGame>(new ExitGame());

            if (State.Input.ReadOnce("CycleMode") > 0)
            {
                isMouseLook = !isMouseLook;
                Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(isMouseLook);
            }
        }
        public override void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {
            cmds.ClearColorTarget(0, RgbaFloat.White);

            Colors.Render(outbuffer, cmds, null, Env.WorldCamera.CamMatrix, Matrix4x4.Identity);
        }
        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            BareE.Rendering.ProspectiveCam cam = (BareE.Rendering.ProspectiveCam)Env.WorldCamera;
            ImGuiNET.ImGui.GetBackgroundDrawList().AddText(new Vector2(0, 0), ImGuiNET.ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)), $"{Env.WorldCamera.Position.X} {Env.WorldCamera.Position.Y} {Env.WorldCamera.Position.Z}");
            ImGuiNET.ImGui.GetBackgroundDrawList().AddText(new Vector2(0, 10), ImGuiNET.ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)), $"Pitch: {cam._Pitch} Yaw: {cam._Yaw} Roll: {cam._Roll}");
            ImGuiNET.ImGui.GetBackgroundDrawList().AddText(new Vector2(0, 20), ImGuiNET.ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)), $"Pitch: {(IgnorePitch ? "No" : "Yes")} Yaw: {(IgnoreYaw ? "No" : "Yes")} ");

            ImGuiNET.ImGui.GetBackgroundDrawList().AddText(new Vector2(0, 40), ImGuiNET.ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)), $"Forward: {Env.WorldCamera.Forward.X.ToString("F2")} {Env.WorldCamera.Forward.Y.ToString("F2")} {Env.WorldCamera.Forward.Z.ToString("F2")} ");
            
            

        }
    }
}
