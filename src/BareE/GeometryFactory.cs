using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BareE
{
    public struct Tri
    {
        private Vector3 pt1;

        public Vector3 Pt1
        {
            get
            {
                if (InvertWinding) return pt3;
                return pt1;
            }
            set
            {
                if (InvertWinding) pt3 = value;
                else pt1 = value;
            }
        }

        private Vector3 pt2;
        public Vector3 Pt2 { get { return pt2; } set { pt2 = value; } }

        private Vector3 pt3;

        public Vector3 Pt3
        {
            get
            {
                if (InvertWinding) return pt1;
                return pt3;
            }
            set
            {
                if (InvertWinding) pt1 = value;
                else pt3 = value;
            }
        }

        public Tri(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            pt1 = p1;
            pt2 = p2;
            pt3 = p3;
            InvertWinding = false;
        }

        public bool InvertWinding { get; set; }

        public IEnumerable<Vector3> Verticies
        {
            get
            {
                if (!InvertWinding)
                {
                    yield return pt1;
                    yield return pt2;
                    yield return pt3;
                }
                else
                {
                    yield return pt3;
                    yield return pt2;
                    yield return pt1;
                }
            }
        }

        public Vector3 Normal
        {
            get
            {
                return Vector3.Cross((pt2 - pt1), (pt3 - pt1));
            }
        }
    }

    public static class GeometryFactory
    {
        public static IEnumerable<Tri> CubeTriangulation
        {
            get
            {
                Vector3[] temp = new Vector3[] { Vector3.Zero, Vector3.Zero, Vector3.Zero };
                int i = 0;
                foreach (var v in CubeVerts())
                {
                    temp[i] = v;
                    if (i == 2)
                        yield return new Tri(temp[0], temp[1], temp[2]);
                    i = (i + 1) % 3;
                }
            }
        }

        /// <summary>
        /// Not Triangluate... Neither finding position by three points, nor finding the three points to generate a triange.
        /// Ony converting a list of pts into sets of three.
        /// </summary>
        public static IEnumerable<Tri> Triangulize(IEnumerable<Vector3> verts)
        {
            Vector3[] temp = new Vector3[] { Vector3.Zero, Vector3.Zero, Vector3.Zero };
            int i = 0;
            foreach (var v in CubeVerts())
            {
                temp[i] = v;
                if (i == 2)
                    yield return new Tri(temp[0], temp[1], temp[2]);
                i = (i + 1) % 3;
            }
        }

        public static IEnumerable<Vector3> QuadVerts()
        {
            //Close
            yield return (new Vector3(-0.5f, -0.5f, -1.0f));
            yield return (new Vector3(0.5f, 0.5f, -1f));
            yield return (new Vector3(0.5f, -0.5f, -1f));

            yield return (new Vector3(-0.5f, -0.5f, -1f));
            yield return (new Vector3(-0.5f, 0.5f, -1f));
            yield return (new Vector3(0.5f, 0.5f, -1f));
        }

        public static IEnumerable<Vector3> CubeVerts()
        {
            //Top
            yield return (new Vector3(-0.5f, 0.5f, 0.5f));
            yield return (new Vector3(0.5f, 0.5f, -0.5f));
            yield return (new Vector3(0.5f, 0.5f, 0.5f));

            yield return (new Vector3(-0.5f, 0.5f, 0.5f));
            yield return (new Vector3(-0.5f, 0.5f, -0.5f));
            yield return (new Vector3(0.5f, 0.5f, -0.5f));

            //Bottom
            yield return (new Vector3(-0.5f, -0.5f, 0.5f));
            yield return (new Vector3(0.5f, -0.5f, 0.5f));
            yield return (new Vector3(0.5f, -0.5f, -0.5f));

            yield return (new Vector3(-0.5f, -0.5f, 0.5f));
            yield return (new Vector3(0.5f, -0.5f, -0.5f));
            yield return (new Vector3(-0.5f, -0.5f, -0.5f));

            //Close
            yield return (new Vector3(-0.5f, -0.5f, 0.5f));
            yield return (new Vector3(0.5f, 0.5f, 0.5f));
            yield return (new Vector3(0.5f, -0.5f, 0.5f));

            yield return (new Vector3(-0.5f, -0.5f, 0.5f));
            yield return (new Vector3(-0.5f, 0.5f, 0.5f));
            yield return (new Vector3(0.5f, 0.5f, 0.5f));

            //Far
            yield return (new Vector3(-0.5f, -0.5f, -0.5f));
            yield return (new Vector3(0.5f, -0.5f, -0.5f));
            yield return (new Vector3(0.5f, 0.5f, -0.5f));

            yield return (new Vector3(-0.5f, -0.5f, -0.5f));
            yield return (new Vector3(0.5f, 0.5f, -0.5f));
            yield return (new Vector3(-0.5f, 0.5f, -0.5f));

            //Left
            yield return (new Vector3(-0.5f, -0.5f, 0.5f));
            yield return (new Vector3(-0.5f, -0.5f, -0.5f));
            yield return (new Vector3(-0.5f, 0.5f, -0.5f));

            yield return (new Vector3(-0.5f, -0.5f, 0.5f));
            yield return (new Vector3(-0.5f, 0.5f, -0.5f));
            yield return (new Vector3(-0.5f, 0.5f, 0.5f));

            //Right
            yield return (new Vector3(0.5f, -0.5f, 0.5f));
            yield return (new Vector3(0.5f, 0.5f, -0.5f));
            yield return (new Vector3(0.5f, -0.5f, -0.5f));

            yield return (new Vector3(0.5f, -0.5f, 0.5f));
            yield return (new Vector3(0.5f, 0.5f, 0.5f));
            yield return (new Vector3(0.5f, 0.5f, -0.5f));
        }

        public static IEnumerable<Vector3> SphereVerts(int lod)
        {
            var vectors = new List<Vector3>();
            var indices = new List<int>();

            Icosahedron(vectors, indices);

            for (var i = 0; i < lod; i++)
                Subdivide(vectors, indices, true);

            /// normalize vectors to "inflate" the icosahedron into a sphere.
            for (var i = 0; i < vectors.Count; i++)
                yield return Vector3.Normalize(vectors[i]);
        }

        private static int GetMidpointIndex(Dictionary<string, int> midpointIndices, List<Vector3> vertices, int i0, int i1)
        {
            var edgeKey = string.Format("{0}_{1}", Math.Min(i0, i1), Math.Max(i0, i1));

            var midpointIndex = -1;

            if (!midpointIndices.TryGetValue(edgeKey, out midpointIndex))
            {
                var v0 = vertices[i0];
                var v1 = vertices[i1];

                var midpoint = (v0 + v1) / 2f;

                if (vertices.Contains(midpoint))
                    midpointIndex = vertices.IndexOf(midpoint);
                else
                {
                    midpointIndex = vertices.Count;
                    vertices.Add(midpoint);
                    midpointIndices.Add(edgeKey, midpointIndex);
                }
            }

            return midpointIndex;
        }

        /// <summary>
        /// create a regular icosahedron (20-sided polyhedron)
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="size"></param>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// note: icosahedron definition may have come from the OpenGL red book. I don't recall where I found it.
        private static void Icosahedron(List<Vector3> vertices, List<int> indices)
        {
            indices.AddRange(
                new int[]
                {
                0,4,1,
                0,9,4,
                9,5,4,
                4,5,8,
                4,8,1,
                8,10,1,
                8,3,10,
                5,3,8,
                5,2,3,
                2,7,3,
                7,10,3,
                7,6,10,
                7,11,6,
                11,0,6,
                0,1,6,
                6,1,10,
                9,0,11,
                9,11,2,
                9,2,5,
                7,2,11
                }
                .Select(i => i + vertices.Count)
            );

            var X = 0.525731112119133606f;
            var Z = 0.850650808352039932f;

            vertices.AddRange(
                new[]
                {
                new Vector3(-X, 0f, Z),
                new Vector3(X, 0f, Z),
                new Vector3(-X, 0f, -Z),
                new Vector3(X, 0f, -Z),
                new Vector3(0f, Z, X),
                new Vector3(0f, Z, -X),
                new Vector3(0f, -Z, X),
                new Vector3(0f, -Z, -X),
                new Vector3(Z, X, 0f),
                new Vector3(-Z, X, 0f),
                new Vector3(Z, -X, 0f),
                new Vector3(-Z, -X, 0f)
                }
            );
        }

        /// <remarks>
        ///      i0
        ///     /  \
        ///    m02-m01
        ///   /  \ /  \
        /// i2---m12---i1
        /// </remarks>
        /// <param name="vectors"></param>
        /// <param name="indices"></param>
        private static void Subdivide(List<Vector3> vectors, List<int> indices, bool removeSourceTriangles)
        {
            var midpointIndices = new Dictionary<string, int>();

            var newIndices = new List<int>(indices.Count * 4);

            if (!removeSourceTriangles)
                newIndices.AddRange(indices);

            for (var i = 0; i < indices.Count - 2; i += 3)
            {
                var i0 = indices[i];
                var i1 = indices[i + 1];
                var i2 = indices[i + 2];

                var m01 = GetMidpointIndex(midpointIndices, vectors, i0, i1);
                var m12 = GetMidpointIndex(midpointIndices, vectors, i1, i2);
                var m02 = GetMidpointIndex(midpointIndices, vectors, i2, i0);

                newIndices.AddRange(
                    new[] {
                    i0,m01,m02
                    ,
                    i1,m12,m01
                    ,
                    i2,m02,m12
                    ,
                    m02,m01,m12
                    }
                    );
            }

            indices.Clear();
            indices.AddRange(newIndices);
        }
    }
}