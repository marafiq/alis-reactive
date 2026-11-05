using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public sealed class Roster : Component
{
    public static ComponentVendor RosterVendor { get; } = ComponentVendor.DefineCallbackProperties(
        "roster",
        objectPath: "roster",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => RosterVendor;

    public void Show(Arg<string[]> names) => Method(names);
}

public static class RosterHtml
{
    public static RenderedComponent<Roster, TModel> Roster<TModel>(this ReactivePlan<TModel> plan, string id)
        where TModel : class
        => plan.Render<Roster>(id, new HtmlString($"<div class=\"roster\" id=\"{HtmlEncoder.Default.Encode(id)}\"></div>"));
}
