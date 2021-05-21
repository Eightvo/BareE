using BareE.EZRend.ModelShader.Color;
using BareE.GameDev;
using BareE.Messages;
using BareE.Rendering;
using BareE.Transvoxel;

using System.Numerics;

using Veldrid;

using IG = ImGuiNET.ImGui;

namespace BareE.Harness
{
    public class TransvoxelScene : GameSceneBase
    {
        private ISceneDataProvider SceneData = new DefaultSceneDataProvider();
        private Vector3 Anchor { get; set; } = Vector3.Zero;
        private CloudView<SurfaceInfo> viewer = new CloudView<SurfaceInfo>();
        private CustomPointProvider<SurfaceInfo> provider = new CustomPointProvider<SurfaceInfo>();
        private TriplanarPBump colorShader;
        private bool isMouseLook = false;
        private float speed = 10.0f;
        private float turnspeed = 8.0f;

        private bool isGrass = true;
        private Texture GrassDirt;
        private Texture Brick;

        private Texture CurrentTexture(GraphicsDevice device)
        {
            if (isGrass)
                return AssetManager.LoadTexture("BareE.Harness.Assets.Textures.GrassDirtVoxelTexture.png", device);
            else return AssetManager.LoadTexture("BareE.Harness.Assets.Textures.brickVoxelTexture.png", device);
        }

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            var Cloud = new PointCloud<SurfaceInfo>();
            provider = new CustomPointProvider<SurfaceInfo>();
            for (int i = -10; i <= 10; i++)
                for (int j = -1; j <= 1; j++)
                    for (int k = -10; k < 10; k++)
                        provider.AddPoint(new Vector3(i, j, k), new SurfaceInfo() { V = -1 });
            Cloud.AddProvider(provider);
            Cloud.AddProvider(new SpherePointProvider<SurfaceInfo>(new Vector3(28, 28, 28), 20, 10, new SurfaceInfo() { V = -1 }));
            viewer = new CloudView<SurfaceInfo>();
            viewer.Initialize(Cloud, Vector3.Zero);

            colorShader = new TriplanarPBump();
            colorShader.CreateResources(Env.Window.Device);
            colorShader.SetTexture(Env.Window.Device, CurrentTexture(Env.Window.Device));
            colorShader.ColorTextureFilter = SamplerFilter.MinLinear_MagLinear_MipLinear;
            TransvoxelTriangulation<SurfaceInfo> tt = new TransvoxelTriangulation<SurfaceInfo>();
            tt.Triangulate(viewer);
            for (int i = 0; i < tt.vertexIndexes.Count; i += 3)
            {
                var pt1 = tt.vertexPoints[(int)tt.vertexIndexes[i + 2]];
                var pt2 = tt.vertexPoints[(int)tt.vertexIndexes[i + 1]];
                var pt3 = tt.vertexPoints[(int)tt.vertexIndexes[i]];
                var n = Vector3.Cross(pt2 - pt1, pt3 - pt2);

                colorShader.AddVertex(new Float3_Float2_Float3(pt1, new Vector2(0, 0), n));
                colorShader.AddVertex(new Float3_Float2_Float3(pt2, new Vector2(0, 0), n));
                colorShader.AddVertex(new Float3_Float2_Float3(pt3, new Vector2(0, 0), n));
            }
        }

        public override void Initialize(Instant Instant, GameState State, GameEnvironment Env)
        {
            State.Input = InputHandler.Build("System", "Cam", "Test");
            Env.WorldCamera.LockUp = true;
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
            colorShader.Update(Env.Window.Device);
        }

        public override void RenderEye(Instant Instant, GameState State, GameEnvironment Env, Matrix4x4 eyeMat, Framebuffer outbuffer, CommandList cmds)
        {
            colorShader.Render(outbuffer, cmds, SceneData, eyeMat, Matrix4x4.Identity);
        }

        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            if (isMouseLook) return;
            IG.Begin("Opts");
            if (IG.Button($"{(isGrass ? "Brick" : "Grass")}"))
            {
                isGrass = !isGrass;
                colorShader.SetTexture(Env.Window.Device, CurrentTexture(Env.Window.Device));
            }
            if (IG.Button("Home"))
            {
                State.Messages.AddMsg<TransitionScene>(new TransitionScene()
                {
                    Scene = new SceneSelectorScene(),
                    State = new GameState()
                });
            }

            IG.End();
        }
    }

    internal struct SurfaceInfo : IPointData
    {
        public int SurfaceId;
        public int SubsurfaceId;
        public float Degregation;
        public float V;
        public float SampleValue { get => V; }

        public float CreateEmptySpaceSamplevalue()
        {
            V = 0;
            return V;
        }
    }
}