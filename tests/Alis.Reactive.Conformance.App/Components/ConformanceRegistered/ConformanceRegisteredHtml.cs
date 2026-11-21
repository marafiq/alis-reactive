using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Components;

public static class ConformanceRegisteredHtml
{
    public static Rendered<ConformanceRegistered, TModel> ConformanceRegistered<TModel>(this ReactivePlan<TModel> plan, string id, RegisteredStart start)
    {
        var host = new TagBuilder("section");
        host.Attributes["id"] = id;
        host.Attributes["data-conformance-registered"] = plan.Json(start);
        return plan.Render<ConformanceRegistered>(id, host);
    }
}
