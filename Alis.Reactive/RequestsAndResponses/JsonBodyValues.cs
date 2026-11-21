using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Execution;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;
using Expression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class JsonLeaf
{
    private JsonLeaf(Expression value, Shape kept, bool leftOutWhenNothing, IReadOnlyList<(string Name, JsonForm Form)> enums, IReadOnlyList<Expression> sentWhen)
    {
        Value = value;
        Kept = kept;
        LeftOutWhenNothing = leftOutWhenNothing;
        Enums = enums;
        SentWhen = sentWhen;
    }

    internal Expression Value { get; }

    internal Shape Kept { get; }

    internal bool LeftOutWhenNothing { get; }

    internal IReadOnlyList<(string Name, JsonForm Form)> Enums { get; }

    internal IReadOnlyList<Expression> SentWhen { get; }

    internal static JsonLeaf NullWhenNothing(Expression value) => new(value, AnyShape.Instance, false, [], []);

    internal static JsonLeaf LeftOut(Expression value, Shape kept) => new(value, kept, true, [], []);

    internal JsonLeaf SentBy(string name, JsonForm form) => new(Value, Kept, LeftOutWhenNothing, [(name, form)], SentWhen);

    internal JsonLeaf SentWhenItReads() => new(Value, Kept, LeftOutWhenNothing, Enums, [RequestValues.ReadsSomething(Value)]);
}

internal readonly record struct JsonStep(string Name, bool IsIndex)
{
    internal int Index => int.Parse(Name, NumberStyles.None, CultureInfo.InvariantCulture);

    internal PathStep Step => IsIndex ? new IndexStep(Index) : new MemberStep(Name);
}

internal static class JsonBodyValues
{
    internal static JsonLeaf Literal<TValue>(TValue value, BlockWriter writer, LambdaExpression member)
    {
        Literals.RefuseAtAnyDepth(value, writer.Content.Json.BodyForm, member.ToString());
        var written = new LitExpression(EnumEndpoints.Literal(value, writer.Content.Json));
        return value is null ? Leaf<TValue>(written, writer, member) : JsonLeaf.NullWhenNothing(written);
    }

    internal static JsonLeaf Source<TValue>(ITypedSource<TValue> source, BlockWriter writer, RequestMember member, Action<LetStatement> capture)
        => Captured<TValue>(RequestValues.Captured(Sources.Of(source), writer, capture), Sources.FormOf(source), writer, member);

    internal static JsonLeaf Captured<TValue>(Expression captured, HeldForm form, BlockWriter writer, RequestMember member)
    {
        var lambda = member.Lambda;
        var held = form.DataOptions.GetTypeInfo(typeof(TValue));
        var scalar = held.IsList(out var item) ? item : held;
        if (!EnumRows.IsEnum(scalar.Type)) return Leaf<TValue>(Value<TValue>(captured, form, writer, lambda.ToString()), writer, lambda);
        var sent = RequestValues.JsonFormOf(typeof(TValue), form, writer, lambda.ToString());
        return Leaf(captured, writer, lambda, () => AnyShape.Instance).SentBy(member.Name, sent);
    }

    private static JsonLeaf Leaf<TValue>(Expression value, BlockWriter writer, LambdaExpression member)
    {
        var body = writer.Content.Json.BodyForm.GetTypeInfo(typeof(TValue));
        var leaf = Leaf(value, writer, member, () => Kept<TValue>(body));
        return leaf.LeftOutWhenNothing && WritesItsDefaultAsNull<TValue>(body) ? leaf.SentWhenItReads() : leaf;
    }

    private static bool WritesItsDefaultAsNull<TValue>(JsonTypeInfo body) => !body.IsList(out _) && JsonSerializer.SerializeToElement(default(TValue), body).ValueKind == JsonValueKind.Null;

