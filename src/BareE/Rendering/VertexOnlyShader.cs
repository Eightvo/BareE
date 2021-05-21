using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Veldrid;
using Veldrid.SPIRV;

namespace BareE.Rendering
{
    public class VertexOnlyShader<V> : IRenderUnit
         where V : unmanaged, IProvideVertexLayoutDescription
    {
        private V VertexInstance = new V();
        public VertexOverflowBehaviour VertexOverflowBehaviour = VertexOverflowBehaviour.EXPAND;
        public float VertexOverflowExpansionFactor { get; set; } = 0.5f;
        public int MaximumVerticies { get { return MaxVerts; } set { MaxVerts = value; MaxVertexCountDirty = true; } }
        public int VertexCount { get { return verts.Count; } }

        public String VertexShaderName { get; private set; }
        public String FragmentShaderName { get; private set; }

        public VertexOnlyShader(String partialName) : this($"{partialName}.vert.spv", $"{partialName}.frag.spv")
        {
        }

        public VertexOnlyShader(String vertexShaderName, String fragmentShaderName)
        {
            // PixelFormat = NativePixelFormat;
            VertexShaderName = vertexShaderName;
            FragmentShaderName = fragmentShaderName;
        }

        private List<V> verts = new List<V>();
        private int MaxVerts = 100;

        private bool MaxVertexCountDirty = true;
        private bool VertexContentDirty = false;

        public virtual bool UseDepthStencil { get { return true; } }
        private bool DrawReady = false;

        private PixelFormat NativePixelFormat;
        private Pipeline GraphicsPipeline;
        private DeviceBuffer VertexBuffer;

        private DeviceBuffer CameraMatrixBuffer;
        private ResourceLayout CameraMatrixResourceLayout;
        private DeviceBuffer ModelMatrixBuffer;

        private ResourceSet CameraResourceSet;
        private Shader[] ShaderSet;
        private ShaderSetDescription ShaderSetDesc;
        private bool DrawToggle = false;

        public void Clear()
        {
            verts.Clear();

            VertexContentDirty = true;
        }

        public virtual void AddVertex(V vert)
        {
            if (verts.Count >= MaximumVerticies)
            {
                switch (VertexOverflowBehaviour)
                {
                    case VertexOverflowBehaviour.EXCEPTION:
                        throw new Exception($"Vertex List exceeded {MaximumVerticies} Maximum");
                        return;

                    case VertexOverflowBehaviour.IGNORE:
                        throw new Exception($"Vertex List exceeded {MaximumVerticies} Maximum");
                        return;

                    case VertexOverflowBehaviour.EXPAND:
                        MaximumVerticies = MaximumVerticies + (int)Math.Ceiling(MaximumVerticies * VertexOverflowExpansionFactor);
                        MaxVertexCountDirty = true;
                        break;
                }
            }
            verts.Add(vert);
            VertexContentDirty = true;
        }

        public uint NextMultipleOf16(uint ival)
        {
            return (uint)(Math.Floor(ival / 16.0f) * 16 + 16);
        }

        public virtual Shader[] CreateShaders(ResourceFactory factory)
        {
            byte[] VertexShaderData = AssetManager.FindFileData(VertexShaderName);
            byte[] FragShaderData = AssetManager.FindFileData(FragmentShaderName);
            var VertDesc = new ShaderDescription(ShaderStages.Vertex, VertexShaderData, "main");
            var FragDesc = new ShaderDescription(ShaderStages.Fragment, FragShaderData, "main");
            return factory.CreateFromSpirv(VertDesc, FragDesc);
        }

        public virtual void CreateResources(GraphicsDevice device)
        {
            NativePixelFormat = device.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
            var factory = device.ResourceFactory;
            CameraMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            CameraMatrixBuffer.Name = "CamMatrixBuff";
            ModelMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            CameraMatrixBuffer.Name = "ModelMatrixBuff";
            CameraMatrixResourceLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("CameraMat", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                        new ResourceLayoutElementDescription("ModelMat", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                    )
                );
            CameraResourceSet = factory.CreateResourceSet(new ResourceSetDescription(CameraMatrixResourceLayout, CameraMatrixBuffer, ModelMatrixBuffer));
            CameraResourceSet.Name = "CAMERAS";

            //Create and Compile the Shaders.
            ShaderSet = CreateShaders(factory);
            ShaderSetDesc = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { VertexInstance.GetVertexLayoutDescription() },
                shaders: ShaderSet
                );

            CreatePipeLine(device);
        }

