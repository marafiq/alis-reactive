using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public sealed class Meter : Component
{
    public static ComponentVendor MeterVendor { get; } = ComponentVendor.Define(
        "meter",
        objectPath: "",
        subscribe: "addEventListener",
        unsubscribe: "removeEventListener",
        payload: VendorEventPayload.Element);

    public override ComponentVendor Vendor => MeterVendor;

    public Property<int> Level => Property<int>();

    public ReadOnly<int> Max => ReadOnly<int>();

    public Event<MeterLevelChanged> LevelChanged => Event<MeterLevelChanged>();

    public Event<MeterAsChanged> Change => Event<MeterAsChanged>();
}

public sealed class MeterLevelChanged : JsObject
{
    public ReadOnly<int> Level => ReadOnly<int>();

    public ReadOnly<int> Previous => ReadOnly<int>();
}

public sealed class MeterAsChanged : JsObject
{
    public ReadOnly<int> Level => ReadOnly<int>();
}

public static class MeterHtml
{
    public static RenderedComponent<Meter, TModel> Meter<TModel>(this ReactivePlan<TModel> plan, string id, int level, int max)
        where TModel : class =>
        plan.Render<Meter>(id, new HtmlString(
            $"<fw-meter id=\"{HtmlEncoder.Default.Encode(id)}\" level=\"{level}\" max=\"{max}\"></fw-meter>"));
}
