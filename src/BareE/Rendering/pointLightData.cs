using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BareE.Rendering
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct pointLightData
    {
        [FieldOffset(0)]
        public fixed float RAW[4 * 32];

        public void AddPointLight(Vector3 pos, Vector3 clr, Vector3 att)
        {
            var currLightCount = RAW[0];
            if (currLightCount >= 10) throw new Exception("Enough with the lights");
            int sI = ((int)currLightCount * 9) + 1;
            RAW[sI + 0] = pos.X;
            RAW[sI + 1] = pos.Y;
            RAW[sI + 2] = pos.Z;

            RAW[sI + 3] = pos.X;
            RAW[sI + 4] = pos.Y;
            RAW[sI + 5] = pos.Z;

            RAW[sI + 6] = pos.X;
            RAW[sI + 7] = pos.Y;
            RAW[sI + 8] = pos.Z;

            RAW[0] += 1;
        }

        public static uint Size { get { return 512; } }
    }
}