using System;
using System.Collections.Generic;

namespace BareE.GameDev
{
    public class PairControl : InputControl
    {
        private InputAlias negativeAlias;
        private InputAlias positiveAlias;

        private String NegAlias
        { get { return $"{Alias}_Neg"; } }

        private String PosAlias
        { get { return $"{Alias}_Pos"; } }

        public PairControl(InputAlias neg, InputAlias pos)
        {
            Alias = neg.Alias;
            neg.Alias = NegAlias;
            pos.Alias = PosAlias;
            negativeAlias = neg;
            positiveAlias = pos;
        }

        public override IEnumerable<InputAlias> GetChildAliases()
        {
            yield return negativeAlias;
            yield return positiveAlias;
        }

        public override float GetControlValue(ref Dictionary<string, float> currentValues)
        {
            float posV = 0.0f;
            float negV = 0.0f;
            if (currentValues.ContainsKey(PosAlias))
                posV = currentValues[PosAlias];
            if (currentValues.ContainsKey(NegAlias))
                negV = currentValues[NegAlias];
            return (posV - negV) * (Invert ? -1 : 1);
        }
    }
}