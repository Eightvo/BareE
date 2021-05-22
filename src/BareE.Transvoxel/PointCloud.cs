using System;
using System.Collections.Generic;
using System.Numerics;

namespace BareE.Transvoxel
{
    public class PointCloud<D>
        where D : struct, IPointData
    {
        private List<PointProvider<D>> providers = new List<PointProvider<D>>();

        public void AddProvider(PointProvider<D> provider)
        {
            providers.Add(provider);
        }

        public int BlockSize = 16;

        //To create a Block of BlockSizexBlockSizexBlockSize Transvoxel (Cells BlockSize+2)x(Cells BlockSize+2)x(Cells BlockSize+2) volume sample points are required.
        //1 Extra layer around the minimum edge, 2 extra layers around the maximum edge.
        //
        //The center Cube will always be rendered 16x16x16 so we'll always store at least enough information to render that size.
        //The outter cubes can each be trimmed to be shorter on the edge which requires the extra layers of information.
        //This way we don't have to touch more then 27 blocks at a time.
        public D[,,] GetPointBlock(int BlockX, int BlockY, int BlockZ)
        {
            D[,,] ret = new D[BlockSize, BlockSize, BlockSize];
            for (int m = 0; m < BlockSize; m++)
            {
                for (int n = 0; n < BlockSize; n++)
                {
                    for (int l = 0; l < BlockSize; l++)
                    {
                        ret[m, n, l] = GetPoint(BlockX, BlockY, BlockZ, m, n, l);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Block <X,y,z> Sample<m,n,l>
        /// </summary>
        /// <param name="BlockX"></param>
        /// <param name="BlockY"></param>
        /// <param name="BlockZ"></param>
        /// <param name="Samplex"></param>
        /// <param name="Samplen"></param>
        /// <param name="Samplez"></param>
        /// <returns></returns>
        private D GetPoint(int BlockX, int BlockY, int BlockZ, int Samplex, int Sampley, int Samplez)
        {
            //We want the sample that is negative and closest to 0.
            D contributer = default(D);
            D farContrib = default(D);
            var sX = (BlockX * BlockSize) + Samplex;
            var sY = (BlockY * BlockSize) + Sampley;
            var sZ = (BlockZ * BlockSize) + Samplez;
            farContrib.CreateEmptySpaceSamplevalue();
            float nearestSampleValue = float.MinValue;
            float farthestSampleValue = float.MaxValue;
            bool near = false;
            foreach (PointProvider<D> provider in providers)
            {
                if (!provider.HasSample(sX, sY, sZ))
                    continue;
                var sample = provider.GetSample(sX, sY, sZ);
                if (sample >= 0)
                {
                    if (farthestSampleValue > sample)
                    {
                        farContrib = provider.GetPoint(sX, sY, sZ);
                        farthestSampleValue = sample;
                    }
                }
                else
                {
                    if (nearestSampleValue < sample)
                    {
                        contributer = provider.GetPoint(sX, sY, sZ);
                        nearestSampleValue = sample;
                        near = true;
                    }
                }
            }
            if (near)
                return contributer;
            else
                return farContrib;
        }

        public Vector3 GetBlockCordinate(Vector3 worldPoint)
        {
            return new Vector3((float)Math.Floor(worldPoint.X / BlockSize),
                               (float)Math.Floor(worldPoint.Y / BlockSize),
                               (float)Math.Floor(worldPoint.Z / BlockSize));
        }

        public Vector3 GetSampleCordinate(Vector3 worldPoint)
        {
            return new Vector3((float)Math.Truncate(worldPoint.X - (Math.Floor(worldPoint.X / BlockSize) * BlockSize)),
                               (float)Math.Truncate(worldPoint.Y - (Math.Floor(worldPoint.Y / BlockSize) * BlockSize)),
                               (float)Math.Truncate(worldPoint.Z - (Math.Floor(worldPoint.Z / BlockSize) * BlockSize)));
        }
    }
}