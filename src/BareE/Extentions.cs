using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BareE
{
    public partial class Extentions
    {
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
