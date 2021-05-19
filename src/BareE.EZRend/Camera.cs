using System;
using System.Numerics;

namespace EZRend
{
    public abstract class Camera
    {
        public abstract Matrix4x4 CamMatrix
        {
            get;
        }
        public virtual bool LockUp { get; set; }
        public abstract void Zoom(float amount);
        public abstract void Move(Vector3 amount);
        public abstract void Roll(float v);
        public abstract void Pitch(float v);
        public abstract void Yaw(float v);
        public abstract void Tilt(float v);
        public abstract void Pan(float v);
        public abstract void Set(Vector3 position, Vector3 lookat, Vector3 up);
        public virtual Vector3 Position { get;  set; }
        public virtual Vector3 Forward { get; }
        public virtual Vector3 Up { get; }

        public virtual Vector3 ToEyespace(Vector3 pt)
        {
            throw new NotImplementedException();
        }
        public virtual Vector3 ToWorldspace(Vector3 pt)
        {
            throw new NotImplementedException();
        }
        public virtual Vector3 Project(Vector3 pt)
        {
            throw new NotImplementedException();
        }
    }
  /*
    public class PerspectiveCamera : Camera
    {
        Vector2 Size = new Vector2(2.0f, 2.0f);
        Quaternion Rotation = Quaternion.CreateFromRotationMatrix(Matrix4x4.Identity);
        public override Matrix4x4 CamMatrix
        {

            get
            {
                var p = Matrix4x4.CreatePerspective(Size.X, Size.Y, 0.5f, 100.0f);
                var t = Matrix4x4.CreateTranslation(Position);
                var r = Matrix4x4.CreateFromQuaternion(Rotation);
                //return p * t * r;
                //return r * t * p;
                var q = Matrix4x4.Multiply(r, t);
                return Matrix4x4.Multiply(q, p);
            }
        }

        public override void Zoom(float amount)
        {
            Position = new Vector3(Position.X, Position.Y, Position.Z + amount);
        }
        public override void Move(Vector3 amount)
        {
            Position += new Vector3(-amount.X, -amount.Y, amount.Z);
        }
        public override void Roll(float v)
        {
            Rotation = Rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, v);
        }
        public override void Pitch(float v)
        {
            Rotation = Rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitX, v);
        }
        public override void Yaw(float v)
        {
            Rotation = Rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitY, v);
        }


    }
    */
    public class LookAtQuaternionCamera : Camera
    {
        
        float AspectRatio { get { return Size.X / Size.Y; } }
        float FieldOfView { get; set; } = 1.5f;
        public bool useLeftHanded = false;
        Matrix4x4 LookAtMatrix
        {
            get
            {
                if (useLeftHanded)
                    return CreateLookAtLH(Position, Position + Forward, Up);
                return CreateLookAtRH(Position, Position + Forward, Up);
            }
        }
        Matrix4x4 ProjectionMatrix
        {
            get
            {
                return Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, 0.25f, 1024.0f);
            }
        }

        Matrix4x4 TranslationMatrix { get; set; } = Matrix4x4.Identity;
        Matrix4x4 RotationMatrix { get; set; } = Matrix4x4.Identity;
 
        public override Vector3 Position
        {
            get { return Vector3.Transform(Vector3.Zero, TranslationMatrix); }
            set { TranslationMatrix = Matrix4x4.CreateTranslation(value); }
        }
        Vector2 Size;
        public override Vector3 Up
        {
            get
            {
                if (LockUp) return Vector3.UnitY;
                return Vector3.Transform(Vector3.UnitY, RotationMatrix);
            }
        }
        public override Vector3 Forward
        {
            get
            {
                return Vector3.Transform(-Vector3.UnitZ, RotationMatrix);
            }
        }
        public LookAtQuaternionCamera(): this(new Vector2(2.0f, 2.0f)) { }
        public LookAtQuaternionCamera(Vector2 size)
        {
            Size = size;
        }
        Matrix4x4 CreateLookAtLH(Vector3 camPos, Vector3 camTarget, Vector3 camUp)
        {
            var zAxis = Vector3.Normalize( camTarget-camPos);
            var xAxis = Vector3.Normalize(Vector3.Cross(camUp, zAxis));
            var yAxis = Vector3.Cross(zAxis, xAxis);
            return new Matrix4x4(
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -Vector3.Dot(xAxis, camPos), -Vector3.Dot(yAxis, camPos), -Vector3.Dot(zAxis, camPos), 1.0f
                );
        }
        Matrix4x4 CreateLookAtRH(Vector3 camPos, Vector3 camTarget, Vector3 camUp)
        {
            var zaxis = Vector3.Normalize(camPos - camTarget);
            var xaxis = Vector3.Normalize(Vector3.Cross(camUp, zaxis));
            var yaxis = Vector3.Cross(zaxis, xaxis);
            return new Matrix4x4(xaxis.X, yaxis.X, zaxis.X, 0,
                                 xaxis.Y, yaxis.Y, zaxis.Y, 0,
                                 xaxis.Z, yaxis.Z, zaxis.Z, 0,
                                 -Vector3.Dot(xaxis, camPos), -Vector3.Dot(yaxis, camPos), -Vector3.Dot(zaxis, camPos), 1);

        }
        public override Matrix4x4 CamMatrix
        {
            get
            {
                return LookAtMatrix * ProjectionMatrix;
            }
        }

