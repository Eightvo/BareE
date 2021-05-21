namespace BareE.EZRend.ImageShader.FullscreenTexture
{
    /*
    public class MultiTextureMonolithShader
    {
        struct MultiTextureVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Forward;
            public Vector3 BiTan;
            public Vector2 Uv;

            public MultiTextureVertex(Vector3 pos, Vector3 normal, Vector3 forward, Vector3 biTan, Vector2 uv)
            {
                Position = pos;
                Normal = normal;
                Forward = forward;
                BiTan = biTan;
                Uv = uv;
            }
            public uint SizeInBytes { get => (4 * 3) + (4 * 3) + (4 * 3) + (4 * 3) + (4 * 2); }

            public static VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
            {
                return new VertexLayoutDescription(
                    new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("forward", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("up", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                  );
            }
        }
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

        public int MaximumVerticies { get { return MaxVerts; } set { MaxVerts = value; MaxVertexCountDirty = true; } }
        public int VertexCount { get { return verts.Count; } }

        public String VertexShaderName { get; private set; }
        public String FragmentShaderName { get; private set; }

        public MultiTextureMonolithShader(String partialName) : this($"{partialName}.vert.spv", $"{partialName}.frag.spv") { }
        public MultiTextureMonolithShader(String vertexShaderName, String fragmentShaderName)
        {
            VertexShaderName = vertexShaderName;
            FragmentShaderName = fragmentShaderName;
        }

        List<MultiTextureMonolithShader> verts = new List<MultiTextureMonolithShader>();
        int MaxVerts = 100;

        bool MaxVertexCountDirty = true;
        bool VertexContentDirty = false;

        public virtual bool UseDepthStencil { get { return true; } }

        Pipeline GraphicsPipeline;
        DeviceBuffer VertexBuffer;

        DeviceBuffer CameraMatrixBuffer;
        ResourceLayout CameraMatrixResourceLayout;
        DeviceBuffer ModelMatrixBuffer;

        ResourceSet CameraResourceSet;
        Shader[] ShaderSet;
        ShaderSetDescription ShaderSetDesc;

        public void Clear()
        {
            verts.Clear();

            VertexContentDirty = true;
        }

        public virtual void AddVertex(MultiTextureMonolithShader vert)
        {
            if (verts.Count >= MaximumVerticies)
            {
                MaximumVerticies = MaximumVerticies + (int)Math.Ceiling(MaximumVerticies * 0.5f);
                MaxVertexCountDirty = true;
            }
            verts.Add(vert);
            VertexContentDirty = true;
        }
        public virtual Shader[] CreateShaders(ResourceFactory factory)
        {
            byte[] VertexShaderData = UTIL.FindFileData(VertexShaderName);
            byte[] FragShaderData = UTIL.FindFileData(FragmentShaderName);
            var VertDesc = new ShaderDescription(ShaderStages.Vertex, VertexShaderData, "main");
            var FragDesc = new ShaderDescription(ShaderStages.Fragment, FragShaderData, "main");
            return factory.CreateFromSpirv(VertDesc, FragDesc);
        }

        public virtual void CreateResources(GraphicsDevice device)
        {
            var factory = device.ResourceFactory;
            CameraMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            CameraMatrixBuffer.Name = "CamMatrixBuff";
            ModelMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            CameraMatrixBuffer.Name = "ModelMatrixBuff";
            CameraMatrixResourceLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("CameraMat", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("ModelMat", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
                    )
                );
            CameraResourceSet = factory.CreateResourceSet(new ResourceSetDescription(CameraMatrixResourceLayout, CameraMatrixBuffer, ModelMatrixBuffer));
            CameraResourceSet.Name = "CAMERAS";
            Console.WriteLine($"*Created Camera Layout. with Camera DataBuffers:{CameraMatrixBuffer.Name}  and {ModelMatrixBuffer.Name} set name:{CameraResourceSet.Name}");

            //Create and Compile the Shaders.
            ShaderSet = CreateShaders(factory);
            ShaderSetDesc = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { MultiTextureVertex.GetVertexLayoutDescription() },
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
        OutputDescription? _outputDesc;
        public void SetOutputDescription(OutputDescription odesc) { _outputDesc = odesc; }
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
        public virtual void Render(Framebuffer Trgt, CommandList cmds, Matrix4x4 CameraMatrix, Matrix4x4 ModelMatrix)
        {
            //if (!DrawToggle) return;
            //DrawToggle = false;
            cmds.UpdateBuffer(CameraMatrixBuffer, 0, CameraMatrix);
            cmds.UpdateBuffer(ModelMatrixBuffer, 0, ModelMatrix);
            UpdateBuffers(cmds);
            cmds.SetPipeline(this.GraphicsPipeline);

            Console.WriteLine($"Pipeline Resource Layout:");

            cmds.SetFramebuffer(Trgt);
            cmds.SetVertexBuffer(0, this.VertexBuffer);
            uint i = 0;
            var RssL = GetResourceSets().ToList();
            foreach (ResourceSet set in RssL)
            {
                //if (set as )
                Console.WriteLine($"Setting Resource Set {set.Name} into slot {i}");
                cmds.SetGraphicsResourceSet(i++, set);
            }

            cmds.Draw((uint)verts.Count);
        }
    }
    */
}