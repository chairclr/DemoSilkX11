using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;
using Silk.NET;
using Assimp = Silk.NET.Assimp;
using System.IO;
using System.Numerics;
using Silk.NET.DXGI;
using DemoSilkX11.Engine.Graphics.Textures;

namespace DemoSilkX11.Engine.Graphics
{
    public class Model : IDisposable
    {
        private static Assimp.Assimp AssimpAPI;

        public Ref<ID3D11Device> Device;
        public Ref<ID3D11DeviceContext> Context;

        private List<Mesh> meshes;
        public IReadOnlyList<Mesh> Meshes => meshes;

        private string dir;
        public string Directory => dir;

        public Model(Ref<ID3D11Device> device, Ref<ID3D11DeviceContext> context)
        {
            Device = device;
            Context = context;
            meshes = new List<Mesh>();
        }

        public unsafe void LoadFromFile(string path)
        {
            path = Path.GetFullPath(path);
            if (AssimpAPI == null)
            {
                AssimpAPI = Assimp.Assimp.GetApi();
            }
            dir = Path.GetDirectoryName(path);
            Ref<Assimp.Scene> scene = new Ref<Assimp.Scene>(AssimpAPI.ImportFile(path, (uint)(Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.SortByPrimitiveType | Assimp.PostProcessSteps.FlipWindingOrder | Assimp.PostProcessSteps.FlipUVs)));

            if (scene.Get() == null)
                throw new NullReferenceException($"Could not load {path} as scene");

            ProcessNode(new Ref<Assimp.Node>(scene.Value.MRootNode), scene);

            AssimpAPI.FreeScene(scene);
        }

        public unsafe void Render()
        {
            foreach (Mesh mesh in Meshes)
            {
                mesh.Render();
            }
        }
        public unsafe void RenderShadows(ref VertexShaderBuffer shaderBuffer, Buffer<VertexShaderBuffer> data)
        {
            shaderBuffer.ViewProjection = Matrix4x4.Transpose(shaderBuffer.ViewProjection);
            shaderBuffer.World = Matrix4x4.Transpose(shaderBuffer.World);
            data.WriteData(Context, ref shaderBuffer);

            Context.Value.VSSetConstantBuffers(0, 1, data.DataBuffer);

            foreach (Mesh mesh in Meshes)
            {
                mesh.Render();
            }
        }

        private unsafe List<Texture2D> ProcessMaterialTextures(Ref<Assimp.Material> material, Assimp.TextureType textureType, Ref<Assimp.Scene> scene)
        {
            List<Texture2D> textures = new List<Texture2D>();
            
            int textureCount = (int)AssimpAPI.GetMaterialTextureCount(material, textureType);

            if (textureCount == 0)
            {
                switch (textureType)
                {
                    case Assimp.TextureType.TextureTypeDiffuse:
                    {
                        Vector4 color = new Vector4();
                        Assimp.Return ret = AssimpAPI.GetMaterialColor(material, Assimp.Assimp.MatkeyColorDiffuse, 0, 0, ref color);

                        if (ret == Assimp.Return.ReturnSuccess)
                        {
                            textures.Add(TextureLoader.LoadTextureFromPixel(Device, Context, 1, 1, new Utility.Color(color))); 
                            textures.Last().TextureType = TextureType.Diffuse;
                        }
                    }
                        break;
                }
            }
            else 
            {
                for (int i = 0; i < textureCount; i++)
                {
                    string path;

                    Ref<Assimp.MaterialProperty> prop = new();

                    if (AssimpAPI.GetMaterialProperty(material, Assimp.Assimp.MatkeyTextureBase/*"$tex.file"*/, (uint)textureType, (uint)i, prop) == Assimp.Return.ReturnSuccess)
                    {
                        string fileRelative = ((Assimp.AssimpString*)prop.Value.MData)->AsString;

                        if (fileRelative.StartsWith(@"/") || fileRelative.StartsWith(@"\"))
                            fileRelative = fileRelative.Remove(0, 1);

                        string fullPath = Path.Combine(Directory, fileRelative);

                        textures.Add(TextureLoader.LoadTextureFromFile(Device, Context, fullPath));
                        textures.Last().TextureType = (TextureType)textureType;
                    }

                }
            }

            return textures;
        }
        private unsafe Mesh ProcessMesh(Ref<Assimp.Mesh> mesh, Ref<Assimp.Scene> scene)
        {
            List<Vertex> vertices = new List<Vertex>((int)mesh.Value.MNumVertices);

            List<int> indicies = new List<int>((int)(mesh.Value.MNumFaces * 3));


            for (int i = 0; i < mesh.Value.MNumVertices; i++)
            {
                Vertex vertex = new Vertex();

                vertex.Position = mesh.Value.MVertices[i];

                if (mesh.Value.MTextureCoords[0] != null)
                {
                    vertex.UV.X = mesh.Value.MTextureCoords[0][i].X;
                    vertex.UV.Y = mesh.Value.MTextureCoords[0][i].Y;
                }

                if (mesh.Value.MNormals != null)
                {
                    vertex.Normal = mesh.Value.MNormals[i];
                }

                vertices.Add(vertex);
            }

            for (int i = 0; i < mesh.Value.MNumFaces; i++)
            {
                Assimp.Face face = mesh.Value.MFaces[i];

                for (int j = 0; j < face.MNumIndices; j++)
                {
                    indicies.Add((int)face.MIndices[j]);
                }
            }

            Ref<Assimp.Material> material = new Ref<Assimp.Material>(scene.Value.MMaterials[mesh.Value.MMaterialIndex]);

            List<Texture2D> textures = ProcessMaterialTextures(material, Assimp.TextureType.TextureTypeDiffuse, scene);

            return new Mesh(Device, Context, vertices, indicies, textures);
        }
        private unsafe void ProcessNode(Ref<Assimp.Node> node, Ref<Assimp.Scene> scene)
        {
            for (int i = 0; i < node.Value.MNumMeshes; i++)
            {
                Ref<Assimp.Mesh> mesh = new Ref<Assimp.Mesh>(scene.Value.MMeshes[node.Value.MMeshes[i]]);

                if ((mesh.Value.MPrimitiveTypes & (uint)Assimp.PrimitiveType.PrimitiveTypeTriangle) != 0)
                {
                    meshes.Add(ProcessMesh(mesh, scene));
                }
            }

            for (int i = 0; i < node.Value.MNumChildren; i++)
            {
                ProcessNode(new Ref<Assimp.Node>(node.Value.MChildren[i]), scene);
            }
        }

        public void Dispose()
        {
            foreach (Mesh ms in meshes)
            {
                ms.Dispose();
            }
        }
    }
}
