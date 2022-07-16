using Silk.NET;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System.Numerics;

namespace DemoSilkX11.Engine.Graphics
{
    public unsafe class GraphicsAdapter
    {
        public Ref<IDXGIAdapter> Adapter;
        private AdapterDesc desc;

        public ref AdapterDesc Description => ref desc;

        public GraphicsAdapter(Ref<IDXGIAdapter> adapter)
        {
            Adapter = adapter;

            SilkMarshal.ThrowHResult(Adapter.Value.GetDesc(ref desc));
        }
    }
}
