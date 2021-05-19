using System.Numerics;
using System.Runtime.InteropServices;

namespace BareE.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CommonData
    {
        public int time;
        public int flags;
        public int pad1;
        public int pad2;
        public Vector2 u_resolution;
        public Vector2 u_mouse;
        public Vector3 u_campos;
        float seed;
        public static uint Size { get { return 48; } }
    }
}
