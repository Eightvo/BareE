using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BareE.RNG
{
    public static class RngHelper
    {
        public static int NextInt(this IPrng src, int max)
        {
            return NextInt(src, 0, max);
        }
        public static int NextInt(this IPrng src, int min, int max)
        {
            return (int)(src.Next() * (max - min)) + min;
        }
        public static T NextElement<T>(this IPrng src, IEnumerable<T> set)
        {
            return set.ElementAt(NextInt(src, 0, set.Count()));
        }
        public static Vector2 NextUnitVector2(this IPrng src, Vector2 min, Vector2 max)
        {
            return Vector2.Normalize(new Vector2((float)src.Next(), (float)src.Next()));
        }
        public static Vector2 NextPointNear(this IPrng src, Vector2 location, float radius)
        {
            var rX = (src.Next() * (2 * radius)) - radius;
            var rY = (src.Next() * (2 * radius)) - radius;
            return location + new Vector2((float)rX, (float)rY);
        }



    }
}
