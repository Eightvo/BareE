using System.Collections.Generic;
using System.Numerics;

namespace BareE.Transvoxel
{
    public class CustomPointProvider<D> : PointProvider<D>
        where D : struct, IPointData
    {
        private Dictionary<float, Dictionary<float, Dictionary<float, D>>> _pointCache;

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
}