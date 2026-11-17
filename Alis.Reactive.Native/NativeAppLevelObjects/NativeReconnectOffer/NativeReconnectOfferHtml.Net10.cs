#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The door of a layout to the page's reconnect offer.
/// </summary>
public static class NativeReconnectOfferHtml
{
    /// <summary>Renders the page's reconnect offer, hidden, once in the layout, with the app's texts.</summary>
    /// <remarks>The offer shows while a connection has given up and hides once none has, when the app's startup chooses <c>ShowReconnect()</c>.</remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="text">The text the offer shows when live updates stopped.</param>
    /// <param name="retry">The text of the button that reconnects.</param>
    /// <returns>The offer's markup.</returns>
    public static IHtmlContent NativeReconnectOffer(this IHtmlHelper html, string text, string retry)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));
        if (retry is null) throw new ArgumentNullException(nameof(retry));
        var message = new TagBuilder("p");
        message.InnerHtml.Append(text);
        var button = new TagBuilder("button");
        button.MergeAttribute("type", "button");
        button.AddCssClass("alis-reconnect__retry");
        button.InnerHtml.Append(retry);
        var offer = new TagBuilder("div");
        offer.MergeAttribute("id", "alis-reconnect");
        offer.AddCssClass("alis-reconnect");
        offer.MergeAttribute("role", "alert");
        offer.MergeAttribute("hidden", "hidden");
        offer.MergeAttribute("data-native-reconnect-offer", "");
        offer.InnerHtml.AppendHtml(message);
        offer.InnerHtml.AppendHtml(button);
        return offer;
    }
}
#endif
