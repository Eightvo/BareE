using System;
using System.Collections.Generic;
using System.Numerics;

namespace BareE.GameDev
{
    public abstract class InputControl
    {
        public String Alias { get; set; }
        public bool Invert { get; set; } = false;
        public Vector2 DeadZoneBounds { get; set; } = new Vector2(0.1f, 0.9f);

        public abstract IEnumerable<InputAlias> GetChildAliases();

        public abstract float GetControlValue(ref Dictionary<String, float> currentValues);
    }
}