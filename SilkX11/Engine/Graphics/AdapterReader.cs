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
    public class AdapterReader
    {
        private static bool adaptersHaveBeenRead = false;
        private static List<GraphicsAdapter> adapterCache = new List<GraphicsAdapter>();

        public static unsafe IReadOnlyList<GraphicsAdapter> Read(DXGI graphicsInterface)
        {
            if (adaptersHaveBeenRead)
                return adapterCache;

            IDXGIFactory* adapterFactory = null;

            Guid factoryUUID = IDXGIFactory.Guid;

            SilkMarshal.ThrowHResult(graphicsInterface.CreateDXGIFactory(ref SilkMarshal.GuidOf<IDXGIFactory>(), (void**)&adapterFactory)); ;

            IDXGIAdapter* pAdapter = null;
            uint i = 0;

            while (HResult.IndicatesSuccess(adapterFactory->EnumAdapters(i, ref pAdapter)))
            {
                adapterCache.Add(new GraphicsAdapter(new Ref<IDXGIAdapter>(pAdapter)));
                i++;
            }

            return adapterCache;
        }
    }
}
