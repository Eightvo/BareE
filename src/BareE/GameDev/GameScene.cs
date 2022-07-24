using BareE.DataStructures;

using BareE.Rendering;
using BareE.Widgets;

using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

using Veldrid;
using Veldrid.VirtualReality;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using BareE.UTIL;
using Veldrid.Sdl2;

namespace BareE.GameDev
{
    /// <summary>
    /// Represents a distinct unit of Gameplay.
    /// </summary>
    public abstract class GameSceneBase : IDisposable
    {
        public PriorityQueue<GameSystem> Systems = new PriorityQueue<GameSystem>();
        List<IWidget> _widgets = new List<IWidget>();
        public void AddWidget(IWidget widget)
        {
            _widgets.Add(widget);
        }
        public GameState State { get; set; }
        private FullScreenTexture leftEyeToScreen;
        private FullScreenTexture rightEyeToScreen;
        private FullScreenTexture hudToScreen;
        private FullScreenTexture ScreenToMonitor;
        
        private IntPtr leftEyePtr;
        private IntPtr rightEyePtr;
        private IntPtr hudPtr;
        private Veldrid.CommandList hudCmds;

        private bool rebuildHud;

        private ISceneDataProvider tranfserSceneData = new DefaultSceneDataProvider();
        private Texture resolvedTexture;

