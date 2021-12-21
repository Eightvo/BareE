using BareE.EZRend.ImageShader.FullscreenTexture;
using BareE.EZRend.ModelShader.Color;
using BareE.EZRend.VertexTypes;
using BareE.Rendering;

using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.EZRend.Flat
{
    public struct InstSpriteInstanceData : IProvideVertexLayoutDescription
    {
        public Vector4 UvBox;
        public Vector4 Rotation;

        public InstSpriteInstanceData(Vector4 uvbox, Vector4 TRS)
        {
            UvBox = uvbox;
            Rotation = TRS;
        }

        public uint SizeInBytes { get => (4 * 4)+(4 * 4)+ (4 * 4)+ (4 * 4); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("uvbox", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("rot", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }

    public class InstSpriteBatchShader : InstancedMultiTextureShader<Float3_Float2, InstSpriteInstanceData>
    {
        public InstSpriteBatchShader() : base("BareE.EZRend.Flat.AdvSpriteBatch.InstSpriteBatch",1) {
            this.ColorTextureFilter = SamplerFilter.MinLinear_MagLinear_MipLinear;
        }


        public void AddSprite(RectangleF uvBox, Vector2 translation, float rotation, float scale=1.0f)
        {
            this.AddInstance(new InstSpriteInstanceData(new Vector4(uvBox.Left, uvBox.Top, uvBox.Width, uvBox.Height), new Vector4(translation.X, translation.Y, rotation, scale)));
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
