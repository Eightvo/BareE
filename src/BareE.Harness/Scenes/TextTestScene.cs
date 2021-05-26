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

namespace BareE.Harness.Scenes
{
    public class TextTestScene:GameSceneBase 
    {
        TextShader text;
        TextSettings textSettings;


        float RowsOfText = 30.0f;
        float blurOutDist;
        Vector2 dropShadow;
        Vector3 glowColor;
        float glowDist;
        float outlineThreshold;
        float scale = 0.1f;

        Vector3 Color1=new Vector3(1,0,0);
        Vector3 Color2 = new Vector3(1, 0, 0);

        String newFontName = String.Empty;

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {

            Systems.Enqueue(new BareE.Systems.ConsoleSystem(),1);
            State.ECC.SpawnEntity(new ConsoleCommandSet()
            {
                SetName = "Text Test",
                Commands = new ConsoleCommand[]
                {
                    new ConsoleCommand()
                    {
                         Cmd="ChangeFont",
                          Callback=new Func<string, GameState, Instant, object[]>(
                              (args, state,inst)=>{
                                  newFontName=args.Trim();
                                  return new object[]{ "Done" };
                              })
                    }
                }
            });
            text = new TextShader(new Vector2(Env.Window.Resolution.Width, Env.Window.Resolution.Height));
            text.CreateResources(Env.Window.Device);
            //text.LoadFont(Env.Window.Device, "BareE.Harness.Assets.Fonts.SDF.Times New Roman.txt");
            //text.LoadFont(Env.Window.Device, @"Assets\Fonts\SDF\A.txt");
            //text.SetTexture(Env.Window.Device, AssetManager.LoadTexture(@"Assets\Fonts\SDF\c.png", Env.Window.Device));
            text.LoadFont(Env.Window.Device, @"Assets\Fonts\SDF\c.fnt");
            textSettings = TextSettings.Default;
            blurOutDist = textSettings.BlurOutDist;
            dropShadow = textSettings.DropShadow;
            glowColor = textSettings.GlowColor;// textSettings.GlowColor;
            glowDist = textSettings.GlowDist;
            outlineThreshold = textSettings.OutlineThreshold;
        }


        public override void Initialize(Instant Instant, GameState State, GameEnvironment Env)
        {
            InputHandler.Build("System");
        }

        bool AB = false;
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {

            if (!String.IsNullOrEmpty(newFontName))
            {
                try
                {
                    text.LoadFont(Env.Window.Device, newFontName);
                }catch(Exception e)
                {
                    Log.EmitError(e);
                }
                newFontName = String.Empty;
               
            }

            textSettings.BlurOutDist = blurOutDist;
            textSettings.DropShadow = dropShadow;
            textSettings.GlowColor = glowColor;
            textSettings.GlowDist = glowDist;
            //textSettings.GlowColor = new Vector4(glowDist, glowColor.X, glowColor.Y, glowColor.Z);
            textSettings.OutlineThreshold = outlineThreshold;

            text.Settings = textSettings;
            text.Clear();
            //for (int i = -1; i < RowsOfText; i++)
            // {
            //     text.AddCharacter('A', ':', Color1, Color2,
            //     new Vector2(0.1f, -1 + ((2.0f / RowsOfText) * i) + (2.0f / RowsOfText) / 2.0f), scale);
            // }




            Vector3 cp = new Vector3(0, 0, 0);
            Matrix4x4 transformMatrix = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(new Vector3(0.0f,0.0f, 1));
            Vector3 Tint1 = Color1;
            Vector3 Tint2 = Color2;

            text.PutCharacter('A', new Vector2(0, 0), 0.1f, new Vector3(1, 0, 0), new Vector3(0, 1, 1));
            //Close Face

            /*
            float lStart = 0;
            float lWidth = 77f;
            float tWidth = 5996;
            float fx1 = lStart / tWidth;
            float fx2 = fx1+ lWidth / tWidth;

            text.AddVertex(new TextVertex(Vector3.Transform(new Vector3(-0.5f, -0.5f, 0.5f), transformMatrix), new Vector2(fx1, 1), Tint1, Tint2));
            text.AddVertex(new TextVertex(Vector3.Transform(new Vector3(0.5f, 0.5f, 0.5f), transformMatrix), new Vector2(fx2, 0), Tint1, Tint2));
            text.AddVertex(new TextVertex(Vector3.Transform(new Vector3(0.5f, -0.5f, 0.5f), transformMatrix), new Vector2(fx2, 1), Tint1, Tint2));

            text.AddVertex(new TextVertex(Vector3.Transform(new Vector3(-0.5f, -0.5f, 0.5f), transformMatrix), new Vector2(fx1, 1), Tint1, Tint2));
            text.AddVertex(new TextVertex(Vector3.Transform(new Vector3(-0.5f, 0.5f, 0.5f), transformMatrix), new Vector2(fx1, 0), Tint1, Tint2));
            text.AddVertex(new TextVertex(Vector3.Transform(new Vector3(0.5f, 0.5f, 0.5f), transformMatrix), new Vector2(fx2, 0), Tint1, Tint2));
            */


            text.Update(Env.Window.Device);

            if (State.Input.ReadOnce("Cancel") > 0)
                State.Messages.EmitMsg(new SceneSelectorScene());

        }
        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            
            IG.Begin("Text Settings");
            IG.InputFloat("bluroutdist", ref blurOutDist);
            IG.InputFloat2("Drop Shadow", ref dropShadow);
            IG.InputFloat("glowdist", ref glowDist);
            IG.InputFloat("outlinethreshold", ref outlineThreshold);
            //IG.ColorPicker3("Glow Color", ref glowColor);
            IG.ColorEdit3("Glow Color", ref glowColor);
            IG.End();

            IG.Begin("Text");
            IG.DragFloat("Scale", ref scale);
            IG.ColorEdit3("Color1", ref Color1);
            IG.ColorEdit3("Color2", ref Color2);
            
            IG.End();
            
        }

        public override void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {
            text.Render(outbuffer, cmds, null, Matrix4x4.Identity, Matrix4x4.Identity);
        }
    }
}
