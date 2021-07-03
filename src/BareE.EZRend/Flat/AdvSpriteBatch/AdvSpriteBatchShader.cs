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
    public struct AdvSpriteInstanceData : IProvideVertexLayoutDescription
    {
        public Vector4 UvBox;
        public Vector4 Rotation;
        public Vector4 PrimaryTint;
        public Vector4 SecondaryTint;

        public AdvSpriteInstanceData(Vector4 uvbox, Vector4 TRS, Vector4 primaryTint, Vector4 secondaryTint)
        {
            UvBox = uvbox;
            Rotation = TRS;
            PrimaryTint = primaryTint;
            SecondaryTint = secondaryTint;
        }

        public uint SizeInBytes { get => (4 * 4)+(4 * 4)+ (4 * 4)+ (4 * 4); }

        public VertexLayoutDescription GetVertexLayoutDescription(uint instanceStepRate = 0)
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("uvbox", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("rot", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("pTint", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("sTint", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
              )
            { InstanceStepRate = instanceStepRate };
        }
    }

    public class AdvSpriteBatchShader : InstancedMultiTextureShader<Float3_Float2, AdvSpriteInstanceData>
    {
        public AdvSpriteBatchShader() : base("BareE.EZRend.Flat.AdvSpriteBatch.AdvSpriteBatch",1) {
            this.ColorTextureFilter = SamplerFilter.MinPoint_MagPoint_MipPoint;
        }


        public void AddSprite(RectangleF uvBox, Vector2 translation, float rotation,Vector4 PrimaryColor, Vector4 SecondaryColor, float scale=1.0f)
        {
            this.AddInstance(new AdvSpriteInstanceData(new Vector4(uvBox.X, uvBox.Y, uvBox.Width, uvBox.Height), new Vector4(translation.X, translation.Y, rotation, scale), PrimaryColor, SecondaryColor));
        }

        public override DepthStencilStateDescription DepthStencilDescription
        {
            get => new DepthStencilStateDescription(
                        depthTestEnabled: true,
                        depthWriteEnabled: true,
                        comparisonKind: ComparisonKind.Always
                        );
        }

        
    }
}
