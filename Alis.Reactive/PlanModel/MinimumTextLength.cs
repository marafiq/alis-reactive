using System;

namespace Alis.Reactive.PlanModel
{
    internal sealed class MinimumTextLength
    {
        private MinimumTextLength(int value)
        {
            Value = value;
        }

        internal int Value { get; }

        internal static MinimumTextLength From(int length, string parameterName)
        {
            if (parameterName == null) throw new ArgumentNullException(nameof(parameterName));
            if (length < 0)
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    length,
                    "Condition minimum text length must be zero or greater.");

            return new MinimumTextLength(length);
        }
    }
}
