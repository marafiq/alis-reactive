using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A meter (meter.ts): the custom element <c>&lt;fw-meter&gt;</c> is the object itself. It takes listeners
/// through <c>addEventListener</c>, and a listener receives the DOM event, whose <c>detail</c> is the payload.
/// </summary>
public sealed class Meter : Component
{
    public static ComponentVendor MeterVendor { get; } = ComponentVendor.Define(
        "meter",
        objectPath: "",
        subscribe: "addEventListener",
        unsubscribe: "removeEventListener",
        payload: VendorEventPayload.DomEvent);

    public override ComponentVendor Vendor => MeterVendor;

    public Property<int> Level => Property<int>();

    public ReadOnly<int> Max => ReadOnly<int>();

    /// <summary>Raised when the meter's own buttons change its level.</summary>
    public Event<MeterLevelChanged> LevelChanged => Event<MeterLevelChanged>();
}

public sealed class MeterLevelChanged : JsObject
{
    public ReadOnly<int> Level => ReadOnly<int>();

    public ReadOnly<int> Previous => ReadOnly<int>();
}

public static class MeterHtml
{
    public static RenderedComponent<Meter, TModel> Meter<TModel>(this ReactivePlan<TModel> plan, string id, int level, int max)
        where TModel : class =>
        plan.Render<Meter>(id, new HtmlString(
            $"<fw-meter id=\"{HtmlEncoder.Default.Encode(id)}\" level=\"{level}\" max=\"{max}\"></fw-meter>"));
}
