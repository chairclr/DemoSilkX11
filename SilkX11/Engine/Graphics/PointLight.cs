using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using DemoSilkX11.Utility;
using DemoSilkX11.Engine.Graphics.Windows;
using ImGuiNET;
using Color = DemoSilkX11.Utility.Color;
using System.Runtime.InteropServices;
using DemoSilkX11.Engine.Primitives;
using DemoSilkX11.Engine.Graphics.Textures;

namespace DemoSilkX11.Engine.Graphics
{
    public class PointLight
    {
        public Matrix4x4 ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, 1f, 0.1f, 250f);
        public Matrix4x4[] GetViewMatrices()
        {
            Matrix4x4[] mats =
            {
                 // right
                Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + Vector3.UnitX, Vector3.UnitY),

                // left
                Matrix4x4.CreateLookAt(Transform.Position, Transform.Position - Vector3.UnitX, Vector3.UnitY),

                // up
                Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + Vector3.UnitY, Vector3.UnitZ),

                // down
                Matrix4x4.CreateLookAt(Transform.Position, Transform.Position - Vector3.UnitY, -Vector3.UnitZ),

                // forwards
                Matrix4x4.CreateLookAt(Transform.Position, Transform.Position - Vector3.UnitZ, Vector3.UnitY),

                // backwards
                Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + Vector3.UnitZ, Vector3.UnitY),
            };

            return mats;
        }

        public const int ShadowMapSize = 2048;
        public Texture2D ShadowDepthBuffer;
        public Ref<ID3D11DepthStencilView> ShadowDepthStencilView = new();
        public Ref<ID3D11DepthStencilState> ShadowDepthStencilState = new();
        public Ref<ID3D11Device> Device;
        public Ref<ID3D11DeviceContext> Context;


        public Viewport ShadowViewport;


        public Transform Transform;
        public float LightStrength;
        public Vector3 LightColor;
        public Vector3 Attenuation;

        private unsafe void InitShadows()
        {
            ShadowDepthBuffer = new Texture2D(Device, ShadowMapSize, ShadowMapSize, new SampleDesc(1, 0), BindFlag.BindDepthStencil | BindFlag.BindShaderResource, TextureType.None, Format.FormatR32Typeless, arraySize: 6, miscFlags: ResourceMiscFlag.ResourceMiscTexturecube);


            DepthStencilViewDesc viewDesc = new DepthStencilViewDesc()
            {
                Format = Format.FormatD32Float,
                ViewDimension = DsvDimension.DsvDimensionTexture2Darray,
            };

            viewDesc.Texture2DArray.ArraySize = 6;
            viewDesc.Texture2DArray.FirstArraySlice = 0;
            viewDesc.Texture2DArray.MipSlice = 0;

            SilkMarshal.ThrowHResult(Device.Value.CreateDepthStencilView(ShadowDepthBuffer, ref viewDesc, ShadowDepthStencilView));

            DepthStencilDesc depthstencildesc = new DepthStencilDesc()
            {
                DepthEnable = 1,
                DepthWriteMask = DepthWriteMask.DepthWriteMaskAll,
                DepthFunc = ComparisonFunc.ComparisonLessEqual,
            };

            SilkMarshal.ThrowHResult(Device.Value.CreateDepthStencilState(ref depthstencildesc, ShadowDepthStencilState));

            Viewport viewport = new Viewport()
            {
                TopLeftX = 0,
                TopLeftY = 0,
                Width = ShadowMapSize,
                Height = ShadowMapSize,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };

            ShadowViewport = viewport;
        }
        public unsafe void RenderShadows()
        {
            Context.Value.ClearDepthStencilView(ShadowDepthStencilView, (uint)(ClearFlag.ClearDepth | ClearFlag.ClearStencil), 1.0f, 0);
            Context.Value.OMSetRenderTargets(0, null, ShadowDepthStencilView);
            Context.Value.OMSetDepthStencilState(ShadowDepthStencilState, 0);
            Context.Value.RSSetViewports(1, ref ShadowViewport);
        }

        public PointLight(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context)
        {
            Transform = new Transform();

            LightStrength = 1.0f;
            LightColor = Color.White.ToVector3();
            Attenuation = new Vector3(1.0f, 0.1f, 0.035f);

            Device = device;
            Context = context;

            InitShadows();
        }
    }
}
