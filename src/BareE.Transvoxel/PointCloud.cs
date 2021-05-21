using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BareE.Transvoxel
{
    public interface IPointData
    {
        /// <summary>
        /// The sample Value indicates whether the sample is below or above the isosurface.
        /// According to the .pdf, the value 0 is not special and can be treated as either in empty space or in the surface.
        /// A positive value indicates the sample is above the surface in empty space and not within an object.
        /// A Negative value indicates the sample is below the isosurface.
        /// According to the .pdf, the value 0 is not special and can be treated as either in empty space or in the surface.
        /// TransvoxelDODN implements the algrorithm such that 0 indicates the sample is in empty space.
        /// </summary>
        float SampleValue { get; }
        float CreateEmptySpaceSamplevalue();
    }
    public abstract class PointProvider<D>
        where D : struct,IPointData
    {
        D pointData;
        public abstract D GetPoint(int Samplex, int Sampley, int Samplez);
        public D GetPoint(Vector3 loc) { return GetPoint((int)loc.X, (int)loc.Y, (int)loc.Z); }
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
        public float GetSample(Vector3 loc) { return GetSample((int)loc.X, (int)loc.Y, (int)loc.Z); }
        public bool HasSample(Vector3 loc) { return HasSample((int)loc.X, (int)loc.Y, (int)loc.Z); }
    }
    public class PointCloud<D>
        where D : struct, IPointData
    {
        List<PointProvider<D>> providers = new List<PointProvider<D>>();
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
            if (BlockX == BlockY && BlockY == BlockZ && BlockZ == 0)
            {
                int i = 1;
            }

            D[,,] ret = new D[BlockSize, BlockSize, BlockSize];
            for(int m=0;m< BlockSize; m++)
            {
                for(int n=0;n< BlockSize; n++)
                {
                    for(int l=0; l< BlockSize; l++)
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
        D GetPoint(int BlockX, int BlockY, int BlockZ, int Samplex, int Sampley, int Samplez)
        {
            //We want the sample that is negative and closest to 0.
            D contributer=default(D);
            D farContrib = default(D);
            var sX = BlockX * BlockSize + Samplex;
            var sY = BlockY * BlockSize + Sampley;
            var sZ = BlockZ * BlockSize + Samplez;
            farContrib.CreateEmptySpaceSamplevalue();
            float nearestSampleValue = float.MinValue;
            float farthestSampleValue = float.MaxValue;
            bool near = false;
            foreach(PointProvider<D> provider in providers)
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
            return new Vector3((float)Math.Truncate(worldPoint.X - Math.Floor(worldPoint.X / BlockSize) * BlockSize),
                               (float)Math.Truncate(worldPoint.Y - Math.Floor(worldPoint.Y / BlockSize) * BlockSize),
                               (float)Math.Truncate(worldPoint.Z - Math.Floor(worldPoint.Z / BlockSize) * BlockSize));
        }

    }
    public class CloudView<D>
        where D : struct, IPointData
    {
        public Vector3 AnchorBlock;
        public Vector3 Anchor { get; set; }
        public int BlockSize { get { return Data.BlockSize; } }

        PointCloud<D> Data;

        int GetRelativeBlockIndex(int x, int y, int z)
        {
            return (x + 1) + 3 * (y + 1) + 9 * (z + 1);
        }
        static List<Vector3> RelativeBlocks = new List<Vector3>()
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
        public  static List<Vector3> Corners = new List<Vector3>()
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

        Vector4 calculateCacheIndex(int relPtX, int relPtY, int relPtZ)
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

        D[][,,] NearDataCache = new D[27][,,];

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
        void RefillCache(Vector3 anchor)
        {
            Anchor = anchor;
            AnchorBlock = Data.GetBlockCordinate(anchor);
            int c = 0;
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
                } else
                {
                    var requestedWorldBlock = newAnchorBlock + RelativeBlocks[i];
                    NearDataCache[i] = Data.GetPointBlock((int)requestedWorldBlock.X, (int)requestedWorldBlock.Y, (int)requestedWorldBlock.Z);
                }
            }
            AnchorBlock = newAnchorBlock;
            Anchor = new Vector3(Anchor.X + Data.BlockSize * deltaVec.X,
                               Anchor.Y + Data.BlockSize * deltaVec.Y,
                               Anchor.Z + Data.BlockSize * deltaVec.Z);

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
            Vector3 c2= Corners[v.Corner1] + cellRootPt;
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
                                                 new Vector3((Data.BlockSize * 2) - 2, (Data.BlockSize * 2) - 2,(Data.BlockSize * 2) - 2)))
            {
                yield return r;
            }
        }

        public IEnumerable<Tuple<Vector3,Vector3,Vector3>> TriangulateSurface(Vector3 minCell, Vector3 maxCell)
        {
            for(int z=(int)minCell.Z;z<maxCell.Z;z++)
            {
                for (int y=(int)minCell.Y;y<maxCell.Y;y++)
                {
                    for(int x=(int)minCell.X;x<maxCell.X;x++)
                    {
                        foreach(var r in TriangulateCell(x, y, z))
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
    struct cellConstructionData
    {
        uint[] createdVerticies;
        public void SaveVertex(int cellVertexIndex, uint globalVertexIndex)
        {
            (createdVerticies ?? new uint[4])[cellVertexIndex] = globalVertexIndex;
        }
        public uint retreiveVertex(int cellVertexIndex)
        {
            return (createdVerticies ?? new uint[4])[cellVertexIndex];
        }
    }
    public class TransvoxelTriangulation<D>
        where D: struct, IPointData
    {
        public List<Vector3> vertexPoints = new List<Vector3>();
        public List<uint> vertexIndexes = new List<uint>();
        cellConstructionData[,,] tempData;

        byte validDirections = 0;
        //False Means create vertex.
        //So Point 7 <Maximal point> Indicated with 8th bit always returns false to IsDirectionValid.
        //Otherwise return false on minimal planes while constructing cells.
        bool IsDirectionValid(byte dir)
        {
            return (dir & validDirections) == dir;
        }
        #region DebugVisualizationHelpers
        bool IsCornerActive(int cornerIndex, byte casecode)
        {
            var mdp = (int)Math.Pow(2, cornerIndex);
            return (casecode & (mdp)) == (mdp);
        }
        bool IsEdgeActive(int edgeLabel, byte casecode)
        {
            var eqClass = Trannsvoxel.regularCellClass[casecode];
            var vrtxData = Trannsvoxel.regularVertexData[casecode];
            foreach(var v in vrtxData)
            {
                if (v.UpperByte == edgeLabel) return true;
            }
            return false;
        }
        void DisplayPoints(int col, int row, byte ccode)
        {
            var eqClass = Trannsvoxel.regularCellClass[ccode];
            var eqClassData = Trannsvoxel.regularCellData[eqClass];
            var vrtxData = Trannsvoxel.regularVertexData[eqClass];
            //About to attempt to draw the following figure.
            var c = @"
   +--------+
  /|       /|
 / |      / |
+--------+  |
|  |     |  |
|  +-----|--+
| /      | /
|/       |/
+--------+
";

            ConsoleColor IFC = Console.ForegroundColor;
            ConsoleColor IBC = Console.BackgroundColor;

            ConsoleColor InternalSample = ConsoleColor.Green;
            ConsoleColor ExternalSample = ConsoleColor.Red;
            ConsoleColor AE = ConsoleColor.Green;
            ConsoleColor IE = ConsoleColor.Red;
            DrawCube(col, row, ccode, InternalSample, ExternalSample, AE, IE);
            Console.ForegroundColor = IFC;
            Console.BackgroundColor = IBC;
        }

        private void DrawCube(int col, int row, byte ccode, ConsoleColor InternalSample, ConsoleColor ExternalSample, ConsoleColor AE, ConsoleColor IE)
        {
            Console.SetCursorPosition(col, row);
            colorize("   +", InternalSample, ExternalSample, IsCornerActive(6, ccode));
            colorize("--------", AE, IE, IsEdgeActive(0x82, ccode));
            colorize("+", InternalSample, ExternalSample, IsCornerActive(7, ccode));
            Console.SetCursorPosition(col, row + 1);
            colorize("  /", AE, IE, IsEdgeActive(0x11, ccode));
            colorize("|       ", AE, IE, IsEdgeActive(0x13, ccode));
            colorize("/", AE, IE, IsEdgeActive(0x81, ccode));
            colorize("|", AE, IE, IsEdgeActive(0x83, ccode));
            Console.SetCursorPosition(col, row + 2);
            colorize(" / ", AE, IE, IsEdgeActive(0x11, ccode));
            colorize("|      ", AE, IE, IsEdgeActive(0x13, ccode));
            colorize("/ ", AE, IE, IsEdgeActive(0x81, ccode));
            colorize("|", AE, IE, IsEdgeActive(0x83, ccode));
            Console.SetCursorPosition(col, row + 3);
            colorize("+", InternalSample, ExternalSample, IsCornerActive(4, ccode));
            colorize("--------", AE, IE, IsEdgeActive(0x22, ccode));
            colorize("+  ", InternalSample, ExternalSample, IsCornerActive(5, ccode));
            colorize("|", AE, IE, IsEdgeActive(0x83, ccode));
            Console.SetCursorPosition(col, row + 4);
            colorize("|  ", AE, IE, IsEdgeActive(0x33, ccode));
            colorize("|     ", AE, IE, IsEdgeActive(0x13, ccode));
            colorize("|  ", AE, IE, IsEdgeActive(0x23, ccode));
            colorize("|", AE, IE, IsEdgeActive(0x83, ccode));
            Console.SetCursorPosition(col, row + 5);
            colorize("|  ", AE, IE, IsEdgeActive(0x33, ccode));
            colorize("+", InternalSample, ExternalSample, IsCornerActive(2, ccode));
            colorize("-----", AE, IE, IsEdgeActive(0x42, ccode));
            colorize("|", AE, IE, IsEdgeActive(0x23, ccode));
            colorize("--", AE, IE, IsEdgeActive(0x42, ccode));
            colorize("+", InternalSample, ExternalSample, IsCornerActive(3, ccode));
            Console.SetCursorPosition(col, row + 6);
            colorize("| ", AE, IE, IsEdgeActive(0x33, ccode));
            colorize("/      ", AE, IE, IsEdgeActive(0x51, ccode));
            colorize("| ", AE, IE, IsEdgeActive(0x23, ccode));
            colorize("/", AE, IE, IsEdgeActive(0x41, ccode));
            Console.SetCursorPosition(col, row + 7);
            colorize("|", AE, IE, IsEdgeActive(0x33, ccode));
            colorize("/       ", AE, IE, IsEdgeActive(0x51, ccode));
            colorize("|", AE, IE, IsEdgeActive(0x23, ccode));
            colorize("/", AE, IE, IsEdgeActive(0x41, ccode));
            Console.SetCursorPosition(col, row + 8);
            colorize("+", InternalSample, ExternalSample, IsCornerActive(0, ccode));
            colorize("--------", AE, IE, IsEdgeActive(0x62, ccode));
            colorize("+", InternalSample, ExternalSample, IsCornerActive(1, ccode));
            Console.SetCursorPosition(col, row + 9);
        }

        private void colorize(string v1, ConsoleColor trueColor, ConsoleColor falseColor, bool det)
        {
            Console.ForegroundColor = det ? trueColor : falseColor;
            Console.Write(v1);
        }
        #endregion


        bool hasArea(Vector3 pt1, Vector3 pt2, Vector3 pt3)
        {

            return true;
        }
        void TriangulateCell(CloudView<D> view, int x, int y, int z, int cLvl, int pLvl)
        {
            //Console.Clear();
            var caseCode = view.CalculateCaseCode(x, y, z, 0);
            var eqClass = Trannsvoxel.regularCellClass[caseCode];
            var eqClassData = Trannsvoxel.regularCellData[eqClass];
            var vrtxData = Trannsvoxel.regularVertexData[caseCode];
            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine($"Anchor: {x}, {y}, {z}. Case Code: {caseCode} => EC:{eqClass} Tris:{eqClassData.TriangleCount} Verts:{eqClassData.VertexCount}");
            List<Vector3> generatedVertex = new List<Vector3>();
            List<int> useOrder = new List<int>();
            foreach(var ed in vrtxData)
            {
                //Do the calculating of the vertex if it is needed to be created. Otherwise try to reuse a vertex.
                var vPos = view.CalculateVertex(x, y, z, ed);
                generatedVertex.Add(vPos);
                useOrder.Add(-1);
              //  Console.WriteLine($"\t0x{ed.VertData:X} CurrentCell? {ed.CurrentCellFlag} {ed.Direction} Stored: {ed.StoreageIndex} Vertex Position: { vPos}");
            }

           // Console.WriteLine();
            for (int i = 0; i < eqClassData.TriangleCount; i++)
            {
                var pt1 = eqClassData.TriVertList[(i * 3) + 0];
                var pt2 = eqClassData.TriVertList[(i * 3) + 1];
                var pt3 = eqClassData.TriVertList[(i * 3) + 2];
            //    Console.Write($"{pt1} {vrtxData[pt1]:X} {generatedVertex[pt1]}| {pt2} {vrtxData[pt2]:X} {generatedVertex[pt2]}|{pt3} {vrtxData[pt3]:X} {generatedVertex[pt3]}");
                var noArea = !hasArea(generatedVertex[pt1], generatedVertex[pt2], generatedVertex[pt3]);
            //    Console.WriteLine($"No Area: {noArea}");
                if (noArea) continue;
                var uo = useOrder[pt1];
                if (uo == -1)
                {
                    uo = vertexPoints.Count;
                    vertexPoints.Add(generatedVertex[pt1]);
                };
                vertexIndexes.Add((uint)uo);

                uo = useOrder[pt2];
                if (uo == -1)
                {
                    uo = vertexPoints.Count;
                    vertexPoints.Add(generatedVertex[pt2]);
                };
                vertexIndexes.Add((uint)uo);

                uo = useOrder[pt3];
                if (uo == -1)
                {
                    uo = vertexPoints.Count;
                    vertexPoints.Add(generatedVertex[pt3]);
                };
                vertexIndexes.Add((uint)uo);


            }
          //  var ct = Console.CursorTop;
           // DisplayPoints(0, ct, caseCode);
        }
        public void Triangulate(CloudView<D> view)
        {
            var minCell = -view.BlockSize + 1;
            var maxCell = (view.BlockSize * 2) - 2;
            Triangulate(view,new Vector3(minCell), new Vector3(maxCell));
            
        }
        public void Triangulate(CloudView<D> view, Vector3 minCell, Vector3 maxCell)
        {
            vertexIndexes.Clear();
            vertexPoints.Clear();
            tempData = new cellConstructionData[2,view.BlockSize, view.BlockSize];
            int currLvl = 1;
            int prevLvl = 0;
            validDirections = 0;
            for (int z = (int)minCell.Z; z < maxCell.Z; z++)
            {
                validDirections = (byte)(validDirections & 0b0101);
                for (int y = (int)minCell.Y; y < maxCell.Y; y++)
                {
                    validDirections = (byte)(validDirections & 0b0110);
                    for (int x = (int)minCell.X; x < maxCell.X; x++)
                    {
                        TriangulateCell(view, x, y, z, currLvl, prevLvl);
                        validDirections =(byte)(validDirections | 0b0001);
                    }
                    validDirections = (byte)(validDirections | 0b0010);
                }
                validDirections = (byte)(validDirections | (0b0100));
                currLvl = 1 - currLvl;
                prevLvl = 1 - prevLvl;
            }
        }
    }

}
