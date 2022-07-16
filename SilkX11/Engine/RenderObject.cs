using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;
using DemoSilkX11.Engine.Graphics;
using System.Numerics;

namespace DemoSilkX11.Engine
{
    public abstract class RenderObject : IDisposable
    {
        public Transform Transform;
        public Ref<ID3D11Device> Device;
        public Ref<ID3D11DeviceContext> Context;

        public VertexShaderBuffer VertexShaderData;
        public Buffer<VertexShaderBuffer> VertexShaderDataBuffer;

        public ShadowVertexShaderBuffer ShadowVertexShaderData;
        public Buffer<ShadowVertexShaderBuffer> ShadowVertexShaderDataBuffer;

        public RenderObject(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context)
        {
            Device = device;
            Context = context;

            Transform = new Transform();

            unsafe
            {
                uint vertexSize = (uint)(sizeof(VertexShaderBuffer) + (16 - sizeof(VertexShaderBuffer) % 16));
                VertexShaderDataBuffer = new Buffer<VertexShaderBuffer>(Device, BindFlag.BindConstantBuffer, vertexSize, Usage.UsageDynamic, CpuAccessFlag.CpuAccessWrite);


                uint shadowVertexSize = (uint)(sizeof(ShadowVertexShaderBuffer) + (16 - sizeof(ShadowVertexShaderBuffer) % 16));
                ShadowVertexShaderDataBuffer = new Buffer<ShadowVertexShaderBuffer>(Device, BindFlag.BindConstantBuffer, shadowVertexSize, Usage.UsageDynamic, CpuAccessFlag.CpuAccessWrite);
            }
        }

        public virtual void Start()
        {

        }

        public virtual void Update(float dt)
        {

        }

        public virtual void Render(Camera camera)
        {
            VertexShaderData.World = Transform.WorldMatrix;
            VertexShaderData.ViewProjection = camera.ViewMatrix * camera.ProjectionMatrix;
        }

        public virtual void RenderShadows()
        {
            ShadowVertexShaderData.World = Transform.WorldMatrix;
        }

        public virtual void Dispose()
        {
            VertexShaderDataBuffer.Dispose();
            ShadowVertexShaderDataBuffer.Dispose();
        }
    }
}