    private static JsonLeaf Leaf(Expression value, BlockWriter writer, LambdaExpression member, Func<Shape> kept)
        => ConversionShapes.NothingOf(writer.Content.Json.BodyForm.GetTypeInfo(member.ReturnType)) == FieldNothing.Omit
            ? JsonLeaf.LeftOut(value, kept())
            : JsonLeaf.NullWhenNothing(value);

    internal static Expression Object(IReadOnlyList<(IReadOnlyList<JsonStep> Path, JsonLeaf Leaf)> members)
    {
        var fields = new List<RecordField>();
        var leftOut = new Dictionary<string, FieldConversion>(StringComparer.Ordinal);
        foreach (var named in members.GroupBy(member => member.Path[0].Name, StringComparer.Ordinal))
        {
            var nested = Past(named);
            if (nested.Length > 0)
            {
                fields.Add(new RecordField(named.Key, Nested(nested)));
                continue;
            }

            var leaf = named.Single().Leaf;
            fields.Add(new RecordField(named.Key, leaf.Value));
            if (leaf.LeftOutWhenNothing) leftOut[named.Key] = new FieldConversion(leaf.Kept, FieldNothing.Omit);
        }

        var record = new RecordExpression(fields);
        return leftOut.Count == 0 ? record : new AsExpression(record, new ObjectShape(leftOut));
    }

    private static Expression Nested(IReadOnlyList<(IReadOnlyList<JsonStep> Path, JsonLeaf Leaf)> members)
        => members[0].Path[0].IsIndex ? List(members) : Object(members);

    private static ListExpression List(IReadOnlyList<(IReadOnlyList<JsonStep> Path, JsonLeaf Leaf)> positions)
    {
        var items = new List<Expression>();
        foreach (var position in positions.GroupBy(member => member.Path[0].Index).OrderBy(position => position.Key))
        {
            if (position.Key != items.Count)
            {
                throw new InvalidOperationException(
                    $"The request sets a member at position {position.Key} of a list in its JSON body and none at position {items.Count}, and a JSON list holds an item at every position before its last, which the endpoint binds as an item the request never set; set a member at each position from 0, or send the list whole.");
            }

            var nested = Past(position);
            items.Add(nested.Length > 0 ? Nested(nested) : ItemValue(position.Single().Leaf, position.Key));
        }

        return new ListExpression([.. items]);
    }

    private static Expression ItemValue(JsonLeaf leaf, int position)
    {
        if (!leaf.LeftOutWhenNothing) return leaf.Value;
        throw new InvalidOperationException(
            $"The request sets position {position} of a list in its JSON body from a value the body leaves out when it reads nothing, and a JSON list holds an item at every position, which the endpoint binds as an item the request never set; set it from a literal, or send the list whole.");
    }

    private static (IReadOnlyList<JsonStep> Path, JsonLeaf Leaf)[] Past(IEnumerable<(IReadOnlyList<JsonStep> Path, JsonLeaf Leaf)> named)
        => [.. named.Where(member => member.Path.Count > 1).Select(member => ((IReadOnlyList<JsonStep>)[.. member.Path.Skip(1)], member.Leaf))];

    private static Expression Value<TValue>(Expression captured, HeldForm form, BlockWriter writer, string member)
    {
        var json = writer.Content.Json;
        var held = form.DataOptions.GetTypeInfo(typeof(TValue));
        var isList = held.IsList(out var item);
        var scalar = item ?? held;
        if (form.Own is { } own)
        {
            throw new InvalidOperationException(
                $"'{member}' is read from a member {own.Cause} writes, and a JSON body sends a member in the form its endpoint binds, which the browser cannot read from that writing; set it from a literal, or send it in form data.");
        }

        var appWritesItItself = json.App.Converters.Any(converter => converter.CanConvert(scalar.Type));
        if (appWritesItItself || !form.WritesAsAScalar(scalar.Type))
        {
            if (form.IsTheAppsOwn && !appWritesItItself) return InTheBodysForm<TValue>(captured, form, held, writer.Content.RendersUnder, member);
            if (form.IsTheAppsOwn && !LiveShapes.Converts(held, out _)) return captured;
            if (appWritesItItself)
            {
                throw new InvalidOperationException(
                    $"'{member}' holds what a JSON converter of the app's writes, and a JSON body sends a member in the form its endpoint binds, which the browser cannot read from that writing; set it from a literal, or send it in form data.");
            }

            return AsALiveObjectHoldsIt<TValue>(captured, form, writer, member);
        }

        var live = form.NumbersAndDatesIntoLive<TValue>(captured);
        var text = ConversionShapes.TextOf(scalar, writer.Content.RendersUnder);
        if (text.Date == DateMeaning.None) return live;
        return new AsExpression(live, isList ? new ArrayShape(text) : text);
    }

