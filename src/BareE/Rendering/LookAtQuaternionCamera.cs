﻿using System;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace BareE.Rendering
{

    public class OrthographicCamera : Camera
    {
        public OrthographicCamera(float camWidth, float camHeight, float camDepth)
        {
            CameraSize = new Vector3(camWidth, camHeight, camDepth);
        }
        public override Vector3 Position { get; set; }
        public Vector3 CameraSize { get; set; }
        
        public float BoundsLeft
        {
            get { return Position.X - (CameraSize.X / 2.0f); }
        }
        public float BoundsRight
        {
            get { return Position.X + (CameraSize.X / 2.0f); }
        }
        public float BoundsBottom
        {
            get { return Position.Y - (CameraSize.Y / 2.0f); }
        }

        public float BoundsTop
        {
            get { return Position.Y + (CameraSize.Y / 2.0f); }
        }

        public float BoundsNear
        {
            get { return Position.Z; }
        }

        public float BoundsFar
        {
            get { return Position.Z + (CameraSize.Z); }
        }

        public override Matrix4x4 CamMatrix
        {
            get
            {
                return Matrix4x4.CreateOrthographicOffCenter(BoundsLeft, BoundsRight, BoundsBottom, BoundsTop, BoundsNear, BoundsFar);
                //return Matrix4x4.CreateOrthographicOffCenter(boundsLeft, boundsRight, boundsBottom, boundsTop, boundsNear, boundsFar);
            }
        }

        public override void Move(Vector3 amount)
        {
            Position += amount;
        }

        public override void Pan(float v)
        {
            throw new NotImplementedException();
        }

        public override void Pitch(float v)
        {
            throw new NotImplementedException();
        }

        public override void Roll(float v)
        {
            throw new NotImplementedException();
        }

        public override void Set(Vector3 position, Vector3 lookat, Vector3 up)
        {
            Position = position;
        }

        public override void Tilt(float v)
        {
            throw new NotImplementedException();
        }

        public override void Yaw(float v)
        {
            throw new NotImplementedException();
        }

        public override void Zoom(float amount)
        {
            var newX = CameraSize.X + amount;
            if (newX <= 0)
                return;
            var newY = CameraSize.Y + (amount * (CameraSize.Y / CameraSize.X));
            if (newY <= 0) return;
            CameraSize = new Vector3(
                newX,
                newY,
                CameraSize.Z
                );
        }
    }
    public class LookAtQuaternionCamera : Camera
    {
        private float AspectRatio
        { get { return Size.X / Size.Y; } }

        private float FieldOfView { get; set; } = 1.5f;
        public bool useLeftHanded = false;

        public override float NearPlane { get; set; } = 0.25f;
        public override float FarPlane { get; set; } = 1024.0f;


        private Matrix4x4 LookAtMatrix
        {
            get
            {
                if (useLeftHanded)
                    return CreateLookAtLH(Position, Position + Forward, Up);
                return CreateLookAtRH(Position, Position + Forward, Up);
            }
        }

        private Matrix4x4 ProjectionMatrix
        {
            get
            {
                return Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
            }
        }

        private Matrix4x4 TranslationMatrix { get; set; } = Matrix4x4.Identity;
        private Matrix4x4 RotationMatrix { get; set; } = Matrix4x4.Identity;

        public override Vector3 Position
        {
            get { return Vector3.Transform(Vector3.Zero, TranslationMatrix); }
            set { TranslationMatrix = Matrix4x4.CreateTranslation(value); }
        }

        private Vector2 Size;

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

        public LookAtQuaternionCamera() : this(new Vector2(2.0f, 2.0f))
        {
        }

        public LookAtQuaternionCamera(Vector2 size)
        {
            Size = size;
        }

        private Matrix4x4 CreateLookAtLH(Vector3 camPos, Vector3 camTarget, Vector3 camUp)
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

        private Matrix4x4 CreateLookAtRH(Vector3 camPos, Vector3 camTarget, Vector3 camUp)
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

        private Matrix4x4 _InvertexProjectionMatrix;

        private Matrix4x4 InvertedProjectionMatrix
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

        private Matrix4x4 _InvertedWorldMatrix;

        private Matrix4x4 InvertedLookAtMatrix
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
        /*
        public override Vector3 ToEyespace(Vector3 pt)
        {
            return Vector3.Transform(pt, InvertedProjectionMatrix);
        }

        public override Vector3 ToWorldspace(Vector3 pt)
        {
            Matrix4x4 toEyespace = InvertedProjectionMatrix;
            Matrix4x4 toWorldSpace = InvertedLookAtMatrix;
            Matrix4x4 invertedCam;
            Matrix4x4.Invert(CamMatrix, out invertedCam);

            var v = Vector4.Transform(new Vector4(pt, 1), invertedCam);
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }
        */
        //Point in WorldSpace Sent To NDC
        public override Vector3 Project(Vector3 pt)
        {
            var v = Vector4.Transform(new Vector4(pt, 1), CamMatrix);
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }

        //Point in NDC Sent to Worldspace
        public override Vector3 Unproject(Vector3 pt)
        {
            Matrix4x4 mC=CamMatrix;
            Matrix4x4.Invert(mC, out mC);
            var v = Vector4.Transform(new Vector4(pt, 1), mC);
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }


    }
}