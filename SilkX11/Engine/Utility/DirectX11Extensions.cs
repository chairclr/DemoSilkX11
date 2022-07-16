using Silk.NET;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System.Numerics;
using DemoSilkX11.Engine.Graphics;
using DemoSilkX11.Engine.Graphics.Windows;
using Color = DemoSilkX11.Utility.Color;

namespace DemoSilkX11.Utility
{
    public static unsafe class DirectX11Extensions
    {
        public static void ClearRenderTargetView(this ref ID3D11DeviceContext context, ID3D11RenderTargetView* pRenderTargetView, Color color)
        {
            Vector4 colorVector = color.ToVector4();
            context.ClearRenderTargetView(pRenderTargetView, &colorVector.X);
        }
    }
}