        public override void Move(Vector3 amount)
        {
            Vector3 trueDir = Vector3.Transform(amount, RotationMatrix);
            Position = Position + trueDir;
        }

        /// <summary>
        /// Rotate on X axis relative to camera
        /// </summary>
        /// <param name="v"></param>
        public override void Pitch(float v)
        {
            RotationMatrix = Matrix4x4.Transform(RotationMatrix, Quaternion.CreateFromAxisAngle(Vector3.Cross(Forward, Up), v));
        }
        /// <summary>
        /// Rotate on X axis relative to world
        /// </summary>
        /// <param name="v"></param>
        public override void Tilt(float v)
        {
            RotationMatrix = Matrix4x4.Transform(RotationMatrix, Quaternion.CreateFromAxisAngle(Vector3.Cross(Vector3.UnitZ, Vector3.UnitY), v));

        }

        public override void Roll(float v)
        {
            RotationMatrix = Matrix4x4.Transform(RotationMatrix, Quaternion.CreateFromAxisAngle(Forward, v));
        }

        /// <summary>
        /// Rotate on y axis relative to cam.
        /// </summary>
        /// <param name="v"></param>
        public override void Yaw(float v)
        {
            RotationMatrix = Matrix4x4.Transform(RotationMatrix, Quaternion.CreateFromAxisAngle(Up, v));
        }
        /// <summary>
        /// Rotate on y axis relative to world.
        /// </summary>
        /// <param name="v"></param>
        public override void Pan(float v)
        {
            RotationMatrix = Matrix4x4.Transform(RotationMatrix, Quaternion.CreateFromAxisAngle(Vector3.UnitY, v));
        }
        public override void Zoom(float amount)
        {
            FieldOfView += amount;
            if (FieldOfView <= 0.1f) FieldOfView = 0.1f;
            if (FieldOfView > Math.PI / 2.0f) FieldOfView = (float)Math.PI / 2.0f;
        }
        public override void Set(Vector3 position, Vector3 lookat, Vector3 up)
        {
            this.Position = position;
            this.RotationMatrix = CreateRotationMatrixFromForwardAndUp(lookat - position, up);
            //throw new NotImplementedException();
        }
        private Matrix4x4 CreateRotationMatrixFromForwardAndUp(Vector3 fwrd, Vector3 camUp)
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



        Matrix4x4 _InvertexProjectionMatrix;
        Matrix4x4 InvertedProjectionMatrix
        {
            get
            {

                if (Matrix4x4.Invert(ProjectionMatrix, out _InvertexProjectionMatrix))
                {
                    return _InvertexProjectionMatrix;
                }
                throw new Exception("Invertion not exist?");
            }
        }
        Matrix4x4 _InvertedWorldMatrix;
        Matrix4x4 InvertedWorldMatrix
        {
            get
            {
                if (Matrix4x4.Invert(LookAtMatrix, out _InvertedWorldMatrix))
                {
                    return _InvertedWorldMatrix;
                }
                throw new Exception("Invertion does not exist");
            }
        }

