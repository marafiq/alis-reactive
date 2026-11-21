using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.Declarations;

public sealed class ResidentRoster : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public void SortBy(Arg<Field> field) => Method([field]);
}

public sealed class Resident
{
    public string Name { get; set; } = "Ada";

    public int MultiWord { get; set; } = 2;

    [JsonPropertyName("care-priority")]
    public int CarePriority { get; set; } = 1;

    public ResidentRoom Room { get; set; } = new();

    [JsonConverter(typeof(ResidentRoomText))]
    public ResidentRoom AssignedRoom { get; set; } = new();

    [JsonIgnore]
    public string PrivateNote { get; set; } = "";
}

public sealed class ResidentRoom
{
    public int Number { get; set; } = 12;
}

public sealed class ResidentRoomText : JsonConverter<ResidentRoom>
{
    public override ResidentRoom Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new() { Number = int.Parse(reader.GetString()!, CultureInfo.InvariantCulture) };

    public override void Write(Utf8JsonWriter writer, ResidentRoom value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Number.ToString(CultureInfo.InvariantCulture));
}
