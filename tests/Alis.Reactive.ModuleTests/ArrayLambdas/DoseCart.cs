using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public DateTime Due { get; set; }

    public DateTime Given { get; set; }

    public DoseStatus Status { get; set; }

    public DoseStatus Previous { get; set; }

    public Carer? Carer { get; set; }

    public Carer? Backup { get; set; }

    public Seal Seal { get; set; } = new();

    [JsonConverter(typeof(SealText))]
    public Seal Tagged { get; set; } = new();
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
