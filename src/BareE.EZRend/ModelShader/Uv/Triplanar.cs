using BareE.Rendering;

using Veldrid;

namespace BareE.EZRend.ModelShader.Color
{
    public class Triplanar : VertexTextureShader<Float3_Float2_Float3>
    {
        public Triplanar() : base("BareE.EZRend.ModelShader.Uv.TriplanarP")
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