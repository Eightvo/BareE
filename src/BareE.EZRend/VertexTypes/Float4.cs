using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.VertexTypes
{
    public struct Float4 : IProvideVertexLayoutDescription
    {
        public Vector4 Layout_1;

        public Float4(Vector4 v4)
        {
            Layout_1 = v4;
        }

        public uint SizeInBytes { get => (4 * 4); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }
}
