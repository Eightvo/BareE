using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BareE
{
    public static partial class Extentions
    {
        public static bool Percent(this Random rng, int p)
        {
            return rng.Next(100) < p;
        }
        public static Q GetRandomElement<Q>(this Random rng, List<Q> list)
        {
            return list[rng.Next(list.Count)];
        }
        public static Q RemoveRandomElement<Q>(this Random rng, List<Q> list)
        {
            int i = rng.Next(list.Count);
            Q ret = list[i];
            list.RemoveAt(i);
            return ret;
        }
        public static Vector2 GetPointNear(this Random rng, Vector2 anchor, float direction, float arc, float minRadii, float maxRadii)
        {
            float dist = (float)(rng.NextDouble(maxRadii - minRadii));
            float angle = direction + (float)(rng.NextDouble((2 * arc) + 1) - arc);
            while (angle < 0) angle += 360;
            while (angle > 360) angle -= 360;

            return anchor + (minRadii + dist) * new Vector2((float)Math.Sin(MathHelper.DegToRad(angle)), (float)Math.Cos(MathHelper.DegToRad(angle)));

        }

        #region NextDouble
        public static double NextDouble(this Random rng, double max)
        {
           return NextDouble(rng, 0, max);
        }

        public static double NextDouble(this Random rng, double min, double max)
        {
            return min + rng.NextDouble() * (max - min);
        }
        #endregion
    }
}
