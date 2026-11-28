#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.RenderingAndDelivery;

internal static class ViewPaths
{
    internal static string Of(IHtmlHelper html)
        => html.ViewContext.ExecutingFilePath ?? throw new InvalidOperationException("A view that renders a plan is a file MVC names by its path.");
}
#endif
