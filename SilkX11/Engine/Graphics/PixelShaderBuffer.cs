using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DemoSilkX11.Engine.Graphics
{
    public struct PointLightData
    {
        public Vector3 Position;
        public float Intensity;
        public Vector3 Color;
        private float padding1;
        public Vector3 Attenuation;
        private float padding2;
        private Vector4 padding3;
    }
    public struct PixelShaderBuffer
    {
        public PointLightData Light; // 64 bytes
    }
}
