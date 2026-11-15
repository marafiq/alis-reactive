using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(StringShape), "string")]
[JsonDerivedType(typeof(NumberShape), "number")]
[JsonDerivedType(typeof(BooleanShape), "boolean")]
[JsonDerivedType(typeof(DateShape), "date")]
[JsonDerivedType(typeof(InstantShape), "instant")]
[JsonDerivedType(typeof(FileShape), "file")]
[JsonDerivedType(typeof(AnyShape), "any")]
[JsonDerivedType(typeof(ArrayShape), "array")]
[JsonDerivedType(typeof(NullableShape), "nullable")]
[JsonDerivedType(typeof(ObjectShape), "object")]
internal abstract class Shape
{
    private protected Shape()
    {
    }
}

internal sealed class StringShape : Shape
{
    internal StringShape(DateMeaning date, string negativeZero)
    {
        Date = date;
        NegativeZero = negativeZero;
    }

    public DateMeaning Date { get; }

    public string NegativeZero { get; }
}

internal sealed class NumberShape : Shape
{
    internal NumberShape(NumberOf of, NumberCulture text, NumberOverflow overflow)
    {
        Of = of;
        Text = text;
        Overflow = overflow;
    }

    public NumberOf Of { get; }

    public NumberCulture Text { get; }

    public NumberOverflow Overflow { get; }
}

[JsonConverter(typeof(ChoiceConverter<NumberOf>))]
internal sealed class NumberOf : IChoice
{
    internal static readonly NumberOf Integer = new("integer");
    internal static readonly NumberOf Fraction = new("fraction");

    private NumberOf(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<NumberCulture>))]
internal sealed class NumberCulture : IChoice
{
    internal static readonly NumberCulture View = new("view");
    internal static readonly NumberCulture Invariant = new("invariant");

    private NumberCulture(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

internal sealed class BooleanShape : Shape
{
    internal static readonly BooleanShape Instance = new();

    private BooleanShape()
    {
    }
}

internal sealed class DateShape : Shape
{
    internal static readonly DateShape Instance = new();

    private DateShape()
    {
    }
}

internal sealed class InstantShape : Shape
{
    internal InstantShape(DateMeaning from)
    {
        From = from;
    }

    public DateMeaning From { get; }
}

internal sealed class FileShape : Shape
{
    internal static readonly FileShape Instance = new();

    private FileShape()
    {
    }
}

internal sealed class AnyShape : Shape
{
    internal static readonly AnyShape Instance = new();

    private AnyShape()
    {
    }
}

internal sealed class ArrayShape : Shape
{
    internal ArrayShape(Shape item)
    {
        Item = item;
    }

    public Shape Item { get; }
}

internal sealed class NullableShape : Shape
{
    internal NullableShape(Shape inner)
    {
        Inner = inner;
    }

    public Shape Inner { get; }
}

internal sealed class ObjectShape : Shape
{
    internal ObjectShape(IReadOnlyDictionary<string, FieldConversion> fields)
    {
        Fields = fields;
    }

    public IReadOnlyDictionary<string, FieldConversion> Fields { get; }
}

internal sealed class FieldConversion
{
    internal FieldConversion(Shape shape, FieldNothing nothing)
    {
        Shape = shape;
        Nothing = nothing;
    }

    public Shape Shape { get; }

    public FieldNothing Nothing { get; }
}

[JsonConverter(typeof(ChoiceConverter<DateMeaning>))]
internal sealed class DateMeaning : IChoice
{
    internal static readonly DateMeaning None = new("none");
    internal static readonly DateMeaning Date = new("date");
    internal static readonly DateMeaning Instant = new("instant");

    private DateMeaning(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<NumberOverflow>))]
internal sealed class NumberOverflow : IChoice
{
    internal static readonly NumberOverflow Nothing = new("nothing");
    internal static readonly NumberOverflow Infinity = new("infinity");

    private NumberOverflow(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<FieldNothing>))]
internal sealed class FieldNothing : IChoice
{
    internal static readonly FieldNothing Omit = new("omit");
    internal static readonly FieldNothing Null = new("null");

    private FieldNothing(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
