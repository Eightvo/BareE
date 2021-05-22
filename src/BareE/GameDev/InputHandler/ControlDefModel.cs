using System;

namespace BareE.GameDev
{
    /// <summary>
    /// Used for serialization
    /// </summary>
    public class ControlDefModel
    {
        public String Title { get; set; }
        public String Alias { get; set; }
        public InputSource Src { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "Type")]
        public String ControlType { get; set; }

        public String Def { get; set; }

        public float DZMin { get; set; }
        public float DZMax { get; set; }
    }
}