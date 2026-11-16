#if NET48
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.RequestsAndResponses;

internal static partial class EnumEndpoints
{
    internal static partial JsonElement Sent(Enum value, Type enumeration, AppJson json) => JsonSerializer.SerializeToElement(value.ToString(), PlanJson.Options);

    internal static partial JsonElement Literal<TValue>(TValue value, AppJson json)
    {
        var written = JsonSerializer.SerializeToElement(value, json.BodyForm);
        using var named = new MemoryStream();
        using (var writer = new Utf8JsonWriter(named)) WriteNamed(writer, value, json.BodyForm.GetTypeInfo(typeof(TValue)), written);
        using var document = JsonDocument.Parse(named.ToArray());
        return document.RootElement.Clone();
    }

    private static void WriteNamed(Utf8JsonWriter writer, object? value, JsonTypeInfo declared, JsonElement written)
    {
        if (value is Enum held)
            writer.WriteStringValue(held.ToString());
        else if (value is IEnumerable items && declared.IsList(out var item) && written.ValueKind == JsonValueKind.Array)
            WriteItems(writer, items, item, written);
        else if (value is IDictionary entries && declared.IsDictionary(out _, out var entry) && written.ValueKind == JsonValueKind.Object)
            WriteEntries(writer, entries, entry, written);
        else if (value is not null && declared.Kind == JsonTypeInfoKind.Object && written.ValueKind == JsonValueKind.Object)
            WriteMembers(writer, value, declared.ContractItWasWrittenBy(written), written);
        else
            written.WriteTo(writer);
    }

    private static void WriteItems(Utf8JsonWriter writer, IEnumerable items, JsonTypeInfo item, JsonElement written)
    {
        writer.WriteStartArray();
        foreach (var (held, element) in items.Cast<object?>().Zip(written.EnumerateArray(), (held, element) => (held, element))) WriteNamed(writer, held, item, element);
        writer.WriteEndArray();
    }

    private static void WriteEntries(Utf8JsonWriter writer, IDictionary entries, JsonTypeInfo entry, JsonElement written)
    {
        writer.WriteStartObject();
        foreach (var (held, property) in entries.Values.Cast<object?>().Zip(written.EnumerateObject(), (held, property) => (held, property)))
        {
            writer.WritePropertyName(property.Name);
            WriteNamed(writer, held, entry, property.Value);
        }

        writer.WriteEndObject();
    }

    private static void WriteMembers(Utf8JsonWriter writer, object holder, JsonTypeInfo contract, JsonElement written)
    {
        writer.WriteStartObject();
        foreach (var property in written.EnumerateObject())
        {
            writer.WritePropertyName(property.Name);
            var member = contract.Properties.FirstOrDefault(declared => declared.Name == property.Name);
            var held = member?.Get?.Invoke(holder);
            if (held is Enum || member is { CustomConverter: null })
                WriteNamed(writer, held, contract.Options.GetTypeInfo(member!.PropertyType), property.Value);
            else
                property.Value.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}
#endif
