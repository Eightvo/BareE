using System;
using System.Collections.Generic;
using System.Numerics;

namespace BareE.Transvoxel
{
    public class TransvoxelTriangulation<D>
        where D : struct, IPointData
    {
        public List<Vector3> vertexPoints = new List<Vector3>();
        public List<uint> vertexIndexes = new List<uint>();
        private cellConstructionData[,,] tempData;

        private byte validDirections = 0;

        //False Means create vertex.
        //So Point 7 <Maximal point> Indicated with 8th bit always returns false to IsDirectionValid.
        //Otherwise return false on minimal planes while constructing cells.
        private bool IsDirectionValid(byte dir)
        {
            return (dir & validDirections) == dir;
        }

        #region DebugVisualizationHelpers

        private bool IsCornerActive(int cornerIndex, byte casecode)
        {
            var mdp = (int)Math.Pow(2, cornerIndex);
            return (casecode & (mdp)) == (mdp);
        }

        private bool IsEdgeActive(int edgeLabel, byte casecode)
        {
            var eqClass = Trannsvoxel.regularCellClass[casecode];
            var vrtxData = Trannsvoxel.regularVertexData[casecode];
            foreach (var v in vrtxData)
            {
                if (v.UpperByte == edgeLabel) return true;
            }
            return false;
        }

        private void DisplayPoints(int col, int row, byte ccode)
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

        #endregion DebugVisualizationHelpers

        private bool hasArea(Vector3 pt1, Vector3 pt2, Vector3 pt3)
        {
            return true;
        }

        private void TriangulateCell(CloudView<D> view, int x, int y, int z, int cLvl, int pLvl)
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
            foreach (var ed in vrtxData)
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
                }
                vertexIndexes.Add((uint)uo);

                uo = useOrder[pt2];
                if (uo == -1)
                {
                    uo = vertexPoints.Count;
                    vertexPoints.Add(generatedVertex[pt2]);
                }
                vertexIndexes.Add((uint)uo);

                uo = useOrder[pt3];
                if (uo == -1)
                {
                    uo = vertexPoints.Count;
                    vertexPoints.Add(generatedVertex[pt3]);
                }
                vertexIndexes.Add((uint)uo);
            }
            //  var ct = Console.CursorTop;
            // DisplayPoints(0, ct, caseCode);
        }

        public void Triangulate(CloudView<D> view)
        {
            var minCell = -view.BlockSize + 1;
            var maxCell = (view.BlockSize * 2) - 2;
            Triangulate(view, new Vector3(minCell), new Vector3(maxCell));
        }

        public void Triangulate(CloudView<D> view, Vector3 minCell, Vector3 maxCell)
        {
            vertexIndexes.Clear();
            vertexPoints.Clear();
            tempData = new cellConstructionData[2, view.BlockSize, view.BlockSize];
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
                        validDirections = (byte)(validDirections | 0b0001);
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