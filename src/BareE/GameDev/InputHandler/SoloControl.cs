using System;
using System.Collections.Generic;

namespace BareE.GameDev
{
    public class SoloControl : InputControl
    {
        private InputAlias alias;

        public SoloControl(InputAlias btn)
        {
            alias = btn;
            Alias = btn.Alias;
        }

        public override IEnumerable<InputAlias> GetChildAliases()
        {
            yield return alias;
            yield break;
        }

        public override float GetControlValue(ref Dictionary<String, float> currentValues)
        {
            if (!currentValues.ContainsKey(alias.Alias))
                return 0.0f;
            return currentValues[alias.Alias] * (Invert ? -1 : 1);
        }
    }
}