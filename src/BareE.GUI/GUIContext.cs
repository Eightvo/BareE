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

namespace BareE.GUI
{
    public class GUIContext
    {
        FullScreenTexture GUICanvas;
        BaseGuiShader GuiShader;

        Framebuffer GuiBuffer;
        Texture resolvedTexture;

        SpriteAtlas FontAtlas = new SpriteAtlas();
        Dictionary<String, FontData> FontLib = new Dictionary<string, FontData>(StringComparer.CurrentCultureIgnoreCase);
        
        SpriteAtlas StyleAtlas = new SpriteAtlas();

        Dictionary<String, Dictionary<String, SpriteModel>> SpriteModels = new Dictionary<string, Dictionary<string, SpriteModel>>(StringComparer.CurrentCultureIgnoreCase);
        Dictionary<String, Image<Rgba32>> Images = new Dictionary<string, Image<Rgba32>>(StringComparer.CurrentCultureIgnoreCase);

        internal void SetResolution(Vector2 vector2)
        {
            _nextResolution = vector2;
            _setNextResolution = true;

        }

        private Vector2 _nextResolution;
        private bool _setNextResolution = false;

        Camera guiCam;


        TextureSampleCount GuiBufferTextureCount = TextureSampleCount.Count1;

        public StyleTree Styles { get; set; }
        Dictionary<String, GuiWidgetBase> Widgets { get; set; }=new Dictionary<String, GuiWidgetBase>(StringComparer.InvariantCultureIgnoreCase);
        public Size Resolution { get { return new Size(Canvas.Width, Canvas.Height); } }

        public Image Canvas;
        
        public GUIContext(Size size)
        {
            Canvas = new SixLabors.ImageSharp.Image<Rgba32>(size.Width, size.Height);

        }

