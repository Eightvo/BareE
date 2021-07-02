using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using BareE;
using BareE.Rendering;

using Veldrid;

namespace BareE.GUI.TextRendering
{
    public struct TextSettings
    {
        public float OutlineThreshold { get; set; }
        public float BlurOutDist { get; set; }
        public Vector2 DropShadow { get; set; }
        public Vector3 GlowColor { get; set; }
        public float GlowDist { get; set; }
        public int Type { get; 
            set; }
        public int Flags { get; set; }
        public Vector2 TextureResolution { get; set; }
        public static TextSettings Default
        {
            get
            {
                return new TextSettings()
                {
                    OutlineThreshold = 0.01f,
                    BlurOutDist = 0.0f,
                    DropShadow = new Vector2(0, 0),
                    GlowDist = 0.0f,
                    GlowColor = new Vector3(0, 0, 1),
                    Type = 2,
                    Flags=0,
                    TextureResolution=new Vector2(0,0)
                };
            }
        }
    }


    public struct TextVertex : IProvideVertexLayoutDescription
    {
        public Vector3 Position;
        public Vector2 Uv;
        public Vector3 tint1;
        public Vector3 tint2;

        public TextVertex(Vector3 pos, Vector2 uv, Vector3 PrimaryColor, Vector3 OutlineColor)
        {
            Position = pos;
            Uv = uv;
            tint1 = PrimaryColor;
            tint2 = OutlineColor;
        }

        public uint SizeInBytes { get => (4 * 3) + (4 * 2) + (4 * 3) + (4 * 3); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Tangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }
    public class TextShader : VertexTextureShader<TextVertex>
    {
        FontData ActiveFont { get; set; }
        public Vector2 Resolution { get; set; }

        private bool SettingsDirty = true;
        private TextSettings _settings = TextSettings.Default;
        public TextSettings Settings { get { return _settings; } set { _settings = value; SettingsDirty = true; } }
        private DeviceBuffer SettingsBuffer;
        private ResourceLayout SettingsLayout;
        private ResourceSet SettingsResourceSet;

        public bool UseDepth { get; set; } = false;

        public TextShader(Vector2 resolution) : base("BareE.GUI.TextRendering.Text")
        {
            Resolution = resolution;
            BlendState = BlendStateDescription.SingleAlphaBlend;
            this.SampDesc = new SamplerDescription()
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
                ComparisonKind = ComparisonKind.Always,
            };
        }

        public override DepthStencilStateDescription DepthStencilDescription
        {
            get => new DepthStencilStateDescription(
                        depthTestEnabled: UseDepth,
                        depthWriteEnabled: UseDepth,
                        comparisonKind: ComparisonKind.LessEqual
                        );
        }


        public float Scale { get; set; } = 1f; 

        public Vector2 Cursor { get; set; } = new Vector2(-1, -1);
        public Vector3 TextColor { get; set; }
        public Vector3 OutlineColor { get; set; } = new Vector3(0, 0, 1);

