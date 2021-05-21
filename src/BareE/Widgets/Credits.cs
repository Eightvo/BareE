using BareE.GameDev;

using ImGuiNET;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BareE.Widgets
{
    using IG = ImGuiNET.ImGui;

    namespace BareE.Widgets
    {
        public enum AssetCreditType
        {
            General = 0,

            DONER_Graphics,
            DONER_Sound,
            DONER_Bread,

            TEAM_Graphics,
            TEAM_Sound,
            TEAM_Dev,

            THANKS
        }

        internal class creditSorter : IEqualityComparer<AssetCredits>, IComparer<AssetCredits>
        {
            public int Compare(AssetCredits x, AssetCredits y)
            {
                if (x.CreditType == y.CreditType)
                {
                    return x.Asset.CompareTo(y.Asset);
                }
                else
                {
                    return x.CreditType.CompareTo(y.CreditType);
                }
            }

            public bool Equals(AssetCredits x, AssetCredits y)
            {
                if (x.CreditType != y.CreditType) return false;
                if (x.Asset != y.Asset) return false;
                if (x.Filenames.Length != y.Filenames.Length) return false;
                if (x.Authors.Length != y.Authors.Length) return false;
                if (x.Licenses.Length != y.Authors.Length) return false;
                foreach (String f in x.Filenames)
                    if (!y.Filenames.Contains(f))
                        return false;
                foreach (String f in x.Authors)
                    if (!y.Authors.Contains(f))
                        return false;
                foreach (String f in x.Licenses)
                    if (!y.Licenses.Contains(f))
                        return false;

                return true;
            }

            public int GetHashCode(AssetCredits obj)
            {
                return $"{obj.CreditType}{obj.Asset}".GetHashCode();
            }
        }

        public struct AssetCredits
        {
            public String Asset { get; set; }

            public AssetCreditType CreditType { get; set; }
            public String[] Filenames { get; set; }
            public String[] Authors { get; set; }
            public String[] Licenses { get; set; }
            public String[] Urls { get; set; }
            public String[] Text { get; set; }
        }

        public class Credits
        {
            public string Name { get; set; } = "ImGuiCreditsScreen";
            public bool IsVisible { get; set; } = false;
            private HashSet<AssetCredits> _allCredits = new HashSet<AssetCredits>();

            private static IEnumerable<AssetCredits> FILEFREECREDITS()
            {
                yield return new AssetCredits()
                {
                    Asset = "BareE",
                    CreditType = AssetCreditType.TEAM_Dev,
                    Authors = new string[] { "Eightvo" },
                    //Text ="Thanks... someone"
                };
                yield return new AssetCredits()
                {
                    Asset = "Graphics / Textures / Sounds / Music",
                    CreditType = AssetCreditType.THANKS,
                    Urls = new string[] { "https://www.opengameart.org" }
                };
                yield return new AssetCredits()
                {
                    Asset = "Veldrid",
                    Authors = new string[] { "mellinoe" },
                    CreditType = AssetCreditType.THANKS,
                    Urls = new string[] { "http://veldrid.dev" }
                };
                yield return new AssetCredits()
                {
                    Asset = "ImageSharp",
                    Authors = new string[] { "SixLabors" },
                    CreditType = AssetCreditType.THANKS,
                    Urls = new string[] { "https://github.com/SixLabors/ImageSharp" }
                };
            }

            public Credits()
            {
                Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings()
                {
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Include
                };
                foreach (string creditFile in AssetManager.AllFiles("*.credits"))
                {
                    try
                    {
                        AssetCredits[] aca = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetCredits[]>(System.IO.File.ReadAllText(creditFile), settings);
                        foreach (var ac in aca)
                        {
                            if (ac.Text != null)
                                for (int i = 0; i < ac.Text.Length; i++)
                                {
                                    if (ac.Text[i].EndsWith(".txt"))
                                    {
                                        ac.Text[i] = System.IO.File.ReadAllText(ac.Text[i]);
                                    }
                                }

                            _allCredits.Add(ac);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.EmitError(e);
                    }
                }
                foreach (AssetCredits ac in FILEFREECREDITS())
                {
                    _allCredits.Add(ac);
                }
                //foreach(.);
                //Union(FILEFREECREDITS();
            }

            public void Render(Instant instant, GameState state, GameEnvironment env)
            {
                if (!IsVisible) return;
                int tPx = (int)(env.Window.Width * 0.1f);
                int tPy = (int)(env.Window.Height * 0.1f);

                IG.SetNextWindowPos(new System.Numerics.Vector2(tPx, tPy));
                IG.SetNextWindowSize(new System.Numerics.Vector2(tPx * 8, tPy * 8));
                IG.Begin("Credits", ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoSavedSettings);
                AssetCreditType cType = AssetCreditType.THANKS;
                foreach (AssetCredits credit in _allCredits.OrderBy(x => x, new creditSorter()))
                {
                    if (cType != credit.CreditType)
                    {
                        IG.Text("--------------------------");
                        IG.Text($"       {credit.CreditType.ToString().Replace("_", ": ")}");
                        IG.Text("--------------------------");
                        cType = credit.CreditType;
                    }
                    RenderCredits(credit);
                    IG.Text("");
                }

                if (IG.Button("Close"))
                {
                    IsVisible = false;
                }
                IG.End();
            }

            private void RenderCredits(AssetCredits credit)
            {
                StringBuilder authors = new StringBuilder();
                if (credit.Authors != null)
                    foreach (var a in credit.Authors)
                        authors.Append($"{(a.Length > 0 ? "" : ", ")}{a}");
                IG.Text($"{credit.Asset} by {authors.ToString()}");
                if (credit.Filenames != null)
                    foreach (var f in credit.Filenames)
                    {
                        if (String.IsNullOrWhiteSpace(f)) continue;
                        IG.Text($"    {f}");
                    }
                if (credit.Text != null)
                    foreach (var txt in credit.Text)
                    {
                        if (String.IsNullOrWhiteSpace(txt)) continue;
                        IG.Text(txt);
                    }
                if (credit.Urls != null)
                    foreach (var u in credit.Urls)
                    {
                        if (String.IsNullOrWhiteSpace(u)) continue;
                        UTIL.IGHelper.Link(u, u, new System.Numerics.Vector4(1, 0, 0, 1));
                        IG.Text("");
                    }
            }
        }
    }
}