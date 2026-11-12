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
    internal static readonly StringShape Instance = new();

    private StringShape()
    {
    }
}

internal sealed class NumberShape : Shape
{
    internal NumberShape(NumberOf of, NumberCulture text)
    {
        Of = of;
        Text = text;
    }

    public NumberOf Of { get; }

    public NumberCulture Text { get; }
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
    internal static readonly InstantShape Instance = new();

    private InstantShape()
    {
    }
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
    internal ObjectShape(IReadOnlyDictionary<string, Shape> fields)
    {
        Fields = fields;
    }

    public IReadOnlyDictionary<string, Shape> Fields { get; }
}
