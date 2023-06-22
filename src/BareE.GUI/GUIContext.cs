using BareE.DataStructures;
using BareE.GameDev;
using BareE.GUI.TextRendering;
using BareE.Rendering;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using System.IO.Compression;

using Veldrid;

using static BareE.DataStructures.SpriteAtlas;

using Rectangle = SixLabors.ImageSharp.Rectangle;
using Size = System.Drawing.Size;
using Veldrid.Sdl2;
using System.Linq;
using System.Reflection.Emit;
using SixLabors.ImageSharp.Drawing.Processing;
using FFmpeg.AutoGen;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;
using BareE.GUI.Widgets;
using Microsoft.VisualBasic.FileIO;

namespace BareE.GUI
{
    public class GUIContext
    {
        static int _ID = 0;
        public int ID { get; private set; } = ++_ID;
        
        Camera guiCam;
        Framebuffer GuiBuffer;
        BaseGuiShader GuiShader;
        BareE.EZRend.ColoredLineShader lines;
        public StyleBook StyleBook;
        //Key is <Font>_<pt>_<charInt>
        Dictionary<String, int> FontAdvance;
        //Key is <Foint>_<pt>
        Dictionary<String, int> FontLineHeights;

        SpriteAtlas FontAtlas;
        SpriteAtlas StyleAtlas;
        private Size _resolution;
        private bool Dirty = true;
        public List<WidgetBase> Widgets;
        public Size Resolution { get { return _resolution; } set { _resolution = value;GuiBuffer?.Dispose();GuiBuffer = null; Dirty = true; } }
        public Texture Canvas { get { return GuiBuffer.ColorTargets[0].Target; } }
        public GUIContext(GameEnvironment env):this(env, env.Window.Resolution){}
        public GUIContext(GameEnvironment env, Size initialResolution)
        {
            Widgets = new List<WidgetBase>();
            Resolution = initialResolution;


            guiCam = new OrthographicCamera(Resolution.Width, Resolution.Height, 1000);
            guiCam.Move(new Vector3(Resolution.Width / 2.0f, Resolution.Height / 2.0f, 0));

            FontAdvance = new Dictionary<string, int>();
            FontLineHeights = new Dictionary<string, int>();
            FontAtlas = new SpriteAtlas();
            StyleAtlas = new SpriteAtlas();
            StyleAtlas.AutoPad = false;

            GuiBuffer = CreateGuiContextFrameBuffer(env);
            GuiShader = new BaseGuiShader();
            GuiShader.SetOutputDescription(GuiBuffer.OutputDescription);
            // GuiShader.RastorizerDescription = RasterizerStateDescription.CullNone;
            GuiShader.RastorizerDescription = new RasterizerStateDescription()
            {
                CullMode= FaceCullMode.Back,
                DepthClipEnabled= true,
                FillMode = PolygonFillMode.Solid,
                ScissorTestEnabled=true
            };
            GuiShader.BlendState = BlendStateDescription.SingleAlphaBlend;
             
            GuiShader.DepthStencilDescription = new DepthStencilStateDescription()
            {
                DepthComparison = ComparisonKind.Always,
                DepthWriteEnabled = false,
            };

            GuiShader.ColorTextureFilter = SamplerFilter.MinPoint_MagPoint_MipPoint;
            GuiShader.SampleAddressModeU = SamplerAddressMode.Wrap;
            GuiShader.SampleAddressModeV = SamplerAddressMode.Wrap;

            GuiShader.CreateResources(env.Window.Device);
            GuiShader.resolution = new Vector2(900, 900);
            GuiShader.mousepos = new Vector2(30, 540);

            lines = new EZRend.ColoredLineShader();
            lines.SetOutputDescription(GuiBuffer.OutputDescription);
            lines.CreateResources(env.Window.Device);

            GuiShader.Update(env.Window.Device);

            FontAtlas = new SpriteAtlas();
            ImportDefaultFont(env.Window.Device);
            ImportDefaultStyle(env.Window.Device);
            
            
        }


