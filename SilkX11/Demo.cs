using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using DemoSilkX11.Engine.Graphics;
using DemoSilkX11.Utility;
using ImGuiNET;
using Color = DemoSilkX11.Utility.Color;
using System.Runtime.InteropServices;
using DemoSilkX11.Engine.Primitives;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using DemoSilkX11.Engine.Graphics.Windows;
using DemoSilkX11.Engine;
using DemoSilkX11.Engine.Graphics.Shaders;
using DemoSilkX11.Engine.Graphics.Textures;
using BepuPhysics;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Collections;
using Box = Silk.NET.Direct3D11.Box;
using BepuPhysics.Trees;
using DemoSilkX11.Engine.Graphics.Primitives;

namespace DemoSilkX11
{
    public unsafe class Demo
    {
        public D3D11 DirectX11;
        public DXGI GraphicsInterface;

        public RenderWindow Window;
        public int WindowWidth => Window.Control.ClientSize.Width;
        public int WindowHeight => Window.Control.ClientSize.Height;
        public Vector2 WindowSize => new Vector2(WindowWidth, WindowHeight);

        public Ref<ID3D11Device> Device = new();
        public Ref<ID3D11DeviceContext> Context = new();
        public Ref<IDXGISwapChain> SwapChain = new();

        // Render Targets and Viewport
        public Ref<ID3D11RenderTargetView> RenderTargetView = new();
        public Viewport Viewport;
        public Texture2D BackBuffer;
        public Texture2D BackBufferNonMS;

        // Rasterizer
        Rasterizer Rasterizer;

        // Depth Stencil
        public Ref<ID3D11DepthStencilView> DepthStencilView = new();
        public Ref<ID3D11DepthStencilState> DepthStencilState = new();
        public Texture2D DepthBuffer;

        // Sampler and Blending
        public Ref<ID3D11SamplerState> SamplerState = new();
        public Ref<ID3D11BlendState> BlendState = new();

        // Shaders
        public VertexShader VertexShader;
        public PixelShader PixelShader;
        

        // Buffers
        public PixelShaderBuffer PixelShaderData;
        public Buffer<PixelShaderBuffer> PixelShaderDataBuffer;

        // Scene stuff
        public List<RenderObject> RenderObjects = new List<RenderObject>();
        public PlaneObject groundPlane;
        public CubeObject testCube;
        public PointLight Light;


        public Camera Camera;
        
        public Texture2D ShowLightTexture;

        public float DeltaTime;
        public float ElapsedTime;
        public int ElapsedTicks;


        

        public Demo()
        {
            DirectX11 = D3D11.GetApi();
            GraphicsInterface = DXGI.GetApi();
            Window = new RenderWindow();
        }

        public void Init()
        {
            InitWindow();
            InitDirectX();
            InitShaders();
            InitScene();
            InitImGui();

            Application.AddMessageFilter(new ImGuiMessageFilter());
            Window.Run();
        }

        private void InitWindow()
        {
            Window.Render += Render;
            Window.Update += Update;
            KeysConverter kv = new KeysConverter();
            Window.Control.KeyDown += (x, y) => 
            { 
                //if (ImGui.GetCurrentContext() != IntPtr.Zero) 
                //    ImGui.GetIO().AddInputCharacter((uint)kv.ConvertToString(y.KeyCode)[0]); 
            };
            ((Form)Window.Control).KeyPreview = true;

            Window.Control.Text = "Silk DirectX11";
            ((Form)Window.Control).ShowIcon = false;

            Window.Control.ClientSize = new System.Drawing.Size(1280, 720);

            Window.Control.AllowDrop = true;
            Window.Control.DragEnter += (x, y) =>
            {
                if (y.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    y.Effect = DragDropEffects.Copy;
                }
            };
            Window.Control.DragDrop += (x, y) =>
            {
                string[] files = (string[])y.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    //TestModel.Dispose();
                    //TestModel = new Model(Device, Context);
                    //
                    //TestModel.LoadFromFile(files[0]);
                    RenderModel obj = new TestRenderObject(Device, Context);
                    obj.InitModel();
                    obj.Start();
                    RenderObjects.Add(obj);
                }
                //foreach (string file in files) Console.WriteLine(file);
            };

            Window.Control.Resize += (x, y) =>
            {
                Context.Value.OMSetRenderTargets(0, null, null);

                BackBuffer.Dispose();
                RenderTargetView.Value.Release();

                SilkMarshal.ThrowHResult(SwapChain.Value.ResizeBuffers(1, (uint)WindowWidth, (uint)WindowHeight, Format.FormatR8G8B8A8Unorm, (uint)SwapChainFlag.SwapChainFlagAllowModeSwitch));

                DepthBuffer.Dispose();
                DepthStencilState.Value.Release();

                CreateBackBuffer();
                CreateDepthStencil();
                CreateViewport();

                Camera.UpdateProjection(60f, (float)WindowWidth / (float)WindowHeight, 0.05f, 1000f);
            };
        }
        private void InitImGui()
        {
            NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, (libraryName, assembly, searchPath) =>
            {
                IntPtr libHandle = IntPtr.Zero;
                if (libraryName == "cimgui")
                {
                    if (Environment.Is64BitProcess)
                    {
                        NativeLibrary.TryLoad("lib/cimgui64.dll", out libHandle);
                    }
                    if (!Environment.Is64BitProcess)
                    {
                        NativeLibrary.TryLoad("lib/cimgui32.dll", out libHandle);
                    }
                }
            
                return libHandle;
            });
            ImGui.CreateContext();
            ImGuiIOPtr io = ImGui.GetIO();


