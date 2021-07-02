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

    public struct Float4 : IProvideVertexLayoutDescription
    {
        public Vector4 Layout_1;

        public Float4(Vector4 v4)
        {
            Layout_1 = v4;
        }

        public uint SizeInBytes { get => (4 * 4); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }
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
