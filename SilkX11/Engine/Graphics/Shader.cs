using Silk.NET;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System.Numerics;
using DemoSilkX11.Engine.Graphics;
using DemoSilkX11.Engine.Graphics.Windows;
using DemoSilkX11.Utility;
using Color = DemoSilkX11.Utility.Color;
using System.IO;

namespace DemoSilkX11.Engine.Graphics
{
    public unsafe class Shader : IDisposable
    {
        private static D3DCompiler Compiler = D3DCompiler.GetApi();

        public Ref<ID3D10Blob> ShaderData = new();

        public Shader()
        {
        }

        public const uint D3D10_SHADER_DEBUG = (1 << 0);
        public const uint D3D10_SHADER_SKIP_VALIDATION = (1 << 1);
        public const uint D3D10_SHADER_SKIP_OPTIMIZATION = (1 << 2);
        public const uint D3D10_SHADER_PACK_MATRIX_ROW_MAJOR = (1 << 3);
        public const uint D3D10_SHADER_PACK_MATRIX_COLUMN_MAJOR = (1 << 4);
        public const uint D3D10_SHADER_PARTIAL_PRECISION = (1 << 5);
        public const uint D3D10_SHADER_FORCE_VS_SOFTWARE_NO_OPT = (1 << 6);
        public const uint D3D10_SHADER_FORCE_PS_SOFTWARE_NO_OPT = (1 << 7);
        public const uint D3D10_SHADER_NO_PRESHADER = (1 << 8);
        public const uint D3D10_SHADER_AVOID_FLOW_CONTROL = (1 << 9);
        public const uint D3D10_SHADER_PREFER_FLOW_CONTROL = (1 << 10);
        public const uint D3D10_SHADER_ENABLE_STRICTNESS = (1 << 11);
        public const uint D3D10_SHADER_ENABLE_BACKWARDS_COMPATIBILITY = (1 << 12);
        public const uint D3D10_SHADER_IEEE_STRICTNESS = (1 << 13);
        public const uint D3D10_SHADER_WARNINGS_ARE_ERRORS = (1 << 18);
        public const uint D3D10_SHADER_RESOURCES_MAY_ALIAS = (1 << 19);
        public const uint D3D10_ENABLE_UNBOUNDED_DESCRIPTOR_TABLES = (1 << 20);
        public const uint D3D10_ALL_RESOURCES_BOUND = (1 << 21);
        public const uint D3D10_SHADER_DEBUG_NAME_FOR_SOURCE = (1 << 22);
        public const uint D3D10_SHADER_DEBUG_NAME_FOR_BINARY = (1 << 23);
        public const uint D3D10_SHADER_OPTIMIZATION_LEVEL0 = (1 << 14);
        public const uint D3D10_SHADER_OPTIMIZATION_LEVEL1 = 0;
        public const uint D3D10_SHADER_OPTIMIZATION_LEVEL2 = ((1 << 14) | (1 << 15));
        public const uint D3D10_SHADER_OPTIMIZATION_LEVEL3 = (1 << 15);
        public const uint D3D10_SHADER_FLAGS2_FORCE_ROOT_SIGNATURE_LATEST = 0;
        public const uint D3D10_SHADER_FLAGS2_FORCE_ROOT_SIGNATURE_1_0 = (1 << 4);
        public const uint D3D10_SHADER_FLAGS2_FORCE_ROOT_SIGNATURE_1_1 = (1 << 5);

