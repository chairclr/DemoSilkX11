using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DemoSilkX11.Utility
{
    public static class MatrixExtensions
    {
        public static Matrix4x4 CreatePerspectiveFieldOfViewLH(float fovRad, float aspectRatio, float nearZ, float farZ)
        {
            if (nearZ <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(fovRad));

            if (farZ <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(aspectRatio));

            if (nearZ == farZ)
                throw new ArgumentOutOfRangeException(nameof(nearZ));

            Matrix4x4 mat = new Matrix4x4();

            float frustumDepth = farZ - nearZ;
            float oneOverDepth = 1 / frustumDepth;

            mat.M22 = 1 / MathF.Tan(0.5f * fovRad);
            mat.M11 = mat.M22 / aspectRatio;
            mat.M33 = farZ * oneOverDepth;
            mat.M43 = (-farZ * nearZ) * oneOverDepth;
            mat.M34 = 1;
            mat.M44 = 0;

            return mat;
        }
    }
}
