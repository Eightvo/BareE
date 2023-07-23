using BareE.GameDev;
using BareE.GUI;
using BareE.GUI.Widgets;
using BareE.Messages;
using BareE.Rendering;
using BareE.Systems;

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Collections.Generic;
using System.Numerics;

using Veldrid;
using Veldrid.Sdl2;

using Vulkan;

using IG = ImGuiNET.ImGui;

namespace BareE.Harness.Scenes
{
    public class EZGUITestScene : GameSceneBase
    {
        ConsoleSystem cSys;
        GUIContext Context;
        FullScreenTexture FST;
        Texture FSTex;

        GUIContext LoadGui(Instant Instant, GameState State, GameEnvironment Env)
        {
            var context = new GUIContext(Env, Env.Window.Resolution);
            StyleDefinition fantasyStyle = CreateFantasyStyle(context, Env);
            StyleDefinition sciFiStyle = CreateSciFiStyle(context, Env);

            context.StyleBook.DefineStyle("Fantasy", fantasyStyle);
            context.StyleBook.DefineStyle("SciFi", sciFiStyle);
            var window = context.CreateWindow(Env.Window.Device, @"Assets\Gui\TestWindow.widget");
           window.Style = "Default";
            context.Widgets.Add(window);
            var window2 = context.CreateWindow(Env.Window.Device, @"Assets\Gui\MessageBox.widget");
            window2.Style = "Fantasy";
            context.Widgets.Add(window2);
            var window3 = context.CreateWindow(Env.Window.Device, @"Assets\Gui\MessageBox.widget");
            window3.Style = "SciFi";
            context.Widgets.Add(window3);
            //            context.Widgets.Add(new Window() { Position = new Vector2(100, 100), Size = new Vector2(300, 300),Style = "Default",Text="Default" });
            //            context.Widgets.Add(new Window() { Position = new Vector2(400, 100), Size = new Vector2(300, 300),Style = "Fantasy",Text="Fantasy" });
            //            context.Widgets.Add(new Window() { Position = new Vector2(400, 400), Size = new Vector2(300, 300), Style = "SciFi",Text="SciFi" });
            return context;
        }
        private static StyleDefinition CreateSciFiStyle(GUIContext context, GameEnvironment Env)
        {
            context.ImportFrame("SciFi", Env.Window.Device, @"BareE.GUI.Assets.Styles.SimpleRPG.SciFiFrameRS.png");
            context.ImportFrame("SciFiSimple", Env.Window.Device, @"BareE.GUI.Assets.Styles.SimpleRPG.SciFiSimpleFrame.png");
            context.ImportImage("SciFiCloseIcon", Env.Window.Device, @"BareE.GUI.Assets.Styles.MinGui.close.png");

            var fantasyStyle = new StyleDefinition();
            fantasyStyle.Add(StyleElement.TitleFrame, "SciFi");
            fantasyStyle.Add(StyleElement.TitleFrameColor, new Vector4(1, 1, 1, 1));
            fantasyStyle.Add(StyleElement.TitleMarginHorizontal, 8);
            fantasyStyle.Add(StyleElement.TitleMarginVertical, 8);
            //fantasyStyle.Add(StyleElement.TitleHeight, 32);

            fantasyStyle.Add(StyleElement.Frame, "SciFiSimple");
            fantasyStyle.Add(StyleElement.FrameColor, new Vector4(1, 1, 1, 1));
            fantasyStyle.Add(StyleElement.MarginHorizontal, 3);
            fantasyStyle.Add(StyleElement.MarginVertical, 3);

            fantasyStyle.Add(StyleElement.Font, "Default");
            fantasyStyle.Add(StyleElement.FontSize, 8);
            fantasyStyle.Add(StyleElement.FontColor, new Vector4(0, 0, 0, 1));
            fantasyStyle.Add(StyleElement.CloseButton_Normal, "SciFiCloseIcon");
            fantasyStyle.Add(StyleElement.CloseButton_Hover, "SciFiCloseIcon");
            fantasyStyle.Add(StyleElement.CloseButton_Down, "SciFiCloseIcon");

            fantasyStyle.Add(StyleElement.CloseButton_NormalColor, new Vector4(1, 0, 1, 1));
            fantasyStyle.Add(StyleElement.CloseButton_HoverColor, new Vector4(1f, 0f, 1f, 1));
            fantasyStyle.Add(StyleElement.CloseButton_DownColor, new Vector4(1f, 0f, 1f, 1));
            return fantasyStyle;
        }

