using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Numerics;

using Veldrid;
using Veldrid.SPIRV;

namespace BareE.EZRend
{
    public class DiskShaderV1 : IRenderUnit
    {
        private struct DiskShaderVertex : IProvideVertexLayoutDescription
        {
            public Vector2 Position;
            public Vector3 DiameterAlphaHeight;
            public int ColorIndex;

            public uint SizeInBytes
            {
                get
                {
                    return (2 * 4) + (3 * 4) + 8;
                }
            }

            public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
            {
                return new VertexLayoutDescription(
                     new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                     new VertexElementDescription("DiameterAlphaHeight", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                     new VertexElementDescription("ColorIndex", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1)
                    )
                { InstanceStepRate = instanceStepRate };
            }

            public static DiskShaderVertex Instance = new DiskShaderVertex();
        }

        private struct DiskShaderConfig
        {
            public float time;
            public float seed;
            public int res_x;
            public int res_y;
            public int mpos_x;
            public int mpos_y;
            public int PalletSize;

            public uint SizeInBytes
            {
                get
                {
                    return (2 * 4) + (5 * 4);
                }
            }

            public static DiskShaderConfig Instance = new DiskShaderConfig();
        }

        public void SetPallet(Texture texture)
        {
            trgtPalletTexture = texture;
            ///PalletTexture = texture;
            TextureDirty = true;
        }

        private List<DiskShaderVertex> verts = new List<DiskShaderVertex>();
        private VertexOverflowBehaviour VertOverflowBehaviour = VertexOverflowBehaviour.EXPAND;
        private int MaximumVerts = 100;
        private float ExpansionFactor = 0.5f;
        private bool MaxVertexCountDirty = true;
        private bool VertexContentDirty = false;

        private DiskShaderConfig Config;
        private bool ConfigDirty = true;

        private Sampler PalletSampler;
        private TextureView PalletTextureView;
        private Texture PalletTexture;
        private Texture trgtPalletTexture = null;
        private bool TextureDirty = false;

        private bool DrawToggle = false;

        private DeviceBuffer VertexBuffer;
        private DeviceBuffer ProjModelBuffer;
        private DeviceBuffer ConfigBuffer;

        private Shader[] DiskShaders;
        private Pipeline DiskShaderPipeline;

        private ResourceSet ModelProjection;
        private ResourceSet ConfigRS;

        public void ClearVerts()
        {
            verts.Clear();
        }

        public void AddVert(Vector2 pos, int colorIndx, Vector3 DiamAplhaHght)
        {
            AddVert(new DiskShaderVertex()
            {
                Position = pos,
                ColorIndex = colorIndx,
                DiameterAlphaHeight = DiamAplhaHght
            });
        }

        private void AddVert(DiskShaderVertex vert)
        {
            if (verts.Count >= MaximumVerts)
            {
                switch (VertOverflowBehaviour)
                {
                    case VertexOverflowBehaviour.EXCEPTION:
                        new Exception($"Vertex List exceeded {MaximumVerts} Maximum");
                        return;

                    case VertexOverflowBehaviour.IGNORE:
                        //LibrarySettings.RaiseEZRendWarning(this, new Exception($"Vertex List exceeded {MaximumVerts} Maximum"));
                        return;

                    case VertexOverflowBehaviour.EXPAND:
                        MaximumVerts = MaximumVerts + (int)Math.Ceiling(MaximumVerts * ExpansionFactor);
                        MaxVertexCountDirty = true;
                        break;
                }
            }
            verts.Add(vert);
            VertexContentDirty = true;
        }

        private PixelFormat NativePixelFormat;
        ShaderDescription ByName(String resource, ShaderStages stage)
        {
            List<byte> data = new List<byte>();
            using (var rdr = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                int nxt = rdr.ReadByte();
                while (nxt != -1)
                {
                    data.Add((byte)nxt);
                    nxt = rdr.ReadByte();
                }
            }
            byte[] bytes = data.ToArray();
            return new ShaderDescription(stage, bytes, "main");
        }

        private Shader[] CreateShaderSet(ResourceFactory factory, String res)
        {
            var vert = ByName($"{res}.vert.spv", ShaderStages.Vertex);
            var frag = ByName($"{res}.frag.spv", ShaderStages.Fragment);
            return factory.CreateFromSpirv(vert, frag);
        }

