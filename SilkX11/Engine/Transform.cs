using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DemoSilkX11
{
    public class Transform
    {

        private Vector3 position;
        private Vector3 scale;
        private Quaternion rotation;

        private Vector3 forward => Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(Rotation));
        private Vector3 left => Vector3.Transform(-Vector3.UnitX, Matrix4x4.CreateFromQuaternion(Rotation));
        private Vector3 up => Vector3.Transform(Vector3.UnitY, Matrix4x4.CreateFromQuaternion(Rotation));

        public Vector3 Position
        {
            get => position;
            set
            {
                needsToUpdateCache = true;
                position = value;
            }
        }
        public Vector3 Scale
        {
            get => scale;
            set
            {
                needsToUpdateCache = true;
                scale = value;
            }
        }
        public Quaternion Rotation
        {
            get => rotation;
            set
            {
                needsToUpdateCache = true;
                rotation = value;
            }
        }
        public Vector3 YawPitchRoll
        {
            set => Rotation = Quaternion.CreateFromYawPitchRoll(value.X, value.Y, value.Z);
        }

        private Matrix4x4 worldMatrixCache;
        private bool needsToUpdateCache = false;

        public Matrix4x4 WorldMatrix
        {
            get
            {
                if (needsToUpdateCache)
                {
                    worldMatrixCache = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
                    needsToUpdateCache = false;
                }
                return worldMatrixCache;
            }
        }

        public Transform()
        {
            Position = Vector3.Zero;
            Scale = Vector3.One;
            Rotation = Quaternion.Identity;
        }
        public Transform(Vector3 position, Vector3 scale)
        {
            Position = position;
            Scale = scale;
            Rotation = Quaternion.Identity;
        }
        public Transform(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }
        public Transform(Vector3 position, Vector3 scale, Vector3 yawPitchRoll)
        {
            Position = position;
            Scale = scale;
            Rotation = Quaternion.CreateFromYawPitchRoll(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z);
        }

        public Vector3 Forward => forward;
        public Vector3 Backward => -forward;

        public Vector3 Left => left;
        public Vector3 Right => -left;

        public Vector3 Up => up;
        public Vector3 Down => -up;

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }
        public void SetScale(Vector3 scale)
        {
            Scale = scale;
        }
        public void SetRotation(Quaternion rotation)
        {
            Rotation = rotation;
        }

        public Vector3 GetPosition()
        {
            return Position;
        }
        public Vector3 GetScale()
        {
            return Scale;
        }
        public Quaternion GetRotation()
        {
            return Rotation;
        }

        public void SetRotation(Vector3 yawPitchRoll)
        {
            Rotation = Quaternion.CreateFromYawPitchRoll(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z);
        }

        public void LookAt(Vector3 point)
        {
            if (point == Position)
                return;

            point = Position - point;

            float pitch = 0.0f;
            if (point.Y != 0.0f)
            {
                float distance = MathF.Sqrt(point.X * point.X + point.Z * point.Z);
                pitch = MathF.Atan(point.Y / distance);
            }

            float yaw = 0.0f;
            if (point.X != 0.0f)
            {
                yaw = MathF.Atan(point.X / point.Z);
            }
            if (point.Z > 0)
                yaw += MathF.PI;

            YawPitchRoll = new Vector3(yaw, pitch, 0.0f);
        }

        public void SmoothLookAt(Vector3 point, float amount)
        {
            if (point == Position)
                return;

            point = Position - point;

            float pitch = 0.0f;
            if (point.Y != 0.0f)
            {
                float distance = MathF.Sqrt(point.X * point.X + point.Z * point.Z);
                pitch = MathF.Atan(point.Y / distance);
            }

            float yaw = 0.0f;
            if (point.X != 0.0f)
            {
                yaw = MathF.Atan(point.X / point.Z);
            }
            if (point.Z > 0)
                yaw += MathF.PI;

            Rotation = Quaternion.Slerp(Rotation, Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0.0f), amount);
        }
    }
}
