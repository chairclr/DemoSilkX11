using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DemoSilkX11.Engine.Graphics
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 UV;
        public Vector3 Normal;

        public Vertex(float x, float y, float z, float u, float v, float normalX, float normalY, float noramlZ)
        {
            Position = new Vector3(x, y, z);
            UV = new Vector2(u, v);
            Normal = new Vector3(normalX, normalY, noramlZ);
        }
        public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
        {
            Position = position;
            UV = uv;
            Normal = normal;
        }
    }
}
