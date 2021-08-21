using System;
using System.Numerics;

namespace BareE
{
    public partial class Extentions
    {
        public static Vector2 Center(this SixLabors.ImageSharp.RectangleF r)
        {
            return new Vector2(r.Right + r.Left, r.Top + r.Bottom) / 2.0f;
        }
        public static bool HasArea(this SixLabors.ImageSharp.Rectangle r)
        {
            return r.Width > 0 && r.Height > 0;
        }

        public static bool HasArea(this Veldrid.Rectangle rect)
        {
            return (rect.Width > 0 && rect.Height > 0);
        }

        public static double ToAngle(this Vector2 vec)
        {
            if (vec.X < 0)
            {
                return 360 - (Math.Atan2(vec.X, vec.Y) * MathHelper.RadToDeg(1) * -1);
            }
            else
            {
                return Math.Atan2(vec.X, vec.Y) * MathHelper.RadToDeg(1);
            }
        }

        public static float ToFacingAngle(this Vector2 vec)
        {
            return MathHelper.ToFacingAngle(vec);
        }

    }
}