using BareE.UTIL;

using System;
using System.Collections.Generic;

using Veldrid;

namespace BareE.Rendering
{
    /// <summary>
    /// a baseclass to extend for shaders that accept a Vertex data and a single texture.
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public class VertexTextureShader<V> : VertexOnlyShader<V>
        where V : unmanaged, IProvideVertexLayoutDescription
    {
        public VertexTextureShader(String partial) : base(partial)
        {
        }

        public VertexTextureShader(String vertShadername, String fragShaderName) : base(vertShadername, fragShaderName)
        {
        }

        public VertexTextureShader(TextureDescription textureDescription, SamplerDescription samplerDescription, String partial) : base(partial)
        {
            TexDesc = textureDescription;
            SampDesc = samplerDescription;
        }

        public VertexTextureShader(TextureDescription textureDescription, SamplerDescription samplerDescription, String vertShadername, String fragShaderName) : base(vertShadername, fragShaderName)
        {
            TexDesc = textureDescription;
            SampDesc = samplerDescription;
        }

        public TextureDescription? TexDesc;
        public SamplerDescription? SampDesc;

        private Sampler ColorTextureSampler;
        private TextureView ColorTextureView;
        private Texture ColorTexture;
        private ResourceLayout ColorTextureLayout;
        private ResourceSet ColorTextureResourceSet;
        public SamplerFilter ColorTextureFilter = SamplerFilter.Anisotropic;

        public void SetTexture(GraphicsDevice device, Texture texture)
        {
            if (ColorTexture == texture) return;
            //if (texture.)
            ColorTexture?.Dispose();
            ColorTextureView?.Dispose();
            ColorTextureResourceSet?.Dispose();

            ColorTexture = texture;
            ColorTextureView = device.ResourceFactory.CreateTextureView(ColorTexture);
            ColorTextureResourceSet = device.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(ColorTextureLayout,
                ColorTextureView,
                ColorTextureSampler
                ));
        }

        public override void CreateResources(GraphicsDevice device)
        {
            if (TexDesc == null)
            {
                TexDesc = new TextureDescription(device.MainSwapchain.Framebuffer.Width, device.MainSwapchain.Framebuffer.Height, 1, 1, 1, Util.GetNativePixelFormat(device),
    TextureUsage.RenderTarget | TextureUsage.Sampled, TextureType.Texture2D);
            }
            if (SampDesc == null)
            {
                SampDesc = new SamplerDescription()
                {
                    AddressModeU = SamplerAddressMode.Clamp,
                    AddressModeV = SamplerAddressMode.Clamp,
                    Filter = ColorTextureFilter,
                    //Filter = SamplerFilter.Anisotropic,
                    ComparisonKind = ComparisonKind.Always,
                    MaximumAnisotropy = 4
                };
            }
            ColorTexture = device.ResourceFactory.CreateTexture(TexDesc.Value);
            ColorTextureView = device.ResourceFactory.CreateTextureView(ColorTexture);
            ColorTextureSampler = device.ResourceFactory.CreateSampler(SampDesc.Value);
            ColorTextureLayout = device.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("ColorTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("ColorSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                    )
                );
            ColorTextureResourceSet = device.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(ColorTextureLayout,
                ColorTextureView,
                ColorTextureSampler
                ));
            base.CreateResources(device);
        }

        protected override IEnumerable<ResourceLayout> CreateResourceLayout()
        {
            foreach (var v in base.CreateResourceLayout())
                yield return v;
            yield return ColorTextureLayout;
        }

        public override IEnumerable<ResourceSet> GetResourceSets()
        {
            foreach (var v in base.GetResourceSets())
                yield return v;
            yield return ColorTextureResourceSet;
        }
    }
}