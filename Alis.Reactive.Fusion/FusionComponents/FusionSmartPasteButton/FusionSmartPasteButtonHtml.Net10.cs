#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a smart paste button.
/// </summary>
public static class FusionSmartPasteButtonHtml
{
    /// <summary>Renders a smart paste button under the id the view gives it, with the page's antiforgery token.</summary>
    /// <remarks>A button whose <see cref="FusionSmartPasteButtonOptions.AiAssistEndpoint"/> is empty is refused when the view renders it.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="elementId">The id the view gives the button's element.</param>
    /// <param name="options">What the view sets on the button.</param>
    /// <returns>The button's markup, written where the view puts it.</returns>
    public static IHtmlContent FusionSmartPasteButton<TModel>(this IHtmlHelper<TModel> html, string elementId, Action<FusionSmartPasteButtonOptions> options)
        => throw new NotImplementedException();
}
#endif
