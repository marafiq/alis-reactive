using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Consumer.Net10.Components;

public static class AppButtonHtml
{
    public static Rendered<AppButton, TModel> AppButton<TModel>(this ReactivePlan<TModel> plan, string id, string text)
    {
        var button = new TagBuilder("button");
        button.MergeAttribute("id", id);
        button.MergeAttribute("type", "button");
        button.InnerHtml.Append(text);
        return plan.Render<AppButton>(id, button);
    }
}
