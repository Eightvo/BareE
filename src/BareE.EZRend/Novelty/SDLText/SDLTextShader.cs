using BareE.EZRend.ModelShader.Color;
using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using Veldrid;

namespace BareE.EZRend.Novelty.SDLText
{
    public struct SDLTextSettings
    {
        public float OutlineThreshold { get; set; }
        public float BlurOutDist { get; set; }
        public Vector2 DropShadow { get; set; }
        public Vector3 GlowColor { get; set; }
        public float GlowDist { get; set; }
        public static SDLTextSettings Default
        {
            get
            {
                return new SDLTextSettings()
                {
                    OutlineThreshold = 0.01f,
                    BlurOutDist = 0.0f,
                    DropShadow = new Vector2(0, 0),
                    GlowDist = 0.0f,
                    GlowColor = new Vector3(0, 0, 1)
                };
            }
        }
    }

    public class SDLTextShader : VertexTextureShader<Float3_Float2_Float3_Float3>
    {
        private struct glyphInfo
        {
            public Vector2 UvBL { get; set; }
            public Vector2 UvTR { get; set; }
            public int CursorAdvance { get; set; }
        }

        private Dictionary<String, glyphInfo> _glyphCache;
        private Texture _glyphSheet;

        private bool SettingsDirty = true;
        private SDLTextSettings _settings = SDLTextSettings.Default;
        public SDLTextSettings Settings { get { return _settings; } set { _settings = value;SettingsDirty = true; } }
        private DeviceBuffer SettingsBuffer;
        private ResourceLayout SettingsLayout;
        private ResourceSet SettingsResourceSet;

        public String UnknownGlyphKey { get; set; }
        private float CharacterUvWidth { get; set; }

        public bool UseDepth { get; set; } = false;

        public SDLTextShader() : base("BareE.EZRend.Novelty.SDLText.SDLText")
        {
            BlendState = BlendStateDescription.SingleAlphaBlend;
            this.SampDesc = new SamplerDescription()
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
                //Filter = SamplerFilter.Anisotropic,
                ComparisonKind = ComparisonKind.Always,
                MaximumAnisotropy = 4
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

        public void LoadFont(GraphicsDevice device, String fontname)
        {
            var descFile = fontname;
            var textureFile = System.IO.Path.ChangeExtension(descFile, ".png");
            System.Text.RegularExpressions.Regex sdfRegex = new System.Text.RegularExpressions.Regex(@"^(?<char>.)(?<style>[:/_!]) (?<charLeft>\d+),(?<charTop>\d+),(?<charWid>\d+),(?<charHeight>\d+).*$");

            int i = 0;
            using (var rdr = new StreamReader(AssetManager.FindFileStream(descFile)))
            {
                _glyphCache = new Dictionary<string, glyphInfo>();

                _glyphSheet = AssetManager.LoadTexture(textureFile, device);
                base.SetTexture(device, _glyphSheet);
                //CharacterUvWidth = (float)i / (float)_glyphSheet.Width;

                while (!rdr.EndOfStream)
                {
                    var line = rdr.ReadLine();
                    Console.WriteLine(line);
                    var m = sdfRegex.Match(line);
                    if (!m.Success) continue;

                    glyphInfo gI = new glyphInfo()
                    {
                        UvBL = new Vector2((float)(i * 32) / (float)_glyphSheet.Width, 1),
                        UvTR = new Vector2((float)((i + 1) * (32)) / (float)_glyphSheet.Width, 0),
                        CursorAdvance = int.Parse(m.Groups["charWid"].Value)
                    };
                    _glyphCache.Add($"{m.Groups["char"].Value}{m.Groups["style"].Value}", gI);
                    i += 1;
                }
            }
        }

        public void AddCharacter(char c, char style, Vector3 Tint1, Vector3 Tint2, Vector2 pos, float scale)
        {
            //var key = $"{c}{style}";
            var gI = GetInfo(c, style);
            Vector3 cp = new Vector3(0, 0, 0);
            Matrix4x4 transformMatrix = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(new Vector3(pos, 1));

            //Close Face
            AddVertex(new Float3_Float2_Float3_Float3(Vector3.Transform(new Vector3(-0.5f, -0.5f, 0.5f), transformMatrix), new Vector2(gI.UvBL.X, gI.UvBL.Y), Tint1, Tint2));
            AddVertex(new Float3_Float2_Float3_Float3(Vector3.Transform(new Vector3(0.5f, 0.5f, 0.5f), transformMatrix), new Vector2(gI.UvTR.X, gI.UvTR.Y), Tint1, Tint2));
            AddVertex(new Float3_Float2_Float3_Float3(Vector3.Transform(new Vector3(0.5f, -0.5f, 0.5f), transformMatrix), new Vector2(gI.UvTR.X, gI.UvBL.Y), Tint1, Tint2));

            AddVertex(new Float3_Float2_Float3_Float3(Vector3.Transform(new Vector3(-0.5f, -0.5f, 0.5f), transformMatrix), new Vector2(gI.UvBL.X, gI.UvBL.Y), Tint1, Tint2));
            AddVertex(new Float3_Float2_Float3_Float3(Vector3.Transform(new Vector3(-0.5f, 0.5f, 0.5f), transformMatrix), new Vector2(gI.UvBL.X, gI.UvTR.Y), Tint1, Tint2));
            AddVertex(new Float3_Float2_Float3_Float3(Vector3.Transform(new Vector3(0.5f, 0.5f, 0.5f), transformMatrix), new Vector2(gI.UvTR.X, gI.UvTR.Y), Tint1, Tint2));
        }

        private glyphInfo GetInfo(Char glyph, Char style = ':')
        {
            String key = $"{glyph}{style}";
            if (_glyphCache.ContainsKey(key))
                return _glyphCache[key];
            if (!String.IsNullOrEmpty(UnknownGlyphKey))
                return _glyphCache[UnknownGlyphKey];
            return default(glyphInfo);
        }

        public override void CreateResources(GraphicsDevice device)
        {
            SettingsBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer));
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
    }
}