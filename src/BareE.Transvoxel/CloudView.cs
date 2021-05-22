using System;
using System.Collections.Generic;
using System.Numerics;

namespace BareE.Transvoxel
{
    /// <summary>
    /// Contains Sample Data for a Cube of volume withing a point cloud.
    /// </summary>
    /// <typeparam name="D"></typeparam>
    public class CloudView<D>
        where D : struct, IPointData
    {
        public Vector3 AnchorBlock;
        public Vector3 Anchor { get; set; }
        public int BlockSize { get { return Data.BlockSize; } }

        private PointCloud<D> Data;

        private int GetRelativeBlockIndex(int x, int y, int z)
        {
            return (x + 1) + (3 * (y + 1)) + (9 * (z + 1));
        }

        private static List<Vector3> RelativeBlocks = new List<Vector3>()
        {
            new Vector3(-1,-1,-1),
            new Vector3( 0,-1,-1),
            new Vector3( 1,-1,-1),

            new Vector3(-1, 0,-1),
            new Vector3( 0, 0,-1),
            new Vector3( 1, 0,-1),

            new Vector3(-1, 1,-1),
            new Vector3( 0, 1,-1),
            new Vector3( 1, 1,-1),

            new Vector3(-1,-1, 0),
            new Vector3( 0,-1, 0),
            new Vector3( 1,-1, 0),

            new Vector3(-1, 0, 0),
            new Vector3( 0, 0, 0),
            new Vector3( 1, 0, 0),

            new Vector3(-1, 1, 0),
            new Vector3( 0, 1, 0),
            new Vector3( 1, 1, 0),

            new Vector3(-1,-1, 1),
            new Vector3( 0,-1, 1),
            new Vector3( 1,-1, 1),

            new Vector3(-1, 0, 1),
            new Vector3( 0, 0, 1),
            new Vector3( 1, 0, 1),

            new Vector3(-1, 1, 1),
            new Vector3( 0, 1, 1),
            new Vector3( 1, 1, 1),
        };

        //Per Fig 3.1
        public static List<Vector3> Corners = new List<Vector3>()
        {
            new Vector3(0,0,0),
            new Vector3(1,0,0),
            new Vector3(0,1,0),
            new Vector3(1,1,0),
            new Vector3(0,0,1),
            new Vector3(1,0,1),
            new Vector3(0,1,1),
            new Vector3(1,1,1)
        };

        private Vector4 calculateCacheIndex(int relPtX, int relPtY, int relPtZ)
        {
            int rBx = relPtX < 0 ? -1 : relPtX / Data.BlockSize;
            int rBy = relPtY < 0 ? -1 : relPtY / Data.BlockSize;
            int rBz = relPtZ < 0 ? -1 : relPtZ / Data.BlockSize;
            var relCord = Data.GetSampleCordinate(new Vector3(relPtX, relPtY, relPtZ));
            return new Vector4(
                GetRelativeBlockIndex(rBx, rBy, rBz),
                relCord.X,
                relCord.Y,
                relCord.Z
                );
        }

        private D[][,,] NearDataCache = new D[27][,,];

        /// <summary>
        /// Returns a Cube of Sample Data for the Block of data
        /// at x,y,z relative to the block of data the Anchor is located in.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public D[,,] this[int RelBlockX, int RelBlockY, int RelBlockZ]
        {
            get
            {
                var indx = GetRelativeBlockIndex(RelBlockX, RelBlockY, RelBlockZ);
                if (indx < 0 || indx >= 27) throw new IndexOutOfRangeException();
                return NearDataCache[indx];
            }
        }

        //Anchor is is sample space.
        //There are BlockSize samples per Block
        public void Initialize(PointCloud<D> data, Vector3 anchor)
        {
            Data = data;
            Anchor = anchor;
            RefillCache(anchor);
        }

        private void RefillCache(Vector3 anchor)
        {
            Anchor = anchor;
            AnchorBlock = Data.GetBlockCordinate(anchor);
            foreach (var relBlock in RelativeBlocks)
            {
                var indx = GetRelativeBlockIndex((int)relBlock.X, (int)relBlock.Y, (int)relBlock.Z);
                var blk = Data.GetPointBlock(
                    (int)(AnchorBlock.X + relBlock.X),
                    (int)(AnchorBlock.Y + relBlock.Y),
                    (int)(AnchorBlock.Z + relBlock.Z));
                NearDataCache[indx] = blk;
            }
        }

        public void UpdateAnchor(Vector3 newAnchor)
        {
            var newAnchorBlock = Data.GetBlockCordinate(newAnchor);
            if (newAnchorBlock == AnchorBlock) return;
            var deltaVec = (newAnchorBlock - AnchorBlock);
            if (deltaVec.LengthSquared() > (Data.BlockSize * Data.BlockSize))
            {
                RefillCache(newAnchor);
                return;
            }
            deltaVec = deltaVec / new Vector3(Math.Max(1, Math.Abs(deltaVec.X)), Math.Max(1, Math.Abs(deltaVec.Y)), Math.Max(1, Math.Abs(deltaVec.Z)));
            for (int i = 0; i < 27; i++)
            {
                var shiftedRelative = RelativeBlocks[i] + deltaVec;
                int shiftedIndex = -1;
                if (Math.Abs(shiftedRelative.X) <= 1 && Math.Abs(shiftedRelative.Y) <= 1 && Math.Abs(shiftedRelative.Z) <= 1)
                    shiftedIndex = GetRelativeBlockIndex((int)shiftedRelative.X, (int)shiftedRelative.Y, (int)shiftedRelative.Z);
                if (shiftedIndex >= 0 && shiftedIndex < 27)
                {
                    NearDataCache[i] = NearDataCache[shiftedIndex];
                }
                else
                {
                    var requestedWorldBlock = newAnchorBlock + RelativeBlocks[i];
                    NearDataCache[i] = Data.GetPointBlock((int)requestedWorldBlock.X, (int)requestedWorldBlock.Y, (int)requestedWorldBlock.Z);
                }
            }
            AnchorBlock = newAnchorBlock;
            Anchor = new Vector3(Anchor.X + (Data.BlockSize * deltaVec.X),
                                 Anchor.Y + (Data.BlockSize * deltaVec.Y),
                                 Anchor.Z + (Data.BlockSize * deltaVec.Z));
        }

        /// <summary>
        /// Cell 0,0,0 is the Anchor Cell.
        /// Cell Ranges -(BlockSize+2) to (BlockSize*2)-2 in each dimention.
        /// (I.E: For BlockSize 16. Cells range from -14 to 34)
        /// The camera is assumed to be in the anchor block so there should always be a minimum of BlockSize-2 cells between the anchor and
        /// the edge of data.
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="CellZ"></param>
        /// <returns></returns>
        public byte CalculateCaseCode(int CellX, int CellY, int CellZ, float surfaceValue = 0.0f)
        {
            var cellRootPt = Anchor + new Vector3(CellX, CellY, CellZ);
            byte f = 1;
            byte m = 0;
            int cI = 0;
            foreach (var c in Corners)
            {
                var cornerPt = cellRootPt + c;
                var cacheKey = calculateCacheIndex((int)cornerPt.X, (int)cornerPt.Y, (int)cornerPt.Z);
                D[,,] Blk = NearDataCache[(int)cacheKey.X];
                D PT = Blk[(int)cacheKey.Y, (int)cacheKey.Z, (int)cacheKey.W];
                //Console.WriteLine($"[C{cI}]{cornerPt} = {PT.SampleValue}");
                cI++;

                if (PT.SampleValue < surfaceValue)
                    m = (byte)(m | f);
                f = (byte)(f << 1);
            }
            // if (m == (0xFF)) return 0;
            return m;
        }

        public Vector3 CalculateVertex(int Cellx, int Celly, int Cellz, VDat v)
        {
            //  Console.WriteLine($"\t\tCalculating Vertex on Edge {v.UpperByte:X} C{v.Corner1} to C{v.Corner2} for cell {Cellx} {Celly} {Cellz}");
            //Translate cellRootPt to C0 of cell being calculated
            Vector3 cellRootPt = Anchor + new Vector3(Cellx, Celly, Cellz);
            //    Console.WriteLine($"\t\t\tRoot Point: {cellRootPt}");
            //cellRootPt += v.Direction;
            //Get the Corner Directions.
            Vector3 c1 = Corners[v.Corner2] + cellRootPt;
            Vector3 c2 = Corners[v.Corner1] + cellRootPt;
            float sampleValue1 = GetSampleValue(c1);
            float sampleValue2 = GetSampleValue(c2);
            if (sampleValue1 == 0) return c1;
            if (sampleValue2 == 0) return c2;
            //    Console.WriteLine($"\t\t\tEP0: {c1} = {sampleValue1}");
            //    Console.WriteLine($"\t\t\tEP1: {c2} = {sampleValue2}");
            var T = sampleValue1 / (sampleValue1 - sampleValue2);
            //    Console.WriteLine($"\t\t\t T={T}");
            var ret = (T * c1) + ((1 - T) * c2);
            return ret;
        }

        /// <summary>
        /// Return a sample value where the input is a global offset from the anchor point.
        /// </summary>
        /// <param name="gX"></param>
        /// <param name="gY"></param>
        /// <param name="gZ"></param>
        /// <returns></returns>
        public float GetSampleValue(int gX, int gY, int gZ)
        {
            var cacheKey = calculateCacheIndex(gX, gY, gZ);
            D[,,] Blk = NearDataCache[(int)cacheKey.X];
            D PT = Blk[(int)cacheKey.Y, (int)cacheKey.Z, (int)cacheKey.W];
            return PT.SampleValue;
        }

        public float GetSampleValue(Vector3 gV)
        {
            return GetSampleValue((int)gV.X, (int)gV.Y, (int)gV.Z);
        }

        public IEnumerable<Tuple<Vector3, Vector3, Vector3>> TriangulateSurface()
        {
            foreach (var r in TriangulateSurface(new Vector3(-Data.BlockSize + 2, -Data.BlockSize + 2, -Data.BlockSize + 2),
                                                 new Vector3((Data.BlockSize * 2) - 2, (Data.BlockSize * 2) - 2, (Data.BlockSize * 2) - 2)))
            {
                yield return r;
            }
        }

        public IEnumerable<Tuple<Vector3, Vector3, Vector3>> TriangulateSurface(Vector3 minCell, Vector3 maxCell)
        {
            for (int z = (int)minCell.Z; z < maxCell.Z; z++)
            {
                for (int y = (int)minCell.Y; y < maxCell.Y; y++)
                {
                    for (int x = (int)minCell.X; x < maxCell.X; x++)
                    {
                        foreach (var r in TriangulateCell(x, y, z))
                            yield return r;
                    }
                }
            }
        }

        public IEnumerable<Tuple<Vector3, Vector3, Vector3>> TriangulateCell(int x, int y, int z)
        {
            yield return null;
        }

        public void ForceRefreshAll()
        {
            RefillCache(Anchor);
        }
    }
}