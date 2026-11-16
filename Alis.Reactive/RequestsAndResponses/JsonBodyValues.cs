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
    private JsonLeaf(Expression value, Shape kept, bool leftOutWhenNothing, IReadOnlyList<(string Name, JsonForm Form)> enums)
    {
        Value = value;
        Kept = kept;
        LeftOutWhenNothing = leftOutWhenNothing;
        Enums = enums;
    }

    internal Expression Value { get; }

    internal Shape Kept { get; }

    internal bool LeftOutWhenNothing { get; }

    internal IReadOnlyList<(string Name, JsonForm Form)> Enums { get; }

    internal static JsonLeaf NullWhenNothing(Expression value) => new(value, AnyShape.Instance, false, []);

    internal static JsonLeaf LeftOut(Expression value, Shape kept) => new(value, kept, true, []);

    internal JsonLeaf SentBy(string name, JsonForm form) => new(Value, Kept, LeftOutWhenNothing, [(name, form)]);
}

internal static class JsonBodyValues
{
    internal static JsonLeaf Literal<TValue>(TValue value, BlockWriter writer, LambdaExpression member)
    {
        Literals.RefuseAtAnyDepth(value, writer.Content.Json.BodyForm, member.ToString());
        var written = new LitExpression(EnumEndpoints.Literal(value, writer.Content.Json));
        return value is null ? Leaf<TValue>(written, writer, member) : JsonLeaf.NullWhenNothing(written);
    }

    internal static JsonLeaf Source<TValue>(ITypedSource<TValue> source, BlockWriter writer, LambdaExpression member, Action<LetStatement> capture)
        => Captured<TValue>(RequestValues.Captured(Sources.Of(source), writer, capture), Sources.FormOf(source), writer, member);

    internal static JsonLeaf Captured<TValue>(Expression captured, HeldForm form, BlockWriter writer, LambdaExpression member)
    {
        var held = form.DataOptions.GetTypeInfo(typeof(TValue));
        var scalar = held.IsList(out var item) ? item : held;
        if (!RequestValues.IsEnum(scalar.Type)) return Leaf<TValue>(Value<TValue>(captured, form, writer, member.ToString()), writer, member);
        var sent = RequestValues.JsonFormOf(typeof(TValue), form, writer, member.ToString());
        return Leaf(captured, writer, member, () => AnyShape.Instance).SentBy(new RequestMember(member).Name, sent);
    }

    private static JsonLeaf Leaf<TValue>(Expression value, BlockWriter writer, LambdaExpression member)
        => Leaf(value, writer, member, () => Kept<TValue>(writer.Content.Json.BodyForm.GetTypeInfo(typeof(TValue)), member.ToString()));

    private static JsonLeaf Leaf(Expression value, BlockWriter writer, LambdaExpression member, Func<Shape> kept)
        => ConversionShapes.NothingOf(writer.Content.Json.BodyForm.GetTypeInfo(member.ReturnType)) == FieldNothing.Omit
            ? JsonLeaf.LeftOut(value, kept())
            : JsonLeaf.NullWhenNothing(value);

    internal static Expression Object(IReadOnlyList<(IReadOnlyList<string> Names, JsonLeaf Leaf)> members)
    {
        var fields = new List<RecordField>();
        var leftOut = new Dictionary<string, FieldConversion>(StringComparer.Ordinal);
        foreach (var named in members.GroupBy(member => member.Names[0], StringComparer.Ordinal))
        {
            var nested = named.Where(member => member.Names.Count > 1).Select(member => ((IReadOnlyList<string>)[.. member.Names.Skip(1)], member.Leaf)).ToArray();
            if (nested.Length > 0)
            {
                fields.Add(new RecordField(named.Key, Object(nested)));
                continue;
            }

            var leaf = named.Single().Leaf;
            fields.Add(new RecordField(named.Key, leaf.Value));
            if (leaf.LeftOutWhenNothing) leftOut[named.Key] = new FieldConversion(leaf.Kept, FieldNothing.Omit);
        }

        var record = new RecordExpression(fields);
        return leftOut.Count == 0 ? record : new AsExpression(record, new ObjectShape(leftOut));
    }

