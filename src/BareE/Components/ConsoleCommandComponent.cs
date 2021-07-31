using BareE.DataStructures;
using BareE.GameDev;

using Newtonsoft.Json;

using System;

namespace BareE.Components
{
    [Component("MenubarItemSet")]
    public struct MenubarItemSet
    {
        public String SetName { get; set; }
        public MenubarItem[] Items { get; set; }
    }
    public struct MenubarItem
    {
        public String MenuIdentifierName { get; set; }

        [JsonIgnore]
        public Action<Instant, GameState, GameEnvironment> Callback { get; set; }

    }

    /// <summary>
    /// Defines a set of Console Commands.
    /// </summary>
    [Component("ConsoleCommandSet")]
    public struct ConsoleCommandSet
    {
        public String SetName { get; set; }

        public ConsoleCommand[] Commands;
    }

    /// <summary>
    /// Defines a console Command.
    /// </summary>
    public struct ConsoleCommand
    {
        public String Cmd { get; set; }

        [JsonProperty]
        public String HelpText { get; set; }

        [JsonIgnore]
        public Func<String, GameState, Instant, object[]> Callback { get; set; }
    }
}