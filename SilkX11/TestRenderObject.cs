using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using DemoSilkX11.Engine.Graphics;
using DemoSilkX11.Utility;
using DemoSilkX11.Engine.Graphics.Windows;
using ImGuiNET;
using Color = DemoSilkX11.Utility.Color;
using System.Runtime.InteropServices;
using DemoSilkX11.Engine;
using BepuPhysics;

namespace DemoSilkX11
{
    public class TestRenderObject : RenderModel
    {
        public TestRenderObject(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context) : base(device, context)
        {
        }

        public override string ModelPath => @"Models\cube.obj";

        public BodyHandle bodyHandle;

        public override void Update(float dt)
        {

        }
    }
}
