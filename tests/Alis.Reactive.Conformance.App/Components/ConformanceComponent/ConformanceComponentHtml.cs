using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Components;

public static class ConformanceComponentHtml
{
    public static Rendered<ConformanceComponent, TModel> ConformanceComponent<TModel>(this ReactivePlan<TModel> plan, string id, ConformanceStart start)
    {
        var host = new TagBuilder("section");
        host.Attributes["id"] = id;
        host.Attributes["data-conformance-component"] = plan.Json(start);
        return plan.Render<ConformanceComponent>(id, host);
    }

    public static Rendered<ConformanceComponent, TModel> ConformanceComponent<TModel, TValue>(this InputField<TModel, TValue> field, ConformanceStart start)
    {
        var host = new TagBuilder("section");
        host.Attributes["id"] = field.Id;
        host.Attributes["data-conformance-component"] = field.Json(start);
        return field.Render<ConformanceComponent>(host);
    }
}
