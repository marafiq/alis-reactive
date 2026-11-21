using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(DateShape), "date")]
[JsonDerivedType(typeof(InstantShape), "instant")]
[JsonDerivedType(typeof(NullableShape), "nullable")]
[JsonDerivedType(typeof(ArrayShape), "array")]
[JsonDerivedType(typeof(ObjectShape), "object")]
internal abstract class Shape
{
    private protected Shape()
    {
    }
}

internal sealed class DateShape : Shape
{
    private DateShape()
    {
    }

    internal static DateShape Instance { get; } = new();
}

internal sealed class InstantShape : Shape
{
    private InstantShape()
    {
    }

    internal static InstantShape Instance { get; } = new();
}

internal sealed class NullableShape : Shape
{
    internal NullableShape(Shape inner)
    {
        Inner = inner;
    }

    public Shape Inner { get; }
}

internal sealed class ArrayShape : Shape
{
    internal ArrayShape(Shape item)
    {
        Item = item;
    }

    public Shape Item { get; }
}

internal sealed class ObjectShape : Shape
{
    internal ObjectShape(IReadOnlyDictionary<string, Shape> fields)
    {
        Fields = fields;
    }

    public IReadOnlyDictionary<string, Shape> Fields { get; }
}
