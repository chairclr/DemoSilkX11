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

namespace DemoSilkX11.Engine.Graphics.Textures
{
    public unsafe class Texture2D : IDisposable
    {
        public static ref Guid Guid => ref SilkMarshal.GuidOf<ID3D11Texture2D>();

        public Ref<ID3D11Device> Device = new();
        public Ref<ID3D11ShaderResourceView> ShaderResourceView = new();
        protected TextureType textureType;

        public Ref<ID3D11Texture2D> TextureData = new();
        public TextureType TextureType  { get => textureType; set => textureType = value; }


        public static implicit operator ID3D11Texture2D*(Texture2D val)
        {
            return val.TextureData;
        }

        public static implicit operator ID3D11Resource*(Texture2D val)
        {
            return (ID3D11Resource*)(ID3D11Texture2D*)val.TextureData;
        }

        public static implicit operator void**(Texture2D val)
        {
            return val.TextureData;
        }

        public Texture2D(Ref<ID3D11Device> device, int width, int height, SampleDesc sampleDesc, BindFlag bindFlags, ref SubresourceData data,TextureType type, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.UsageDefault, CpuAccessFlag cpuAccessFlags = 0, ResourceMiscFlag miscFlags = 0, int mipLevels = 1, int arraySize = 1)
        {
            this.Device = device;
            Texture2DDesc textureDesc = new Texture2DDesc()
            {
                Width = (uint)width,
                Height = (uint)height,
                SampleDesc = sampleDesc,
                MipLevels = (uint)mipLevels,
                ArraySize = (uint)arraySize,
                Format = format,
                BindFlags = (uint)bindFlags,
                Usage = usage,
                CPUAccessFlags = (uint)cpuAccessFlags,
                MiscFlags = (uint)miscFlags,
            };

            this.textureType = type;
            SilkMarshal.ThrowHResult(device.Value.CreateTexture2D(ref textureDesc, ref data, TextureData));
        }
        public Texture2D(Ref<ID3D11Device> device, int width, int height, SampleDesc sampleDesc, BindFlag bindFlags,TextureType type, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.UsageDefault, CpuAccessFlag cpuAccessFlags = 0, ResourceMiscFlag miscFlags = 0, int mipLevels = 1, int arraySize = 1)
        {
            this.Device = device;
            Texture2DDesc textureDesc = new Texture2DDesc()
            {
                Width = (uint)width,
                Height = (uint)height,
                SampleDesc = sampleDesc,
                MipLevels = (uint)mipLevels,
                ArraySize = (uint)arraySize,
                Format = format,
                BindFlags = (uint)bindFlags,
                Usage = usage,
                CPUAccessFlags = (uint)cpuAccessFlags,
                MiscFlags = (uint)miscFlags,
            };

            this.textureType = type;
            SilkMarshal.ThrowHResult(device.Value.CreateTexture2D(ref textureDesc, null, TextureData));
        }
        public Texture2D(Ref<ID3D11Device> device, int width, int height, BindFlag bindFlags, TextureType type, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.UsageDefault, CpuAccessFlag cpuAccessFlags = 0, ResourceMiscFlag miscFlags = 0, int mipLevels = 1, int arraySize = 1) : this(device, width, height, new SampleDesc(1, 0), bindFlags, type, format, usage, cpuAccessFlags, miscFlags, mipLevels, arraySize)
        {

        }
        public Texture2D(Ref<ID3D11Device> device, TextureType type)
        {
            this.Device = device;
            this.textureType = type;
        }
        public Texture2D(Ref<ID3D11Device> device, TextureType type, Texture2DDesc textureDesc)
        {
            this.Device = device;
            this.textureType = type;
            SilkMarshal.ThrowHResult(device.Value.CreateTexture2D(ref textureDesc, null, TextureData));
        }

        public static Texture2D GetBlankTexture(Ref<ID3D11Device> device) => GetColoredTexture(device, Color.White);
        public static Texture2D GetColoredTexture(Ref<ID3D11Device> device, Color color)
        {
            SubresourceData data = new SubresourceData();
            data.PSysMem = &color;
            data.SysMemPitch = (uint)sizeof(Color);
            return new Texture2D(device, 1, 1, new SampleDesc(1, 0), BindFlag.BindShaderResource, ref data, TextureType.Diffuse);
        }

        public Texture2DDesc GetDescription()
        {
            Texture2DDesc textureDesc = new Texture2DDesc();
            TextureData.Value.GetDesc(ref textureDesc);
            return textureDesc;
        }

