using System;

namespace BareE.GameDev
{
    public struct InputAlias
    {
        public String Alias { get; set; }
        public InputSource Source { get; set; }
        public int SourceKey { get; set; }
    }
}