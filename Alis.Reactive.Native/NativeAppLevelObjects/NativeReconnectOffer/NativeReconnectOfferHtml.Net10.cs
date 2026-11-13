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
    public static IHtmlContent NativeReconnectOffer(this IHtmlHelper html, string text, string retry) => throw new NotImplementedException();
}
#endif
