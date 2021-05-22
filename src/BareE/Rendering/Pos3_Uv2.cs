using System.Numerics;

using Veldrid;

namespace BareE.Rendering
{
    /// <summary>
    /// Represents a vertex data of vec3; vec2
    /// </summary>
    public struct Pos3_Uv2 : IProvideVertexLayoutDescription
    {
        public Vector3 Position;
        public Vector2 Uv;

        public Pos3_Uv2(Vector3 pos, Vector2 uv)
        {
            Position = pos;
            Uv = uv;
        }

        public uint SizeInBytes { get => (3 * 4) + (2 * 4); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("Pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                );
        }
    }
}