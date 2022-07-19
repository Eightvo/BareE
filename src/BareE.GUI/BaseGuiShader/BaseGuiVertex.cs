using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.GUI
{
    public struct BaseGuiVertex : IProvideVertexLayoutDescription
    {
        public Vector3 Pos;
        public Vector3 UvT;
        public Vector4 Color;

        public BaseGuiVertex(Vector3 pos, Vector3 uv, Vector4 Clr)
        {
            Pos = pos;
            UvT = uv;
            Color = Clr;
        }

        public uint SizeInBytes { get => (3 * 4) + (3 * 4)+(4*4); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("Pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("UVT", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                )
            { InstanceStepRate = instanceStepRate };
        }
    }
}
