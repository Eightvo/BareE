using BareE.DataStructures;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LilScrapper.Components
{
    [Component("AdvSprite")]
    public struct AdvSprite
    {
        public String Root { get; set; }
        [JsonIgnore]
        public String Frame { get; set; }
        [JsonIgnore]
        public String Alias { get { return $"{Root}{Frame}"; } }
        public Vector4 PrimaryColor { get; set; }
        public Vector4 SecondaryColor { get; set; }
        public float Scale { get; set; }
    }
    [Component("AdvCompositeSprite")]
    public struct AdvCompositeSprite
    {
        [JsonIgnore]
        public String Frame { get; set; }
        public ACSData[] Children { get; set; }
    }

    public struct ACSData
    {
        public String Root { get; set; }
        public Vector4 PrimaryColor { get; set; }
        public Vector4 SecondaryColor { get; set; }
        public float Scale { get; set; }
    }

}
