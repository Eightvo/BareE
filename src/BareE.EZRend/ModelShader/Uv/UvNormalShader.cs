using BareE.Rendering;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{
    public class UvNormalShader : VertexTextureShader<Float3_Float2_Float3>
    {
        public UvNormalShader() : base("BareE.EZRend.ModelShader.Uv.UvNormal")
        {
        }

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