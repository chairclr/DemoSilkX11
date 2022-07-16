using Silk.NET;
using Silk.NET.Core;
using Silk.NET.Direct3D;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System.Numerics;

namespace DemoSilkX11
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Demo demo = new Demo();

            demo.Init();
        }
    }
}