using System;
using System.Numerics;

namespace BareE
{
    public class MathHelper
    {
        public static float DegToRad(float deg)
        {
            return (float)(deg * (Math.PI / 180.0f));
        }

        public static float RadToDeg(float rad)
        {
            return (float)(rad * (180.0f / Math.PI));
        }

        public static Vector2 DegToVector(float deg)
        {
            return RadToVector(DegToRad(deg));
        }

        public static Vector2 RadToVector(float rad)
        {
            return Vector2.Normalize(new Vector2(-(float)Math.Cos(rad), (float)Math.Sin(rad)));
        }
        public static uint NxtPowerOfTwo(int i)
        {
            uint p = 1;
            while (p <= i)
                p = p * 2;
            return p;
            //return (uint)(i + (16 - (i % 16)));
        }
        public static uint NxtMultipleOfSixteen(int i)
        {
            return (uint)(i + (16 - (i % 16)));
        }
        public static uint NxtMultipleOfSixteen(uint i)
        {
            return (uint)(i + (16 - (i % 16)));
        }
        public static Vector2 ToVector2(float angle)
        {
            var X = (float)Math.Sin(angle);
            var Y = (float)Math.Cos(angle);
            return new Vector2(X, Y);
        }

        public static float ToFacingAngle(Vector2 f)
        {
            if (f.Y > 0 && f.Y > Math.Abs(f.X)) return MathHelper.DegToRad(0);
            if (f.Y < 0 && Math.Abs(f.Y) > Math.Abs(f.X)) return MathHelper.DegToRad(180);
            if (f.X < 0) return MathHelper.DegToRad(270);
            if (f.X > 0) return MathHelper.DegToRad(90);
            return MathHelper.DegToRad(180);
        }
        public static Vector2 AngleToUnitVector(float rotation)
        {
            return Vector2.Normalize(new Vector2((float)Math.Sin(rotation), (float)Math.Cos(rotation)));
        }

        public static Vector2 ToVelocityVector(float thrust, float angle)
        {
            var v = AngleToUnitVector(angle) * thrust;
            return new Vector2((float)Math.Round(v.X, 5), (float)Math.Round(v.Y, 5));
        }

    }
}