using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public sealed class Bell : Component
{
    public static ComponentVendor BellVendor { get; } = ComponentVendor.DefineCallbackProperties(
        "bell",
        objectPath: "bell",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => BellVendor;

    public ReadOnly<int> Rings => ReadOnly<int>();

    public void Ring() => Method();

    public Event<BellRung> OnRung => Event<BellRung>();
}

public sealed class BellRung : JsObject
{
    public ReadOnly<int> Times => ReadOnly<int>();
}

public static class BellHtml
{
    public static RenderedComponent<Bell, TModel> Bell<TModel>(this ReactivePlan<TModel> plan, string id)
        where TModel : class
        => plan.Render<Bell>(id, new HtmlString($"<div class=\"bell\" id=\"{HtmlEncoder.Default.Encode(id)}\"></div>"));
}
