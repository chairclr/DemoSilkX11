using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;
using DemoSilkX11.Engine.Graphics;
using DemoSilkX11.Engine.Graphics.Textures;
using Color = DemoSilkX11.Utility.Color;

namespace DemoSilkX11.Engine.Primitives
{
    public class SphereObject : RenderObject
    {
        public Mesh PlaneMesh;

        public SphereObject(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context, Texture2D texture, int subdivisionCount = 30) : base(device, context)
        {
            //List<Vertex> vertices = new List<Vertex>()
            //{
            //    new Vertex(-0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 0f, 0f, -1f),
            //    new Vertex( 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0f, 0f, -1f),
            //    new Vertex( 0.5f,  0.5f, 0.0f, 1.0f, 1.0f, 0f, 0f, -1f),
            //    new Vertex(-0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0f, 0f, -1f)
            //};
            //List<int> indicies = new List<int>()
            //{
            //    0, 1, 2,
            //    2, 3, 0,
            //    0, 2, 1,
            //    0, 3, 2,
            //};
            throw new NotImplementedException("ill make a sphere later, ok?");
            //List<Vertex> vertices = new List<Vertex>();
            //
            //float goldenRatio = 1.618033988749894f;
            //float angleIncrement = MathF.Tau * goldenRatio;
            //
            //for (int i = 0; i < subdivisionCount; i++)
            //{
            //    float t = (float)i / (float)subdivisionCount;
            //    float a1 = MathF.Acos(1f - 2f * t);
            //    float a2 = angleIncrement * i;
            //
            //    Vector3 pos = new Vector3(MathF.Sin(a1) * MathF.Cos(a2), MathF.Sin(a1) * MathF.Sin(a2), MathF.Cos(a1));
            //
            //    // https://en.wikipedia.org/wiki/UV_mapping
            //    Vector2 uv = new Vector2(
            //        0.5f + (MathF.Atan2(pos.X, pos.Z)) / MathF.Tau,
            //        0.5f + (MathF.Asin(pos.Y)) / MathF.PI);
            //
            //    vertices.Add(new Vertex(pos, uv, Vector3.Zero));
            //}
            //List<int> indicies = new List<int>();
            //float t = 2 * MathF.PI / subdivisionCount;
            //int a = 0, b, c;
            //for (int i = 0; i < subdivisionCount; i++)
            //{
            //    //vertices.Add(u.normalized * uSize * Mathf.Cos(t * i) -
            //    //v.normalized * vSize * Mathf.Sin(t * i));
            //    //
            //    
            //    //normals.Add(normal);
            //    //normals.Add(normal);
            //    //normals.Add(normal);
            //
            //    vertices.Add(new Vertex());
            //
            //    b = i + 1;
            //    c = (i < (subdivisionCount - 1)) ? i + 2 : 1;
            //
            //    indicies.Add(a);
            //    indicies.Add(b);
            //    indicies.Add(c);
            //}


            //PlaneMesh = new Mesh(Device, context, vertices, indicies, new List<Texture2D>() { texture });
        }

        public SphereObject(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context, Color color, int subdivisionCount = 30) : this(device, context, Texture2D.GetColoredTexture(device, color), subdivisionCount)
        {
            throw new NotImplementedException("ill make a sphere later, ok?");
        }

        public unsafe override void Render(Camera camera)
        {
            throw new NotImplementedException("ill make a sphere later, ok?");
            base.Render(camera);

            VertexShaderData.ViewProjection = Matrix4x4.Transpose(VertexShaderData.ViewProjection);
            VertexShaderData.World = Matrix4x4.Transpose(VertexShaderData.World);
            VertexShaderDataBuffer.WriteData(Context, ref VertexShaderData);

            Context.Value.VSSetConstantBuffers(0, 1, VertexShaderDataBuffer.DataBuffer);

            PlaneMesh.Render();
        }


        public override void RenderShadows()
        {
            throw new NotImplementedException("ill make a sphere later, ok?");
            base.RenderShadows();

            ShadowVertexShaderData.World = Matrix4x4.Transpose(ShadowVertexShaderData.World);
            ShadowVertexShaderDataBuffer.WriteData(Context, ref ShadowVertexShaderData);

            unsafe
            {
                Context.Value.VSSetConstantBuffers(0, 1, ShadowVertexShaderDataBuffer.DataBuffer);
            }

            PlaneMesh.Render();
        }

        public new void Dispose()
        {
            throw new NotImplementedException("ill make a sphere later, ok?");
            base.Dispose();
            PlaneMesh.Dispose();
        }
    }
}
