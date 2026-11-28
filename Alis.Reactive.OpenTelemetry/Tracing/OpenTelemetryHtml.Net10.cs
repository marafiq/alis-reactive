#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.OpenTelemetry.Tracing;

/// <summary>
/// The layout's door to OpenTelemetry: a traced page sends the spans it ends to your collector.
/// </summary>
public static class OpenTelemetryHtml
{
    private const string Script = "/_content/AlisReactive.OpenTelemetry/js/alis-reactive-opentelemetry.js";

    /// <summary>Links the script that sends each span a traced page ends to your OpenTelemetry collector.</summary>
    /// <remarks>
    /// Call it once in the layout, before the framework's own script. A page traces only when your app recorded the
    /// request that rendered it, so your sampler decides which pages send spans; an untraced page sends none.
    /// </remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="tracesUrl">Your collector's OTLP HTTP traces URL, such as <c>https://collector.example/v1/traces</c>.</param>
    /// <param name="serviceName">The service name your traces are listed under.</param>
    /// <returns>The script, linked by a URL that changes when the script does.</returns>
    public static IHtmlContent OpenTelemetryExporter(this IHtmlHelper html, string tracesUrl, string serviceName)
    {
        if (string.IsNullOrWhiteSpace(tracesUrl)) throw new ArgumentException("The OpenTelemetry exporter needs your collector's OTLP HTTP traces URL.", nameof(tracesUrl));
        if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException("The OpenTelemetry exporter needs the service name your traces are listed under.", nameof(serviceName));

        var request = html.ViewContext.HttpContext;
        var pathBase = request.Request.PathBase;
        var versions = request.RequestServices.GetRequiredService<IFileVersionProvider>();
        var script = new TagBuilder("script");
        script.MergeAttribute("src", versions.AddFileVersionToPath(pathBase, pathBase.Add(new PathString(Script)).ToString()));
        script.MergeAttribute("data-traces-url", tracesUrl);
        script.MergeAttribute("data-service-name", serviceName);
        return script;
    }
}
#endif
