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

namespace BareE.Harness.Scenes
{
    public class AdvSpriteBatchTestScene : GameSceneBase 
    {

        SpriteAtlas actorAtlas;
        AdvSpriteBatchShader actorSpriteBatch;


        AdvSpriteBatchShader actorSpriteBatch2;

        Framebuffer offscreenBuffer;

        private IntPtr offScreenColorTexturePtr;

        Vector4 Color1=new Vector4(1,0,0,1);
        Vector4 Color2 = new Vector4(1, 0, 0,1);

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {

            Systems.Enqueue(new BareE.Systems.ConsoleSystem(),1);

            actorAtlas = new SpriteAtlas();
            var txt = AssetManager.ReadFile(@"BareE.EZRend.Flat.AdvSpriteBatch.LPCChar.atlas");
            var actorModel = Newtonsoft.Json.JsonConvert.DeserializeObject<SpriteAtlas.SpriteModel>(txt);
            actorAtlas.Merge("Ship", actorModel, "BareE.EZRend.Flat.AdvSpriteBatch.prefabchar_rc.png");
            actorAtlas.Build(0, false);

            offscreenBuffer = GameEnvironment.CreateFlatbuffer(Env.Window.Device, (uint)Env.Window.Window.Width, (uint)Env.Window.Window.Height, PixelFormat.R8_G8_B8_A8_UNorm, TextureSampleCount.Count1);

            offScreenColorTexturePtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, offscreenBuffer.ColorTargets[0].Target);

            actorSpriteBatch = new AdvSpriteBatchShader();
            actorSpriteBatch.SetOutputDescription(Env.GetBackbufferOutputDescription());
            actorSpriteBatch.CreateResources(Env.Window.Device);
            actorSpriteBatch.SetTexture(0, Env.Window.Device, AssetManager.LoadTexture(actorAtlas.AtlasSheet, Env.Window.Device,false,false));
            foreach (var v in BareE.GeometryFactory.QuadVerts())
                actorSpriteBatch.AddVertex(new Float3_Float2(v, new Vector2(v.X < 0 ? 0 : 1, v.Y < 0 ? 1 : 0)));

            actorSpriteBatch2 = new AdvSpriteBatchShader();
            actorSpriteBatch2.SetOutputDescription(offscreenBuffer.OutputDescription);
            actorSpriteBatch2.CreateResources(Env.Window.Device);
            actorSpriteBatch2.SetTexture(0, Env.Window.Device, AssetManager.LoadTexture(actorAtlas.AtlasSheet, Env.Window.Device, false, false));
            foreach (var v in BareE.GeometryFactory.QuadVerts())
                actorSpriteBatch2.AddVertex(new Float3_Float2(v, new Vector2(v.X < 0 ? 0 : 1, v.Y < 0 ? 1 : 0)));

        }


        public override void Initialize(Instant Instant, GameState State, GameEnvironment Env)
        {
            InputHandler.Build("System");
            actorSpriteBatch.Update(Env.Window.Device);
            actorSpriteBatch2.Update(Env.Window.Device);
        }

        bool AB = false;
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {

            Vector4 Tint1 = Color1;
            Vector4 Tint2 = Color2;

            actorSpriteBatch.ClearInstanceData();
            actorSpriteBatch.AddSprite(actorAtlas[$"Ship.South.Cast.0"], new Vector2(0, 0), 0, Color1, Color2, 0.5f);
            actorSpriteBatch.Update(Env.Window.Device);


            actorSpriteBatch2.ClearInstanceData();
            actorSpriteBatch2.AddSprite(actorAtlas[$"Ship.South.Cast.0"], new Vector2(0, 0), 0, Color1, Color2, 0.5f);
            actorSpriteBatch2.Update(Env.Window.Device);


            if (State.Input.ReadOnce("Cancel") > 0)
                State.Messages.EmitMsg(new SceneSelectorScene());

        }
        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {

            IG.Begin("Colors");
            IG.ColorEdit4("Color1", ref Color1);
            IG.ColorEdit4("Color2", ref Color2);

            ImGuiNET.ImGui.Image(offScreenColorTexturePtr, new Vector2(offscreenBuffer.Width / 1.0f, offscreenBuffer.Height / 1.0f), new Vector2(0, 1), new Vector2(1, 0));
            IG.End();
            
        }

        public override void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {

            actorSpriteBatch.Render(outbuffer, cmds, null, eyeMat, Matrix4x4.Identity);
            actorSpriteBatch2.Render(offscreenBuffer, cmds, null, eyeMat, Matrix4x4.Identity);
        }
    }
}
