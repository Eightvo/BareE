using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{
    public struct Float3_Float3_Float3_Float3_Float2 : IProvideVertexLayoutDescription
    {
        public Vector3 Layout_0;
        public Vector3 Layout_1;
        public Vector3 Layout_2;
        public Vector3 Layout_3;
        public Vector2 Layout_4;

        public Float3_Float3_Float3_Float3_Float2(Vector3 v1_3, Vector3 v2_3, Vector3 v3_3, Vector3 v4_3, Vector2 v5_2)
        {
            Layout_0 = v1_3;
            Layout_1 = v2_3;
            Layout_2 = v3_3;
            Layout_3 = v4_3;
            Layout_4 = v5_2;
        }

        public uint SizeInBytes { get => (4 * 3) + (4 * 3) + (4 * 3) + (4 * 3) + (4 * 2); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("forward", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("up", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }
}