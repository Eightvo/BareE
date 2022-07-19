using BareE.Rendering;
using BareE.UTIL;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Numerics;

using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.VirtualReality;

namespace BareE.GameDev
{
    [Flags]
    public enum DisplayMode
    {
        MonitorOnly = 1,
        VROnly = 2,
        Emulate = 4,
        Mirror = 8
    }

    public struct Resolution
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector2 Res { get { return new Vector2(Width, Height); } }
        public float AspectRatio { get { return (float)Width / (float)Height; } }

        public Resolution(int w, int h)
        {
            Width = w; Height = h;
        }
        public static implicit operator Resolution(System.Drawing.Size sz) {  return new Resolution(sz.Width, sz.Height); }
        public static implicit operator System.Drawing.Size(Resolution r) { return new System.Drawing.Size(r.Width, r.Height); }
    }
    public class GameEnvironmentWindow
    {
        [JsonIgnore]
        public bool RenderReady { get; set; }

        public String Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector2 Size { get { return new Vector2(Width, Height); } }

        [JsonProperty("Left")]
        private string _lStr
        {
            get { return Left.ToString(); }
            set
            {
                switch (value.ToLower())
                {
                    case "centered":
                        Left = Sdl2Native.SDL_WINDOWPOS_CENTERED;
                        break;

                    default:
                        Left = int.Parse(value);
                        break;
                }
            }
        }

        [JsonIgnore]
        public int Left { get; set; } = Sdl2Native.SDL_WINDOWPOS_CENTERED;

        [JsonProperty("Top")]
        private string _tStr
        {
            get { return Top.ToString(); }
            set
            {
                switch (value.ToLower())
                {
                    case "centered":
                        Left = Sdl2Native.SDL_WINDOWPOS_CENTERED;
                        break;

                    default:
                        Left = int.Parse(value);
                        break;
                }
            }
        }

        [JsonIgnore]
        public int Top { get; set; } = Sdl2Native.SDL_WINDOWPOS_CENTERED;

        public Vector2 Position { get { return new Vector2(Left, Top); } }
        public Resolution Resolution { get; set; }

        [JsonIgnore]
        public GraphicsDevice Device { get; internal set; }

        [JsonIgnore]
        public Sdl2Window Window { get; internal set; }

        [JsonIgnore]
        public CommandList Cmds { get; internal set; }

        [JsonIgnore]
        public ImGuiRenderer IGR { get; set; }
    }

    public class VRSettings
    {
        [JsonIgnore]
        public bool AllowVR { get; set; } = false;

        [JsonConverter(typeof(StringEnumConverter))]
        public MirrorTextureEyeSource MirrorTexEyeSource { get; set; }

        [JsonIgnore]
        public HmdPoseState Pose { get; set; }

        [JsonIgnore]
        public bool IsOcculus { get; set; }

        [JsonIgnore]
        public VRContext Context { get; set; }
    }

    public class GameEnvironment
    {
        private GameEnvironment() { }
        [JsonConverter(typeof(StringEnumConverter))]
        public GraphicsBackend PrefferedBackend { get; set; } = GraphicsBackend.OpenGL;

        [JsonProperty]
        public float TargetFramerate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Veldrid.TextureSampleCount PrefferedTextureCount { get; set; } 

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("DisplayMode")]
        public DisplayMode DisplayMode { get; set; }

        [JsonIgnore]
        public bool IsVR
        {
            get { return ((DisplayMode & (DisplayMode.VROnly | DisplayMode.Mirror)) != 0); }
        }

        [JsonProperty("VR")]
        public VRSettings VRSettings { get; set; } = new VRSettings();

        [JsonProperty]
        public GameEnvironmentWindow Window;

        [JsonIgnore]
        public Framebuffer LeftEyeBackBuffer { get; internal set; }

        [JsonIgnore]
        public Framebuffer RightEyeBackBuffer { get; internal set; }

        [JsonIgnore]
        public Framebuffer HUDBackBuffer { get; internal set; }

        [JsonIgnore]
        public BareE.Rendering.Camera WorldCamera { get; set; }

        [JsonIgnore]
        public RenderDoc rd;

        [JsonIgnore]
        private GameSceneBase LoadingScene { get; set; }

        public OutputDescription GetBackbufferOutputDescription()
        {
            var d = LeftEyeBackBuffer.OutputDescription;
            d = new OutputDescription()
            {
                ColorAttachments = d.ColorAttachments,
                DepthAttachment = d.DepthAttachment,
                SampleCount =PrefferedTextureCount
            };
            return d;
        }

        public PixelFormat VRPixelFormat { get { return IsVR ? VRSettings.Context.LeftEyeFramebuffer.ColorTargets[0].Target.Format : Window.Device.SwapchainFramebuffer.ColorTargets[0].Target.Format; } }

        private static GameEnvironmentWindow initWindow(GameEnvironmentWindow gew, bool isvr, VRSettings vrs, GraphicsBackend backend)
        {
            var winCI = new WindowCreateInfo()
            {
                X = gew.Left,
                Y = gew.Top,
                WindowWidth = gew.Width,
                WindowHeight = gew.Height,
                WindowTitle = gew.Title,
                WindowInitialState = WindowState.Normal
            };
            gew.Window = VeldridStartup.CreateWindow(winCI);

            if (isvr)
            {
                if (VRContext.IsOculusSupported())
                    vrs.IsOcculus = true;
                else
                {
                    if (!VRContext.IsOpenVRSupported())
                        throw new Exception("No VR.");
                }

                var opts = new VRContextOptions()
                {
                    EyeFramebufferSampleCount = TextureSampleCount.Count4
                };
                vrs.Context = vrs.IsOcculus ? VRContext.CreateOculus(opts) : VRContext.CreateOpenVR(opts);
            }
            GraphicsDeviceOptions gdo = new GraphicsDeviceOptions(false, null, false, ResourceBindingModel.Default, true, true, true);

            if (backend == GraphicsBackend.Vulkan && isvr)
            {
                (string[] instance, string[] device) = vrs.Context.GetRequiredVulkanExtensions();
                VulkanDeviceOptions vdo = new VulkanDeviceOptions(instance, device);
                GraphicsDevice gd = GraphicsDevice.CreateVulkan(gdo, new SwapchainDescription(
                    VeldridStartup.GetSwapchainSource(gew.Window),
                    (uint)gew.Window.Width, (uint)gew.Window.Height,
                    gdo.SwapchainDepthFormat, gdo.SyncToVerticalBlank, true), vdo);
                gew.Device = gd;
            }
            else
            {
                GraphicsDevice gd = VeldridStartup.CreateGraphicsDevice(gew.Window, gdo, backend);
                Swapchain sc = gd.MainSwapchain;
                gew.Device = gd;
            }
            if (isvr)
            {
                vrs.Context.Initialize(gew.Device);
                vrs.AllowVR = true;
            }
            //gew.Device = VeldridStartup.CreateGraphicsDevice(gew.Window, backend);

            gew.Cmds = gew.Device.ResourceFactory.CreateCommandList();

            return gew;
        }

        public static GameEnvironment Load()
        {
            return Load("BareE.GameDev.environmentdefault.json");
        }

        public static GameEnvironment Load(String resource)
        {
            
            var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<GameEnvironment>(AssetManager.ReadFile(resource));
            ret.Window.Title = ret.Window.Title
                                       .Replace("{BACKEND}", ret.PrefferedBackend.ToString())
                                       .Replace("{DISPLAYMODE}", ret.DisplayMode.ToString())
                ;

            initWindow(ret.Window, ret.IsVR, ret.VRSettings, ret.PrefferedBackend);
            var vrPixelFormat = ret.IsVR ? ret.VRSettings.Context.LeftEyeFramebuffer.ColorTargets[0].Target.Format : ret.Window.Device.SwapchainFramebuffer.ColorTargets[0].Target.Format;
            var winPixelFormat = ret.Window.Device.SwapchainFramebuffer.ColorTargets[0].Target.Format;

            ret.HUDBackBuffer = CreateFlatbuffer(ret.Window.Device, (uint)ret.Window.Resolution.Width, (uint)ret.Window.Resolution.Height, vrPixelFormat, TextureSampleCount.Count1);
            ret.LeftEyeBackBuffer = Util.CreateFramebuffer(ret.Window.Device, (uint)ret.Window.Resolution.Width, (uint)ret.Window.Resolution.Height, vrPixelFormat, TextureSampleCount.Count1);
            ret.RightEyeBackBuffer = Util.CreateFramebuffer(ret.Window.Device, (uint)ret.Window.Resolution.Width, (uint)ret.Window.Resolution.Height, vrPixelFormat, TextureSampleCount.Count1);

            ret.WorldCamera = new LookAtQuaternionCamera(new Vector2(ret.Window.Resolution.Width, ret.Window.Resolution.Height));

            ret.Window.IGR = new ImGuiRenderer(ret.Window.Device, ret.HUDBackBuffer.OutputDescription, (int)ret.HUDBackBuffer.Width, (int)ret.HUDBackBuffer.Height);
            //var io = ImGuiNET.ImGui.GetIO();
            ret.Window.Window.Resized += () => ret.Window.Device.MainSwapchain.Resize((uint)(ret.Window.Window.Width), (uint)(ret.Window.Window.Height));
            ret.Window.Window.Resized += () => ret.Window.IGR.WindowResized(
                (ret.Window.Window.Width), (ret.Window.Window.Height)
                );

            return ret;
        }

        public static Framebuffer CreateFlatbuffer(GraphicsDevice device, uint resolutionX, uint resolutionY, PixelFormat pixelFormat, TextureSampleCount sampleCount)

        {
            var drawTrgt = device.ResourceFactory.CreateTexture(
                new TextureDescription(resolutionX,
                                       resolutionY,
                                       1, 1, 1,
                                       pixelFormat,
                                       TextureUsage.RenderTarget | TextureUsage.Sampled,
                                       TextureType.Texture2D,
                                       sampleCount)
            );

            FramebufferAttachmentDescription[] cltTrgs = new FramebufferAttachmentDescription[1]
            {
                new FramebufferAttachmentDescription()
                {
                    ArrayLayer=0,
                    MipLevel=0,
                    Target=drawTrgt
                }
            };

            var frameBuffDesc = new FramebufferDescription()
            {
                ColorTargets = cltTrgs
            };
            var offscreenBuffer = device.ResourceFactory.CreateFramebuffer(frameBuffDesc);
            return offscreenBuffer;
        }
    }
}