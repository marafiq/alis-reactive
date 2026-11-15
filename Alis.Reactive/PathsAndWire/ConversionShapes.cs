using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.PathsAndWire;

internal static class ConversionShapes
{
    internal static StringShape TextOf(JsonTypeInfo source, CultureInfo culture)
    {
        if (source.IsList(out var item)) return TextOf(item, culture);
        var zero = source switch
        {
            JsonTypeInfo<double> or JsonTypeInfo<double?> => (-0d).ToString(culture),
            JsonTypeInfo<float> or JsonTypeInfo<float?> => (-0f).ToString(culture),
            JsonTypeInfo<decimal> or JsonTypeInfo<decimal?> => new decimal(0, 0, 0, true, 0).ToString(culture),
            _ => 0.ToString(culture),
        };
        return new StringShape(DateOf(source), zero);
    }

    private static DateMeaning DateOf(JsonTypeInfo source)
    {
        if (source.IsList(out var item)) return DateOf(item);
        return source switch
        {
            JsonTypeInfo<DateTime> or JsonTypeInfo<DateTime?> => DateMeaning.Date,
            JsonTypeInfo<DateTimeOffset> or JsonTypeInfo<DateTimeOffset?> => DateMeaning.Instant,
            _ => DateMeaning.None,
        };
    }

    internal static FieldNothing NothingOf(JsonTypeInfo member)
    {
        if (member.IsList(out _)) return FieldNothing.Omit;
        try
        {
            return JsonSerializer.Deserialize("null", member) is null ? FieldNothing.Null : FieldNothing.Omit;
        }
        catch (JsonException)
        {
            return FieldNothing.Omit;
        }
    }

    internal static NumberOverflow OverflowOf(JsonTypeInfo member)
    {
        var accepts = member switch
        {
            JsonTypeInfo<double> or JsonTypeInfo<double?> => double.TryParse("1e400", NumberStyles.Float, CultureInfo.InvariantCulture, out _),
            JsonTypeInfo<float> or JsonTypeInfo<float?> => float.TryParse("1e400", NumberStyles.Float, CultureInfo.InvariantCulture, out _),
            _ => false,
        };
        return accepts ? NumberOverflow.Infinity : NumberOverflow.Nothing;
    }
}
