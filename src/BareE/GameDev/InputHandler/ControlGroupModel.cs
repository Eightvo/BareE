using System;

namespace BareE.GameDev
{
    /// <summary>
    /// Used for serialization
    /// </summary>
    public class ControlGroupModel
    {
        public String Group { get; set; }
        public ControlDefModel[] Controls { get; set; }
    }
}