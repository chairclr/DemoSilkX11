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

namespace DemoSilkX11.Engine.Graphics.Windows
{
    public class ImGuiMessageFilter : IMessageFilter
    {
        public bool PreFilterMessage(ref System.Windows.Forms.Message m)
        {
            ImGuiNative.ImGui_ImplWin32_WndProcHandler(m.HWnd, (uint)m.Msg, m.WParam, m.LParam);
            return false;
        }
    }
}
