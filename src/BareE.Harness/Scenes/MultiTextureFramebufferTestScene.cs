using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.Harness.Scenes
{
    public class MultiTextureFramebufferTestScene : GameSceneBase
    {
        Framebuffer mtFramebuffer;
        IntPtr ColorTarget0TexturePtr;
        IntPtr ColorTarget1TexturePtr;
        IntPtr ColorTarget2TexturePtr;
        IntPtr DepthTargetTexturePtr;

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            base.Load(Instant, State, Env);

            mtFramebuffer = UTIL.Util.CreateFramebuffer(Env.Window.Device, 800, 600, new PixelFormat[] { PixelFormat.R8_G8_B8_A8_UNorm, PixelFormat.R8_G8_B8_A8_UNorm, PixelFormat.R8_G8_B8_A8_UNorm }, TextureSampleCount.Count1, PixelFormat.R32_Float);
            ColorTarget0TexturePtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, mtFramebuffer.ColorTargets[0].Target);
            ColorTarget1TexturePtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, mtFramebuffer.ColorTargets[1].Target);
            ColorTarget2TexturePtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, mtFramebuffer.ColorTargets[2].Target);
            DepthTargetTexturePtr = Env.Window.IGR.GetOrCreateImGuiBinding(Env.Window.Device.ResourceFactory, mtFramebuffer.DepthTarget.Value.Target);

        }

    }
}
