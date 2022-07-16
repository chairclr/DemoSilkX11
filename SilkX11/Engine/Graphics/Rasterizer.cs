using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace DemoSilkX11.Engine.Graphics
{
    public class Rasterizer
    {
        public Ref<ID3D11Device> Device = new();
        public Ref<ID3D11RasterizerState> RasterizerState = new();
        public RasterizerDesc Description;

        public unsafe Rasterizer(Ref<ID3D11Device> device, RasterizerDesc desc)
        {
            Device = device;
            Description = desc;


            SilkMarshal.ThrowHResult(Device.Value.CreateRasterizerState(ref Description, RasterizerState));
        }

        public unsafe void CreateState(RasterizerDesc desc)
        {
            Description = desc;
            SilkMarshal.ThrowHResult(Device.Value.CreateRasterizerState(ref Description, RasterizerState));
        }
    }
}
