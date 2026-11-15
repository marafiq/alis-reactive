using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

public sealed record MedicationRound(string Ward, Cover? Cover);

public sealed record Cover(string Name);

public sealed class DoseCart : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<List<CartDose>> Doses => ReadOnly<List<CartDose>>();

    public Property<int> Counted => Property<int>();

    public Property<bool> Flagged => Property<bool>();

    public Property<double> Measured => Property<double>();

    public Property<decimal> Charged => Property<decimal>();

    public Property<IReadOnlyList<CartDose>> Shown => Property<IReadOnlyList<CartDose>>();

    public Property<IReadOnlyList<int>> Units => Property<IReadOnlyList<int>>();

    public Property<IReadOnlyList<string>> Drugs => Property<IReadOnlyList<string>>();

    public Property<CartDose> Chosen => Property<CartDose>();

    public ReadOnly<string> Note => ReadOnly<string>();

    public ReadOnly<char[]> Letters => ReadOnly<char[]>();
}

public enum DoseStatus
{
    Planned,
    Due,
    Given,
}

public sealed class CartDose
{
    public string Drug { get; set; } = "none";

    public string Ward { get; set; } = "none";

    public int Units { get; set; } = 1;

    public double Volume { get; set; }

    public decimal Price { get; set; }

    public int? Priority { get; set; }

    public char Initial { get; set; } = 'A';

    public bool Late { get; set; }

    public Guid Batch { get; set; }

    public DateTime Due { get; set; }

    public DateTime Given { get; set; }

    public DoseStatus Status { get; set; }

    public DoseStatus Previous { get; set; }

    public Carer? Carer { get; set; }

    public Carer? Backup { get; set; }

    public Seal Seal { get; set; } = new();

    [JsonConverter(typeof(SealText))]
    public Seal Tagged { get; set; } = new();

    public string? Note { get; set; }

    public RegexOptions Matching { get; set; }

    public decimal Copay { get; set; }

    public DateTimeOffset Seen { get; set; }

    public DateTime? Recorded { get; set; }

    public List<Carer> Checks { get; set; } = [];

    public List<int> Hours { get; set; } = [];

    public IReadOnlyList<int> Rounds { get; set; } = [];

    public int[] Slots { get; set; } = [];

    public DateOnly Round { get; set; }

    public TimeOnly Shift { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DoseStatus Charted { get; set; } = DoseStatus.Due;

    public Supply Supply { get; set; } = new();

    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public decimal Rebate { get; set; }

    [JsonConverter(typeof(DecimalText))]
    public decimal Tariff { get; set; }

    [JsonConverter(typeof(DayText))]
    public DateTime Stamped { get; set; }
}

[JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
public sealed class Supply
{
    public int Vials { get; set; }

    public double Fill { get; set; }

    public string Lot { get; set; } = "none";
}

public sealed class Carer
{
    public string Name { get; set; } = "unnamed";
}

public sealed class Seal
{
    public string Tag { get; set; } = "";

    public static implicit operator string(Seal seal) => seal.Tag;
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
