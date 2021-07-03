using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.VertexTypes
{
    public struct Float4_Float4 : IProvideVertexLayoutDescription
    {
        public Vector4 Layout_0;
        public Vector4 Layout_1;

        public Float4_Float4(Vector4 v14, Vector4 v4)
        {
            Layout_0 = v14;
            Layout_1 = v4;
        }

        public uint SizeInBytes { get => (4 * 4) + (4 * 4); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }
}