        public static Shader CompileFromSourceCode(string src, string entryPoint, string shaderModel)
        {
            Shader shader = new Shader();
            Ref<ID3D10Blob> shaderErrors = new();

            uint flags = 0;

#if DEBUG
            flags |= D3D10_SHADER_DEBUG | D3D10_SHADER_SKIP_OPTIMIZATION;
#endif

            int hr = Compiler.Compile((void*)SilkMarshal.StringToPtr(src), (nuint)src.Length, (string)null, null, null, entryPoint, shaderModel, flags, 0, shader.ShaderData, shaderErrors);

            if (HResult.IndicatesFailure(hr))
            {
                if (shaderErrors.Get() != null)
                {
                    throw new InvalidOperationException($"Failed to compile shader. {System.Text.Encoding.UTF8.GetString((byte*)shaderErrors.Value.GetBufferPointer(), (int)shaderErrors.Value.GetBufferSize())}");
                }
                else
                {
                    SilkMarshal.ThrowHResult(hr);
                }
            }

            return shader;
        }
        public static Shader CompileFromSourceCode(string src, string? sourceName, string entryPoint, string shaderModel)
        {
            Shader shader = new Shader();
            Ref<ID3D10Blob> shaderErrors = new();

            uint flags = 0;

#if DEBUG
            flags |= D3D10_SHADER_DEBUG | D3D10_SHADER_SKIP_OPTIMIZATION;
#endif
            string? sourcePath = sourceName == null ? null : Path.GetFullPath(sourceName);
            int hr = Compiler.Compile((void*)SilkMarshal.StringToPtr(src), (nuint)src.Length, sourcePath, null, (ID3DInclude*)((IntPtr)1), entryPoint, shaderModel, flags, 0, shader.ShaderData, shaderErrors);

            if (HResult.IndicatesFailure(hr))
            {
                if (shaderErrors.Get() != null)
                {
                    throw new InvalidOperationException($"Failed to compile shader. {System.Text.Encoding.UTF8.GetString((byte*)shaderErrors.Value.GetBufferPointer(), (int)shaderErrors.Value.GetBufferSize())}");
                }
                else
                {
                    SilkMarshal.ThrowHResult(hr);
                }
            }

            return shader;
        }
        public static Shader CompileFromFile(string path, string entryPoint, string shaderModel)
        {
            Shader shader = new Shader();
            Ref<ID3D10Blob> shaderErrors = new();

            uint flags = 0;

#if DEBUG
            flags |= D3D10_SHADER_DEBUG | D3D10_SHADER_SKIP_OPTIMIZATION;
#endif

            string src = File.ReadAllText(path);

            int hr = Compiler.Compile((void*)SilkMarshal.StringToPtr(src), (nuint)src.Length, Path.GetFullPath(path), null, (ID3DInclude*)((IntPtr)1), entryPoint, shaderModel, flags, 0, shader.ShaderData, shaderErrors);

            if (HResult.IndicatesFailure(hr))
            {
                if (shaderErrors.Get() != null)
                {
                    throw new InvalidOperationException($"Failed to compile shader. {System.Text.Encoding.UTF8.GetString((byte*)shaderErrors.Value.GetBufferPointer(), (int)shaderErrors.Value.GetBufferSize())}");
                }
                else
                {
                    SilkMarshal.ThrowHResult(hr);
                }
            }

            return shader;
        }

        public void Dispose()
        {
            ShaderData.Value.Release();
        }
    }
    public class ShaderFileData
    {
        public string Source;
        public string SourceName;

        public ShaderFileData(string source, string sourceName)
        {
            Source = source;
            SourceName = sourceName;
        }
    }

    public unsafe class VertexShader : Shader
    {
        public Ref<ID3D11VertexShader> VertexShaderData = new();
        public Ref<ID3D11InputLayout> InputLayoutData = new();

