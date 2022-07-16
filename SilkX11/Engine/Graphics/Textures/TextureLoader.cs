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
using DemoSilkX11.Utility;
using Color = DemoSilkX11.Utility.Color;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;

namespace DemoSilkX11.Engine.Graphics.Textures
{
    public class TextureLoader
    {
        public static unsafe Texture2D LoadTextureFromFile(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context, string file)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(file);

            Size size = image.Size();
            int width = size.Width;
            int height = size.Height;
            int stride = width * 4;
            byte[] imageDataBytes = new byte[stride * height];

            image.CopyPixelDataTo(imageDataBytes);

            fixed (void* pixelData = &imageDataBytes[0])
            {
                SubresourceData data = new SubresourceData()
                {
                    PSysMem = pixelData,
                    SysMemPitch = (uint)stride,
                };

                Texture2D texture = new Texture2D(device, width, height, new SampleDesc(1, 0), BindFlag.BindShaderResource, ref data, TextureType.None, usage: Usage.UsageDynamic, cpuAccessFlags: CpuAccessFlag.CpuAccessWrite);
                return texture;
            }

        }
        public static unsafe Texture2D LoadEngineTexture(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context, string name)
        {
            return LoadTextureFromFile(device, context, $@"Engine\Graphics\Textures\{name}");
        }
        public static unsafe Texture2D LoadTextureFromPixel(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context, int width, int height, Color color)
        {
            uint pv = color.PackedValue;
            SubresourceData data = new SubresourceData()
            {
                PSysMem = &pv,
                SysMemPitch = 4,
            };

            Texture2D texture = new Texture2D(device, width, height, new SampleDesc(1, 0), BindFlag.BindShaderResource, ref data, TextureType.None, usage: Usage.UsageDynamic, cpuAccessFlags: CpuAccessFlag.CpuAccessWrite);
            return texture;
        }

    }
}