        internal void Load(Instant instant, GameState state, GameEnvironment env)
        {

            guiCam = new OrthographicCamera(Resolution.Width, Resolution.Height, 100);
            guiCam.Move(new Vector3(Resolution.Width / 2.0f, Resolution.Height / 2.0f, 0.0f));

            Veldrid.RenderDoc.Load(out env.rd);
            env.rd.SetCaptureSavePath(@"C:\TestData\RenderDocOutput\");

            GUICanvas = new FullScreenTexture();
            GUICanvas.SetOutputDescription(env.HUDBackBuffer.OutputDescription);
            GUICanvas.Flip = true;
            GUICanvas.CreateResources(env.Window.Device);

            //   cTex = env.Window.Device.ResourceFactory.CreateTexture(
            //       new TextureDescription((uint)Resolution.Width,
            //                              (uint)Resolution.Height,
            //                              1, 1, 1,
            //                              PixelFormat.R8_G8_B8_A8_UNorm,
            //                              TextureUsage.Staging,
            //                              TextureType.Texture2D,
            //                              TextureSampleCount.Count1)
            //   );

            //fontCollection = new SixLabors.Fonts.FontCollection();
            //fontFam = fontCollection.Add(@"C:\AA_Main\Git\Eightvo\BareE\src\BareE.GUI\Assets\Fonts\Roboto-Regular.ttf");
            //font = fontFam.CreateFont(12, SixLabors.Fonts.FontStyle.Regular);
            //  font = SystemFonts.Get("Arial").CreateFont(39);

            //  opts = new SixLabors.Fonts.TextOptions(font);
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

            StyleAtlas = new SpriteAtlas();
            var minGuiSpriteModels = SpriteModel.LoadSpriteModelsFromSrc(AssetManager.ReadFile(@"BareE.GUI.Assets.Styles.MinGui.Atlas"));
            StyleAtlas.Merge("squareFrame", minGuiSpriteModels["NineFrame"], @"BareE.GUI.Assets.Styles.squareFrame.png");
            StyleAtlas.Merge("R2Frame", minGuiSpriteModels["NineFrame"], @"BareE.GUI.Assets.Styles.R2Frame.png");
            StyleAtlas.Merge("CloseIcon", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.close.png");
            StyleAtlas.Merge("ExpandIcon", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.Expand.png");
            StyleAtlas.Merge("CheckBoxEmpty", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.OvalEmpty.png");
            StyleAtlas.Merge("CheckBoxChecked", minGuiSpriteModels["Glyph"], @"BareE.GUI.Assets.Styles.Oval_Filled.png");
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


            GuiShader = new BaseGuiShader();
            GuiShader.SetOutputDescription(GuiBuffer.OutputDescription);
            GuiShader.DepthStencilDescription = new DepthStencilStateDescription()
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.GreaterEqual,

            };
            GuiShader.CreateResources(env.Window.Device);
            GuiShader.SetStyleTexture(env.Window.Device, AssetManager.LoadTexture(StyleAtlas.AtlasSheet, env.Window.Device));
            GuiShader.SetFontTexture(env.Window.Device, AssetManager.LoadTexture(FontAtlas.AtlasSheet, env.Window.Device));
            GuiShader.resolution = new Vector2(900, 900);
            GuiShader.mousepos = new Vector2(30, 540);

            GuiShader.Update(env.Window.Device);

        }

        internal void Update(Instant instant, GameState state, GameEnvironment env)
        {

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


            var uv = StyleAtlas[$"R2Frame"];
            
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 0, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Left, uv.Bottom, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 250, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Right, uv.Top, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 0, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Right, uv.Bottom, 0) });

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 0, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Left, uv.Bottom, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 250, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Left, uv.Top, 0) });
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 250, 0), Color = new Vector4(1, 1, 1, 1), UvT = new Vector3(uv.Right, uv.Top, 0) });





            uv = FontAtlas[$"Cookie.{(((int)'A').ToString())}"];
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 0, 0),     Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Left , uv.Bottom, 1)});
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 250, 0), Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Right, uv.Top   , 1)});
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 0, 0),   Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Right, uv.Bottom, 1)});

            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 0, 0),     Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Left , uv.Bottom, 1)});
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(0, 250, 0),   Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Left , uv.Top   , 1)});
            GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(250, 250, 0), Color = new Vector4(0, 0, 0, 1), UvT = new Vector3(uv.Right, uv.Top   , 1)});





            DrawString("The quick brown fox jumped over the lazy dog.", new RectangleF(250, 250, 200, 200), 0, 32, "Cookie", ((Vector4)Color.Black), true);

            DrawString("The quick brown fox jumped over the lazy dog.", new RectangleF(550, 250, 200, 200), 0, 16, "Neuton", ((Vector4)Color.Blue), true);

            DrawString("The quick brown fox jumped over the lazy dog.", new RectangleF(750, 250, 200, 200), 0, 10, "Neuton", ((Vector4)Color.Red), true);

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
            if (fontName.LastIndexOf(".")!= fontName.IndexOf("."))
                fontName=fontName.Substring(fontName.LastIndexOf(".")+1);
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
            foreach(var k in refs.Attributes)
            {
                var def = (AttributeCollection)k.Value;
                if(def==null) continue;
                switch(def.DataAs<String>("Type").Trim().ToLower())
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
        public void DrawString(String str, RectangleF textArea, float zIndex,float textSize, String font, Vector4 color, bool AutoWrap=true)
        {
            if (!FontLib.ContainsKey(font))
                throw new Exception($"No font {font}");
            FontData fontData = FontLib[font];

            var resizeRatioV = (float)(textSize) / (float)fontData.LineHeight;

            float trueLineHeight = fontData.LineHeight * resizeRatioV;
            float penX = textArea.Left;
            float penY = textArea.Bottom - trueLineHeight;

            int i = 0;
            while(i<str.Length)
            {
                if (penY + trueLineHeight < textArea.Top)
                    break;
                var currCh = (int)str[i];
                if (!fontData.Glyphs.ContainsKey(currCh))
                {
                    penX += (fontData.SpaceWidth*resizeRatioV);
                    if (penX>textArea.Right)
                    {
                        penX = textArea.Left;
                        penY = penY - (trueLineHeight * 1.25f);
                    }
                    //MoveNext and continue
                    i += 1;
                    continue;
                }
                var glyphData = fontData.Glyphs[currCh];
                float offsetX = glyphData.BearingX*resizeRatioV;
                float offsetY = glyphData.Drop*resizeRatioV;
                float glyphWidth = glyphData.Width * resizeRatioV;
                float glyphHeight = glyphData.Height* resizeRatioV;
                if (penX+offsetX+glyphWidth>textArea.Right)
                {
                    penX = textArea.Left;
                    penY = penY - (trueLineHeight * 1.25f);
                    //Continue on new line without moving next or placing char.
                    continue;
                }
                var uv = FontAtlas[$"{font}.{currCh}"];

                var trueX = penX + offsetX;
                var trueY = penY - offsetY;

                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX             , trueY , zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, 1) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX + glyphWidth, trueY + glyphHeight, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, 1) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX + glyphWidth, trueY , zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Bottom, 1) });

                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX             , trueY , zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Bottom, 1) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX             , trueY + glyphHeight, zIndex), Color = color, UvT = new Vector3(uv.Left, uv.Top, 1) });
                GuiShader.AddVertex(new BaseGuiVertex() { Pos = new Vector3(trueX + glyphWidth, trueY + glyphHeight, zIndex), Color = color, UvT = new Vector3(uv.Right, uv.Top, 1) });
                i += 1;
                penX += glyphData.Advance * resizeRatioV;
            }
        }

    }
}
