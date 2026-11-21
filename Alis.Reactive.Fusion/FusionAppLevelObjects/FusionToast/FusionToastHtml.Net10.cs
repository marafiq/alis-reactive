#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Fusion.FusionAppLevelObjects;

/// <summary>
/// The door of a layout to the page's FusionToast.
/// </summary>
public static class FusionToastHtml
{
    /// <summary>Renders the element the page's FusionToast is made on, once in the layout.</summary>
    /// <remarks>A view reaches the toast with <c>pipeline.Component&lt;FusionToast&gt;()</c>, sets its members and calls <c>Show()</c>.</remarks>
    /// <returns>The toast's host element.</returns>
    public static IHtmlContent FusionToast(this IHtmlHelper html) => throw new NotImplementedException();
}
#endif
