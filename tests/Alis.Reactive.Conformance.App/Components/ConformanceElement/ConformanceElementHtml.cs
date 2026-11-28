using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Components;

public static class ConformanceElementHtml
{
    public static Rendered<ConformanceElement, TModel> ConformanceElement<TModel>(this ReactivePlan<TModel> plan, string id, ElementStart start)
    {
        var host = new TagBuilder("conformance-element");
        host.Attributes["id"] = id;
        host.Attributes["data-conformance-element"] = plan.Json(start);
        return plan.Render<ConformanceElement>(id, host);
    }
}
