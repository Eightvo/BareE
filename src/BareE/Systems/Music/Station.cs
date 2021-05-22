
using Newtonsoft.Json.Converters;

using System.Text.Json.Serialization;

namespace BareE.Systems
{
    public class Station
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RadioStationPlayOrder PlayOrder { get; set; }

        public int SongDelay { get; set; }
        public string[] PlayList { get; set; }
    }
}