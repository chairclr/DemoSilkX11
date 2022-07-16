using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;
using Silk.NET.Assimp;
using Silk.NET.DXGI;
using DemoSilkX11.Utility;
using DemoSilkX11.Engine.Graphics.Textures;

namespace DemoSilkX11.Engine.Graphics
{
    public class Mesh : IDisposable
    {
        public Ref<ID3D11Device> Device;
        public Ref<ID3D11DeviceContext> Context;

        public Buffer<Vertex> VertexBuffer;
        public Buffer<int> IndexBuffer;

        public List<Texture2D> Textures;

        public Mesh(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context, List<Vertex> vertices, List<int> indices, List<Texture2D> textures)
        {
            Device = device;
            Context = context;

            Textures = textures;

            Vertex[] vData = vertices.ToArray();
            int[] iData = indices.ToArray();
            VertexBuffer = new Buffer<Vertex>(Device, ref vData, BindFlag.BindVertexBuffer);

            IndexBuffer = new Buffer<int>(Device, ref iData, BindFlag.BindIndexBuffer);
        }

        public unsafe void Render()
        {
            uint offset = 0;

            foreach (Texture2D tex in Textures)
            {
                if (tex.TextureType == DemoSilkX11.Engine.Graphics.Textures.TextureType.Diffuse)
                {
                    Context.Value.PSSetShaderResources(0, 1, tex.GetTextureShaderResource());
                    break;
                }
            }

            Context.Value.IASetVertexBuffers(0, 1, VertexBuffer.DataBuffer, ref VertexBuffer.DataStride, ref offset);
            Context.Value.IASetIndexBuffer(IndexBuffer.DataBuffer, Format.FormatR32Uint, 0);
            Context.Value.DrawIndexed(IndexBuffer.DataLength, 0, 0);
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
            foreach (Texture2D tx in Textures)
            {
                tx.Dispose();
            }
        }
    }
}