    private static Expression Value<TValue>(Expression captured, HeldForm form, BlockWriter writer, string member)
    {
        var json = writer.Content.Json;
        var held = form.DataOptions.GetTypeInfo(typeof(TValue));
        var isList = held.IsList(out var item);
        var scalar = item ?? held;
        if (form.Own is { } own)
        {
            throw new NotImplementedException(
                $"'{member}' is read from a member {own.Cause} writes, and a JSON body sends the form its endpoint binds, which no node of the plan carries yet; set it from a literal, or send it in form data.");
        }

        var appWritesItItself = json.App.Converters.Any(converter => converter.CanConvert(scalar.Type));
        if (appWritesItItself || !form.WritesAsAScalar(scalar.Type))
        {
            if (form.IsTheAppsOwn && !LiveShapes.Converts(held, out _)) return captured;
            if (form.IsTheAppsOwn && !appWritesItItself) return InTheBodysForm<TValue>(captured, form, held, writer.Content.RendersUnder, member);
            throw new NotImplementedException(
                $"'{member}' holds what {form.BaseFormName} writes, and a JSON body sends it as the app's JSON options write it (D9.7, D9.19): no node of the plan carries that writing from this source yet; send it from the server's JSON, or set its scalar members.");
        }

        var live = form.IntoLive<TValue>(captured);
        var text = ConversionShapes.TextOf(scalar, writer.Content.RendersUnder);
        if (text.Date == DateMeaning.None) return live;
        return new AsExpression(live, isList ? new ArrayShape(text) : text);
    }

    private static Expression InTheBodysForm<TValue>(Expression captured, HeldForm form, JsonTypeInfo held, CultureInfo culture, string member)
    {
        var live = form.IntoLive<TValue>(captured);
        return DatesAsText(held, culture, member, []) is { } texts ? new AsExpression(live, texts) : live;
    }

    private static Shape? DatesAsText(JsonTypeInfo type, CultureInfo culture, string member, HashSet<JsonTypeInfo> path)
    {
        if (IsADate(type)) return ConversionShapes.TextOf(type, culture);
        if (!path.Add(type)) return null;
        try
        {
            if (type.IsList(out var item)) return DatesAsText(item, culture, member, path) is { } itemTexts ? new ArrayShape(itemTexts) : null;
            if (type.Kind != JsonTypeInfoKind.Object) return null;
            var fields = new Dictionary<string, FieldConversion>(StringComparer.Ordinal);
            foreach (var property in type.Properties.Where(property => property.Get is not null))
            {
                var held = type.Options.GetTypeInfo(property.PropertyType);
                RefuseAWritingTheLiveFormMisreads(type, property, held, member);
                if (DatesAsText(held, culture, member, path) is not { } texts) continue;
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
        throw new NotImplementedException(
            $"'{member}' holds '{property.Name}', which {writesIt} writes, and a JSON body sends it in the form its endpoint binds (D9.7): no node of the plan reads that writing into it yet; set it from a literal, or set its other members.");
    }

    private static bool IsADate(JsonTypeInfo type) => type is JsonTypeInfo<DateTime> or JsonTypeInfo<DateTime?> or JsonTypeInfo<DateTimeOffset> or JsonTypeInfo<DateTimeOffset?>;

    private static Shape Kept<TValue>(JsonTypeInfo body, string member)
    {
        if (body.IsList(out var item)) return new ArrayShape(KeptItem(item));
        return JsonSerializer.SerializeToElement(default(TValue), body).ValueKind switch
        {
            JsonValueKind.Number => Numbers,
            JsonValueKind.True or JsonValueKind.False => BooleanShape.Instance,
            JsonValueKind.String => Texts,
            JsonValueKind.Object => Objects,
            _ => throw new NotImplementedException($"'{member}' is a value type its JSON writes as a list, and a JSON body leaves out a member that reads nothing through a shape that keeps it, which no shape holds yet; set it from a literal."),
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