        public void PutCharacter(char c, Vector2 loc, float scale, Vector3 clr, Vector3 outline)
        {
            Scale = scale;
            TextColor = clr;
            OutlineColor = outline;
            Cursor = loc;
            AddCharacter(c);
        }
        public void AddCharacter(char c)
        {
            if (!ActiveFont.Glyphs.ContainsKey((int)c))
            {
                Cursor += new Vector2((ActiveFont.SpaceWidth*Scale)/2.0f, 0);
                return;
            }
            var glyphData = ActiveFont.Glyphs[(int)c];

            Vector2 HalfRes = Resolution / 2.0f;
            Vector2 trueCursor = Cursor-HalfRes;
            trueCursor += new Vector2(0,-(glyphData.Drop*Scale)/2.0f);
            trueCursor = trueCursor / HalfRes;

            Cursor += new Vector2((glyphData.Advance/2.0f) * Scale, 0);
            //Cursor += new Vector2( 
            //    ((glyphData.Advance*Scale)/(float)ActiveFont.TotalWidth)*(float)Resolution.X
            //    , 0);
            //Cursor = new Vector2(glyphData.Advance, 0);

            Matrix4x4 transformMatrix = 
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateTranslation(new Vector3(trueCursor, 1));

            Vector2 UvBL = new Vector2(glyphData.Rect.Left / (float)ActiveFont.TotalWidth, glyphData.Rect.Height /(float)ActiveFont.LineHeight); 
            Vector2 UvTR = new Vector2(glyphData.Rect.Right / (float)ActiveFont.TotalWidth, 0);

            //Close Face

            ///1Pixel = 1.0f/(float)Resolution.X;

            var pixelWidth = (float)glyphData.Width / (float)Resolution.X;
            var pixelHeight = (float)glyphData.Height / (float)Resolution.Y;

            AddVertex(new TextVertex(Vector3.Transform(new Vector3(0, 0, 0.5f), transformMatrix), new Vector2(UvBL.X, UvBL.Y), TextColor, OutlineColor));
            AddVertex(new TextVertex(Vector3.Transform(new Vector3(pixelWidth, pixelHeight, 0.5f), transformMatrix), new Vector2(UvTR.X, UvTR.Y), TextColor, OutlineColor));
            AddVertex(new TextVertex(Vector3.Transform(new Vector3(pixelWidth, 0, 0.5f), transformMatrix), new Vector2(UvTR.X, UvBL.Y), TextColor, OutlineColor));

            AddVertex(new TextVertex(Vector3.Transform(new Vector3(0, 0, 0.5f), transformMatrix), new Vector2(UvBL.X, UvBL.Y), TextColor, OutlineColor));
            AddVertex(new TextVertex(Vector3.Transform(new Vector3(0, pixelHeight, 0.5f), transformMatrix), new Vector2(UvBL.X, UvTR.Y), TextColor, OutlineColor));
            AddVertex(new TextVertex(Vector3.Transform(new Vector3(pixelWidth, pixelHeight, 0.5f), transformMatrix), new Vector2(UvTR.X, UvTR.Y), TextColor, OutlineColor));

            /*
            AddVertex(new TextVertex(Vector3.Transform(new Vector3(-0.5f*pixelWidth , -0.5f, 0.5f), transformMatrix), new Vector2(UvBL.X, UvBL.Y), TextColor, OutlineColor));
            AddVertex(new TextVertex(Vector3.Transform(new Vector3(0.5f * pixelWidth, 0.5f, 0.5f), transformMatrix), new Vector2(UvTR.X, UvTR.Y), TextColor, OutlineColor));
            AddVertex(new TextVertex(Vector3.Transform(new Vector3(0.5f * pixelWidth, -0.5f, 0.5f), transformMatrix), new Vector2(UvTR.X, UvBL.Y), TextColor, OutlineColor));

            AddVertex(new TextVertex(Vector3.Transform(new Vector3(-0.5f* pixelWidth, -0.5f, 0.5f), transformMatrix), new Vector2(UvBL.X, UvBL.Y), TextColor, OutlineColor));
            AddVertex(new TextVertex(Vector3.Transform(new Vector3(-0.5f* pixelWidth, 0.5f, 0.5f), transformMatrix), new Vector2(UvBL.X, UvTR.Y), TextColor, OutlineColor));
            AddVertex(new TextVertex(Vector3.Transform(new Vector3(0.5f * pixelWidth, 0.5f, 0.5f), transformMatrix), new Vector2(UvTR.X, UvTR.Y), TextColor, OutlineColor));
            */


        }

        public void AddString(String str)
        {
            var currPos = Cursor;
            foreach (var c in str)
            {
                if (c.isNewLineChar())
                {
                    Cursor = currPos - new Vector2(0, (ActiveFont.LineHeight*Scale) / 2.0f);
                    currPos = Cursor;
                    continue;
                }
                AddCharacter(c);
            }
        }

        /*
        private glyphInfo GetInfo(Char glyph, Char style = ':')
        {
            String key = $"{glyph}{style}";
            if (_glyphCache.ContainsKey(key))
                return _glyphCache[key];
            if (!String.IsNullOrEmpty(UnknownGlyphKey))
                return _glyphCache[UnknownGlyphKey];
            return default(glyphInfo);
        }
        */
        public override void CreateResources(GraphicsDevice device)
        {
            SettingsBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription(48, BufferUsage.UniformBuffer));
            SettingsLayout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Settings", ResourceKind.UniformBuffer, ShaderStages.Fragment)
                )
            );
            SettingsResourceSet = device.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(SettingsLayout,
                SettingsBuffer
                ));
            base.CreateResources(device);
        }

        public override void UpdateBuffers(CommandList cmds)
        {
            if (SettingsDirty)
            {
                cmds.UpdateBuffer(SettingsBuffer, 0, _settings);
                SettingsDirty = false;
            }
            base.UpdateBuffers(cmds);
        }

        protected override IEnumerable<ResourceLayout> CreateResourceLayout()
        {
            foreach (var v in base.CreateResourceLayout())
                yield return v;
            yield return SettingsLayout;
        }

        public override IEnumerable<ResourceSet> GetResourceSets()
        {
            foreach (var v in base.GetResourceSets())
                yield return v;
            yield return SettingsResourceSet;
        }

        public void LoadFont(GraphicsDevice device, String resource)
        {
            ActiveFont = Newtonsoft.Json.JsonConvert.DeserializeObject<FontData>(AssetManager.ReadFile(resource));
            var fontTextureFileName = System.IO.Path.ChangeExtension(resource, "png");
            var texture = AssetManager.LoadTexture(fontTextureFileName, device, true);
            var sett = Settings;
            sett.TextureResolution = new Vector2(texture.Width, texture.Height);
            sett.Type = (int)ActiveFont.FontType;
            Settings = sett;
            SetTexture(device, texture);
        }
    }
}
