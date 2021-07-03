using Assimp;

using BareE.EZRend.ImageShader.FullscreenTexture;
using BareE.EZRend.ModelShader.Color;

using System;
using System.Collections.Generic;
using System.Numerics;

using Veldrid;

namespace BareE.EZRend
{
    public static class ModelHelper
    {
        public static PostProcessSteps DefaultSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.FixInFacingNormals | PostProcessSteps.ForceGenerateNormals | PostProcessSteps.GenerateNormals | PostProcessSteps.Triangulate | PostProcessSteps.FlipWindingOrder;

        public static EZModel LoadTexturedMesh(String filename, PostProcessSteps pps, OutputDescription oDesc, GraphicsDevice device, Texture texture)
        {
            EZModel ret = new EZModel();
            AssimpContext ac = new AssimpContext();
            var scene = ac.ImportFile(filename, pps);
            for (int i = 0; i < scene.MeshCount; i++)
            {
                UvNormalShader uvn = new UvNormalShader();
                uvn.SetOutputDescription(oDesc);

                var mesh = scene.Meshes[i];
                foreach (var g in mesh.GetIndices())
                {
                    var posr = mesh.Vertices[g];
                    var normr = mesh.Normals[g];
                    var uvr = mesh.TextureCoordinateChannels[0][g];
                    Vector3 pt = new Vector3(posr.X, posr.Y, posr.Z);
                    Vector3 n = new Vector3(normr.X, normr.Y, normr.Z);
                    Vector2 uv = new Vector2(uvr.X, 1.0f - uvr.Y);
                    uvn.AddVertex(new Float3_Float2_Float3(pt, uv, n));
                }
                uvn.CreateResources(device);
                uvn.SetTexture(device, texture);
                ret.AddMesh(mesh.Name, uvn);
            }
            return ret;
        }

        public static EZModel LoadStaticTexturedBumpMesh(String filename, PostProcessSteps pps, OutputDescription oDesc, GraphicsDevice device, params Texture[] textures)
        {
            EZModel ret = new EZModel();
            AssimpContext ac = new AssimpContext();
            var scene = ac.ImportFile(filename, pps);
            LitUvNormalBump uvn = new LitUvNormalBump(textures.Length);
            uvn.SetOutputDescription(oDesc);
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var mesh = scene.Meshes[i];
                foreach (var g in mesh.GetIndices())
                {
                    var posr = mesh.Vertices[g];
                    var normr = mesh.Normals[g];
                    var fwdr = mesh.Tangents[g];
                    var bitanr = mesh.BiTangents[g];

                    var uvr = mesh.TextureCoordinateChannels[0][g];
                    Vector3 pt = new Vector3(posr.X, posr.Y, posr.Z);
                    Vector3 n = new Vector3(normr.X, normr.Y, normr.Z);
                    Vector3 f = new Vector3(fwdr.X, fwdr.Y, fwdr.Z);
                    Vector3 up = new Vector3(bitanr.X, bitanr.Y, bitanr.Z);
                    Vector2 uv = new Vector2(uvr.X, 1.0f - uvr.Y);

                    uvn.AddVertex(new Float3_Float3_Float3_Float3_Float2(pt, n, f, up, uv));
                }
            }
            uvn.CreateResources(device);
            for (int t = 0; t < textures.Length; t++)
            {
                uvn.SetTexture(t, device, textures[t]);
            }

            ret.AddMesh("mesg", uvn);
            return ret;
        }

        public static EZModel LoadTexturedBumpMesh(String filename, PostProcessSteps pps, OutputDescription oDesc, GraphicsDevice device, params Texture[] textures)
        {
            EZModel ret = new EZModel();
            AssimpContext ac = new AssimpContext();
            var scene = ac.ImportFile(filename, pps);
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var mesh = scene.Meshes[i];
                LitUvNormalBump uvn = new LitUvNormalBump(textures.Length);
                uvn.SetOutputDescription(oDesc);

                foreach (var g in mesh.GetIndices())
                {
                    var posr = mesh.Vertices[g];
                    var normr = mesh.Normals[g];
                    var fwdr = mesh.Tangents[g];
                    var bitanr = mesh.BiTangents[g];

                    var uvr = mesh.TextureCoordinateChannels[0][g];
                    Vector3 pt = new Vector3(posr.X, posr.Y, posr.Z);
                    Vector3 n = new Vector3(normr.X, normr.Y, normr.Z);
                    Vector3 f = new Vector3(fwdr.X, fwdr.Y, fwdr.Z);
                    Vector3 up = new Vector3(bitanr.X, bitanr.Y, bitanr.Z);
                    Vector2 uv = new Vector2(uvr.X, 1.0f - uvr.Y);

                    uvn.AddVertex(new Float3_Float3_Float3_Float3_Float2(pt, n, f, up, uv));
                }
                uvn.CreateResources(device);
                //uvn.SetTexture(device, texture);
                for (int t = 0; t < textures.Length; t++)
                {
                    uvn.SetTexture(t, device, textures[t]);
                }
                ret.AddMesh(mesh.Name, uvn);
            }

