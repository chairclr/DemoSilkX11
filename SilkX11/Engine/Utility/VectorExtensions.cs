using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DemoSilkX11.Utility
{
    public static class VectorExtensions
    {
        public static Quaternion ToQuaternion(this Vector3 v)
        {
            return Quaternion.CreateFromYawPitchRoll(v.X, v.Y, v.Z);
        }

        public static Vector2 XY(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }
    }
}
