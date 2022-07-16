using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DemoSilkX11.Engine.Graphics.Shaders
{
    public static class ShaderLoader
    {
        public static string LoadShaderSource(string shaderName)
        {
            return File.ReadAllText($@"Engine\Graphics\Shaders\{shaderName}");
        }
        public static string GetShaderPath(string shaderName)
        {
            return $@"Engine\Graphics\Shaders\{shaderName}";
        }
        public static ShaderFileData LoadShaderFileData(string shaderName)
        {
            return new ShaderFileData(LoadShaderSource(shaderName), GetShaderPath(shaderName));
        }
    }
}
