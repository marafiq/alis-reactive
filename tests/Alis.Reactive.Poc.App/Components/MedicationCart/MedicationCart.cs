using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class MedicationCart : Component
{
    public static ComponentVendor CartVendor { get; } = ComponentVendor.Define("cart", objectPath: "cart", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => CartVendor;

    public ReadOnly<List<MedicationDose>> Doses => ReadOnly<List<MedicationDose>>();
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
    Carer? Witness);

public static class MedicationCartJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}

public static class MedicationCartHtml
{
    public static Rendered<MedicationCart> MedicationCart<TModel>(this ReactivePlan<TModel> plan, string id, IReadOnlyList<MedicationDose> doses)
    {
        var cart = new TagBuilder("section");
        cart.Attributes["id"] = id;
        cart.Attributes["data-medication-cart"] = JsonSerializer.Serialize(doses, MedicationCartJson.Options);
        return plan.Render<MedicationCart>(id, cart);
    }
}
