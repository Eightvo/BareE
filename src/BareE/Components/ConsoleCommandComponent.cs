using BareE.DataStructures;
using BareE.GameDev;

using Newtonsoft.Json;

using System;

namespace BareE.Components
{
    [Component("ConsoleCommandSet")]
    public struct ConsoleCommandSet
    {
        public String SetName { get; set; }

        public ConsoleCommand[] Commands;
    }

    public struct ConsoleCommand
    {
        //Action<object[], object[]>
        public String Cmd { get; set; }

        [JsonProperty]
        public String HelpText { get; set; }

        [JsonIgnore]
        public Func<String, GameState, Instant, object[]> Callback { get; set; }
    }
}