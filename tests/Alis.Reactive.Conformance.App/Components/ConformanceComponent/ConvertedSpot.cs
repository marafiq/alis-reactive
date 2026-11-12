using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

[JsonConverter(typeof(ConvertedSpotAsItsName))]
public sealed class ConvertedSpot
{
    public string Name { get; set; } = "";
}

public sealed class ConvertedSpotAsItsName : JsonConverter<ConvertedSpot>
{
    public override ConvertedSpot Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new() { Name = reader.GetString() ?? "" };

    public override void Write(Utf8JsonWriter writer, ConvertedSpot value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Name);
}
