using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A roster (roster.ts): the names it is given, one line each, in the order given. It keeps itself on its
/// element as <c>roster</c> and raises no events, so nothing listens to it.
/// </summary>
public sealed class Roster : Component
{
    public static ComponentVendor RosterVendor { get; } = ComponentVendor.DefineCallbackProperties(
        "roster",
        objectPath: "roster",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => RosterVendor;

    /// <summary>Shows the names, one line each, in the order given, in place of the lines it showed.</summary>
    public void Show(Arg<string[]> names) => Method(names);
}

public static class RosterHtml
{
    /// <summary>An empty roster; roster.ts builds its list when the page mounts.</summary>
    public static RenderedComponent<Roster, TModel> Roster<TModel>(this ReactivePlan<TModel> plan, string id)
        where TModel : class =>
        plan.Render<Roster>(id, new HtmlString($"<div class=\"roster\" id=\"{HtmlEncoder.Default.Encode(id)}\"></div>"));
}
