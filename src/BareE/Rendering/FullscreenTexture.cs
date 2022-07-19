using System.Numerics;

using Veldrid;

namespace BareE.Rendering
{
    /// <summary>
    /// Draws a specified texture to cover the entire output window.
    /// </summary>
    public class FullScreenTexture : VertexTextureShader<Pos3_Uv2>
    {
        internal bool _flip = false;
        public bool Flip { get { return _flip; } set { if (value != _flip) { _flip = value; AddVerticies(); } } }
        public override bool UseDepthStencil { get => true; }

        private void AddVerticies()
        {
            if (_flip)
            {
                Clear();
                AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 0) });
                AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 1) });
                AddVertex(new Pos3_Uv2() { Position = new Vector3(1, -1, 0), Uv = new Vector2(1, 0) });

                AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 0) });
                AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, 1, 0), Uv = new Vector2(0, 1) });
                AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 1) });
            }
            else
            {
                Clear();
                AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 1) });
                AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 0) });
                AddVertex(new Pos3_Uv2() { Position = new Vector3(1, -1, 0), Uv = new Vector2(1, 1) });

                AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, -1, 0), Uv = new Vector2(0, 1) });
                AddVertex(new Pos3_Uv2() { Position = new Vector3(-1, 1, 0), Uv = new Vector2(0, 0) });
                AddVertex(new Pos3_Uv2() { Position = new Vector3(1, 1, 0), Uv = new Vector2(1, 0) });

            }

        }
        public FullScreenTexture() : base("BareE.Rendering.FullScreenTexture")
        {
            AddVerticies();
        }
    }
}