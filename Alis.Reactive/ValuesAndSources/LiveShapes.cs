using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.ValuesAndSources;

internal static class LiveShapes
{
    private static readonly Type[] Integers = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)];
    private static readonly Type[] Fractions = [typeof(float), typeof(double), typeof(decimal)];
    private static readonly Type[] NullableIntegers = [typeof(byte?), typeof(sbyte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?)];
    private static readonly Type[] NullableFractions = [typeof(float?), typeof(double?), typeof(decimal?)];

    internal static bool Converts(JsonTypeInfo type, [NotNullWhen(true)] out Shape? shape) => Converts(type, null, out shape);

    internal static bool Converts(JsonTypeInfo type, HeldIntoLive? into, [NotNullWhen(true)] out Shape? shape)
    {
        shape = new Walk().Of(type, type.Options.NumberHandling, null, into);
        return shape is not null;
    }

    private static Shape? Scalar(JsonTypeInfo type, JsonNumberHandling handling) => type switch
    {
        JsonTypeInfo<DateTime> => DateShape.Instance,
        JsonTypeInfo<DateTime?> => new NullableShape(DateShape.Instance),
        JsonTypeInfo<DateTimeOffset> => new InstantShape(DateMeaning.Instant),
        JsonTypeInfo<DateTimeOffset?> => new NullableShape(new InstantShape(DateMeaning.Instant)),
        _ when (handling & JsonNumberHandling.WriteAsString) == 0 => null,
        _ when Integers.Contains(type.Type) => new NumberShape(NumberOf.Integer, NumberCulture.Invariant, ConversionShapes.OverflowOf(type)),
        _ when Fractions.Contains(type.Type) => new NumberShape(NumberOf.Fraction, NumberCulture.Invariant, ConversionShapes.OverflowOf(type)),
        _ when NullableIntegers.Contains(type.Type) => new NullableShape(new NumberShape(NumberOf.Integer, NumberCulture.Invariant, ConversionShapes.OverflowOf(type))),
        _ when NullableFractions.Contains(type.Type) => new NullableShape(new NumberShape(NumberOf.Fraction, NumberCulture.Invariant, ConversionShapes.OverflowOf(type))),
        _ => null,
    };

    private static bool HoldsADate(Shape shape) => Holds(shape, held => held is DateShape or InstantShape);

    private static bool HoldsAnEnum(Shape shape) => Holds(shape, held => held is EnumShape);

    private static bool Holds(Shape shape, Func<Shape, bool> isOne) => shape switch
    {
        _ when isOne(shape) => true,
        NullableShape nullable => Holds(nullable.Inner, isOne),
        ArrayShape array => Holds(array.Item, isOne),
        ObjectShape holder => holder.Fields.Values.Any(field => Holds(field.Shape, isOne)),
        _ => false,
    };

    private sealed class Walk
    {
        private readonly HashSet<JsonTypeInfo> _path = [];
        private readonly HashSet<JsonTypeInfo> _heldAgain = [];

        internal Shape? Of(JsonTypeInfo type, JsonNumberHandling handling, HeldMember? at, HeldIntoLive? into)
        {
            if (into is not null && EnumRows.IsEnum(type.Type)) return into.OfAnEnum(type, at);
            into?.RefuseAConverterOfTheApps(type, at);
            if (Scalar(type, handling) is { } scalar) return scalar;
            if (!_path.Add(type))
            {
                _heldAgain.Add(type);
                return null;
            }

            try
            {
                var shape = type.Kind switch
                {
                    JsonTypeInfoKind.Enumerable => List(type, handling, at, into),
                    JsonTypeInfoKind.Object => Object(type, into),
                    JsonTypeInfoKind.Dictionary => Dictionary(type, handling, at, into),
                    _ => null,
                };
                if (shape is not null && _heldAgain.Contains(type)) throw HoldsItsOwnType(shape);
                return shape;
            }
            finally
            {
                _path.Remove(type);
            }
        }

        private ArrayShape? List(JsonTypeInfo type, JsonNumberHandling handling, HeldMember? at, HeldIntoLive? into)
            => type.IsList(out var item) && Of(item, handling, at, into) is { } itemShape ? new ArrayShape(itemShape) : null;

        private ObjectShape? Object(JsonTypeInfo type, HeldIntoLive? into)
        {
            var fields = new Dictionary<string, FieldConversion>(StringComparer.Ordinal);
            foreach (var property in type.Properties.Where(property => property.Get is not null))
            {
                var handling = property.NumberHandling ?? type.NumberHandling ?? type.Options.NumberHandling;
                var member = type.Options.GetTypeInfo(property.PropertyType);
                var at = new HeldMember(type, property);
                var field = property.CustomConverter is { } own ? OfAMemberItsOwnConverterWrites(member, own, handling, at, into) : Of(member, handling, at, into);
                if (field is null) continue;
                fields[property.Name] = new FieldConversion(field, ConversionShapes.NothingOf(member));
            }

            return fields.Count == 0 ? null : new ObjectShape(fields);
        }

        private Shape? OfAMemberItsOwnConverterWrites(JsonTypeInfo member, JsonConverter own, JsonNumberHandling handling, HeldMember at, HeldIntoLive? into)
            => into is not null && EnumRows.IsEnum(member.Type) ? into.OfAnEnumItsOwnConverterWrites(member, own, at) : Of(member, handling, at, null);

        private Shape? Dictionary(JsonTypeInfo type, JsonNumberHandling handling, HeldMember? at, HeldIntoLive? into)
        {
            if (!type.IsDictionary(out _, out var value) || Of(value, handling, at, into) is not { } valueShape) return null;
            if (HoldsADate(valueShape))
            {
                throw new InvalidOperationException(
                    "A value written into a live member is a dictionary whose values hold dates. A date reaches a live member by its member's name: pass the entries as a list of objects that name each key.");
            }

            if (HoldsAnEnum(valueShape))
            {
                throw new InvalidOperationException(
                    "A value written into a live member is a dictionary whose values hold an enum the app's JSON converter writes. Such an enum reaches a live member by its member's name: pass the entries as a list of objects that name each key.");
            }

            return null;
        }

        private static InvalidOperationException HoldsItsOwnType(Shape shape) => HoldsAnEnum(shape) && !HoldsADate(shape)
            ? new("A value written into a live member holds its own type and an enum the app's JSON converter writes in it. Such an enum reaches a live member through a shape that ends: pass the values as a list of their own.")
            : new("A value written into a live member holds its own type and a date in it. A date reaches a live member through a shape that ends: pass the dates as a list of their own.");
    }
}

