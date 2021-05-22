using System.Numerics;

namespace BareE.Transvoxel
{
    public abstract class PointProvider<D>
        where D : struct, IPointData
    {
        public abstract D GetPoint(int Samplex, int Sampley, int Samplez);

        public D GetPoint(Vector3 loc)
        {
            return GetPoint((int)loc.X, (int)loc.Y, (int)loc.Z);
        }

        /// <summary>
        /// A positive value indicates the sample is above the surface in empty space and not within an object.
        /// A Negative value indicates the sample is below the isosurface.
        /// According to the .pdf, the value 0 is not special and can be treated as either in empty space or in the surface.
        /// TransvoxelDODN implements the algrorithm such that 0 indicates the sample is in empty space.
        /// </summary>
        /// <param name="BlockX"></param>
        /// <param name="BlockY"></param>
        /// <param name="BlockZ"></param>
        /// <param name="Samplex"></param>
        /// <param name="Sampley"></param>
        /// <param name="Samplez"></param>
        /// <returns></returns>
        public abstract float GetSample(int Samplex, int Sampley, int Samplez);

        public abstract bool HasSample(int Samplex, int Sampley, int Samplez);

        public float GetSample(Vector3 loc)
        {
            return GetSample((int)loc.X, (int)loc.Y, (int)loc.Z);
        }

        public bool HasSample(Vector3 loc)
        {
            return HasSample((int)loc.X, (int)loc.Y, (int)loc.Z);
        }
    }
}