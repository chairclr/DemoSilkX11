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

namespace DemoSilkX11.Engine.Graphics.Primitives
{
    public class CubeObject : RenderObject
    {
        public Mesh CubeMesh;

        public CubeObject(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context, Texture2D texture) : base(device, context)
        {
            List<Vertex> vertices = new List<Vertex>()
            {
                new Vertex(-0.5f, -0.5f,  0.5f, 0.0f, 0.0f, 0f, 0f, -1f),
                new Vertex( 0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0f, 0f, -1f),
                new Vertex(-0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 0f, 0f, -1f),
                new Vertex( 0.5f,  0.5f,  0.5f, 0.0f, 1.0f, 0f, 0f, -1f),
                new Vertex(-0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0f, 0f, -1f),
                new Vertex( 0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0f, 0f, -1f),
                new Vertex(-0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 0f, 0f, -1f),
                new Vertex( 0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 0f, 0f, -1f),
            };
            List<int> indicies = new List<int>()
            {
                //Top
                2, 6, 7,
                2, 3, 7,

                //Bottom
                0, 4, 5,
                0, 1, 5,

                //Left
                0, 2, 6,
                0, 4, 6,

                //Right
                1, 3, 7,
                1, 5, 7,

                //Front
                0, 2, 3,
                0, 1, 3,

                //Back
                4, 6, 7,
                4, 5, 7
            };

            CubeMesh = new Mesh(Device, context, vertices, indicies, new List<Texture2D>() { texture });
        }

        public CubeObject(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context, Color color) : this(device, context, Texture2D.GetColoredTexture(device, color))
        {
        }

        public unsafe override void Render(Camera camera)
        {
            base.Render(camera);

            VertexShaderData.ViewProjection = Matrix4x4.Transpose(VertexShaderData.ViewProjection);
            VertexShaderData.World = Matrix4x4.Transpose(VertexShaderData.World);
            VertexShaderDataBuffer.WriteData(Context, ref VertexShaderData);

            Context.Value.VSSetConstantBuffers(0, 1, VertexShaderDataBuffer.DataBuffer);

            CubeMesh.Render();
        }


        public override void RenderShadows()
        {
            base.RenderShadows();

            ShadowVertexShaderData.World = Matrix4x4.Transpose(ShadowVertexShaderData.World);
            ShadowVertexShaderDataBuffer.WriteData(Context, ref ShadowVertexShaderData);

            unsafe
            {
                Context.Value.VSSetConstantBuffers(0, 1, ShadowVertexShaderDataBuffer.DataBuffer);
            }

            CubeMesh.Render();
        }

        public new void Dispose()
        {
            base.Dispose();

            CubeMesh.Dispose();
        }
    }
}
