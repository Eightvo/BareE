using BareE.EZRend.VertexTypes;
using BareE.Rendering;

using System.Numerics;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{


    public class ColorShader : VertexOnlyShader<Float3_Float4>
    {
        public ColorShader() : base("BareE.EZRend.ModelShader.Color.Color")
        {
        }

        public override DepthStencilStateDescription DepthStencilDescription
        {
            get
            {
                return new DepthStencilStateDescription(
                     depthTestEnabled: true,
                     depthWriteEnabled: true,
                     comparisonKind: ComparisonKind.LessEqual);
            }
        }
    }
}