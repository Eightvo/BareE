using System.Collections.Generic;

namespace BareE.GameDev
{
    public class MouseWheelAxis : InputControl
    {
        private InputAlias iAlias;

        public MouseWheelAxis(InputAlias alias)
        {
            Alias = alias.Alias;
            iAlias = alias;
        }

        public override float GetControlValue(ref Dictionary<string, float> currentValues)
        {
            float posV = 0.0f;
            float negV = 0.0f;
            if (currentValues.ContainsKey(Alias))
                posV = currentValues[Alias];
            //currentValues[Alias] = 0;
            return posV;
        }

        public override IEnumerable<InputAlias> GetChildAliases()
        {
            yield return iAlias;
            yield break;
        }
    }
}