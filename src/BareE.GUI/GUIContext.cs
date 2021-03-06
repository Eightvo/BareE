using BareE.DataStructures;
using BareE.GameDev;
using BareE.GUI.TextRendering;
using BareE.GUI.Widgets;
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

using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
using Size = System.Drawing.Size;
using Veldrid.Sdl2;
using System.Linq;

namespace BareE.GUI
{
    public class GUIContext
    {

        Camera guiCam;

        FullScreenTexture GUICanvas;
        BaseGuiShader GuiShader;
        Framebuffer GuiBuffer;
        Texture resolvedTexture;
        public Image Canvas;
        public Size Resolution { get { return new Size(Canvas.Width, Canvas.Height); } }

        Dictionary<String, GuiWidgetBase> Widgets { get; set; } = new Dictionary<String, GuiWidgetBase>(StringComparer.InvariantCultureIgnoreCase);

        SpriteAtlas FontAtlas = new SpriteAtlas();
        Dictionary<String, FontData> FontLib = new Dictionary<string, FontData>(StringComparer.CurrentCultureIgnoreCase);

        public StyleTree Styles { get; set; } = new StyleTree();
        SpriteAtlas StyleAtlas = new SpriteAtlas();
        Dictionary<String, Dictionary<String, SpriteModel>> SpriteModels = new Dictionary<string, Dictionary<string, SpriteModel>>(StringComparer.CurrentCultureIgnoreCase);
        Dictionary<String, Image<Rgba32>> Images = new Dictionary<string, Image<Rgba32>>(StringComparer.CurrentCultureIgnoreCase);

        private Vector2 _nextResolution;
        private bool _setNextResolution = false;

        private int maxGuiDepth = 100;
        TextureSampleCount GuiBufferTextureCount = TextureSampleCount.Count1;
        public bool UseSystemMouseCursor { get; set; } = false;

        private bool _isMouseDown=false;
        private bool _rebuildStyleAtlas = true;
        private bool _rebuildFontAtlas = true;

        public GUIContext(Size size)
        {
            Canvas = new SixLabors.ImageSharp.Image<Rgba32>(size.Width, size.Height);

        }