        public virtual BlendStateDescription BlendState { get; set; } = BlendStateDescription.SingleOverrideBlend;

        public virtual DepthStencilStateDescription DepthStencilDescription { get; } =
            new DepthStencilStateDescription(
                depthTestEnabled: false,
                depthWriteEnabled: false,
                comparisonKind: ComparisonKind.Always);

        public RasterizerStateDescription RastorizerDescription { get; set; } =
            new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: false,
                    scissorTestEnabled: true
                );

        protected PrimitiveTopology PrimitiveTopology { get; set; } = PrimitiveTopology.TriangleList;

        protected virtual IEnumerable<ResourceLayout> CreateResourceLayout()
        {
            yield return CameraMatrixResourceLayout;
        }

        public virtual IEnumerable<ResourceSet> GetResourceSets()
        {
            yield return this.CameraResourceSet;
        }

        private OutputDescription? _outputDesc;

        public void SetOutputDescription(OutputDescription odesc)
        {
            _outputDesc = odesc;
        }

        protected virtual OutputDescription CreateOutputDescription(PixelFormat pixelFormat, bool withDepthstencil = false)
        {
            if (_outputDesc != null) return _outputDesc.Value;
            if (withDepthstencil)
            {
                return new OutputDescription()
                {
                    ColorAttachments = new OutputAttachmentDescription[]
                   {
                   new OutputAttachmentDescription(pixelFormat)
                   },
                    DepthAttachment = new OutputAttachmentDescription(PixelFormat.R32_Float),
                    SampleCount = TextureSampleCount.Count1
                };
            }
            else
            {
                return new OutputDescription()
                {
                    ColorAttachments = new OutputAttachmentDescription[]
                   {
                   new OutputAttachmentDescription(pixelFormat)
                   },
                    SampleCount = TextureSampleCount.Count1
                };
            }
        }

        protected virtual void CreatePipeLine(GraphicsDevice device)
        {
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = this.BlendState;
            pipelineDescription.DepthStencilState = this.DepthStencilDescription;
            pipelineDescription.RasterizerState = this.RastorizerDescription;
            pipelineDescription.PrimitiveTopology = this.PrimitiveTopology;
            var rl = CreateResourceLayout().ToList().ToArray();
            pipelineDescription.ResourceLayouts = rl;

            pipelineDescription.ShaderSet = ShaderSetDesc;

            var oDesc = CreateOutputDescription(NativePixelFormat, this.UseDepthStencil);

            pipelineDescription.Outputs = oDesc;
            GraphicsPipeline = device.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
        }

        public virtual void Update(GraphicsDevice device)
        {
            if (verts.Count == 0) return;
            if (MaxVertexCountDirty)
            {
                if (VertexBuffer != null) VertexBuffer.Dispose();
                VertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)MaxVerts * this.VertexInstance.SizeInBytes, BufferUsage.VertexBuffer));
                MaxVertexCountDirty = false;
            }
            if (VertexContentDirty)
            {
                device.UpdateBuffer<V>(VertexBuffer, 0, verts.ToArray());
                VertexContentDirty = false;
            }
            DrawToggle = true;
        }

        public virtual void UpdateBuffers(CommandList cmds)
        {
        }

        public virtual void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData, Matrix4x4 CameraMatrix, Matrix4x4 ModelMatrix)
        {
            if (VertexBuffer == null) return;
            cmds.UpdateBuffer(CameraMatrixBuffer, 0, CameraMatrix);
            cmds.UpdateBuffer(ModelMatrixBuffer, 0, ModelMatrix);
            UpdateBuffers(cmds);
            cmds.SetPipeline(this.GraphicsPipeline);

            cmds.SetFramebuffer(Trgt);
            cmds.SetVertexBuffer(0, this.VertexBuffer);
            uint i = 0;
            var RssL = GetResourceSets().ToList();
            foreach (ResourceSet set in RssL)
            {
                //if (set as )
                cmds.SetGraphicsResourceSet(i++, set);
            }

            cmds.Draw((uint)verts.Count);
        }
    }
}