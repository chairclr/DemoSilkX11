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
    public abstract class RenderModel : RenderObject
    {
        public Model Model;

        public RenderModel(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context) : base(device, context)
        {
            Model = new Model(device, context);
        }

        public abstract string ModelPath { get; }

        public void InitModel()
        {
            Model = new Model(Device, Context);
            Model.LoadFromFile(ModelPath);
        }

        public override void Start()
        {
            InitModel();
        }

        public override void Render(Camera camera)
        {
            base.Render(camera);

            VertexShaderData.ViewProjection = Matrix4x4.Transpose(VertexShaderData.ViewProjection);
            VertexShaderData.World = Matrix4x4.Transpose(VertexShaderData.World);
            VertexShaderDataBuffer.WriteData(Context, ref VertexShaderData);

            unsafe
            {
                Context.Value.VSSetConstantBuffers(0, 1, VertexShaderDataBuffer.DataBuffer);
            }

            Model.Render();
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

            Model.Render();
        }

        public override void Dispose()
        {
            base.Dispose();

            Model.Dispose();
        }
    }
}
