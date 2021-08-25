using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Veldrid;

using IG = ImGuiNET.ImGui;
namespace BareE.Widgets.OpenFileDialog
{

    public struct GetFilepathDialogSettings
    {
        public String RootDirectory { get; set; }
        public String DefaultExtention { get; set; }
        public bool AllowAccessOutsideRoot { get; set; }
        public bool AllowAccessSubdirectories { get; set; } 
        public bool AllowCreateFolder { get; set; }
        public bool AllowCreateFile { get; set; }
        public bool AllowSelectFolder { get; set; }
        public bool AllowAccessEmbedded { get; set; }
        
    }

    public class GetFilepathDialog:IWidget
    {
        static int ofdV = 0;
        String MyId = $"ofd{ofdV}";

        private bool closing;
        public DialogResult Result { get; set; }= DialogResult.Cancel;

        //String _selectedFullPath = String.Empty;
        public String SelectedFullPath {
            get {
                if (IsEmbeddedAsset)
                    return currFile;
                var v = CurrDirectory;
                if (String.IsNullOrEmpty(currFile))
                    if (Settings.AllowSelectFolder)
                        return v;
                    else
                        return "INVALID";
                v = System.IO.Path.Combine(v, currFile);
                if (!System.IO.Path.HasExtension(v) && !String.IsNullOrEmpty(Settings.DefaultExtention))
                    System.IO.Path.ChangeExtension(v, Settings.DefaultExtention);
                return v;
            } 
        }
        public bool Closed { get; private set; }
        public bool IsEmbeddedAsset { get; private set; }
        bool IsFilePath = true;

