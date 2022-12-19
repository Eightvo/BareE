using BareE.Rendering;

using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BareE.EZRend.Flat
{
    public class SpriteBatch : VertexTextureShader<SpritebatchVertex>
    {
        public SpriteBatch() : base("BareE.EZRend.Flat.SpriteBatch.Spritebatch") { }

        public void AddSprite(RectangleF dest, RectangleF src, float z=0)
        {
            Vector3 p0 = new Vector3(dest.Left, dest.Bottom, z);
            Vector3 p1 = new Vector3(dest.Left, dest.Top, z);
            Vector3 p2 = new Vector3(dest.Right, dest.Top, z);
            Vector3 p3 = new Vector3(dest.Right, dest.Bottom, z);
            Vector2 uv0 = new Vector2(src.Left, src.Top);
            Vector2 uv1 = new Vector2(src.Left, src.Bottom);
            Vector2 uv2 = new Vector2(src.Right, src.Bottom);
            Vector2 uv3 = new Vector2(src.Right, src.Top);
            AddSprite(p0, p1, p2, p3, uv0, uv1, uv2, uv3);
        }
        public void AddSprite(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            AddVertex(new SpritebatchVertex() { Position = p0, Uv = uv0 });
            AddVertex(new SpritebatchVertex() { Position = p2, Uv = uv2 });
            AddVertex(new SpritebatchVertex() { Position = p1, Uv = uv1 });

            AddVertex(new SpritebatchVertex() { Position = p0, Uv = uv0 });
            AddVertex(new SpritebatchVertex() { Position = p3, Uv = uv3 });
            AddVertex(new SpritebatchVertex() { Position = p2, Uv = uv2 });
        }

    }
}
