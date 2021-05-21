using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BareE.Transvoxel
{

    public class CustomPointProvider<D>:PointProvider<D>
        where D:struct, IPointData
    {
        Dictionary<float, Dictionary<float, Dictionary<float, D>>> _pointCache;
        public CustomPointProvider()
        {
            _pointCache = new Dictionary<float, Dictionary<float, Dictionary<float, D>>>();
        }
        public void RemovePoint(Vector3 location)
        {
            if (!_pointCache.ContainsKey(location.X))
                return;
            if (!_pointCache[location.X].ContainsKey(location.Y))
                return;
            if (!_pointCache[location.X][location.Y].ContainsKey(location.Z))
                return;
            _pointCache[location.X][location.Y].Remove(location.Z);
            return;
        }
        public void AddPoint(Vector3 location, D PointData)
        {
            AddPoint((int)location.X, (int)location.Y, (int)location.Z, PointData);
        }
        public void AddPoint(int X, int Y, int Z, D PointData)
        {
            if (!_pointCache.ContainsKey(X))
                _pointCache.Add(X, new Dictionary<float, Dictionary<float, D>>());
            var x = _pointCache[X];
            if (!x.ContainsKey(Y))
                x.Add(Y, new Dictionary<float, D>());
            var y = x[Y];
            if (!y.ContainsKey(Z))
                y.Add(Z, PointData);
            else
                y[Z] = PointData;
        }

        public override D GetPoint(int Samplex, int Sampley, int Samplez)
        {

            if (_pointCache.ContainsKey(Samplex))
                if (_pointCache[Samplex].ContainsKey(Sampley))
                    if (_pointCache[Samplex][Sampley].ContainsKey(Samplez))
                        return _pointCache[Samplex][Sampley][Samplez];
            return default(D);
        }

        public override float GetSample(int Samplex, int Sampley, int Samplez)
        {
            return _pointCache[Samplex][Sampley][Samplez].SampleValue;
        }

        public override bool HasSample(int Samplex, int Sampley, int Samplez)
        {
            if (_pointCache.ContainsKey(Samplex))
                if (_pointCache[Samplex].ContainsKey(Sampley))
                    if (_pointCache[Samplex][Sampley].ContainsKey(Samplez))
                        return true;
            return false;
        }
    }

    public class SpherePointProvider<D>:PointProvider<D>
        where D : struct, IPointData
    {
        Vector3 worldPoint { get; set; }
        float radius { get; set; }
        D ShereData;
        int BlockSize;
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
            var r = (  delta.LengthSquared()- (radius * radius));
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
    public class FunctionDefinedPointProvider<D> : PointProvider<D>
        where D : struct, IPointData
    {
        Func<int, int, int, D> PointFunc;
        public FunctionDefinedPointProvider(Func<int, int, int, D> f)
        {
            PointFunc = f;
        }
        public override float GetSample(int Samplex, int Sampley, int Samplez)
        {
            return GetPoint(Samplex,Sampley,Samplez).SampleValue;
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
