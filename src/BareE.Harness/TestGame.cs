
using BareE.Components;
using BareE.DataStructures;
using BareE.GameDev;
using BareE.Messages;

using BareE.EZRend;
using BareE.EZRend.ModelShader.Color;
using BareE.EZRend.ImageShader.FullscreenTexture;

using System;
using System.Collections.Generic;
using System.Numerics;

using IG = ImGuiNET.ImGui;
using Veldrid;

using Matrix4x4 = System.Numerics.Matrix4x4;
using BareE.Rendering;

namespace BareE.Harness
{
    public class TestGame : GameDev.Game
    {
        public TestGame(GameState initialState, GameEnvironment env) : base(new TestGameScene(),initialState, env)
        {
            InputHandler.LoadFromConfig("BareE.Harness.Assets.Def.default.controls");
        }
    }

    public class TestGameScene : GameDev.GameSceneBase
    {
        EZRend.ModelShader.Color.ColorNormalShader clrNrml;
        ISceneDataProvider LightData = new DefaultSceneDataProvider();

        Dictionary<String, Texture> _loadedTextures = new Dictionary<string, Texture>();

        bool isMouseLook = false;
        float speed = 10.0f;
        float turnspeed = 8.0f;
        float modelTurnSpeed = 0.0f;
        float modelSpeed = 0.0f;
        RenderDoc rd;
        Entity cat;

        String pText = "Pause";

        float ali;
        Vector3 alc;
        Vector3 dlp = new Vector3(0, 0, 0);

