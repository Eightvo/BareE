using System.Numerics;

namespace BareE.Rendering
{
    /// <summary>
    /// Draws a specified Framebuffer to cover the entire output window.
    /// </summary>
    public class FramebufferToScreen : VertexTextureShader<Pos3_Uv2>
    {
        public override bool UseDepthStencil { get; set; }

        public FramebufferToScreen() : base("BareE.Rendering.FullScreenTexture")
        {
            AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 0) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 1) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(1, -1, 0), Uv = new Vector2(1, 0) });

            AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 0) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, 1, 0), Uv = new Vector2(0, 1) });
            AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 1) });
        }
    }
}