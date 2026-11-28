using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Components;

public static class ConformanceStoreHtml
{
    public static Rendered<ConformanceStore, TModel> ConformanceStore<TModel>(this ReactivePlan<TModel> plan, string id)
    {
        var host = new TagBuilder("section");
        host.Attributes["id"] = id;
        host.Attributes["data-conformance-store"] = string.Empty;
        return plan.Render<ConformanceStore>(id, host);
    }
}
