using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{
    public struct Float3_Float2_Float3_Float3 : IProvideVertexLayoutDescription
    {
        public Vector3 Layout_0;
        public Vector2 Layout_1;
        public Vector3 Layout_2;
        public Vector3 Layout_3;

        public Float3_Float2_Float3_Float3(Vector3 v1_3, Vector2 v2_2, Vector3 v3_3, Vector3 v3_4)
        {
            Layout_0 = v1_3;
            Layout_1 = v2_2;
            Layout_2 = v3_3;
            Layout_3 = v3_4;
        }

        public uint SizeInBytes { get => (4 * 3) + (4 * 2) + (4 * 3) + (4 * 3); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Tangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }
}