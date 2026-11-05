using System.Text.Encodings.Web;
using Alis.Reactive.Framework.App.Components;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Slots;

public sealed class LoungePage;

public sealed class VisitorCard;

public sealed class LoungeBellRang
{
    public int Times { get; set; }
}

public static class LoungeEvents
{
    public static readonly AppEvent<LoungeBellRang> LoungeBellRang = AppEvent.Declare<LoungeBellRang>();
}

public sealed class MeterWithoutRelease : Component
{
    public static ComponentVendor MeterWithoutReleaseVendor { get; } = ComponentVendor.Define(
        "meter-without-release",
        objectPath: "",
        subscribe: "addEventListener",
        unsubscribe: "dropEventListener",
        payload: VendorEventPayload.Element);

    public override ComponentVendor Vendor => MeterWithoutReleaseVendor;

    public Event<MeterLevelChanged> LevelChanged => Event<MeterLevelChanged>();
}

public static class MeterWithoutReleaseHtml
{
    public static RenderedComponent<MeterWithoutRelease, TModel> MeterWithoutRelease<TModel>(this ReactivePlan<TModel> plan, string id)
        where TModel : class
        => plan.Render<MeterWithoutRelease>(id, new HtmlString(
            $"<fw-meter id=\"{HtmlEncoder.Default.Encode(id)}\" level=\"1\" max=\"5\"></fw-meter>"));
}
