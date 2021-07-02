using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{
    public struct Float3_Float2 : IProvideVertexLayoutDescription
    {
        public Vector3 Layout_0;
        public Vector2 Layout_1;

        public Float3_Float2(Vector3 v1_3, Vector2 v2_2)
        {
            Layout_0 = v1_3;
            Layout_1 = v2_2;
        }

        public uint SizeInBytes { get => (4 * 3) + (4 * 2); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            { InstanceStepRate = instanceStepRate };
        }
    }
}