using BareE.Rendering;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{
    public class TriplanarPBump : VertexTextureShader<Float3_Float2_Float3>
    {
        public TriplanarPBump() : base("BareE.EZRend.ModelShader.Uv.TriplanarPBump")
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
    }
}