        private static StyleDefinition CreateFantasyStyle(GUIContext context, GameEnvironment Env)
        {
            //context.ImportFont("Roboto", Env.Window.Device, @"C:\AA_Main\Assets\Fonts\ttf\Roboto\Roboto-Regular.ttf", 8, 10, 12, 14, 24);
            context.ImportFont("Cookie", Env.Window.Device, @"C:\AA_Main\Assets\Fonts\ttf\Roboto\Roboto-Regular.ttf", 8, 10, 12, 14, 24);

            context.ImportFrame("Gem", Env.Window.Device, @"BareE.GUI.Assets.Styles.SimpleRPG.GemFrameRS.png");
            context.ImportFrame("Basic", Env.Window.Device, @"BareE.GUI.Assets.Styles.SimpleRPG.BasicFrameRS.png");
            context.ImportImage("FantasyCloseIcon", Env.Window.Device, @"BareE.GUI.Assets.Styles.SimpleRPG.CloseIcon.png");

            
            var fantasyStyle = new StyleDefinition();

           // fantasyStyle.Add(StyleElement.Font, "");
           // fantasyStyle.Add(StyleElement.FontSize, "12");


            fantasyStyle.Add(StyleElement.TitleFrame, "Gem");
            fantasyStyle.Add(StyleElement.TitleFrameColor, new Vector4(1, 1, 1, 1));
            fantasyStyle.Add(StyleElement.TitleMarginHorizontal, 12);
            fantasyStyle.Add(StyleElement.TitleMarginVertical, 12);
            //fantasyStyle.Add(StyleElement.TitleHeight, 32);

            fantasyStyle.Add(StyleElement.Frame, "Basic");
            fantasyStyle.Add(StyleElement.FrameColor, new Vector4(1, 1, 1, 1));
            fantasyStyle.Add(StyleElement.MarginHorizontal, 3);
            fantasyStyle.Add(StyleElement.MarginVertical, 3);

            fantasyStyle.Add(StyleElement.Font, "Cookie");
            fantasyStyle.Add(StyleElement.FontSize, 12);
            fantasyStyle.Add(StyleElement.FontColor, new Vector4(0, 0, 0, 1));
            fantasyStyle.Add(StyleElement.CloseButton_Normal, "FantasyCloseIcon");
            fantasyStyle.Add(StyleElement.CloseButton_Hover, "FantasyCloseIcon");
            fantasyStyle.Add(StyleElement.CloseButton_Down, "FantasyCloseIcon");

            fantasyStyle.Add(StyleElement.CloseButton_NormalColor, new Vector4(1, 1, 1, 1));
            fantasyStyle.Add(StyleElement.CloseButton_HoverColor, new Vector4(1f, 1f, 1f, 1));
            fantasyStyle.Add(StyleElement.CloseButton_DownColor, new Vector4(1f, 1f, 1f, 1));
            return fantasyStyle;
        }

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            cSys = (ConsoleSystem)this.Systems.Enqueue(new ConsoleSystem(), 0);
            cSys.IsShowingMainMenuBar = false;
            Context = LoadGui(Instant, State, Env);


            FST = new FullScreenTexture();
            FST.SetOutputDescription(Env.HUDBackBuffer.OutputDescription);
            FST.CreateResources(Env.Window.Device);
            FSTex = Env.Window.Device.ResourceFactory.CreateTexture(new TextureDescription(){
                 Width=(uint)Context.Canvas.Width,
                 Height=(uint)Context.Canvas.Height,
                 SampleCount = TextureSampleCount.Count1,
                  ArrayLayers=1,
                 Depth=1,
                 MipLevels=1,
                  Format = PixelFormat.R8_G8_B8_A8_UNorm,
                 Type = TextureType.Texture2D,
                 Usage = TextureUsage.Sampled| TextureUsage.RenderTarget

            });
            FST.SetTexture(Env.Window.Device, FSTex);
            FST.Update(Env.Window.Device);
        }
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            Context.Update(Instant, State, Env, ImGuiNET.ImGui.GetIO().MousePos);
        }

        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            Context.Render(Instant, State, Env, cmds);
            cmds.CopyTexture(Context.Canvas,FSTex );
           
            cmds.SetFramebuffer(outbuffer);
            cmds.ClearColorTarget(0, RgbaFloat.White);
            FST.Render(outbuffer, cmds, null, Matrix4x4.Identity, Matrix4x4.Identity);
            ImGuiNET.ImGui.ShowMetricsWindow();
        }
        public override bool HandleMouseWheel(SDL_MouseWheelEvent mouseWheelEvent)
        {
            if (!Context.HandleMouseWheelEvent(mouseWheelEvent))
                return base.HandleMouseWheel(mouseWheelEvent);
            return true;
        }
        public override bool HandleMouseButtonEvent(SDL_MouseButtonEvent mouseButtonEvent)
        {
            if (!Context.HandleMouseButtonEvent(mouseButtonEvent))
                return base.HandleMouseButtonEvent(mouseButtonEvent);
            return true;
        }
        public override void OnResize(Instant instant, GameState state, GameEnvironment env)
        {
            base.OnResize(instant, state, env);
            Context.Resolution = env.Window.Resolution;
            FSTex = env.Window.Device.ResourceFactory.CreateTexture(new TextureDescription()
            {
                Width = (uint)env.Window.Resolution.Width,
                Height = (uint)env.Window.Resolution.Height,
                SampleCount = TextureSampleCount.Count1,
                ArrayLayers = 1,
                Depth = 1,
                MipLevels = 1,
                Format = PixelFormat.R8_G8_B8_A8_UNorm,
                Type = TextureType.Texture2D,
                Usage = TextureUsage.Sampled | TextureUsage.RenderTarget

            });
            FST.SetTexture(env.Window.Device, FSTex);
        }
    }
}