        internal void DoLoad(Instant Instant, GameState State, GameEnvironment Env)
        {
            resolvedTexture = Env.Window.Device.ResourceFactory.CreateTexture(
                new TextureDescription((uint)Env.Window.Width, (uint)Env.Window.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D)
            );
            resolvedTexture.Name = "Resolved Texture";
            ScreenToMonitor = new FullScreenTexture();
            ScreenToMonitor.SetOutputDescription(Env.Window.Device.MainSwapchain.Framebuffer.OutputDescription);
            ScreenToMonitor.CreateResources(Env.Window.Device);
            ScreenToMonitor.SetTexture(Env.Window.Device, resolvedTexture);
            

            Env.Window.Window.Resized += Window_Resized;
            leftEyeToScreen = new FullScreenTexture();
            rightEyeToScreen = new FullScreenTexture();
            hudToScreen = new FullScreenTexture();
            
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

            leftEyeToScreen.SetOutputDescription(Env.ScreenBackBuffer.OutputDescription);
            rightEyeToScreen.SetOutputDescription(Env.ScreenBackBuffer.OutputDescription);
            hudToScreen.SetOutputDescription(Env.ScreenBackBuffer.OutputDescription);
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


            leftEyeToScreen.SetTexture(Env.Window.Device, Env.LeftEyeBackBuffer.ColorTargets[0].Target);
            rightEyeToScreen.SetTexture(Env.Window.Device, Env.RightEyeBackBuffer.ColorTargets[0].Target);
            hudToScreen.SetTexture(Env.Window.Device, Env.HUDBackBuffer.ColorTargets[0].Target);

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
            ScreenToMonitor.Update(Env.Window.Device);
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
                resolvedTexture.Dispose();
                resolvedTexture = Env.Window.Device.ResourceFactory.CreateTexture(
                    new TextureDescription((uint)Env.Window.Width, (uint)Env.Window.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D)
                );
                resolvedTexture.Name = "ResolvedTexture";
                ScreenToMonitor.SetTexture(Env.Window.Device, resolvedTexture);

                Env.HUDBackBuffer.Dispose();
                Env.HUDBackBuffer = GameEnvironment.CreateFlatbuffer(Env.Window.Device, (uint)Env.Window.Window.Width, (uint)Env.Window.Window.Height, PixelFormat.R8_G8_B8_A8_UNorm, TextureSampleCount.Count1);
                Env.HUDBackBuffer.Name = "Hud Backbuffer";

                Env.ScreenBackBuffer.Dispose();
                Env.ScreenBackBuffer = Util.CreateFramebuffer(Env.Window.Device, Env.Window.Device.ResourceFactory.CreateTexture(new TextureDescription((uint)Env.Window.Width,
                                           (uint)Env.Window.Height,
                                           1, 1, 1,
                                           PixelFormat.R8_G8_B8_A8_UNorm,
                                           TextureUsage.RenderTarget | TextureUsage.Sampled,
                                           TextureType.Texture2D,
                                           Env.PrefferedTextureCount)));
                Env.ScreenBackBuffer.Name = "Screen Backbuffer";
                Env.ScreenBackBuffer.ColorTargets[0].Target.Name = "Screen Backbuffer tex";

                hudToScreen = new FullScreenTexture();
                hudToScreen.SetOutputDescription(Env.ScreenBackBuffer.OutputDescription);
                hudToScreen.CreateResources(Env.Window.Device);
                hudToScreen.SetTexture(Env.Window.Device, Env.HUDBackBuffer.ColorTargets[0].Target);
                Env.Window.IGR = new ImGuiRenderer(Env.Window.Device, Env.HUDBackBuffer.OutputDescription, (int)Env.HUDBackBuffer.Width, (int)Env.HUDBackBuffer.Height);
                DoOnHudRefresh(Instant, State, Env);
                // hudToScreen.Update(Env.Window.Device);
                leftEyePtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, Env.LeftEyeBackBuffer.ColorTargets[0].Target);
                rightEyePtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, Env.RightEyeBackBuffer.ColorTargets[0].Target);
                hudPtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, Env.HUDBackBuffer.ColorTargets[0].Target);

            }

            leftEyeToScreen.Update(Env.Window.Device);
            rightEyeToScreen.Update(Env.Window.Device);
            hudToScreen.Update(Env.Window.Device);

            Update(Instant, State, Env);
            foreach (var sys in Systems.Elements)
                sys.Update(Instant, State, Env);
            List<IWidget> toBeRemoved=new List<IWidget>();
            foreach (var wid in _widgets)
            {
                wid.Update(Instant, State, Env);
                if (wid.Closed) toBeRemoved.Add(wid);
            }
            foreach (var v in toBeRemoved)
                _widgets.Remove(v);

        }

        internal void DoRender(Instant Instant, GameState State, GameEnvironment Env)
        {

            if ((Env.DisplayMode & (DisplayMode.MonitorOnly | DisplayMode.Emulate)) == 0)
            {
                Env.VRSettings.Pose = Env.VRSettings.Context.WaitForPoses();
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
            foreach (var wid in _widgets)
                wid.Render(Instant, State, Env, Env.HUDBackBuffer, hudCmds);
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
                    cmds.Begin(); //Env.Window.Device.MainSwapchain.Framebuffer
                    cmds.SetFramebuffer(Env.ScreenBackBuffer);
                    cmds.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
                    leftEyeToScreen.Render(Env.ScreenBackBuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);
                    hudToScreen.Render(Env.ScreenBackBuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);

                    Env.Window.Cmds.End();
                    Env.Window.Device.SubmitCommands(Env.Window.Cmds);

                    Env.Window.Cmds.Begin();
                    //cmds.GenerateMipmaps(Env.ScreenBackBuffer.ColorTargets[0].Target);
                    if (Env.ScreenBackBuffer.ColorTargets[0].Target.SampleCount == TextureSampleCount.Count1)
                        cmds.CopyTexture(Env.ScreenBackBuffer.ColorTargets[0].Target, resolvedTexture);
                    else
                        cmds.ResolveTexture(Env.ScreenBackBuffer.ColorTargets[0].Target, resolvedTexture);
                    Env.Window.Cmds.End();
                    Env.Window.Device.SubmitCommands(Env.Window.Cmds);
                    Env.Window.Cmds.Begin();
                    ScreenToMonitor.Render(Env.Window.Device.MainSwapchain.Framebuffer, cmds, null, Matrix4x4.Identity, Matrix4x4.Identity);
                    Env.Window.Cmds.End();
                    Env.Window.Device.SubmitCommands(Env.Window.Cmds);



                    if (false)
                    {
                        //this._screenshotQueued = false;
                        int i = 0;
                        foreach (var t in new List<Texture>() { Env.ScreenBackBuffer.ColorTargets[0].Target, Env.HUDBackBuffer.ColorTargets[0].Target, Env.LeftEyeBackBuffer.ColorTargets[0].Target, Env.RightEyeBackBuffer.ColorTargets[0].Target, resolvedTexture })
                        {
                            i++;
                            TextureDescription desc = TextureDescription.Texture2D(
                                t.Width,
                                t.Height,
                                t.MipLevels,
                                t.ArrayLayers,
                                t.Format,
                                TextureUsage.Staging,
                                t.SampleCount

                            );

                            Texture? tex = Env.Window.Device.ResourceFactory.CreateTexture(desc);

                            Env.Window.Cmds.Begin();
                            Env.Window.Cmds.CopyTexture(t, tex);
                            Env.Window.Cmds.End();
                            Env.Window.Device.SubmitCommands(Env.Window.Cmds);

                            MappedResource mapped = Env.Window.Device.Map(tex, MapMode.Read);

                            byte[] bytes = new byte[mapped.SizeInBytes];
                            Marshal.Copy(mapped.Data, bytes, 0, (int)mapped.SizeInBytes);

                            Image img = Image.LoadPixelData<Rgba32>(bytes, (int)tex.Width, (int)tex.Height);
                            Env.Window.Device.Unmap(tex);

                            img = img.CloneAs<Rgb24>();

                            if (!Env.Window.Device.IsUvOriginTopLeft)
                            {
                                img.Mutate(x => x.Flip(FlipMode.Vertical));
                            }
                            img.SaveAsBmp($"C:\\TestData\\SS{i}.bmp");
                            //this.InvokeScreenshotTaken(img);

                            tex.Dispose();
                        }
                    }

                    Env.Window.Device.SwapBuffers();

                    break;
                //Should Left and Right VR On screen
                case DisplayMode.Emulate:
                    cmds.Begin();
                    cmds.SetFramebuffer(Env.ScreenBackBuffer);
                    cmds.ClearColorTarget(0, RgbaFloat.Cyan);

                    hudToScreen.Render(Env.ScreenBackBuffer, cmds, tranfserSceneData, Matrix4x4.Identity, Matrix4x4.Identity);

                    //Env.Window.IGR.Render(Env.Window.Device, Env.Window.Cmds);

                    Env.Window.Cmds.End();
                    Env.Window.Device.SubmitCommands(Env.Window.Cmds);
                    //Env.Window.Device.SubmitCommands(Env.Window.Cmds, f);
                    //Env.Window.Device.WaitForFence(f);
                    //Env.Window.Device.ResetFence(f);

                    Env.Window.Cmds.Begin();
                    //cmds.GenerateMipmaps(Env.ScreenBackBuffer.ColorTargets[0].Target);
                    if (Env.ScreenBackBuffer.ColorTargets[0].Target.SampleCount == TextureSampleCount.Count1)
                        cmds.CopyTexture(Env.ScreenBackBuffer.ColorTargets[0].Target, resolvedTexture);
                    else
                        cmds.ResolveTexture(Env.ScreenBackBuffer.ColorTargets[0].Target, resolvedTexture);
                    Env.Window.Cmds.End();
                    Env.Window.Device.SubmitCommands(Env.Window.Cmds);
                    Env.Window.Cmds.Begin();
                    ScreenToMonitor.Render(Env.Window.Device.MainSwapchain.Framebuffer, cmds, null, Matrix4x4.Identity, Matrix4x4.Identity);
                    Env.Window.Cmds.End();
                    Env.Window.Device.SubmitCommands(Env.Window.Cmds);


                    Env.Window.Device.SwapBuffers();
                    break;
                //Show mirror texture on screen, Show left and Right buffer in VR.
                case DisplayMode.Mirror:
                    cmds.Begin();
                    cmds.SetFramebuffer(Env.ScreenBackBuffer);
                    cmds.ClearColorTarget(0, RgbaFloat.Pink);
                    Env.VRSettings.Context.RenderMirrorTexture(cmds, Env.ScreenBackBuffer, Env.VRSettings.MirrorTexEyeSource);
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
        public virtual void DoOnHudRefresh(Instant instant, GameState state, GameEnvironment env)
        {
            OnHudRefresh(instant, state, env);
            foreach(var sys in Systems.Elements)
            {
                
                sys.OnHudRefresh(instant, state, env);
            }
        }
        public virtual void OnHudRefresh(Instant instant, GameState State, GameEnvironment Env)
        {

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


        public virtual bool HandleControllerAxisMotion(SDL_ControllerAxisEvent axisEvent) {
            foreach (var sys in Systems.Elements)
                if (sys.HandleControllerAxisMotion(axisEvent))
                    return true;
            return false; 
        }
        public virtual bool HandleControllerButtonEvent(SDL_ControllerButtonEvent axisEvent) {
            foreach (var sys in Systems.Elements)
                if (sys.HandleControllerButtonEvent(axisEvent))
                    return true;
            return false; 
        }
        public virtual bool HandleKeyboardEvent(SDL_KeyboardEvent keyboardEvent) {
            foreach (var sys in Systems.Elements)
                if (sys.HandleKeyboardEvent(keyboardEvent))
                    return true;
            return false; 
        }
        public virtual bool HandleMouseButtonEvent(SDL_MouseButtonEvent mouseButtonEvent) {
            foreach (var sys in Systems.Elements)
                if (sys.HandleMouseButtonEvent(mouseButtonEvent))
                    return true;
            return false; 
        }
        public virtual bool HandleMouseMotion(SDL_MouseMotionEvent mouseMotionEvent) {
            foreach (var sys in Systems.Elements)
                if (sys.HandleMouseMotion(mouseMotionEvent))
                    return true;
            return false; 
        }
        public virtual bool HandleMouseWheel(SDL_MouseWheelEvent mouseWheelEvent) {
            foreach (var sys in Systems.Elements)
                if (sys.HandleMouseWheel(mouseWheelEvent))
                    return true;
            return false; 
        }

    }
}