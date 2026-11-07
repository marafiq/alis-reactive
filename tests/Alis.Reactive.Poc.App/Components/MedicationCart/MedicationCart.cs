using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/MedicationCart/medication-cart", "MedicationCart")]
public sealed class MedicationCart : Component
{
    public static ComponentVendor CartVendor { get; } = ComponentVendor.Define("cart", objectPath: "cart", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => CartVendor;

    public ReadOnly<List<MedicationDose>> Doses => ReadOnly<List<MedicationDose>>();

    public ReadOnly<MedicationDose> Featured => ReadOnly<MedicationDose>();
}

public enum DoseStatus
{
    Due,
    Given,
    Missed,
}

public sealed record MedicationDose(
    string Medicine,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Ward,
    int? Priority,
    DoseStatus Status,
    DoseStatus Planned,
    double Milligrams,
    char Route,
    DateTime Scheduled,
    Carer? Carer,
    Carer? Witness,
    string Label,
    string Prescriber,
    string? Note,
    IReadOnlyList<Carer> Checks,
    int[] Hours,
    decimal Charge,
    double Volume,
    int StockChange,
    decimal Claim,
    bool Controlled,
    [property: JsonNumberHandling(JsonNumberHandling.WriteAsString)] decimal Copay)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Refills { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Unit { get; init; } = "mg";

    [JsonPropertyName("constructor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Maker { get; init; }

    public DoseStore? Store { get; init; }

    public DoseBatch? Batch { get; init; }

    public DoseSupply? Supply { get; init; }

    public DoseSeal? Seal { get; init; }

    public DoseDelivery? Delivery { get; init; }

    public string Instruction { get; init; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DoseStatus Charted { get; init; }

    [JsonConverter(typeof(CarerNameConverter))]
    public Carer? Charter { get; init; }

    [JsonConverter(typeof(UpperTextConverter))]
    public string Code { get; init; } = "";
}

public sealed class UpperTextConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString()!;

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToUpperInvariant());
}

public sealed class CarerNameConverter : JsonConverter<Carer>
{
    public override Carer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, Carer value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);
}

public sealed class DoseStore
{
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public DoseShelf Shelf { get; set; } = new() { Name = "fridge" };

    public DoseShelf Spare { get; set; } = new() { Name = "fridge" };

    public DoseShelf Locker { get; } = new() { Name = "fridge" };
}

public sealed class DoseShelf
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; } = "cupboard";
}

public sealed class DoseBatch
{
    [JsonRequired]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Lot { get; init; }

    public string Expiry { get; init; } = "";
}

[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
public sealed class DoseSupply
{
    public int Vials { get; init; }

    public string Source { get; init; } = "";
}

public sealed class DoseSeal
{
    private DoseSeal(string tag, string? breaker)
    {
        Tag = tag;
        Breaker = breaker;
    }

    public string Tag { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Breaker { get; }

    public static DoseSeal Of(string tag, string? breaker) => new(tag, breaker);

    public static implicit operator string(DoseSeal seal) => seal.Tag;
}

[JsonPolymorphic]
[JsonDerivedType(typeof(DoseInfusion), "infusion")]
public class DoseDelivery
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Site { get; init; }
}

public sealed class DoseInfusion : DoseDelivery
{
    public DoseInfusion()
    {
        Site = "cannula";
    }
}

public static class MedicationCartJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}

public static class MedicationCartHtml
{
    public static Rendered<MedicationCart, TModel> MedicationCart<TModel>(this ReactivePlan<TModel> plan, string id, IReadOnlyList<MedicationDose> doses)
    {
        var cart = new TagBuilder("section");
        cart.Attributes["id"] = id;
        cart.Attributes["data-medication-cart"] = JsonSerializer.Serialize(doses, MedicationCartJson.Options);
        return plan.Render<MedicationCart>(id, cart);
    }

    public static Rendered<MedicationCart, TModel> MedicationCartFeaturing<TModel>(this ReactivePlan<TModel> plan, string id, IReadOnlyList<MedicationDose> doses, MedicationDose featured)
    {
        var cart = new TagBuilder("section");
        cart.Attributes["id"] = id;
        cart.Attributes["data-medication-cart"] = JsonSerializer.Serialize(doses, MedicationCartJson.Options);
        cart.Attributes["data-featured-dose"] = JsonSerializer.Serialize(featured, MedicationCartJson.Options);
        return plan.Render<MedicationCart>(id, cart);
    }
}