        internal void Load(Instant instant, GameState state, GameEnvironment env)
        {

            guiCam = new OrthographicCamera(Resolution.Width, Resolution.Height, maxGuiDepth);
            guiCam.Move(new Vector3(Resolution.Width / 2.0f, Resolution.Height / 2.0f, 0.0f));

            Veldrid.RenderDoc.Load(out env.rd);
            env.rd.SetCaptureSavePath(@"C:\TestData\RenderDocOutput\");

            GUICanvas = new FullScreenTexture();
            GUICanvas.SetOutputDescription(env.HUDBackBuffer.OutputDescription);
            GUICanvas.Flip = true;
            GUICanvas.CreateResources(env.Window.Device);
            resolvedTexture = env.Window.Device.ResourceFactory.CreateTexture(
                new TextureDescription((uint)Resolution.Width, (uint)Resolution.Height,
                                       1, 1, 1,
                                       PixelFormat.R8_G8_B8_A8_UNorm,
                                       TextureUsage.Sampled,
                                       TextureType.Texture2D,
                                       TextureSampleCount.Count1)
            );

            GuiBuffer = UTIL.Util.CreateFramebuffer(env.Window.Device, (uint)Resolution.Width, (uint)Resolution.Height, PixelFormat.R8_G8_B8_A8_UNorm, GuiBufferTextureCount);
            //GUICanvas.SetTexture(env.Window.Device, GuiBuffer.ColorTargets[0].Target);
            GUICanvas.SetTexture(env.Window.Device, GuiBuffer.ColorTargets[0].Target);
            GUICanvas.Update(env.Window.Device);

            //StyleAtlas = new SpriteAtlas();
            /*
            var minGuiSpriteModels = SpriteModel.LoadSpriteModelsFromSrc(AssetManager.ReadFile(@"BareE.GUI.Assets.Styles.MinGui.MinGui.Atlas"));
            StyleAtlas.Merge("MinGui:squareFrame", minGuiSpriteModels["NineFrame"], @"BareE.GUI.Assets.Styles.MinGui.squareFrame.png");
            StyleAtlas.Merge("MinGui:R2Frame", minGuiSpriteModels["NineFrame"], @"BareE.GUI.Assets.Styles.MinGui.R2Frame.png");
            StyleAtlas.Merge("MinGui:R2FrameV", minGuiSpriteModels["VerticalThreeFrame"], @"BareE.GUI.Assets.Styles.MinGui.R2Frame.png");
            StyleAtlas.Merge("MinGui:R2FrameH", minGuiSpriteModels["HorizontalThreeFrame"], @"BareE.GUI.Assets.Styles.MinGui.R2Frame.png");
            StyleAtlas.Merge("MinGui:CloseIcon", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.close.png");
            StyleAtlas.Merge("MinGui:ExpandIcon", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.Expand.png");
            StyleAtlas.Merge("MinGui:CollapseIcon", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.Collapse.png");
            StyleAtlas.Merge("MinGui:CheckBoxEmpty", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.OvalEmpty.png");
            StyleAtlas.Merge("MinGui:CheckBoxChecked", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.Oval_Filled.png");
            StyleAtlas.Merge("MinGui:ResizeIcon", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.resize.png");
            StyleAtlas.Merge("MinGui:ArrowRight", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.Right.png");
            StyleAtlas.Merge("MinGui:ArrowDown", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.Down.png");
            StyleAtlas.Merge("MinGui:ArrowLeft", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.Left.png");
            StyleAtlas.Merge("MinGui:ArrowUp", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.MinGui.Up.png");


            var SimpleRPGSpriteModels = SpriteModel.LoadSpriteModelsFromSrc(AssetManager.ReadFile(@"BareE.GUI.Assets.Styles.SimpleRPG.SimpleRpg.Atlas"));
            StyleAtlas.Merge("SimpleRPG:GemFrame", SimpleRPGSpriteModels["NineFrame"], @"BareE.GUI.Assets.Styles.SimpleRPG.GemFrame.png");
            StyleAtlas.Merge("SimpleRPG:BasicFrame"      , SimpleRPGSpriteModels["NineFrame"], @"BareE.GUI.Assets.Styles.SimpleRPG.BasicFrame.png");
            StyleAtlas.Merge("SimpleRPG:BasicFrameV", SimpleRPGSpriteModels["VerticalThreeFrame"], @"BareE.GUI.Assets.Styles.SimpleRPG.BasicFrame.png");
            StyleAtlas.Merge("SimpleRPG:BasicFrameH", SimpleRPGSpriteModels["HorizontalThreeFrame"], @"BareE.GUI.Assets.Styles.SimpleRPG.BasicFrame.png");
            StyleAtlas.Merge("SimpleRPG:CloseIcon", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.CloseIcon.png");
            StyleAtlas.Merge("SimpleRPG:ExpandIcon", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.ExpandIcon.png");
            StyleAtlas.Merge("SimpleRPG:CollapseIcon", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.CollapseIcon.png");
//            StyleAtlas.Merge("SimpleRPG.CheckBoxEmpty" , SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.OvalEmpty.png");
//           StyleAtlas.Merge("SimpleRPG.CheckBoxChecked", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.Oval_Filled.png");
            StyleAtlas.Merge("SimpleRPG:ResizeIcon", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.ResizeIcon.png");
            StyleAtlas.Merge("SimpleRPG:ArrowRight", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.ArrowRight.png");
            StyleAtlas.Merge("SimpleRPG:ArrowDown", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.ArrowDown.png");
            StyleAtlas.Merge("SimpleRPG:ArrowLeft", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.ArrowLeft.png");
            StyleAtlas.Merge("SimpleRPG:ArrowUp", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.ArrowUp.png");
            StyleAtlas.Merge("SimpleRPG:GemIcon", SimpleRPGSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.SimpleRPG.GemIcon.png");

            StyleAtlas.Merge("KennyGUI:GoldSword", AssetManager.GetImage(@"BareE.GUI.Assets.Styles.KennyUI.UIpackSheet_transparent.png", new Veldrid.Rectangle(198, 450, 15, 15)));
            StyleAtlas.Merge("KennyGUI:SilverSword", AssetManager.GetImage(@"BareE.GUI.Assets.Styles.KennyUI.UIpackSheet_transparent.png", new Veldrid.Rectangle(180, 450, 15, 15)));

            StyleAtlas.Build(0, null);


            FontAtlas = new SpriteAtlas();
            LoadFont(@"BareE.Harness.Assets.Fonts.Bitmap.Cookie.Cookie.bff");
            LoadFont(@"BareE.Harness.Assets.Fonts.Bitmap.Neuton.Neuton.bff");
            //var spriteModels = SpriteModel.LoadSpriteModelsFromSrc(AssetManager.ReadFile(@"BareE.Harness.Assets.Fonts.Bitmap.Cookie.Cookie.atlas"));
            //FontAtlas.Merge("Cookie", spriteModels["Cookie"], @"BareE.Harness.Assets.Fonts.Bitmap.Cookie.Cookie.png");

            //spriteModels = SpriteModel.LoadSpriteModelsFromSrc(AssetManager.ReadFile(@"BareE.Harness.Assets.Fonts.Bitmap.Neuton.Neuton.atlas"));
            //FontAtlas.Merge("Neuton", spriteModels["Neuton"], @"BareE.Harness.Assets.Fonts.Bitmap.Neuton.Neuton.png");

            if (FontAtlas.Dirty)
                FontAtlas.Build(0, null);

            */
            GuiShader = new BaseGuiShader();
            GuiShader.SetOutputDescription(GuiBuffer.OutputDescription);
            GuiShader.DepthStencilDescription = new DepthStencilStateDescription()
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.GreaterEqual,

            };
            GuiShader.CreateResources(env.Window.Device);
            GuiShader.resolution = new Vector2(900, 900);
            GuiShader.mousepos = new Vector2(30, 540);

            GuiShader.Update(env.Window.Device);

        }
        
