using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alis.Reactive.TypeKnowledge;

internal static class DictionaryKeys
{
    internal static void RefuseAKeyNamedApart(JsonTypeInfo key)
    {
        if (key.Type == typeof(DateTime)) throw NamedApart(key.Type, "2026-11-17T08:30:00", "11/17/2026 08:30:00");
        if (key.Type == typeof(DateTimeOffset)) throw NamedApart(key.Type, "2026-11-17T08:30:00+00:00", "11/17/2026 08:30:00 +00:00");
        if (!new JsonStringEnumConverter().CanConvert(key.Type)) return;
        foreach (Enum value in Enum.GetValues(key.Type))
        {
            var written = JsonSerializer.SerializeToElement(value, key);
            var name = value.ToString();
            var namedByAPolicy = written.ValueKind == JsonValueKind.String && written.GetString() != name;
            if (namedByAPolicy) throw NamedApart(key.Type, written.GetString()!, name);
        }
    }

    private static InvalidOperationException NamedApart(Type key, string json, string mvc)
        => new($"A dictionary keyed by {ClassNames.AsCSharpWritesIt(key)} cannot be sent: System.Text.Json and MVC's binding name write its keys apart, as {json} and {mvc}, so a JSON body and form data name one entry two ways; key it by text or a number.");
}