        public T[,] GetData<T>(Ref<ID3D11DeviceContext> Context, Format format)
            where T : unmanaged
        {
            Ref<ID3D11Texture2D> staging = new();
            Texture2DDesc desc = GetDescription();

            T[,] outputData = new T[desc.Width, desc.Height];

            desc.Format = format;
            desc.BindFlags = 0;
            desc.Usage = Usage.UsageStaging;
            desc.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessRead;

            SilkMarshal.ThrowHResult(Device.Value.CreateTexture2D(ref desc, null, staging));

            Context.Value.CopyResource(staging.Get<ID3D11Resource>(), this);

            MappedSubresource data = new MappedSubresource();

            SilkMarshal.ThrowHResult(Context.Value.Map(staging.Get<ID3D11Resource>(), 0, Map.MapRead, 0, ref data));

            unsafe
            {
                if (data.PData != null)
                {
                    T* textureData = (T*)data.PData;
                    for (int x = 0; x < desc.Width; x++)
                    {
                        for (int y = 0; y < desc.Height; y++)
                        {
                            outputData[x, y] = textureData[x + y * desc.Width];
                        }
                    }
                }
            }

            Context.Value.Unmap(staging.Get<ID3D11Resource>(), 0);

            return outputData;
        }
        public T[,] GetData<T>(Ref<ID3D11DeviceContext> Context, Format format, Func<T, T> transformation)
            where T : unmanaged
        {
            Ref<ID3D11Texture2D> staging = new();
            Texture2DDesc desc = GetDescription();

            T[,] outputData = new T[desc.Width, desc.Height];

            desc.Format = format;
            desc.BindFlags = 0;
            desc.Usage = Usage.UsageStaging;
            desc.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessRead;

            SilkMarshal.ThrowHResult(Device.Value.CreateTexture2D(ref desc, null, staging));

            Context.Value.CopyResource(staging.Get<ID3D11Resource>(), this);

            MappedSubresource data = new MappedSubresource();

            SilkMarshal.ThrowHResult(Context.Value.Map(staging.Get<ID3D11Resource>(), 0, Map.MapRead, 0, ref data));

            unsafe
            {
                if (data.PData != null)
                {
                    T* textureData = (T*)data.PData;
                    for (int x = 0; x < desc.Width; x++)
                    {
                        for (int y = 0; y < desc.Height; y++)
                        {
                            outputData[x, y] = transformation(textureData[x + y * desc.Width]);
                        }
                    }
                }
            }

            Context.Value.Unmap(staging.Get<ID3D11Resource>(), 0);

            return outputData;
        }

        public T[,] GetData<T>(Ref<ID3D11DeviceContext> Context, Format format, int subresource)
            where T : unmanaged
        {
            Ref<ID3D11Texture2D> staging = new();
            Texture2DDesc desc = GetDescription();

            T[,] outputData = new T[desc.Width, desc.Height];

            desc.Format = format;
            desc.BindFlags = 0;
            desc.Usage = Usage.UsageStaging;
            desc.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessRead;

            SilkMarshal.ThrowHResult(Device.Value.CreateTexture2D(ref desc, null, staging));

            Context.Value.CopyResource(staging.Get<ID3D11Resource>(), this);

            MappedSubresource data = new MappedSubresource();

            SilkMarshal.ThrowHResult(Context.Value.Map(staging.Get<ID3D11Resource>(), (uint)subresource, Map.MapRead, 0, ref data));

            unsafe
            {
                if (data.PData != null)
                {
                    T* textureData = (T*)data.PData;
                    for (int x = 0; x < desc.Width; x++)
                    {
                        for (int y = 0; y < desc.Height; y++)
                        {
                            outputData[x, y] = textureData[x + y * desc.Width];
                        }
                    }
                }
            }

            Context.Value.Unmap(staging.Get<ID3D11Resource>(), 0);

            return outputData;
        }
        public T[,] GetData<T>(Ref<ID3D11DeviceContext> Context, Format format, int subresource, Func<T, T> transformation)
            where T : unmanaged
        {
            Ref<ID3D11Texture2D> staging = new();
            Texture2DDesc desc = GetDescription();

            T[,] outputData = new T[desc.Width, desc.Height];

            desc.Format = format;
            desc.BindFlags = 0;
            desc.Usage = Usage.UsageStaging;
            desc.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessRead;

            SilkMarshal.ThrowHResult(Device.Value.CreateTexture2D(ref desc, null, staging));

            Context.Value.CopyResource(staging.Get<ID3D11Resource>(), this);

            MappedSubresource data = new MappedSubresource();

            SilkMarshal.ThrowHResult(Context.Value.Map(staging.Get<ID3D11Resource>(), (uint)subresource, Map.MapRead, 0, ref data));

            unsafe
            {
                if (data.PData != null)
                {
                    T* textureData = (T*)data.PData;
                    for (int x = 0; x < desc.Width; x++)
                    {
                        for (int y = 0; y < desc.Height; y++)
                        {
                            outputData[x, y] = transformation(textureData[x + y * desc.Width]);
                        }
                    }
                }
            }

            Context.Value.Unmap(staging.Get<ID3D11Resource>(), 0);

            return outputData;
        }

        public void Dispose()
        {
            TextureData.Value.Release();
            if (ShaderResourceView.Get() != null)
                ShaderResourceView.Value.Release();
        }
    }
}
