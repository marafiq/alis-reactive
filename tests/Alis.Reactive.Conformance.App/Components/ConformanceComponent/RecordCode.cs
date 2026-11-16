using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

[JsonConverter(typeof(RecordCodeAsItsJson))]
public sealed class RecordCode
{
    private readonly string? _held;

    private RecordCode(string? held)
    {
        _held = held;
    }

    public static RecordCode Null { get; } = new(null);

    public static RecordCode Text(string code) => new(code);

    internal void WriteTo(Utf8JsonWriter writer)
    {
        if (_held is null) writer.WriteNullValue();
        else writer.WriteStringValue(_held);
    }
}

public sealed class RecordCodeAsItsJson : JsonConverter<RecordCode>
{
    public override RecordCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => RecordCode.Text(reader.GetString() ?? throw new JsonException("A record's code is a text or null."));

    public override void Write(Utf8JsonWriter writer, RecordCode value, JsonSerializerOptions options) => value.WriteTo(writer);
}