        internal void Update(Instant instant, GameState state, GameEnvironment env)
        {
            if (_rebuildFontAtlas)
            {
                FontAtlas.Build(0, null);
                GuiShader.SetFontTexture(env.Window.Device, AssetManager.LoadTexture(FontAtlas.AtlasSheet, env.Window.Device));
                _rebuildFontAtlas = false;
            }
            if (_rebuildStyleAtlas)
            {
                StyleAtlas.Build(0,null);
                GuiShader.SetStyleTexture(env.Window.Device, AssetManager.LoadTexture(StyleAtlas.AtlasSheet, env.Window.Device));
                _rebuildStyleAtlas = false;
            }
            
            

            var mPos = ImGuiNET.ImGui.GetIO().MousePos;
            //ImGuiNET.ImGui.GetIO().
            bool showCursor = (this.UseSystemMouseCursor | !(ImGuiNET.ImGui.GetIO().WantCaptureMouse));
            Veldrid.Sdl2.Sdl2Native.SDL_ShowCursor(showCursor ? 0 : 1);
            if (_setNextResolution)
            {
                Canvas = new SixLabors.ImageSharp.Image<Rgba32>((int)_nextResolution.X, (int)_nextResolution.Y);
                GuiBuffer.Dispose();
                GuiBuffer = UTIL.Util.CreateFramebuffer(env.Window.Device, (uint)Resolution.Width, (uint)Resolution.Height, PixelFormat.R8_G8_B8_A8_UNorm, GuiBufferTextureCount);
                resolvedTexture.Dispose();
                resolvedTexture = env.Window.Device.ResourceFactory.CreateTexture(
                new TextureDescription((uint)_nextResolution.X, (uint)_nextResolution.Y,
                                       1, 1, 1,
                                       PixelFormat.R8_G8_B8_A8_UNorm,
                                       TextureUsage.Sampled,
                                       TextureType.Texture2D,
                                       TextureSampleCount.Count1));

                GUICanvas.SetTexture(env.Window.Device, GuiBuffer.ColorTargets[0].Target);
                GUICanvas.Update(env.Window.Device);
                _setNextResolution = false;
            }


            GuiShader.Clear();
            foreach(var widget in Widgets.OrderBy(x=>x.Value.ZIndex))
            {
                widget.Value.Render(instant, state, env, this, new RectangleF(0,0,Resolution.Width, Resolution.Height));
            }
            /*
            var uv = StyleAtlas[$"MinGui:R2Frame"];

            DrawNineFrame($"MinGui:squareFrame", new RectangleF(100, 298, 200, 32), -0.01f, ((Vector4)Color.CornflowerBlue));
            DrawString("MinGui:Title", new RectangleF(115, 303, 190, 20), -0.01f, 16, "Neuton", (Vector4)Color.Black);
            DrawNineFrame($"MinGui:R2Frame", new RectangleF(100, 116, 200, 200), 1, ((Vector4)Color.AntiqueWhite));
            DrawGlyph($"MinGui:CollapseIcon", new RectangleF(242, 305, 16, 16), -0.01f, ((Vector4)Color.Black));
            DrawGlyph($"MinGui:ExpandIcon", new RectangleF(258, 305, 16, 16), -0.01f, ((Vector4)Color.Black));
            DrawGlyph($"MinGui:CloseIcon", new RectangleF(274, 305, 16, 16), -0.01f, ((Vector4)Color.Black));

            DrawGlyph($"MinGui:R2Frame", new RectangleF(280, 160, 20, 20), 0, ((Vector4)Color.White));
            DrawGlyph($"MinGui:ArrowUp", new RectangleF(280, 280, 20, 20), 0, ((Vector4)Color.Black));
            DrawGlyph($"MinGui:ArrowDown", new RectangleF(280, 130, 20, 20), 0, ((Vector4)Color.Black));

            DrawGlyph($"MinGui:R2Frame", new RectangleF(180, 116, 20, 20), 0, ((Vector4)Color.White));
            DrawGlyph($"MinGui:ArrowLeft", new RectangleF(108, 116, 20, 20), 0, ((Vector4)Color.Black));
            DrawGlyph($"MinGui:ArrowRight", new RectangleF(268, 116, 20, 20), 0, ((Vector4)Color.Black));





            DrawNineFrame($"SimpleRpg:GemFrame", new RectangleF(600, 298, 200, 32), -0.01f, ((Vector4)Color.White));
            DrawString("SimpleRpg:Title", new RectangleF(615, 303, 190, 20), -0.01f, 16, "Neuton", (Vector4)Color.White);
            DrawNineFrame($"SimpleRpg:GemFrame", new RectangleF(600, 116, 200, 200), 1, ((Vector4)Color.AntiqueWhite));
            DrawGlyph($"SimpleRpg:CollapseIcon", new RectangleF(742, 305, 16, 16), -0.01f, ((Vector4)Color.White));
            DrawGlyph($"SimpleRpg:ExpandIcon", new RectangleF(758, 305, 16, 16), -0.01f, ((Vector4)Color.White));
            DrawGlyph($"SimpleRpg:CloseIcon", new RectangleF(774, 305, 16, 16), -0.01f, ((Vector4)Color.White));

            DrawGlyph($"SimpleRpg:GemIcon", new RectangleF(780, 160, 20, 20), 0, ((Vector4)Color.White));
            DrawGlyph($"SimpleRpg:ArrowUp", new RectangleF(780, 280, 20, 20), 0, ((Vector4)Color.White));
            DrawGlyph($"SimpleRpg:ArrowDown", new RectangleF(780, 130, 20, 20), 0, ((Vector4)Color.White));

            DrawGlyph($"SimpleRpg:GemIcon", new RectangleF(680, 116, 20, 20), 0, ((Vector4)Color.White));
            DrawGlyph($"SimpleRpg:ArrowLeft", new RectangleF(608, 116, 20, 20), 0, ((Vector4)Color.White));
            DrawGlyph($"SimpleRpg:ArrowRight", new RectangleF(768, 116, 20, 20), 0, ((Vector4)Color.White));

            if (showCursor)
            {
                var c = _isMouseDown;
                var cursorStyle = (_isMouseDown) ? $"KennyGUI:GoldSword" : $"KennyGUI:SilverSword";
                var cursorOffSet = StyleAtlas.EstimateOriginalSize($"KennyGUI:GoldSword");
                
                DrawGlyph(cursorStyle, new RectangleF(mPos.X, Resolution.Height - (mPos.Y + cursorOffSet.Y), cursorOffSet.X, cursorOffSet.Y), -maxGuiDepth, ((Vector4)Color.White));
            }



        

                        //DrawGlyph($"SimpleRpg:ResizeIcon", new RectangleF(780, 100, 20, 20), 0, ((Vector4)Color.Black));

            //DrawNineFrame($"squareFrame", new RectangleF(100, 600, 200, 200), -1, ((Vector4)Color.Blue));
            */
            /*
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 0, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Left, uv.Bottom, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 250, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Right, uv.Top, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 0, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Right, uv.Bottom, 0) });

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 0, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Left, uv.Bottom, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 250, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Left, uv.Top, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 250, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Right, uv.Top, 0) });
            */



            /*
            uv = FontAtlas[$"Cookie.{(((int)'A').ToString())}"];
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 0, 0),     Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Left , uv.Bottom, 1)});
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 250, 0), Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Right, uv.Top   , 1)});
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 0, 0),   Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Right, uv.Bottom, 1)});

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 0, 0),     Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Left , uv.Bottom, 1)});
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 250, 0),   Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Left , uv.Top   , 1)});
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 250, 0), Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Right, uv.Top   , 1)});
            */



            /*
            DrawString("The quick brown fox jumped over the lazy dog.", new RectangleF(250, 250, 200, 200), 0, 32, "Cookie", ((Vector4)Color.Black), true);

            DrawString("The quick brown fox jumped over the lazy dog.", new RectangleF(550, 250, 200, 200), 0, 16, "Neuton", ((Vector4)Color.Blue), true);

            DrawString("The quick brown fox jumped over the lazy dog.", new RectangleF(750, 250, 200, 200), 0, 10, "Neuton", ((Vector4)Color.Red), true);
            */
            GuiShader.Update(env.Window.Device);


            //Pull Canvas from card
            /*
            MappedResource mapped = env.Window.Device.Map(cTex, MapMode.Read);
            byte[] bytes = new byte[mapped.SizeInBytes];
            Marshal.Copy(mapped.Data, bytes, 0, (int)mapped.SizeInBytes);

            Canvas = Image.LoadPixelData<Rgba32>(bytes, (int)cTex.Width, (int)cTex.Height);
            env.Window.Device.Unmap(cTex);
            */

            //Modify Canvas *SWRender*
            /*
             t++;
             Canvas.Mutate<Rgba32>(i => 
             {
                 //i.Clear(Color.Transparent);
                  i.DrawText((t / ((instant.SessionDuration+1) / 1000.0f)).ToString(), font, Color.Orange, new PointF(100, 0100));
             });
            */

            //Send Canvas to Card
            //AssetManager.UpdateTextureData(env.Window.Device, cTex, Canvas);

        }


