using System.Numerics;
using System.Runtime.InteropServices;

namespace BareE.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ambientLightData
    {
        private Vector4 r0;
        private Vector4 r1;
        private Vector4 r2;
        private Vector4 r3;

        public Vector3 alc
        {
            get { return new Vector3(r0.X, r0.Y, r0.Z); }
            set { r0 = new Vector4(value, r0.W); }
        }

        public float ali
        {
            get { return r0.W; }
            set { r0.W = value; }
        }

        public Vector3 dlp
        {
            get { return new Vector3(r1.X, r1.Y, r1.Z); }
            set
            {
                r1.X = value.X;
                r1.Y = value.Y;
                r1.Z = value.Z;
            }
        }

        public Vector3 slp
        {
            get
            {
                return new Vector3(r1.W, r2.X, r2.Y);
            }
            set
            {
                r1.W = value.X;
                r2.X = value.Y;
                r2.Y = value.Z;
            }
        }

        public Vector3 slc
        {
            get
            {
                return new Vector3(r2.Z, r2.W, r3.X);
            }
            set
            {
                r2.Z = value.X;
                r2.W = value.Y;
                r3.X = value.Z;
            }
        }

        public Vector3 slt
        {
            get
            {
                return new Vector3(r3.Y, r3.Z, r3.W);
            }
            set
            {
                r3.Y = value.X;
                r3.Z = value.Y;
                r3.W = value.Z;
            }
        }

        public static uint Size { get { return 64; } }
    }
}