            io.ConfigFlags = ImGuiConfigFlags.NoMouseCursorChange;
            ImGuiNative.ImGui_ImplWin32_Init(Window.Control.Handle);
            ImGuiNative.ImGui_ImplDX11_Init(Device, Context);

            io.Fonts.AddFontFromFileTTF("C:\\Windows\\Fonts\\Tahoma.ttf", 18f);

            ImGuiStylePtr style = ImGui.GetStyle();

            style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(1.000f, 1.000f, 1.000f, 1.000f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new System.Numerics.Vector4(0.500f, 0.500f, 0.500f, 1.000f);
            style.Colors[(int)ImGuiCol.WindowBg] = new System.Numerics.Vector4(0.180f, 0.180f, 0.180f, 1.000f);
            style.Colors[(int)ImGuiCol.ChildBg] = new System.Numerics.Vector4(0.280f, 0.280f, 0.280f, 0.000f);
            style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.313f, 0.313f, 0.313f, 1.000f);
            style.Colors[(int)ImGuiCol.Border] = new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1.000f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new System.Numerics.Vector4(0.000f, 0.000f, 0.000f, 0.000f);
            style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.160f, 0.160f, 0.160f, 1.000f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new System.Numerics.Vector4(0.200f, 0.200f, 0.200f, 1.000f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new System.Numerics.Vector4(0.280f, 0.280f, 0.280f, 1.000f);
            style.Colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0.148f, 0.148f, 0.148f, 1.000f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0.148f, 0.148f, 0.148f, 1.000f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new System.Numerics.Vector4(0.148f, 0.148f, 0.148f, 1.000f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new System.Numerics.Vector4(0.195f, 0.195f, 0.195f, 1.000f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new System.Numerics.Vector4(0.160f, 0.160f, 0.160f, 1.000f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new System.Numerics.Vector4(0.277f, 0.277f, 0.277f, 1.000f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new System.Numerics.Vector4(0.300f, 0.300f, 0.300f, 1.000f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new System.Numerics.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            style.Colors[(int)ImGuiCol.CheckMark] = new System.Numerics.Vector4(1.000f, 1.000f, 1.000f, 1.000f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new System.Numerics.Vector4(0.391f, 0.391f, 0.391f, 1.000f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new System.Numerics.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(1.000f, 1.000f, 1.000f, 0.000f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(1.000f, 1.000f, 1.000f, 0.156f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(1.000f, 1.000f, 1.000f, 0.391f);
            style.Colors[(int)ImGuiCol.Header] = new System.Numerics.Vector4(0.313f, 0.313f, 0.313f, 1.000f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new System.Numerics.Vector4(0.469f, 0.469f, 0.469f, 1.000f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new System.Numerics.Vector4(0.469f, 0.469f, 0.469f, 1.000f);
            style.Colors[(int)ImGuiCol.Separator] = style.Colors[(int)ImGuiCol.Border];
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new System.Numerics.Vector4(0.391f, 0.391f, 0.391f, 1.000f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new System.Numerics.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new System.Numerics.Vector4(1.000f, 1.000f, 1.000f, 0.250f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new System.Numerics.Vector4(1.000f, 1.000f, 1.000f, 0.670f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new System.Numerics.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.098f, 0.098f, 0.098f, 1.000f);
            style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.352f, 0.352f, 0.352f, 1.000f);
            style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.195f, 0.195f, 0.195f, 1.000f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.098f, 0.098f, 0.098f, 1.000f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.195f, 0.195f, 0.195f, 1.000f);
            style.Colors[(int)ImGuiCol.PlotLines] = new System.Numerics.Vector4(0.469f, 0.469f, 0.469f, 1.000f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new System.Numerics.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new System.Numerics.Vector4(0.586f, 0.586f, 0.586f, 1.000f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new System.Numerics.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new System.Numerics.Vector4(1.000f, 1.000f, 1.000f, 0.156f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new System.Numerics.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new System.Numerics.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new System.Numerics.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new System.Numerics.Vector4(0.000f, 0.000f, 0.000f, 0.586f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new System.Numerics.Vector4(0.000f, 0.000f, 0.000f, 0.586f);

            style.ChildRounding = 4.0f;
            style.FrameBorderSize = 1.0f;
            style.FrameRounding = 2.0f;
            style.GrabMinSize = 7.0f;
            style.PopupRounding = 2.0f;
            style.ScrollbarRounding = 12.0f;
            style.ScrollbarSize = 13.0f;
            style.TabBorderSize = 1.0f;
            style.TabRounding = 0.0f;
            style.WindowRounding = 4.0f;
        }

        private unsafe void InitDirectX()
        {
            CreateSwapChain();
            CreateBackBuffer();
            CreateDepthStencil();
            CreateViewport();
            CreateRasterizer();
            CreateBlendState();
            CreateSampler();

            InitShadows();
        }
        private unsafe void CreateSwapChain()
        {
            IReadOnlyList<GraphicsAdapter> availableAdapters = AdapterReader.Read(GraphicsInterface);

            if (availableAdapters.Count == 0)
                throw new InvalidOperationException("No valid adapters");

            GraphicsAdapter defaultAdapter = availableAdapters[0];

            ModeDesc bufferDesc = new ModeDesc()
            {
                Width = (uint)WindowWidth,
                Height = (uint)WindowHeight,
                RefreshRate = new Silk.NET.DXGI.Rational(60, 1),
                Format = Format.FormatR8G8B8A8Unorm,
            };
            

            SwapChainDesc swapChainDesc = new SwapChainDesc()
            {
                BufferDesc = bufferDesc,
                SampleDesc = new SampleDesc(8, 0),
                BufferUsage = DXGI.UsageShaderInput | DXGI.UsageRenderTargetOutput ,
                BufferCount = 1,
                OutputWindow = Window.Control.Handle,
                Windowed = 1,
                SwapEffect = SwapEffect.SwapEffectDiscard,
                Flags = (uint)SwapChainFlag.SwapChainFlagAllowModeSwitch,
            };


            SilkMarshal.ThrowHResult(DirectX11.CreateDeviceAndSwapChain(defaultAdapter.Adapter, // Graphics Adapter
                                                                        D3DDriverType.D3DDriverTypeUnknown, // Driver Type
                                                                        0,
                                                                        0,
                                                                        null,
                                                                        0,
                                                                        D3D11.SdkVersion, // SDK Version
                                                                        ref swapChainDesc, // Swap Chain Description
                                                                        SwapChain.GetAddressOf(), // Swap Chain
                                                                        Device.GetAddressOf(), // Device
                                                                        null,
                                                                        Context.GetAddressOf())); // Device Context
        }
        private unsafe void CreateBackBuffer()
        {
            BackBuffer = new Texture2D(Device, TextureType.BackBuffer);

            SilkMarshal.ThrowHResult(SwapChain.Value.GetBuffer(0, ref Texture2D.Guid, BackBuffer.TextureData));

            SilkMarshal.ThrowHResult(Device.Value.CreateRenderTargetView(BackBuffer, null, RenderTargetView));

            BackBufferNonMS = BackBuffer.CreateNonMSCopy();
        }
        private unsafe void CreateDepthStencil()
        {
            DepthBuffer = new Texture2D(Device, WindowWidth, WindowHeight, new SampleDesc(8, 0), BindFlag.BindDepthStencil | BindFlag.BindShaderResource, TextureType.DepthBuffer, Format.FormatR32Typeless);

            DepthStencilViewDesc viewDesc = new DepthStencilViewDesc()
            {
                Format = Format.FormatD32Float,
                ViewDimension = DsvDimension.DsvDimensionTexture2Dms,
            };

            SilkMarshal.ThrowHResult(Device.Value.CreateDepthStencilView(DepthBuffer, ref viewDesc, DepthStencilView));

            Context.Value.OMSetRenderTargets(1, RenderTargetView, DepthStencilView);

            DepthStencilDesc depthstencildesc = new DepthStencilDesc()
            {
                DepthEnable = 1,
                DepthWriteMask = DepthWriteMask.DepthWriteMaskAll,
                DepthFunc = ComparisonFunc.ComparisonLessEqual,
            };

            SilkMarshal.ThrowHResult(Device.Value.CreateDepthStencilState(ref depthstencildesc, DepthStencilState));
        }
        private unsafe void CreateViewport()
        {
            Viewport viewport = new Viewport()
            {
                TopLeftX = 0,
                TopLeftY = 0,
                Width = WindowWidth,
                Height = WindowHeight,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };

            Viewport = viewport;

            Context.Value.RSSetViewports(1, ref Viewport);
        }
        private unsafe void CreateRasterizer()
        {
            RasterizerDesc rasterizerDesc = new RasterizerDesc()
            {
                MultisampleEnable = 1,
                AntialiasedLineEnable = 1,
                FillMode = FillMode.FillSolid,
                CullMode = CullMode.CullBack,
            };

            Rasterizer = new Rasterizer(Device, rasterizerDesc);
        }
        private unsafe void CreateBlendState()
        {
            RenderTargetBlendDesc rtbd = new RenderTargetBlendDesc()
            {
                BlendEnable = 1,
                SrcBlend = Blend.BlendSrcAlpha,
                DestBlend = Blend.BlendInvSrcAlpha,
                BlendOp = BlendOp.BlendOpAdd,
                SrcBlendAlpha = Blend.BlendOne,
                DestBlendAlpha = Blend.BlendZero,
                BlendOpAlpha = BlendOp.BlendOpAdd,
                RenderTargetWriteMask = (byte)ColorWriteEnable.ColorWriteEnableAll
            };

            BlendDesc blendDesc = new BlendDesc();

            blendDesc.RenderTarget[0] = rtbd;

            SilkMarshal.ThrowHResult(Device.Value.CreateBlendState(ref blendDesc, BlendState));
        }
        private unsafe void CreateSampler()
        {
            SamplerDesc sampDesc = new SamplerDesc()
            {
                Filter = Filter.FilterComparisonMinMagMipLinear,
                AddressU = TextureAddressMode.TextureAddressWrap,
                AddressV = TextureAddressMode.TextureAddressWrap,
                AddressW = TextureAddressMode.TextureAddressWrap,
                ComparisonFunc = ComparisonFunc.ComparisonNever,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            };

            SilkMarshal.ThrowHResult(Device.Value.CreateSamplerState(ref sampDesc, SamplerState));
        }

        // SHADOWS
        
        public static int ShadowMapSize = 2048;
        
        public Ref<ID3D11SamplerState> ShadowSamplerState = new();
        public Rasterizer ShadowRasterizer;
        public VertexShader ShadowDepthVertexShader;
        public PixelShader ShadowDepthPixelShader;
        public GeometryShader ShadowDepthGeometryShader;
        public ShadowGeometryShaderBuffer ShadowGeometryShaderData;
        public Buffer<ShadowGeometryShaderBuffer> ShadowGeometryShaderDataBuffer;

        private unsafe void InitShadows()
        {
            SamplerDesc shadowSampDesc = new SamplerDesc()
            {
                Filter = Filter.FilterComparisonMinMagMipLinear,
                AddressU = TextureAddressMode.TextureAddressBorder,
                AddressV = TextureAddressMode.TextureAddressBorder,
                AddressW = TextureAddressMode.TextureAddressBorder,
                ComparisonFunc = ComparisonFunc.ComparisonLessEqual,
                MinLOD = 0.0f,
                MaxLOD = float.MaxValue,
            };

            shadowSampDesc.BorderColor[0] = 1.0f;
            shadowSampDesc.BorderColor[1] = 1.0f;
            shadowSampDesc.BorderColor[2] = 1.0f;
            shadowSampDesc.BorderColor[3] = 1.0f;

                
            SilkMarshal.ThrowHResult(Device.Value.CreateSamplerState(ref shadowSampDesc, ShadowSamplerState));

            RasterizerDesc rasterizerDesc = new RasterizerDesc()
            {
                MultisampleEnable = 1,
                AntialiasedLineEnable = 1,
                DepthClipEnable = 1,
                FillMode = FillMode.FillSolid,
                CullMode = CullMode.CullBack,
            };
            ShadowRasterizer = new Rasterizer(Device, rasterizerDesc);

            uint geoSize = (uint)(sizeof(ShadowGeometryShaderBuffer) + (16 - (sizeof(ShadowGeometryShaderBuffer) % 16)));
            ShadowGeometryShaderDataBuffer = new Buffer<ShadowGeometryShaderBuffer>(Device, BindFlag.BindConstantBuffer, geoSize, Usage.UsageDynamic, CpuAccessFlag.CpuAccessWrite);
        }
        private unsafe void InitShaders()
        {
            InputElementDesc[] vertexLayout =
            {
                new InputElementDesc((byte*)SilkMarshal.StringToPtr("POSITION"), 0, Format.FormatR32G32B32Float, 0, 0,                          InputClassification.InputPerVertexData),
                new InputElementDesc((byte*)SilkMarshal.StringToPtr("TEXCOORD"), 0, Format.FormatR32G32Float,    0, D3D11.AppendAlignedElement, InputClassification.InputPerVertexData),
                new InputElementDesc((byte*)SilkMarshal.StringToPtr("NORMAL"),   0, Format.FormatR32G32B32Float, 0, D3D11.AppendAlignedElement, InputClassification.InputPerVertexData),
            };

            VertexShader = new VertexShader(Device, vertexLayout, ShaderLoader.LoadShaderFileData("VertexShader.hlsl"));

            PixelShader = new PixelShader(Device, ShaderLoader.LoadShaderFileData("PixelShader.hlsl"));

            InputElementDesc[] shadowVertexLayout =
            {
                new InputElementDesc((byte*)SilkMarshal.StringToPtr("POSITION"), 0, Format.FormatR32G32B32Float, 0, 0, InputClassification.InputPerVertexData),
            };

            ShadowDepthVertexShader = new VertexShader(Device, shadowVertexLayout, ShaderLoader.LoadShaderFileData("ShadowVertexShader.hlsl"));
            ShadowDepthPixelShader = new PixelShader(Device, ShaderLoader.LoadShaderFileData(@"ShadowPixelShader.hlsl"));
            ShadowDepthGeometryShader = new GeometryShader(Device, ShaderLoader.LoadShaderFileData(@"ShadowGeometryShader.hlsl"));
        }
        private unsafe void InitScene()
        {
            uint pixelSize = (uint)(sizeof(PixelShaderBuffer) + (16 - (sizeof(PixelShaderBuffer) % 16)));
            PixelShaderDataBuffer = new Buffer<PixelShaderBuffer>(Device, BindFlag.BindConstantBuffer, pixelSize, Usage.UsageDynamic, CpuAccessFlag.CpuAccessWrite);

            ShowLightTexture = TextureLoader.LoadEngineTexture(Device, Context, @"light.png");

            Camera = new Camera(60f, (float)WindowWidth / (float)WindowHeight, 0.05f, 1000f);
            Camera.Position += new Vector3(0, 0, 2.5f);
            cameraMouseRotation.X = MathF.PI;
            Camera.Rotation = cameraMouseRotation.ToQuaternion();

            Light = new PointLight(Device, Context);
            Light.Transform.Position += new Vector3(0f, 4f, 1.4f);
            Light.LightStrength = 3.5f;


            for (int i = 0; i < 5; i++)
            {
                RenderObjects.Add(new TestRenderObject(Device, Context));
                RenderObjects.Last().Transform.Position += new Vector3(8f,0f, 0f) - (Vector3.UnitX * i * 4);
                RenderObjects.Last().Transform.YawPitchRoll = new Vector3(Util.Random.NextFloat() * 3f, Util.Random.NextFloat() * 3f, 0.0f);
            }

            groundPlane = new PlaneObject(Device, Context, Color.White);
            groundPlane.Transform.YawPitchRoll = new Vector3(0f, MathF.PI / 2f, 0.0f);
            groundPlane.Transform.Position -= Vector3.UnitY * 6.5f;
            groundPlane.Transform.Scale = new Vector3(20f);
            RenderObjects.Add(groundPlane);

            testCube = new CubeObject(Device, Context, Color.White);
            RenderObjects.Add(groundPlane);

            foreach (RenderObject obj in RenderObjects)
            {
                obj.Start();
            }
            InitPhysics();
        }
        Simulation sim;
        private void InitPhysics()
        {
            BepuUtilities.Memory.BufferPool bp = new BepuUtilities.Memory.BufferPool();
            sim = Simulation.Create(bp, new Engine.Physics.NarrowPhaseCallbacks(), new Engine.Physics.PoseIntegratorCallbacks(new Vector3(0, -10f, 0)), new SolveDescription(8, 1));

            foreach (RenderObject obj in RenderObjects)
            {
                if (obj.GetType() == typeof(TestRenderObject))
                {
                    BepuPhysics.Collidables.Box box = new BepuPhysics.Collidables.Box(obj.Transform.Scale.X, obj.Transform.Scale.Y, obj.Transform.Scale.Z);
                    BodyInertia inertia = box.ComputeInertia(1);
                    ((TestRenderObject)obj).bodyHandle = sim.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(obj.Transform.Position, obj.Transform.Rotation), inertia, sim.Shapes.Add(box), 0.01f));
                }
                if (obj.GetType() == typeof(PlaneObject))
                {
                    //BepuPhysics.Collidables.Box box = new BepuPhysics.Collidables.Box(obj.Transform.Scale.X, obj.Transform.Scale.Y, obj.Transform.Scale.Z);
                    //BodyInertia sphereInertia = box.ComputeInertia(1);
                    //((TestRenderObject)obj).bodyHandle = sim.Bodies.Add(BodyDescription.CreateDynamic(obj.Transform.Position, sphereInertia, sim.Shapes.Add(box), 0.01f));

                    BepuPhysics.Collidables.Box box = new BepuPhysics.Collidables.Box(obj.Transform.Scale.X, obj.Transform.Scale.Y, 0.0001f);
                    sim.Statics.Add(new StaticDescription(obj.Transform.Position, obj.Transform.Rotation, sim.Shapes.Add(box)));
                }
            }
        }

        public static Color ClearColor = new Color(0.45f, 0.55f, 0.60f);
        public void PreRenderScene()
        {
            Context.Value.OMSetRenderTargets(1, RenderTargetView, DepthStencilView);
            Context.Value.OMSetDepthStencilState(DepthStencilState, 0);
            Context.Value.OMSetBlendState(BlendState, null, (uint)0xFFFFFFFF);

            Context.Value.ClearRenderTargetView(RenderTargetView, ClearColor);
            Context.Value.ClearDepthStencilView(DepthStencilView, (uint)(ClearFlag.ClearDepth | ClearFlag.ClearStencil), 1.0f, 0);

            Context.Value.IASetInputLayout(VertexShader.InputLayoutData);
            Context.Value.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D10PrimitiveTopologyTrianglelist);


            Context.Value.RSSetState(Rasterizer.RasterizerState);
            Context.Value.RSSetViewports(1, ref Viewport);

            
            Context.Value.PSSetSamplers(0, 1, SamplerState);
            Context.Value.PSSetSamplers(1, 1, ShadowSamplerState);
        }
        public void RenderScenePass()
        {
            PreRenderScene();
            Context.Value.VSSetShader(VertexShader.VertexShaderData, null, 0);
            Context.Value.PSSetShader(PixelShader.PixelShaderData, null, 0);
            Context.Value.GSSetShader(null, null, 0);

            PixelShaderData.Light.Position = Light.Transform.Position;
            PixelShaderData.Light.Color = Light.LightColor;
            PixelShaderData.Light.Attenuation = Light.Attenuation;
            PixelShaderData.Light.Intensity = Light.LightStrength;
            PixelShaderDataBuffer.WriteData(Context, ref PixelShaderData);

            Context.Value.PSSetConstantBuffers(0, 1, PixelShaderDataBuffer.DataBuffer);
            Context.Value.PSSetShaderResources(1, 1, Light.ShadowDepthBuffer.GetDepthTextureCubeShaderResource());
            
            foreach (RenderObject obj in RenderObjects)
            {
                obj.Render(Camera);
            }
        }

        public void PreRenderShadows()
        {
            Matrix4x4[] mats = Light.GetViewMatrices();
            ShadowGeometryShaderData.Projection = Matrix4x4.Transpose(Light.ProjectionMatrix);
            ShadowGeometryShaderData.View1 = Matrix4x4.Transpose(mats[0]);
            ShadowGeometryShaderData.View2 = Matrix4x4.Transpose(mats[1]);
            ShadowGeometryShaderData.View3 = Matrix4x4.Transpose(mats[2]);
            ShadowGeometryShaderData.View4 = Matrix4x4.Transpose(mats[3]);
            ShadowGeometryShaderData.View5 = Matrix4x4.Transpose(mats[4]);
            ShadowGeometryShaderData.View6 = Matrix4x4.Transpose(mats[5]);
            ShadowGeometryShaderDataBuffer.WriteData(Context, ref ShadowGeometryShaderData);
            Context.Value.GSSetConstantBuffers(0, 1, ShadowGeometryShaderDataBuffer.DataBuffer);

            Context.Value.IASetInputLayout(ShadowDepthVertexShader.InputLayoutData);
            Context.Value.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D10PrimitiveTopologyTrianglelist);
            Context.Value.RSSetState(ShadowRasterizer.RasterizerState);
            
        }
        public void RenderShadowPass()
        {
            PreRenderShadows();

            Context.Value.VSSetShader(ShadowDepthVertexShader.VertexShaderData, null, 0);
            Context.Value.PSSetShader(ShadowDepthPixelShader.PixelShaderData, null, 0);
            Context.Value.GSSetShader(ShadowDepthGeometryShader.GeometryShaderData, null, 0);

            Light.RenderShadows();

            foreach (RenderObject obj in RenderObjects)
            {
                obj.RenderShadows();
            }
        }


        public void Render()
        {
            RenderShadowPass();
            RenderScenePass();
            RenderImGui();

            SwapChain.Value.Present((uint)(vsync ? 1 : 0), 0);
        }


        float cameraSpeed = 4f;
        float cameraSensetivity = 0.7f;
        bool vsync = true;

        string[] cullModeNames = Util.EnumFancyNames<CullMode>();
        string[] fillModeNames = Util.EnumFancyNames<FillMode>();
        TestRenderObject selectedBody = null;

        private void RenderImGui()
        {
            ImGuiNative.ImGui_ImplDX11_NewFrame();
            ImGuiNative.ImGui_ImplWin32_NewFrame();
            ImGui.NewFrame();

            ImGuiIOPtr io = ImGui.GetIO();


            ImGui.Begin("Test");
            ImGui.Text($"FPS {io.Framerate}");

            if (ImGui.CollapsingHeader("Camera Settings"))
            {
                Vector3 tmp = Camera.Position;
                if (ImGui.DragFloat3("Position", ref tmp, 0.05f))
                {
                    Camera.Position = tmp;
                }

                ImGui.DragFloat("Speed", ref cameraSpeed, 0.05f);
                ImGui.DragFloat("Sense", ref cameraSensetivity, 0.01f);

                ImGui.Checkbox("VSync", ref vsync);

                int currentCullMode = Array.IndexOf(Enum.GetValues<CullMode>(), Rasterizer.Description.CullMode);
                if (ImGui.Combo("Cull mode", ref currentCullMode, cullModeNames, cullModeNames.Length))
                {
                    Rasterizer.Description.CullMode = Enum.GetValues<CullMode>()[currentCullMode];
                    Rasterizer.CreateState(Rasterizer.Description);
                }

                int currentFillMode = Array.IndexOf(Enum.GetValues<FillMode>(), Rasterizer.Description.FillMode);
                if (ImGui.Combo("Fill mode", ref currentFillMode, fillModeNames, fillModeNames.Length))
                {
                    Rasterizer.Description.FillMode = Enum.GetValues<FillMode>()[currentFillMode];
                    Rasterizer.CreateState(Rasterizer.Description);
                }
            }
            if (ImGui.CollapsingHeader("Plane Settings"))
            {
                Vector3 gs = groundPlane.Transform.Scale;
                if (ImGui.DragFloat3("Scale", ref gs))
                {
                    groundPlane.Transform.Scale = gs;
                }

            }
            if (ImGui.CollapsingHeader("Lighting"))
            {
                Vector3 lpp = Light.Transform.Position;
                if (ImGui.DragFloat3("Light Position", ref lpp))
                {
                    Light.Transform.Position = lpp;
                }

                ImGui.DragFloat("Light Strength", ref Light.LightStrength);

                ImGui.DragFloat3("Light Attenuation", ref Light.Attenuation);

                if (ImGui.Button("Dump to file"))
                {
                    DumbToFile();
                }
            }
            if (ImGui.CollapsingHeader("Objects"))
            {
                for (int i = 0; i < RenderObjects.Count; i++)
                {
                    RenderObject obj = RenderObjects[i];
                    if (obj.GetType() == typeof(TestRenderObject))
                    {
                        TestRenderObject sex = (TestRenderObject)obj;

                        if (ImGui.DragFloat3($"Body {i}", ref sim.Bodies[sex.bodyHandle].Velocity.Linear, 0.05f))
                        {
                            sim.Awakener.AwakenBody(sex.bodyHandle);
                        }

                        bool value = selectedBody == sex;

                        if (ImGui.Checkbox($"Move body {i}", ref value))
                        {
                            if (value == false)
                            {
                                selectedBody = null;
                            }
                            else
                            {
                                selectedBody = sex;
                            }
                        }
                    }
                }
            }
            if (ImGui.Button("spawn more cubes"))
            {
                for (int i = 0; i < 5; i++)
                {
                    RenderObjects.Add(new TestRenderObject(Device, Context));
                    TestRenderObject obj = (TestRenderObject)RenderObjects.Last();
                    obj.Transform.Position += new Vector3(8f, 0f, 0f) - (Vector3.UnitX * i * 4);
                    obj.Transform.YawPitchRoll = new Vector3(Util.Random.NextFloat() * 3f, Util.Random.NextFloat() * 3f, 0.0f);

                    obj.Start();

                    BepuPhysics.Collidables.Box box = new BepuPhysics.Collidables.Box(obj.Transform.Scale.X, obj.Transform.Scale.Y, obj.Transform.Scale.Z);
                    BodyInertia inertia = box.ComputeInertia(1);
                    obj.bodyHandle = sim.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(obj.Transform.Position, obj.Transform.Rotation), inertia, sim.Shapes.Add(box), 0.01f));
                }
            }

            ImGui.End();

            ImGui.SetNextWindowSize(Vector2.Zero);
            ImGui.Begin("DrawList Window", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRectFullScreen();


            Vector3 pos = Camera.WorldToScreen(Light.Transform.Position, WindowSize);
            // dont draw if its behind the camera, you fucking belland
            if (pos.Z > 0)
            {
                float size = (16f / pos.Z) * 4f;
                drawList.AddImage(ShowLightTexture.GetTextureShaderResource(), new Vector2(pos.X - size, pos.Y - size), new Vector2(pos.X + size, pos.Y + size));
            }

            //Vector3 lineStart = Camera.WorldToScreen(Camera.Position + rayDir * 1f, WindowSize);
            //Vector3 lineEnd = Camera.WorldToScreen(Camera.Position + rayDir * 5f, WindowSize);
            //
            //drawList.AddLine(lineStart.XY(), lineEnd.XY(), Color.Red.PackedValue);

            ImGui.End();

            ImGui.Render();
            ImGuiNative.ImGui_ImplDX11_RenderDrawData(ImGui.GetDrawData());
        }


        Vector3 cameraMouseRotation = Vector3.Zero;
        Stopwatch dtWatch = new Stopwatch();
        public void Update()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            DeltaTime = (float)dtWatch.Elapsed.TotalSeconds;
            dtWatch.Restart();

            ElapsedTime += DeltaTime;
            ElapsedTicks++;

            if (!io.WantCaptureKeyboard && !io.WantCaptureMouse)
            {
                if (io.MouseDown[0])
                {
                    cameraMouseRotation.X += -io.MouseDelta.X * (cameraSensetivity / 70f);
                    cameraMouseRotation.Y += io.MouseDelta.Y * (cameraSensetivity / 70f);
                
                    Camera.Rotation = cameraMouseRotation.ToQuaternion();
                }
                float s = cameraSpeed * DeltaTime;
                if (io.KeysDown['W'])
                {
                    Camera.Position += Camera.Forward * s;
                }
                if (io.KeysDown['S'])
                {
                    Camera.Position += Camera.Backward * s;
                }
                if (io.KeysDown['A'])
                {
                    Camera.Position += Camera.Right * s;
                }
                if (io.KeysDown['D'])
                {
                    Camera.Position += Camera.Left * s;
                }
                if (io.KeysDown['Q'])
                {
                    Camera.Position += Vector3.UnitY * s;
                }
                if (io.KeysDown['E'])
                {
                    Camera.Position += -Vector3.UnitY * s;
                }
            }

            //foreach (RenderObject obj in RenderObjects)
            //{
            //    if (obj.GetType() == typeof(TestRenderObject))
            //    {
            //        BodyReference bh = sim.Bodies[((TestRenderObject)obj).bodyHandle];
            //        bh.Pose.Position = obj.Transform.Position;
            //        bh.Pose.Orientation = obj.Transform.Rotation;
            //    }
            //}

            if (io.MouseClicked[2])
            {
                Vector3 rayDir = Camera.ScreenToRayWorld(io.MousePos, WindowSize);
                RayHitHandler hitHandler = new RayHitHandler();
                hitHandler.T = float.MaxValue;
                sim.RayCast(Camera.Position, rayDir, 100f, ref hitHandler);
                if (hitHandler.T < float.MaxValue && hitHandler.HitCollidable.Mobility == CollidableMobility.Dynamic)
                {
                    hitT = hitHandler.T;
                    hitLocation = Camera.Position + rayDir * hitT;

                    body = sim.Bodies[hitHandler.HitCollidable.BodyHandle];

                    grabbingBody = true;
                    RigidPose.TransformByInverse(hitLocation, body.Pose, out localGrabPoint);
                    targetOrientation = body.Pose.Orientation;

                    //foreach (RenderObject obj in RenderObjects)
                    //{
                    //    if (obj.GetType() == typeof(TestRenderObject))
                    //    {
                    //        if (hitHandler.HitCollidable.BodyHandle == ((TestRenderObject)obj).bodyHandle)
                    //        {
                    //            selectedBody = ((TestRenderObject)obj);
                    //            grabbingBody = true;
                    //
                    //        }
                    //    }
                    //}
                }
            }

            if (io.MouseDown[2] && grabbingBody)
            {
                Vector3 rayDir = Camera.ScreenToRayWorld(io.MousePos, WindowSize);
                Vector3 targetPoint = Camera.Position + rayDir * hitT;

                body.Velocity.Linear = (targetPoint - body.Pose.Position) * 3f; 
                sim.Awakener.AwakenBody(body);
            }

            if (io.MouseReleased[2] && grabbingBody)
            {
                grabbingBody = false;
            }

            sim.Timestep(DeltaTime <= 0 ? (1f/144f) : DeltaTime);
            for (int i = 0; i < RenderObjects.Count; i++)
            {
                RenderObject obj = RenderObjects[i];
                if (obj.GetType() == typeof(TestRenderObject))
                {
                    BodyReference bh = sim.Bodies[((TestRenderObject)obj).bodyHandle];
                    obj.Transform.Position = bh.Pose.Position;
                    obj.Transform.Rotation = bh.Pose.Orientation;
                }

                
                obj.Update(DeltaTime);

                if (obj.Transform.Position.Y < -100f)
                {
                    obj.Dispose();
                    RenderObjects.RemoveAt(i);
                }
            }
        }
        bool grabbingBody = false;
        float hitT;
        Vector3 hitLocation;
        Vector3 localGrabPoint;
        Quaternion targetOrientation;
        BodyReference body;

        public void DumbToFile()
        {
            for (int i = 0; i < 6; i++)
            {
                float[,] data = Light.ShadowDepthBuffer.GetData<float>(Context, Format.FormatR32Float, i, (x) => 0.01f / (1.01f - x));
                using Image<Rgba32> imag = new Image<Rgba32>(data.GetLength(0), data.GetLength(1));

                for (int x = 0; x < data.GetLength(0); x++)
                {
                    for (int y = 0; y < data.GetLength(1); y++)
                    {
                        imag[x, y] = new Rgba32(data[x, y], data[x, y], data[x, y]);
                    }
                }


                imag.SaveAsPng($"depthBuffer{i}.png");
            }
        }

        struct RayHitHandler : IRayHitHandler
        {
            public float T;
            public CollidableReference HitCollidable;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AllowTest(CollidableReference collidable)
            {
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
            {
                //We are only interested in the earliest hit. This callback is executing within the traversal, so modifying maximumT informs the traversal
                //that it can skip any AABBs which are more distant than the new maximumT.
                maximumT = t;
                //Cache the earliest impact.
                T = t;
                HitCollidable = collidable;
            }
        }
    }
}