        internal bool HandleMouseButtonEvent(SDL_MouseButtonEvent mouseButtonEvent)
        {
            var w = GetWidgetAtPt(ImGuiNET.ImGui.GetIO().MousePos);
            if (mouseButtonEvent.button == SDL_MouseButton.Left)
                _isMouseDown = (mouseButtonEvent.type == SDL_EventType.MouseButtonDown);
            return false;
        }


        /*
Func()
{
   if (this._screenshotQueued)
   {
       this._screenshotQueued = false;

       TextureDescription desc = TextureDescription.Texture2D(
           this.MainFramebufferTexture.Width,
           this.MainFramebufferTexture.Height,
           this.MainFramebufferTexture.MipLevels,
           this.MainFramebufferTexture.ArrayLayers,
           this.MainFramebufferTexture.Format,
           TextureUsage.Staging
       );

       Texture? tex = this.ResourceFactory.CreateTexture(desc);

       this.CommandList.Begin();
       this.CommandList.CopyTexture(this.MainFramebufferTexture, tex);
       this.CommandList.End();
       this.GraphicsDevice.SubmitCommands(this.CommandList);

       MappedResource mapped = this.GraphicsDevice.Map(tex, MapMode.Read);

       byte[] bytes = new byte[mapped.SizeInBytes];
       Marshal.Copy(mapped.Data, bytes, 0, (int)mapped.SizeInBytes);

       Image img = Image.LoadPixelData<Rgba32>(bytes, (int)tex.Width, (int)tex.Height);
       this.GraphicsDevice.Unmap(tex);

       img = img.CloneAs<Rgb24>();

       if (!GraphicsDevice.IsUvOriginTopLeft)
       {
           img.Mutate(x => x.Flip(FlipMode.Vertical));
       }

       this.InvokeScreenshotTaken(img);

       tex.Dispose();
   }

}
*/
        ~GUIContext()
        {
            Canvas.Dispose();
        }
        internal void SetResolution(Vector2 vector2)
        {
            _nextResolution = vector2;
            _setNextResolution = true;

        }

        internal void AddWindow(string name, AttributeCollection result)
        {
            var widget = CreateWindow(result);
            this.Widgets.Add(name, widget);
        }
        public GuiWidgetBase CreateWidget(AttributeCollection def)
        {
            GuiWidgetBase widget = null;
            switch (def["type"])
            {
                case null:
                    throw new Exception("Invalid window definition");
                case string typeName:
                    switch (typeName.Trim().ToLower())
                    {
                        case "panel":
                            widget = new Panel(def, this);
                            break;
                        case "frame":
                            widget = new Frame(def, this);
                            break;
                        case "border":
                            widget = new Border(def, this);
                            break;
                        case "window":
                            widget = new Window(def, this);
                            break;
                        case "text":
                            widget = new Text(def, this);
                            break;
                        case "button":
                            widget = new Button(def, this);
                            break;
                    }
                    break;
            }
            return widget;

        }


