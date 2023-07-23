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
using SixLabors.ImageSharp.Drawing.Processing;
using BareE.GUI.Widgets;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp.Drawing;

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
        public bool UseSystemMouse;
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


 
        private Vector4 ReadColor(AttributeCollection from, String attribute)
        {
            var o = from[attribute];
            if (o == null) return new Vector4(0, 0, 0, 0);
            if (o is Vector4) return (Vector4)o;
            if (o is String)
                return Helper.DecodeColor((String)o);
            return new Vector4(0, 0, 0, 0);

        }
        private void ImportDefaultStyle(GraphicsDevice device)
        {
            var style = AttributeCollectionDeserializer.FromAsset(@"BareE.GUI.Assets.Styles.MinGui.MinGuiStyle.style");

            Image<Rgba32> blank = new Image<Rgba32>(5, 5);
            blank.Mutate(x => x.Clear(Color.White));
            StyleAtlas.Merge($"Blank", blank);

            ImportStyle(device, "Default", style);
            
        }
        public void ImportStyle(GraphicsDevice device, String styleName, AttributeCollection style)
        {
            ImportFrame($"{styleName}_TitleFrame", device, (String)style["TitleFrame"]);
            ImportFrame($"{styleName}_InnerFrame", device, (String)style["InnerFrame"]);
            ImportBanner($"{styleName}_Banner", device, (String)style["Banner"]);

            StyleAtlas.Merge($"{styleName}_CloseButton", AssetManager.GetImage((String)style["CloseButton_Normal"]));
            StyleAtlas.Merge($"{styleName}_MouseNormal", AssetManager.GetImage((String)style["MouseCursor_Normal"]));
            StyleAtlas.Merge($"{styleName}_MouseDown", AssetManager.GetImage((String)style["MouseCursor_Down"]));

            StyleAtlas.Merge($"{styleName}_ScrollTop", AssetManager.GetImage((String)style["Scroll_Top"]));
            StyleAtlas.Merge($"{styleName}_ScrollBottom", AssetManager.GetImage((String)style["Scroll_Bottom"]));
            StyleAtlas.Merge($"{styleName}_ScrollLeft", AssetManager.GetImage((String)style["Scroll_Left"]));
            StyleAtlas.Merge($"{styleName}_ScrollRight", AssetManager.GetImage((String)style["Scroll_Right"]));

            StyleAtlas.Merge($"{styleName}_ScrollHorizontal", AssetManager.GetImage((String)style["Scroll_Horizontal"]));
            StyleAtlas.Merge($"{styleName}_ScrollVertical", AssetManager.GetImage((String)style["Scroll_Vertical"]));
            StyleAtlas.Merge($"{styleName}_ScrollHorizontalKnob", AssetManager.GetImage((String)style["Scroll_VerticalKnob"]));
            StyleAtlas.Merge($"{styleName}_ScrollVerticalKnob", AssetManager.GetImage((String)style["Scroll_HorizontalKnob"]));

            StyleAtlas.Merge($"{styleName}_ResizeButton", AssetManager.GetImage((String)style["ResizeButton_Normal"]));

            StyleAtlas.Build(0, false);
            GuiShader.SetStyleTexture(device, AssetManager.LoadTexture(StyleAtlas.AtlasSheet, device, false, false));

            var defaultStyle = new StyleDefinition();
            defaultStyle.Add(StyleElement.TitleFrame, $"{styleName}_TitleFrame");
            
            defaultStyle.Add(StyleElement.TitleFrameColor, ReadColor(style, "TitleFrameColor"));
            defaultStyle.Add(StyleElement.TitleMarginHorizontal, (int)style["TitleFrameMarginHorizontal"]);
            defaultStyle.Add(StyleElement.TitleMarginVertical, (int)style["TitleFrameMarginVertical"]);
            

            defaultStyle.Add(StyleElement.Frame, $"{styleName}_InnerFrame");
            defaultStyle.Add(StyleElement.FrameColor, ReadColor(style, "InnerFrameColor"));
            defaultStyle.Add(StyleElement.MarginHorizontal, (int)style["InnerFrameMarginHorizontal"]);
            defaultStyle.Add(StyleElement.MarginVertical, (int)style["InnerFrameMarginVertical"]);

            defaultStyle.Add(StyleElement.Font, $"{style["Font"]}");
            defaultStyle.Add(StyleElement.FontSize, (int)style["FontSize"]);
            defaultStyle.Add(StyleElement.FontColor, ReadColor(style, "FontColor"));

            defaultStyle.Add(StyleElement.MouseCursor_Normal, $"{styleName}_MouseNormal");
            defaultStyle.Add(StyleElement.MouseCursor_NormalColor, ReadColor(style, "MouseCursor_NormalColor"));
            defaultStyle.Add(StyleElement.MouseCursor_Down, $"{styleName}_MouseDown");
            defaultStyle.Add(StyleElement.MouseCursor_DownColor, ReadColor(style, "MouseCursor_DownColor"));


            defaultStyle.Add(StyleElement.CloseButton_Normal, $"{styleName}_CloseButton");
            defaultStyle.Add(StyleElement.CloseButton_Hover, $"{styleName}_CloseButton");
            defaultStyle.Add(StyleElement.CloseButton_Down, $"{styleName}_CloseButton");

            defaultStyle.Add(StyleElement.ResizeButton_Normal, $"{styleName}_ResizeButton");
            defaultStyle.Add(StyleElement.ResizeButton_NormalColor, ReadColor(style, "ResizeButton_NormalColor"));


            defaultStyle.Add(StyleElement.Scroll_Bottom, $"{styleName}_ScrollBottom");
            defaultStyle.Add(StyleElement.Scroll_BottomColor, ReadColor(style, "Scroll_BottomColor"));
            defaultStyle.Add(StyleElement.Scroll_Top, $"{styleName}_ScrollTop");
            defaultStyle.Add(StyleElement.Scroll_TopColor, ReadColor(style, "Scroll_TopColor"));
            defaultStyle.Add(StyleElement.Scroll_Left, $"{styleName}_ScrollLeft");
            defaultStyle.Add(StyleElement.Scroll_LeftColor, ReadColor(style, "Scroll_LeftColor"));
            defaultStyle.Add(StyleElement.Scroll_Right, $"{styleName}_ScrollRight");
            defaultStyle.Add(StyleElement.Scroll_RightColor, ReadColor(style, "Scroll_RightColor"));
            defaultStyle.Add(StyleElement.Scroll_Horizontal, $"{styleName}_ScrollHorizontal");
            defaultStyle.Add(StyleElement.Scroll_HorizontalColor, ReadColor(style, "Scroll_HorizontalColor"));
            defaultStyle.Add(StyleElement.Scroll_Vertical, $"{styleName}_ScrollVertical");
            defaultStyle.Add(StyleElement.Scroll_VerticalColor, ReadColor(style, "Scroll_VerticalColor"));
            defaultStyle.Add(StyleElement.Scroll_HorizontalKnob, $"{styleName}_ScrollHorizontalKnob");
            defaultStyle.Add(StyleElement.Scroll_HorizontalKnobColor, ReadColor(style, "Scroll_HorizontalKnobColor"));
            defaultStyle.Add(StyleElement.Scroll_VerticalKnob, $"{styleName}_ScrollVerticalKnob");
            defaultStyle.Add(StyleElement.Scroll_VerticalKnobColor, ReadColor(style, "Scroll_VerticalKnobColor"));

            defaultStyle.Add(StyleElement.CloseButton_NormalColor, ReadColor(style, "CloseButton_NormalColor"));
            defaultStyle.Add(StyleElement.CloseButton_HoverColor, ReadColor(style, "CloseButton_HoverColor"));
            defaultStyle.Add(StyleElement.CloseButton_DownColor, ReadColor(style, "CloseButton_CloseColor"));

            defaultStyle.Add(StyleElement.MouseCursor_OffsetX, (int)style["MouseCursor_OffsetX"]);
            defaultStyle.Add(StyleElement.MouseCursor_OffsetY, (int)style["MouseCursor_OffsetY"]);

            if (StyleBook==null)
                StyleBook = new StyleBook();
            StyleBook.DefineStyle($"{styleName}", defaultStyle);
        }

        public void ImportFont(String fontName, GraphicsDevice device,String ttfFileName, params int[] fontSizes)
        {
            var bgColor = Color.Transparent;
            var fgColor = Color.White;
            FontCollection collection = new FontCollection();
            var fam =collection.Add(ttfFileName);
            foreach (var fontsizeInPts in fontSizes)
            {
                var currentFont = new Font(fam, fontsizeInPts);
                var fontKey = $"{fontName}_{fontsizeInPts}";
                if (FontLineHeights.ContainsKey(fontKey)) 
                    continue;
                var lineHeight = 0;
                //foreach (var ch in Alphabet)
                for(int i=0;i<16*16;i++)
                {
                    CodePoint cp = new CodePoint(i);
                    Image<Rgba32> charImage = GenerateImage(((char)i).ToString(), fontsizeInPts, currentFont);
                    FontAtlas.Merge($"{fontKey}_{i}", charImage);
                    if (lineHeight < charImage.Height)
                        lineHeight = charImage.Height;
                    //TODO: Do it right.
                    FontAdvance[$"{fontKey}_{i}"] = charImage.Width;
                }
                //TODO: Do it right.
                FontLineHeights[$"{fontName}_{fontsizeInPts}"] = lineHeight;
            }
            FontAtlas.Build(1024, @"C:\TestData\afont.png");
            GuiShader.SetFontTexture(device, AssetManager.LoadTexture(FontAtlas.AtlasSheet, device, true, false));
        }
        private Image<Rgba32> GenerateImage(string text, int pt, Font font)
        {
            var glyphs = TextBuilder.GenerateGlyphs(text, new TextOptions(font));
            var bounds = TextMeasurer.Measure("M", new TextOptions(font));
            var img = new Image<Rgba32>((int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
            img.Mutate(i => i.Fill(new DrawingOptions() { GraphicsOptions = new GraphicsOptions() { Antialias = true } }, Color.White, glyphs));
            return img;
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
            int dY = (int)Math.Floor(img.Height / 3.0f);
            var inX = img.Width - 2 * dX;
            int inY = img.Height - 2 * dY;

            StyleAtlas.Merge($"{frameStyleName}_Frame_TL", img.Clone(), new Vector4(0, 0, dX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_TM", img.Clone(), new Vector4(dX, 0, inX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_TR", img.Clone(), new Vector4(dX + inX, 0, dX, dY));

            StyleAtlas.Merge($"{frameStyleName}_Frame_ML", img.Clone(), new Vector4(0, dY, dX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_MM", img.Clone(), new Vector4(dX, dY, inX, inY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_MR", img.Clone(), new Vector4(dX + inX, dY, dX, dY));


            StyleAtlas.Merge($"{frameStyleName}_Frame_BL", img.Clone(), new Vector4(0, dY + inY, dX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_BM", img.Clone(), new Vector4(dX, dX + inY, inX, dY));
            StyleAtlas.Merge($"{frameStyleName}_Frame_BR", img.Clone(), new Vector4(dX + inX, dY + inY, dX, dY));


            StyleAtlas.Build(0, false);
            GuiShader.SetStyleTexture(device, AssetManager.LoadTexture(StyleAtlas.AtlasSheet, device, false, false));
        }
        public void ImportBanner(String bannerName, GraphicsDevice device, String imageSource)
        {
            var img = AssetManager.GetImage(imageSource);
            int dX = (int)Math.Floor(img.Width / 3.0f);
            int dY = (int)img.Height;
            var inX = img.Width - 2 * dX;
            int inY = img.Height - 2 * dY;

            StyleAtlas.Merge($"{bannerName}_Banner_L", img.Clone(), new Vector4(0, 0, dX, dY));
            StyleAtlas.Merge($"{bannerName}_Banner_M", img.Clone(), new Vector4(dX, 0, inX, dY));
            StyleAtlas.Merge($"{bannerName}_Banner_R", img.Clone(), new Vector4(dX + inX, 0, dX, dY));

            StyleAtlas.Build(0, false);
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

        public Rectangle AddTextBlock(Rectangle textArea, int zIndex, String Text, String font, int fontSize, Vector4 fontColor, bool addToPass = true)
        {
            return AddTextBlock(textArea, textArea, zIndex, Text, font, fontSize, fontColor, addToPass);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textArea">The designspace allotted the textbox</param>
        /// <param name="viewArea">The Screen space allotted the textbox</param>
        /// <param name="zIndex"></param>
        /// <param name="Text"></param>
        /// <param name="font"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontColor"></param>
        /// <param name="addToPass"></param>
        /// <returns></returns>
        public Rectangle AddTextBlock(Rectangle textArea, Rectangle viewArea, int zIndex, String Text, String font, int fontSize, Vector4 fontColor, bool addToPass=true)
        {
            if (String.IsNullOrEmpty(Text)) Text = String.Empty;
            
            int cX = textArea.X;
            int cY = textArea.Y;
            
            var lineHeight = FontLineHeights[$"{font}_{fontSize}"];
            foreach(var ch in Text)
            {
                var chId = (int)ch;
                var key = $"{font}_{fontSize}_{chId}";
                if (!FontAdvance.ContainsKey(key))
                {
                    key = $"{font}_{fontSize}_0";
                }
                var adv = FontAdvance[key];
                if (cX+adv> textArea.X+textArea.Width)
                {
                    cX = textArea.X;
                    cY += lineHeight;
                }
                cX += adv;
                
                if (cX < viewArea.X) continue;
                if (cY < viewArea.Y-lineHeight/2.0f) continue;
                if (cX > viewArea.X + viewArea.Width) continue;
                if (cY > viewArea.Y + viewArea.Height)
                {
                    if (textArea.Height != 0) return textArea;
                    continue;
                }
                
                AddCharacter(new Vector2(cX, cY), ch,font,fontSize, fontColor, addToPass);
            }
            if (textArea.Height == 0) textArea.Height = cY-textArea.Y;
            return textArea;

        }
        public void AddCharacter(Vector2 pos, char ch, String font, int fontSize, Vector4 clr, bool addToPass=true)
        {
            var chId = (int)ch;
            var key = $"{font}_{fontSize}_{chId}";
            var src = FontAtlas[key];
            var ogSize = FontAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, ogSize.X, ogSize.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, true);
        }
    
        public Rectangle AddBanner(String frameStyleName, Vector2 pos, int Width, Vector4 clr, bool addToPass=true)
        {
            var src = StyleAtlas[$"{frameStyleName}_Frame_TL"];
            var srcSz = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_TR"];
            dest = new RectangleF(pos.X + Width - srcSz.X, pos.Y, srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_BL"];
            dest = new RectangleF(pos.X, pos.Y + 2*srcSz.Y - srcSz.Y, srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_BR"];
            dest = new RectangleF(pos.X + Width - srcSz.X, pos.Y + 2*srcSz.Y - srcSz.Y, srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_TM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y, Width - 2 * srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_BM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y + 2*srcSz.Y - srcSz.Y, Width- 2 * srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);
            return new Rectangle((int)pos.X, (int)pos.Y, Width, (int)(2 * srcSz.Y));

        }
        public Rectangle AddFrame(String frameStyleName, Vector2 pos, Vector2 sz, Vector4 clr, bool addToPass = true)
        {
            var src = StyleAtlas[$"{frameStyleName}_Frame_TL"];
            var srcSz = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);

            src= StyleAtlas[$"{frameStyleName}_Frame_TR"];
            dest = new RectangleF(pos.X + sz.X - srcSz.X, pos.Y, srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_BL"];
            dest = new RectangleF(pos.X, pos.Y+sz.Y-srcSz.Y, srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_BR"];
            dest = new RectangleF(pos.X + sz.X - srcSz.X, pos.Y+sz.Y-srcSz.Y, srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_TM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y, sz.X-2*srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_BM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y+sz.Y-srcSz.Y, sz.X - 2 * srcSz.X, srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_ML"];
            dest = new RectangleF(pos.X, pos.Y + srcSz.Y, srcSz.X, sz.Y-2*srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);

            src = StyleAtlas[$"{frameStyleName}_Frame_MR"];
            dest = new RectangleF(pos.X+sz.X-srcSz.X, pos.Y + srcSz.Y, srcSz.X, sz.Y - 2 * srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);


            src = StyleAtlas[$"{frameStyleName}_Frame_MM"];
            dest = new RectangleF(pos.X + srcSz.X, pos.Y + srcSz.Y, sz.X - 2 * srcSz.X, sz.Y - 2 * srcSz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);

            Rectangle contentRegion = new Rectangle((int)(pos.X), (int)(pos.Y), (int)(sz.X), (int)(sz.Y));
            return contentRegion;
        }
        public Vector2 GetOriginalSize(String image)
        {
            var src = StyleAtlas[image];
            return StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
        }
        public int GetLineHeight(string font, int fontSize)
        {
            return FontLineHeights[$"{font}_{fontSize}"];
        }
        public void AddImage(String image, Vector2 pos, Vector4 clr, bool addToPass = true)
        {
            var src = StyleAtlas[image];
            var ogSize = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, ogSize.X, ogSize.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);
        }


        public Rectangle AddScaledImage(String image, Vector2 pos, Vector2 sz, Vector4 clr, bool addToPass = true)
        {
            var src = StyleAtlas[image];
            //var ogSize = StyleAtlas.EstimateOriginalSize(src.Width, src.Height);
            var dest = new RectangleF(pos.X, pos.Y, sz.X, sz.Y);
            DrawPassQuad(0, clr, src, dest, addToPass, false);
            return (Rectangle)dest;
        }

        Random rng = new Random();
        public void EndVertSet(Rectangle rect, bool forPass=true)
        {
            rect.Y = Resolution.Height - (rect.Y+rect.Height);

            //GuiShader.EndVertSet(new Rectange(rect.X, Resolution.Height - rect.Y, rect.Width, rect.Height);
            if (forPass)
                GuiShader.EndPassVertSet(rect);
            else
                GuiShader.EndVertSet(rect);
            
        }

        int mZIndx = 1;

        WidgetBase GetHoveredControl(WidgetBase window, Vector2 MousePosition)
        {
            
            if (window==null || window.Children == null) return window;
            MousePosition -= window.Position;
            
            foreach (var widget in window.Children.OrderBy(x => x.ZIndex))
            {
                if (MousePosition.X < widget.Position.X || MousePosition.Y < widget.Position.Y)
                    continue;
                if (MousePosition.X > widget.Position.X + widget.Size.X || MousePosition.Y > widget.Position.Y + widget.Size.Y)
                    continue;
                var r = GetHoveredControl(widget, MousePosition);
                if (r != null) return r;
            }
            return window;
        }
        WidgetBase GetHoveredWindow(Vector2 MousePosition)
        {
            if (MousePosition.X < 0 || MousePosition.Y < 0) return null;
            if (MousePosition.X > Resolution.Width || MousePosition.Y > Resolution.Height) return null;
            foreach(var widget in Widgets.OrderByDescending(x=>x.ZIndex))
            {
                if (MousePosition.X < widget.Position.X || MousePosition.Y < widget.Position.Y)
                    continue;
                if (MousePosition.X > widget.Position.X + widget.Size.X || MousePosition.Y > widget.Position.Y + widget.Size.Y)
                    continue;
                return widget;
            }
            return null;
        }

        WidgetBase FocusedControl;

        

        /// <summary>
        /// MousePosition is passed in to account for GUI when the context does not cover the entire screen.
        /// So we can't use ImGUI for mouse position. We can assume that if the mouse position is within the resolution
        /// of the GUI and that ImGUI isn't attempting to capture the mouse then the other input states such as mouse down/up
        /// and keyboard input is for us.
        /// </summary>
        /// <param name="Instant"></param>
        /// <param name="State"></param>
        /// <param name="env"></param>
        /// <param name="MousePosition"></param>
        public void Update(Instant Instant, GameState State, GameEnvironment env,Vector2 MousePosition)
        {
            var MouseCursor_Normal = (String)this.StyleBook[StyleElement.MouseCursor_Normal];
            var MouseCursor_Down = (String)this.StyleBook[StyleElement.MouseCursor_Down];
            var MouseCursorColor = (Vector4)this.StyleBook[StyleElement.MouseCursor_NormalColor];
            var MouseOffSet = new Vector2((int)StyleBook[StyleElement.MouseCursor_OffsetX], (int)StyleBook[StyleElement.MouseCursor_OffsetY]);
            bool IsMouseDown = ImGuiNET.ImGui.IsMouseDown(ImGuiNET.ImGuiMouseButton.Left);
            SDL_MouseButton btn;
            
            if (FocusedControl!=null)
                FocusedControl.OnMouseMoved(new SDL_MouseMotionEvent() {  x=(int)MousePosition.X, y=(int)MousePosition.Y, type = SDL_EventType.MouseMotion });
            
            GuiShader.Clear();
            if (GuiBuffer==null)
            {
                guiCam = new OrthographicCamera(Resolution.Width, Resolution.Height, 1000);
                guiCam.Move(new Vector3(Resolution.Width / 2.0f, Resolution.Height / 2.0f, 0));
                GuiBuffer = CreateGuiContextFrameBuffer(env);
                foreach (var v in Widgets)
                    v.Dirty = true;
            }
            Rectangle contentRegion = new Rectangle(0, 0, Resolution.Width, Resolution.Height);
            foreach (var widget in Widgets)
            {

                if (widget.Dirty)
                {
                    if (widget.StyleDirty)
                        widget.ReadStyle(this);
                    widget.Render(this, contentRegion, Vector2.Zero);
                }
                widget.DynamicRender(this, contentRegion, Vector2.Zero);
            }
            bool showCursor = (this.UseSystemMouse | !(ImGuiNET.ImGui.GetIO().WantCaptureMouse));
            Veldrid.Sdl2.Sdl2Native.SDL_ShowCursor(showCursor ? 0 : 1);
            if (showCursor)
            {
                if (IsMouseDown)
                {
                    AddImage(MouseCursor_Down, MousePosition + MouseOffSet, MouseCursorColor, false);
                }
                else
                    AddImage(MouseCursor_Normal, MousePosition + MouseOffSet, MouseCursorColor, false);
                GuiShader.EndVertSet(new Rectangle(0, 0, Resolution.Width, Resolution.Height));
            }

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

        private void DrawPassQuad(float zIndex, Vector4 color, RectangleF uv, RectangleF pos,bool addToPass, bool useFontTexture)
        {
            var closeX = pos.X;
            var farX = pos.X + pos.Width;

            var lowY = pos.Y ;
            var highY = pos.Y + pos.Height;

            float fT = useFontTexture ? 1 : 0;
            if (addToPass)
            {
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(farX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, fT) });

                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, highY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, fT) });
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });
            }
            else
            {
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, fT) });

                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, highY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, fT) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });

            }
        }
        /*
        private void DrawQuad(float zIndex, Vector4 color, RectangleF uv, RectangleF pos, bool useFontTexture = false)
        {
            var closeX = pos.X;
            var farX = pos.X + pos.Width;

            var lowY = pos.Y;
            var highY = pos.Y + pos.Height;

            float fT = useFontTexture ? 1 : 0;

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, fT) });

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, highY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, fT) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });
        }
        */
        internal void BeginPass(string v,int zIndex, Texture value)
        {
            GuiShader.BeginPass($"{v}",zIndex, value);
        }


        public WidgetBase CreateWidget(GraphicsDevice device, AttributeCollection def)
        {
            if (def.HasAttribute("Style"))
            {
                var styleDef = (AttributeCollection)def["Style"];
                ImportStyle(device, (String)styleDef["StyleName"] ?? "Default", styleDef);
            }
            if (def.HasAttribute("Gui"))
                def = (AttributeCollection)def["Gui"];
            return WidgetFactory.CreateWidget(def);
        }
        public WidgetBase CreateWindow(GraphicsDevice device, String asset)
        {
            var def = AttributeCollectionDeserializer.FromAsset(asset);
            return CreateWidget(device, def);
        }
        #region HandleEvents
        public bool HandleMouseWheelEvent(SDL_MouseWheelEvent e)
        {
            if (FocusedControl != null)
            {
                FocusedControl.OnMouseWheelMoved(e);
                return true;
            }
            return false;
        }

        public bool HandleMouseButtonEvent(SDL_MouseButtonEvent mouseButtonEvent)
        {
            Vector2 MousePosition = new Vector2(mouseButtonEvent.x, mouseButtonEvent.y);
            var hoveredWindow = GetHoveredWindow(MousePosition);
            WidgetBase hoveredControl = GetHoveredControl(hoveredWindow, MousePosition);
            if (FocusedControl != null && hoveredControl != null && FocusedControl.ID == hoveredControl.ID)
            {
                FocusedControl.OnMouseButtonEvent(mouseButtonEvent);
                return true;
            }
            else
            {
                if (FocusedControl != null)
                    FocusedControl.OnLostFocus();

                FocusedControl = hoveredControl;
                if (FocusedControl != null)
                {
                    // _focusedControlPositionStart = FocusedControl.Position;
                    // _focusedControlSizeStart = FocusedControl.Size;
                    FocusedControl.ZIndex = ++mZIndx;
                    FocusedControl.OnFocused();
                    FocusedControl.OnMouseButtonEvent(mouseButtonEvent);
                    FocusedControl.Dirty = true;
                    return true;
                }
            }
            return false;
        }
        public bool HandleMouseMoveEvent(SDL_MouseMotionEvent mouseMoveEvent)
        {
            if (FocusedControl != null)
            {
                FocusedControl.OnMouseMoved(mouseMoveEvent);
                return true;
            }
            return false;
        }
        #endregion

        internal void AddQuad(Rectangle childContentRegion, int zIndex, Vector4 color, bool addToPass=true)
        {
            var closeX = childContentRegion.X;
            var farX = childContentRegion.X + childContentRegion.Width;
            var lowY = childContentRegion.Y;
            var highY = childContentRegion.Y + childContentRegion.Height;
            var uv = StyleAtlas["Blank"];
            float fT = 0;
            if (addToPass)
            {
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(farX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, fT) });

                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, fT) });
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, highY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, fT) });
                GuiShader.AddPassVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, fT) });

            }
            else
            {
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(0, 0, fT) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(0, 0, fT) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, lowY, zIndex), Color = color, UvT = new Vector3(0, 0, fT) });

                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(0, 0, fT) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, highY, zIndex), Color = color, UvT = new Vector3(0, 0, fT) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(0, 0, fT) });
            }
        }


    }

}
