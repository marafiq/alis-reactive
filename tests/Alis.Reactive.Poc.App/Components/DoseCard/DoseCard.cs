using System.Globalization;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/DoseCard/dose-card", "DoseCard")]
public sealed class DoseCard : Component
{
    public static ComponentVendor CardVendor { get; } = ComponentVendor.Define("dose-card", objectPath: "card", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => CardVendor;

    public Property<decimal?> Value => Property<decimal?>();

    public Event Change => Event();
}

public enum MedicationRoute
{
    Oral,
    Inhaled,
}

public sealed class DoseCardOptions
{
    public decimal Step { get; init; }

    public string Unit { get; init; } = "";

    public MedicationRoute Route { get; init; }

    public DateTime Due { get; init; }

    public IReadOnlyList<string> Times { get; init; } = [];
}

public static class DoseCardHtml
{
    public static Rendered<DoseCard, TModel> DoseCard<TModel>(this InputField<TModel, decimal> field, string label, DoseCardOptions options)
    {
        var card = new TagBuilder("fieldset");
        card.Attributes["id"] = field.Id;
        card.Attributes["data-dose-card"] = field.Json(options);
        card.Attributes["data-value"] = field.ValueJson;
        card.Attributes["data-decimal-separator"] = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var legend = new TagBuilder("legend");
        legend.InnerHtml.Append(label);
        var posted = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        posted.Attributes["type"] = "hidden";
        posted.Attributes["name"] = field.Name;
        card.InnerHtml.AppendHtml(legend).AppendHtml(posted);
        return field.Render<DoseCard>(card);
    }
}
