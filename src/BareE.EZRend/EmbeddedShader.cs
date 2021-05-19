using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.IO;

using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;

namespace EZRend
{
    public class EZRendExceptionEventArgs
    {
        public Exception ex { get; }
        public EZRendExceptionEventArgs(Exception e) { ex = e; }
    }

    public static class LibrarySettings
    {
        public delegate void EZRendExceptionEventHandler(object sender, EZRendExceptionEventArgs args);
        public static event EZRendExceptionEventHandler ExceptionOccured;
        public static event EZRendExceptionEventHandler WarningOccured;
        internal static void RaiseEZRendException(object sender, Exception e)
        {
            ExceptionOccured?.Invoke(sender, new EZRendExceptionEventArgs(e));
        }
        internal static void RaiseEZRendWarning(object sender, Exception e)
        {
            WarningOccured?.Invoke(sender, new EZRendExceptionEventArgs(e));
        }
    }
    internal static class EmbeddedShader
    {
        public static ShaderDescription ByName(String resource, ShaderStages stage)
        {
            List<byte> data = new List<byte>();
            using (var rdr=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                int nxt = rdr.ReadByte();
                while (nxt != -1)
                {
                    data.Add((byte)nxt);
                    nxt = rdr.ReadByte();
                }
            }
            byte[] bytes = data.ToArray();
            return new ShaderDescription(stage, bytes, "main");
        }

        public static Shader[] CreateShaderSet(ResourceFactory factory, String res)
        {
            var vert = ByName($"{res}.vert.spv", ShaderStages.Vertex);
            var frag = ByName($"{res}.frag.spv", ShaderStages.Fragment);
            return factory.CreateFromSpirv(vert, frag);
            
        }
    }

    public static class EmbeddedImage
    {
        public static ImageSharpTexture ByName(String resource)
        {
            var strm = new MemoryStream(UTIL.FindFileData(resource));
            return new Veldrid.ImageSharp.ImageSharpTexture(strm);
        }
    }
    public static class UTIL
    {
        public static Byte[] FindFileData(String name)
        {
            if (System.IO.File.Exists(name))
            {
                return System.IO.File.ReadAllBytes(name);
            }
            foreach (var asm in new List<System.Reflection.Assembly>() { System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.Assembly.GetExecutingAssembly() })
            {
                try
                {
                    List<byte> data = new List<byte>();
                    using (var rdr = asm.GetManifestResourceStream(name))
                    {
                        if (rdr != null)
                        {
                            int nxt = rdr.ReadByte();
                            while (nxt != -1)
                            {
                                data.Add((byte)nxt);
                                nxt = rdr.ReadByte();
                            }
                            return data.ToArray();
                        }
                    }
                }
                catch (FileNotFoundException fnfe) { }
            }
            throw new FileNotFoundException(name);
        }
        public static StreamReader FindStreamReader(String name)
        {
            if (System.IO.File.Exists(name)) return new StreamReader(new FileStream(name, FileMode.Open));
            return new StreamReader(new MemoryStream(FindFileData(name)));
        }


        public static Texture FindTexture(GraphicsDevice device, String textureResource)
        {
            return EmbeddedImage.ByName(textureResource).CreateDeviceTexture(device, device.ResourceFactory);
        }

        public static Texture CreateDeviceTexture(GraphicsDevice device, Image<Rgba32> img, bool mipmap=false)
        {
            var txt = new ImageSharpTexture(img, mipmap);
            return txt.CreateDeviceTexture(device, device.ResourceFactory);
        }
        public static Texture CreateDeviceTexture(GraphicsDevice device, String path)
        {
            if (System.IO.File.Exists(path))
            {
                var ImgShrpTextire = new Veldrid.ImageSharp.ImageSharpTexture(path, false);
                return ImgShrpTextire.CreateDeviceTexture(device, device.ResourceFactory);
            }
            return EmbeddedImage.ByName(path).CreateDeviceTexture(device,device.ResourceFactory);
        }


        public static void GetFramebufferContents(String fileName, GraphicsDevice device, Framebuffer framebuffer)
        {
            var resource = device.Map(framebuffer.ColorTargets[0].Target, MapMode.Read);
            
           // var Image = new SixLabors.ImageSharp.Image();
        }

    }
}
