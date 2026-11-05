using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public sealed class Dispenser : Component
{
    public static ComponentVendor DispenserVendor { get; } = ComponentVendor.Define(
        "dispenser",
        objectPath: "",
        subscribe: "addEventListener",
        unsubscribe: "removeEventListener",
        payload: VendorEventPayload.Element);

    public override ComponentVendor Vendor => DispenserVendor;

    public TypedSource<int> Take() => Method<int>();

    public TypedSource<int> TakeFromTheDesk() => MethodAsync<int>();
}

public static class DispenserHtml
{
    public static RenderedComponent<Dispenser, TModel> Dispenser<TModel>(this ReactivePlan<TModel> plan, string id)
        where TModel : class
        => plan.Render<Dispenser>(id, new HtmlString($"<fw-dispenser id=\"{HtmlEncoder.Default.Encode(id)}\"></fw-dispenser>"));
}
