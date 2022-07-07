using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Veldrid;
using Veldrid.ImageSharp;

namespace BareE
{
    /// <summary>
    /// Allows access to Resources.
    /// Resources can be Packaged into an assembly or a file in a specified asset repository.
    /// </summary>
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

        /// <summary>
        /// Add a new folder to search for assets within.
        /// </summary>
        /// <param name="location"></param>
        public static void AddAssetRepository(String location)
        {
            _assetRepositories.Add(location);
        }

        /// <summary>
        /// remove all non default asset repositories.
        /// </summary>
        public static void ResetAssetRepositories()
        {
            _assetRepositories.Clear();
            _assetRepositories.Add(Environment.CurrentDirectory);
        }

        /// <summary>
        /// Return a Veldird Device Texture from asset
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="device"></param>
        /// <param name="mipmap"></param>
        /// <returns></returns>
        public static Texture LoadTexture(String resource, GraphicsDevice device, bool mipmap = false, bool srgb=true)
        {
            var v = LoadImageSharpTexture(resource, mipmap, srgb);
            
            return LoadTexture(v, device);
        }

        public static Texture LoadTexture(SixLabors.ImageSharp.Image img, GraphicsDevice device, bool mipmap = false, bool srgb = true)
        {
            return LoadTexture(AssetManager.LoadImageSharpTexture(img, mipmap,srgb), device);
        }

        /// <summary>
        /// Return a Veldrid device texture from ImageSharpTexture.
        /// </summary>
        /// <param name="imgSharpTexture"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Texture LoadTexture(ImageSharpTexture imgSharpTexture, GraphicsDevice device)
        {
            var t = imgSharpTexture.CreateDeviceTexture(device, device.ResourceFactory);
            return t;
        }

        /// <summary>
        /// Return an Image SharpTexture from asset.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="mipmap"></param>
        /// <returns></returns>
        public static ImageSharpTexture LoadImageSharpTexture(String resource, bool mipmap = false, bool srgb = true)
        {
            var strm = new MemoryStream(FindFileData(resource));
            return (new Veldrid.ImageSharp.ImageSharpTexture(strm, mipmap, srgb));
        }

        public static ImageSharpTexture LoadImageSharpTexture(SixLabors.ImageSharp.Image img, bool mipmap=false, bool srgb=true)
        {
            byte[] data;
            using (MemoryStream strm = new MemoryStream())
            {
                img.Save(strm, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                data = strm.ToArray();
            }
            using (MemoryStream strm = new MemoryStream(data))
            {
                data = strm.ToArray();
                return new ImageSharpTexture(strm, mipmap, srgb);
            }
        }

        public static unsafe void UpdateTextureData(GraphicsDevice device, Texture tex, SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image)
        {
            if (!image.TryGetSinglePixelSpan(out Span<SixLabors.ImageSharp.PixelFormats.Rgba32> pixelSpan))
            {
                throw new VeldridException("Unable to get image pixelspan.");
            }
            fixed (void* pin = &MemoryMarshal.GetReference(pixelSpan))
            {
                device.UpdateTexture(
                    tex,
                    (IntPtr)pin,
                    (uint)(sizeof(byte) * 4 * image.Width * image.Height),
                    0,
                    0,
                    0,
                    (uint)image.Width,
                    (uint)image.Height,
                    1,
                    (uint)0,
                    0);
            }
        }

        /// <summary>
        /// Get a file stream from resource/Asset
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Stream FindFileStream(String name)
        {
            if (System.IO.File.Exists(name)) return new FileStream(name, FileMode.Open);
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
                                return new MemoryStream(data.ToArray());
                            }
                        }
                    }
                    catch (Exception e) { throw new FileNotFoundException("File not found", name, e); }
                }
            }
            throw new FileNotFoundException("File not found", name);
        }

        /// <summary>
        /// Get raw byte data from a Resource/Asset
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Byte[] FindFileData(String name)
        {
            if (File.Exists(name)) return File.ReadAllBytes(name);
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

        internal static IEnumerable<String> GetAssetsByPath(string currDirectory)
        {
            foreach (var asm in _knownAssemblies)
            {
                foreach (var v in asm.GetManifestResourceNames())
                    yield return v;
                
            }
        }

        /// <summary>
        /// Read an Entire asset into a string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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