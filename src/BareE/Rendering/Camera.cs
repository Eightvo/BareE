using System;
using System.Numerics;

namespace BareE.Rendering
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

        public virtual Vector3 Position { get; set; }
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
}