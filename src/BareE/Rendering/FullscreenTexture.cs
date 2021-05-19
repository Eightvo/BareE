using BareE.UTIL;

using System;
using System.Collections.Generic;
using System.Numerics;

using Veldrid;

namespace BareE.Rendering
{
    public struct Pos3_Uv2:IProvideVertexLayoutDescription
    {
        public Vector3 Position;
        public Vector2 Uv;
        public Pos3_Uv2(Vector3 pos,Vector2 uv)
        {
            Position = pos;
            Uv = uv;
        }
        public uint SizeInBytes { get => 3*4+2*4; }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("Pos",VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                );
        }
    }

    public class FullScreenTexture:VertexTextureShader<Pos3_Uv2>
    {
        public override bool UseDepthStencil { get => true; }
        public FullScreenTexture() : base("BareE.EZRend.ImageShader.FullscreenTexture.FullScreenTexture") 
        {
            AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 1) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 0) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(1, -1, 0), Uv = new Vector2(1, 1) });

            AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 1) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, 1, 0), Uv = new Vector2(0, 0) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 0) });

        }
    }
    public class FramebufferToScreen : VertexTextureShader<Pos3_Uv2>
    {
   
        public override bool UseDepthStencil { get => false; }


        public FramebufferToScreen() : base("BareE.Rendering.FullScreenTexture")
        {
            
            
            AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 0) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 1) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(1, -1, 0), Uv = new Vector2(1, 0) });

            AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 0) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, 1, 0), Uv = new Vector2(0, 1) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 1) });

        }
    }

    public class VertexTextureShader<V> : VertexOnlyShader<V>
        where V : unmanaged, IProvideVertexLayoutDescription
    {
        
        public VertexTextureShader(String partial) : base(partial) 
        {
        }
        public VertexTextureShader(String vertShadername, String fragShaderName):base(vertShadername, fragShaderName)
        {
        }

        public VertexTextureShader(TextureDescription textureDescription, SamplerDescription samplerDescription, String partial):base(partial)
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

        Sampler ColorTextureSampler;
        TextureView ColorTextureView;
        Texture ColorTexture;
        Texture TargtetColorTexture;
        ResourceLayout ColorTextureLayout;
        ResourceSet ColorTextureResourceSet;
        public SamplerFilter ColorTextureFilter = SamplerFilter.Anisotropic;
        bool TextureDirty = false;
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
            if (SampDesc==null)
            {
                SampDesc= new SamplerDescription()
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
                        new ResourceLayoutElementDescription("ColorTexture",ResourceKind.TextureReadOnly, ShaderStages.Fragment),
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