    private static Expression InTheBodysForm<TValue>(Expression captured, HeldForm form, JsonTypeInfo held, CultureInfo culture, string member)
    {
        var live = form.NumbersAndDatesIntoLive<TValue>(captured);
        return DatesAsText(held, culture, member, [], (_, _, _) => { }) is { } texts ? new AsExpression(live, texts) : live;
    }

    private static Expression AsALiveObjectHoldsIt<TValue>(Expression captured, HeldForm form, BlockWriter writer, string member)
    {
        var endpoint = writer.Content.Json.App.GetTypeInfo(typeof(TValue));
        var texts = DatesAsText(endpoint, writer.Content.RendersUnder, member, [], (holder, property, held) => RefuseWhatALiveObjectHoldsApart(holder, property, held, form, writer.Content.Json, member));
        return texts is null ? captured : new AsExpression(captured, texts);
    }

    private static Shape? DatesAsText(JsonTypeInfo type, CultureInfo culture, string member, HashSet<JsonTypeInfo> path, Action<JsonTypeInfo, JsonPropertyInfo, JsonTypeInfo> refuse)
    {
        if (IsADate(type)) return ConversionShapes.TextOf(type, culture);
        if (!path.Add(type)) return null;
        try
        {
            if (type.IsList(out var item)) return DatesAsText(item, culture, member, path, refuse) is { } itemTexts ? new ArrayShape(itemTexts) : null;
            if (type.Kind != JsonTypeInfoKind.Object) return null;
            var fields = new Dictionary<string, FieldConversion>(StringComparer.Ordinal);
            foreach (var property in type.Properties.Where(property => property.Get is not null))
            {
                var held = type.Options.GetTypeInfo(property.PropertyType);
                RefuseAWritingTheLiveFormMisreads(type, property, held, member);
                RefuseANameItsEndpointBindsOtherwise(type, property, member);
                refuse(type, property, held);
                if (DatesAsText(held, culture, member, path, refuse) is not { } texts) continue;
                fields[property.Name] = new FieldConversion(texts, ConversionShapes.NothingOf(held));
            }

            return fields.Count == 0 ? null : new ObjectShape(fields);
        }
        finally
        {
            path.Remove(type);
        }
    }

    private static void RefuseAWritingTheLiveFormMisreads(JsonTypeInfo holder, JsonPropertyInfo property, JsonTypeInfo held, string member)
    {
        var scalar = held.IsList(out var item) ? item : held;
        var handling = property.NumberHandling ?? holder.NumberHandling ?? holder.Options.NumberHandling;
        var convertedIntoTheLiveForm = IsADate(scalar) || ComparedTypes.IsNumber(scalar.Type) && (handling & JsonNumberHandling.WriteAsString) != 0;
        if (!convertedIntoTheLiveForm) return;
        var writesIt = property.CustomConverter is not null ? "its own JSON converter"
            : holder.Options.Converters.Any(converter => converter.CanConvert(scalar.Type)) ? "a JSON converter of the app's"
            : null;
        if (writesIt is null) return;
        throw new InvalidOperationException(
            $"'{member}' holds '{property.Name}', which {writesIt} writes, and a JSON body sends it in the form its endpoint binds, which the browser cannot read from that writing; set it from a literal, or set its other members.");
    }

