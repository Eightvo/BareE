using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.Rendering
{
    public class InstancedShader<V,I>:VertexOnlyShader<V>
        where V:unmanaged, IProvideVertexLayoutDescription
        where I:unmanaged, IProvideVertexLayoutDescription
    {
        private static I InstanceInstance = new I();
        public List<I> InstanceData = new List<I>();
        private int MaxInstances = 100;
        private bool MaxInstanceCountDirty = true;
        private bool InstanceContentDirty = true;

        private DeviceBuffer InstanceDataBuffer;
        public InstancedShader(String partial) : base(partial) { }

        public InstancedShader(String vertShadername, String fragShaderName) : base(vertShadername, fragShaderName) { }


        public int InstanceCount { get { return InstanceData.Count; } }
        public void ClearInstanceData()
        {
            InstanceData.Clear();
            InstanceContentDirty = true;
        }

        public virtual void AddInstance(I instace)
        {
            if (InstanceData.Count>=MaxInstances)
            {
                 switch(VertexOverflowBehaviour)
                {
                    case VertexOverflowBehaviour.EXCEPTION:
                        throw new Exception($"Instance List exceeded {MaxInstances} Instances.");
                    case VertexOverflowBehaviour.IGNORE:
                        throw new Exception($"Instance List exceeded {MaxInstances} Instances.");
                    case VertexOverflowBehaviour.EXPAND:
                        MaxInstances = MaxInstances + (int)Math.Ceiling(MaxInstances * VertexOverflowExpansionFactor);
                        MaxInstanceCountDirty = true;
                        break;
                }
            }
            InstanceData.Add(instace);
            InstanceContentDirty = true;
        }

        public override IEnumerable<VertexLayoutDescription> GetVertexLayoutDescriptions()
        {
            return base.GetVertexLayoutDescriptions().Union(new List<VertexLayoutDescription>() {InstanceInstance.GetVertexLayoutDescription(1) });
        }

        public override void CreateResources(GraphicsDevice device)
        {
            base.CreateResources(device);
        }
        public override void Update(GraphicsDevice device)
        {
            if (InstanceData.Count == 0) return;
            if (MaxInstanceCountDirty)
            {
                if (InstanceDataBuffer != null) InstanceDataBuffer.Dispose();
                InstanceDataBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)MaxInstances * InstanceInstance.SizeInBytes, BufferUsage.VertexBuffer));
                MaxInstanceCountDirty = false;
            }
            if (InstanceContentDirty)
            {
                device.UpdateBuffer<I>(InstanceDataBuffer, 0, InstanceData.ToArray());
                InstanceContentDirty = false;
            }
            base.Update(device);
        }
        
        public override void Render(Framebuffer Trgt, CommandList cmds, ISceneDataProvider sceneData, Matrix4x4 CameraMatrix, Matrix4x4 ModelMatrix)
        {
            cmds.SetVertexBuffer(1, InstanceDataBuffer);
            base.BaseRender(Trgt, cmds, sceneData, CameraMatrix, ModelMatrix);
            cmds.Draw((uint)VertexCount, (uint)InstanceCount, 0, 0);           
        }

    }
}
