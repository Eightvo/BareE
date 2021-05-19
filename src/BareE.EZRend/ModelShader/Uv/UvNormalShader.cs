using BareE.Rendering;

using BareE.EZRend.ImageShader.FullscreenTexture;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{
    public struct Float3_Float2:IProvideVertexLayoutDescription
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
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)              );
        }

    }
    public struct Float3_Float2_Float3 : IProvideVertexLayoutDescription
    {
        public Vector3 Layout_0;
        public Vector2 Layout_1;
        public Vector3 Layout_2;

        public Float3_Float2_Float3(Vector3 v1_3, Vector2 v2_2, Vector3 v3_3)
        {
            Layout_0 = v1_3;
            Layout_1 = v2_2;
            Layout_2 = v3_3;
        }
        public uint SizeInBytes { get => (4 * 3) + (4 * 2)+(4*3); }


        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
              );
        }
    }
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
        public uint SizeInBytes { get => (4 * 3) + (4 * 3) + (4 * 3) + (4 * 3) + (4 * 2) ; }


        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("forward", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("up", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
              );
        }
    }

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
        public uint SizeInBytes { get => (4 * 3) + (4 * 2) + (4 * 3)+ (4*3); }


        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Tangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
              );
        }
    }
    public class Triplanar:VertexTextureShader<Float3_Float2_Float3>
    {
        public Triplanar() : base("BareE.EZRend.ModelShader.Uv.TriplanarP") { }
        public override DepthStencilStateDescription DepthStencilDescription
        {
            get => new DepthStencilStateDescription(
                        depthTestEnabled: true,
                        depthWriteEnabled: true,
                        comparisonKind: ComparisonKind.Less
                        );
        }

    }
    public class UvNormalBump:VertexTextureShader<Float3_Float3_Float3_Float3_Float2>
    {
        public UvNormalBump() : base("BareE.EZRend.ModelShader.Uv.UvNormalBump") { }
        public override DepthStencilStateDescription DepthStencilDescription
        {
            get => new DepthStencilStateDescription(
                        depthTestEnabled: true,
                        depthWriteEnabled: true,
                        comparisonKind: ComparisonKind.Less
                        );
        }
        public void AddVertex(Vector3 pos, Vector3 normal, Vector3 fwd, Vector3 up, Vector2 uv)
        {
            base.AddVertex(new Float3_Float3_Float3_Float3_Float2(pos, normal, fwd, up, uv));
        }
    }
    public class TriplanarPBump : VertexTextureShader<Float3_Float2_Float3>
    {
        public TriplanarPBump() : base("BareE.EZRend.ModelShader.Uv.TriplanarPBump") { }
        public override DepthStencilStateDescription DepthStencilDescription
        {
            get => new DepthStencilStateDescription(
                        depthTestEnabled: true,
                        depthWriteEnabled: true,
                        comparisonKind: ComparisonKind.Less
                        );
        }

    }

    public class UvNormalShader:VertexTextureShader<Float3_Float2_Float3>
    {
        public UvNormalShader() : base("BareE.EZRend.ModelShader.Uv.UvNormal") { }
        public override DepthStencilStateDescription DepthStencilDescription
        {
            get => new DepthStencilStateDescription(
                        depthTestEnabled: true,
                        depthWriteEnabled: true,
                        comparisonKind: ComparisonKind.LessEqual
                        );
        }
    }
}
