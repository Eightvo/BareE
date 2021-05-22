using System.Numerics;

namespace BareE.Transvoxel
{
    public class SpherePointProvider<D> : PointProvider<D>
        where D : struct, IPointData
    {
        private Vector3 worldPoint { get; set; }
        private float radius { get; set; }
        private D ShereData;
        private int BlockSize;

        public SpherePointProvider(Vector3 cp, float r, int blksz, D data)
        {
            worldPoint = cp;
            radius = r;
            BlockSize = blksz;
            ShereData = data;
        }

        public override float GetSample(int Samplex, int Sampley, int Samplez)
        {
            var delta = worldPoint - new Vector3(Samplex, Sampley, Samplez);
            var r = (delta.LengthSquared() - (radius * radius));
            //if (r < 0) return r;
            return r;
        }

        public override bool HasSample(int Samplex, int Sampley, int Samplez)
        {
            var delta = worldPoint - new Vector3(Samplex, Sampley, Samplez);
            var r = (delta.LengthSquared() - (radius * radius));
            return r < 0;
        }

        public override D GetPoint(int Samplex, int Sampley, int Samplez)
        {
            var tx = Samplex;
            var ty = Sampley;
            var tz = Samplez;
            var delta = worldPoint - new Vector3(tx, ty, tz);
            if (delta.LengthSquared() < radius * radius)
                return ShereData;
            return default(D);
        }
    }
}