        public GetFilepathDialogSettings Settings { get; set; }
        Action<Instant, GameState, GameEnvironment, GetFilepathDialog> Callback { get; set; }
        bool DirectoryAllowed(String dir)
        {
            if (String.IsNullOrEmpty(dir)) return false;
            if (!System.IO.Path.IsPathFullyQualified(dir) || dir.Contains(".."))
                return DirectoryAllowed(System.IO.Path.GetFullPath(dir));

            if (!System.IO.Directory.Exists(dir))
                return false;

            if (Settings.AllowAccessOutsideRoot)
                return true;
            if (!dir.StartsWith(Settings.RootDirectory, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!Settings.AllowAccessSubdirectories)
                return String.Compare(dir, Settings.RootDirectory, true) == 0;
            return true;
        }
        String _currentDir;
        String CurrDirectory { get { return _currentDir; }set {
                if (DirectoryAllowed(value))
                    _currentDir = value;
            } }

        String currFile;
        String currExt;
        public GetFilepathDialog(Action<Instant, GameState, GameEnvironment, GetFilepathDialog> callback, GetFilepathDialogSettings settings= default)
        {
            if (String.IsNullOrEmpty(settings.RootDirectory)) settings.RootDirectory = Environment.CurrentDirectory;
            if (String.IsNullOrEmpty(settings.DefaultExtention)) settings.DefaultExtention = ".*";

            Callback = callback;
            Settings = settings;
            CurrDirectory = Settings.RootDirectory;
            currFile = String.Empty;
            currExt = Settings.DefaultExtention;
        }
        public void Update(Instant Instant, GameState State, GameEnvironment Env) 
        {
            if (this.closing)
            {
                this.Closed = true;
                this.closing = false;
                Callback(Instant, State, Env, this);
            }
        }

        public void RenderPathButtons(String path)
        {
            if (path == null) return;// String.Empty;
            var d = System.IO.Path.GetDirectoryName(path);
            if (!DirectoryAllowed(path))
            {
                IG.Text(path);
                IG.SameLine();
                return;
            }
            RenderPathButtons(d);
            var e = path;
            if (d != null)
                e = e.Substring(d.Length);
                if (IG.Button(e))
                {
                    CurrDirectory = path;
                }
            
            IG.SameLine();

        }


        public bool IsAssetLeaf(String path)
        {
            if (path.IndexOf(".") == path.LastIndexOf(".")) return true;
            return false;
        }
        public void Render(Instant Instant, GameState State, GameEnvironment Env, Framebuffer outbuffer, CommandList cmds)
        {
            if (closing || Closed) return;

            IG.Begin($"OpenFileDialog##{MyId}");
            //            if (IsFilePath)

            if (IG.BeginTabBar("FileAttrBar"))
            {
                if (IG.BeginTabItem("File"))
                {
                    RenderPathButtons(CurrDirectory);
                    //          else
                    //              throw new NotImplementedException();
                    IG.NewLine();
                    IG.BeginChild($"##frame{MyId}", new System.Numerics.Vector2(IG.GetWindowWidth(), IG.GetWindowHeight() * 0.75f), true);


                    //foreach(var v in System.IO.Directory.EnumerateFileSystemEntries(currDirectory))
                    if (DirectoryAllowed(System.IO.Path.Combine(CurrDirectory, "..\\")))
                    {
                        if (IG.Button("..\\"))
                        {
                            CurrDirectory = System.IO.Path.GetDirectoryName(CurrDirectory);
                            currFile = String.Empty;
                            IsEmbeddedAsset = false;
                        }
                    }
                    foreach (var v in System.IO.Directory.GetDirectories(CurrDirectory))
                    {
                        if (!DirectoryAllowed(v))
                            continue;
                        var fn = System.IO.Path.GetFileName(v);
                        if (IG.Button(fn))
                        {
                            CurrDirectory = v;
                            currFile = String.Empty;
                            IsEmbeddedAsset = false;
                        }
                    }

                    foreach (var v in System.IO.Directory.GetFiles(CurrDirectory))
                    {
                        var fn = System.IO.Path.GetFileName(v);
                        IG.Text(" "); IG.SameLine();
                        if (UTIL.IGHelper.Link(fn, new System.Numerics.Vector4(1, 1, 1, 1)))
                        {
                            CurrDirectory = System.IO.Path.GetDirectoryName(v);
                            currFile = fn;
                            IsEmbeddedAsset = false;
                        }
                    }

                    IG.EndChild();
                    IG.EndTabItem();
                }
                if (Settings.AllowAccessEmbedded)
                {
                    if (IG.BeginTabItem("Embedded"))
                    {
                        IG.BeginChild($"##frame{MyId}", new System.Numerics.Vector2(IG.GetWindowWidth(), IG.GetWindowHeight() * 0.75f), true);
                        foreach (var v in AssetManager.GetAssetsByPath(CurrDirectory))
                        {
                            if (v.Contains(currFile, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (UTIL.IGHelper.Link(v, new System.Numerics.Vector4(1, 1, 1, 1)))
                                {
                                    currFile = v;
                                    IsEmbeddedAsset = true;
                                }
                            }
                        }
                        IG.EndChild();
                        IG.EndTabItem();
                    }
                }
                IG.EndTabBar();
            }
            if (IG.InputText($"##tb1{MyId}", ref currFile, 255, ImGuiNET.ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (!System.IO.Path.IsPathRooted(currFile))
                    currFile = System.IO.Path.Combine(CurrDirectory, currFile);
                var d = System.IO.Path.GetDirectoryName(currFile);
                if (!DirectoryAllowed(d))
                    currFile = String.Empty;
                else
                {
                    CurrDirectory = d;
                    currFile = System.IO.Path.GetFileName(currFile);
                }
            }

            if (IG.Button("Ok"))
            {
                this.Result = DialogResult.Ok;
                this.closing= true;
                //Callback(Instant, State, Env, this);
            }IG.SameLine();
            if (IG.Button("Nope"))
            {
                this.Result = DialogResult.Cancel;
                this.closing = true;
                //Callback(Instant, State, Env, this);
            }

            IG.End();
        }

    }

}