        ambientLightData ambientLD = new ambientLightData();
        CommonData comDat = new CommonData();

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            State.ECC.SpawnEntity(new ConsoleCommandSet()
            {
                SetName = "TestGame",
                Commands = new ConsoleCommand[] {
                new ConsoleCommand(){
                    Callback = new Func<string, GameState, Instant, object[]>((a, s, i) =>
                    {
                        Env.WorldCamera.Set(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
                        return new object[] { "Done" };
                    }),
                    Cmd = "ResetPos",
                    HelpText = "Put Camera back at origin."
                    },
                }

            });
            this.Systems.Push(new BareE.Systems.ConsoleSystem(), 1);
            this.Systems.Push(new BareE.Systems.SoundSystem(), 2);
            this.Systems.Push(new BareE.Systems.MusicSystem("BareE.Harness.Assets.Def.default.radio"), 3);
            State.Messages.AddMsg<ConsoleInput>(new ConsoleInput() { System = true, Text = "BOOT" });

            Env.WorldCamera.LockUp = true;

            clrNrml = new ColorNormalShader();
            var d = Env.GetBackbufferOutputDescription();
            clrNrml.SetOutputDescription(d);
            var clr = new Vector4(0, 1, 1, 1);

            clrNrml.CreateResources(Env.Window.Device);

            State.Input = InputHandler.Build("System", "Cam", "Test");
            cat = State.ECC.SpawnEntity("cat",
                Pos.Create(2f, new Vector3(-20, 0, -15)),
                BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/cat.obj", @"Assets/Models/cat_diff.png"));

            /*

                        State.ECC.SpawnEntity("adventurer",
                            Pos.Create(0.5f,new Vector3(0,0,-15)),
                            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/BlockyCharacters/Models/advancedCharacter.fbx", @"Assets/Models/BlockyCharacters/Skins/Advanced/skin_adventurer.png")); 


                        State.ECC.SpawnEntity("orc",
                            Pos.Create(0.5f, new Vector3(20, 0, -15)),
                            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/BlockyCharacters/Models/advancedCharacter.fbx", @"Assets/Models/BlockyCharacters/Skins/Advanced/skin_orc.png"));

                        State.ECC.SpawnEntity("man",
                            Pos.Create(0.5f, new Vector3(40, 0, -15)),
                            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/BlockyCharacters/Models/advancedCharacter.fbx", @"Assets/Models/BlockyCharacters/Skins/Advanced/skin_man.png"));

                        State.ECC.SpawnEntity("manalt",
                            Pos.Create(0.5f, new Vector3(60, 0, -15)),
                            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/BlockyCharacters/Models/advancedCharacter.fbx", @"Assets/Models/BlockyCharacters/Skins/Advanced/skin_manAlternative.png"));
                        State.ECC.SpawnEntity("robot",
                            Pos.Create(0.5f, new Vector3(80, 0, -15)),
                            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/BlockyCharacters/Models/advancedCharacter.fbx", @"Assets/Models/BlockyCharacters/Skins/Advanced/skin_robot.png"));
                        State.ECC.SpawnEntity("soldier",
                            Pos.Create(0.5f, new Vector3(100, 0, -15)),
                            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/BlockyCharacters/Models/advancedCharacter.fbx", @"Assets/Models/BlockyCharacters/Skins/Advanced/skin_soldier.png"));
                        State.ECC.SpawnEntity("woman",
                            Pos.Create(0.5f, new Vector3(120, 0, -15)),
                            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/BlockyCharacters/Models/advancedCharacter.fbx", @"Assets/Models/BlockyCharacters/Skins/Advanced/skin_woman.png"));
                        State.ECC.SpawnEntity("womanalt",
                            Pos.Create(0.5f, new Vector3(140, 0, -15)),
                            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/BlockyCharacters/Models/advancedCharacter.fbx", @"Assets/Models/BlockyCharacters/Skins/Advanced/skin_womanAlternative.png"));
                        State.ECC.SpawnEntity("Ship",
                            Pos.Create(50f, new Vector3(0, 0, -300)),
                            BareE.Harness.Components.EZModel.CreateStaticColored(@"Assets/Models/watercraftPack_003.obj", new Dictionary<Vector3, Vector3>()));

                        State.ECC.SpawnEntity("Barrel",
                        Pos.Create(0.025f, new Vector3(-20, 0, 000)),
                        BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/wood_barrels/big_wood_barrel.obj",
                         @"/Assets/Models/wood_barrels/big_diffus.png", @"/Assets/Models/wood_barrels/big_normal.png", 
                         @"/Assets/Models/wood_barrels/big_specular.png")
                        );
            */
            //State.ECC.SpawnEntity("StatueAD",
            //Pos.Create(0.025f, new Vector3(00, 0, 000)),
            //BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/Statue/Statue_With_Lamp_LP.obj",
            //    @"/Assets/Models/statue/Statue_dDo03_d.png"
            //));



            State.ECC.SpawnEntity("StatueADN",
            Pos.Create(0.025f, new Vector3(00, 0, 000)),
            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/Statue/Statue_With_Lamp_LP.obj",
              @"/Assets/Models/statue/Statue_dDo03_d.png",
              @"/Assets/Models/statue/Statue_dDo03_n.png"
            ));


            //            State.ECC.SpawnEntity("StatueADNS",
            //            Pos.Create(0.025f, new Vector3(20, 0, 000)),
            //            BareE.Harness.Components.EZModel.CreateTextured(@"Assets/Models/Statue/Statue_With_Lamp_LP.obj",
            //              @"/Assets/Models/statue/Statue_dDo03_d.png",
            //              @"/Assets/Models/statue/Statue_dDo03_n.png",
            //              @"/Assets/Models/statue/Statue_dDo03_s.png"              
            //            ));


            State.ECC.SpawnEntity("StatueADNSE1",
                Pos.Create(0.025f, new Vector3(0, -10, 0)),
                BareE.Harness.Components.EZModel.CreateTextured(
                    @"Assets/Models/Statue/Statue_With_Lamp_LP.obj",
                    @"/Assets/Models/statue/Statue_dDo03_d.png",
                    @"/Assets/Models/statue/Statue_dDo03_n.png",
                    @"/Assets/Models/statue/Statue_dDo03_s.png",
                    @"/Assets/Models/statue/Emissive_Map.png"
                ));

            State.ECC.SpawnEntity("StatueADNSE2",
                Pos.Create(0.025f, new Vector3(0, 10, 0)),
                BareE.Harness.Components.EZModel.CreateTextured(
                  @"Assets/Models/Statue/Statue_With_Lamp_LP.obj",
                  @"/Assets/Models/statue/Statue_dDo03_d.png",
                  @"/Assets/Models/statue/Statue_dDo03_n.png",
                  @"/Assets/Models/statue/Statue_dDo03_s.png",
                  @"/Assets/Models/statue/Emissive_Map.png"
                ));


            State.ECC.SpawnEntity("StatueADNSE3",
            Pos.Create(0.025f, new Vector3(0, 0, -10)),
            BareE.Harness.Components.EZModel.CreateTextured(
              @"Assets/Models/Statue/Statue_With_Lamp_LP.obj",
              @"/Assets/Models/statue/Statue_dDo03_d.png",
              @"/Assets/Models/statue/Statue_dDo03_n.png",
              @"/Assets/Models/statue/Statue_dDo03_s.png",
              @"/Assets/Models/statue/Emissive_Map.png"
            ));

            State.ECC.SpawnEntity("StatueADNSE4",
                Pos.Create(0.025f, new Vector3(0, 0, 10)),
                BareE.Harness.Components.EZModel.CreateTextured(
                    @"Assets/Models/Statue/Statue_With_Lamp_LP.obj",
                    @"/Assets/Models/statue/Statue_dDo03_d.png",
                    @"/Assets/Models/statue/Statue_dDo03_n.png",
                    @"/Assets/Models/statue/Statue_dDo03_s.png",
                    @"/Assets/Models/statue/Emissive_Map.png"
                ));

            State.ECC.SpawnEntity("StatueADNSE5",
                Pos.Create(0.025f, new Vector3(-10, 0, 000)),
                BareE.Harness.Components.EZModel.CreateTextured(
                  @"Assets/Models/Statue/Statue_With_Lamp_LP.obj",
                  @"/Assets/Models/statue/Statue_dDo03_d.png",
                  @"/Assets/Models/statue/Statue_dDo03_n.png",
                  @"/Assets/Models/statue/Statue_dDo03_s.png",
                  @"/Assets/Models/statue/Emissive_Map.png"
                ));

            State.ECC.SpawnEntity("StatueADNSE6",
                Pos.Create(0.025f, new Vector3(10, 0, 000)),
                BareE.Harness.Components.EZModel.CreateTextured(
                  @"Assets/Models/Statue/Statue_With_Lamp_LP.obj",
                  @"/Assets/Models/statue/Statue_dDo03_d.png",
                  @"/Assets/Models/statue/Statue_dDo03_n.png",
                  @"/Assets/Models/statue/Statue_dDo03_s.png",
                  @"/Assets/Models/statue/Emissive_Map.png"
                ));
        }

