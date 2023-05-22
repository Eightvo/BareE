using BareE.EZRend.VertexTypes;
using BareE.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BareE.EZRend
{
    public class ColoredLineShader : LineShader<Float3_Float4>
    {
        public ColoredLineShader() : base("BareE.EZRend.Novelty.ColoredLines.Line")
        {
        }

        public void AddOrigin()
        {

            AddVertex(new Float3_Float4(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector4(1, 1, 1, 1)));
            AddVertex(new Float3_Float4(new System.Numerics.Vector3(1, 0, 0), new System.Numerics.Vector4(1, 0, 0, 1)));

            AddVertex(new Float3_Float4(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector4(1, 1, 1, 1)));
            AddVertex(new Float3_Float4(new System.Numerics.Vector3(0, 1, 0), new System.Numerics.Vector4(0, 1, 0, 1)));

            AddVertex(new Float3_Float4(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector4(1, 1, 1, 1)));
            AddVertex(new Float3_Float4(new System.Numerics.Vector3(0, 0, 1), new System.Numerics.Vector4(0, 0, 1, 1)));
        }
        public void AddCircle(Vector4 color, Vector2 center, float radius, int resolution)
        {
            AddCircle(color, new Vector3(center, 0), radius, resolution);
        }
        public void AddCircle(Vector4 color, Vector3 center, float radius, int resolution)
        {
            if (resolution < 3) return;
            float delta = 360f / (float)resolution;

            for (int i = 0; i < resolution; i++)
            {
                var n = (i + 1) % resolution;
                var cX = (float)Math.Sin(MathHelper.DegToRad(i * delta)) * radius;
                var cY = (float)Math.Cos(MathHelper.DegToRad(i * delta)) * radius;
                var nX = (float)Math.Sin(MathHelper.DegToRad(n * delta)) * radius;
                var nY = (float)Math.Cos(MathHelper.DegToRad(n * delta)) * radius;
                AddVertex(new Float3_Float4(center + new Vector3(cX, cY, center.Z), color));
                AddVertex(new Float3_Float4(center + new Vector3(nX, nY, center.Z), color));
            }
        }
        public void AddArc(Vector4 color, Vector2 center, float radius, float startAngle, float endAngle, int resolution)
        {
            AddArc(color, new Vector3(center, 0), radius, startAngle, endAngle, resolution);
        }
        public void AddArc(Vector4 color, Vector3 center, float radius, float startAngle, float endAngle, int resolution)
        {
            if (resolution < 3) return;
            while (startAngle < 0) startAngle += 360;
            while (startAngle >= 360) startAngle -= 360;
            while (endAngle < 0) endAngle += 360;
            while (endAngle >= 360) endAngle -= 360;
            if (endAngle < startAngle)
            {
                var t = startAngle;
                startAngle = endAngle;
                endAngle = startAngle;
            }
            float delta = (endAngle - startAngle) / (float)resolution;

            for (int i = 0; i < resolution; i++)
            {
                var n = (i + 1) % resolution;
                var cX = (float)Math.Sin(MathHelper.DegToRad(i * delta)) * radius;
                var cY = (float)Math.Cos(MathHelper.DegToRad(i * delta)) * radius;
                var nX = (float)Math.Sin(MathHelper.DegToRad(n * delta)) * radius;
                var nY = (float)Math.Cos(MathHelper.DegToRad(n * delta)) * radius;
                AddVertex(new Float3_Float4(center + new Vector3(cX, cY, center.Z), color));
                AddVertex(new Float3_Float4(center + new Vector3(nX, nY, center.Z), color));
            }
        }
        public void AddPath(Vector4 color, params Vector2[] points)
        {
            AddPath(color, points, 0);
        }
        public  void AddPath(Vector4 color, IEnumerable<Vector2> points, float z)
        {
            var pointIterator = points.GetEnumerator();
            if (!pointIterator.MoveNext())
                return;
            var prev = pointIterator.Current;
            while (pointIterator.MoveNext())
            {
                AddVertex(new Float3_Float4(new Vector3(prev, z), color));
                AddVertex(new Float3_Float4(new Vector3(pointIterator.Current, z), color));
                prev = pointIterator.Current;
            }
        }

        public void AddPath(Vector4 color, params Vector3[] points)
        {
            bool started = false;
            var prev = points[0];
            foreach (var point in points)
            {
                if (!started) { started = true; continue; }
                AddVertex(new Float3_Float4(prev, color));
                AddVertex(new Float3_Float4(point, color));
                prev = point;
            }
        }
    }
}
