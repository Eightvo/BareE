using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Veldrid;
using Veldrid.ImageSharp;

namespace BareE
{
    public static class AssetManager
    {
        private static List<String> _assetRepositories;
        private static List<Assembly> _knownAssemblies;

        static AssetManager()
        {
            _assetRepositories = new List<string>();
            _assetRepositories.Add(Environment.CurrentDirectory);
            _knownAssemblies = new List<Assembly>();
            _knownAssemblies.Add(System.Reflection.Assembly.GetEntryAssembly());
            foreach (var asmN in Assembly.GetEntryAssembly().GetReferencedAssemblies())
                _knownAssemblies.Add(Assembly.Load(asmN));
        }

        public static void AddAssetRepository(String location)
        {
            _assetRepositories.Add(location);
        }

        public static void ResetAssetRepositories()
        {
            _assetRepositories.Clear();
            _assetRepositories.Add(Environment.CurrentDirectory);
        }

        public static Texture LoadTexture(String resource, GraphicsDevice device, bool mipmap = false)
        {
            return LoadTexture(LoadImageSharpTexture(resource, mipmap), device);
        }

        public static Texture LoadTexture(ImageSharpTexture imgSharpTexture, GraphicsDevice device)
        {
            return imgSharpTexture.CreateDeviceTexture(device, device.ResourceFactory);
        }

        public static ImageSharpTexture LoadImageSharpTexture(String resource, bool mipmap = false)
        {
            var strm = new MemoryStream(FindFileData(resource));
            return (new Veldrid.ImageSharp.ImageSharpTexture(strm, mipmap));
        }

        public static Stream FindFileStream(String name)
        {
            if (Path.IsPathRooted(name))
                name = name.Substring(Path.GetPathRoot(name).Length);
            if (name.StartsWith("assets", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var assetRepoPath in _assetRepositories)
                {
                    var aP = Path.Combine(assetRepoPath, name);
                    if (File.Exists(aP))
                    {
                        return new FileStream(aP, FileMode.Open);
                    }
                }
            }
            else
            {
                foreach (var asm in new List<System.Reflection.Assembly>() { System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.Assembly.GetExecutingAssembly() })
                {
                    try
                    {
                        List<byte> data = new List<byte>();
                        using (var rdr = asm.GetManifestResourceStream(name))
                        {
                            if (rdr != null)
                            {
                                int nxt = rdr.ReadByte();
                                while (nxt != -1)
                                {
                                    data.Add((byte)nxt);
                                    nxt = rdr.ReadByte();
                                }
                                return new MemoryStream(data.ToArray());
                            }
                        }
                    }
                    catch (Exception e) { throw new FileNotFoundException("File not found", name, e); }
                }
            }
            throw new FileNotFoundException("File not found", name);
        }

        public static Byte[] FindFileData(String name)
        {
            if (Path.IsPathRooted(name))
                name = name.Substring(Path.GetPathRoot(name).Length);
            if (name.StartsWith("assets", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var assetRepoPath in _assetRepositories)
                {
                    var aP = Path.Combine(assetRepoPath, name);
                    if (File.Exists(aP))
                    {
                        return File.ReadAllBytes(aP);
                    }
                }
            }
            else
            {
                foreach (var asm in _knownAssemblies)
                {
                    try
                    {
                        List<byte> data = new List<byte>();
                        using (var rdr = asm.GetManifestResourceStream(name))
                        {
                            if (rdr != null)
                            {
                                int nxt = rdr.ReadByte();
                                while (nxt != -1)
                                {
                                    data.Add((byte)nxt);
                                    nxt = rdr.ReadByte();
                                }
                                return data.ToArray();
                            }
                        }
                    }
                    catch (Exception e) { throw new FileNotFoundException("File not found", name, e); }
                }
            }
            throw new FileNotFoundException($"File not found: {name}", name);
        }

        public static String ReadFile(string name)
        {
            return new StreamReader(new MemoryStream(FindFileData(name))).ReadToEnd();
        }

        /// <summary>
        /// App manifest only matches extentions
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        /// <param name="searchAssets"></param>
        /// <returns></returns>
        public static IEnumerable<String> AllFiles(string path, string pattern, bool searchAssets = true)
        {
            HashSet<String> all = new HashSet<string>();
            foreach (var v in System.IO.Directory.EnumerateFiles(path, pattern, System.IO.SearchOption.AllDirectories))
            {
                if (all.Add(v))
                    yield return v;
            }
            if (!searchAssets) yield break;
            foreach (var p in _assetRepositories)
            {
                foreach (var v in System.IO.Directory.EnumerateFiles(p, pattern, System.IO.SearchOption.AllDirectories))
                {
                    if (all.Add(v))
                        yield return v;
                }
            }
            var ext = System.IO.Path.GetExtension(pattern);
            foreach (var asm in new List<System.Reflection.Assembly>() { System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.Assembly.GetExecutingAssembly() })
            {
                foreach (var rn in asm.GetManifestResourceNames())
                {
                    var rne = System.IO.Path.GetExtension(rn);
                    if (String.Compare(rne, ext, true) == 0 || pattern == "*.*")
                        if (String.Compare(rne, ".dll", true) != 0)
                            yield return rn;
                }
            }
            yield break;
        }

        public static IEnumerable<String> AllFiles(string pattern)
        {
            return AllFiles("./", pattern);
        }
    }
}