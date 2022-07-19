using BareE.EZRend.ModelShader.Color;
using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Numerics;

using Veldrid;

namespace BareE.EZRend.ImageShader.FullscreenTexture
{
    public class LitUvNormalBump : LitMultiTextureShader<Float3_Float3_Float3_Float3_Float2>
    {
        public LitUvNormalBump(int textureCount) : base("BareE.EZRend.ModelShader.Uv.UvNormalBump", textureCount)
        {
        }
    }

    

    public class InstancedMultiTextureShader<V,I> : InstancedShader<V,I>
             where V : unmanaged, IProvideVertexLayoutDescription
             where I: unmanaged, IProvideVertexLayoutDescription
    {
        private class TextureDescData
        {
            public TextureDescription? TexDesc;
            public SamplerDescription? SampDesc;

            internal TextureDescData(TextureDescription texDesc, SamplerDescription sampDesc)
            {
                TexDesc = texDesc; SampDesc = sampDesc;
            }
        }

        private class TextureData
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

        private TextureDescData[] textureDescriptions;
        private TextureData[] Textures;

        private int TextureCount;

        public SamplerFilter ColorTextureFilter = SamplerFilter.Anisotropic;





        public InstancedMultiTextureShader(String partial, int textureCount) : base(partial)
        {
            textureDescriptions = new TextureDescData[textureCount];
            Textures = new TextureData[textureCount];
            TextureCount = textureCount;
        }

        public override void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData, Matrix4x4 camMat, Matrix4x4 ModelMatrix)
        {
            base.Render(Trgt, cmds, sceneData, camMat, ModelMatrix);
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

    public class LitMultiTextureShader<V> : LitVertexOnlyShader<V>
         where V : unmanaged, IProvideVertexLayoutDescription
    {
        private TextureDescData[] textureDescriptions;
        private TextureData[] Textures;

        private int TextureCount;
        public SamplerFilter ColorTextureFilter = SamplerFilter.Anisotropic;


        public LitMultiTextureShader(String partial, int textureCount) : base(partial)
        {
            textureDescriptions = new TextureDescData[textureCount];
            Textures = new TextureData[textureCount];
            TextureCount = textureCount;
        }

        public override void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData, Matrix4x4 camMat, Matrix4x4 ModelMatrix)
        {
            base.Render(Trgt, cmds, sceneData, camMat, ModelMatrix);
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

    public class MultiTextureShader<V> : VertexOnlyShader<V>
     where V : unmanaged, IProvideVertexLayoutDescription
    {

        private TextureDescData[] textureDescriptions;
        private TextureData[] Textures;

        private int TextureCount;
        protected CommonData commondata;

        private DeviceBuffer dataBuffer;
        private ResourceLayout DataLayout;
        private ResourceSet DataSet;
        
        public SamplerFilter ColorTextureFilter = SamplerFilter.Anisotropic;

        public MultiTextureShader(String partial, int textureCount) : base(partial)
        {
            textureDescriptions = new TextureDescData[textureCount];
            Textures = new TextureData[textureCount];
            TextureCount = textureCount;
        }

        public override void UpdateBuffers(CommandList cmds)
        {
            cmds.UpdateBuffer(dataBuffer, 0, commondata);
            base.UpdateBuffers(cmds);
        }


        public override void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData, Matrix4x4 camMat, Matrix4x4 ModelMatrix)
        {
            if (sceneData!=null) commondata = sceneData.CommonData;
            else
            {
             
                commondata.u_resolution = new Vector2(Trgt.Width, Trgt.Height);
            }
            base.Render(Trgt, cmds, sceneData, camMat, ModelMatrix);
        }

        public override void Update(GraphicsDevice device)
        {
            base.Update(device);
        }
        DepthStencilStateDescription _depthStencilDesc= new DepthStencilStateDescription(
                        depthTestEnabled: true,
                        depthWriteEnabled: true,
                        comparisonKind: ComparisonKind.Less
                        );
        public override DepthStencilStateDescription DepthStencilDescription
        {
            get { return _depthStencilDesc; }
            set { _depthStencilDesc = value; }
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
            dataBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription(CommonData.Size, BufferUsage.UniformBuffer));
            dataBuffer.Name = "CommonData";

            DataLayout = device.ResourceFactory.CreateResourceLayout(
                                new ResourceLayoutDescription(
                                        new ResourceLayoutElementDescription("CommonData",
                                        ResourceKind.UniformBuffer, ShaderStages.Fragment | ShaderStages.Vertex)
                                    )
                                );
            DataSet = device.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(DataLayout,
                    dataBuffer
                    ));
            DataSet.Name = "CommonData";

            base.CreateResources(device);
        }

        protected override IEnumerable<ResourceLayout> CreateResourceLayout()
        {
            //Cameras/Lights
            foreach (var v in base.CreateResourceLayout())
                yield return v;
            yield return DataLayout;
            //Textures
            foreach (var v in Textures)
                yield return v.layout;
        }

        public override IEnumerable<ResourceSet> GetResourceSets()
        {
            foreach (var v in base.GetResourceSets())
                yield return v;
            yield return DataSet;
            //Textures
            foreach (var v in Textures)
                yield return v.set;
        }
    }

    internal class TextureDescData
    {
        public TextureDescription? TexDesc;
        public SamplerDescription? SampDesc;

        internal TextureDescData(TextureDescription texDesc, SamplerDescription sampDesc)
        {
            TexDesc = texDesc; SampDesc = sampDesc;
        }
    }
    internal class TextureData
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

}