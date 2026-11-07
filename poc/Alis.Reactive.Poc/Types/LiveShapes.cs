using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Types;

internal abstract class LiveShape
{
    private protected LiveShape()
    {
    }

    internal static LiveShape AsWritten { get; } = new Written();

    private sealed class Written : LiveShape
    {
    }
}

internal sealed class ConvertedShape : LiveShape
{
    internal ConvertedShape(Shape shape)
    {
        Shape = shape;
    }

    internal Shape Shape { get; }
}

internal static class LiveShapes
{
    internal static LiveShape Of(JsonTypeInfo type, JsonSerializerOptions options) => new Walk(options).Of(type, "$");

    private sealed class Walk(JsonSerializerOptions options)
    {
        private readonly HashSet<JsonTypeInfo> _path = [];
        private readonly HashSet<JsonTypeInfo> _heldAgain = [];

        internal LiveShape Of(JsonTypeInfo type, string at)
        {
            switch (type)
            {
                case JsonTypeInfo<DateTime>:
                    return new ConvertedShape(DateShape.Instance);
                case JsonTypeInfo<DateTime?>:
                    return new ConvertedShape(new NullableShape(DateShape.Instance));
                case JsonTypeInfo<DateTimeOffset>:
                    return new ConvertedShape(InstantShape.Instance);
                case JsonTypeInfo<DateTimeOffset?>:
                    return new ConvertedShape(new NullableShape(InstantShape.Instance));
            }

            if (!_path.Add(type))
            {
                _heldAgain.Add(type);
                return LiveShape.AsWritten;
            }

            try
            {
                var shape = type.Kind switch
                {
                    JsonTypeInfoKind.Enumerable => List(type, at),
                    JsonTypeInfoKind.Object => Object(type, at),
                    JsonTypeInfoKind.Dictionary => Dictionary(type, at),
                    _ => LiveShape.AsWritten,
                };
                if (shape is ConvertedShape && _heldAgain.Contains(type))
                {
                    throw new InvalidOperationException(
                        $"A value written into a live member holds its own type{Where(at)} and a date in it. A date reaches a live member through a shape that ends: pass the dates as a list of their own.");
                }

                return shape;
            }
            finally
            {
                _path.Remove(type);
            }
        }

        private LiveShape List(JsonTypeInfo type, string at)
            => Of(options.GetTypeInfo(type.ElementType!), at + "[]") is ConvertedShape item ? new ConvertedShape(new ArrayShape(item.Shape)) : LiveShape.AsWritten;

        private LiveShape Object(JsonTypeInfo type, string at)
        {
            var fields = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var property in type.Properties)
            {
                if (property.Get is null) continue;
                if (Of(options.GetTypeInfo(property.PropertyType), at + "." + property.Name) is ConvertedShape field) fields[property.Name] = field.Shape;
            }

            return fields.Count == 0 ? LiveShape.AsWritten : new ConvertedShape(new ObjectShape(fields));
        }

        private LiveShape Dictionary(JsonTypeInfo type, string at)
        {
            if (Of(options.GetTypeInfo(type.ElementType!), at + "[key]") is not ConvertedShape) return LiveShape.AsWritten;
            throw new InvalidOperationException(
                $"A value written into a live member is a dictionary whose values hold dates{Where(at)}. A date reaches a live member by its member's name: pass the entries as a list of objects that name each key.");
        }

        private static string Where(string at) => at == "$" ? "" : $" at '{at}'";
    }
}
