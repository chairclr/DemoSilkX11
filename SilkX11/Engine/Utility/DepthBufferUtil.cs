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
using DemoSilkX11.Engine.Graphics.Textures;

namespace DemoSilkX11.Utility
{
    public class DepthBufferUtil
    {
		public static unsafe Texture2D ReadBuffer(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context, Ref<ID3D11DepthStencilView> view, Texture2D buffer)
		{
			Texture2D tex = new Texture2D(device, TextureType.None);

			Texture2DDesc textureDesc = new Texture2DDesc();

			buffer.TextureData.Value.GetDesc(ref textureDesc);

			textureDesc.Format = Format.FormatR32Float;

			SilkMarshal.ThrowHResult(device.Value.CreateTexture2D(ref textureDesc, null, tex.TextureData));

			Ref<ID3D11Resource> resSource = new();
			view.Value.GetResource(resSource);

			context.Value.CopyResource(tex, buffer.TextureData.Get<ID3D11Resource>());

			return tex;
        }
    }
}
