using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.Declarations;

public sealed record Infusion(string Ward);

public sealed class InfusionPump : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<int> FlowRate => ReadOnly<int>();

    public ReadOnly<int> Volume => ReadOnly<int>(name: "VTBI");

    public ReadOnly<string?> Alarm => ReadOnly<string?>(optional: true);

    public Property<int> Rate => Property<int>();

    public Property<DateTime> StartsAt => Property<DateTime>();

    public Property<DateTime?> StopsAt => Property<DateTime?>();

    public Property<string?> Label => Property<string?>(optional: true);

    public Property<int?> Limit => Property<int?>();

    public WriteOnly<int> Bolus => WriteOnly<int>();

    public WriteOnly<int?> Lock => WriteOnly<int?>(optional: true);

    public WriteOnly<DateTimeOffset> PausedUntil => WriteOnly<DateTimeOffset>();

    public ReadOnly<Prescription> CurrentPrescription => ReadOnly<Prescription>();

    public Property<Seal> Seal => Property<Seal>();
}

public sealed class Prescription
{
    public string Drug { get; set; } = "none";

    public int MaxDailyDoses { get; set; } = 4;

    [JsonPropertyName("Route")]
    public string Route { get; set; } = "oral";

    public Dose Dose { get; set; } = new();

    public Carer? Prescriber { get; set; }

    [JsonConverter(typeof(SealText))]
    public Seal Seal { get; set; } = new();

    [JsonIgnore]
    public string Note { get; set; } = "";
}

public sealed class Dose
{
    public int Units { get; set; } = 1;
}

public sealed class Carer
{
    public string Name { get; set; } = "unnamed";
}

public sealed class Seal
{
    public string Tag { get; set; } = "";
}

public sealed class SealText : JsonConverter<Seal>
{
    public override Seal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new() { Tag = reader.GetString() ?? "" };

    public override void Write(Utf8JsonWriter writer, Seal value, JsonSerializerOptions options) => writer.WriteStringValue(value.Tag);
}