            return ret;
        }

        public static EZModel LoadStaticTexturedMesh(String filename, PostProcessSteps pps, OutputDescription oDesc, GraphicsDevice device, Texture texture)
        {
            EZModel ret = new EZModel();
            AssimpContext ac = new AssimpContext();
            var scene = ac.ImportFile(filename, pps);
            UvNormalShader uvn = new UvNormalShader();
            uvn.SetOutputDescription(oDesc);
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var mesh = scene.Meshes[i];
                foreach (var g in mesh.GetIndices())
                {
                    var posr = mesh.Vertices[g];
                    var normr = mesh.Normals[g];
                    var uvr = mesh.TextureCoordinateChannels[0][g];
                    Vector3 pt = new Vector3(posr.X, posr.Y, posr.Z);
                    Vector3 n = new Vector3(normr.X, normr.Y, normr.Z);
                    Vector2 uv = new Vector2(uvr.X, 1.0f - uvr.Y);
                    uvn.AddVertex(new Float3_Float2_Float3(pt, uv, n));
                }
            }
            uvn.CreateResources(device);
            uvn.SetTexture(device, texture);
            ret.AddMesh("mesg", uvn);
            return ret;
        }

        public static EZModel LoadStaticColoredMesh(String filename, PostProcessSteps pps, OutputDescription oDesc, GraphicsDevice device, Dictionary<Vector3, Vector3> clrMap)
        {
            EZModel ret = new EZModel();
            AssimpContext ac = new AssimpContext();
            var scene = ac.ImportFile(filename, pps);
            ColorNormalShader uvn = new ColorNormalShader();
            uvn.SetOutputDescription(oDesc);
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var mesh = scene.Meshes[i];
                foreach (var g in mesh.GetIndices())
                {
                    var posr = mesh.Vertices[g];
                    var normr = mesh.Normals[g];
                    Color4D uvr;
                    if (scene.HasMaterials)
                    {
                        uvr = scene.Materials[mesh.MaterialIndex].ColorDiffuse;
                    }
                    else
                    {
                        uvr = mesh.VertexColorChannels[0][g];
                    }
                    Vector3 uv = new Vector3(uvr.R, uvr.G, uvr.B);
                    float alpha = uvr.A;
                    if (clrMap.ContainsKey(uv))
                    {
                        uv = clrMap[uv];
                    }
                    Vector3 pt = new Vector3(posr.X, posr.Y, posr.Z);
                    Vector3 n = new Vector3(normr.X, normr.Y, normr.Z);
                    uvn.AddVertex(new Float3_Float4_Float3(pt, new Vector4(uv, alpha), n));
                }
            }
            uvn.CreateResources(device);
            ret.AddMesh("mesh", uvn);
            return ret;
        }

        public static EZModel LoadColoredMesh(String filename, PostProcessSteps pps, OutputDescription oDesc, GraphicsDevice device, Dictionary<Vector3, Vector3> clrMap)
        {
            EZModel ret = new EZModel();
            AssimpContext ac = new AssimpContext();
            var scene = ac.ImportFile(filename, pps);
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var mesh = scene.Meshes[i];
                ColorNormalShader uvn = new ColorNormalShader();
                uvn.SetOutputDescription(oDesc);
                foreach (var g in mesh.GetIndices())
                {
                    var posr = mesh.Vertices[g];
                    var normr = mesh.Normals[g];
                    Color4D uvr;
                    if (scene.HasMaterials)
                    {
                        uvr = scene.Materials[mesh.MaterialIndex].ColorDiffuse;
                    }
                    else
                    {
                        uvr = mesh.VertexColorChannels[0][g];
                    }
                    Vector3 uv = new Vector3(uvr.R, uvr.G, uvr.B);
                    float alpha = uvr.A;
                    if (clrMap.ContainsKey(uv))
                    {
                        uv = clrMap[uv];
                    }
                    Vector3 pt = new Vector3(posr.X, posr.Y, posr.Z);
                    Vector3 n = new Vector3(normr.X, normr.Y, normr.Z);
                    uvn.AddVertex(new Float3_Float4_Float3(pt, new Vector4(uv, alpha), n));
                }
                uvn.CreateResources(device);
                ret.AddMesh(mesh.Name, uvn);
            }
            return ret;
        }
    }
}