        public override Vector3 ToEyespace(Vector3 pt)
        {

            return Vector3.Transform(pt, InvertedProjectionMatrix);
            
        }
        public override Vector3 ToWorldspace(Vector3 pt)
        {

            Matrix4x4 toEyespace = InvertedProjectionMatrix;
            Matrix4x4 toWorldSpace = InvertedWorldMatrix;
            Matrix4x4 invertedCam;
            Matrix4x4.Invert(CamMatrix, out invertedCam);

            // return Vector3.Transform(pt, toWorldSpace * toEyespace);
            //return Vector3.Transform(pt, toEyespace*toWorldSpace);
            //return Vector3.Transform(ToEyespace(pt), toWorldSpace);
            //return Vector3.Transform(pt, invertedCam);
            var v = Vector4.Transform(new Vector4(pt, 1), invertedCam);
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);

        }

        //Point in WorldSpace Sent To NDC
        public override Vector3 Project(Vector3 pt)
        {
            var v = Vector4.Transform(new Vector4(pt,1), CamMatrix);
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }
    }
    /*
    public class LookAtCamera : Camera
    {

        Vector2 Size = new Vector2(2.0f, 2.0f);
        Vector3 Forward = Vector3.UnitZ;
        Vector3 Up = Vector3.UnitY;
        public LookAtCamera() : this(new Vector2(2.0f, 2.0f)) { }
        public LookAtCamera(Vector2 size)
        {
            Size = size;
        }
        public void printMat(Matrix4x4 mR)
        {
            Console.WriteLine($"{mR.M11}{mR.M12}{mR.M13}{mR.M14}");
            Console.WriteLine($"{mR.M21}{mR.M22}{mR.M23}{mR.M24}");
            Console.WriteLine($"{mR.M31}{mR.M32}{mR.M33}{mR.M34}");
            Console.WriteLine($"{mR.M41}{mR.M42}{mR.M43}{mR.M44}");
        }

        Matrix4x4 CreateLookAtLH(Vector3 camPos, Vector3 camTarget, Vector3 camUp)
        {
            var zAxis = Vector3.Normalize(camTarget - camPos);
            var xAxis = Vector3.Normalize(Vector3.Cross(camUp, zAxis));
            var yAxis = Vector3.Cross(zAxis, xAxis);
            return new Matrix4x4(
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -Vector3.Dot(xAxis, camPos), -Vector3.Dot(yAxis, camPos), -Vector3.Dot(zAxis, camPos), 1.0f
                );
        }


        public override Matrix4x4 CamMatrix
        {
            get
            {

               
 return CreateLookAtLH(Position, Position + Forward, Up) * Matrix4x4.CreatePerspective(Size.X, Size.Y, 0.5f, 100); 
                //Original
                //return Matrix4x4.CreateLookAt(Position + Forward, Position, Up) * Matrix4x4.CreatePerspective(Size.X, Size.Y, 0.5f, 100);

                //return Matrix4x4.CreateLookAt(Position, Position-Forward, -Up) * Matrix4x4.CreatePerspective(Size.X, Size.Y, 0.5f, 100);
                
                
                //return  Matrix4x4.CreatePerspective(Size.X, Size.Y, 0.5f, 100)* Matrix4x4.CreateLookAt(Position + Forward, Position, Up);
            }
        }

        public override void Move(Vector3 amount)
        {
            var dir = Vector3.Normalize(amount);

            var delta = Vector3.Transform(Vector3.Normalize(amount), CreateLookAtLH(Position, Position + Forward, Up));
            var mag = amount.Length();
            Position += delta*mag;
        }

        public override void Zoom(float amount)
        {
            Position += -Forward;
        }
        public override void Roll(float v)
        {
            var r = Quaternion.CreateFromAxisAngle(Forward, v);
            Up = Vector3.Transform(Up, r);
            Forward = Vector3.Transform(Forward, r);
        }
        public override void Pitch(float v)
        {
            var r = Quaternion.CreateFromAxisAngle(Vector3.Cross(Forward,Up), v);
            Up = Vector3.Transform(Up, r);
            Forward = Vector3.Transform(Forward, r);
        }
        public override void Yaw(float v)
        {
            var r = Quaternion.CreateFromAxisAngle(Up, v);
            Up = Vector3.Transform(Up, r);
            Forward = Vector3.Transform(Forward, r);
        }
    }
    public class OrthoCamera : Camera
    {
        //Vector3 Position = new Vector3(0.0f, 0.0f,0f);
        Vector2 Size = new Vector2(2.0f, 2.0f);
        float Rotation = 0.0f;
        public override Matrix4x4 CamMatrix
        {
            get
            {
                var P = Matrix4x4.CreateOrthographic(Size.X, Size.Y, 0.5f, 100); //Matrix4x4.CreateOrthographicOffCenter(BottomLeft.X, BottomLeft.X + Size.X, BottomLeft.Y, BottomLeft.Y + Size.Y, 0, 100);
                var R = Matrix4x4.CreateRotationZ(Rotation);
                var T = Matrix4x4.CreateTranslation(new Vector3(-Position.X,-Position.Y,0));
                return   T*R*P;
            }
        }

        public override void Zoom(float amount)
        {
            Size = new Vector2(Size.X + amount, Size.Y + amount);
        }
        public override void Move(Vector3 amount)
        {
             Position += Vector3.Transform(amount, Matrix4x4.CreateRotationZ(Rotation));
            //Position += amount;
        }
        public override void Roll(float v)
        {
            Rotation += v;
        }
        public override void Pitch(float v)
        {
            throw new NotImplementedException();
        }
        public override void Yaw(float v)
        {
            throw new NotImplementedException();
        }

    }
    */
}
