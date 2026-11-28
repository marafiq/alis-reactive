#if NET48
using System;
using System.Web;
using System.Web.Mvc;

namespace Alis.Reactive.OpenTelemetry.Tracing;

/// <summary>
/// The layout's door to OpenTelemetry: a traced page sends the spans it ends to your collector.
/// </summary>
public static class OpenTelemetryHtml
{
    /// <summary>Links the script that sends each span a traced page ends to your OpenTelemetry collector.</summary>
    /// <remarks>
    /// Call it once in the layout, before the framework's own script. A page traces only when your app recorded the
    /// request that rendered it, so your sampler decides which pages send spans; an untraced page sends none.
    /// </remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="tracesUrl">Your collector's OTLP HTTP traces URL, such as <c>https://collector.example/v1/traces</c>.</param>
    /// <param name="serviceName">The service name your traces are listed under.</param>
    /// <returns>The script, linked by a URL that changes when the script does.</returns>
    public static IHtmlString OpenTelemetryExporter(this HtmlHelper html, string tracesUrl, string serviceName) => throw new NotImplementedException();
}
#endif
