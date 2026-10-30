using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A bell that rings and counts its rings (bell.ts). It keeps itself on its element as <c>bell</c> and
/// takes a listener by assignment to the callback property an event names (<c>bell.onRung = ..</c>),
/// which already holds the bell's own callback.
/// </summary>
public sealed class Bell : Component
{
    public static ComponentVendor BellVendor { get; } = ComponentVendor.DefineCallbackProperties(
        "bell",
        objectPath: "bell",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => BellVendor;

    public ReadOnly<int> Rings => ReadOnly<int>();

    public void Ring() => Method();

    /// <summary>Raised each time the bell rings, by its own button or by <see cref="Ring"/>.</summary>
    public Event<BellRung> OnRung => Event<BellRung>();
}

public sealed class BellRung : JsObject
{
    public ReadOnly<int> Times => ReadOnly<int>();
}

public static class BellHtml
{
    public static RenderedComponent<Bell, TModel> Bell<TModel>(this ReactivePlan<TModel> plan, string id)
        where TModel : class =>
        plan.Render<Bell>(id, new HtmlString($"<div class=\"bell\" id=\"{HtmlEncoder.Default.Encode(id)}\"></div>"));
}