    private static void RefuseANameItsEndpointBindsOtherwise(JsonTypeInfo holder, JsonPropertyInfo property, string member)
    {
        if (MemberPath.BoundUnderAnotherName(holder, property) is not { } declared) return;
        throw new InvalidOperationException(
            $"'{member}' holds '{property.Name}', which MVC 5 binds by its C# name {declared}, and a JSON body sends an object's members by their JSON names; give it a JSON name that differs from {declared} by case alone.");
    }

    private static void RefuseWhatALiveObjectHoldsApart(JsonTypeInfo holder, JsonPropertyInfo property, JsonTypeInfo held, HeldForm form, AppJson json, string member)
    {
        var scalar = held.IsList(out var item) ? item : held;
        if (property.CustomConverter is not null || IsADate(scalar)) return;
        if (EnumRows.IsEnum(scalar.Type))
        {
            RefuseAnEnumItsEndpointReadsApart(property, Nullable.GetUnderlyingType(scalar.Type) ?? scalar.Type, form, json, member);
            return;
        }

        if (!holder.Options.Converters.Any(converter => converter.CanConvert(scalar.Type))) return;
        throw new InvalidOperationException(
            $"'{member}' holds '{property.Name}', which a JSON converter of the app's writes, and a JSON body sends it in the form its endpoint binds, which the browser cannot read from that writing; set it from a literal, or set its other members.");
    }

    private static void RefuseAnEnumItsEndpointReadsApart(JsonPropertyInfo property, Type enumeration, HeldForm form, AppJson json, string member)
    {
        foreach (Enum value in Enum.GetValues(enumeration))
        {
            var held = form.Beside(value, enumeration).Value;
            if (EnumEndpoints.ReadsAsHeld(value, held, enumeration, json)) continue;
            throw new InvalidOperationException(
                $"'{member}' holds '{property.Name}', whose enum a live object holds as {held.GetRawText()} for {value}, a form its endpoint does not read as {value}; let the endpoint read the form the live object holds, or send the rows from JSON the server writes.");
        }
    }

    private static bool IsADate(JsonTypeInfo type) => type is JsonTypeInfo<DateTime> or JsonTypeInfo<DateTime?> or JsonTypeInfo<DateTimeOffset> or JsonTypeInfo<DateTimeOffset?>;

    private static Shape Kept<TValue>(JsonTypeInfo body)
    {
        if (body.IsList(out var item)) return new ArrayShape(KeptItem(item));
        return JsonSerializer.SerializeToElement(default(TValue), body).ValueKind switch
        {
            JsonValueKind.Number => Numbers,
            JsonValueKind.True or JsonValueKind.False => BooleanShape.Instance,
            JsonValueKind.String => Texts,
            JsonValueKind.Object => Objects,
            JsonValueKind.Array => new ArrayShape(AnyShape.Instance),
            _ => AnyShape.Instance,
        };
    }

    private static Shape KeptItem(JsonTypeInfo item)
    {
        if (item.IsList(out var inner)) return new ArrayShape(KeptItem(inner));
        if (item.Kind is JsonTypeInfoKind.Object or JsonTypeInfoKind.Dictionary) return Objects;
        if (ComparedTypes.IsNumber(item.Type)) return Numbers;
        return item.Type == typeof(bool) || item.Type == typeof(bool?) ? BooleanShape.Instance : Texts;
    }

    private static NumberShape Numbers => new(NumberOf.Fraction, NumberCulture.Invariant, NumberOverflow.Nothing);

    private static StringShape Texts => new(DateMeaning.None, "0");

    private static ObjectShape Objects => new(new Dictionary<string, FieldConversion>(StringComparer.Ordinal));
}
