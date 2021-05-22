using System;

namespace BareE.Transvoxel
{
    public class FunctionDefinedPointProvider<D> : PointProvider<D>
        where D : struct, IPointData
    {
        private Func<int, int, int, D> PointFunc;

        public FunctionDefinedPointProvider(Func<int, int, int, D> f)
        {
            PointFunc = f;
        }

        public override float GetSample(int Samplex, int Sampley, int Samplez)
        {
            return GetPoint(Samplex, Sampley, Samplez).SampleValue;
        }

        public override D GetPoint(int Samplex, int Sampley, int Samplez)
        {
            return PointFunc(Samplex, Sampley, Samplez);
        }

        public override bool HasSample(int Samplex, int Sampley, int Samplez)
        {
            return true;
        }
    }
}