        private Texture LoadTexture(string v, GraphicsDevice device)
        {
            if (!_loadedTextures.ContainsKey(v))
                _loadedTextures.Add(v, AssetManager.LoadTexture(v, device));
            return _loadedTextures[v];
        }


        public void UpdateFromControls(Instant Instant, GameState State, GameEnvironment Env)
        {
            Env.WorldCamera.Move(new Vector3(State.Input["Truck"] * (Instant.TickDelta / (1000.0f / speed)),
                                 0.0f,
                                 State.Input["Dolly"] * -(Instant.TickDelta / (1000.0f / speed))));
            if (isMouseLook)
            {
                Env.WorldCamera.Pitch((State.Input.ReadOnce("Tilt")) * -(Instant.TickDelta / (1000.0f / turnspeed)));
                Env.WorldCamera.Yaw((State.Input.ReadOnce("Pan")) * -(Instant.TickDelta / (1000.0f / turnspeed)));
            }
            if (State.Input.ReadOnce("Cancel") > 0)
                State.Messages.AddMsg<ExitGame>(new ExitGame());

            if (State.Input.ReadOnce("CycleMode") > 0)
            {
                isMouseLook = !isMouseLook;
                Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(isMouseLook);
            }
        }
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            UpdateFromControls(Instant, State, Env);

