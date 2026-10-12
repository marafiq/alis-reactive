using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    internal static class ValidationDateLiteral
    {
        internal static object From(object value, Shape shape)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            if (shape == Shape.Instant && value is DateTimeOffset instant) return instant.ToString("O");
            if (shape != Shape.Date) return value;

            return FromDateValue(value);
        }

        private static object FromDateValue(object value)
        {
            if (value is DateTime dateTime)
            {
                var dateHasNoTimeComponent = dateTime.TimeOfDay == TimeSpan.Zero;
                if (dateHasNoTimeComponent)
                    return dateTime.ToString("yyyy-MM-dd");

                return dateTime.ToString("s");
            }

#if NET6_0_OR_GREATER
            if (value is DateOnly dateOnly)
                return dateOnly.ToString("yyyy-MM-dd");
#endif

            return value;
        }
    }
}
