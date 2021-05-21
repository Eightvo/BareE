using BareE.DataStructures;

using Newtonsoft.Json;

using System.Numerics;

namespace BareE.Components
{
    [Component("Pos")]
    public class Pos
    {
        [JsonIgnore]
        public Matrix4x4 TranslationMatrix { get; set; } = Matrix4x4.Identity;

        [JsonIgnore]
        public Matrix4x4 RotationMatrix { get; set; } = Matrix4x4.Identity;

        [JsonIgnore]
        public Matrix4x4 Scale { get; set; } = Matrix4x4.Identity;

        public Vector3 Position
        {
            get { return Vector3.Transform(Vector3.Zero, TranslationMatrix); }
            set { TranslationMatrix = Matrix4x4.CreateTranslation(value); }
        }

        public Vector3 Up
        {
            get
            {
                //if (LockUp) return Vector3.UnitY;
                return Vector3.Transform(Vector3.UnitY, RotationMatrix);
            }
        }

        public Vector3 Forward
        {
            get
            {
                return Vector3.Transform(Vector3.UnitZ, RotationMatrix);
            }
        }

        public void Set(Vector3 position, Vector3 lookat, Vector3 up)
        {
            this.Position = position;
            this.RotationMatrix = CreateRotationMatrixFromForwardAndUp(lookat - position, up);
            //throw new NotImplementedException();
        }

        public Matrix4x4 CreateRotationMatrixFromForwardAndUp(Vector3 fwrd, bool lockUp = true)
        {
            return CreateRotationMatrixFromForwardAndUp(fwrd, lockUp ? Vector3.UnitY : Up);
        }

        public Matrix4x4 CreateRotationMatrixFromForwardAndUp(Vector3 fwrd, Vector3 camUp)
        {
            var zAxis = fwrd;
            var xAxis = Vector3.Normalize(Vector3.Cross(camUp, zAxis));
            var yAxis = Vector3.Cross(zAxis, xAxis);
            return new Matrix4x4(
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -Vector3.Dot(xAxis, Vector3.Zero), -Vector3.Dot(yAxis, Vector3.Zero), -Vector3.Dot(zAxis, Vector3.Zero), 1.0f
                );
        }

        public static Pos Create(float scale, Vector3 pos, Vector3 lookat, Vector3 up)
        {
            var p = new Pos();
            p.Set(pos, lookat, up);
            p.Scale = Matrix4x4.CreateScale(scale);
            return p;
        }

        public static Pos Create(Vector3 pos, Vector3 lookat)
        {
            return Create(1.0f, pos, lookat, Vector3.UnitY);
        }

        public static Pos Create(Vector3 pos)
        {
            return Create(1.0f, pos, pos + Vector3.UnitZ, Vector3.UnitY);
        }

        public static Pos Create(float scale, Vector3 pos)
        {
            return Create(scale, pos, pos + Vector3.UnitZ, Vector3.UnitY);
        }
    }
}