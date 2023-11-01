using System;
using System.Numerics;

namespace BareE.Rendering
{
    public class Perspective : Camera
    {
        public override Matrix4x4 CamMatrix
        {
            get
            {
                return Matrix4x4.CreateLookAt(Position, Position + Forward, Up)*Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegToRad(Fov), ScreenSize.X/ScreenSize.Y, NearPlane, FarPlane);
            }
        }

        Vector3 cameraRight;
        Vector3 worldUp = Vector3.UnitY;



        internal Vector2 ScreenSize;
        internal float Fov = 45.0f;

        public float _Yaw =-90;
        public float _Pitch;
        public float _Roll;

        bool ConstrainPitch = true;

        public Perspective(Vector3 cameraPos, Vector3 cameraForward, Vector3 cameraUp, Vector2 screenSize, float nearPlane, float farPlane, float fov)
        {
            Position = cameraPos;
            Forward = cameraForward;
            Up = cameraUp;
            ScreenSize = screenSize;
            NearPlane = nearPlane;
            FarPlane = farPlane;
            Fov = fov;

            UpdateVectors();
        }

        public override void Move(Vector3 amount)
        {
            var forwardDist = amount.Z * Vector3.Normalize(-Forward);
            var rightDist = amount.X * Vector3.Normalize(cameraRight);
            var upDist = amount.Y * Vector3.Normalize(Up);
            //if (amount.LengthSquared() == 0) return;
            Position += (forwardDist + rightDist + upDist);
        }

        public override void Pan(float v)
        {
            throw new NotImplementedException();
            //Move(new Vector3())
        }

        public override void Pitch(float v)
        {
            _Pitch += v;
            if (ConstrainPitch)
            {
                if (_Pitch < -89) _Pitch = -89;
                if (_Pitch > 89) _Pitch = 89;
            }
            
            UpdateVectors();
        }

        public override void Roll(float v)
        {
            //0,1,0 : Normal
            //1,0,0: Rolled Right
            //-1,0,0:Rolled Left
            _Roll += v;
            UpdateVectors();
        }

        public override void Set(Vector3 position, Vector3 forward, Vector3 up)
        {
            Position= position;
            Forward = forward;
            Up= up;
        }

        public override void Tilt(float v)
        {
            throw new NotImplementedException();
        }

        public override void Yaw(float v)
        {
            _Yaw += v;
           // while (_Yaw < 0) _Yaw += 360;
           // while (_Yaw > 360) _Yaw -= 360;
            //while (_Yaw < 0) _Yaw += 360;
            //if (_Yaw > 360) _Yaw = _Yaw % 360;
            UpdateVectors();
        }

        public override void Zoom(float amount)
        {
            Fov -= amount;
            if (Fov < 10) Fov = 10;
            if (Fov > 170) Fov = 170;
        }
        void UpdateVectors()
        {
            Vector3 forward = new Vector3();
                        forward.X = (float)(Math.Cos(MathHelper.DegToRad(_Yaw) ));
                        forward.Y = (float)(Math.Sin(MathHelper.DegToRad(_Pitch)));
                        forward.Z = (float)(Math.Sin(MathHelper.DegToRad(_Yaw) ));

            //            forward.X = (float)(Math.Cos(MathHelper.DegToRad(_Yaw) * Math.Cos(MathHelper.DegToRad(_Pitch))));
            //            forward.Y = (float)(Math.Sin(MathHelper.DegToRad(_Pitch)));
            //            forward.Z = (float)(Math.Sin(MathHelper.DegToRad(_Yaw) * Math.Cos(MathHelper.DegToRad(_Pitch))));
            Forward = Vector3.Normalize(forward);


            worldUp = new Vector3(0, 0, 0);
            worldUp.X = (float)(Math.Sin(MathHelper.DegToRad(_Roll)));
            worldUp.Y = (float)(Math.Cos(MathHelper.DegToRad(_Roll)));
            worldUp = Vector3.Normalize(worldUp);

            cameraRight = Vector3.Normalize(Vector3.Cross(Forward, worldUp));
            Up = Vector3.Normalize(Vector3.Cross(cameraRight, Forward));

        }

        //Point in WorldSpace Sent To NDC
        public override Vector3 Project(Vector3 pt)
        {
            var v = Vector4.Transform(new Vector4(pt, 1), CamMatrix);
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }

        //Point in NDC Sent to Worldspace
        public override Vector3 Unproject(Vector3 pt)
        {
            Matrix4x4 mC = CamMatrix;
            Matrix4x4.Invert(mC, out mC);
            var v = Vector4.Transform(new Vector4(pt, 1), mC);
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }
    }
}