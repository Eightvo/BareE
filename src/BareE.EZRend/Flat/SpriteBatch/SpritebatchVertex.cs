using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.Flat
{
    public struct SpritebatchVertex : IProvideVertexLayoutDescription
    {
        public Vector3 Position;
        public Vector2 Uv;

        public SpritebatchVertex(Vector3 pos, Vector2 uv)
        {
            Position = pos;
            Uv = uv;
        }
        public uint SizeInBytes { get { return 4 * 3 + 4 * 2; } }


        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
              );
        }
    }
}
