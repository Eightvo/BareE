using BareE.DataStructures;

using BareE.Rendering;

using System;
using System.Numerics;

using Veldrid;
using Veldrid.VirtualReality;

namespace BareE.GameDev
{
    /// <summary>
    /// Represents a distinct unit of Gameplay.
    /// </summary>
    public abstract class GameSceneBase : IDisposable
    {
        public PriorityQueue<GameSystem> Systems = new PriorityQueue<GameSystem>();
        public GameState State { get; set; }
        private FramebufferToScreen leftEyeToScreen;
        private FramebufferToScreen rightEyeToScreen;
        private FramebufferToScreen hudToScreen;
        private IntPtr leftEyePtr;
        private IntPtr rightEyePtr;
        private IntPtr hudPtr;
        private Veldrid.CommandList hudCmds;

        private bool rebuildHud;

        private ISceneDataProvider tranfserSceneData = new DefaultSceneDataProvider();

        internal void DoLoad(Instant Instant, GameState State, GameEnvironment Env)
        {
            Env.Window.Window.Resized += Window_Resized;
            leftEyeToScreen = new FramebufferToScreen();
            rightEyeToScreen = new FramebufferToScreen();
            hudToScreen = new FramebufferToScreen();
            var vrODesc = Env.Window.Device.MainSwapchain.Framebuffer.OutputDescription;
            var SampDesc = new SamplerDescription()
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                Filter = SamplerFilter.Anisotropic,
                MaximumAnisotropy = 4,
                ComparisonKind = ComparisonKind.Always
            };

            if (Env.IsVR)
            {
                //OutputDescription vrODesc = new OutputDescription()
                //{
                //    ColorAttachments = Env.VRSettings.Context.LeftEyeFramebuffer.OutputDescription.ColorAttachments,
                //    DepthAttachment = Env.VRSettings.Context.LeftEyeFramebuffer.OutputDescription.DepthAttachment,
                //     SampleCount= TextureSampleCount.Count4
                //};
                vrODesc = Env.VRSettings.Context.LeftEyeFramebuffer.OutputDescription;
            }

            leftEyeToScreen.SetOutputDescription(vrODesc);
            rightEyeToScreen.SetOutputDescription(vrODesc);
            hudToScreen.SetOutputDescription(vrODesc);
            leftEyeToScreen.SampDesc = SampDesc;
            rightEyeToScreen.SampDesc = SampDesc;
            rightEyeToScreen.SampDesc = SampDesc;

            leftEyeToScreen.CreateResources(Env.Window.Device);
            rightEyeToScreen.CreateResources(Env.Window.Device);
            hudToScreen.CreateResources(Env.Window.Device);
            leftEyeToScreen.SetTexture(Env.Window.Device, Env.LeftEyeBackBuffer.ColorTargets[0].Target);
            rightEyeToScreen.SetTexture(Env.Window.Device, Env.RightEyeBackBuffer.ColorTargets[0].Target);
            hudToScreen.SetTexture(Env.Window.Device, Env.HUDBackBuffer.ColorTargets[0].Target);
            leftEyeToScreen.Update(Env.Window.Device);
            rightEyeToScreen.Update(Env.Window.Device);
            hudToScreen.Update(Env.Window.Device);

            hudCmds = Env.Window.Device.ResourceFactory.CreateCommandList();

            leftEyePtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, Env.LeftEyeBackBuffer.ColorTargets[0].Target);
            rightEyePtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, Env.RightEyeBackBuffer.ColorTargets[0].Target);
            hudPtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, Env.HUDBackBuffer.ColorTargets[0].Target);

            //WinGuiPtr = ImGuiNET.ImGui.CreateContext();
            // ImGuiNET.ImGui.SetCurrentContext(WinGuiPtr);
            // var io = ImGuiNET.ImGui.GetIO();
            //io.Fonts.AddFontDefault();
            // Env.Window.IGR.RecreateFontDeviceTexture(Env.Window.Device);
            //  ImGuiNET.ImGui.NewFrame();

            // HudGuiPtr = ImGuiNET.ImGui.CreateContext();
            // ImGuiNET.ImGui.SetCurrentContext(HudGuiPtr);
            // io = ImGuiNET.ImGui.GetIO();
            // io.Fonts.AddFontDefault();
            // Env.Window.IGR.RecreateFontDeviceTexture(Env.Window.Device);
            //  ImGuiNET.ImGui.NewFrame();
            //  ImGuiNET.ImGui.SetCurrentContext(WinGuiPtr);

            Load(Instant, State, Env);
            foreach (var sys in Systems.Elements)
                sys.Load(Instant, State, Env);
        }

        private void Window_Resized()
        {
            rebuildHud = true;
        }

        internal void DoInitialize(Instant Instant, GameState State, GameEnvironment Env)
        {
            Initialize(Instant, State, Env);
            foreach (var sys in Systems.Elements)
                sys.Initialize(Instant, State, Env);
        }

        internal void DoUpdate(Instant Instant, GameState State, GameEnvironment Env)
        {
            if (rebuildHud)
            {
                rebuildHud = false;
                //hudToScreen.Dispose(); //Todo:Make renderunits disposable

                Env.HUDBackBuffer.Dispose();
                Env.HUDBackBuffer = GameEnvironment.CreateFlatbuffer(Env.Window.Device, (uint)Env.Window.Window.Width, (uint)Env.Window.Window.Height, Env.VRPixelFormat, TextureSampleCount.Count1);

                hudToScreen = new FullscreenTexture.FramebufferToScreen();
                hudToScreen.SetOutputDescription(Env.Window.Device.MainSwapchain.Framebuffer.OutputDescription);
                hudToScreen.CreateResources(Env.Window.Device);
                hudToScreen.SetTexture(Env.Window.Device, Env.HUDBackBuffer.ColorTargets[0].Target);
                Env.Window.IGR = new ImGuiRenderer(Env.Window.Device, Env.HUDBackBuffer.OutputDescription, (int)Env.HUDBackBuffer.Width, (int)Env.HUDBackBuffer.Height);
                // hudToScreen.Update(Env.Window.Device);
            }

            leftEyeToScreen.Update(Env.Window.Device);
            rightEyeToScreen.Update(Env.Window.Device);
            hudToScreen.Update(Env.Window.Device);

            Update(Instant, State, Env);
            foreach (var sys in Systems.Elements)
                sys.Update(Instant, State, Env);
        }

        internal void DoRender(Instant Instant, GameState State, GameEnvironment Env)
        {
            if ((Env.DisplayMode & (DisplayMode.MonitorOnly | DisplayMode.Emulate)) == 0)
            {
                Env.VRSettings.Pose = Env.VRSettings.Context.WaitForPoses();
            }

            // Fence f = Env.Window.Device.ResourceFactory.CreateFence(false);
            var cmds = Env.Window.Cmds;

            cmds.Begin();
            cmds.SetFramebuffer(Env.LeftEyeBackBuffer);
            cmds.ClearDepthStencil(1.0f);
            cmds.ClearColorTarget(0, RgbaFloat.LightGrey);
            //Render World To Left Eye

            Matrix4x4 leftEyeView = Env.WorldCamera.CamMatrix;
            if ((Env.DisplayMode & (DisplayMode.MonitorOnly | DisplayMode.Emulate)) == 0)
            {
                leftEyeView = Env.VRSettings.Pose.CreateView(VREye.Left, Env.WorldCamera.Position, Env.WorldCamera.Forward, Env.WorldCamera.Up);
                leftEyeView = leftEyeView * Env.VRSettings.Pose.LeftEyeProjection;
            }
            this.RenderEye(Instant, State, Env, leftEyeView, Env.LeftEyeBackBuffer, cmds);
            foreach (var sys in Systems.Elements)
                sys.RenderEye(Instant, State, Env, leftEyeView, Env.LeftEyeBackBuffer, cmds);
            cmds.End();
            //Env.Window.Device.SubmitCommands(cmds, f);
            Env.Window.Device.SubmitCommands(cmds);
            //Env.Window.Device.WaitForFence(f);
            //Env.Window.Device.ResetFence(f);

            if (Env.DisplayMode != DisplayMode.MonitorOnly)
            {
                cmds.Begin();
                cmds.SetFramebuffer(Env.RightEyeBackBuffer);
                cmds.ClearColorTarget(0, RgbaFloat.Cyan);
                cmds.ClearDepthStencil(1.0f);
                //Render World To Right Eye
                var rightEyeView = Env.WorldCamera.CamMatrix;
                if ((Env.DisplayMode & (DisplayMode.MonitorOnly | DisplayMode.Emulate)) == 0)
                {
                    rightEyeView = Env.VRSettings.Pose.CreateView(VREye.Right, Env.WorldCamera.Position, Env.WorldCamera.Forward, Env.WorldCamera.Up);
                    rightEyeView = rightEyeView * Env.VRSettings.Pose.RightEyeProjection;
                }
                this.RenderEye(Instant, State, Env, rightEyeView, Env.RightEyeBackBuffer, cmds);
                foreach (var sys in Systems.Elements)
                    sys.RenderEye(Instant, State, Env, rightEyeView, Env.RightEyeBackBuffer, cmds);
                cmds.End();
                Env.Window.Device.SubmitCommands(cmds);
                //Env.Window.Device.SubmitCommands(cmds, f);
                //Env.Window.Device.WaitForFence(f);
                //Env.Window.Device.ResetFence(f);
            }

            //ImGuiNET.ImGui.SetCurrentContext(HudGuiPtr);
            hudCmds.Begin();
            hudCmds.SetFramebuffer(Env.HUDBackBuffer);
            hudCmds.ClearColorTarget(0, RgbaFloat.Clear);

            if (Env.DisplayMode == DisplayMode.Emulate)
            {
                //ImGuiNET.ImGui.ShowMetricsWindow();
                ImGuiNET.ImGui.Begin("View");
                var sz = ImGuiNET.ImGui.GetWindowSize();
                var ps = ImGuiNET.ImGui.GetWindowPos();

                ImGuiNET.ImGui.Image(leftEyePtr, new Vector2((sz.X - 20.0f) / 2.0f, sz.Y - 40), new Vector2(1, 1), new Vector2(0, 0));
                ImGuiNET.ImGui.SameLine();
                ImGuiNET.ImGui.Image(rightEyePtr, new Vector2((sz.X - 20.0f) / 2.0f, sz.Y - 40), new Vector2(1, 1), new Vector2(0, 0));
                ImGuiNET.ImGui.End();
                //ImGuiNET.ImGui.Image(hudPtr, new Vector2(200, 200));
            }

            RenderHud(Instant, State, Env, Env.HUDBackBuffer, hudCmds);
            foreach (var sys in Systems.Elements)
                sys.RenderHud(Instant, State, Env, Env.HUDBackBuffer, hudCmds);

            Env.Window.IGR.Render(Env.Window.Device, hudCmds);
            hudCmds.End();
            Env.Window.Device.SubmitCommands(hudCmds);
            //            Env.Window.Device.SubmitCommands(hudCmds, f);
            //            Env.Window.Device.WaitForFence(f);
            //            Env.Window.Device.ResetFence(f);

            switch (Env.DisplayMode)
            {
                //None: No VR. Only a single 3D Window output.
                case DisplayMode.MonitorOnly:
                    cmds.Begin();
                    cmds.SetFramebuffer(Env.Window.Device.MainSwapchain.Framebuffer);
                    cmds.ClearColorTarget(0, RgbaFloat.Pink);
                    leftEyeToScreen.Render(Env.Window.Device.MainSwapchain.Framebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    hudToScreen.Render(Env.Window.Device.MainSwapchain.Framebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);

                    Env.Window.Cmds.End();
                    Env.Window.Device.SubmitCommands(Env.Window.Cmds);
                    //Env.Window.Device.SubmitCommands(Env.Window.Cmds, f);
                    //Env.Window.Device.WaitForFence(f);
                    //Env.Window.Device.ResetFence(f);

                    Env.Window.Device.SwapBuffers();

                    break;
                //Should Left and Right VR On screen
                case DisplayMode.Emulate:
                    cmds.Begin();
                    cmds.SetFramebuffer(Env.Window.Device.MainSwapchain.Framebuffer);
                    cmds.ClearColorTarget(0, RgbaFloat.Cyan);

                    hudToScreen.Render(Env.Window.Device.MainSwapchain.Framebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);

                    //Env.Window.IGR.Render(Env.Window.Device, Env.Window.Cmds);

                    Env.Window.Cmds.End();
                    Env.Window.Device.SubmitCommands(Env.Window.Cmds);
                    //Env.Window.Device.SubmitCommands(Env.Window.Cmds, f);
                    //Env.Window.Device.WaitForFence(f);
                    //Env.Window.Device.ResetFence(f);

                    Env.Window.Device.SwapBuffers();
                    break;
                //Show mirror texture on screen, Show left and Right buffer in VR.
                case DisplayMode.Mirror:
                    cmds.Begin();
                    cmds.SetFramebuffer(Env.Window.Device.MainSwapchain.Framebuffer);
                    cmds.ClearColorTarget(0, RgbaFloat.Pink);
                    Env.VRSettings.Context.RenderMirrorTexture(cmds, Env.Window.Device.MainSwapchain.Framebuffer, Env.VRSettings.MirrorTexEyeSource);
                    Env.Window.Cmds.End();
                    Env.Window.Device.SubmitCommands(Env.Window.Cmds);
                    //Env.Window.Device.SubmitCommands(Env.Window.Cmds, f);
                    //Env.Window.Device.WaitForFence(f);
                    //Env.Window.Device.ResetFence(f);

                    cmds.Begin();
                    leftEyeToScreen.Render(Env.VRSettings.Context.LeftEyeFramebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    hudToScreen.Render(Env.VRSettings.Context.LeftEyeFramebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    rightEyeToScreen.Render(Env.VRSettings.Context.RightEyeFramebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    hudToScreen.Render(Env.VRSettings.Context.RightEyeFramebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    cmds.End();
                    Env.Window.Device.SubmitCommands(cmds);
                    //Env.Window.Device.SubmitCommands(cmds, f);
                    //Env.Window.Device.WaitForFence(f);
                    //Env.Window.Device.ResetFence(f);
                    Env.Window.Device.SwapBuffers();
                    Env.VRSettings.Context.SubmitFrame();
                    break;
                //Show left and right buffer in VR.
                case DisplayMode.VROnly:
                    cmds.Begin();
                    leftEyeToScreen.Render(Env.VRSettings.Context.LeftEyeFramebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    hudToScreen.Render(Env.VRSettings.Context.LeftEyeFramebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    rightEyeToScreen.Render(Env.VRSettings.Context.RightEyeFramebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    hudToScreen.Render(Env.VRSettings.Context.RightEyeFramebuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    cmds.End();
                    Env.Window.Device.SubmitCommands(cmds);
                    Env.Window.Device.SwapBuffers();

                    Env.VRSettings.Context.SubmitFrame();
                    break;
            }
        }

        public void Dispose()
        {
            foreach (var sys in Systems.Elements)
                sys.Unload();
            Unload();
        }

        public virtual void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
        }

        public virtual void Initialize(Instant Instant, GameState State, GameEnvironment Env)
        {
        }

        public virtual void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
        }

        public virtual void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {
        }

        public virtual void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
        }

        public virtual void Unload()
        {
        }
    }
}