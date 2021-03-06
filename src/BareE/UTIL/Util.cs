using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Veldrid;

namespace BareE.UTIL
{
    public static class Util
    {
        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public static PixelFormat GetNativePixelFormat(GraphicsDevice device)
        {
            return device.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
        }
        [System.Obsolete]
        public static Framebuffer CreateFramebuffer(GraphicsDevice device, uint resolutionX, uint resolutionY)
        {
            return CreateFramebuffer(device, resolutionX, resolutionY, GetNativePixelFormat(device), TextureSampleCount.Count1);
        }

        [System.Obsolete]
        public static Framebuffer CreateFramebuffer(GraphicsDevice device, uint resX, uint resY, PixelFormat[] piexlFormats, TextureSampleCount sampleCount, PixelFormat? depthFormat = PixelFormat.R32_Float)
        {
            FramebufferAttachmentDescription[] clrTargets = new FramebufferAttachmentDescription[piexlFormats.Length];
            for (int i = 0; i < piexlFormats.Length; i++)
            {
                var pxlFormat = piexlFormats[i];
                clrTargets[i] = new FramebufferAttachmentDescription()
                {
                    ArrayLayer = 0,
                    MipLevel = 0,
                    Target = device.ResourceFactory.CreateTexture(
                        new TextureDescription(resX, resY, 1, 1, 1, pxlFormat, TextureUsage.RenderTarget | TextureUsage.Sampled, TextureType.Texture2D, sampleCount)
                    )
                };
            }
            FramebufferAttachmentDescription? depTrg=null;
            if (depthFormat.HasValue)
            {
                depTrg = new FramebufferAttachmentDescription()
                {
                    ArrayLayer = 0,
                    MipLevel = 0,
                    Target = device.ResourceFactory.CreateTexture(
                    new TextureDescription(resX,
                                           resY,
                                           1, 1, 1,
                                           PixelFormat.R32_Float,
                                           TextureUsage.DepthStencil,
                                           TextureType.Texture2D,
                                           sampleCount)
                        )
                };
            }
            var frameBuffDesc = new FramebufferDescription()
            {
                ColorTargets = clrTargets,
                DepthTarget = depTrg
            };
            var offscreenBuffer = device.ResourceFactory.CreateFramebuffer(frameBuffDesc);
            return offscreenBuffer;
        }
        
        [System.Obsolete]
        public static Framebuffer CreateFramebuffer(GraphicsDevice device, uint resolutionX, uint resolutionY, PixelFormat pixelFormat, TextureSampleCount sampleCount)
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

            var depthTrgt = device.ResourceFactory.CreateTexture(
                new TextureDescription(resolutionX,
                                       resolutionY,
                                       1, 1, 1,
                                       PixelFormat.R32_Float,
                                       TextureUsage.DepthStencil,
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

            FramebufferAttachmentDescription depTrg = new FramebufferAttachmentDescription()
            {
                ArrayLayer = 0,
                MipLevel = 0,
                Target = depthTrgt
            };

            var frameBuffDesc = new FramebufferDescription()
            {
                ColorTargets = cltTrgs,
                DepthTarget = depTrg
            };
            var offscreenBuffer = device.ResourceFactory.CreateFramebuffer(frameBuffDesc);
            return offscreenBuffer;
        }
        /// <summary>
        /// Create Frame buffer with specified texture as ColorTarget.
        /// Depth Buffer will be created at same width, height, miplevel and sample count.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="colorTarget"></param>
        /// <returns></returns>
        public static Framebuffer CreateFramebuffer(GraphicsDevice device, Texture colorTarget)
        {
            var depthTrgt = device.ResourceFactory.CreateTexture(
                new TextureDescription(colorTarget.Width,
                                       colorTarget.Height,
                                       1, colorTarget.MipLevels, 1,
                                       PixelFormat.R32_Float,
                                       TextureUsage.DepthStencil,
                                       TextureType.Texture2D,
                                       colorTarget.SampleCount)
            );

            FramebufferAttachmentDescription[] cltTrgs = new FramebufferAttachmentDescription[1]
            {
                new FramebufferAttachmentDescription()
                {
                    ArrayLayer=0,
                    MipLevel=0,
                    Target=colorTarget
                }
            };

            FramebufferAttachmentDescription depTrg = new FramebufferAttachmentDescription()
            {
                ArrayLayer = 0,
                MipLevel = 0,
                Target = depthTrgt
            };

            var frameBuffDesc = new FramebufferDescription()
            {
                ColorTargets = cltTrgs,
                DepthTarget = depTrg
            };
            var offscreenBuffer = device.ResourceFactory.CreateFramebuffer(frameBuffDesc);
            return offscreenBuffer;
        }
    }
}