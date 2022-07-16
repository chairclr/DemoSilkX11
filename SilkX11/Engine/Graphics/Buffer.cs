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
using System.IO;
using System.Runtime.CompilerServices;

namespace DemoSilkX11.Engine.Graphics
{
    public unsafe class Buffer<T> : IDisposable 
        where T : unmanaged
    {
        public Ref<ID3D11Device> Device = new();
        public Ref<ID3D11Buffer> DataBuffer = new();

        /// <summary>
        /// Number of elements 
        /// </summary>
        public uint DataLength;

        /// <summary>
        /// Size of buffer data in bytes
        /// </summary>
        public uint DataSize;

        /// <summary>
        /// The size of a single element in the buffer
        /// </summary>
        public uint DataStride;

        public Buffer(Ref<ID3D11Device> device, ref T[] data, BindFlag binding, Usage usage = Usage.UsageDefault, CpuAccessFlag cpuAccessFlags = 0, ResourceMiscFlag resourceMiscFlags = 0)
        {
            DataLength = (uint)data.Length;
            DataStride = (uint)sizeof(T);
            DataSize = DataLength * DataStride;
            Device = device;

            BufferDesc bufferDesc = new BufferDesc()
            {
                Usage = usage,
                ByteWidth = DataSize,
                BindFlags = (uint)binding,
                CPUAccessFlags = (uint)cpuAccessFlags,
                MiscFlags = (uint)resourceMiscFlags,
            };

            fixed (void* bufferData = &data[0])
            {
                SubresourceData bufferSubresource = new SubresourceData(bufferData);

                SilkMarshal.ThrowHResult(Device.Value.CreateBuffer(ref bufferDesc, ref bufferSubresource, DataBuffer));
            }
        }
        public Buffer(Ref<ID3D11Device> device, ref T data, BindFlag binding, Usage usage = Usage.UsageDefault, CpuAccessFlag cpuAccessFlags = 0, ResourceMiscFlag resourceMiscFlags = 0)
        {
            DataLength = 1;
            DataStride = (uint)sizeof(T);
            DataSize = DataLength * DataStride;
            Device = device;

            BufferDesc bufferDesc = new BufferDesc()
            {
                Usage = usage,
                ByteWidth = DataSize,
                BindFlags = (uint)binding,
                CPUAccessFlags = (uint)cpuAccessFlags,
                MiscFlags = (uint)resourceMiscFlags,
            };

            fixed (void* bufferData = &data)
            {
                SubresourceData bufferSubresource = new SubresourceData(bufferData);

                SilkMarshal.ThrowHResult(Device.Value.CreateBuffer(ref bufferDesc, ref bufferSubresource, DataBuffer));
            }
        }
        public Buffer(Ref<ID3D11Device> device, BindFlag binding, Usage usage = Usage.UsageDefault, CpuAccessFlag cpuAccessFlags = 0, ResourceMiscFlag resourceMiscFlags = 0)
        {
            DataLength = 1;
            DataStride = (uint)sizeof(T);
            DataSize = DataLength * DataStride;
            Device = device;

            BufferDesc bufferDesc = new BufferDesc()
            {
                Usage = usage,
                ByteWidth = DataSize,
                BindFlags = (uint)binding,
                CPUAccessFlags = (uint)cpuAccessFlags,
                MiscFlags = (uint)resourceMiscFlags,
                StructureByteStride = 0,
            };

            SilkMarshal.ThrowHResult(Device.Value.CreateBuffer(ref bufferDesc, null, DataBuffer));
        }
        public Buffer(Ref<ID3D11Device> device, BindFlag binding, uint byteWidth, Usage usage = Usage.UsageDefault, CpuAccessFlag cpuAccessFlags = 0, ResourceMiscFlag resourceMiscFlags = 0)
        {
            DataLength = 1;
            DataStride = (uint)sizeof(T);
            DataSize = byteWidth;
            Device = device;

            BufferDesc bufferDesc = new BufferDesc()
            {
                Usage = usage,
                ByteWidth = byteWidth,
                BindFlags = (uint)binding,
                CPUAccessFlags = (uint)cpuAccessFlags,
                MiscFlags = (uint)resourceMiscFlags,
                StructureByteStride = 0,
            };

            SilkMarshal.ThrowHResult(Device.Value.CreateBuffer(ref bufferDesc, null, DataBuffer));
        }
        public Buffer(Ref<ID3D11Device> device, BufferDesc desc)
        {
            DataLength = 1;
            DataStride = (uint)sizeof(T);
            DataSize = desc.ByteWidth;
            Device = device;

            SilkMarshal.ThrowHResult(Device.Value.CreateBuffer(ref desc, null, DataBuffer));
        }

        public void WriteData(Ref<ID3D11DeviceContext> context, ref T data)
        {
            MappedSubresource mappedSubresource = new MappedSubresource();
            context.Value.Map((ID3D11Resource*)DataBuffer.Get(), 0, Map.MapWriteDiscard, 0, ref mappedSubresource);

            Unsafe.Copy(mappedSubresource.PData, ref data);

            context.Value.Unmap(DataBuffer.Get<ID3D11Resource>(), 0);
        }
        public void WriteData(Ref<ID3D11DeviceContext> context, ref T data, uint subresource, Map mapType, MapFlag mapFlags)
        {
            MappedSubresource mappedSubresource = new MappedSubresource();
            context.Value.Map(DataBuffer.Get<ID3D11Resource>(), subresource, mapType, (uint)mapFlags, ref mappedSubresource);

            Unsafe.Copy(mappedSubresource.PData, ref data);

            context.Value.Unmap(DataBuffer.Get<ID3D11Resource>(), 0);
        }

        public void WriteData(Ref<ID3D11DeviceContext> context, ref T[] data)
        {
            MappedSubresource mappedSubresource = new MappedSubresource();
            context.Value.Map(DataBuffer.Get<ID3D11Resource>(), 0, Map.MapWriteDiscard, 0, ref mappedSubresource);

            Unsafe.Copy(mappedSubresource.PData, ref data);

            context.Value.Unmap(DataBuffer.Get<ID3D11Resource>(), 0);
        }
        public void WriteData(Ref<ID3D11DeviceContext> context, ref T[] data, uint subresource, Map mapType, MapFlag mapFlags)
        {
            MappedSubresource mappedSubresource = new MappedSubresource();
            context.Value.Map(DataBuffer.Get<ID3D11Resource>(), subresource, mapType, (uint)mapFlags, ref mappedSubresource);

            Unsafe.Copy(mappedSubresource.PData, ref data);

            context.Value.Unmap(DataBuffer.Get<ID3D11Resource>(), 0);
        }

        public void Dispose()
        {
            DataBuffer.Value.Release();
        }
    }
}
