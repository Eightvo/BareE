using BareE.DataStructures;
using BareE.EZRend.Flat;
using BareE.Rendering;

using FFmpeg.AutoGen;

using ImGuiNET;

using Microsoft.VisualBasic;

using SixLabors.Fonts;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using Veldrid;
using Veldrid.ImageSharp;

namespace BareE.GUI.EZText
{
    public struct EZTextVert : IProvideVertexLayoutDescription
    {
        public Vector3 Position;
        public Vector2 Uv;
        public Vector3 Color;
        public uint SizeInBytes {get{return 4*3+4*2+3*4;} }
        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("clr", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
              );
        }
    }


    public class EZText: VertexTextureShader<EZTextVert>
    {
        SpriteAtlas FontAtlas;
        SixLabors.Fonts.FontCollection collection = new SixLabors.Fonts.FontCollection();
        HashSet<String> _knownFontSizeCominations = new HashSet<string>();
        String Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-=`~!@#$%^&*()_+\\|/?'\";:.><,";
        public EZText():base("BareE.GUI.EZText.EZText") {
            this.SampDesc = new SamplerDescription()
            {
                Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
                
            };
            this.BlendState = BlendStateDescription.SingleAlphaBlend;
            FontAtlas = new SpriteAtlas();
        }
        
        
        public String AddConsoleFont(GraphicsDevice device)
        {
            var filePath = @"BareE.GUI.EZText.dos_8x8_font_white.png";
            int i = 0;
            for(int x=0;x<16;x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    Image<Rgba32> img = AssetManager.GetImage(filePath);
                    FontAtlas.Merge($"Console_8_{i}", img, new Vector4(y*9 + 1, x*9 + 1, 8, 8));
                    i++;
                }
            }
            FontAtlas.Build(0,false);
            SetTexture(device, AssetManager.LoadTexture(FontAtlas.AtlasSheet, device, true, false));
            return "Console";
        }

        //https://stackoverflow.com/questions/52866348/imagesharp-and-font-height
        /// <param name="text">one or more characters to scale to fill as much of the target image size as required.</param>
        /// <param name="targetSize">the size in pixels to generate the image</param>
        /// <param name="outputFileName">path/filename where to save the image to</param>
        private Image<Rgba32> GenerateImage(string text, int pt, Font font)
        {
            var glyphs = TextBuilder.GenerateGlyphs(text, new TextOptions(font));
            //var bounds = glyphs.Bounds;
            var bounds = TextMeasurer.Measure("M", new TextOptions(font));
            var img = new Image<Rgba32>((int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
            img.Mutate(i => i.DrawText(new DrawingOptions() { GraphicsOptions = new GraphicsOptions() { Antialias = false}},text, font, Color.White, new PointF(0, 0)));
            //img.Mutate(i => i.Draw(new DrawingOptions() { GraphicsOptions = new GraphicsOptions() { Antialias = true } }, Color.White, 1.0f, glyphs));
            return img;
        }

        public string AddFont(GraphicsDevice device, String fontFile, params int[] fontSizes)
        {
            var bgColor = Color.Transparent;
            var fgColor = Color.White;
            var fam = collection.Add(fontFile);
            foreach(var fontsizeInPts in fontSizes)
            {
                var currentFont = new Font(fam, fontsizeInPts);
                var fontKey = $"{currentFont.Name}_{fontsizeInPts}";
                if (_knownFontSizeCominations.Contains(fontKey))
                    continue;
                foreach (var ch in Alphabet)
                {
                    CodePoint cp = new CodePoint(ch);
                    Image<Rgba32> charImage = GenerateImage(ch.ToString(), fontsizeInPts, currentFont);
                    FontAtlas.Merge($"{fontKey}_{(int)ch}", charImage);
                }
            }
            FontAtlas.Build(1024,false);
            SetTexture(device, AssetManager.LoadTexture(FontAtlas.AtlasSheet, device, true, false));

            return fam.Name;
        }


        public void AddString(String font, int pt, Vector2 pos, String txt, Vector3 clr)
        {
            foreach(var v in txt)
            {
                pos.X+=AddChar(font, pt,pos, v,clr);
            }
        }
        public float AddChar(String font, int pt, Vector2 pos, char ch,Vector3 clr,float z =-1)
        {
            //var key = $"{(int)ch}_{pt}";
            var key = $"{font}_{pt}_{(int)ch}";
            var src = FontAtlas[key];
            var ogSize = FontAtlas.EstimateOriginalSize(src.Width,src.Height);
            var dest = new RectangleF(pos.X, pos.Y, ogSize.X, ogSize.Y);

            Vector3 p0 = new Vector3(dest.Left, dest.Bottom, z);
            Vector3 p1 = new Vector3(dest.Left, dest.Top, z);
            Vector3 p2 = new Vector3(dest.Right, dest.Top, z);
            Vector3 p3 = new Vector3(dest.Right, dest.Bottom, z);
            Vector2 uv0 = new Vector2(src.Left, src.Top);
            Vector2 uv1 = new Vector2(src.Left, src.Bottom);
            Vector2 uv2 = new Vector2(src.Right, src.Bottom);
            Vector2 uv3 = new Vector2(src.Right, src.Top);
            AddSprite(p0, p1, p2, p3, uv0, uv1, uv2, uv3, clr,clr,clr,clr);
            return ogSize.X;
        }
        private void AddSprite(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector3 clr0, Vector3 clr1, Vector3 clr2, Vector3 clr3)
        {
            AddVertex(new EZTextVert() { Position = p0, Uv = uv0, Color = clr0 });
            AddVertex(new EZTextVert() { Position = p2, Uv = uv2, Color = clr2 });
            AddVertex(new EZTextVert() { Position = p1, Uv = uv1, Color = clr1 });

            AddVertex(new EZTextVert() { Position = p0, Uv = uv0, Color = clr0 });
            AddVertex(new EZTextVert() { Position = p3, Uv = uv3, Color = clr3 });
            AddVertex(new EZTextVert() { Position = p2, Uv = uv2, Color = clr2 });
        }

    }
}
