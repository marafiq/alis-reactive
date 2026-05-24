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
            if (shape != Shape.Date) return value;

            return FromDateValue(value);
        }

        private static object FromDateValue(object value)
        {
            if (value is DateTime dt)
            {
                var dateHasNoTimeComponent = dt.TimeOfDay == TimeSpan.Zero;
                if (dateHasNoTimeComponent)
                    return dt.ToString("yyyy-MM-dd");

                return dt.ToString("s");
            }

            if (value is DateTimeOffset dto)
            {
                var dateHasNoTimeComponent = dto.TimeOfDay == TimeSpan.Zero;
                if (dateHasNoTimeComponent)
                    return dto.ToString("yyyy-MM-dd");

                return dto.ToString("s");
            }

#if NET6_0_OR_GREATER
            if (value is DateOnly d)
                return d.ToString("yyyy-MM-dd");
#endif

            return value;
        }
    }
}
