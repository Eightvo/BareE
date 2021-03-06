using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.VertexTypes
{
    public struct Float3_Float4 : IProvideVertexLayoutDescription
    {
        public Vector3 Layout_0;
        public Vector4 Layout_1;

        public Float3_Float4(Vector3 v3, Vector4 v4)
        {
            Layout_0 = v3;
            Layout_1 = v4;
        }

        public uint SizeInBytes { get => (4 * 3) + (4 * 4); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }
}