        public void CreateResources(GraphicsDevice device)
        {
            NativePixelFormat = device.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
            var factory = device.ResourceFactory;

            //Create The ResourceSet and Buffer required for the ModelProjection Matrix to be passed
            ProjModelBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            var projViewLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ModelProjection", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )
            );
            ModelProjection = factory.CreateResourceSet(new ResourceSetDescription(projViewLayout, ProjModelBuffer));

            //Create the resource set and Buffers for the
            //Config, Pallet and Pallet sampler.
            Config = new DiskShaderConfig()
            {
                PalletSize = 8
            };
            ConfigBuffer = factory.CreateBuffer(new BufferDescription(MathHelper.NxtMultipleOfSixteen(DiskShaderConfig.Instance.SizeInBytes), BufferUsage.UniformBuffer));
            PalletTexture = AssetManager.LoadTexture("BareE.EZRend.palletts.rng1.png", device, true);
            PalletTextureView = factory.CreateTextureView(PalletTexture);

            PalletSampler = factory.CreateSampler(new SamplerDescription()
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
                ComparisonKind = ComparisonKind.Always
            });
            var DiskShaderLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Config", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("PalletTexture", ResourceKind.TextureReadOnly, ShaderStages.Vertex | ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("PalletSampler", ResourceKind.Sampler, ShaderStages.Vertex | ShaderStages.Fragment)
                    )
                );
            ConfigRS = factory.CreateResourceSet(new ResourceSetDescription(DiskShaderLayout,
                ConfigBuffer,
                PalletTextureView,
                PalletSampler));

            //Create and Compile the Shaders.
            DiskShaders = CreateShaderSet(device.ResourceFactory, "BareE.EZRend.Novelty.DiskShader.DiskShader_V1");
            var DiskShadersDescription = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { DiskShaderVertex.Instance.GetVertexLayoutDescription() },
                shaders: DiskShaders
                );

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: false,
                depthWriteEnabled: false,
                comparisonKind: ComparisonKind.Always);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: false,
                scissorTestEnabled: true
                );
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = new ResourceLayout[2]{
                projViewLayout,
                DiskShaderLayout
            };

            pipelineDescription.ShaderSet = DiskShadersDescription;

            pipelineDescription.Outputs = oDesc;
            DiskShaderPipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        }

        private OutputDescription oDesc;

        public void SetOutputDescription(OutputDescription oD)
        {
            oDesc = oD;
        }

        public void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider scenedata, Matrix4x4 cameraMatrix, Matrix4x4 modelMatrix)
        {
            if (!DrawToggle) return;
            var CameraMatrix = cameraMatrix;
            DrawToggle = false;
            cmds.UpdateBuffer(ProjModelBuffer, 0, CameraMatrix);
            if (TextureDirty)
            {
                cmds.CopyTexture(trgtPalletTexture, PalletTexture);
                TextureDirty = false;
            }
            cmds.SetFramebuffer(Trgt);
            cmds.SetVertexBuffer(0, VertexBuffer);
            cmds.SetPipeline(DiskShaderPipeline);

            cmds.SetGraphicsResourceSet(0, ModelProjection);
            cmds.SetGraphicsResourceSet(1, ConfigRS);

            cmds.Draw((uint)verts.Count);
        }

        public void Update(GraphicsDevice device)
        {
            if (verts.Count == 0) return;
            if (MaxVertexCountDirty)
            {
                if (VertexBuffer != null) VertexBuffer.Dispose();
                VertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)MaximumVerts * DiskShaderVertex.Instance.SizeInBytes, BufferUsage.VertexBuffer));
                MaxVertexCountDirty = false;
            }
            if (VertexContentDirty)
            {
                device.UpdateBuffer<DiskShaderVertex>(VertexBuffer, 0, verts.ToArray());
                VertexContentDirty = false;
            }
            if (ConfigDirty)
            {
                device.UpdateBuffer<DiskShaderConfig>(ConfigBuffer, 0, Config);
                ConfigDirty = false;
            }
            //            if (TextureDirty)
            //            {
            //device.UpdateTexture(PalletTexture, trgtPalletTexture., trgtPalletTexture.Width * trgtPalletTexture.Height, 0, 0, 0, 8, 1, 1, 0, 0);
            //device.UpdateTexture(PalletTexture, )
            //PalletTextureView.Dispose();
            // PalletTextureView = device.ResourceFactory.CreateTextureView(PalletTexture);
            //                TextureDirty = false;
            //            }
            DrawToggle = true;
        }
    }
}