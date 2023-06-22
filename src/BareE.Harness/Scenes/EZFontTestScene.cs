using BareE.EZRend;
using BareE.GameDev;
using BareE.Messages;
using BareE.Rendering;
using BareE.Systems;

using SixLabors.Fonts;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.Harness.Scenes
{
    internal class EZFontTestScene : GameSceneBase
    {
        BareE.GUI.EZText.EZText eztxt;
        ColoredLineShader lines;
        String RobotoFont;
        String NeutonFont;
        String CookieFont;

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            lines = new ColoredLineShader();
            lines.SetOutputDescription(Env.LeftEyeBackBuffer.OutputDescription);
            lines.CreateResources(Env.Window.Device);

            eztxt = new GUI.EZText.EZText();
            eztxt.SetOutputDescription(Env.LeftEyeBackBuffer.OutputDescription);
            eztxt.CreateResources(Env.Window.Device);

            RobotoFont = eztxt.AddFont(Env.Window.Device, @"C:\AA_Main\Assets\Fonts\ttf\Roboto\Roboto-Regular.ttf",8,10,12,16, 42);
            NeutonFont = eztxt.AddFont(Env.Window.Device, @"C:\AA_Main\Assets\Fonts\ttf\Neuton\Neuton-Regular.ttf",  42);
            CookieFont = eztxt.AddFont(Env.Window.Device, @"C:\AA_Main\Assets\Fonts\ttf\Cookie\Cookie-Regular.ttf", 42);
            eztxt.AddConsoleFont(Env.Window.Device);

            Env.WorldCamera = new OrthographicCamera(Env.Window.Resolution.Width, Env.Window.Resolution.Height, 1000);

            Env.WorldCamera.Move(new Vector3(Env.Window.Resolution.Width / 2.0f, Env.Window.Resolution.Height / 2.0f, 0));
            watch.Start();
        }

        Stopwatch watch = new Stopwatch();
        Random rng = new Random();
        Vector3 cl1;
        Vector3 cl2;
        Vector3 cl3;
        Vector3 cl4;
        Vector3 cl5;
        Vector3 cl6;
        Vector3 cl7;
        int prevSec = -1;
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            eztxt.Clear();
            var sec = (int)Math.Floor(watch.Elapsed.TotalSeconds);
            if (sec != prevSec)
            {
                cl1 = new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
                cl2 = new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
                cl3 = new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
                cl4 = new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
                cl5 = new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
                cl6 = new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
                cl7 = new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            }
            prevSec = sec;
            eztxt.AddString("Console",   8,  new Vector2(00, 0), $"{watch.Elapsed.TotalSeconds}", new Vector3(0,1,1));
            eztxt.AddString(RobotoFont, 16,  new Vector2(0, 20), $"{RobotoFont} - 16pt: the quick brown fox jumped over the lazy dog. THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG?!", cl1);
            eztxt.AddString(RobotoFont, 12,  new Vector2(0, 50), $"{RobotoFont} - 12pt: the quick brown fox jumped over the lazy dog. THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG?!", cl2);
            eztxt.AddString(RobotoFont, 10,  new Vector2(0, 70), $"{RobotoFont} - 10pt: the quick brown fox jumped over the lazy dog. THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG?!", cl3);
            eztxt.AddString(RobotoFont,  8,  new Vector2(0, 90), $"{RobotoFont} - 8pt: the quick brown fox jumped over the lazy dog. THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG?!" , cl4);
            eztxt.AddString(RobotoFont, 42, new Vector2(0, 110), $"{RobotoFont} - 8pt: the quick brown fox jumped over the lazy dog. THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG?!" , cl5);
            eztxt.AddString(NeutonFont, 42, new Vector2(0, 140), $"{NeutonFont} - 8pt: the quick brown fox jumped over the lazy dog. THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG?!" , cl6);
            eztxt.AddString(CookieFont, 42, new Vector2(0, 170), $"{CookieFont} - 8pt: the quick brown fox jumped over the lazy dog. THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG?!" , cl7);

            var v = ImGuiNET.ImGui.GetIO().MousePos;
            v = new Vector2(v.X, Env.LeftEyeBackBuffer.ColorTargets[0].Target.Height-v.Y);

            eztxt.AddString("Console", 8, v, $"{(char)1}Mouse", new Vector3(1, 1, 0));
            eztxt.AddString(RobotoFont, 12, v + new Vector2(0, 20), "Mouse", new Vector3(1, 1, 0));
            eztxt.Update(Env.Window.Device);

            lines.Clear();
            lines.AddPath(new Vector4(1, 0, 0, 1), new Vector2(20, -40), new Vector2(28, -40), new Vector2(28, -34), new Vector2(20, -34), new Vector2(20, -40));
            lines.Update(Env.Window.Device);

        }
        public override void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {
            cmds.ClearColorTarget(0, RgbaFloat.Black);
            cmds.SetFramebuffer(outbuffer);
            cmds.ClearColorTarget(0, RgbaFloat.Black);
            eztxt.Render(outbuffer, cmds, null, eyeMat, Matrix4x4.Identity);

            lines.Render(outbuffer, cmds, null, eyeMat, Matrix4x4.Identity);
        }
        public override void OnResize(Instant Instant, GameState State, GameEnvironment Env)
        {
            Env.WorldCamera = new OrthographicCamera(Env.Window.Resolution.Width, Env.Window.Resolution.Height, 1000);
            Env.WorldCamera.Move(new Vector3(Env.Window.Resolution.Width / 2.0f, Env.Window.Resolution.Height / 2.0f, 0));
        }

    }
}