        public VertexShader(ID3D11Device* device, InputElementDesc[] inputLayout, string path, string entryPoint = "VSMain", string shaderModel = "vs_5_0")
        {
            Shader s = CompileFromFile(path, entryPoint, shaderModel);
            this.ShaderData = s.ShaderData;

            SilkMarshal.ThrowHResult(device->CreateVertexShader(ShaderData.Value.GetBufferPointer(), ShaderData.Value.GetBufferSize(), null, VertexShaderData));

            fixed (InputElementDesc* layoutPtr = &inputLayout[0])
            {
                SilkMarshal.ThrowHResult(device->CreateInputLayout(layoutPtr, (uint)inputLayout.Length, ShaderData.Value.GetBufferPointer(), ShaderData.Value.GetBufferSize(), InputLayoutData));
            }
        }
        public VertexShader(ID3D11Device* device, InputElementDesc[] inputLayout, string src, string? sourceName, string entryPoint = "VSMain", string shaderModel = "vs_5_0")
        {
            Shader s = CompileFromSourceCode(src, sourceName, entryPoint, shaderModel);
            this.ShaderData = s.ShaderData;

            SilkMarshal.ThrowHResult(device->CreateVertexShader(ShaderData.Value.GetBufferPointer(), ShaderData.Value.GetBufferSize(), null, VertexShaderData));

            fixed (InputElementDesc* layoutPtr = &inputLayout[0])
            {
                SilkMarshal.ThrowHResult(device->CreateInputLayout(layoutPtr, (uint)inputLayout.Length, ShaderData.Value.GetBufferPointer(), ShaderData.Value.GetBufferSize(), InputLayoutData));
            }
        }
        public VertexShader(ID3D11Device* device, InputElementDesc[] inputLayout, ShaderFileData fileData, string entryPoint = "VSMain", string shaderModel = "vs_5_0") : this(device, inputLayout, fileData.Source, fileData.SourceName, entryPoint, shaderModel)
        {

        }
    }
    public unsafe class PixelShader : Shader
    {
        public Ref<ID3D11PixelShader> PixelShaderData = new();

        public PixelShader(ID3D11Device* device, string path, string entryPoint = "PSMain", string shaderModel = "ps_5_0")
        {
            Shader s = CompileFromFile(path, entryPoint, shaderModel);
            this.ShaderData = s.ShaderData;

            SilkMarshal.ThrowHResult(device->CreatePixelShader(ShaderData.Value.GetBufferPointer(), ShaderData.Value.GetBufferSize(), null, PixelShaderData));
        }
        public PixelShader(ID3D11Device* device, string src, string? soruceName, string entryPoint = "PSMain", string shaderModel = "ps_5_0")
        {
            Shader s = CompileFromSourceCode(src, soruceName, entryPoint, shaderModel);
            this.ShaderData = s.ShaderData;

            SilkMarshal.ThrowHResult(device->CreatePixelShader(ShaderData.Value.GetBufferPointer(), ShaderData.Value.GetBufferSize(), null, PixelShaderData));
        }
        public PixelShader(ID3D11Device* device, ShaderFileData fileData, string entryPoint = "PSMain", string shaderModel = "ps_5_0") : this(device, fileData.Source, fileData.SourceName, entryPoint, shaderModel)
        {

        }
    }
    public unsafe class GeometryShader : Shader
    {
        public Ref<ID3D11GeometryShader> GeometryShaderData = new();

        public GeometryShader(ID3D11Device* device, string path, string entryPoint = "GSMain", string shaderModel = "gs_5_0")
        {
            Shader s = CompileFromFile(path, entryPoint, shaderModel);
            this.ShaderData = s.ShaderData;

            SilkMarshal.ThrowHResult(device->CreateGeometryShader(ShaderData.Value.GetBufferPointer(), ShaderData.Value.GetBufferSize(), null, GeometryShaderData));
        }
        public GeometryShader(ID3D11Device* device, string src, string sourceName, string entryPoint = "GSMain", string shaderModel = "gs_5_0")
        {
            Shader s = CompileFromSourceCode(src, sourceName, entryPoint, shaderModel);
            this.ShaderData = s.ShaderData;

            SilkMarshal.ThrowHResult(device->CreateGeometryShader(ShaderData.Value.GetBufferPointer(), ShaderData.Value.GetBufferSize(), null, GeometryShaderData));
        }
        public GeometryShader(ID3D11Device* device, ShaderFileData fileData, string entryPoint = "GSMain", string shaderModel = "gs_5_0") : this(device, fileData.Source, fileData.SourceName, entryPoint, shaderModel)
        {

        }
    }
}
