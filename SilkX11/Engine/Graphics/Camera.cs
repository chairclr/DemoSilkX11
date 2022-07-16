using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using DemoSilkX11.Engine.Graphics;
using DemoSilkX11.Utility;
using DemoSilkX11.Engine.Graphics.Windows;
using ImGuiNET;
using Color = DemoSilkX11.Utility.Color;
using System.Runtime.InteropServices;

namespace DemoSilkX11.Engine.Graphics
{
    public class Camera
    {
        public static Camera MainCamera;

        public Camera(float fovDegrees, float aspectRatio, float nearZ, float farZ)
        {
            if (MainCamera == null)
                MainCamera = this;
            rotation = Quaternion.Identity;
            UpdateProjection(fovDegrees, aspectRatio, nearZ, farZ);
            UpdateView();
        }

        public Camera(Vector2 size, float nearZ, float farZ)
        {
            if (MainCamera == null)
                MainCamera = this;
            rotation = Quaternion.Identity;
            UpdateProjection(size, nearZ, farZ);
            UpdateView();
        }

        private Matrix4x4 viewMatrix;
        private Matrix4x4 projectionMatrix;
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 forward;
        private Vector3 left;
        private Vector3 up;

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                UpdateView();
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                UpdateView();
            }
        }
        public Vector3 YawPitchRoll
        {
            set
            {
                Rotation = Quaternion.CreateFromYawPitchRoll(value.X, value.Y, value.Z);
            }
        }

        public Matrix4x4 ViewMatrix => viewMatrix;
        public Matrix4x4 ProjectionMatrix => projectionMatrix;


        public Vector3 Forward => forward;
        public Vector3 Backward => -forward;

        public Vector3 Left => left;
        public Vector3 Right => -left;

        public Vector3 Up => up;
        public Vector3 Down => -up;

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

        public Vector3 WorldToScreenUV(Vector3 point)
        {
            Vector3 v = (Vector3.Transform(Vector3.Transform(point, ViewMatrix), projectionMatrix));
            v.X /= v.Z;
            v.Y /= v.Z;
            v.Y = -v.Y;
            v.X /= 2f;
            v.Y /= 2f;
            v.X += 0.5f;
            v.Y += 0.5f;
            return v;
        }
        public Vector3 WorldToScreen(Vector3 point, Vector2 screenSize)
        {
            Vector3 v = (Vector3.Transform(Vector3.Transform(point, ViewMatrix), projectionMatrix));
            v.X /= v.Z;
            v.Y /= v.Z;
            v.Y = -v.Y;
            v.X /= 2f;
            v.Y /= 2f;
            v.X += 0.5f;
            v.Y += 0.5f;
            return v * new Vector3(screenSize, 1.0f);
        }

        public Vector3 ScreenToWorld(Vector3 point, Vector2 screenSize)
        {
            //point /= new Vector3(screenSize, 1.0f);

            Vector3 inp = point;

            float winZ = 1.0f;

            inp.X = (2.0f * ((float)(point.X - 0.0f) / (screenSize.X - 0.0f))) - 1.0f;
            inp.Y = 1.0f - (2.0f * ((float)(point.Y - 0.0f) / (screenSize.Y - 0.0f)));
            inp.Z = point.Z;

            Matrix4x4.Invert(ViewMatrix * projectionMatrix, out Matrix4x4 c);

            Vector3 v = (Vector3.Transform(inp, c));

            v.X *= v.Z;
            v.Y *= v.Z;

            return new Vector3(v.X, v.Y, v.Z);
        }

        public Vector3 ScreenToRayWorld(Vector2 pos, Vector2 winSize)
        {
            Matrix4x4.Invert(ViewMatrix * ProjectionMatrix, out Matrix4x4 c);

            Vector2 halfWindowSize = winSize / 2f;
            Vector4 near = new Vector4((pos.X - halfWindowSize.X) / halfWindowSize.X, -1 * (pos.X - halfWindowSize.Y) / halfWindowSize.Y, -1f, 1.0f);
            Vector4 far = new Vector4((pos.X - halfWindowSize.X) / halfWindowSize.X, -1 * (pos.Y - halfWindowSize.Y) / halfWindowSize.Y, 1f, 1.0f);
            Vector4 nearResult = Vector4.Transform(near, c);
            Vector4 farResult = Vector4.Transform(far, c);
            nearResult /= nearResult.W;
            farResult /= farResult.W;
            Vector4 dir4 = farResult - nearResult;
            return Vector3.Normalize(new Vector3(dir4.X, dir4.Y, dir4.Z));

        }

        public void UpdateProjection(float fovDegrees, float aspectRatio, float nearZ, float farZ)
        {
            float fovRadians = (fovDegrees / 360.0f) * (MathF.PI * 2f);
            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, aspectRatio, nearZ, farZ);
        }
        public void UpdateProjection(Vector2 size, float nearZ, float farZ)
        {
            projectionMatrix = Matrix4x4.CreatePerspective(size.X, size.Y, nearZ, farZ);
        }
        private void UpdateView()
        {
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(rotation);

            left = Vector3.Transform(-Vector3.UnitX, rotationMatrix);
            up = Vector3.Transform(Vector3.UnitY, rotationMatrix);
            forward = Vector3.Transform(Vector3.UnitZ, rotationMatrix);

            viewMatrix = Matrix4x4.CreateLookAt(position, position + forward, up);
        }
    }
}
