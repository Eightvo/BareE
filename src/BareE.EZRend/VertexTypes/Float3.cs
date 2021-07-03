using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.EZRend.VertexTypes
{

    public struct Float3 : IProvideVertexLayoutDescription
    {
        public Vector3 Layout_0;

        public Float3(Vector3 v3)
        {
            Layout_0 = v3;
        }

        public uint SizeInBytes { get => (4 * 3); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }
}
