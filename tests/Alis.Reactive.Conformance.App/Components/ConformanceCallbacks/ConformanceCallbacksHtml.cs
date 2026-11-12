using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Components;

public static class ConformanceCallbacksHtml
{
    public static Rendered<ConformanceCallbacks, TModel> ConformanceCallbacks<TModel>(this ReactivePlan<TModel> plan, string id)
    {
        var host = new TagBuilder("section");
        host.Attributes["id"] = id;
        host.Attributes["data-conformance-callbacks"] = string.Empty;
        return plan.Render<ConformanceCallbacks>(id, host);
    }
}
