using BareE.Rendering;

using BareE.EZRend.ModelShader.Color;

using System;
using System.Collections.Generic;
using System.Numerics;

using Veldrid;

namespace BareE.EZRend.ImageShader.FullscreenTexture
{

    public class MultiTextureShader:LitVertexOnlyShader<Float3_Float3_Float3_Float3_Float2>
    {
        class TextureDescData
        {
            public TextureDescription? TexDesc;
            public SamplerDescription? SampDesc;
            internal TextureDescData(TextureDescription texDesc, SamplerDescription sampDesc)
            {
                TexDesc = texDesc; SampDesc = sampDesc;
            }
        }
        class TextureData
        {
            public ResourceLayout layout;
            public ResourceSet set;
            public Texture tex;
            public TextureView view;
            public Sampler sampler;
            public TextureData(ResourceLayout lo, ResourceSet s, Texture t, TextureView tv, Sampler sm)
            {
                layout = lo;
                set = s;
                tex = t;
                view = tv;
                sampler = sm;
            }
        }

        TextureDescData[] textureDescriptions;
        TextureData[] Textures;


        //ResourceLayout ambientLightsLayout;
        //ResourceSet ambientLightsSet;

        //ResourceLayout pointLightsLayout;
        //ResourceSet pointLightsSet;



        int TextureCount;

        public SamplerFilter ColorTextureFilter = SamplerFilter.Anisotropic;

        public MultiTextureShader(int textureCount) : this("BareE.EZRend.ModelShader.Uv.UvNormalBump", textureCount) { }
        public MultiTextureShader(String partial, int textureCount):base(partial){
            textureDescriptions = new TextureDescData[textureCount];
            Textures = new TextureData[textureCount];
            TextureCount = textureCount;
        }

        public override void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData,Matrix4x4 camMat, Matrix4x4 ModelMatrix)
        {
            base.Render(Trgt, cmds, sceneData,camMat, ModelMatrix);
        }

        public override void Update(GraphicsDevice device)
        {
            base.Update(device);
        }

        public override DepthStencilStateDescription DepthStencilDescription
        {
            get => new DepthStencilStateDescription(
                        depthTestEnabled: true,
                        depthWriteEnabled: true,
                        comparisonKind: ComparisonKind.Less
                        );
        }
        public void SetTexture(int textureId, GraphicsDevice device, Texture texture)
        {
            if (Textures[textureId].tex == texture) return;
            //if (texture.)
            Textures[textureId].tex?.Dispose();
            Textures[textureId].view?.Dispose();
            Textures[textureId].set?.Dispose();

            Textures[textureId].tex = texture;
            Textures[textureId].view = device.ResourceFactory.CreateTextureView(Textures[textureId].tex);
            Textures[textureId].set = device.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(Textures[textureId].layout,
                Textures[textureId].view,
                Textures[textureId].sampler
                ));
            Textures[textureId].set.Name = $"TexSet{textureId}";
        }

        public override void CreateResources(GraphicsDevice device)
        {
            for (int i = 0; i < TextureCount; i++)
            {


                var tDesc = new TextureDescription(device.MainSwapchain.Framebuffer.Width, device.MainSwapchain.Framebuffer.Height, 1, 1, 1, BareE.UTIL.Util.GetNativePixelFormat(device), TextureUsage.RenderTarget | TextureUsage.Sampled, TextureType.Texture2D);

                var sDesc = new SamplerDescription()
                {
                    AddressModeU = SamplerAddressMode.Clamp,
                    AddressModeV = SamplerAddressMode.Clamp,
                    Filter = ColorTextureFilter,
                    ComparisonKind = ComparisonKind.LessEqual,
                    MaximumAnisotropy = 4
                };
                textureDescriptions[i] = new TextureDescData(tDesc, sDesc);

                var Tex = device.ResourceFactory.CreateTexture(tDesc);
                var ColorTextureView = device.ResourceFactory.CreateTextureView(Tex);
                var ColorTextureSampler = device.ResourceFactory.CreateSampler(sDesc);
                var ColorTextureLayout = device.ResourceFactory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                            new ResourceLayoutElementDescription($"Texture{i}", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                            new ResourceLayoutElementDescription($"Sampler{i}", ResourceKind.Sampler, ShaderStages.Fragment)
                        )
                    );
                var ColorTextureResourceSet = device.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(ColorTextureLayout,
                    ColorTextureView,
                    ColorTextureSampler
                    ));
                ColorTextureResourceSet.Name = $"TexSet{i}";

                this.Textures[i] = new TextureData(ColorTextureLayout, ColorTextureResourceSet, Tex, ColorTextureView, ColorTextureSampler);
            }
            base.CreateResources(device);

        }

        protected override IEnumerable<ResourceLayout> CreateResourceLayout()
        {
            //Cameras/Lights
            foreach (var v in base.CreateResourceLayout())
                yield return v;


            //Textures
            foreach (var v in Textures)
                yield return v.layout;


        }
        public override IEnumerable<ResourceSet> GetResourceSets()
        {
            foreach (var v in base.GetResourceSets())
                yield return v;
            //Textures
            foreach (var v in Textures)
                yield return v.set;


        }

    }
}
