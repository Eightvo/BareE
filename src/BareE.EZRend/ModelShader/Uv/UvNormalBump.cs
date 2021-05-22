using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{
    public class UvNormalBump : VertexTextureShader<Float3_Float3_Float3_Float3_Float2>
    {
        public UvNormalBump() : base("BareE.EZRend.ModelShader.Uv.UvNormalBump")
        {
        }

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
}