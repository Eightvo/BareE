using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;

using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonConverterAttribute = Newtonsoft.Json.JsonConverterAttribute;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace BareE.DataStructures
{
    public class SpriteAtlas
    {
        private class AtlasPage
        {
            public SpriteModel model;
            public Image src;
            public Veldrid.Rectangle PageRect;
        }
        public bool AutoPad { get; set; } = true;
        private bool _dirty;
        public bool Dirty { get { return _dirty; } }
        public Image<Rgba32> AtlasSheet;
        private Dictionary<String, AtlasPage> _Pages = new Dictionary<string, AtlasPage>(StringComparer.CurrentCultureIgnoreCase);

        public IEnumerable<String> Pages
        {
            get { return _Pages.Keys; }
        }

        public void Merge(String name, Image<Rgba32> image, System.Numerics.Vector4 subRec)
        {
            Merge(name, image, new Rectangle((int)subRec.X, (int)subRec.Y, (int)subRec.Z, (int)subRec.W));
        }

        public void Merge(String name, Image<Rgba32> image, Rectangle subRec)
        {
            var tSpriteModel = new SpriteModel()
            {
                Name = name,
                SrcRect = subRec
            };
            Merge(name, tSpriteModel, image);
        }
        public void Merge(String name, Image<Rgba32> image)
        {
            var tSpriteModel = new SpriteModel()
            {
                Name = name,
                SrcRect = new Rectangle(0, 0, image.Width, image.Height)

            };
            Merge(name, tSpriteModel, image);
        }

        public void Merge(String name, SpriteModel sprite, Image<Rgba32> image, float scale=1.0f)
        {
            AtlasPage newPage = new AtlasPage();
            newPage.model = sprite;
            if (scale != 1) newPage.model.Scale(scale);


            newPage.src = image;



            if (!_Pages.ContainsKey(name))
                _Pages.Add(name, newPage);
            else
                _Pages[name] = newPage;

            _dirty = true;

        }


        public void Merge(String name, SpriteModel sprite, String TextureSrc, float scale=1.0f)
        {
            Merge(name, sprite,GetTextureSrcImage(sprite, TextureSrc),scale);
        }

        public Image<Rgba32> GetTextureSrcImage(SpriteModel sprite, String TextureSrc)
        {
            using (var strm = AssetManager.FindFileStream(TextureSrc))
            {
                Image<Rgba32> img = Image.Load<Rgba32>(strm);
                if (sprite.SrcRect.Left <= 0 && sprite.SrcRect.Width >= img.Width && sprite.SrcRect.Bottom <= 0 && sprite.SrcRect.Top >= img.Height)
                    return img;
                

                img.Mutate(x =>
                {
                    
                    x.Crop(new Rectangle(Math.Max(0, sprite.SrcRect.X),
                                                              Math.Max(0, sprite.SrcRect.Y),
                                                              Math.Min(img.Width, sprite.SrcRect.X+sprite.SrcRect.Width),
                                                              Math.Min(img.Height, sprite.SrcRect.Y + sprite.SrcRect.Height)));
                });
                return img;
            }
        }

        public void Build(int maxWidth = 0, bool save = false)
        {
            Build(maxWidth, save ? $"newImage{ DateTime.Now.ToString("yyyyMMddhhmmss")}.png" : String.Empty);
        }
        public void Build(int maxWidth = 0, String saveFile = null)
        {
            var maxW = 0;
            foreach (var v in _Pages)
            {
                var w = (v.Value.src.Width+2);
                if (w > maxW)
                    maxW = w;
            }
            if (maxWidth < maxW)
                maxWidth = maxW;

            SpatialPartion layout = new SpatialPartion(new Veldrid.Rectangle(0, 0, maxWidth == 0 ? int.MaxValue : maxWidth, int.MaxValue));
            foreach (var v in _Pages)
            {
                v.Value.PageRect = layout.AddRectangle(new System.Drawing.Size(v.Value.src.Width+1, v.Value.src.Height+1));
            }
            layout.Crop();
            Image<Rgba32> newImage = new Image<Rgba32>((int)MathHelper.NxtPowerOfTwo(layout.Space.Width), (int)MathHelper.NxtPowerOfTwo(layout.Space.Height));
            foreach (var v in _Pages)
            {
                newImage.Mutate<Rgba32>(x =>
                {
                    GraphicsOptions opts = new GraphicsOptions()
                    {
                        Antialias = false,
                        AlphaCompositionMode = PixelAlphaCompositionMode.Src
                    };
                    x.DrawImage(v.Value.src, new Point(v.Value.PageRect.X, v.Value.PageRect.Y), opts);
                });
            }
            AtlasSheet = newImage;
            if (!String.IsNullOrEmpty(saveFile))
                AtlasSheet.Save(saveFile);
            _dirty = false;
        }

        private RectangleF GetSpriteRect(AtlasPage page, String key)
        {
            if (String.IsNullOrEmpty(key))
            {
                return new RectangleF(page.PageRect.X / (float)AtlasSheet.Width,
                                      page.PageRect.Y / (float)AtlasSheet.Height,
                                      page.PageRect.Width / (float)AtlasSheet.Width,
                                      page.PageRect.Height / (float)AtlasSheet.Height
                    );
            }
            var r = page.model[key];
            return new RectangleF((page.PageRect.X + r.X) / (float)AtlasSheet.Width,
                                  (page.PageRect.Y + r.Y) / (float)AtlasSheet.Height,
                                  (r.Width) / (float)AtlasSheet.Width,
                                  (r.Height) / (float)AtlasSheet.Height
                );
        }

        public RectangleF this[String query]
        {
            get
            {
                string key = query;
                string subKey = String.Empty;
                int dIndx = query.IndexOf('.');
                if (dIndx > 0)
                {
                    key = query.Substring(0, dIndx);
                    subKey = query.Substring(dIndx + 1, query.Length - (dIndx + 1));
                }
                if (_Pages.ContainsKey(key))
                {
                    return GetSpriteRect(_Pages[key], subKey);
                }

                return new RectangleF(0, 0, 0, 0);
            }
        }
        public System.Numerics.Vector2 EstimateOriginalSize(String query)
        {
            return EstimateOriginalSize(this[query]);
        }
        public System.Numerics.Vector2 EstimateOriginalSize(RectangleF uvBox)
        {
            return EstimateOriginalSize((float)uvBox.Width, (float)uvBox.Height);
        }

        public System.Numerics.Vector2 EstimateOriginalSize(System.Numerics.Vector2 dimentions)
        {
            return EstimateOriginalSize(dimentions.X, dimentions.Y);
        }
        public System.Numerics.Vector2 EstimateOriginalSize(float uvWidth, float uvHeight)
        {
            var ogWidth = uvWidth*(float)AtlasSheet.Width;
            var ogHeight = uvHeight*(float)AtlasSheet.Height;
            return new System.Numerics.Vector2(ogWidth, ogHeight);
        }

        public class SpriteModel
        {
            public String Name { get; set; }
            
            [JsonProperty("Children")]
            private SpriteModel[] _Children { get; set; }
            public void SetChildren(SpriteModel[] ch) { _Children = ch; }
            [JsonIgnore]
            public IEnumerable<SpriteModel> Children
            {
                get
                {
                    if (_Children == null) yield break;
                    foreach (var model in _Children)
                        yield return model;
                }
            }

            [JsonConverter(typeof(RectangleJsonConverter))]
            public Rectangle SrcRect { get; set; }

            [JsonProperty("Alternatives")]
            private SpriteAlternativeModel[] _Alternatives { get; set; }

            [JsonIgnore]
            public IEnumerable<SpriteAlternativeModel> Alternatives
            {
                get
                {
                    if (_Alternatives == null) yield break;
                    foreach (var v in _Alternatives)
                        yield return v;
                }
            }

            [JsonIgnore]
            public Rectangle this[String query]
            {
                get
                {
                    if (String.IsNullOrEmpty(query))
                        return SrcRect;
                    string key = query;
                    string subKey = String.Empty;
                    int dIndx = query.IndexOf('.');
                    if (dIndx > 0)
                    {
                        key = query.Substring(0, dIndx);
                        subKey = query.Substring(dIndx + 1, query.Length - (dIndx + 1));
                    }

                    foreach (var ch in Children)
                    {
                        if (String.Compare(ch.Name, key) == 0)
                            return ch[subKey];
                    }

                    foreach (var alt in Alternatives)
                    {
                        if (String.Compare(alt.Name, key) == 0)
                        {
                            var r = this[subKey];
                            return new Rectangle(r.X + alt.Off.X, r.Y + alt.Off.Y, r.Width, r.Height);
                        }
                    }

                    return new Rectangle(0, 0, 0, 0);
                }
            }
            public void Scale(float scale)
            {
                SrcRect = new Rectangle((int)(SrcRect.X * scale), (int)(SrcRect.Y * scale),(int)(SrcRect.Width * scale),(int)(SrcRect.Height * scale));
                foreach(var c in _Children)
                {
                    c.Scale(scale);
                }
                foreach(var a in _Alternatives)
                {
                    a.Off = new Point((int)(a.Off.X * scale), (int)(a.Off.Y * scale));
                }
            }
            public static SpriteModel LoadSpriteModel(String filename)
            {
                return JsonConvert.DeserializeObject<SpriteModel>(
                    System.IO.File.ReadAllText(filename)
                );
            }

            public static Dictionary<String, SpriteModel> LoadSpriteModels(String filename, StringComparer cmp = null)
            {
                var src = System.IO.File.ReadAllText(filename);
                return LoadSpriteModelsFromSrc(src);
            }
            public static Dictionary<String, SpriteModel> LoadSpriteModelsFromSrc(String src)
            {
                Dictionary<String, SpriteModel> ret = new Dictionary<string, SpriteModel>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var v in JsonConvert.DeserializeObject<SpriteModel[]>(src))
                {
                    ret.Add(v.Name, v);
                }
                return ret;

            }
        }

        public class SpriteAlternativeModel
        {
            public String Name { get; set; }

            [JsonConverter(typeof(PoingJsonConverter))]
            public Point Off { get; set; }
        }

 

        public class RectangleJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(Rectangle));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                //JObject jo = JObject.Load(reader);
                JToken jo = JToken.Load(reader);
                if (jo.Type == JTokenType.String)
                {
                    //0,0,660,64
                    string[] parts = jo.Value<String>().Split(',');
                    return new Rectangle(
                        int.Parse(parts[0]),
                        int.Parse(parts[1]),
                        int.Parse(parts[2]),
                        int.Parse(parts[3])
                        );
                }

                return new Rectangle(jo["left"] != null ? (int)jo["left"] : 10,
                                     jo["top"] != null ? (int)jo["top"] : 10,
                                     (int)jo["width"], (int)jo["height"]);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Rectangle rect = (Rectangle)value;
                JObject jo = new JObject();
                jo.Add("left", rect.Left);
                jo.Add("top", rect.Top);
                jo.Add("width", rect.Width);
                jo.Add("height", rect.Height);
                jo.WriteTo(writer);
            }
        }

        public class SizeJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(Size));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Size size = (Size)value;
                JObject jo = new JObject();
                jo.Add("width", size.Width);
                jo.Add("height", size.Height);
                jo.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jo = JObject.Load(reader);
                return new Size((int)jo["width"], (int)jo["height"]);
            }
        }

        public class PoingJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(Point));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Point pt = (Point)value;
                JObject jo = new JObject();
                jo.Add("X", pt.X);
                jo.Add("Y", pt.Y);
                jo.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JToken jo = JToken.Load(reader);
                if (jo.Type == JTokenType.String)
                {
                    var sVal = jo.Value<String>();
                    int sIndx = 0;
                    int eIndx = sVal.Length;
                    if (sVal.IndexOf('(') > 0)
                        sIndx = sVal.IndexOf('(') + 1;
                    if (sVal.IndexOf(')') > 0)
                        eIndx = sVal.IndexOf(')');
                    sVal = sVal.Substring(sIndx, eIndx - sIndx);
                    string[] parts = sVal.Split(',');
                    return new Point(
                        int.Parse(parts[0]),
                        int.Parse(parts[1])
                        );
                }

                return new Point((int)jo["X"], (int)jo["Y"]);
            }
        }
    }
}