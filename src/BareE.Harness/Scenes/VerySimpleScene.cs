using BareE.GameDev;
using BareE.Messages;
using BareE.Rendering;
using BareE.Systems;

using System.Collections.Generic;
using System.Numerics;

using Veldrid;

using IG = ImGuiNET.ImGui;

namespace BareE.Harness
{
    public class VerySimpleScene : GameSceneBase
    {

        FullScreenTexture BGTexture;
       
        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            this.Systems.Enqueue(new ConsoleSystem(), 1);
            this.Systems.Enqueue(new SoundSystem(), 2);
            this.Systems.Enqueue(new MusicSystem("BareE.Harness.Assets.Def.default.radio"), 3);
            BGTexture = new BareE.Rendering.FullScreenTexture();
            BGTexture.CreateResources(Env.Window.Device);
            BGTexture.SetTexture(Env.Window.Device, AssetManager.LoadTexture("BareE.Harness.Assets.Textures.brickVoxelTexture.png", Env.Window.Device));
        }

        private long p = 0;
        public override void Initialize(Instant Instant, GameState State, GameEnvironment Env)
        {
            BGTexture.Update(Env.Window.Device);

            State.ECC.SpawnEntity("MenuBarItems",
                new BareE.Components.MenubarItemSet()
                {
                    SetName = "IDK",
                    Items = new List<BareE.Components.MenubarItem>()
                    {
                        new BareE.Components.MenubarItem()
                        {
                            MenuIdentifierName="File>Open",
                            Callback=(i,s,e)=>{
                                AddWidget(
                                new Widgets.OpenFileDialog.GetFilepathDialog(
                                    (i, s, e, d) =>
                                        {
                                            State.Messages.EmitMsg<ConsoleInput>(new ConsoleInput()
                                            {
                                                    System= false,
                                                    Text=$"From File Open: {d.Result} {d.SelectedFullPath}"
                                            });

                                        }
                                        , new Widgets.OpenFileDialog.GetFilepathDialogSettings()
                                        {
                                             AllowAccessSubdirectories=true,
                                             DefaultExtention=".txt",
                                             RootDirectory=@"G:\TestData"
                                        }

                                    )); ;

                            }
                        },
                        new BareE.Components.MenubarItem()
                        {
                             MenuIdentifierName="File>Quit",
                              Callback=(i,s,e)=>{ State.Messages.EmitMsg(new ExitGame()); }
                        },
                        new BareE.Components.MenubarItem()
                        {
                             MenuIdentifierName="File>Home",
                              Callback=(i,s,e)=>{ State.Messages.EmitMsg(new TransitionScene(){ Scene= new SceneSelectorScene(), State=new GameState()}); }
                        },

                    }.ToArray()
                }
                );

        }
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {

        }

        public override void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {
            BGTexture.Render(outbuffer, cmds, null, Matrix4x4.Identity, Matrix4x4.Identity);
        }

        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {

            IG.Begin("Simple Scene");
            IG.Text("Text");
            IG.Text($"Window: {Env.Window.Size.X} {Env.Window.Size.Y}");
            if (IG.Button("Home"))
            {
                State.Messages.EmitMsg<Messages.TransitionScene>(new Messages.TransitionScene()
                {
                    Preloaded = false,
                    Scene = new SceneSelectorScene(),
                    State = new GameState()
                });
            }
            IG.End();
        }
    }
}