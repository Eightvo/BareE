using BareE.EZRend.VertexTypes;
using BareE.Rendering;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{

    public class ColorNormalShader : VertexOnlyShader<Float3_Float4_Float3>
    {
        public ColorNormalShader() : base("BareE.EZRend.ModelShader.Color.ColorNormal")
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