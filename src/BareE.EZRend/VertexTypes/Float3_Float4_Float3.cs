using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.VertexTypes
{
    public struct Float3_Float4_Float3 : IProvideVertexLayoutDescription
    {
        public Vector3 Layout_0;
        public Vector4 Layout_1;
        public Vector3 Layout_2;

        public Float3_Float4_Float3(Vector3 v1_3, Vector4 v2_4, Vector3 v3_3)
        {
            Layout_0 = v1_3;
            Layout_1 = v2_4;
            Layout_2 = v3_3;
        }

        public uint SizeInBytes { get => (4 * 3) + (4 * 4) + (4 * 3); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }
}