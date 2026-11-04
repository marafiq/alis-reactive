using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Framework.App.Components;

public sealed class Banner : Component, IAppLevelComponent
{
    public const string ElementId = "app-banner";

    public static ComponentVendor BannerVendor { get; } = ComponentVendor.Define(
        "banner",
        objectPath: "banner",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => BannerVendor;

    public string DefaultId => ElementId;

    public Property<string> Message => Property<string>();

    public void Flash() => Method();
}

public static class BannerHtml
{
    public static IHtmlContent Banner(this IHtmlHelper html) =>
        new HtmlString($"<div class=\"banner\" id=\"{Components.Banner.ElementId}\"></div>");
}
