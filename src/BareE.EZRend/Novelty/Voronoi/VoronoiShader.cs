using BareE.EZRend.VertexTypes;
using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

namespace BareE.EZRend.Novelty
{
    public class VoronoiShader:InstancedShader<Float3, Float3_Float4>
    //public class VoronoiShader : VertexOnlyShader<Float3>
    {
        public override DepthStencilStateDescription DepthStencilDescription
        {
            get => new DepthStencilStateDescription(
                        depthTestEnabled: true,
                        depthWriteEnabled: true,
                        comparisonKind: ComparisonKind.Less);
        }
        

        public VoronoiShader(int tri=64,float r=1024, float d=1024):base("BareE.EZRend.Novelty.Voronoi.Voronoi")
        {

            foreach (var v in GeometryFactory.ConeTriangulation(tri,r,d))
            //foreach (var v in GeometryFactory.CubeTriangulation)
            {
                AddVertex(new Float3(v.Pt1));
                AddVertex(new Float3(v.Pt2));
                AddVertex(new Float3(v.Pt3));
            }
        }
        public void AddSeed(Vector2 pos, Vector4 Color)
        {
            AddInstance(new Float3_Float4(new Vector3(pos, 0), Color));
        }

    }
}
