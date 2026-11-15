using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.Conditions;

public sealed record DoseRound(string Ward);

public sealed class DoseCart : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<List<CartDose>> Doses => ReadOnly<List<CartDose>>();

    public Property<bool> Flagged => Property<bool>();

    public Property<DoseStatus> Status => Property<DoseStatus>();

    public Property<decimal> Charged => Property<decimal>();

    public Property<decimal> Ceiling => Property<decimal>();

    public Property<decimal?> Copay => Property<decimal?>();

    public ReadOnly<List<DoseStatus>> Allowed => ReadOnly<List<DoseStatus>>();

    public Property<DateTime> Due => Property<DateTime>();

    public ReadOnly<List<DateTime>> RoundDays => ReadOnly<List<DateTime>>();

    public Property<Seal> Seal => Property<Seal>();

    public ReadOnly<List<Seal>> Seals => ReadOnly<List<Seal>>();
}

public sealed class CartDose
{
    public string Drug { get; set; } = "none";

    public DoseStatus Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DoseStatus Charted { get; set; } = DoseStatus.Due;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DoseStatus? EarlierCharted { get; set; }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public decimal Rebate { get; set; }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public double Fill { get; set; }

    [JsonConverter(typeof(DecimalText))]
    public decimal Tariff { get; set; }

    [JsonConverter(typeof(DayText))]
    public DateTime Stamped { get; set; }

    [JsonConverter(typeof(SealText))]
    public Seal Tagged { get; set; } = new();

    [JsonConverter(typeof(NoteText))]
    public string Note { get; set; } = "";

    [JsonConverter(typeof(LotsText))]
    public List<string> Lots { get; set; } = [];
}

public enum DoseStatus
{
    Planned,
    Due,
    Given,
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

public sealed class DecimalText : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => decimal.Parse(reader.GetString() ?? "0", CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
}

public sealed class DayText : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateTime.ParseExact(reader.GetString() ?? "00010101", "yyyyMMdd", CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
}

public sealed class NoteText : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => (reader.GetString() ?? "").Trim();

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value.Trim());
}

public sealed class LotsText : JsonConverter<List<string>>
{
    public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => [.. (reader.GetString() ?? "").Split(',')];

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options) => writer.WriteStringValue(string.Join(",", value));
}