internal readonly record struct HeldMember(JsonTypeInfo Holder, JsonPropertyInfo Member)
{
    internal string AsCSharpDeclaresIt => ClassNames.AsCSharpWritesIt(Holder.Type) + "." + WrittenMember.AsCSharpDeclaresIt(Holder, Member);
}

internal sealed class HeldIntoLive
{
    private readonly AppJson _json;
    private readonly string? _read;

    internal HeldIntoLive(AppJson json, string? read)
    {
        _json = json;
        _read = read;
    }

    internal Shape? OfAnEnum(JsonTypeInfo held, HeldMember? at) => Mapped(held, held, _json.LiveForm.GetTypeInfo(held.Type), at);

    internal Shape? OfAnEnumItsOwnConverterWrites(JsonTypeInfo held, JsonConverter own, HeldMember at)
        => Mapped(held, WrittenByItsOwn(held.Options, own).GetTypeInfo(held.Type), WrittenByItsOwn(_json.LiveForm, own).GetTypeInfo(held.Type), at);

    internal void RefuseAConverterOfTheApps(JsonTypeInfo held, HeldMember? at)
    {
        var written = EnumRows.EnumOf(held.Type);
        var theLiveForms = _json.LiveForm.Converters;
        if (held.Options.Converters.Any(converter => !theLiveForms.Contains(converter) && converter.CanConvert(written))) throw WrittenOtherwise(held, at);
    }

    private Shape? Mapped(JsonTypeInfo declared, JsonTypeInfo source, JsonTypeInfo live, HeldMember? at)
    {
        var isFlags = _json.NamesFlags(EnumRows.EnumOf(declared.Type));
        var rows = EnumRows.Of(declared.Type, isFlags, Named(declared, at), (value, _) => OneScalar(JsonSerializer.SerializeToElement(value, source), declared, at));
        var mapped = rows.Select(row => new LiveEnumValue(row.Held, OneScalar(JsonSerializer.SerializeToElement(row.Value, live), declared, at), row.Number)).ToArray();
        if (mapped.All(row => EnumRows.WrittenAlike(row.Held, row.Live))) return null;
        var joinsTextsWithNumbers = isFlags && mapped.Select(row => row.Live.ValueKind == JsonValueKind.String).Distinct().Count() > 1;
        if (joinsTextsWithNumbers) throw WrittenOtherwise(declared, at);
        var shape = new EnumShape(isFlags, isFlags ? [.. mapped.OrderByDescending(row => row.Number)] : mapped);
        return Nullable.GetUnderlyingType(declared.Type) is null ? shape : new NullableShape(shape);
    }

    private JsonElement OneScalar(JsonElement written, JsonTypeInfo declared, HeldMember? at)
        => EnumRows.IsOneTextNumberOrBoolean(written) ? written : throw WrittenOtherwise(declared, at);

    private string Named(JsonTypeInfo held, HeldMember? at) => at is { } member ? member.AsCSharpDeclaresIt : _read ?? ClassNames.AsCSharpWritesIt(held.Type);

    private InvalidOperationException WrittenOtherwise(JsonTypeInfo held, HeldMember? at)
    {
        if (at is not { } member)
            return new($"A live object takes '{Named(held, at)}' as JavaScript holds its type, and the app's JSON converter writes it otherwise; pass the value from the server.");
        var named = member.AsCSharpDeclaresIt;
        return new($"A live object takes '{_read ?? named}' as JavaScript holds its type, and the app's JSON converter writes '{named}' otherwise; give the member a JSON converter of its own, or pass the value from the server.");
    }

    private static JsonSerializerOptions WrittenByItsOwn(JsonSerializerOptions options, JsonConverter own)
    {
        var byItsConverter = new JsonSerializerOptions(options);
        byItsConverter.Converters.Insert(0, own);
        return byItsConverter;
    }
}