        private void ImportDefaultStyle(GraphicsDevice device)
        {
            //Image<Rgba32> blank = new Image<Rgba32>(10, 10);
            //blank.Mutate(x => x.Clear(Color.White));
            //StyleAtlas.Merge("BLANK", blank);
            ImportFrame("Default_TitleFrame", device, @"BareE.GUI.Assets.Styles.MinGui.R2Frame.png");
            ImportFrame("Default_InnerFrame", device, @"BareE.GUI.Assets.Styles.MinGui.squareFrame.png");
            StyleAtlas.Merge("Default_CloseButton", AssetManager.GetImage(@"BareE.GUI.Assets.Styles.MinGui.close.png"));
            StyleAtlas.Merge("Default_MouseNormal", AssetManager.GetImage(@"BareE.GUI.Assets.Styles.MinGui.HandOpen.png"));
            StyleAtlas.Merge("Default_MouseDown", AssetManager.GetImage(@"BareE.GUI.Assets.Styles.MinGui.HandClosed.png"));
            StyleAtlas.Build(0, false);
            GuiShader.SetStyleTexture(device, AssetManager.LoadTexture(StyleAtlas.AtlasSheet, device, false, false));

            var defaultStyle = new Dictionary<StyleElement, object>();
            defaultStyle.Add(StyleElement.TitleFrame, "Default_TitleFrame");
            defaultStyle.Add(StyleElement.TitleFrameColor, new Vector4(0.8f, 0.8f, 0.9f, 1));
            defaultStyle.Add(StyleElement.TitleFrameMarginHorizontal,5);
            defaultStyle.Add(StyleElement.TitleFrameMarginVertical, 5);
            

            defaultStyle.Add(StyleElement.InnerFrame, "Default_InnerFrame");
            defaultStyle.Add(StyleElement.InnerFrameColor, new Vector4(0.8f, 0.8f, 0.9f, 1));
            defaultStyle.Add(StyleElement.InnerFrameMarginHorizontal, 3);
            defaultStyle.Add(StyleElement.InnerFrameMarginVertical, 3);

            defaultStyle.Add(StyleElement.Font, "Default");
            defaultStyle.Add(StyleElement.FontSize, 8);
            defaultStyle.Add(StyleElement.FontColor, new Vector4(0, 0, 0, 1));

            defaultStyle.Add(StyleElement.MouseCursor_Normal, "Default_MouseNormal");
            defaultStyle.Add(StyleElement.MouseCursor_NormalColor, new Vector4(0.8f, 0.8f, 0.9f, 1));
            defaultStyle.Add(StyleElement.MouseCursor_Down, "Default_MouseDown");
            defaultStyle.Add(StyleElement.MouseCursor_DownColor, new Vector4(0.8f, 0.8f, 0.9f, 1));


            defaultStyle.Add(StyleElement.CloseButton_Normal, "Default_CloseButton");
            defaultStyle.Add(StyleElement.CloseButton_Hover , "Default_CloseButton");
            defaultStyle.Add(StyleElement.CloseButton_Down  , "Default_CloseButton");



            defaultStyle.Add(StyleElement.CloseButton_NormalColor, new Vector4(1, 0, 0, 1));
            defaultStyle.Add(StyleElement.CloseButton_HoverColor, new Vector4(0.8f, 0.1f, 0.1f, 1));
            defaultStyle.Add(StyleElement.CloseButton_DownColor, new Vector4(0.25f, 0.25f, 0.25f, 1));

            StyleBook = new StyleBook();
            StyleBook.DefineStyle("Default", defaultStyle);
        }