        public void LoadFont(String fontFile, String fontName = null)
        {
            if (String.IsNullOrEmpty(fontName))
                fontName = System.IO.Path.GetFileNameWithoutExtension(fontFile);
            if (fontName.LastIndexOf(".") != fontName.IndexOf("."))
                fontName = fontName.Substring(fontName.LastIndexOf(".") + 1);
            Dictionary<string, SpriteModel> sprites = new Dictionary<string, SpriteModel>();
            FontData data = new FontData();
            Image<Rgba32> bitmap = null;
            int f = 0;

            using (var stream = AssetManager.FindFileStream(fontFile))
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    foreach (var entry in zip.Entries)
                    {
                        using (var entryStream = entry.Open())
                        {

                            switch (System.IO.Path.GetExtension(entry.FullName).ToLower())
                            {
                                case ".atlas":
                                    using (var rdr = new StreamReader(entryStream))
                                    {
                                        sprites = SpriteModel.LoadSpriteModelsFromSrc(rdr.ReadToEnd());
                                    }
                                    f = f & 1;
                                    break;
                                case ".fnt":
                                    using (var rdr = new StreamReader(entryStream))
                                    {
                                        data = Newtonsoft.Json.JsonConvert.DeserializeObject<FontData>(rdr.ReadToEnd());
                                    }
                                    f = f & 2;
                                    break;
                                case ".png":
                                    bitmap = (Image<Rgba32>)Image<Rgba32>.Load(entryStream);
                                    f = f & 4;
                                    break;
                            }
                        }
                    }

                }
            }
            if (f != (1 & 2 & 4))
            {
                throw new Exception("Missing something");
            }
            if (sprites.Keys.Count > 1)
                throw new Exception("Extra sprite models?");
            foreach (var k in sprites.Keys)
            {
                FontAtlas.Merge(fontName, sprites[k], bitmap);
                FontLib.Add(fontName, data);
            }
        }

        public void LoadResources(AttributeCollection refs)
        {
            foreach (var k in refs.Attributes)
            {
                var def = (AttributeCollection)k.Value;
                if (def == null) continue;
                switch (def.DataAs<String>("Type").Trim().ToLower())
                {
                    case "atlas":
                        {
                            /*
                            var src = def.DataAs<String>("src");
                            var models = SpriteModel.LoadSpriteModels(src);
                            if (!SpriteModels.ContainsKey(k.AttributeName))
                                SpriteModels.Add(k.AttributeName, models);
                            else SpriteModels[k.AttributeName] = models;
                            */

                        }
                        break;
                    case "image":
                        {
                            var src = def.DataAs<String>("src");
                            Image<Rgba32> img = SixLabors.ImageSharp.Image<Rgba32>.Load<Rgba32>(src);
                            if (def["Region"] != null)
                            {
                                Vector4 region;
                                switch (def["Region"])
                                {
                                    case Vector4 v4: region = v4; break;
                                    default:
                                        throw new Exception("Invalid region");
                                        break;
                                }
                                if (region.X < 0 || region.Y < 0 || region.X + region.Z > img.Width || region.Y + region.W > img.Height)
                                {
                                    throw new Exception("Invalid Region");
                                }
                                if (!(region.Z == img.Width && region.W == img.Height))
                                    img.Mutate(X => X.Crop(new SixLabors.ImageSharp.Rectangle((int)region.X, (int)region.Y, (int)region.W, (int)region.Z)));
                            }

                            if (!Images.ContainsKey(k.AttributeName))
                                Images.Add(k.AttributeName, img);
                            else Images[k.AttributeName] = img;
                        }
                        break;
                    case "font":
                        {
                            var src = def.DataAs<String>("src");
                            
                            // FontDescription desc;
                            // this.fontCollection.Add(src, out desc);
                            // if (!Fonts.ContainsKey(k.AttributeName))
                            //     Fonts.Add(k.AttributeName, desc);
                            // else Fonts[k.AttributeName] = desc;

                        }
                        break;
                    case "script":
                        break;
                    case "style":
                        {
                            var src = def.DataAs<AttributeCollection>("Src");
                            if (src == null) continue;
                            var name = def.DataAs<String>("Name");
                            if (String.IsNullOrEmpty(name)) continue;
                            foreach(var v in src.Attributes)
                            {
                                if (v.Type!=typeof(AttributeCollection))
                                {
                                    Styles.DefineStyle($"{name}.{v.AttributeName}", v.Value.ToString());
                                    continue;
                                }
                                var childAtt = (AttributeCollection)v.Value;
                                switch((childAtt.DataAs<String>("Type")??"").ToLower())
                                {
                                    case "atlas":
                                        {
                                            var atlasName = childAtt.DataAs<String>("Name");
                                            if (String.IsNullOrEmpty(atlasName)) atlasName = v.AttributeName;
                                            var atlasSrc = childAtt.DataAs<String>("src");
                                            var models = SpriteModel.LoadSpriteModelsFromSrc(AssetManager.ReadFile( atlasSrc));
                                            if (!SpriteModels.ContainsKey(atlasName))
                                                SpriteModels.Add(atlasName, models);
                                            else SpriteModels[atlasName] = models;
                                        }
                                        break;
                                    case "font":
                                        {
                                            var fontName = childAtt.DataAs<String>("Name");
                                            if (String.IsNullOrEmpty(fontName)) fontName = v.AttributeName;
                                            var fontSrc = childAtt.DataAs<String>("src");
                                            LoadFont(fontSrc,fontName);

                                        }
                                        break;
                                    default:
                                        {
                                            var spriteModel = childAtt.DataAs<String>("Sprite");
                                            var spriteSrc = childAtt.DataAs<String>("Src");
                                            
                                            if (String.IsNullOrEmpty(spriteSrc)) continue;
                                            var spriteRef = $"{def["Name"]}_{v.AttributeName}";
                                            if (String.IsNullOrEmpty(spriteModel))
                                            {
                                                if (childAtt["rect"] != null)
                                                    StyleAtlas.Merge(spriteRef, AssetManager.GetImage(spriteSrc), (Vector4)childAtt["rect"]);
                                                else
                                                    StyleAtlas.Merge(spriteRef, AssetManager.GetImage(spriteSrc));
                                            }
                                            else
                                            {
                                                var dIndex = spriteModel.IndexOf(".");
                                                var atlasname = spriteModel.Substring(0, dIndex);
                                                var modelName = spriteModel.Substring(dIndex + 1);
                                                StyleAtlas.Merge(spriteRef, SpriteModels[atlasname][modelName], spriteSrc);
                                            }

                                            Styles.DefineStyle($"{name}.{v.AttributeName}", spriteRef);

                                        }
                                        break;
                                }
                            }
                        }



                        break;

                }
                //k.AttributeName
            }
        }

        public GuiWidgetBase CreateWindow(AttributeCollection result)
        {
            var refs = result.DataAs<AttributeCollection>("Resources");
            var def = result.DataAs<AttributeCollection>("Root");
            LoadResources(refs);
            _rebuildFontAtlas = true;
            _rebuildStyleAtlas=true; 
            return CreateWidget(def);

        }
        bool doCap = true;
        internal void Render(Instant instant, GameState state, GameEnvironment env, Framebuffer outbuffer, CommandList cmds)
        {

            //env.rd.StartFrameCapture();
            //cmds.CopyTexture(cTex, GuiBuffer.ColorTargets[0].Target);
            cmds.SetFramebuffer(GuiBuffer);
            cmds.ClearColorTarget(0, RgbaFloat.Clear);
            cmds.ClearDepthStencil(0);

            GuiShader.Render(GuiBuffer, cmds, null, guiCam.CamMatrix, Matrix4x4.Identity);


            //cmds.ClearColorTarget(0, RgbaFloat.Green);
            //cmds.ClearDepthStencil(0);
            //cmds.ResolveTexture(, resolvedTexture);
            cmds.SetFramebuffer(outbuffer);
            GUICanvas.Render(outbuffer, cmds, null, Matrix4x4.Identity, Matrix4x4.Identity);
            //cmds.SetFramebuffer(GuiBuffer);


            //cmds.CopyTexture(GuiBuffer.ColorTargets[0].Target, cTex);
            //cmds.SetFramebuffer(outbuffer);
            //env.rd.EndFrameCapture();
            if (doCap)
            {
                env.rd.TriggerCapture();
                doCap = false;
            }

            int q = 1;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="str">The text to display</param>
        /// <param name="textArea">The area to constrain the text to</param>
        /// <param name="zIndex">The depth of the text quads</param>
        /// <param name="textSize">Size of text quads</param>
        /// <param name="style">Name of Font style</param>
        /// <param namne="AutoWrap">Automatically wrap text that doesn't fit</param>
        /// <exception cref="Exception"></exception>
        public void DrawString(String str, RectangleF textArea, float zIndex, float textSize, String font, Vector4 color, bool AutoWrap = true)
        {
            if (!FontLib.ContainsKey(font))
                throw new Exception($"No font {font}");
            FontData fontData = FontLib[font];

            var resizeRatioV = (float)(textSize) / (float)fontData.LineHeight;

            float trueLineHeight = fontData.LineHeight * resizeRatioV;
            float penX = textArea.Left;
            float penY = textArea.Bottom - trueLineHeight;

            int i = 0;
            while (i < str.Length)
            {
                if (penY + trueLineHeight < textArea.Top)
                    break;
                var currCh = (int)str[i];
                if (!fontData.Glyphs.ContainsKey(currCh))
                {
                    penX += (fontData.SpaceWidth * resizeRatioV);
                    if (penX > textArea.Right)
                    {
                        penX = textArea.Left;
                        penY = penY - (trueLineHeight * 1.25f);
                    }
                    //MoveNext and continue
                    i += 1;
                    continue;
                }
                var glyphData = fontData.Glyphs[currCh];
                float offsetX = glyphData.BearingX * resizeRatioV;
                float offsetY = glyphData.Drop * resizeRatioV;
                float glyphWidth = glyphData.Width * resizeRatioV;
                float glyphHeight = glyphData.Height * resizeRatioV;
                if (penX + offsetX + glyphWidth > textArea.Right)
                {
                    penX = textArea.Left;
                    penY = penY - (trueLineHeight * 1.25f);
                    //Continue on new line without moving next or placing char.
                    continue;
                }
                var uv = FontAtlas[$"{font}.{currCh}"];

                var trueX = penX + offsetX;
                var trueY = penY - offsetY;

                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX, trueY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, 1) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX + glyphWidth, trueY + glyphHeight, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, 1) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX + glyphWidth, trueY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, 1) });

                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX, trueY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, 1) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX, trueY + glyphHeight, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, 1) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX + glyphWidth, trueY + glyphHeight, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, 1) });
                i += 1;
                penX += glyphData.Advance * resizeRatioV;
            }
        }

        public void DrawNineFrame(string StyleName, RectangleF footprint, float zIndex)
        {
            DrawNineFrame(StyleName, footprint, zIndex, Vector4.One);
        }
        public void DrawNineFrame(string StyleName, RectangleF footprint, float zIndex, Vector4 color)
        {
            var Uv_TLCorner = StyleAtlas[$"{StyleName}.TL"];
            var Sz_TLCorner = StyleAtlas.EstimateOriginalSize(Uv_TLCorner);

            var Uv_TFill = StyleAtlas[$"{StyleName}.TM"];

            var Uv_TRCorner = StyleAtlas[$"{StyleName}.TR"];
            var Sz_TRCorner = StyleAtlas.EstimateOriginalSize(Uv_TRCorner);


            var Uv_MLFill = StyleAtlas[$"{StyleName}.ML"];
            var Uv_MMFill = StyleAtlas[$"{StyleName}.MM"];
            var Uv_MRFill = StyleAtlas[$"{StyleName}.MR"];

            var Uv_BLCorner = StyleAtlas[$"{StyleName}.BL"];
            var Sz_BLCorner = StyleAtlas.EstimateOriginalSize(Uv_BLCorner);

            var Uv_BFill = StyleAtlas[$"{StyleName}.BM"];

            var Uv_BRCorner = StyleAtlas[$"{StyleName}.BR"];
            var Sz_BRCorner = StyleAtlas.EstimateOriginalSize(Uv_BRCorner);

            if (footprint.Height < Sz_TLCorner.Y + Sz_BLCorner.Y)
            {
                var newTLCornerHeight = Sz_TLCorner.Y * (Sz_TLCorner.Y / (Sz_TLCorner.Y + Sz_BLCorner.Y));
                var newBLCornerHeight = Sz_BLCorner.Y * (Sz_BLCorner.Y / (Sz_TLCorner.Y + Sz_BLCorner.Y));
                Sz_TLCorner.Y = newTLCornerHeight;
                Sz_BLCorner.X = newBLCornerHeight;
            }

            if (footprint.Height < Sz_TRCorner.Y + Sz_BRCorner.Y)
            {
                var newTRCornerHeight = Sz_TRCorner.Y * (Sz_TRCorner.Y / (Sz_TRCorner.Y + Sz_BRCorner.Y));
                var newBRCornerHeight = Sz_BRCorner.Y * (Sz_BRCorner.Y / (Sz_TRCorner.Y + Sz_BRCorner.Y));
                Sz_TLCorner.Y = newTRCornerHeight;
                Sz_BLCorner.X = newBRCornerHeight;
            }

            if (footprint.Width < Sz_TLCorner.X + Sz_TRCorner.X)
            {
                float d = Sz_TLCorner.X + Sz_TRCorner.X;
                Sz_TLCorner.X = Sz_TLCorner.X * (Sz_TLCorner.X / d);
                Sz_TRCorner.X = Sz_TRCorner.X * (Sz_TRCorner.X / d);
            }

            if (footprint.Width < Sz_BLCorner.X + Sz_BRCorner.X)
            {
                float d = Sz_BLCorner.X + Sz_BRCorner.X;
                Sz_BLCorner.X = Sz_BLCorner.X * (Sz_BLCorner.X / d);
                Sz_BRCorner.X = Sz_BRCorner.X * (Sz_BRCorner.X / d);
            }



            //Draw Top Left Corner
            {
                var closeX = footprint.X;
                var farX = footprint.X + Sz_TLCorner.X;
                var lowY = footprint.Y + footprint.Height - Sz_TLCorner.Y;
                var highY = footprint.Y + footprint.Height;
                DrawQuad(zIndex, color, Uv_TLCorner, closeX, farX, lowY, highY);
            }

            //Draw Bottom Left Corner
            {
                var closeX = footprint.X;
                var farX = footprint.X + Sz_BLCorner.X;
                var lowY = footprint.Y;
                var highY = footprint.Y + Sz_BLCorner.Y;
                DrawQuad(zIndex, color, Uv_BLCorner, closeX, farX, lowY, highY);
            }

            //Draw Top Right Corner
            {
                var closeX = footprint.X + footprint.Width - Sz_TRCorner.X;
                var farX = footprint.X + footprint.Width;
                var lowY = footprint.Y + footprint.Height - Sz_TLCorner.Y;
                var highY = footprint.Y + footprint.Height;
                DrawQuad(zIndex, color, Uv_TRCorner, closeX, farX, lowY, highY);
            }

            //Draw Bottom Right Corner
            {
                var closeX = footprint.X + footprint.Width - Sz_BRCorner.X;
                var farX = footprint.X + footprint.Width;
                var lowY = footprint.Y;
                var highY = footprint.Y + Sz_BLCorner.Y;
                DrawQuad(zIndex, color, Uv_BRCorner, closeX, farX, lowY, highY);
            }


            //Draw top fill
            float topFillWidth = footprint.Width - (Sz_TLCorner.X + Sz_TRCorner.X);
            if (topFillWidth > 0)
            {
                var closeX = footprint.X + Sz_TRCorner.X;
                var farX = footprint.X + footprint.Width - Sz_TRCorner.X;
                var lowY = footprint.Y + footprint.Height - Math.Min(Sz_TRCorner.Y, Sz_TLCorner.Y);
                var highY = footprint.Y + footprint.Height;
                DrawQuad(zIndex, color, Uv_TFill, closeX, farX, lowY, highY);

            }

            //Draw bottom fill
            float bottomFillWidth = footprint.Width - (Sz_BLCorner.X + Sz_BRCorner.X);
            if (bottomFillWidth > 0)
            {
                var closeX = footprint.X + Sz_BRCorner.X;
                var farX = footprint.X + footprint.Width - Sz_BRCorner.X;
                var lowY = footprint.Y;
                var highY = footprint.Y + Math.Min(Sz_BRCorner.Y, Sz_BLCorner.Y);
                DrawQuad(zIndex, color, Uv_BFill, closeX, farX, lowY, highY);

            }

            //Draw left fill
            float leftFillWidth = footprint.Width - (Sz_BLCorner.X + Sz_BRCorner.X);
            if (leftFillWidth > 0)
            {
                var closeX = footprint.X;
                var farX = footprint.X + Math.Min(Sz_BLCorner.X, Sz_TLCorner.X);
                var lowY = footprint.Y + Sz_BLCorner.Y;
                var highY = footprint.Y + footprint.Height - Sz_TLCorner.Y;
                DrawQuad(zIndex, color, Uv_MLFill, closeX, farX, lowY, highY);

            }
            //Draw right fill
            float rightFillWidth = footprint.Width - (Sz_BLCorner.X + Sz_BRCorner.X);
            if (rightFillWidth > 0)
            {
                var closeX = footprint.X + footprint.Width - Math.Min(Sz_TRCorner.X, Sz_BRCorner.X);
                var farX = footprint.X + footprint.Width;
                var lowY = footprint.Y + Sz_BRCorner.Y;
                var highY = footprint.Y + footprint.Height - Math.Min(Sz_TRCorner.Y, Sz_BRCorner.Y);
                DrawQuad(zIndex, color, Uv_MRFill, closeX, farX, lowY, highY);
            }

            //Draw center fill
            {
                var closeX = footprint.X + Math.Min(Sz_TLCorner.X, Sz_BLCorner.X);
                var farX = footprint.X + footprint.Width - Math.Min(Sz_TRCorner.X, Sz_BRCorner.X);
                var lowY = footprint.Y + Math.Min(Sz_BLCorner.Y, Sz_BRCorner.Y);
                var highY = footprint.Y + footprint.Height - Math.Min(Sz_TLCorner.Y, Sz_TRCorner.Y);
                DrawQuad(zIndex, color, Uv_MMFill, closeX, farX, lowY, highY);
            }

        }

        public void DrawVerticalThreeFrame(string style, RectangleF footprint, float zIndex, Vector4 color)
        {
            var uvTop = StyleAtlas[$"{style}.T"];
            var uvMid = StyleAtlas[$"{style}.M"];
            var uvBot = StyleAtlas[$"{style}.B"];

            var szTop = StyleAtlas.EstimateOriginalSize(uvTop);
            var szBottom = StyleAtlas.EstimateOriginalSize(uvBot);

            if (szTop.Y+szBottom.Y>footprint.Height)
            {
                float d = szTop.Y + szBottom.Y;
                szTop.Y = szTop.Y * (szTop.Y / d);
                szBottom.Y = szBottom.Y * (szBottom.Y / d);
            }

            var closeX = footprint.X ;
            var farX = footprint.X + footprint.Width;
            //Draw top
            {
                var lowY = footprint.Y + footprint.Height - szTop.Y;
                var highY = footprint.Y + footprint.Height;
                DrawQuad(zIndex, color, uvTop, closeX, farX, lowY, highY);
            }

            //Draw top
            {
                var lowY = footprint.Y + szBottom.Y;
                var highY = footprint.Y + footprint.Height-szTop.Y;
                DrawQuad(zIndex, color, uvMid, closeX, farX, lowY, highY);
            }

            //Draw bottom
            {
                var lowY = footprint.Y;
                var highY = footprint.Y + szBottom.Y;
                DrawQuad(zIndex, color, uvBot, closeX, farX, lowY, highY);
            }
        }
        public void DrawHorizontalThreeFrame(string style, RectangleF footprint, float zIndex, Vector4 color)
        {
            var uvLeft = StyleAtlas[$"{style}.L"];
            var uvMid = StyleAtlas[$"{style}.M"];
            var uvRight = StyleAtlas[$"{style}.R"];

            var szLeft = StyleAtlas.EstimateOriginalSize(uvLeft);
            var szRight = StyleAtlas.EstimateOriginalSize(uvRight);

            if (szLeft.X + szRight.X > footprint.Width)
            {
                float d = szLeft.X + szRight.X;
                szLeft.X = szLeft.X * (szLeft.X / d);
                szRight.X = szRight.X * (szRight.X / d);
            }

            var lowY = footprint.Y;
            var highY = footprint.Y + footprint.Height;
            //Draw Left
            {
                var closeX = footprint.X;
                var farX = footprint.X + szLeft.X;
                DrawQuad(zIndex, color, uvLeft, closeX, farX, lowY, highY);
            }

            //Draw middle
            {
                var closeX = footprint.X + szLeft.X;
                var farX = footprint.X + footprint.Width- szRight.X;
                DrawQuad(zIndex, color, uvMid, closeX, farX, lowY, highY);
            }

            //Draw right
            {
                var closeX = footprint.X+footprint.Width-szRight.X;
                var farX = footprint.X + footprint.Width;
                DrawQuad(zIndex, color, uvRight, closeX, farX, lowY, highY);
            }

        }

        private void DrawQuad(float zIndex, Vector4 color, RectangleF uv, float closeX, float farX, float lowY, float highY)
        {
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, 0) });

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, highY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, 0) });
        }

        public void DrawGlyph(string glyphName, RectangleF footprint, float zIndex, Vector4 color)
        {
            var closeX = footprint.X;
            var farX = footprint.X + footprint.Width;
            var lowY = footprint.Y;
            var highY = footprint.Y + footprint.Height;
            var uv = StyleAtlas[glyphName];// Uv_MMFill;
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, 0) });

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, lowY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(closeX, highY, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(farX, highY, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, 0) });
        }

        GuiWidgetBase GetWidgetAtPt(Vector2 loc)
        {
            foreach (var widget in this.Widgets.OrderBy(kvp=>kvp.Value.ZIndex))
            {
                if (widget.Value.ContainsPoint(loc))
                    return widget.Value.GetWidgetAtPt(loc - new Vector2(widget.Value.Position.X, widget.Value.Position.Y));
            }
            return null;
        }

    }

}
