using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// The app's banner (banner.ts): one per page, at the id its declaration names, so a page reaches it without
/// one: <c>p.Component&lt;Banner&gt;()</c>. It keeps itself on its element as <c>banner</c>.
/// </summary>
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

    /// <summary>The message the banner shows.</summary>
    public Property<string> Message => Property<string>();

    /// <summary>Flashes the banner; it counts its flashes.</summary>
    public void Flash() => Method();
}

public static class BannerHtml
{
    /// <summary>The banner, rendered once, as a layout would render it.</summary>
    public static IHtmlContent Banner(this IHtmlHelper html) =>
        new HtmlString($"<div class=\"banner\" id=\"{Components.Banner.ElementId}\"></div>");
}
