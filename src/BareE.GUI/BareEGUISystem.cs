using BareE.GameDev;
using BareE.Rendering;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using System.Drawing;


namespace BareE.GUI
{
    public class BareEGUISystem: GameSystem
    {
        FullScreenTexture GUICanvas;
        GUIContext Context;
        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img;
        Texture cTex;
        public BareEGUISystem(System.Drawing.Size Resolution)
        {
            Context = new GUIContext(Resolution);
        }


        public Image<TPixel> ToImageSharpImage<TPixel>(System.Drawing.Bitmap bitmap) where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return SixLabors.ImageSharp.Image.Load<TPixel>(memoryStream);
            }
        }

        public override void Load(Instant Instant, GameState State, GameEnvironment Env)
        {
            GUICanvas= new FullScreenTexture();
            GUICanvas.SetOutputDescription(Env.HUDBackBuffer.OutputDescription);
            GUICanvas.CreateResources(Env.Window.Device);



            img = ToImageSharpImage<Rgba32>(Context.Canvas);
            cTex = AssetManager.LoadTexture(img, Env.Window.Device);
            GUICanvas.SetTexture(Env.Window.Device, cTex);
            GUICanvas.Update(Env.Window.Device);
            
        }
        Random rand = new Random();
        public override void Update(Instant Instant, GameState State, GameEnvironment Env)
        {
            //.img.Mutate<Rgba32>()
            using (var g = Graphics.FromImage(Context.Canvas))
            {
                new SolidBrush(System.Drawing.Color.FromArgb(1, 1, 0, 1));
                g.FillRectangle(Brushes.Red, new System.Drawing.Rectangle(0, 0, 2500, 2050));
            }
            //var imgRect = new System.Drawing.Rectangle(0, 0, Context.Canvas.Width, Context.Canvas.Height);
            //var data = Context.Canvas.LockBits(imgRect, System.Drawing.Imaging.ImageLockMode.ReadWrite, Context.Canvas.PixelFormat);
            //Env.Window.Device.UpdateTexture(cTex, data.Scan0,(uint)(Context.Canvas.Width*Context.Canvas.Height*4), 0, 0, 0, (uint)Context.Canvas.Width, (uint)Context.Canvas.Height, 0, 0, 0);
            //Context.Canvas.UnlockBits(data);

            AssetManager.UpdateTextureData(Env.Window.Device,cTex,ToImageSharpImage<Rgba32>(Context.Canvas));

            //var data = Context.Canvas.LockBits(imgRect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //int bytes = Math.Abs(data.Stride) * Context.Canvas.Height;
            //byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            //System.Runtime.InteropServices.Marshal.Copy(data.Scan0, rgbValues, 0, bytes);
            //Env.Window.Device.UpdateTexture(cTex, rgbValues, 0, 0, 0, (uint)Context.Canvas.Width, (uint)Context.Canvas.Height, 0, 0, 0);
            //Context.Canvas.UnlockBits(data);
            

        }
        public override void RenderHud(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            GUICanvas.Render(outbuffer, cmds, null, System.Numerics.Matrix4x4.Identity, System.Numerics.Matrix4x4.Identity);
        }
    }
}