        private void ImportDefaultFont(GraphicsDevice device)
        {
            var filePath = @"BareE.GUI.EZText.dos_8x8_font_white.png";
            int i = 0;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    Image<Rgba32> img = AssetManager.GetImage(filePath);
                    FontAtlas.Merge($"Default_8_{i}", img, new Vector4(y * 9 + 1, x * 9 + 1, 8, 8));
                    FontAdvance[$"Default_8_{i}"] = 8;
                    i++;
                }
            }
            FontLineHeights["Default_8"] = 10;
            FontAtlas.Build(0, false);
            GuiShader.SetFontTexture(device, AssetManager.LoadTexture(FontAtlas.AtlasSheet, device, false, true));
        }
        /// <summary>
        /// The image source should have a width and height divisible by 3.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="imageSource"></param>
        public void ImportFrame(String frameStyleName, GraphicsDevice device, String imageSource)
        {
            var img = AssetManager.GetImage(imageSource);
            int dX = (int)Math.Floor(img.Width / 3.0f);
            int dY= (int)Math.Floor(img.Height / 3.0f);
            var inX = img.Width - 2 * dX;
            int inY = img.Height - 2 * dY;

            StyleAtlas.Merge($"{frameStyleName}_Frame_TL", img.Clone(), new Vector4(0, 0, dX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_TM", img.Clone(), new Vector4(dX, 0, inX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_TR", img.Clone(), new Vector4(dX + inX, 0, dX, dY));

            StyleAtlas.Merge($"{frameStyleName}_Frame_ML", img.Clone(), new Vector4(0, dY, dX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_MM", img.Clone(), new Vector4(dX, dY, inX, inY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_MR", img.Clone(), new Vector4(dX+inX, dY, dX, dY));


            StyleAtlas.Merge($"{frameStyleName}_Frame_BL", img.Clone(), new Vector4(0, dY + inY, dX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_BM", img.Clone(), new Vector4(dX, dX+inY, inX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_BR", img.Clone(), new Vector4(dX + inX, dY + inY, dX, dY));


            StyleAtlas.Build(0, @"C:\AA_Main\Style.png");
            GuiShader.SetStyleTexture(device, AssetManager.LoadTexture(StyleAtlas.AtlasSheet, device, false, false));
        }
        public void ImportImage(String imageName, GraphicsDevice device, String imageSource)
        {
            ImportImage(imageName, device, imageSource, Rectangle.Empty);
        }
        public void ImportImage(String imageName, GraphicsDevice device, String imageSource, Rectangle subRec)
        {
            var img = AssetManager.GetImage(imageSource);
            //var subRec = new RectangleF(0, 0, img.Width, img.Height);
            if (subRec.Width == 0 || subRec.Height == 0)
                subRec = new Rectangle(0, 0, img.Width, img.Height);
            StyleAtlas.Merge(imageName, img, subRec);
            StyleAtlas.Build(0, false);
            GuiShader.SetStyleTexture(device, AssetManager.LoadTexture(StyleAtlas.AtlasSheet, device, false, false));

        }
        String ActiveFont="Default";
        int ActiveFontSize=8;
        Vector4 ActiveFontColor=new Vector4(0,0,0,1);
        Vector2 CursorPosition=new Vector2(0,0);

        public void AddTextBlock(RectangleF textArea, int zIndex, String Text)
        {
            AddTextBlock(textArea, zIndex, Text, ActiveFont, ActiveFontSize, ActiveFontColor);
        }
        public void AddTextBlock(RectangleF textArea, int zIndex, String Text, String font, int fontSize, Vector4 fontColor)
        {
            float cX = textArea.X;
            float cY = textArea.Y;
            var lineHeight = FontLineHeights[$"{font}_{fontSize}"];
            foreach(var ch in Text)
            {
                var chId = (int)ch;
                var key = $"{font}_{fontSize}_{chId}";
                var adv = FontAdvance[key];
                if (cX+adv> textArea.X+textArea.Width)
                {
                    cX = textArea.X;
                    cY += lineHeight;
                }
                if (cY > textArea.Height + textArea.Y)
                    return;
                AddCharacter(new Vector2(cX, cY), ch, fontColor);
                cX += adv;
            }

        }
        public void AddCharacter(Vector2 pos, char ch, Vector4 clr)
        {
            var chId = (int)ch;
            var key = $"{ActiveFont}_{ActiveFontSize}_{chId}";
            var src = FontAtlas[key];
            var ogSize = FontAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, ogSize.X, ogSize.Y);
            DrawQuad(0, clr, src, dest, true);
            

        }
        /*
        protected void AddHorizontal(Vector2 pos, Vector2 sz, Vector4 clr)
        {
            var src = StyleAtlas["Horizontal_L"];
            var srcSz = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, srcSz.X, sz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas["Horizontal_M"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y,sz.X-2*srcSz.X, sz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas["Horizontal_R"];
            dest = new RectangleF(pos.X+srcSz.X+sz.X - 2*srcSz.X, pos.Y, srcSz.X, sz.Y);
            DrawQuad(0, clr, src, dest, false);

        }
        protected void AddVertical(Vector2 pos, Vector2 sz, Vector4 clr)
        {
            var src = StyleAtlas["Vertical_T"];
            var srcSz = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y,sz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas["Vertical_M"];
            dest = new RectangleF(pos.X , pos.Y+srcSz.Y,  sz.X, sz.Y-2*srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas["Vertical_B"];
            dest = new RectangleF(pos.X, pos.Y+srcSz.Y+sz.Y-2*srcSz.Y, sz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

        }
        */
        public Rectangle AddBanner(String frameStyleName, Vector2 pos, int Width, Vector4 clr)
        {
            var src = StyleAtlas[$"{frameStyleName}_Frame_TL"];
            var srcSz = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_TR"];
            dest = new RectangleF(pos.X + Width - srcSz.X, pos.Y, srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_BL"];
            dest = new RectangleF(pos.X, pos.Y + 2*srcSz.Y - srcSz.Y, srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_BR"];
            dest = new RectangleF(pos.X + Width - srcSz.X, pos.Y + 2*srcSz.Y - srcSz.Y, srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_TM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y, Width - 2 * srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_BM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y + 2*srcSz.Y - srcSz.Y, Width- 2 * srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);
            return new Rectangle((int)pos.X, (int)pos.Y, Width, (int)(2 * srcSz.Y));

        }
        public Rectangle AddFrame(String frameStyleName, Vector2 pos, Vector2 sz, Vector4 clr)
        {
            var src = StyleAtlas[$"{frameStyleName}_Frame_TL"];
            var srcSz = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src= StyleAtlas[$"{frameStyleName}_Frame_TR"];
            dest = new RectangleF(pos.X + sz.X - srcSz.X, pos.Y, srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_BL"];
            dest = new RectangleF(pos.X, pos.Y+sz.Y-srcSz.Y, srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_BR"];
            dest = new RectangleF(pos.X + sz.X - srcSz.X, pos.Y+sz.Y-srcSz.Y, srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_TM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y, sz.X-2*srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_BM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y+sz.Y-srcSz.Y, sz.X - 2 * srcSz.X, srcSz.Y);
            DrawQuad(0, clr, src, dest, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_ML"];
            dest = new RectangleF(pos.X, pos.Y + srcSz.Y, srcSz.X, sz.Y-2*srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_MR"];
            dest = new RectangleF(pos.X+sz.X-srcSz.X, pos.Y + srcSz.Y, srcSz.X, sz.Y - 2 * srcSz.Y);
            DrawQuad(0, clr, src, dest, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_MM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y + srcSz.Y, sz.X - 2 * srcSz.X, sz.Y - 2 * srcSz.Y);
            DrawQuad(0, clr, src, dest, false);

            Rectangle contentRegion = new Rectangle((int)(pos.X + srcSz.X), (int)(pos.Y + srcSz.Y), (int)(sz.X - 2 * srcSz.X), (int)(sz.Y - 2 * srcSz.Y));
            return contentRegion;
        }
        public Vector2 GetOriginalSize(String image)
        {
            var src = StyleAtlas[image];
            return StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
        }
        public void AddImage(String image, Vector2 pos, Vector4 clr)
        {
            var src = StyleAtlas[image];
            var ogSize = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, ogSize.X, ogSize.Y);
            DrawQuad(0, clr, src, dest, false);
        }

        public void AddScaledImage(String image, Vector2 pos, Vector2 sz, Vector4 clr)
        {
            var src = StyleAtlas[image];
            //var ogSize = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, sz.X, sz.Y);
            DrawQuad(0, clr, src, dest, false);
        }

        Random rng = new Random();
        public void EndVertSet(Rectangle rect)
        {
            rect.Y = Resolution.Height - (rect.Y+rect.Height);

            //GuiShader.EndVertSet(new Rectange(rect.X, Resolution.Height - rect.Y, rect.Width, rect.Height);
            GuiShader.EndVertSet(rect);
        }
        public void Update(Instant Instant, GameState State, GameEnvironment env,Vector2 MousePosition)
        {
            if (!Dirty) return;
            if (GuiBuffer==null)
            {
                guiCam = new OrthographicCamera(Resolution.Width, Resolution.Height, 1000);
                guiCam.Move(new Vector3(Resolution.Width / 2.0f, Resolution.Height / 2.0f, 0));
                GuiBuffer = CreateGuiContextFrameBuffer(env);
            }
            GuiShader.Clear();
            CursorPosition = new Vector2(0, 0);
            foreach(var widget in Widgets)
            {
                widget.Render(this,new Rectangle(0,0, Resolution.Width, Resolution.Height));
            }
            /*
            AddCharacter(CursorPosition, 'A');
            CursorPosition.X += 8;
            AddCharacter(CursorPosition, 'B');

            CursorPosition.X += 8;
            AddScaledElement("Blank", new Vector2(10, 10), new Vector2(300, 300), new Vector4(1, 0, 1, 1));
              AddStyleElement("Default_CloseButton", CursorPosition, new Vector4(0,0,0,1));

            AddFrame("Default", new Vector2(300, 300), new Vector2(100, 100), new Vector4(0, 1, 1, 1));
            AddFrame("Default", new Vector2(400-12, 300), new Vector2(12, 100), new Vector4(0.75f, 0.75f, 0.75f, 1));

            */
            //AddFrame("Basic", new Vector2(700, 400), new Vector2(300, 100), new Vector4(1f, 1f, 1f, 1));
            AddFrame("Curve", new Vector2(700, 400), new Vector2(300, 100), new Vector4(1f, 1f, 1f, 1));
            AddFrame("SciFi", new Vector2(700, 300), new Vector2(300, 100), new Vector4(1f, 1f, 1f, 1));
            AddFrame("SciFiSimple", new Vector2(700, 000), new Vector2(300, 100), new Vector4(1f, 1f, 1f, 1));
            //AddFrame("SciFi", new Vector2(0, 0), new Vector2(400, 500), new Vector4(1f, 1f, 1f, 1));


            //            AddVertical(new Vector2(500, 300), new Vector2(18, 100), new Vector4(1, 1, 0, 1));

            //           AddHorizontal(new Vector2(520, 300), new Vector2(100, 9), new Vector4(1, 1, 0, 1));




            GuiShader.EndVertSet(new Rectangle(0,0, Resolution.Width, Resolution.Height));
            var MouseCursor = (String)this.StyleBook[StyleElement.MouseCursor_Normal];
            var MouseCursorColor = (Vector4)this.StyleBook[StyleElement.MouseCursor_NormalColor];

            AddImage(MouseCursor, MousePosition, MouseCursorColor);
           
            GuiShader.Update(env.Window.Device);
            Dirty = false;
        }
        public void Render(Instant Instant, GameState Stae, GameEnvironment env, CommandList GuiCmds)
        {
            GuiCmds.SetFramebuffer(GuiBuffer);
            GuiCmds.ClearColorTarget(0, RgbaFloat.Clear);
            GuiCmds.ClearDepthStencil(-1);
            GuiShader.Render(GuiBuffer, GuiCmds, null, guiCam.CamMatrix, Matrix4x4.Identity);
            lines.Render(GuiBuffer, GuiCmds, null, guiCam.CamMatrix, Matrix4x4.Identity);
            
        }
        private Framebuffer CreateGuiContextFrameBuffer(GameEnvironment env)
        {
            var device = env.Window.Device;
            var clrBufferDesc = new TextureDescription((uint)Resolution.Width,
                                       (uint)Resolution.Height,
                                       1, 1, 1,
                                       PixelFormat.R8_G8_B8_A8_UNorm,
                                       TextureUsage.RenderTarget | TextureUsage.Sampled,
                                       TextureType.Texture2D,
                                        TextureSampleCount.Count1);
            var clrTexture = device.ResourceFactory.CreateTexture(clrBufferDesc);
            clrTexture.Name = $"GuiContext{ID} Color Texture";

            var depthBufferDesc = new TextureDescription((uint)Resolution.Width,
                                       (uint)Resolution.Height,
                                       1, 1, 1,
                                       PixelFormat.R32_Float,
                                       TextureUsage.DepthStencil,
                                       TextureType.Texture2D,
                                       TextureSampleCount.Count1);
            var depthTexture = device.ResourceFactory.CreateTexture(depthBufferDesc);
            depthTexture.Name = $"GuiContext{ID} Depth Texture";

            FramebufferAttachmentDescription[] clrTrgs = new FramebufferAttachmentDescription[1]
            {
                new FramebufferAttachmentDescription()
                {
                    ArrayLayer=0,
                    MipLevel=0,
                    Target=clrTexture
                }
            };

            FramebufferAttachmentDescription depTrg = new FramebufferAttachmentDescription()
            {
                ArrayLayer = 0,
                MipLevel = 0,
                Target = depthTexture
            };

            var frameBuffDesc = new FramebufferDescription()
            {
                ColorTargets = clrTrgs,
                DepthTarget = depTrg
            };
            var ret = device.ResourceFactory.CreateFramebuffer(frameBuffDesc);
            ret.Name = $"GuiContext{ID} Framebuffer";
            return ret;

        }

        private void DrawQuad(float zIndex, Vector4 color, RectangleF uv, RectangleF pos, bool useFontTexture=false)
        {
            var closeX = pos.X;
            var farX = pos.X + pos.Width;

            var lowY = pos.Y ;
            var highY = pos.Y + pos.Height;

            float fT = useFontTexture ? 1 : 0;

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, fT) });

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, highY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, fT) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });
        }
    }

}