            clrNrml.Update(Env.Window.Device);
            foreach (var ecp in State.ECC.Components.GetEntitiesByComponentType<Components.EZModel>())
            {
                Components.EZModel v = ecp.Value;
                Entity ent = State.ECC.Entities[ecp.Key];
                if (v.Model == null)
                {
                    if (!String.IsNullOrEmpty(v.Skin))
                    {
                        if (v.HasNormalMap)
                        {

                            List<Texture> textures = new List<Texture>();
                            textures.Add(LoadTexture(v.Skin, Env.Window.Device));

                            if (!String.IsNullOrEmpty(v.NormalMap))
                                textures.Add(LoadTexture(v.NormalMap, Env.Window.Device));

                            if (!String.IsNullOrEmpty(v.SpecularMap))
                                textures.Add(LoadTexture(v.SpecularMap, Env.Window.Device));

                            if (!String.IsNullOrEmpty(v.EmmissiveMap))
                                textures.Add(LoadTexture(v.EmmissiveMap, Env.Window.Device));


                            if (v.isStatic)
                            {
                                v.Model = ModelHelper.LoadStaticTexturedBumpMesh(v.Root, ModelHelper.DefaultSteps,
                                                                       Env.GetBackbufferOutputDescription(),
                                                                       Env.Window.Device,
                                                                       textures.ToArray());
                            }
                            else
                            {
                                v.Model = ModelHelper.LoadTexturedBumpMesh(v.Root, ModelHelper.DefaultSteps,
                                                                       Env.GetBackbufferOutputDescription(),
                                                                       Env.Window.Device,
                                                                       textures.ToArray());

                            }

                        }
                        else
                        {
                            if (v.isStatic)
                            {
                                v.Model = ModelHelper.LoadStaticTexturedMesh(v.Root, ModelHelper.DefaultSteps,
                                                                       Env.GetBackbufferOutputDescription(),
                                                                       Env.Window.Device,
                                                                       LoadTexture(v.Skin, Env.Window.Device));

                            }
                            else
                            {

                                v.Model = ModelHelper.LoadTexturedMesh(v.Root, ModelHelper.DefaultSteps,
                                                                       Env.GetBackbufferOutputDescription(),
                                                                       Env.Window.Device,
                                                                       LoadTexture(v.Skin, Env.Window.Device));
                            }
                        }
                    }
                    else
                    {
                        if (v.isStatic)
                        {
                            v.Model = ModelHelper.LoadStaticColoredMesh(v.Root, ModelHelper.DefaultSteps, Env.GetBackbufferOutputDescription(),
                                                                Env.Window.Device, v.ClrMap);
                        }
                        else
                        {
                            v.Model = ModelHelper.LoadColoredMesh(v.Root, ModelHelper.DefaultSteps, Env.GetBackbufferOutputDescription(),
                                                                Env.Window.Device, v.ClrMap);
                        }
                    }

                }


                var pos = State.ECC.Components.GetComponent<Pos>(ent);
                pos.RotationMatrix = pos.RotationMatrix * Matrix4x4.CreateFromYawPitchRoll(1 / (1000 / modelTurnSpeed), 0, 0);
                pos.TranslationMatrix = pos.TranslationMatrix * Matrix4x4.CreateTranslation(Vector3.Normalize(pos.Forward) * (1 / (1000 / modelSpeed)));
                State.ECC.Components.SetComponent(ent, pos);
                foreach (var m in v.Model.Meshes.Values)
                    m.Update(Env.Window.Device);
            }

        }
        public override void RenderEye(Instant Instant, GameState State, GameEnvironment env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {
            cmds.ClearColorTarget(0, RgbaFloat.Black);

            foreach (var ecp in State.ECC.Components.GetEntitiesByComponentType<Components.EZModel>())
            {
                Components.EZModel v = ecp.Value;
                if (v.Model == null)
                    continue;
                Entity Ent = State.ECC.Entities[ecp.Key];
                var posComponent = State.ECC.Components.GetComponent<Pos>(Ent);
                foreach (var mesh in v.Model.Meshes.Values)
                    mesh.Render(outbuffer, cmds, LightData, eyeMat, posComponent.Scale * posComponent.RotationMatrix * posComponent.TranslationMatrix);
            }

            clrNrml.Render(outbuffer, cmds, LightData, eyeMat, Matrix4x4.Identity);
        }


        public override void RenderHud(Instant Instant, GameState State, GameEnvironment env, Framebuffer outbuffer, CommandList cmds)
        {
            ImGuiNET.ImGui.ShowMetricsWindow();

            ImGuiNET.ImGui.Begin("Lights");
            IG.InputFloat("Ambient Light Intensity", ref ali);
            IG.InputFloat3("Diffuse Light Position", ref dlp);
            IG.ColorPicker3("Ambient Light Color", ref alc);
            ImGuiNET.ImGui.End();
            ambientLD.ali = ali;
            ambientLD.alc = alc;
            ambientLD.dlp = dlp;
            LightData.AmbientLight = ambientLD;

            comDat.u_resolution = new Vector2(env.Window.Resolution.Width, env.Window.Resolution.Height);
            comDat.u_mouse = ImGuiNET.ImGui.GetIO().MousePos;
            comDat.time = ((int)Instant.SessionDuration % 1000);
            comDat.u_campos = env.WorldCamera.Position;
            comDat.flags = 0;

            LightData.CommonData = comDat;


            var p = State.ECC.Components.GetComponent<Pos>(cat);
            p.Position = dlp;
            State.ECC.Components.SetComponent(cat, p);

            ImGuiNET.ImGui.Begin("Window");
            ImGuiNET.ImGui.Text($"Game is {(Instant.IsPaused ? "" : "not ")}paused.");
            ImGuiNET.ImGui.Text($"Mouse Look is {(isMouseLook ? "" : "not ")}on. (Tab to toggle)");
            ImGuiNET.ImGui.Text($"Session Time: {Instant.SessionDuration}");
            ImGuiNET.ImGui.Text($"Effective Time: {Instant.EffectiveDuration}");
            ImGuiNET.ImGui.Text($"Turn: {Instant.Turn}");
            ImGuiNET.ImGui.Text($"");
            ImGuiNET.ImGui.Text($"CamP: {env.WorldCamera.Position.X},{env.WorldCamera.Position.Y},{env.WorldCamera.Position.Z}");
            ImGuiNET.ImGui.Text($"CamF: {env.WorldCamera.Forward.X},{env.WorldCamera.Forward.Y},{env.WorldCamera.Forward.Z}");
            ImGuiNET.ImGui.Text($"CamU: {env.WorldCamera.Up.X},{env.WorldCamera.Up.Y},{env.WorldCamera.Up.Z}");
            ImGuiNET.ImGui.Text($"");

            if (ImGuiNET.ImGui.Button(pText))
            {
                if (State.Clock.IsPaused)
                {
                    pText = "Pause";
                    State.Clock.Unpause();
                }
                else
                {
                    pText = "Unpause";
                    State.Clock.Pause();
                }
            }
            ImGuiNET.ImGui.InputFloat("Speed:", ref speed);
            ImGuiNET.ImGui.InputFloat("TurnSpeed:", ref turnspeed);
            ImGuiNET.ImGui.InputFloat("Model Speed:", ref modelSpeed);
            ImGuiNET.ImGui.InputFloat("Model Turn Speed:", ref modelTurnSpeed);

            if (ImGuiNET.ImGui.Button("Next Turn"))
            {
                State.Clock.AdvanceTurn();
            }

            if (ImGuiNET.ImGui.Button("Send Immediate message"))
            {
                State.Messages.AddMsg<ConsoleInput>(new ConsoleInput() { Text = "Immediately sent message" });
            }
            if (ImGuiNET.ImGui.Button("Send message in 5 seconds"))
            {
                State.Messages.AddMsg(new ConsoleInput($"Message Emitted at SD:{Instant.SessionDuration}"));
                State.Messages.EmitRealTimeDelayedMessage(5000, new ConsoleInput() { Text = "5 sec realtime delay" }, Instant);
            }
            if (ImGuiNET.ImGui.Button("Send message in 5 unpaused seconds"))
            {
                State.Messages.AddMsg(new ConsoleInput() { Text = $"Message Emitted at ED:{Instant.EffectiveDuration}" });
                State.Messages.EmitEffectiveTimeDelayedMessage(5000, new ConsoleInput() { Text = "5 sec effective delay" }, Instant);
            }
            if (ImGuiNET.ImGui.Button("Send message in 5 turns"))
            {
                State.Messages.AddMsg(new ConsoleInput($"Message Dispatched At Turn: {Instant.Turn}"));
                State.Messages.EmitTurnDelayedMessage(5, new ConsoleInput() { Text = "5 turn delay" }, Instant);
            }

            if (ImGuiNET.ImGui.Button("Press [Esc] to Quit."))
            {
                State.Messages.AddMsg(new ExitGame());
            }

            if (ImGuiNET.ImGui.Button("Cylce Mode"))
            {
                State.Messages.AddMsg(new ConsoleInput("CycleMode"));
            }
            if (IG.Button("To that other scene"))
            {
                State.Messages.AddMsg<Messages.TransitionScene>(new Messages.TransitionScene()
                {
                    Preloaded = false,
                    Scene = new VerySimpleScene(),
                    State = new GameState()
                });
            }
            ImGuiNET.ImGui.End();
        }
    }

}
