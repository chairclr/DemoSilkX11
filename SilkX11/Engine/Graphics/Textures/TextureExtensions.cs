using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using DemoSilkX11.Engine.Graphics;

namespace DemoSilkX11.Engine.Graphics.Textures
{
    public static class TextureExtensions
    {
        

        public static unsafe Ref<ID3D11ShaderResourceView> GetTextureShaderResource(this Texture2D texture)
        {
            if (texture.ShaderResourceView.Get() == null)
            {
                texture.ShaderResourceView = texture.CreateTextureShaderResource();
            }

            return texture.ShaderResourceView;
        }
        public static unsafe Ref<ID3D11ShaderResourceView> CreateTextureShaderResource(this Texture2D texture)
        {
            Texture2DDesc textureDesc = new Texture2DDesc();

            texture.TextureData.Value.GetDesc(ref textureDesc);

            Ref<ID3D11ShaderResourceView> resourceView = new();

            ShaderResourceViewDesc shaderResourceViewDesc = new ShaderResourceViewDesc()
            {
                Format = textureDesc.Format,
                ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture2D,
            };
            shaderResourceViewDesc.Texture2D.MipLevels = textureDesc.MipLevels;

            SilkMarshal.ThrowHResult(texture.Device.Value.CreateShaderResourceView(texture, ref shaderResourceViewDesc, resourceView));

            return resourceView;
        }

        public static unsafe Ref<ID3D11ShaderResourceView> GetDepthTextureShaderResource(this Texture2D texture)
        {
            if (texture.ShaderResourceView.Get() == null)
            {
                texture.ShaderResourceView = texture.CreateDepthTextureShaderResource();
            }

            return texture.ShaderResourceView;
        }
        public static unsafe Ref<ID3D11ShaderResourceView> CreateDepthTextureShaderResource(this Texture2D texture)
        {
            Texture2DDesc textureDesc = new Texture2DDesc();

            texture.TextureData.Value.GetDesc(ref textureDesc);

            Ref<ID3D11ShaderResourceView> resourceView = new();

            ShaderResourceViewDesc shaderResourceViewDesc = new ShaderResourceViewDesc()
            {
                Format = Format.FormatR32Float,
                ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture2D,
            };
            shaderResourceViewDesc.Texture2D.MipLevels = textureDesc.MipLevels;

            SilkMarshal.ThrowHResult(texture.Device.Value.CreateShaderResourceView(texture, ref shaderResourceViewDesc, resourceView));

            return resourceView;
        }

        public static unsafe Ref<ID3D11ShaderResourceView> GetDepthTextureShaderResourceMS(this Texture2D texture)
        {
            if (texture.ShaderResourceView.Get() == null)
            {
                texture.ShaderResourceView = texture.CreateDepthTextureShaderResourceMS();
            }

            return texture.ShaderResourceView;
        }
        public static unsafe Ref<ID3D11ShaderResourceView> CreateDepthTextureShaderResourceMS(this Texture2D texture)
        {
            Texture2DDesc textureDesc = new Texture2DDesc();

            texture.TextureData.Value.GetDesc(ref textureDesc);

            Ref<ID3D11ShaderResourceView> resourceView = new();

            ShaderResourceViewDesc shaderResourceViewDesc = new ShaderResourceViewDesc()
            {
                Format = Format.FormatR32Float,
                ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture2Dms,
            };
            shaderResourceViewDesc.Texture2D.MipLevels = textureDesc.MipLevels;

            SilkMarshal.ThrowHResult(texture.Device.Value.CreateShaderResourceView(texture, ref shaderResourceViewDesc, resourceView));

            return resourceView;
        }

        public static unsafe Ref<ID3D11ShaderResourceView> GetRenderTextureShaderResource(this Texture2D texture)
        {
            if (texture.ShaderResourceView.Get() == null)
            {
                texture.ShaderResourceView = texture.CreateRenderTextureShaderResource();
            }

            return texture.ShaderResourceView;
        }
        public static unsafe Ref<ID3D11ShaderResourceView> CreateRenderTextureShaderResource(this Texture2D texture)
        {
            Texture2DDesc textureDesc = new Texture2DDesc();

            texture.TextureData.Value.GetDesc(ref textureDesc);
            ShaderResourceViewDesc shaderResourceViewDesc = new ShaderResourceViewDesc()
            {
                Format = textureDesc.Format,
                ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture2D,
            };
            shaderResourceViewDesc.Texture2D.MipLevels = textureDesc.MipLevels;
            Ref<ID3D11ShaderResourceView> resourceView = new();

            if (textureDesc.SampleDesc.Count > 1)
            {
                throw new InvalidOperationException("You need to create a texture with only 1 sample. Try creating a copy of the texture with only 1 sample.");
            }
            else if (textureDesc.SampleDesc.Count == 1)
            {
                SilkMarshal.ThrowHResult(texture.Device.Value.CreateShaderResourceView(texture, ref shaderResourceViewDesc, resourceView));
            }

            return resourceView;
        }

        public static unsafe Texture2D CreateNonMSCopy(this Texture2D texture)
        {
            Texture2DDesc desc = texture.GetDescription();
            return new Texture2D(texture.Device, (int)desc.Width, (int)desc.Height, (BindFlag)desc.BindFlags, TextureType.None, desc.Format, desc.Usage, (CpuAccessFlag)desc.CPUAccessFlags, (ResourceMiscFlag)desc.MiscFlags, (int)desc.MipLevels, (int)desc.ArraySize);
        }
        public static unsafe Texture2D CreateNonMSCopy(this Texture2D texture, Format format)
        {
            Texture2DDesc desc = texture.GetDescription();
            return new Texture2D(texture.Device, (int)desc.Width, (int)desc.Height, (BindFlag)desc.BindFlags, TextureType.None, format, desc.Usage, (CpuAccessFlag)desc.CPUAccessFlags, (ResourceMiscFlag)desc.MiscFlags, (int)desc.MipLevels, (int)desc.ArraySize);
        }
        public static unsafe void ResolveBuffer(this Texture2D texture, Ref<ID3D11DeviceContext> context, Texture2D srTarget, Format format)
        {
            context.Value.ResolveSubresource(srTarget, 0, texture, 0, format);
        }


        public static unsafe Ref<ID3D11ShaderResourceView> GetDepthTextureCubeShaderResource(this Texture2D texture)
        {
            if (texture.ShaderResourceView.Get() == null)
            {
                texture.ShaderResourceView = texture.CreateDepthTextureCubeShaderResource();
            }

            return texture.ShaderResourceView;
        }
        public static unsafe Ref<ID3D11ShaderResourceView> CreateDepthTextureCubeShaderResource(this Texture2D texture)
        {
            Texture2DDesc textureDesc = new Texture2DDesc();

            texture.TextureData.Value.GetDesc(ref textureDesc);

            Ref<ID3D11ShaderResourceView> resourceView = new();

            ShaderResourceViewDesc shaderResourceViewDesc = new ShaderResourceViewDesc()
            {
                Format = Format.FormatR32Float,
                ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexturecube,
            };

            shaderResourceViewDesc.Texture2DArray.ArraySize = 6;
            shaderResourceViewDesc.Texture2DArray.FirstArraySlice = 0;

            shaderResourceViewDesc.Texture2D.MipLevels = textureDesc.MipLevels;

            SilkMarshal.ThrowHResult(texture.Device.Value.CreateShaderResourceView(texture, ref shaderResourceViewDesc, resourceView));

            return resourceView;
        }
    }
}
